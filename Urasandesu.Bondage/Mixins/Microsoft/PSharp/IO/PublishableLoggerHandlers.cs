/* 
 * File: PublishableLoggerHandlers.cs
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

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO
{
    public delegate void MachineCreatedHandler(MachineId machineId);
    public delegate void MonitorCreatedHandler(string monitorTypeName, MonitorId monitorId);
    public delegate void DefaultHandler(MachineId machineId, string currentStateName);
    public delegate void DequeuedHandler(MachineId machineId, string currentStateName, string eventName);
    public delegate void EnqueuedHandler(MachineId machineId, string eventName);
    public delegate void ErrorHandler(string text);
    public delegate void HaltHandler(MachineId machineId, int inboxSize);
    public delegate void MachineActionHandler(MachineId machineId, string currentStateName, string actionName);
    public delegate void MachineActionHandledHandler(MachineId machineId, string currentStateName, string actionName);
    public delegate void MachineEventHandler(MachineId machineId, string currentStateName, string eventName);
    public delegate void MachineStateHandler(MachineId machineId, string stateName, bool isEntry);
    public delegate void MonitorActionHandler(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName);
    public delegate void MonitorActionHandledHandler(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName);
    public delegate void MonitorEventHandler(string monitorTypeName, MonitorId monitorId, string currentStateName, string eventName, bool isProcessing);
    public delegate void MonitorStateHandler(string monitorTypeName, MonitorId monitorId, string stateName, bool isEntry, bool? isInHotState);
    public delegate void PoppedHandler(MachineId machineId, string currentStateName, string restoredStateName);
    public delegate void PopUnhandledEventHandler(MachineId machineId, string currentStateName, string eventName);
    public delegate void PushedHandler(MachineId machineId, string currentStateName, string newStateName);
    public delegate void RandomHandler(MachineId machineId, object result);
    public delegate void ReceivedHandler(MachineId machineId, string currentStateName, string eventName, bool wasBlocked);
    public delegate void SentHandler(MachineId targetMachineId, MachineId senderId, string senderStateName, string eventName, Guid? operationGroupId, bool isTargetHalted);
    public delegate void StrategyErrorHandler(SchedulingStrategy strategy, string strategyDescription);
    public delegate void WaitedHandler(MachineId machineId, string currentStateName, string eventNames);
}