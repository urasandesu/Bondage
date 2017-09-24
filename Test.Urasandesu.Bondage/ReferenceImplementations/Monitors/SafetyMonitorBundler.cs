/* 
 * File: SafetyMonitorBundler.cs
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



using Urasandesu.Bondage;
using Urasandesu.Bondage.Internals;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Monitors
{
    // This is the reference implementation that is an auto implementation for TBundler of Monitor.
    class SafetyMonitorBundler : MethodizedMonitorBundler<ISafetyMonitorReceiver>, ISafetyMonitorBundler
    {
        public SafetyMonitorBundler(RuntimeHost runtimeHost, MonitorId id, ISafetyMonitorReceiver receiver) :
            base(runtimeHost, id, receiver)
        { }

        public void Configure(ConfigureSafetyMonitor e)
        {
            RuntimeHost.InvokeMonitor(Id, e);
        }

        public void Handshake(HandshakeSafetyMonitor e)
        {
            RuntimeHost.InvokeMonitor(Id, e);
        }

        public void LogUpdated(LogUpdated e)
        {
            RuntimeHost.InvokeMonitor(Id, e);
        }

        public void Ack(Ack e)
        {
            RuntimeHost.InvokeMonitor(Id, e);
        }

        public void Initialized()
        {
            GotoType(typeof(SafetyMonitor.Initialized));
        }

        public void Established()
        {
            GotoType(typeof(SafetyMonitor.Established));
        }

        public void Checking()
        {
            GotoType(typeof(SafetyMonitor.Checking));
        }

        public void HandleConfigure(ConfigureSafetyMonitor e)
        {
            Receiver.HandleConfigure(e);
        }

        public void HandleHandshake(HandshakeSafetyMonitor e)
        {
            Receiver.HandleHandshake(e);
        }

        public void HandleLogUpdated(LogUpdated e)
        {
            Receiver.HandleLogUpdated(e);
        }

        public void HandleAck(Ack e)
        {
            Receiver.HandleAck(e);
        }
    }
}
