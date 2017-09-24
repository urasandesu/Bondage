/* 
 * File: ApplicationMachine.cs
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
using Urasandesu.Bondage.Internals;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;

namespace Urasandesu.Bondage
{
    public abstract class ApplicationMachine<TBundler> : Machine
        where TBundler : IMethodizedMachineSender, IMethodizedMachineReceiver, IMethodizedMachineStatus
    {
        [Dependency]
        public PSharpRuntime Runtime { protected get; set; }

        public TBundler Bundler { get; private set; }

        protected void HandleConstruct()
        {
            var construct = (Construct)ReceivedEvent;
            Bundler = (TBundler)construct.Bundler;
            Bundler.Container.BuildUp(GetType(), this);
            Bundler.LoggerGet = () => Logger;
            Bundler.HashedStateGet = () => HashedState;
            Bundler.CurrentStateGet = () => CurrentState;
            Bundler.ReceivedEventGet = () => ReceivedEvent;
            Bundler.AssertBool = Assert;
            Bundler.AssertBoolStringObjectArray = Assert;
#pragma warning disable 618
            Bundler.GotoType = Goto;
#pragma warning restore 618
            Bundler.RaiseEvent = Raise;
            Bundler.RandomInt32 = Random;
            Bundler.Random = Random;
            Bundler.RandomIntegerInt32 = RandomInteger;
            Bundler.GotoType(construct.UserDefinedStartState);
        }

        protected void RemoteMonitor(MonitorId target, Event e)
        {
            if (Runtime.NetworkProvider is ICommunicationProvider networkProvider2)
            {
                networkProvider2.RemoteMonitor(target, e);
                return;
            }

            Runtime.InvokeMonitor(target, e);
        }
    }
}
