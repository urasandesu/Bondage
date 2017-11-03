/* 
 * File: MainMonitorsController.cs
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
using Test.Urasandesu.Bondage.ReferenceImplementations.Monitors;
using Urasandesu.Bondage;
using Urasandesu.Bondage.Application;
using Urasandesu.Bondage.Mixins.System;
using Urasandesu.NAnonym.Mixins.System;

namespace Test.Urasandesu.Bondage.Application.ReferenceImplementations.Monitors
{
    class MainMonitorsController : ApplicationController
    {
        [Dependency]
        public IProcessExecutor ProcessExecutor { private get; set; }

        //[Dependency]
        //public SafetyMonitorReceiver SafetyMonitorReceiver { private get; set; }

        //[Dependency]
        //public LivenessMonitorReceiver LivenessMonitorReceiver { private get; set; }

        public void Load(MainMonitorsViewModel vm)
        {
            var messages = new MessageCollection();
            var ctx = new DistributedStorageContext();
            vm.Messages = messages;
            vm.Context = ctx;
            NewMonitors(ctx, messages);

            ProcessExecutor.StartProcess(@"..\..\..\DistributedStorage.Remoting.Servers\bin\Debug\DistributedStorage.Remoting.Servers.exe", ctx.ToJson().ToCommandLineArgument());
        }

        public void NewMonitors(DistributedStorageContext ctx, MessageCollection messages)
        {
            ctx.SafetyMonitor = RuntimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver<SafetyMonitorReceiver>());
            ctx.SafetyMonitor.Configure(new ConfigureSafetyMonitor(messages));
            ctx.LivenessMonitor = RuntimeHost.New(MonitorInterface.Sender<ILivenessMonitorSender>().Bundler<ILivenessMonitorBundler>().Receiver<LivenessMonitorReceiver>());
        }
    }
}
