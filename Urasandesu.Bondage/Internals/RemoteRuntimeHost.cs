/* 
 * File: RemoteRuntimeHost.cs
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
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;

namespace Urasandesu.Bondage.Internals
{
    class RemoteRuntimeHost : RuntimeHost
    {
        readonly ICommunicationProvider m_networkProvider2;

        public RemoteRuntimeHost(string assemblyQualifiedName)
        {
            var msg = $"You have to pass an instance implemented '{ nameof(ICommunicationProvider) }' to '{ nameof(RuntimeHost) }.{ nameof(RuntimeHost.SetNetworkProvider) }'.";
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                throw new InvalidOperationException(msg);

            m_networkProvider2 = Activator.CreateInstance(Type.GetType(assemblyQualifiedName)) as ICommunicationProvider;
            if (m_networkProvider2 == null)
                throw new InvalidOperationException(msg);
        }

        public override TSender New<TSender, TBundler, TReceiver>(MonitorInterface<TSender, TBundler, TReceiver> @interface)
        {
            throw new NotSupportedException();
        }

        public override TSender New<TSender, TBundler, TReceiver>(MachineInterface<TSender, TBundler, TReceiver> @interface)
        {
            throw new NotSupportedException();
        }

        public override TContext New<TContext>(params object[] args)
        {
            throw new NotSupportedException();
        }

        public override void SetNetworkProvider(ICommunicationProvider networkProvider)
        {
            throw new NotSupportedException();
        }

        public override void SendEvent(MachineId target, ApplicationEvent e)
        {
            m_networkProvider2.RemoteSend(target, e);
        }

        internal override void SendEvent(MachineId target, Event e)
        {
            throw new NotSupportedException();
        }

        public override void InvokeMonitor(MonitorId target, ApplicationEvent e)
        {
            m_networkProvider2.RemoteMonitor(target, e);
        }

        internal override void InvokeMonitor(MonitorId target, Event e)
        {
            throw new NotSupportedException();
        }

        public override TResult Communicate<TResult>(CommunicationId target, Func<object[], TResult> func, params object[] args)
        {
            return (TResult)m_networkProvider2.RemoteDoCommunication(target, func.Method.ToString(), args);
        }

        public override bool IsLocalEndpoint(string endpoint)
        {
            return false;
        }
    }
}
