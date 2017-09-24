/* 
 * File: ServerReceiver.cs
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

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Servers
{
    class ServerReceiver : MethodizedMachineReceiver<IServerBundler>, IServerReceiver
    {
        MessageCollection m_messages;
        ISafetyMonitorSender m_safetyMonitor;
        ILivenessMonitorSender m_livenessMonitor;
        IClientSender m_client;
        IStorageNodeSender[] m_storageNodes;
        int m_dataToReplicate;
        long m_log;
#if FIXES_SAFETY_BUG
        HashSet<MachineId> m_numReplicas = new HashSet<MachineId>();
#else
        int m_numReplicas;
#endif

        public virtual void HandleConfigure(ConfigureServer e)
        {
            m_messages = e.Messages;
            m_safetyMonitor = e.SafetyMonitor;
            m_livenessMonitor = e.LivenessMonitor;
            Self.Established();
        }

        public virtual void HandleHandshake(HandshakeServer e)
        {
            m_client = e.Client;
            m_storageNodes = e.StorageNodes;
            Self.Active();
        }

        public virtual void HandleClientReq(ClientReq e)
        {
            m_livenessMonitor.ClientReq(e);
            m_dataToReplicate = e.DataToReplicate;
            foreach (var storageNode in m_storageNodes)
                storageNode.ReplReq(new ReplReq(m_dataToReplicate));
        }

        public virtual void HandleSync(Sync e)
        {
            var storageNode = e.StorageNode;
            var log = e.Log;
            if (!IsUpToDate(log))
            {
                m_messages.Add(new Message<Sync>() { Id = Id, Event = e, Value = $"request replication again: { storageNode.Id }, data: { m_dataToReplicate }" });
                storageNode.ReplReq(new ReplReq(m_dataToReplicate));
            }
            else
            {
#if FIXES_SAFETY_BUG
                if (!m_numReplicas.Add(snId))
                    return;

                if (m_numReplicas.Count == 3)
#else
                m_numReplicas++;
                if (m_numReplicas == 3)
#endif
                {
                    m_safetyMonitor.Ack(new Ack());
                    m_livenessMonitor.Ack(new Ack());
                    m_client.Ack(new Ack());
                    m_log = DateTime.Now.Ticks;
                    m_messages.Add(new Message<Sync>() { Id = Id, Event = e, Value = $"response ack: { m_client.Id }, log: { m_log }" });
#if FIXES_LIVENESS_BUG
#if FIXES_SAFETY_BUG
                    m_numReplicas.Clear();
#else
                    m_numReplicas = 0;
#endif
#endif
                }
            }
        }

        bool IsUpToDate(long log)
        {
            if (log <= m_log)
                return false;

            m_log = log;
            return true;
        }
    }
}
