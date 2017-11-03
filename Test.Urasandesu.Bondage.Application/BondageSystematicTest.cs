/* 
 * File: BondageSystematicTest.cs
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
using Microsoft.PSharp;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Test.Urasandesu.Bondage.Application.ReferenceImplementations.Clients;
using Test.Urasandesu.Bondage.Application.ReferenceImplementations.Monitors;
using Test.Urasandesu.Bondage.Application.ReferenceImplementations.Servers;
using Test.Urasandesu.Bondage.Application.ReferenceImplementations.StorageNodes;
using Test.Urasandesu.Bondage.ReferenceImplementations;
using Test.Urasandesu.Bondage.ReferenceImplementations.Servers;
using Urasandesu.Bondage.Application;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices;

namespace Test.Urasandesu.Bondage.Application
{
    [TestFixture]
    public class BondageSystematicTest
    {
        [MyRetry(5)]
        [Test]
        public void Can_find_safety_or_liveness_bug()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(300).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var processExecutorMock = new Mock<IProcessExecutor>();
                var container = runtimeHost.Container;
                container.RegisterType<IProcessExecutor>(new InjectionFactory((_1, _2, _3) => processExecutorMock.Object)).
                          RegisterType<ServerReceiver, ServerReceiverWithSafetyAndLivenessBug>();

                var ctx = runtimeHost.New<DistributedStorageContext>();
                var messages = new MessageCollection();

                container.Resolve<MainMonitorsController>().NewMonitors(ctx, messages);
                container.Resolve<MainServersController>().NewServer(ctx, messages);
                container.Resolve<MainStorageNodesController>().NewStorageNodes(ctx, messages);
                container.Resolve<MainClientsController>().NewClient(ctx, messages);
            });


            // Act
            engine.RunAndEmitTraces();


            // Assert
            Assert.GreaterOrEqual(engine.TestReport.NumOfFoundBugs, 1);
            Assert.That(engine.ReportFully(), Does.Match(@"(<ErrorLog> Detected an assertion failure)|(<ErrorLog> .* detected potential liveness bug in hot state)"));
        }

        [MyRetry(5)]
        [Test]
        public void Can_find_safety_bug()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(300).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var processExecutorMock = new Mock<IProcessExecutor>();
                var container = runtimeHost.Container;
                container.RegisterType<IProcessExecutor>(new InjectionFactory((_1, _2, _3) => processExecutorMock.Object)).
                          RegisterType<ServerReceiver, ServerReceiverWithSafetyBug>();

                var ctx = runtimeHost.New<DistributedStorageContext>();
                var messages = new MessageCollection();

                container.Resolve<MainMonitorsController>().NewMonitors(ctx, messages);
                container.Resolve<MainServersController>().NewServer(ctx, messages);
                container.Resolve<MainStorageNodesController>().NewStorageNodes(ctx, messages);
                container.Resolve<MainClientsController>().NewClient(ctx, messages);
            });


            // Act
            engine.RunAndEmitTraces();


            // Assert
            Assert.GreaterOrEqual(engine.TestReport.NumOfFoundBugs, 1);
            Assert.That(engine.ReportFully(), Does.Match(@"(<ErrorLog> Detected an assertion failure)"));
        }

        [MyRetry(10)]
        [Test]
        public void Can_find_liveness_bug()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(300).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var processExecutorMock = new Mock<IProcessExecutor>();
                var container = runtimeHost.Container;
                container.RegisterType<IProcessExecutor>(new InjectionFactory((_1, _2, _3) => processExecutorMock.Object)).
                          RegisterType<ServerReceiver, ServerReceiverWithLivenessBug>();

                var ctx = runtimeHost.New<DistributedStorageContext>();
                var messages = new MessageCollection();

                container.Resolve<MainMonitorsController>().NewMonitors(ctx, messages);
                container.Resolve<MainServersController>().NewServer(ctx, messages);
                container.Resolve<MainStorageNodesController>().NewStorageNodes(ctx, messages);
                container.Resolve<MainClientsController>().NewClient(ctx, messages);
            });


            // Act
            engine.RunAndEmitTraces();


            // Assert
            Assert.GreaterOrEqual(engine.TestReport.NumOfFoundBugs, 1);
            Assert.That(engine.ReportFully(), Does.Match(@"(<ErrorLog> .* detected potential liveness bug in hot state)"));
        }

        [Repeat(5)]
        [Test]
        public void Can_check_safety_and_liveness_bug_do_not_exist()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(300).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var processExecutorMock = new Mock<IProcessExecutor>();
                var container = runtimeHost.Container;
                container.RegisterType<IProcessExecutor>(new InjectionFactory((_1, _2, _3) => processExecutorMock.Object)).
                          RegisterType<ServerReceiver, ServerReceiverWithoutBug>();

                var ctx = runtimeHost.New<DistributedStorageContext>();
                var messages = new MessageCollection();

                container.Resolve<MainMonitorsController>().NewMonitors(ctx, messages);
                container.Resolve<MainServersController>().NewServer(ctx, messages);
                container.Resolve<MainStorageNodesController>().NewStorageNodes(ctx, messages);
                container.Resolve<MainClientsController>().NewClient(ctx, messages);
            });


            // Act
            engine.RunAndEmitTraces();


            // Assert
            Assert.AreEqual(0, engine.TestReport.NumOfFoundBugs);
        }
    }
}
