/* 
 * File: CompositeBugFindingEngine.cs
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
using Microsoft.PSharp.TestingServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Urasandesu.Bondage.Mixins.System;
using Urasandesu.NAnonym.Mixins.System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices
{
    public class CompositeBugFindingEngine : ITestingEngine
    {
        readonly Configuration m_configuration;
        readonly Action<RuntimeHost> m_action;
        readonly TestingEngineCoordinator m_coordinator;

        public CompositeBugFindingEngine(Configuration configuration, Action<RuntimeHost> action)
        {
            m_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_action = action ?? throw new ArgumentNullException(nameof(action));
            m_coordinator = new TestingEngineCoordinator(configuration.ParallelBugFindingTasks, configuration);
            Directory.CreateDirectory(m_coordinator.TempDirectory);
        }

        public TestReport TestReport { get => m_coordinator.TestReport; }

        public IReadOnlyList<TestReport> TestReports { get => m_coordinator.TestReports; }

        public IRegisterRuntimeOperation Reporter => throw new NotImplementedException();

        public ITestingEngine Run()
        {
            m_coordinator.Initialize();

            var bugFindingTasks = new List<Task>();
            foreach (var testingProcessId in m_coordinator.GenerateTestingProcessIds())
                bugFindingTasks.Add(NewBugFindingTask(testingProcessId));

            Task.WaitAll(bugFindingTasks.ToArray());

            return this;
        }

        Task NewBugFindingTask(int testingProcessId) =>
            Task.Run(() => AppDomain.CurrentDomain.RunAtIsolatedProcess(FindBug, testingProcessId, m_configuration, m_action, m_coordinator));

        static void FindBug(int testingProcessId, Configuration configuration, Action<RuntimeHost> action, TestingEngineCoordinator coordinator)
        {
            //global::System.Diagnostics.Debugger.Launch();
            configuration.TestingProcessId = (uint)testingProcessId;
            configuration.WithNextRandomSchedulingSeed();

            var runtimeHost = default(RuntimeHost);
            var perIterationCallBackException = default(Exception);
            var engine = TestingEngineFactory.CreateBugFindingEngine(configuration, runtime =>
            {
                runtimeHost = HostInfo.NewRuntimeHost(runtime);
                action(runtimeHost);
            });
            engine.RegisterPerIterationCallBack(iteration =>
            {
                try
                {
                    coordinator.NotifyStopEventAllIfEnabled();
                    coordinator.DoPerIterationCallBack(testingProcessId, iteration);
                }
                catch (Exception ex)
                {
                    perIterationCallBackException = ex;
                    coordinator.NotifyStopEventAllImmediately();
                }
                finally
                {
                    runtimeHost?.Dispose();
                }
            });
            coordinator.AddStopEvent(testingProcessId, new MarshalByRefTestingEngine(engine));

            engine.Run();

            if (perIterationCallBackException != null)
                ExceptionDispatchInfo.Capture(perIterationCallBackException).Throw();

            if (0 < engine.TestReport.NumOfFoundBugs)
                coordinator.NotifyStopEventAllImmediately();

            engine.TryEmitTraces(coordinator.TempDirectory, TestingEngineCoordinator.GetTaskTempNameBase(configuration));

            coordinator.SetTestReport(testingProcessId, engine.TestReport.ToJson());
        }

        public void Stop() => m_coordinator.EnableStopEventAllNotification();

        public int RegisterPerIterationCallBackIndex
        {
            get => m_coordinator.RegisterPerIterationCallBackIndex;
            set => m_coordinator.RegisterPerIterationCallBackIndex = value;
        }

        public void RegisterPerIterationCallBack(Action<int> callback) => m_coordinator.RegisterPerIterationCallBack(callback);

        public void RegisterPerIterationCallBacks(Action<int>[] callbacks) => m_coordinator.RegisterPerIterationCallBacks(callbacks);

        public void TryEmitTraces(string directory, string file)
        {
            var tempDirInfo = new DirectoryInfo(m_coordinator.TempDirectory);
            var finalFileInfos = new List<FileInfo>();
            foreach (var tmpFileInfo in tempDirInfo.EnumerateFiles())
                finalFileInfos.Add(tmpFileInfo.CopyTo(TestingEngineCoordinator.GetFinalPath(tmpFileInfo.Name, directory, file), true));

            Directory.Delete(m_coordinator.TempDirectory, true);
            m_coordinator.SetEmittedTracePaths(file, finalFileInfos.Select(_ => _.FullName).ToArray());
        }

        public string Report() => m_coordinator.Report();

        public string ReportFully()
        {
            if (m_coordinator.EmittedTraceInfos == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var readableTracePath in m_coordinator.EmittedTraceInfos.Select(_ => _.EmittedReadableTracePath))
                AppendReadableTraceContents(sb, readableTracePath);

            return sb.ToString();
        }

        static void AppendReadableTraceContents(StringBuilder sb, string readableTracePath)
        {
            sb.AppendFormat("<StrategyLog> {0}{1}", Path.GetFileName(readableTracePath), Environment.NewLine);
            sb.AppendLine(new FileInfo(readableTracePath).OpenText().EnsureDisposalThen(sr => sr.ReadToEnd()));
        }

        public EmittedTraceInfo[] EmittedTraceInfos { get => m_coordinator.EmittedTraceInfos; }
    }
}
