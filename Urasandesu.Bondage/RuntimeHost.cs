/* 
 * File: RuntimeHost.cs
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
using System.Collections.Generic;
using System.Linq;
using Urasandesu.Bondage.Infrastructures;
using Urasandesu.Bondage.Internals;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;

namespace Urasandesu.Bondage
{
    public class RuntimeHost : IDisposable
    {
        protected RuntimeHost()
        { }

        public RuntimeHost(IUnityContainer container, PSharpRuntime runtime)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            Id = new RuntimeHostId() { AdditionalConfiguration = runtime.GetAdditionalConfiguration() };
            Id.SetNetworkInformation(runtime.NetworkProvider);
            RuntimeHostReferences.Register(this);
        }



        public RuntimeHostId Id { get; }

        public IUnityContainer Container { get; }
        public PSharpRuntime Runtime { get; }



        public virtual TSender New<TSender, TBundler, TReceiver>(MonitorInterface<TSender, TBundler, TReceiver> @interface)
            where TSender : class, IMethodizedMonitorSender
            where TBundler : class, IMethodizedMonitorSender, IMethodizedMonitorReceiver, IMethodizedMonitorStatus
            where TReceiver : MethodizedMonitorReceiver<TBundler>, IMethodizedMonitorReceiver
        {
            return MonitorStorage<TSender, TBundler, TReceiver>.Get(this, @interface);
        }

        internal TSender GetMonitorSender<TSender, TBundler, TReceiver>(MonitorInterface<TSender, TBundler, TReceiver> @interface, Type transType, Type bundlerType, Type userDefStartState)
            where TSender : class, IMethodizedMonitorSender
            where TBundler : class, IMethodizedMonitorSender, IMethodizedMonitorReceiver, IMethodizedMonitorStatus
            where TReceiver : MethodizedMonitorReceiver<TBundler>, IMethodizedMonitorReceiver
        {
            var receiver = Container.Resolve<TReceiver>();
            var id = Runtime.NewMonitor(transType);
            var registeredMonitors = Runtime.GetRegisteredMonitors();
            return (TBundler)registeredMonitors.AddOrUpdate(id, _ => default, (_, oldBundler) =>
            {
                if (oldBundler != null)
                    return oldBundler;

                var bundler = (TBundler)Activator.CreateInstance(bundlerType, this, id, receiver);
                receiver.Self = bundler;
                Container.BuildUp(bundler.GetType(), bundler);
                InvokeMonitor(id, new Construct(bundler, userDefStartState));
                return bundler;
            }) as TSender;
        }



        public virtual TSender New<TSender, TBundler, TReceiver>(MachineInterface<TSender, TBundler, TReceiver> @interface)
            where TSender : class, IMethodizedMachineSender
            where TBundler : class, IMethodizedMachineSender, IMethodizedMachineReceiver, IMethodizedMachineStatus
            where TReceiver : MethodizedMachineReceiver<TBundler>, IMethodizedMachineReceiver
        {
            return MachineStorage<TSender, TBundler, TReceiver>.Get(this, @interface);
        }


        internal TSender GetMachineSender<TSender, TBundler, TReceiver>(MachineInterface<TSender, TBundler, TReceiver> @interface, Type transType, Type bundlerType, Type userDefStartState)
            where TSender : class, IMethodizedMachineSender
            where TBundler : class, IMethodizedMachineSender, IMethodizedMachineReceiver, IMethodizedMachineStatus
            where TReceiver : MethodizedMachineReceiver<TBundler>, IMethodizedMachineReceiver
        {
            var receiver = Container.Resolve<TReceiver>();
            var id = Runtime.NewMachine(transType);
            var bundler = (TBundler)Activator.CreateInstance(bundlerType, this, id, receiver);
            receiver.Self = bundler;
            Container.BuildUp(bundler.GetType(), bundler);
            SendEvent(id, new Construct(bundler, userDefStartState));
            return bundler as TSender;
        }



        public virtual TContext New<TContext>(params object[] args)
            where TContext : ApplicationContext, new()
        {
            var ctx = new TContext();
            ctx.LinkTo(this, args);
            return ctx;
        }



        public DistributedDictionary<TKey, TValue> NewDistributedDictionary<TKey, TValue>()
        {
            return New<DistributedDictionary<TKey, TValue>>();
        }

        public DistributedDictionary<TKey, TValue> NewDistributedDictionary<TKey, TValue>(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            return New<DistributedDictionary<TKey, TValue>>(comparer);
        }



        public DistributedRegister<T> NewDistributedRegister<T>(T value = default) where T : struct
        {
            return New<DistributedRegister<T>>(value);
        }



        public DistributedCounter NewDistributedCounter(int value = 0)
        {
            return New<DistributedCounter>(value);
        }



        public virtual void SetNetworkProvider(ICommunicationProvider networkProvider)
        {
            Runtime.SetNetworkProvider(networkProvider);
            Id.SetNetworkInformation(networkProvider);
        }



        public virtual ILogger Logger { get => Runtime.Logger; }
        public virtual IPublishableLogger PublishableLogger { get => Runtime.Logger as IPublishableLogger; }
        public virtual void SetLogger(IPublishableLogger logger)
        {
            if (Runtime.Logger is IPublishableLogger publishableLogger)
                Runtime.OnFailure -= publishableLogger.OnFailure;
            Runtime.SetLogger(logger);
            Runtime.OnFailure += logger.OnFailure;
        }



        internal static TSender GetSender<TSender>(CommunicationId target, MonitorId id) where TSender : class, IMethodizedMonitorSender
        {
            if (id == null)
                return default;

            return MonitorSenderProperty<TSender>.Get(target, id);
        }

        internal static void SetSender<TSender>(ref MonitorId id, TSender value) where TSender : class, IMethodizedMonitorSender
        {
            if (value == null)
                return;

            MonitorSenderProperty<TSender>.Set(ref id, value);
        }

        internal static TSender[] GetSender<TSender>(CommunicationId target, MonitorId[] ids) where TSender : class, IMethodizedMonitorSender
        {
            if (ids == null || ids.Length == 0)
                return new TSender[0];

            return ids.Select(_ => MonitorSenderProperty<TSender>.Get(target, _)).ToArray();
        }

        internal static void SetSender<TSender>(ref MonitorId[] ids, TSender[] values) where TSender : class, IMethodizedMonitorSender
        {
            if (values == null || values.Length == 0)
                return;

            var tmpIds = new List<MonitorId>();
            foreach (var value in values)
            {
                var id = default(MonitorId);
                MonitorSenderProperty<TSender>.Set(ref id, value);
                tmpIds.Add(id);
            }
            ids = tmpIds.ToArray();
        }

        internal static TSender GetSender<TSender>(CommunicationId target, MachineId id) where TSender : class, IMethodizedMachineSender
        {
            if (id == null)
                return default;

            return MachineSenderProperty<TSender>.Get(target, id);
        }

        internal static void SetSender<TSender>(ref MachineId id, TSender value) where TSender : class, IMethodizedMachineSender
        {
            if (value == null)
                return;

            MachineSenderProperty<TSender>.Set(ref id, value);
        }

        internal static TSender[] GetSender<TSender>(CommunicationId target, MachineId[] ids) where TSender : class, IMethodizedMachineSender
        {
            if (ids == null || ids.Length == 0)
                return new TSender[0];

            return ids.Select(_ => MachineSenderProperty<TSender>.Get(target, _)).ToArray();
        }

        internal static void SetSender<TSender>(ref MachineId[] ids, TSender[] values) where TSender : class, IMethodizedMachineSender
        {
            if (values == null || values.Length == 0)
                return;

            var tmpIds = new List<MachineId>();
            foreach (var value in values)
            {
                var id = default(MachineId);
                MachineSenderProperty<TSender>.Set(ref id, value);
                tmpIds.Add(id);
            }
            ids = tmpIds.ToArray();
        }



        public virtual void SendEvent(MachineId target, ApplicationEvent e)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (e == null)
                throw new ArgumentNullException(nameof(e));

            e.LinkTo(this);
            if (IsLocalEndpoint(target.Endpoint))
                Runtime.SendEvent(target, e);
            else
                Runtime.RemoteSendEvent(target, e);
        }

        internal virtual void SendEvent(MachineId target, Event e)
        {
            Runtime.SendEvent(target, e);
        }



        public virtual void InvokeMonitor(MonitorId target, ApplicationEvent e)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (e == null)
                throw new ArgumentNullException(nameof(e));

            e.LinkTo(this);
            if (IsLocalEndpoint(target.Endpoint))
                Runtime.InvokeMonitor(target, e);
            else
                Runtime.RemoteInvokeMonitor(target, e);
        }

        internal virtual void InvokeMonitor(MonitorId target, Event e)
        {
            Runtime.InvokeMonitor(target, e);
        }



        static readonly WeakReferenceKeyTable<CommunicationId, Dictionary<string, Func<object[], object>>> ms_communicationTable = new WeakReferenceKeyTable<CommunicationId, Dictionary<string, Func<object[], object>>>();

        public static void RegisterCommunication(CommunicationId target, Func<object[], object>[] actions)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (actions == null)
                throw new ArgumentNullException(nameof(target));

            if (actions.Length == 0)
                throw new ArgumentException("The parameter does not contain any elements.", nameof(actions));

            ms_communicationTable.TryAdd(target, actions.ToDictionary(_ => _.Method.ToString(), _ => _));
        }

        public static TResult DoCommunication<TResult>(CommunicationId target, Func<object[], TResult> func, params object[] args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var runtimeHost = RuntimeHostReferences.Get(target.RuntimeHostId);
            return runtimeHost.Communicate(target, func, args);
        }

        internal static object DoCommunication(CommunicationId target, string name, params object[] args)
        {
            var communications = default(Dictionary<string, Func<object[], object>>);
            if (!ms_communicationTable.TryGetValue(target, out communications))
                throw new ArgumentException("The object that relates to the parameter has already destructed.", nameof(target));

            var communication = default(Func<object[], object>);
            if (!communications.TryGetValue(name, out communication))
                throw new ArgumentException($"The '{ name }' communication is not contained.", nameof(name));

            return communication(args);
        }

        public virtual TResult Communicate<TResult>(CommunicationId target, Func<object[], TResult> func, params object[] args)
        {
            if (IsLocalEndpoint(target.Endpoint))
                return func(args);
            else
                return Runtime.RemoteDoCommunication(target, func, args);
        }



        public virtual bool IsLocalEndpoint(string endpoint)
        {
            return Runtime.NetworkProvider.GetLocalEndpoint() == endpoint;
        }



        bool m_disposed;
        bool m_isProcessingDispose;

        protected virtual void Dispose(bool disposing)
        {
            if (m_isProcessingDispose)
                return;

            try
            {
                m_isProcessingDispose = true;
                if (!m_disposed)
                {
                    if (disposing)
                    {
                        Container.Dispose();
                        // PSharpRuntime should not dispose of here because it will handle in the runtime host of P#.
                    }

                    m_disposed = true;
                }
            }
            finally
            {
                m_isProcessingDispose = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
