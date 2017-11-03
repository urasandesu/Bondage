/* 
 * File: ClientReceiver.cs
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

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Clients
{
    public class ClientReceiver : MethodizedMachineReceiver<IClientBundler>, IClientReceiver
    {
        MessageCollection m_messages;
        IServerSender m_server;

        public virtual void HandleConfigure(ConfigureClient e)
        {
            m_messages = e.Messages;
            m_server = e.Server;
            Self.Active();
        }

        public virtual void EnterActive()
        {
            var dataToReplicate = RandomInteger(43);
            lock (m_messages)
                m_messages.Add(new Message<ReturnActive>() { Id = Id, Event = new ReturnActive(), Value = $"client: { Id }, data to replicate: { dataToReplicate }" });
            m_server.ClientReq(new ClientReq(dataToReplicate));
        }

        public virtual void EnterExit()
        {
            //if (Random())
            //{
                lock (m_messages)
                    m_messages.Add(new Message<Ack>() { Id = Id, Event = new Ack(), Value = $"halt client: { Id }" });
                Halt();
            //}
            //else
            //{
            //    Self.ReturnActiveImmediately(new ReturnActive());
            //}
        }
    }
}
