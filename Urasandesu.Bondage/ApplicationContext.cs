/* 
 * File: ApplicationContext.cs
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
using System.Runtime.Serialization;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;

namespace Urasandesu.Bondage
{
    [DataContract]
    public abstract class ApplicationContext
    {
        [DataMember]
        public ContextId Id { get; private set; }



        protected ApplicationContext()
        {
            Id = new ContextId();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext ctx)
        {
            OnDeserializedCore(ctx);
        }

        protected virtual void OnDeserializedCore(StreamingContext ctx)
        {
        }



        internal void LinkTo(RuntimeHost runtimeHost, params object[] args)
        {
            Id.LinkTo(runtimeHost.Id);
            OnLinkedTo(runtimeHost, args);
        }

        protected virtual void OnLinkedTo(RuntimeHost runtimeHost, params object[] args)
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
    }
}
