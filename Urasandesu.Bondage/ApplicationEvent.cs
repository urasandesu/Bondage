/* 
 * File: ApplicationEvent.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using ST = System.Threading;

namespace Urasandesu.Bondage
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "EventWaitHandle resource can be collected by garbage collector.")]
    [DataContract]
    public abstract class ApplicationEvent : Event
    {
        [DataMember]
        public EventId Id { get; private set; }



        internal void LinkTo(RuntimeHost runtimeHost)
        {
            Id.LinkTo(runtimeHost.Id);
            OnLinkedTo(runtimeHost);
        }

        protected virtual void OnLinkedTo(RuntimeHost runtimeHost)
        {
        }



        ST::AutoResetEvent m_autoResetEvent;

        protected ApplicationEvent()
        {
            Id = new EventId();
            m_autoResetEvent = new ST::AutoResetEvent(false);
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { SignalCore, WaitToSignalCore });
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext ctx)
        {
            var ctor = typeof(Event).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            ctor.Invoke(this, null);
            m_autoResetEvent = new ST::AutoResetEvent(false);
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { SignalCore, WaitToSignalCore });
            OnDeserializedCore(ctx);
        }

        protected virtual void OnDeserializedCore(StreamingContext ctx)
        {
        }



        public TSender GetSender<TSender>(MonitorId monitorId) where TSender : class, IMethodizedMonitorSender
        {
            return RuntimeHost.GetSender<TSender>(Id, monitorId);
        }

        public void SetSender<TSender>(ref MonitorId monitorId, TSender monitor) where TSender : class, IMethodizedMonitorSender
        {
            RuntimeHost.SetSender(ref monitorId, monitor);
        }

        public TSender[] GetSender<TSender>(MonitorId[] monitorIds) where TSender : class, IMethodizedMonitorSender
        {
            return RuntimeHost.GetSender<TSender>(Id, monitorIds);
        }

        public void SetSender<TSender>(ref MonitorId[] monitorIds, TSender[] monitors) where TSender : class, IMethodizedMonitorSender
        {
            RuntimeHost.SetSender(ref monitorIds, monitors);
        }



        public TSender GetSender<TSender>(MachineId machineId) where TSender : class, IMethodizedMachineSender
        {
            return RuntimeHost.GetSender<TSender>(Id, machineId);
        }

        public void SetSender<TSender>(ref MachineId machineId, TSender machine) where TSender : class, IMethodizedMachineSender
        {
            RuntimeHost.SetSender(ref machineId, machine);
        }

        public TSender[] GetSender<TSender>(MachineId[] machineIds) where TSender : class, IMethodizedMachineSender
        {
            return RuntimeHost.GetSender<TSender>(Id, machineIds);
        }

        public void SetSender<TSender>(ref MachineId[] machineIds, TSender[] machines) where TSender : class, IMethodizedMachineSender
        {
            RuntimeHost.SetSender(ref machineIds, machines);
        }



        public bool Signal()
        {
            return (bool)RuntimeHost.DoCommunication(Id, SignalCore);
        }

        internal object SignalCore(params object[] args)
        {
            return m_autoResetEvent.Set();
        }



        public virtual bool WaitToSignal(int millisecondsTimeout, bool exitContext)
        {
            return (bool)RuntimeHost.DoCommunication(Id, WaitToSignalCore, millisecondsTimeout, exitContext);
        }

        public virtual bool WaitToSignal(TimeSpan timeout)
        {
            return (bool)RuntimeHost.DoCommunication(Id, WaitToSignalCore, timeout);
        }

        public virtual bool WaitToSignal(int millisecondsTimeout)
        {
            return (bool)RuntimeHost.DoCommunication(Id, WaitToSignalCore, millisecondsTimeout);
        }

        public virtual bool WaitToSignal(TimeSpan timeout, bool exitContext)
        {
            return (bool)RuntimeHost.DoCommunication(Id, WaitToSignalCore, timeout, exitContext);
        }

        public virtual bool WaitToSignal()
        {
            return (bool)RuntimeHost.DoCommunication(Id, WaitToSignalCore);
        }

        internal object WaitToSignalCore(params object[] args)
        {
            if (args == null || args.Length == 0)
                return m_autoResetEvent.WaitOne();
            else if (args.Length == 1)
                if (args[0] is TimeSpan timeout)
                    return m_autoResetEvent.WaitOne(timeout);
                else if (args[0] is int millisecondsTimeout)
                    return m_autoResetEvent.WaitOne(millisecondsTimeout);
                else
                    throw new NotSupportedException();
            else if (args.Length == 2)
                if (args[0] is int millisecondsTimeout && args[1] is bool exitContext1)
                    return m_autoResetEvent.WaitOne(millisecondsTimeout, exitContext1);
                else if (args[0] is TimeSpan timeout && args[1] is bool exitContext2)
                    return m_autoResetEvent.WaitOne(timeout, exitContext2);
                else
                    throw new NotSupportedException();
            else
                throw new NotSupportedException();
        }

        public override string ToString()
        {
            return $"{{\"Id\":{ Id }}}";
        }
    }
}
