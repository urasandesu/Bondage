/* 
 * File: PSharpRuntimeMixin.cs
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
using System.Runtime.CompilerServices;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    public static class PSharpRuntimeMixin
    {
        public static void InvokeMonitor(this PSharpRuntime @this, MonitorId target, Event e)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            @this.InvokeMonitor(target.MonitorType, e);
        }

        public static void RemoteInvokeMonitor(this PSharpRuntime @this, MonitorId target, Event e)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (@this.NetworkProvider is ICommunicationProvider networkProvider2)
            {
                networkProvider2.RemoteMonitor(target, e);
                return;
            }

            @this.InvokeMonitor(target, e);
        }

        public static MachineId NewMachine(this PSharpRuntime @this, Type type)
        {
            return NewMachine(@this, type, null);
        }

        public static MachineId NewMachine(this PSharpRuntime @this, Type type, Event e)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var machineId = @this.CreateMachine(type);
            if (e != null)
                @this.SendEvent(machineId, e);
            return machineId;
        }

        public static MonitorId NewMonitor(this PSharpRuntime @this, Type type)
        {
            return NewMonitor(@this, type, null);
        }

        public static MonitorId NewMonitor(this PSharpRuntime @this, Type type, Event e)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var monitorId = new MonitorId(@this, type);
            if (!GetRegisteredMonitors(@this).TryAdd(monitorId, null))
                return monitorId;

            ms_extProps.GetOrCreateValue(@this).LastCreatedMonitorId = monitorId;
            @this.RegisterMonitor(type);
            if (e != null)
                @this.InvokeMonitor(monitorId, e);
            return monitorId;
        }

        public static TResult RemoteDoCommunication<TResult>(this PSharpRuntime @this, CommunicationId target, Func<object[], TResult> func, params object[] args)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return
                @this.NetworkProvider is ICommunicationProvider networkProvider2 ?
                    (TResult)networkProvider2.RemoteDoCommunication(target, func.Method.ToString(), args) :
                    func(args);
        }

        static readonly ConditionalWeakTable<PSharpRuntime, ExtendedProperties> ms_extProps = new ConditionalWeakTable<PSharpRuntime, ExtendedProperties>();
        class ExtendedProperties
        {
            public AdditionalConfiguration AdditionalConfiguration { get; set; }
            public MonitorId LastCreatedMonitorId { get; set; }
            public ConcurrentDictionary<MachineId, MonitorId> MonitorIdMap { get; } = new ConcurrentDictionary<MachineId, MonitorId>();
            public ConcurrentDictionary<MonitorId, object> RegisteredMonitors { get; } = new ConcurrentDictionary<MonitorId, object>();
        }

        public static AdditionalConfiguration GetAdditionalConfiguration(this PSharpRuntime @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return ms_extProps.GetOrCreateValue(@this).AdditionalConfiguration;
        }

        public static void SetAdditionalConfiguration(this PSharpRuntime @this, AdditionalConfiguration value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            ms_extProps.GetOrCreateValue(@this).AdditionalConfiguration = value;
        }

        internal static MonitorId GetLastCreatedMonitorId(this PSharpRuntime @this)
        {
            return ms_extProps.GetOrCreateValue(@this).LastCreatedMonitorId;
        }

        internal static MonitorId GetMonitorId(this PSharpRuntime @this, MachineId internalMonitorId)
        {
            return ms_extProps.GetOrCreateValue(@this).MonitorIdMap.TryGetValue(internalMonitorId, out var result) ? result : null;
        }

        internal static void SetMonitorId(this PSharpRuntime @this, MachineId internalMonitorId, MonitorId monitorId)
        {
            ms_extProps.GetOrCreateValue(@this).MonitorIdMap.AddOrUpdate(internalMonitorId, monitorId, (key, value) => monitorId);
        }

        internal static ConcurrentDictionary<MonitorId, object> GetRegisteredMonitors(this PSharpRuntime @this)
        {
            return ms_extProps.GetOrCreateValue(@this).RegisteredMonitors;
        }
    }
}
