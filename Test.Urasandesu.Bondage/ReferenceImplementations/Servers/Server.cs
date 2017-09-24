/* 
 * File: Server.cs
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

namespace Test.Urasandesu.Bondage.ReferenceImplementations.Servers
{
    // This is the reference implementation that is an auto implementation for Machine.
    class Server : ApplicationMachine<IServerBundler>
    {
        [Start]
        [OnEventDoAction(typeof(Construct), nameof(HandleConstruct))]
        public class Initialized4a78595457e64fc5adc97d35da36d5c5 : MachineState { }

        [OnEventDoAction(typeof(ConfigureServer), nameof(HandleConfigure))]
        [DeferEvents(typeof(HandshakeServer), typeof(ClientReq), typeof(Sync))]
        public class Initialized : MachineState { }

        void HandleConfigure()
        {
            Bundler.HandleConfigure((ConfigureServer)ReceivedEvent);
        }

        [OnEventDoAction(typeof(HandshakeServer), nameof(HandleHandshake))]
        [DeferEvents(typeof(ClientReq), typeof(Sync))]
        public class Established : MachineState { }

        void HandleHandshake()
        {
            Bundler.HandleHandshake((HandshakeServer)ReceivedEvent);
        }

        [OnEventDoAction(typeof(ClientReq), nameof(HandleClientReq))]
        [OnEventDoAction(typeof(Sync), nameof(HandleSync))]
        public class Active : MachineState { }

        void HandleClientReq()
        {
            Bundler.HandleClientReq((ClientReq)ReceivedEvent);
        }

        void HandleSync()
        {
            Bundler.HandleSync((Sync)ReceivedEvent);
        }
    }
}
