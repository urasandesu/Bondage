/* 
 * File: BondageIntegrationTest.cs
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
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Test.Urasandesu.Bondage.ReferenceImplementations;
using Test.Urasandesu.Bondage.ReferenceImplementations.Clients;
using Test.Urasandesu.Bondage.ReferenceImplementations.Monitors;
using Test.Urasandesu.Bondage.ReferenceImplementations.Servers;
using Test.Urasandesu.Bondage.ReferenceImplementations.StorageNodes;
using Urasandesu.Bondage;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO;
using Urasandesu.Bondage.Mixins.System;
using Urasandesu.Enkidu;
using Urasandesu.NAnonym.Mixins.System;

namespace Test.Urasandesu.Bondage
{
    [TestFixture]
    public class BondageIntegrationTest
    {
        [Test]
        public void Monitor_should_notify_safety_bug_if_it_is_existing()
        {
            // Arrange
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            var safetyMonitor = runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));


            // Act
            var messages = new MessageCollection();
            safetyMonitor.Configure(new ConfigureSafetyMonitor(messages));

            var snId = runtime.NewMachine(typeof(StorageNode));
            safetyMonitor.Handshake(new HandshakeSafetyMonitor(new[] { snId }));


            // Assert
            var expectedExType = typeof(PSharpRuntime).Assembly.GetTypes().First(_ => _.FullName == "Microsoft.PSharp.AssertionFailureException");
            Assert.Throws(expectedExType, () => safetyMonitor.Ack(new Ack()));
        }

        [Test]
        public void Monitor_should_not_notify_safety_bug_if_it_is_not_existing()
        {
            // Arrange
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            var safetyMonitor = runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));


            // Act
            var messages = new MessageCollection();
            safetyMonitor.Configure(new ConfigureSafetyMonitor(messages));

            var snId = runtime.NewMachine(typeof(StorageNode));
            safetyMonitor.Handshake(new HandshakeSafetyMonitor(new[] { snId }));

            safetyMonitor.LogUpdated(new LogUpdated(snId, 42));


            // Assert
            Assert.DoesNotThrow(() => safetyMonitor.Ack(new Ack()));
        }



        [Test]
        public void Machine_should_transit_its_state_according_to_passed_event()
        {
            // Arrange
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled().WithVerbosityEnabled(2);
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            var logger = new SynchronizedLogger(new InMemoryLogger());
            var setIfHandledSync = logger.MachineActionHandledSet((_1, _2, actionName) => actionName == "HandleSync");
            logger.ApplySynchronization(setIfHandledSync);
            runtimeHost.SetLogger(logger);
            var server = runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));


            // Act
            var messages = new MessageCollection();
            var safetyMonitorMock = new Mock<ISafetyMonitorSender>();
            var livenessMonitorMock = new Mock<ILivenessMonitorSender>();
            server.Configure(new ConfigureServer(messages, safetyMonitorMock.Object, livenessMonitorMock.Object));

            var clientMock = new Mock<IClientSender>();
            var clientId = runtime.CreateMachine(typeof(Client));
            clientMock.SetupGet(_ => _.Id).Returns(clientId);
            var storageNodeMock = new Mock<IStorageNodeSender>();
            var storageNodeId = runtime.CreateMachine(typeof(StorageNode));
            storageNodeMock.Setup(_ => _.Id).Returns(storageNodeId);
            server.Handshake(new HandshakeServer(clientMock.Object, new[] { storageNodeMock.Object }));

            var clientReq = new ClientReq(42);
            server.ClientReq(clientReq);

            server.Sync(new Sync(storageNodeMock.Object, 0));


            // Assert
            Assert.That(logger.WaitForWriting(3000), Does.Contain("handled action 'HandleSync'"));
            livenessMonitorMock.Verify(_ => _.ClientReq(clientReq));
            storageNodeMock.Verify(_ => _.ReplReq(It.Is<ReplReq>(x => x.Data == clientReq.DataToReplicate)));
        }



        [Test]
        public void Monitor_can_be_created_faster_than_the_first_time()
        {
            AppDomain.CurrentDomain.RunAtIsolatedDomain(() =>
            {
                // Arrange
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);


                // Act
                var sw1 = Stopwatch.StartNew();
                runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));
                var elapsed1 = sw1.ElapsedTicks;

                var sw10 = Stopwatch.StartNew();
                for (var i = 0; i < 10; i++)
                    runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));
                var elapsed10 = sw10.ElapsedTicks;


                // Assert
                Assert.Less(elapsed10, elapsed1 / 10);
            });
        }



        [Test]
        public void Machine_can_be_created_faster_than_the_first_time()
        {
            AppDomain.CurrentDomain.RunAtIsolatedDomain(() =>
            {
                // Arrange
                var configuration = Configuration.Create();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);


                // Act
                var sw1 = Stopwatch.StartNew();
                runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));
                var elapsed1 = sw1.ElapsedTicks;

                var sw10 = Stopwatch.StartNew();
                for (var i = 0; i < 10; i++)
                    runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));
                var elapsed10 = sw10.ElapsedTicks;


                // Assert
                Assert.Less(elapsed10, elapsed1 / 10);
            });
        }



        [Test]
        public void Context_should_return_same_Monitor_if_it_is_in_local_and_single_application()
        {
            // Arrange
            var ctx = default(DistributedStorageContext);
            var expected = default(int);
            var actual = default(int);

            // Act
            {
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);

                ctx = runtimeHost.New<DistributedStorageContext>();
                ctx.SafetyMonitor = runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));
                expected = RuntimeHelpers.GetHashCode(ctx.SafetyMonitor);
            }

            {
                actual = RuntimeHelpers.GetHashCode(ctx.SafetyMonitor);
            }


            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Context_should_return_same_Monitor_if_it_is_in_remote_and_single_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var parameter = default(string);

            // Act
            {
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);

                var ctx = runtimeHost.New<DistributedStorageContext>();
                ctx.SafetyMonitor = runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));
                expected = RuntimeHelpers.GetHashCode(ctx.SafetyMonitor);
                parameter = ctx.ToJson();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            {
                var ctx = parameter.FromJson<DistributedStorageContext>();
                actual = RuntimeHelpers.GetHashCode(ctx.SafetyMonitor);
            }


            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Context_should_not_return_same_Monitor_if_it_is_in_remote_and_multi_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var expected_Assign = new MarshalByRefAction<int>(i => expected = i);
            var actual_Assign = new MarshalByRefAction<int>(i => actual = i);

            // Act
            AppDomain.CurrentDomain.RunAtIsolatedDomain((expected_Assign_, actual_Assign_) =>
            {
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);
                using (var networkProvider = new DomainCommunicationProvider(runtimeHost, "monitors"))
                {
                    var parameter = default(string);
                    {
                        runtimeHost.SetNetworkProvider(networkProvider);

                        var ctx = runtimeHost.New<DistributedStorageContext>();
                        ctx.SafetyMonitor = runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));
                        ctx.SafetyMonitor.Configure(new ConfigureSafetyMonitor(new MessageCollection()));
                        expected_Assign_.Invoke(RuntimeHelpers.GetHashCode(ctx.SafetyMonitor));
                        parameter = ctx.ToJson();
                    }

                    AppDomain.CurrentDomain.RunAtIsolatedDomain((actual_Assign__, parameter_) =>
                    {
                        var ctx = parameter_.FromJson<DistributedStorageContext>();
                        actual_Assign__.Invoke(RuntimeHelpers.GetHashCode(ctx.SafetyMonitor));
                        ctx.SafetyMonitor.Handshake(new HandshakeSafetyMonitor(new MachineId[0]));
                    }, actual_Assign_, parameter);
                }
            }, expected_Assign, actual_Assign);


            // Assert
            Assert.AreNotEqual(expected, actual);
        }



        [Test]
        public void Context_should_return_same_Machine_if_it_is_in_local_and_single_application()
        {
            // Arrange
            var ctx = default(DistributedStorageContext);
            var expected = default(int);
            var actual = default(int);

            // Act
            {
                var configuration = Configuration.Create();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);

                ctx = runtimeHost.New<DistributedStorageContext>();
                ctx.Server = runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));
                expected = RuntimeHelpers.GetHashCode(ctx.Server);
            }

            {
                actual = RuntimeHelpers.GetHashCode(ctx.Server);
            }


            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Context_should_return_same_Machine_if_it_is_in_remote_and_single_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var parameter = default(string);

            // Act
            {
                var configuration = Configuration.Create();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);

                var ctx = runtimeHost.New<DistributedStorageContext>();
                ctx.Server = runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));
                expected = RuntimeHelpers.GetHashCode(ctx.Server);
                parameter = ctx.ToJson();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            {
                var ctx = parameter.FromJson<DistributedStorageContext>();
                actual = RuntimeHelpers.GetHashCode(ctx.Server);
            }


            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Context_should_not_return_same_Machine_if_it_is_in_remote_and_multi_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var expected_Assign = new MarshalByRefAction<int>(i => expected = i);
            var actual_Assign = new MarshalByRefAction<int>(i => actual = i);

            // Act
            AppDomain.CurrentDomain.RunAtIsolatedDomain((expected_Assign_, actual_Assign_) =>
            {
                var configuration = Configuration.Create().WithVerbosityEnabled(2);
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);
                var logger = new SynchronizedLogger(new InMemoryLogger());
                var setIfHandledHandshake = logger.MachineActionHandledSet((_1, _2, actionName) => actionName == "HandleHandshake");
                logger.ApplySynchronization(setIfHandledHandshake);
                runtimeHost.SetLogger(logger);
                using (var networkProvider = new DomainCommunicationProvider(runtimeHost, "servers"))
                {
                    var parameter = default(string);
                    {
                        runtimeHost.SetNetworkProvider(networkProvider);

                        var ctx = runtimeHost.New<DistributedStorageContext>();
                        ctx.Server = runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));
                        ctx.Server.Configure(new ConfigureServer(new MessageCollection(), null, null));
                        expected_Assign_.Invoke(RuntimeHelpers.GetHashCode(ctx.Server));
                        parameter = ctx.ToJson();
                    }

                    AppDomain.CurrentDomain.RunAtIsolatedDomain((actual_Assign__, parameter_) =>
                    {
                        var ctx = parameter_.FromJson<DistributedStorageContext>();
                        actual_Assign__.Invoke(RuntimeHelpers.GetHashCode(ctx.Server));
                        ctx.Server.Handshake(new HandshakeServer(null, new IStorageNodeSender[0]));
                    }, actual_Assign_, parameter);

                    logger.WaitForWriting(1000);
                }
            }, expected_Assign, actual_Assign);


            // Assert
            Assert.AreNotEqual(expected, actual);
        }



        [Test]
        public void System_should_halt_normally_if_events_are_sent_by_intended_order()
        {
            var monitorsNetworkProvider = default(DomainCommunicationProvider);
            var serversNetworkProvider = default(DomainCommunicationProvider);
            var storageNodesNetworkProvider = default(DomainCommunicationProvider);
            var clientsNetworkProvider = default(DomainCommunicationProvider);
            try
            {
                // Arrange
                var sctx = default(string);
                var logger = new SynchronizedLogger(new InMemoryLogger());

                sctx = StartMonitors(sctx, logger, out monitorsNetworkProvider);
                sctx = StartServers(sctx, logger, out serversNetworkProvider);
                sctx = StartStorageNodes(sctx, logger, out storageNodesNetworkProvider);
                sctx = StartClients(sctx, logger, out clientsNetworkProvider);


                // Act
                StartTimers(sctx, logger);


                // Assert
                Assert.DoesNotThrow(() => logger.WaitForWriting(1));
            }
            finally
            {
                monitorsNetworkProvider?.Dispose();
                serversNetworkProvider?.Dispose();
                storageNodesNetworkProvider?.Dispose();
                clientsNetworkProvider?.Dispose();
            }
        }

        static string StartMonitors(string sctx, IPublishableLogger logger, out DomainCommunicationProvider networkProvider)
        {
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled().WithVerbosityEnabled(2);
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            networkProvider = new DomainCommunicationProvider(runtimeHost, "monitors");
            runtimeHost.SetNetworkProvider(networkProvider);
            runtimeHost.SetLogger(logger);

            var messages = new MessageCollection();
            messages.CollectionChanged += (sender, e) => logger.WriteLine(e.NewItems[0].ToString());
            var ctx = runtimeHost.New<DistributedStorageContext>();
            NewMonitors(runtimeHost, ctx, messages);
            return ctx.ToJson();
        }

        static void NewMonitors(RuntimeHost runtimeHost, DistributedStorageContext ctx, MessageCollection messages)
        {
            ctx.SafetyMonitor = runtimeHost.New(MonitorInterface.Sender<ISafetyMonitorSender>().Bundler<ISafetyMonitorBundler>().Receiver(new SafetyMonitorReceiver()));
            ctx.SafetyMonitor.Configure(new ConfigureSafetyMonitor(messages));
            ctx.LivenessMonitor = runtimeHost.New(MonitorInterface.Sender<ILivenessMonitorSender>().Bundler<ILivenessMonitorBundler>().Receiver(new LivenessMonitorReceiver()));
        }

        static string StartServers(string sctx, IPublishableLogger logger, out DomainCommunicationProvider networkProvider)
        {
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled().WithVerbosityEnabled(2);
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            networkProvider = new DomainCommunicationProvider(runtimeHost, "servers");
            runtimeHost.SetNetworkProvider(networkProvider);
            runtimeHost.SetLogger(logger);

            var messages = new MessageCollection();
            messages.CollectionChanged += (sender, e) => logger.WriteLine(e.NewItems[0].ToString());
            var ctx = sctx.FromJson<DistributedStorageContext>();
            NewServers(runtimeHost, ctx, messages);
            return ctx.ToJson();
        }

        static void NewServers(RuntimeHost runtimeHost, DistributedStorageContext ctx, MessageCollection messages)
        {
            ctx.Server = runtimeHost.New(MachineInterface.Sender<IServerSender>().Bundler<IServerBundler>().Receiver(new ServerReceiver()));
            ctx.Server.Configure(new ConfigureServer(messages, ctx.SafetyMonitor, ctx.LivenessMonitor));
        }

        static string StartStorageNodes(string sctx, IPublishableLogger logger, out DomainCommunicationProvider networkProvider)
        {
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled().WithVerbosityEnabled(2);
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            networkProvider = new DomainCommunicationProvider(runtimeHost, "storage_nodes");
            runtimeHost.SetNetworkProvider(networkProvider);
            runtimeHost.SetLogger(logger);

            var messages = new MessageCollection();
            messages.CollectionChanged += (sender, e) => logger.WriteLine(e.NewItems[0].ToString());
            var ctx = sctx.FromJson<DistributedStorageContext>();
            NewStorageNodes(runtimeHost, ctx, messages);
            return ctx.ToJson();
        }

        static void NewStorageNodes(RuntimeHost runtimeHost, DistributedStorageContext ctx, MessageCollection messages)
        {
            var configure = new ConfigureStorageNode(messages, ctx.SafetyMonitor);
            var storageNodes = new List<IStorageNodeSender>();
            for (var i = 0; i < 3; i++)
            {
                var storageNode = runtimeHost.New(MachineInterface.Sender<IStorageNodeSender>().Bundler<IStorageNodeBundler>().Receiver(new StorageNodeReceiver()));
                storageNode.Configure(configure);
                storageNode.Handshake(new HandshakeStorageNode(ctx.Server));
                storageNodes.Add(storageNode);
            }
            ctx.SafetyMonitor.Handshake(new HandshakeSafetyMonitor(storageNodes.Select(_ => _.Id).ToArray()));
            ctx.StorageNodes = storageNodes.ToArray();
        }

        static string StartClients(string sctx, SynchronizedLogger logger, out DomainCommunicationProvider clientsNetworkProvider)
        {
            var configuration = Configuration.Create().WithMonitorsInProductionEnabled().WithVerbosityEnabled(2);
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);
            clientsNetworkProvider = new DomainCommunicationProvider(runtimeHost, "clients");
            runtimeHost.SetNetworkProvider(clientsNetworkProvider);
            runtimeHost.SetLogger(logger);

            var messages = new MessageCollection();
            messages.CollectionChanged += (sender, e) => logger.WriteLine(e.NewItems[0].ToString());
            var ctx = sctx.FromJson<DistributedStorageContext>();

            var synchronizable = Synchronizable.Empty();
            foreach (var storageNode in ctx.StorageNodes)
            {
                var waitIfHandledReplReq = logger.MachineActionHandledWait((machineId, _, actionName) => Equals(machineId, storageNode.Id) && actionName == "HandleReplReq");
                synchronizable = synchronizable.Then(waitIfHandledReplReq.Delay(10));
            }
            logger.ApplySynchronization(synchronizable);

            NewClients(runtimeHost, ctx, messages);
            logger.WaitForWriting(6000);
            return ctx.ToJson();
        }

        static void NewClients(RuntimeHost runtimeHost, DistributedStorageContext ctx, MessageCollection messages)
        {
            ctx.Client = runtimeHost.New(MachineInterface.Sender<IClientSender>().Bundler<IClientBundler>().Receiver(new ClientReceiverMock()));
            ctx.Client.Configure(new ConfigureClient(messages, ctx.Server));
            ctx.Server.Handshake(new HandshakeServer(ctx.Client, ctx.StorageNodes));
        }

        class ClientReceiverMock : ClientReceiver
        {
            protected override bool Random()
            {
                return true;
            }
        }

        static void StartTimers(string sctx, SynchronizedLogger logger)
        {
            var ctx = sctx.FromJson<DistributedStorageContext>();
            var lastStorageNode = ctx.StorageNodes.Last();
            foreach (var storageNode in ctx.StorageNodes)
            {
                if (storageNode == lastStorageNode)
                {
                    var setIfHandledTimeout = logger.MachineActionHandledSet((_1, _2, actionName) => actionName == "HandleTimeout");
                    var setIfEnteredExit = logger.MachineActionHandledSet((_1, _2, actionName) => actionName == "EnterExit");
                    logger.ApplySynchronization(setIfHandledTimeout.And(setIfEnteredExit));
                }
                else
                {
                    var setIfHandledTimeout = logger.MachineActionHandledSet((_1, _2, actionName) => actionName == "HandleTimeout");
                    logger.ApplySynchronization(setIfHandledTimeout);
                }

                storageNode.Timeout(new Timeout());

                logger.WaitForWriting(3000);
            }
        }
    }
}
