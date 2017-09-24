/* 
 * File: SafetyMonitor.cs
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
using Urasandesu.Bondage;
using Urasandesu.Bondage.Internals;

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Monitors
{
    // This is the reference implementation that is an auto implementation for Monitor.
    class SafetyMonitor : ApplicationMonitor<ISafetyMonitorBundler>
    {
        [Start]
        [OnEventDoAction(typeof(Construct), nameof(HandleConstruct))]
        public class Initializedd65a1bf2144142c399147acd5d116653 : MonitorState { }

        [OnEventDoAction(typeof(ConfigureSafetyMonitor), nameof(HandleConfigure))]
        [DeferEvents(typeof(HandshakeSafetyMonitor), typeof(LogUpdated), typeof(Ack))]
        public class Initialized : MonitorState { }

        void HandleConfigure()
        {
            Bundler.HandleConfigure((ConfigureSafetyMonitor)ReceivedEvent);
        }

        [OnEventDoAction(typeof(HandshakeSafetyMonitor), nameof(HandleHandshake))]
        [DeferEvents(typeof(LogUpdated), typeof(Ack))]
        public class Established : MonitorState { }

        void HandleHandshake()
        {
            Bundler.HandleHandshake((HandshakeSafetyMonitor)ReceivedEvent);
        }

        [OnEventDoAction(typeof(LogUpdated), nameof(HandleLogUpdated))]
        [OnEventDoAction(typeof(Ack), nameof(HandleAck))]
        public class Checking : MonitorState { }

        void HandleLogUpdated()
        {
            Bundler.HandleLogUpdated((LogUpdated)ReceivedEvent);
        }

        void HandleAck()
        {
            Bundler.HandleAck((Ack)ReceivedEvent);
        }
    }
}
