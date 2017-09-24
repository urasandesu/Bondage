/* 
 * File: IPublishableLogger.cs
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
using System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO
{
    public interface IPublishableLogger : ILogger
    {
        event MachineCreatedHandler MachineCreated;
        event MonitorCreatedHandler MonitorCreated;
        event DefaultHandler Default;
        event DequeuedHandler Dequeued;
        event EnqueuedHandler Enqueued;
        event ErrorHandler Error;
        event HaltHandler Halt;
        event MachineActionHandler MachineAction;
        event MachineActionHandledHandler MachineActionHandled;
        event MachineEventHandler MachineEvent;
        event MachineStateHandler MachineState;
        event MonitorActionHandler MonitorAction;
        event MonitorActionHandledHandler MonitorActionHandled;
        event MonitorEventHandler MonitorEvent;
        event MonitorStateHandler MonitorState;
        event PoppedHandler Popped;
        event PopUnhandledEventHandler PopUnhandledEvent;
        event PushedHandler Pushed;
        event RandomHandler Random;
        event ReceivedHandler Received;
        event SentHandler Sent;
        event StrategyErrorHandler StrategyError;
        event WaitedHandler Waited;

        void OnMachineActionHandled(MachineId machineId, string currentStateName, string actionName);
        void OnMonitorActionHandled(string monitorTypeName, MonitorId monitorId, string currentStateName, string actionName);
        void OnFailure(Exception ex);
    }
}