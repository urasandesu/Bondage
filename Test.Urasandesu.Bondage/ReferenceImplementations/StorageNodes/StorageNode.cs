/* 
 * File: StorageNode.cs
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
    // This is the reference implementation that is an auto implementation for Machine.
    class StorageNode : ApplicationMachine<IStorageNodeBundler>
    {
        [Start]
        [OnEventDoAction(typeof(Construct), nameof(HandleConstruct))]
        public class Initializedf2ffb4614a9546d7958dcf1c08b611c4 : MachineState { }

        [OnEventDoAction(typeof(ConfigureStorageNode), nameof(HandleConfigure))]
        public class Initialized : MachineState { }

        protected virtual void HandleConfigure()
        {
            Bundler.HandleConfigure((ConfigureStorageNode)ReceivedEvent);
        }

        [OnEventDoAction(typeof(HandshakeStorageNode), nameof(HandleHandshake))]
        public class Established : MachineState { }

        protected virtual void HandleHandshake()
        {
            Bundler.HandleHandshake((HandshakeStorageNode)ReceivedEvent);
        }

        [OnEventDoAction(typeof(ReplReq), nameof(HandleReplReq))]
        [OnEventDoAction(typeof(Timeout), nameof(HandleTimeout))]
        public class Active : MachineState { }

        protected virtual void HandleReplReq()
        {
            Bundler.HandleReplReq((ReplReq)ReceivedEvent);
        }

        protected virtual void HandleTimeout()
        {
            Bundler.HandleTimeout((Timeout)ReceivedEvent);
        }
    }
}
