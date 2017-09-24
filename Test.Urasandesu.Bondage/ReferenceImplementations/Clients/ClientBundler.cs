/* 
 * File: ClientBundler.cs
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

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Clients
{
    // This is the reference implementation that is an auto implementation for TBundler of Machine.
    class ClientBundler : MethodizedMachineBundler<IClientReceiver>, IClientBundler
    {
        public ClientBundler(RuntimeHost runtimeHost, MachineId id, IClientReceiver receiver) :
            base(runtimeHost, id, receiver)
        { }

        public void Configure(ConfigureClient e)
        {
            RuntimeHost.SendEvent(Id, e);
        }

        public void Ack(Ack e)
        {
            RuntimeHost.SendEvent(Id, e);
        }

        public void Initialized()
        {
            GotoType(typeof(Client.Initialized));
        }

        public void Active()
        {
            GotoType(typeof(Client.Active));
        }

        public void Exit()
        {
            GotoType(typeof(Client.Exit));
        }

        public void ReturnActiveImmediately(ReturnActive e)
        {
            RaiseEvent(e);
        }

        public void HandleConfigure(ConfigureClient e)
        {
            Receiver.HandleConfigure(e);
            MachineHandledLog(nameof(HandleConfigure));
        }

        public void EnterActive()
        {
            Receiver.EnterActive();
            MachineHandledLog(nameof(EnterActive));
        }

        public void EnterExit()
        {
            Receiver.EnterExit();
            MachineHandledLog(nameof(EnterExit));
        }
    }
}
