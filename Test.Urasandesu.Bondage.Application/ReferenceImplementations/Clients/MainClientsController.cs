/* 
 * File: MainClientsController.cs
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



using Microsoft.Practices.Unity;
using Test.Urasandesu.Bondage.ReferenceImplementations;
using Test.Urasandesu.Bondage.ReferenceImplementations.Clients;
using Urasandesu.Bondage;
using Urasandesu.Bondage.Application;
using Urasandesu.Bondage.Mixins.System;

namespace Test.Urasandesu.Bondage.Application.ReferenceImplementations.Clients
{
    class MainClientsController : ApplicationController
    {
        [Dependency]
        public IProcessExecutor ProcessExecutor { private get; set; }

        //[Dependency]
        //public ClientReceiver ClientReceiver { private get; set; }

        public void Load(MainClientsViewModel vm, string[] args)
        {
            var messages = new MessageCollection();
            vm.Messages = messages;
            vm.Context = args[0].FromJson<DistributedStorageContext>();
            NewClient(vm.Context, messages);
        }

        public void NewClient(DistributedStorageContext ctx, MessageCollection messages)
        {
            ctx.Client = RuntimeHost.New(MachineInterface.Sender<IClientSender>().Bundler<IClientBundler>().Receiver<ClientReceiver>());
            ctx.Client.Configure(new ConfigureClient(messages, ctx.Server));
            ctx.Server.Handshake(new HandshakeServer(ctx.Client, ctx.StorageNodes));
        }
    }
}
