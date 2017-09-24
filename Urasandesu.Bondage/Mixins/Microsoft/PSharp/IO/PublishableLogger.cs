/* 
 * File: PublishableLogger.cs
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
using Microsoft.PSharp.IO;
using Microsoft.PSharp.Utilities;
using System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO
{
    public abstract class PublishableLogger : StateMachineLogger, IPublishableLogger
    {
        public PublishableLogger(int loggingVerbosity = 2) :
            base(loggingVerbosity)
        {
            Configuration = Configuration.Create();
        }

        public event MachineCreatedHandler MachineCreated;
        public event MonitorCreatedHandler MonitorCreated;
        public event DefaultHandler Default;
        public event DequeuedHandler Dequeued;
        public event EnqueuedHandler Enqueued;
        public event ErrorHandler Error;
        public event HaltHandler Halt;
        public event MachineActionHandler MachineAction;
        public event MachineActionHandledHandler MachineActionHandled;
        public event MachineEventHandler MachineEvent;
        public event MachineStateHandler MachineState;
        public event MonitorActionHandler MonitorAction;
        public event MonitorActionHandledHandler MonitorActionHandled;
        public event MonitorEventHandler MonitorEvent;
        public event MonitorStateHandler MonitorState;
        public event PoppedHandler Popped;
        public event PopUnhandledEventHandler PopUnhandledEvent;
        public event PushedHandler Pushed;
        public event RandomHandler Random;
        public event ReceivedHandler Received;
        public event SentHandler Sent;
        public event StrategyErrorHandler StrategyError;
        public event WaitedHandler Waited;

        public override void OnCreateMachine(MachineId machineId)
        {
            base.OnCreateMachine(machineId);
            MachineCreated?.Invoke(machineId);
        }
        public sealed override void OnCreateMonitor(string monitorTypeName, MachineId internalMonitorId)
        {
            base.OnCreateMonitor(monitorTypeName, internalMonitorId);
            var runtime = internalMonitorId.Runtime;
            var monitorId = runtime.GetLastCreatedMonitorId();
            runtime.SetMonitorId(internalMonitorId, monitorId);
            OnCreateMonitor(monitorTypeName, monitorId);
        }
        public virtual void OnCreateMonitor(string monitorTypeName, MonitorId monitorId)
        {
            MonitorCreated?.Invoke(monitorTypeName, monitorId);
        }
        public override void OnDefault(MachineId machineId, string currentStateName)
        {
            base.OnDefault(machineId, currentStateName);
            Default?.Invoke(machineId, currentStateName);
        }
        public override void OnDequeue(MachineId machineId, string currentStateName, string eventName)
        {
            base.OnDequeue(machineId, currentStateName, eventName);
            Dequeued?.Invoke(machineId, currentStateName, eventName);
        }
        public override void OnEnqueue(MachineId machineId, string currentStateName, string eventName)
        {
            base.OnEnqueue(machineId, currentStateName, eventName);
            Enqueued?.Invoke(machineId, currentStateName, eventName);
        }
        public override void OnError(string text)
        {
            base.OnError(text);
            Error?.Invoke(text);
        }
        public override void OnHalt(MachineId machineId, int inboxSize)
        {
            base.OnHalt(machineId, inboxSize);
            Halt?.Invoke(machineId, inboxSize);
        }
        public override void OnMachineAction(MachineId machineId, string currentStateName, string actionName)
        {
            base.OnMachineAction(machineId, currentStateName, actionName);
            MachineAction?.Invoke(machineId, currentStateName, actionName);
        }
        public virtual void OnMachineActionHandled(MachineId machineId, string currentStateName, string actionName)
        {
            if (IsVerbose)
                WriteLine($"<HandledLog> Machine '{ machineId }' in state '{ currentStateName }' handled action '{ actionName }'.");
            MachineActionHandled?.Invoke(machineId, currentStateName, actionName);
        }
        public override void OnMachineEvent(MachineId machineId, string currentStateName, string eventName)
        {
            base.OnMachineEvent(machineId, currentStateName, eventName);
            MachineEvent?.Invoke(machineId, currentStateName, eventName);
        }
        public override void OnMachineState(MachineId machineId, string stateName, bool isEntry)
        {
            base.OnMachineState(machineId, stateName, isEntry);
            MachineState?.Invoke(machineId, stateName, isEntry);
        }
        public sealed override void OnMonitorAction(string monitorTypeName, MachineId internalMonitorId, string currentStateName, string actionName)
        {
            base.OnMonitorAction(monitorTypeName, internalMonitorId, currentStateName, actionName);
            var runtime = internalMonitorId.Runtime;
            var monitorId = runtime.GetMonitorId(internalMonitorId);
            OnMonitorAction(monitorTypeName, monitorId, currentStateName, actionName);
        }
        public virtual void OnMonitorAction(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName)
        {
            MonitorAction?.Invoke(monitorTypeName, monitorId, currentStateName, actionName);
        }
        public virtual void OnMonitorActionHandled(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName)
        {
            if (IsVerbose)
                WriteLine($"<MonitorLog> Monitor '{ monitorTypeName }' with id '{ monitorId }' in state '{ currentStateName }' handled action '{ actionName }'");
            MonitorActionHandled?.Invoke(monitorTypeName, monitorId, currentStateName, actionName);
        }
        public sealed override void OnMonitorEvent(string monitorTypeName, MachineId internalMonitorId, string currentStateName, string eventName, bool isProcessing)
        {
            base.OnMonitorEvent(monitorTypeName, internalMonitorId, currentStateName, eventName, isProcessing);
            var runtime = internalMonitorId.Runtime;
            var monitorId = runtime.GetMonitorId(internalMonitorId);
            OnMonitorEvent(monitorTypeName, monitorId, currentStateName, eventName, isProcessing);
        }
        public virtual void OnMonitorEvent(string monitorTypeName, MonitorId monitorId, string currentStateName, string eventName, bool isProcessing)
        {
            MonitorEvent?.Invoke(monitorTypeName, monitorId, currentStateName, eventName, isProcessing);
        }
        public sealed override void OnMonitorState(string monitorTypeName, MachineId internalMonitorId, string stateName, bool isEntry, bool? isInHotState)
        {
            base.OnMonitorState(monitorTypeName, internalMonitorId, stateName, isEntry, isInHotState);
            var runtime = internalMonitorId.Runtime;
            var monitorId = runtime.GetMonitorId(internalMonitorId);
            OnMonitorState(monitorTypeName, monitorId, stateName, isEntry, isInHotState);
        }
        public virtual void OnMonitorState(string monitorTypeName, MonitorId monitorId, string stateName, bool isEntry, bool? isInHotState)
        {
            MonitorState?.Invoke(monitorTypeName, monitorId, stateName, isEntry, isInHotState);
        }
        public override void OnPop(MachineId machineId, string currentStateName, string restoredStateName)
        {
            base.OnPop(machineId, currentStateName, restoredStateName);
            Popped?.Invoke(machineId, currentStateName, restoredStateName);
        }
        public override void OnPopUnhandledEvent(MachineId machineId, string currentStateName, string eventName)
        {
            base.OnPopUnhandledEvent(machineId, currentStateName, eventName);
            PopUnhandledEvent?.Invoke(machineId, currentStateName, eventName);
        }
        public override void OnPush(MachineId machineId, string currentStateName, string newStateName)
        {
            base.OnPush(machineId, currentStateName, newStateName);
            Pushed?.Invoke(machineId, currentStateName, newStateName);
        }
        public override void OnRandom(MachineId machineId, object result)
        {
            base.OnRandom(machineId, result);
            Random?.Invoke(machineId, result);
        }
        public override void OnReceive(MachineId machineId, string currentStateName, string eventName, bool wasBlocked)
        {
            base.OnReceive(machineId, currentStateName, eventName, wasBlocked);
            Received?.Invoke(machineId, currentStateName, eventName, wasBlocked);
        }
        public override void OnSend(MachineId targetMachineId, string targetStateName, MachineId senderId, string senderStateName, string eventName, Guid? operationGroupId, bool isTargetHalted)
        {
            base.OnSend(targetMachineId, targetStateName, senderId, senderStateName, eventName, operationGroupId, isTargetHalted);
            Sent?.Invoke(targetMachineId, targetStateName, senderId, senderStateName, eventName, operationGroupId, isTargetHalted);
        }
        public override void OnStrategyError(SchedulingStrategy strategy, string strategyDescription)
        {
            base.OnStrategyError(strategy, strategyDescription);
            StrategyError?.Invoke(strategy, strategyDescription);
        }
        public override void OnWait(MachineId machineId, string currentStateName, string eventNames)
        {
            base.OnWait(machineId, currentStateName, eventNames);
            Waited?.Invoke(machineId, currentStateName, eventNames);
        }
        public virtual void OnFailure(Exception ex)
        { }
    }
}