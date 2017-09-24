/* 
 * File: MethodizedMachineBundler`1.cs
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



using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using Microsoft.PSharp.IO;
using System;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO;

namespace Urasandesu.Bondage.Internals
{
    public abstract class MethodizedMachineBundler<TReceiver> : IMethodizedMachineSender, IMethodizedMachineReceiver, IMethodizedMachineStatus
        where TReceiver : IMethodizedMachineReceiver
    {
        protected MethodizedMachineBundler(RuntimeHost runtimeHost, MachineId id, TReceiver receiver)
        {
            RuntimeHost = runtimeHost;
            Id = id;
            Receiver = receiver;
        }

        public RuntimeHost RuntimeHost { get; private set; }
        public TReceiver Receiver { get; private set; }

        public IUnityContainer Container { get; private set; }

        [InjectionMethod]
        public void Initialize(IUnityContainer container)
        {
            Container = container;
            Container.BuildUp(Receiver.GetType(), Receiver);
        }

        public MachineId Id { get; private set; }
        public Func<ILogger> LoggerGet { get; set; }
        public ILogger Logger { get => LoggerGet(); }
        public Func<int> HashedStateGet { get; set; }
        public Func<Type> CurrentStateGet { get; set; }
        public Type CurrentState { get => CurrentStateGet(); }
        public Func<Event> ReceivedEventGet { get; set; }
        public Action<bool> AssertBool { get; set; }
        public Action<bool, string, object[]> AssertBoolStringObjectArray { get; set; }
        public Action<Type> GotoType { get; set; }
        public Action<Event> RaiseEvent { get; set; }
        public Func<int, bool> RandomInt32 { get; set; }
        public Func<bool> Random { get; set; }
        public Func<int, int> RandomIntegerInt32 { get; set; }

        protected void MachineHandledLog(string actionName)
        {
            if (Logger is IPublishableLogger publishableLogger && 1 < (publishableLogger.Configuration?.Verbose ?? -1))
                publishableLogger.OnMachineActionHandled(Id, AbstractMachineMixin.GetStateName(CurrentState), actionName);
        }
    }
}
