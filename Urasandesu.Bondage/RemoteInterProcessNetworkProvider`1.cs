/* 
 * File: RemoteInterProcessNetworkProvider`1.cs
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
using System.Collections.Concurrent;
using System.ServiceModel;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;

namespace Urasandesu.Bondage
{
    public class RemoteInterProcessNetworkProvider<TRemotePSharpRuntime> : ICommunicationProvider
        where TRemotePSharpRuntime : IRemotePSharpRuntime
    {
        readonly ConcurrentDictionary<string, TRemotePSharpRuntime> m_channel = new ConcurrentDictionary<string, TRemotePSharpRuntime>();
        TRemotePSharpRuntime GetOrAddChannel(string endpointUri)
        {
            return m_channel.GetOrAdd(endpointUri, CreateChannel);
        }

        static TRemotePSharpRuntime CreateChannel(string endpointUri)
        {
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            var endpointAddress = new EndpointAddress(endpointUri);
            return ChannelFactory<TRemotePSharpRuntime>.CreateChannel(binding, endpointAddress);
        }

        public MachineId RemoteCreateMachine(Type type, string friendlyName, string endpoint, Event e)
        {
            throw new NotSupportedException();
        }

        public void RemoteSend(MachineId target, Event e)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            GetOrAddChannel(target.Endpoint).SendEvent(target, e);
        }

        public string GetLocalEndpoint()
        {
            throw new NotSupportedException();
        }

        public void RemoteMonitor(MonitorId target, Event e)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            GetOrAddChannel(target.Endpoint).Monitor(target, e);
        }

        public object RemoteDoCommunication(CommunicationId target, string name, params object[] args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrEmpty(target.Endpoint))
                throw new InvalidOperationException($"The target operation is a local operation. You must pass the { nameof(target) } parameter that is from a remote host.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return GetOrAddChannel(target.Endpoint).DoCommunication(target, name, args);
        }

        public Type RemoteNetworkProviderType => GetType();

        bool m_disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // Free any other managed objects here. 
                    //
                }

                // Free any unmanaged objects here. 
                //
                m_disposed = true;
            }
        }
    }
}
