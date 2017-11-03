/* 
 * File: SenderTypeBuilder.cs
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Urasandesu.Bondage.Internals
{
    abstract class SenderTypeBuilder
    {
        static SenderTypeBuilder()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (!args.Name.StartsWith("Urasandesu.Bondage.Internals"))
                    return null;

                return AppDomain.CurrentDomain.GetAssemblies().First(_ => _.FullName == args.Name);
            };
        }

        public Type DefineSenderType()
        {
            var modBldr = DefineTemporaryModuleBuilder();
            return DefineSenderType(modBldr);
        }

        public ModuleBuilder DefineTemporaryModuleBuilder()
        {
            var asmName = new AssemblyName("Urasandesu.Bondage.Internals." + Guid.NewGuid().ToString("N"));
            var asmBldr = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var modBldr = asmBldr.DefineDynamicModule(asmName.Name + ".dll");
            return modBldr;
        }

        protected abstract Type DefineSenderType(ModuleBuilder modBldr);

        protected void DefineSenderConstructor(TypeBuilder senderBldr, Type parentType)
        {
            var methAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var cc = CallingConventions.HasThis;
            var paramTypes = new[] { typeof(RuntimeHost), TransitionIdType };
            var ctorBldr = senderBldr.DefineConstructor(methAttr, cc, paramTypes);

            var methodizedTransitionSender_ctor = parentType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);

            var gen = ctorBldr.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Call, methodizedTransitionSender_ctor);
            gen.Emit(OpCodes.Ret);
        }

        protected abstract Type TransitionIdType { get; }

        public void DefineSenderSenderMethods(TypeBuilder senderBldr, Type parentType)
        {
            var senderType = GetSenderType();
            var senderMethods = senderType.GetMethods();
            foreach (var senderMethod in senderMethods)
            {
                var eventType = default(Type);
                if (!IsMethodizedEvent(senderMethod, out eventType))
                    continue;

                var name = senderMethod.Name;
                var methAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                var cc = CallingConventions.HasThis;
                var retType = typeof(void);
                var paramTypes = new[] { eventType };
                var eventBldr = senderBldr.DefineMethod(name, methAttr, cc, retType, paramTypes);
                senderBldr.DefineMethodOverride(eventBldr, senderMethod);

                var methodizedTransitionSender_get_RuntimeHost = parentType.GetProperty("RuntimeHost").GetGetMethod();
                var methodizedTransitionSender_get_Id = parentType.GetProperty("Id").GetGetMethod();

                var gen = eventBldr.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, methodizedTransitionSender_get_RuntimeHost);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, methodizedTransitionSender_get_Id);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, SendEventMethod);
                gen.Emit(OpCodes.Ret);
            }
        }

        protected abstract MethodInfo SendEventMethod { get; }

        public abstract Type GetSenderType();

        public bool IsMethodizedEvent(MethodInfo meth, out Type eventType)
        {
            eventType = GetMethodizedEventType(meth);
            return eventType != null;
        }

        public Type GetMethodizedEventType(MethodInfo meth)
        {
            var @params = meth.GetParameters();
            if (@params.Length != 1)
                return null;

            var paramType = @params[0].ParameterType;
            if (!paramType.IsSubclassOf(typeof(Event)))
                return null;

            return paramType;
        }
    }
}
