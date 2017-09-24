/* 
 * File: LoggerPredicates.cs
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
using Microsoft.PSharp.Utilities;
using System;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;

namespace Urasandesu.Bondage
{
    public delegate bool MachineCreatedPredicate(MachineId machineId);
    public delegate bool MonitorCreatedPredicate(string monitorTypeName, MonitorId monitorId);
    public delegate bool DefaultPredicate(MachineId machineId, string currentStateName);
    public delegate bool DequeuedPredicate(MachineId machineId, string currentStateName, string eventName);
    public delegate bool EnqueuedPredicate(MachineId machineId, string currentStateName, string eventName);
    public delegate bool ErrorPredicate(string text);
    public delegate bool HaltPredicate(MachineId machineId, int inboxSize);
    public delegate bool MachineActionPredicate(MachineId machineId, string currentStateName, string actionName);
    public delegate bool MachineActionHandledPredicate(MachineId machineId, string currentStateName, string actionName);
    public delegate bool MachineEventPredicate(MachineId machineId, string currentStateName, string eventName);
    public delegate bool MachineStatePredicate(MachineId machineId, string stateName, bool isEntry);
    public delegate bool MonitorActionPredicate(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName);
    public delegate bool MonitorActionHandledPredicate(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName);
    public delegate bool MonitorEventPredicate(string monitorTypeName, MonitorId monitorId, string currentStateName, string eventName, bool isProcessing);
    public delegate bool MonitorStatePredicate(string monitorTypeName, MonitorId monitorId, string stateName, bool isEntry, bool? isInHotState);
    public delegate bool PoppedPredicate(MachineId machineId, string currentStateName, string restoredStateName);
    public delegate bool PopUnhandledEventPredicate(MachineId machineId, string currentStateName, string eventName);
    public delegate bool PushedPredicate(MachineId machineId, string currentStateName, string newStateName);
    public delegate bool RandomPredicate(MachineId machineId, object result);
    public delegate bool ReceivedPredicate(MachineId machineId, string currentStateName, string eventName, bool wasBlocked);
    public delegate bool SentPredicate(MachineId targetMachineId, string targetStateName, MachineId senderId, string senderStateName, string eventName, Guid? operationGroupId, bool isTargetHalted);
    public delegate bool StrategyErrorPredicate(SchedulingStrategy strategy, string strategyDescription);
    public delegate bool WaitedPredicate(MachineId machineId, string currentStateName, string eventNames);
}