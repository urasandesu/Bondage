/* 
 * File: CompositeBugFindingEngineTest.cs
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
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Urasandesu.Bondage;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices;

namespace Test.Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices
{
    [TestFixture]
    public class CompositeBugFindingEngineTest
    {
        [Test]
        public void Run_should_report_all_results_of_the_tests_that_are_run_concurrently()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(200).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.BugReceiver>());
                m1.Configure(new Notify());
            });


            // Act
            engine.RunAndEmitTraces();


            // Assert
            Assert.GreaterOrEqual(engine.TestReport.NumOfFoundBugs, 1);
            Assert.That(engine.Report(), Does.StartWith("..."));
            Assert.AreEqual(5, engine.TestReports.Count(_ => _ != null));
        }

        [Test]
        public void Run_should_report_all_results_excluding_the_test_that_exception_occurs_in_the_callback()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(10).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.InfiniteLoopReceiver>());
                m1.Configure(new Notify());
            });
            engine.RegisterPerIterationCallBacks(new Action<int>[]
            {
                iteration => { },
                iteration => { },
                iteration => { throw new NotSupportedException(); },
                iteration => { },
                iteration => { },
            });


            // Act, Assert
            var ex = Assert.Throws<AggregateException>(() => engine.Run());
            Assert.IsInstanceOf<TargetInvocationException>(ex.GetBaseException());
            Assert.IsInstanceOf<NotSupportedException>(ex.GetBaseException().InnerException);
            Assert.IsNotNull(engine.TestReports[0]);
            Assert.IsNotNull(engine.TestReports[1]);
            Assert.IsNull(engine.TestReports[2]);
            Assert.IsNotNull(engine.TestReports[3]);
            Assert.IsNotNull(engine.TestReports[4]);
        }



        [Test]
        public void Stop_should_abort_test_in_the_middle_of_the_iteration()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(200).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.InfiniteLoopReceiver>());
                m1.Configure(new Notify());
            });
            var runner = Task.Run(() => engine.Run());


            // Act
            engine.Stop();


            // Assert
            Assert.IsTrue(runner.Wait(5000));
        }



        [Test]
        public void RegisterPerIterationCallBack_should_throw_IndexOutOfRangeException_if_registering_the_number_of_callbacks_greater_than_the_number_of_parallel_tasks()
        {
            // Arrange
            var configuration = Configuration.Create().WithParallelBugFindingTasks(5);
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.InfiniteLoopReceiver>());
                m1.Configure(new Notify());
            });
            engine.RegisterPerIterationCallBack(iteration => { });
            engine.RegisterPerIterationCallBack(iteration => { });
            engine.RegisterPerIterationCallBack(iteration => { });
            engine.RegisterPerIterationCallBack(iteration => { });
            engine.RegisterPerIterationCallBack(iteration => { });


            // Act, Assert
            var ex = Assert.Throws<IndexOutOfRangeException>(() => engine.RegisterPerIterationCallBack(iteration => { }));
            Assert.That(ex.Message, Does.Match(@"'RegisterPerIterationCallBackIndex'\(\d+\) was outside the bounds of the 'ParallelBugFindingTasks'\(\d+\)\."));
        }



        [Test]
        public void RegisterPerIterationCallBacks_should_throw_ArgumentOutOfRangeException_if_specifying_the_number_of_callbacks_different_from_the_number_of_parallel_tasks()
        {
            // Arrange
            var configuration = Configuration.Create().WithParallelBugFindingTasks(5);
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.InfiniteLoopReceiver>());
                m1.Configure(new Notify());
            });


            // Act, Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => engine.RegisterPerIterationCallBacks(new Action<int>[]
            {
                iteration => { },
                iteration => { },
                iteration => { },
                iteration => { },
            })); ;
            Assert.That(ex.Message, Does.Match(@"Specified argument length was out of the range of valid length \d+\."));
        }



        [Test]
        public void TryEmitTraces_should_output_the_artifact_of_the_test_to_the_specified_directory()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(200).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var testArtifact = configuration.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 10));
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.BugReceiver>());
                m1.Configure(new Notify());
            });
            engine.Run();


            // Act
            engine.TryEmitTraces(testArtifact.Directory, testArtifact.TraceNameBase);


            // Assert
            Assert.GreaterOrEqual(engine.TestReport.NumOfFoundBugs, 1);
            var traceFileInfos = new DirectoryInfo(testArtifact.Directory).EnumerateFiles();
            Assert.IsTrue(traceFileInfos.Any(_ => Regex.IsMatch(_.Name, $@"{ testArtifact.TraceNameBase }_\d+_\d+\.pstrace")));
            Assert.IsTrue(traceFileInfos.Any(_ => Regex.IsMatch(_.Name, $@"{ testArtifact.TraceNameBase }_\d+_\d+\.schedule")));
            Assert.IsTrue(traceFileInfos.Any(_ => Regex.IsMatch(_.Name, $@"{ testArtifact.TraceNameBase }_\d+_\d+\.txt")));
        }

        [Test]
        public void TryEmitTraces_should_output_replayable_results()
        {
            // Arrange
            var bugFindingConfig = Configuration.Create().WithNumberOfIterations(200).
                                                          WithMaxSchedulingSteps(200).
                                                          WithParallelBugFindingTasks(5).
                                                          WithRandomSchedulingSeed();
            var testArtifact = bugFindingConfig.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 10));
            var action = default(Action<RuntimeHost>);
            action = runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.BugReceiver>());
                m1.Configure(new Notify());
            };
            var bugFindingEngine = new CompositeBugFindingEngine(bugFindingConfig, action);
            bugFindingEngine.Run();


            // Act
            bugFindingEngine.TryEmitTraces(testArtifact.Directory, testArtifact.TraceNameBase);


            // Assert
            var reproTracePath = bugFindingEngine.EmittedTraceInfos.First().EmittedReproducableTracePath;
            var replayConfig = Configuration.Create().WithVerbosityEnabled(2);//.
                                                      //WithAttachingDebuggerEnabled();
            replayConfig.ScheduleFile = reproTracePath;
            var replayEngine = new CompositeReplayEngine(replayConfig, action);
            replayEngine.Run();

            Assert.GreaterOrEqual(replayEngine.TestReport.NumOfFoundBugs, 1);
        }



        [Test]
        public void ReportFully_should_return_readable_trace_contents()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(200).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var testArtifact = configuration.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 10));
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.BugReceiver>());
                m1.Configure(new Notify());
            });
            engine.RunAndEmitTraces(testArtifact);


            // Act
            var result = engine.ReportFully();


            // Assert
            Assert.That(result, Does.Match($@"<StrategyLog> { testArtifact.TraceNameBase }_\d+_\d+\.txt"));
            Assert.That(result, Does.Match("<ErrorLog> Bug Found!!!!"));
        }

        [Test]
        public void ReportFully_should_return_empty_if_not_emitting_trace()
        {
            // Arrange
            var configuration = Configuration.Create().WithNumberOfIterations(200).
                                                       WithMaxSchedulingSteps(200).
                                                       WithParallelBugFindingTasks(5).
                                                       WithRandomSchedulingSeed();
            var testArtifact = configuration.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 10));
            var engine = new CompositeBugFindingEngine(configuration, runtimeHost =>
            {
                var m1 = runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.BugReceiver>());
                m1.Configure(new Notify());
            });
            engine.Run();


            // Act
            var result = engine.ReportFully();


            // Assert
            Assert.IsEmpty(result);
        }



        public class M1
        {
            public interface ISender : IMethodizedMachineSender
            {
                void Configure(Notify e);
                void Tick(Notify e);
                void StartLoop(Notify e);
            }

            public interface IReceiver : IMethodizedMachineReceiver
            {
                void HandleConfigure(Notify e);
                void EnterLoop();
                void EnterElapsed();
            }

            public abstract class Receiver : MethodizedMachineReceiver<IBundler>, IReceiver
            {
                public void HandleConfigure(Notify e)
                {
                    Self.Loop();
                }

                public void EnterLoop()
                {
                    Self.Tick(new Notify());
                }

                public abstract void EnterElapsed();
            }

            public class BugReceiver : Receiver
            {
                public override void EnterElapsed()
                {
                    if (Random())
                        Assert(false, "Bug Found!!!!");

                    Self.StartLoop(new Notify());
                }
            }

            public class InfiniteLoopReceiver : Receiver
            {
                public override void EnterElapsed()
                {
                    Self.StartLoop(new Notify());
                }
            }

            public interface IBundler : ISender, IReceiver, IMethodizedMachineStatus
            {
                [Initializer]
                [OnEventInvoke(nameof(Configure), nameof(HandleConfigure))]
                void Initialized();

                [OnEnterInvoke(nameof(EnterLoop))]
                [OnEventTransit(nameof(Tick), nameof(Elapsed))]
                void Loop();

                [OnEnterInvoke(nameof(EnterElapsed))]
                [OnEventTransit(nameof(StartLoop), nameof(Loop))]
                void Elapsed();
            }
        }
    }
}
