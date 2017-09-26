/* 
 * File: TransitionAndBundlerStorage`3.cs
 * 
 * Author: Akira Sugiura (urasandesu@gmail.com)
 * 
 * 
 * Copyright (c) 2017 Akira Sugiura
 *  
 *  This software is MIT License.
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */



using Microsoft.PSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Urasandesu.NAnonym.Mixins.System;

namespace Urasandesu.Bondage.Internals
{
    abstract class TransitionAndBundlerStorage<TSender, TBundler, TReceiver>
    {
        protected abstract SenderStorage<TSender> SenderStorage { get; }

        public (Type transType, Type bundlerType, Type userDefStartState) DefineTransitionAndBundlerType()
        {
            var modBldr = SenderStorage.DefineTemporaryModuleBuilder();
            var receiverType = GetReceiverType();
            (var transType, var userDefStartState) = DefineTransitionType(modBldr, receiverType);
            var bundlerType = DefineBundlerType(modBldr, receiverType, transType);
            return (transType, bundlerType, userDefStartState);
        }

        Type GetReceiverType()
        {
            var receiverTypeBase = ReceiverTypeBase;
            var receiverTypes = typeof(TReceiver).GetInterfaces().Except(new[] { receiverTypeBase }).Where(_ => receiverTypeBase.IsAssignableFrom(_)).ToArray();
            if (receiverTypes.Length != 1)
                throw new NotSupportedException($"The generic parameter '{ nameof(TReceiver) }' must inherit the base type '{ receiverTypeBase.FullName }' only one time.");

            return receiverTypes[0];
        }

        protected abstract Type ReceiverTypeBase { get; }

        (Type transType, Type userDefStartState) DefineTransitionType(ModuleBuilder modBldr, Type receiverType)
        {
            var name = TransitionTypeName;
            var typeAttr = TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            var parentType = TransitionParentType;
            var transBldr = modBldr.DefineType(name, typeAttr, parentType);

            var userDefStartState = DefineTransitionStateNestedTypes(transBldr);
            DefineTransitionReceiverMethods(transBldr, receiverType);
            return (transBldr.CreateType(), userDefStartState);
        }

        protected abstract string TransitionTypeName { get; }
        protected abstract Type TransitionParentType { get; }

        Type DefineTransitionStateNestedTypes(TypeBuilder transBldr)
        {
            var stateBuildInfos = new List<StateBuildInfo>();
            var stateMethods = GetStateMethods();
            foreach (var stateMethod in stateMethods)
            {
                var name = stateMethod.Name;
                var typeAttr = TypeAttributes.NestedPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
                var parentType = TransitionStateParentType;
                var stateBldr = transBldr.DefineNestedType(name, typeAttr, parentType);

                var methodizedStateAttrs = stateMethod.GetCustomAttributes(typeof(MethodizedStateAttribute), true).OfType<MethodizedStateAttribute>();
                stateBuildInfos.Add(new StateBuildInfo(transBldr, stateBldr, methodizedStateAttrs));
            }

            var userDefStartState = default(Type);
            foreach (var stateBuildInfo in stateBuildInfos)
            {
                foreach (var methodizedStateAttr in stateBuildInfo.MethodizedStateAttributes)
                    methodizedStateAttr.SetStateAttributeTo<TSender, TReceiver, TBundler>(stateBuildInfo, stateBuildInfos.Select(_ => _.CurrentStateBuilder));

                var type = stateBuildInfo.CreateTypeAndGetUserDefinedStartState();
                if (type != null)
                    userDefStartState = type;
            }

            return userDefStartState;
        }

        protected abstract Type TransitionStateParentType { get; }

        MethodInfo[] GetStateMethods()
        {
            return typeof(TBundler).GetMethods().Where(_ => _.IsDefined(typeof(MethodizedStateAttribute), true)).ToArray();
        }

        void DefineTransitionReceiverMethods(TypeBuilder transBldr, Type receiverType)
        {
            var receiverMethods = receiverType.GetMethods();
            foreach (var receiverMethod in receiverMethods)
            {
                var eventType = SenderStorage.GetMethodizedEventType(receiverMethod);

                var name = receiverMethod.Name;
                var methAttr = MethodAttributes.Private | MethodAttributes.HideBySig;
                var cc = CallingConventions.HasThis;
                var eventHandlerBldr = transBldr.DefineMethod(name, methAttr, cc);

                var applicationTransitionOfTBundler_get_Bundler = TransitionParentType.GetProperty("Bundler").GetGetMethod();
                var transition_get_ReceivedEvent = TransitionType.GetProperty("ReceivedEvent", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);

                var gen = eventHandlerBldr.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, applicationTransitionOfTBundler_get_Bundler);
                if (eventType != null)
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Call, transition_get_ReceivedEvent);
                    gen.Emit(OpCodes.Castclass, eventType);
                }
                gen.Emit(OpCodes.Callvirt, receiverMethod);
                gen.Emit(OpCodes.Ret);
            }
        }

        protected abstract Type TransitionType { get; }

        Type DefineBundlerType(ModuleBuilder modBldr, Type receiverType, Type transType)
        {
            var parentType = BundlerGenericParentTypeDefinition.MakeGenericType(receiverType);
            var interfaceTypes = new[] { typeof(TBundler) }.Concat(typeof(TBundler).GetInterfaces()).ToArray();
            var name = "<Bundler>" +  typeof(TBundler).FullNameWithoutNestedTypeQualification();
            var typeAttr = TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            var bundlerBldr = modBldr.DefineType(name, typeAttr, parentType, interfaceTypes);

            DefineBundlerConstructor(bundlerBldr, receiverType, parentType);
            SenderStorage.DefineSenderSenderMethods(bundlerBldr, parentType);
            DefineBundlerReceiverMethods(bundlerBldr, receiverType, parentType);
            DefineBundlerStateMethods(bundlerBldr, transType, parentType);
            DefineBundlerImmediateSenderMethods(bundlerBldr, transType, parentType);
            return bundlerBldr.CreateType();
        }

        protected abstract Type BundlerGenericParentTypeDefinition { get; }

        void DefineBundlerConstructor(TypeBuilder bundlerBldr, Type receiverType, Type parentType)
        {
            var methAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var cc = CallingConventions.HasThis;
            var paramTypes = new[] { typeof(RuntimeHost), TransitionIdType, receiverType };
            var ctorBldr = bundlerBldr.DefineConstructor(methAttr, cc, paramTypes);

            var methodizedTransitionBundlerOfReceiver_ctor = parentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);

            var gen = ctorBldr.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Ldarg_3);
            gen.Emit(OpCodes.Call, methodizedTransitionBundlerOfReceiver_ctor);
            gen.Emit(OpCodes.Ret);
        }

        protected abstract Type TransitionIdType { get; }

        void DefineBundlerReceiverMethods(TypeBuilder bundlerBldr, Type receiverType, Type parentType)
        {
            var receiverMethods = receiverType.GetMethods();
            foreach (var receiverMethod in receiverMethods)
            {
                var eventType = SenderStorage.GetMethodizedEventType(receiverMethod);

                var name = receiverMethod.Name;
                var methAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                var cc = CallingConventions.HasThis;
                var retType = typeof(void);
                var paramTypes = eventType == null ? Type.EmptyTypes : new[] { eventType };
                var eventHandlerBldr = bundlerBldr.DefineMethod(name, methAttr, cc, retType, paramTypes);
                bundlerBldr.DefineMethodOverride(eventHandlerBldr, receiverMethod);

                var methodizedTransitionBundlerOfReceiver_get_Receiver = parentType.GetProperty("Receiver").GetGetMethod();

                var gen = eventHandlerBldr.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, methodizedTransitionBundlerOfReceiver_get_Receiver);
                if (eventType != null)
                {
                    gen.Emit(OpCodes.Ldarg_1);
                }
                gen.Emit(OpCodes.Callvirt, receiverMethod);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldstr, name);
                gen.Emit(OpCodes.Call, GetHandledLogMethod(bundlerBldr.BaseType));
                gen.Emit(OpCodes.Ret);
            }
        }

        protected abstract MethodInfo GetHandledLogMethod(Type baseType);

        void DefineBundlerStateMethods(TypeBuilder bundlerBldr, Type transType, Type parentType)
        {
            var stateMethods = typeof(TBundler).GetMethods().Where(_ => _.IsDefined(typeof(MethodizedStateAttribute), true));
            foreach (var stateMethod in stateMethods)
            {
                var state = transType.GetNestedType(stateMethod.Name);
                var name = state.Name;
                var methAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                var cc = CallingConventions.HasThis;
                var stateBldr = bundlerBldr.DefineMethod(name, methAttr, cc);
                bundlerBldr.DefineMethodOverride(stateBldr, stateMethod);

                var methodizedTransitionBundlerOfReceiver_get_GotoType = parentType.GetProperty("GotoType").GetGetMethod();
                var actionOfType_Invoke = typeof(Action<Type>).GetMethod("Invoke");
                var type_GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");

                var gen = stateBldr.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, methodizedTransitionBundlerOfReceiver_get_GotoType);
                gen.Emit(OpCodes.Ldtoken, state);
                gen.Emit(OpCodes.Call, type_GetTypeFromHandle);
                gen.Emit(OpCodes.Callvirt, actionOfType_Invoke);
                gen.Emit(OpCodes.Ret);
            }
        }

        void DefineBundlerImmediateSenderMethods(TypeBuilder bundlerBldr, Type transType, Type parentType)
        {
            var immediateSenderMethods = typeof(TBundler).GetMethods();
            foreach (var immediateSenderMethod in immediateSenderMethods)
            {
                var eventType = default(Type);
                if (!SenderStorage.IsMethodizedEvent(immediateSenderMethod, out eventType))
                    continue;

                var name = immediateSenderMethod.Name;
                var methAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                var cc = CallingConventions.HasThis;
                var retType = typeof(void);
                var paramTypes = new[] { eventType };
                var immediateEventBldr = bundlerBldr.DefineMethod(name, methAttr, cc, retType, paramTypes);
                bundlerBldr.DefineMethodOverride(immediateEventBldr, immediateSenderMethod);

                var methodizedTransitionBundlerOfReceiver_get_RaiseEvent = parentType.GetProperty("RaiseEvent").GetGetMethod();
                var actionOfEvent_Invoke = typeof(Action<Event>).GetMethod("Invoke");

                var gen = immediateEventBldr.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, methodizedTransitionBundlerOfReceiver_get_RaiseEvent);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, actionOfEvent_Invoke);
                gen.Emit(OpCodes.Ret);
            }
        }
    }
}
