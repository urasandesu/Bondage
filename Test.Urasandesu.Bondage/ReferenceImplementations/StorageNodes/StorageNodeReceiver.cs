/* 
 * File: StorageNodeReceiver.cs
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



using System;
using Urasandesu.Bondage;

namespace Test.Urasandesu.Bondage.ReferenceImplementations.StorageNodes
{
    class StorageNodeReceiver : MethodizedMachineReceiver<IStorageNodeBundler>, IStorageNodeReceiver
    {
        MessageCollection m_messages;
        ISafetyMonitorSender m_safetyMonitor;
        IServerSender m_server;
        long m_log = -1;

        public virtual void HandleConfigure(ConfigureStorageNode e)
        {
            m_messages = e.Messages;
            m_safetyMonitor = e.SafetyMonitor;
            Self.Established();
        }

        public virtual void HandleHandshake(HandshakeStorageNode e)
        {
            m_server = e.Server;
            Self.Active();
        }

        public virtual void HandleReplReq(ReplReq e)
        {
            var log = DateTime.Now.Ticks;
            var data = e.Data;
            m_safetyMonitor.LogUpdated(new LogUpdated(Id, log));
            lock (m_messages)
                m_messages.Add(new Message<ReplReq>() { Id = Id, Event = e, Value = $"storage node: { Id }, data: { data }, log: { log }" });
            m_log = log;
        }

        public virtual void HandleTimeout(Timeout e)
        {
            lock (m_messages)
                m_messages.Add(new Message<Timeout>() { Id = Id, Event = e, Value = $"storage node: { Id }, log: { m_log }" });
            m_server.Sync(new Sync(Self, m_log));
        }
    }
}
