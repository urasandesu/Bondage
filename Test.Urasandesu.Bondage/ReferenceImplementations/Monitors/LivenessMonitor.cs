/* 
 * File: LivenessMonitor.cs
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
    class LivenessMonitor : ApplicationMonitor<ILivenessMonitorBundler>
    {
        [Start]
        [OnEventDoAction(typeof(Construct), nameof(HandleConstruct))]
        public class Progressing4d2cfbd1fba94744b932e23c40c04961 : MonitorState { }

        [@Hot]
        [OnEventDoAction(typeof(Ack), nameof(HandleAck))]
        [OnEventGotoState(typeof(Notify), typeof(Progressed))]
        [IgnoreEvents(typeof(ClientReq))]
        public class Progressing : MonitorState { }

        void HandleAck()
        {
            Bundler.HandleAck((Ack)ReceivedEvent);
        }

        [@Cold]
        [OnEventDoAction(typeof(ClientReq), nameof(HandleClientReq))]
        [OnEventGotoState(typeof(Notify), typeof(Progressing))]
        [IgnoreEvents(typeof(Ack))]
        public class Progressed : MonitorState { }

        void HandleClientReq()
        {
            Bundler.HandleClientReq((ClientReq)ReceivedEvent);
        }
    }
}
