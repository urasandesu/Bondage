/* 
 * File: StorageNodeBundler.cs
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

namespace Test.Urasandesu.Bondage.ReferenceImplementations.StorageNodes
{
    // This is the reference implementation that is an auto implementation for TBundler of Machine.
    class StorageNodeBundler : MethodizedMachineBundler<IStorageNodeReceiver>, IStorageNodeBundler
    {
        public StorageNodeBundler(RuntimeHost runtimeHost, MachineId id, IStorageNodeReceiver receiver) :
            base(runtimeHost, id, receiver)
        { }

        public void Configure(ConfigureStorageNode e)
        {
            RuntimeHost.SendEvent(Id, e);
        }

        public void Handshake(HandshakeStorageNode e)
        {
            RuntimeHost.SendEvent(Id, e);
        }

        public void ReplReq(ReplReq e)
        {
            RuntimeHost.SendEvent(Id, e);
        }

        public void Timeout(Timeout e)
        {
            RuntimeHost.SendEvent(Id, e);
        }

        public void Initialized()
        {
            GotoType(typeof(StorageNode.Initialized));
        }

        public void Established()
        {
            GotoType(typeof(StorageNode.Established));
        }

        public void Active()
        {
            GotoType(typeof(StorageNode.Active));
        }

        public void HandleConfigure(ConfigureStorageNode e)
        {
            Receiver.HandleConfigure(e);
            MachineHandledLog(nameof(HandleConfigure));
        }

        public void HandleHandshake(HandshakeStorageNode e)
        {
            Receiver.HandleHandshake(e);
            MachineHandledLog(nameof(HandleHandshake));
        }

        public void HandleReplReq(ReplReq e)
        {
            Receiver.HandleReplReq(e);
            MachineHandledLog(nameof(HandleReplReq));
        }

        public void HandleTimeout(Timeout e)
        {
            Receiver.HandleTimeout(e);
            MachineHandledLog(nameof(HandleTimeout));
        }
    }
}
