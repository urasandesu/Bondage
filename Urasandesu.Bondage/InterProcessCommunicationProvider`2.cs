/* 
 * File: InterProcessCommunicationProvider`2.cs
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
using System.ServiceModel;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;

namespace Urasandesu.Bondage
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public abstract class InterProcessCommunicationProvider<TRemotePSharpRuntime, TRemoteNetworkProvider> : ICommunicationProvider, IRemotePSharpRuntime
        where TRemotePSharpRuntime : IRemotePSharpRuntime
        where TRemoteNetworkProvider : ICommunicationProvider, new()
    {
        readonly RuntimeHost m_runtimeHost;
        readonly InterProcessCommunicationSetting m_setting;
        readonly TRemoteNetworkProvider m_remoteNetworkProvider = new TRemoteNetworkProvider();

        ServiceHost m_serviceHost;
        ServiceHost ServiceHost
        {
            get
            {
                if (m_serviceHost == null)
                    m_serviceHost = CreateServiceHost(m_setting.LocalEndpointUri);
                return m_serviceHost;
            }
        }

        ServiceHost CreateServiceHost(Uri serviceHostUri)
        {
            var serviceHost = new ServiceHost(this);
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            serviceHost.AddServiceEndpoint(typeof(TRemotePSharpRuntime), binding, serviceHostUri);
            return serviceHost;
        }

        protected InterProcessCommunicationProvider(RuntimeHost runtimeHost, string localEndpointName) :
            this(runtimeHost, new InterProcessCommunicationSetting(localEndpointName))
        {
        }

        protected InterProcessCommunicationProvider(RuntimeHost runtimeHost, InterProcessCommunicationSetting setting)
        {
            m_runtimeHost = runtimeHost ?? throw new ArgumentNullException(nameof(runtimeHost));
            m_setting = setting ?? throw new ArgumentNullException(nameof(setting));

            ServiceHost.Open();
        }

        public MachineId RemoteCreateMachine(Type type, string friendlyName, string endpoint, Event e)
        {
            throw new NotSupportedException();
        }

        public void RemoteSend(MachineId target, Event e)
        {
            m_remoteNetworkProvider.RemoteSend(target, e);
        }

        public string GetLocalEndpoint()
        {
            return m_setting.LocalEndpointUri.ToString();
        }

        public void RemoteMonitor(MonitorId target, Event e)
        {
            m_remoteNetworkProvider.RemoteMonitor(target, e);
        }

        public object RemoteDoCommunication(CommunicationId target, string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return m_remoteNetworkProvider.RemoteDoCommunication(target, name, args);
        }

        public Type RemoteNetworkProviderType => m_remoteNetworkProvider.RemoteNetworkProviderType;

        public void SendEvent(MachineId target, Event e)
        {
            m_runtimeHost.SendEvent(target, e);
        }

        public void Monitor(MonitorId target, Event e)
        {
            m_runtimeHost.InvokeMonitor(target, e);
        }
        public object DoCommunication(CommunicationId target, string name, params object[] args)
        {
            return RuntimeHost.DoCommunication(target, name, args);
        }

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
                    m_serviceHost?.Close();
                    m_remoteNetworkProvider?.Dispose();
                }

                // Free any unmanaged objects here. 
                //
                m_disposed = true;
            }
        }
    }
}
