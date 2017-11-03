/* 
 * File: SafetyMonitorReceiver.cs
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
using System.Collections.Generic;
using System.Linq;
using Urasandesu.Bondage;

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Monitors
{
    public class SafetyMonitorReceiver : MethodizedMonitorReceiver<ISafetyMonitorBundler>, ISafetyMonitorReceiver
    {
        MessageCollection m_messages;
        Dictionary<MachineId, bool> m_replicas;

        public void HandleConfigure(ConfigureSafetyMonitor e)
        {
            m_messages = e.Messages;
            Self.Established();
        }

        public void HandleHandshake(HandshakeSafetyMonitor e)
        {
            var storageNodes = e.StorageNodes;
            m_replicas = new Dictionary<MachineId, bool>();
            foreach (var storageNode in storageNodes)
                m_replicas.Add(storageNode.Id, false);
            Self.Checking();
        }

        public void HandleLogUpdated(LogUpdated e)
        {
            var storageNode = e.StorageNode;
            var log = e.Log;
            m_replicas[storageNode.Id] = true;
            lock (m_messages)
                m_messages.Add(new Message<LogUpdated>() { Id = Id, Event = e, Value = $"storage node: { storageNode.Id }, log: { log }" });
        }

        public void HandleAck(Ack e)
        {
            lock (m_messages)
                m_messages.Add(new Message<Ack>() { Id = Id, Event = new Ack(), Value = $"ack" });
            Assert(m_replicas.All(_ => _.Value));
            foreach (var snId in m_replicas.Keys.ToArray())
                m_replicas[snId] = false;
        }
    }
}
