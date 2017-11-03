/* 
 * File: MainStorageNodesController.cs
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
using System.Collections.Generic;
using Test.Urasandesu.Bondage.ReferenceImplementations;
using Test.Urasandesu.Bondage.ReferenceImplementations.StorageNodes;
using Urasandesu.Bondage;
using Urasandesu.Bondage.Application;
using Urasandesu.Bondage.Mixins.System;
using Urasandesu.NAnonym.Mixins.System;

namespace Test.Urasandesu.Bondage.Application.ReferenceImplementations.StorageNodes
{
    class MainStorageNodesController : ApplicationController
    {
        [Dependency]
        public IProcessExecutor ProcessExecutor { private get; set; }

        public void Load(MainStorageNodesViewModel vm, string[] args)
        {
            var messages = new MessageCollection();
            vm.Messages = messages;
            vm.Context = args[0].FromJson<DistributedStorageContext>();
            NewStorageNodes(vm.Context, messages);

            ProcessExecutor.StartProcess(@"..\..\..\DistributedStorage.Remoting.Clients\bin\Debug\DistributedStorage.Remoting.Clients.exe", vm.Context.ToJson().ToCommandLineArgument());
        }

        public void NewStorageNodes(DistributedStorageContext ctx, MessageCollection messages)
        {
            var configure = new ConfigureStorageNode(messages, ctx.SafetyMonitor);

            var storageNodes = new List<IStorageNodeSender>();
            for (var i = 0; i < 3; i++)
            {
                var storageNode = RuntimeHost.New(MachineInterface.Sender<IStorageNodeSender>().Bundler<IStorageNodeBundler>().Receiver<StorageNodeReceiver>());
                storageNode.Configure(configure);
                storageNode.Handshake(new HandshakeStorageNode(ctx.Server));
                storageNodes.Add(storageNode);
            }
            ctx.SafetyMonitor.Handshake(new HandshakeSafetyMonitor(storageNodes.ToArray()));
            foreach (var storageNode in storageNodes)
            {
                var timer = RuntimeHost.New(MachineInterface.Sender<ITimerSender>().Bundler<ITimerBundler>().Receiver<TimerReceiver>());
                timer.Configure(new ConfigureTimer(storageNode));
            }
            ctx.StorageNodes = storageNodes.ToArray();
        }
    }
}
