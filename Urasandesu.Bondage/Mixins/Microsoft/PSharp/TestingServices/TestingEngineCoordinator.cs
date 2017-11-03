/* 
 * File: TestingEngineCoordinator.cs
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
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Threading;
using Urasandesu.Bondage.Mixins.System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices
{
    class TestingEngineCoordinator : MarshalByRefObject
    {
        readonly Configuration m_configIgnoringParallelBugFindingTasks;
        readonly TestingHandle[] m_handles;

        public TestingEngineCoordinator(uint parallelBugFindingTasks, Configuration configIgnoringParallelBugFindingTasks)
        {
            m_configIgnoringParallelBugFindingTasks = configIgnoringParallelBugFindingTasks;
            m_handles = TestingHandle.NewHandles(parallelBugFindingTasks);
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) + @"\";
            TempDirectory = tempDir;
        }

        TestReport m_testReport;
        public TestReport TestReport
        {
            get
            {
                if (m_testReport == null)
                    m_testReport = NewTestReport(m_configIgnoringParallelBugFindingTasks, m_handles);
                return m_testReport;
            }
        }
        static TestReport NewTestReport(Configuration configuration, TestingHandle[] handles)
        {
            var testReport = new TestReport(configuration);
            foreach (var handle in handles)
                if (handle.TestReport != null)
                    testReport.Merge(handle.TestReport);
            return testReport;
        }

        IReadOnlyList<TestReport> m_testReports;
        public IReadOnlyList<TestReport> TestReports
        {
            get
            {
                if (m_testReports == null)
                    m_testReports = m_handles.Select(_ => _.TestReport).ToArray();
                return m_testReports;
            }
        }

        public IEnumerable<int> GenerateTestingProcessIds()
        {
            for (var testingProcessId = 0; testingProcessId < m_handles.Length; testingProcessId++)
                yield return testingProcessId;
        }

        public void Initialize()
        {
            m_testReport = null;
            m_testReports = null;
            EmittedTraceInfos = null;
        }

        public static readonly string TempNameBase = "Temp";

        public string TempDirectory { get; }

        public void DoPerIterationCallBack(int testingProcessId, int iteration)
        {
            m_handles[testingProcessId].DoPerIterationCallBack(iteration);
        }

        public void AddStopEvent(int testingProcessId, MarshalByRefTestingEngine engine)
        {
            m_handles[testingProcessId].Stop += (sender, e) =>
            {
                try
                {
                    engine.Stop();
                }
                catch (RemotingException)
                { }
            };
        }

        public void NotifyStopEventAllIfEnabled()
        {
            if (m_stopEventAllNotificationEnabled == True)
                NotifyStopEventAllImmediately();
        }

        public void NotifyStopEventAllImmediately()
        {
            foreach (var handle in m_handles)
                handle.NotifyStopEvent();
        }

        public void SetTestReport(int testingProcessId, string jsonTestReport)
        {
            m_handles[testingProcessId].TestReport = jsonTestReport.FromJson<TestReport>();
        }

        public int RegisterPerIterationCallBackIndex { get; set; }

        public void RegisterPerIterationCallBack(Action<int> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (RegisterPerIterationCallBackIndex < 0 || m_handles.Length <= RegisterPerIterationCallBackIndex)
                throw new IndexOutOfRangeException(Resources.GetString("TestingEngineCoordinator_RegisterPerIterationCallBack_OutOfRange",
                                                                       RegisterPerIterationCallBackIndex,
                                                                       m_handles.Length));

            m_handles[RegisterPerIterationCallBackIndex++].PerIterationCallBack = callback;
        }

        public void RegisterPerIterationCallBacks(Action<int>[] callbacks)
        {
            if (callbacks == null)
                throw new ArgumentNullException(nameof(callbacks));

            if (m_handles.Length != callbacks.Length)
                throw new ArgumentOutOfRangeException(nameof(callbacks),
                                                      callbacks.Length,
                                                      Resources.GetString("TestingEngineCoordinator_RegisterPerIterationCallBacks_OutOfRange",
                                                                          m_handles.Length));

            RegisterPerIterationCallBackIndex = 0;
            foreach (var callback in callbacks)
                RegisterPerIterationCallBack(callback);
        }

        public static string GetTaskTempNameBase(Configuration configuration)
        {
            return TempNameBase + "_" + configuration.TestingProcessId;
        }

        public static string GetFinalPath(string tempName, string directory, string file)
        {
            var finalName = file + tempName.Substring(TempNameBase.Length);
            return Path.Combine(directory, finalName);
        }

        const int False = 0;
        const int True = 1;
        int m_stopEventAllNotificationEnabled;
        public void EnableStopEventAllNotification()
        {
            Interlocked.CompareExchange(ref m_stopEventAllNotificationEnabled, True, False);
        }

        public void DisableStopEventAllNotification()
        {
            Interlocked.CompareExchange(ref m_stopEventAllNotificationEnabled, False, True);
        }

        public string Report()
        {
            return TestReport.GetText(m_configIgnoringParallelBugFindingTasks, "...");
        }

        const string TraceRegexTestingProcessId = "TestingProcessId";
        const string TraceRegexTraceIndex = "TraceIndex";
        public void SetEmittedTracePaths(string file, string[] tracePaths)
        {
            var bugTraceRegex = new Regex($@"{ file }_(?<" + TraceRegexTestingProcessId + @">\d+)_(?<" + TraceRegexTraceIndex + @">\d+)\.pstrace$", RegexOptions.IgnoreCase);
            var reproTraceRegex = new Regex($@"{ file }_(?<" + TraceRegexTestingProcessId + @">\d+)_(?<" + TraceRegexTraceIndex + @">\d+)\.schedule$", RegexOptions.IgnoreCase);
            var readableTraceRegex = new Regex($@"{ file }_(?<" + TraceRegexTestingProcessId + @">\d+)_(?<" + TraceRegexTraceIndex + @">\d+)\.txt$", RegexOptions.IgnoreCase);
            var tmpEmittedTraceInfos = new Dictionary<Tuple<int, int>, EmittedTraceInfo>();
            foreach (var tracePath in tracePaths)
            {
                if (TryUpdateTraceInfo(tmpEmittedTraceInfos, tracePath, bugTraceRegex, _ => _.EmittedBugTracePath = tracePath))
                    continue;
                else if (TryUpdateTraceInfo(tmpEmittedTraceInfos, tracePath, reproTraceRegex, _ => _.EmittedReproducableTracePath = tracePath))
                    continue;
                else if (TryUpdateTraceInfo(tmpEmittedTraceInfos, tracePath, readableTraceRegex, _ => _.EmittedReadableTracePath = tracePath))
                    continue;
            }
            EmittedTraceInfos = tmpEmittedTraceInfos.OrderBy(_ => _.Key.Item1).ThenBy(_ => _.Key.Item2).Select(_ => _.Value).ToArray();
        }

        static bool TryUpdateTraceInfo(Dictionary<Tuple<int, int>, EmittedTraceInfo> tmpEmittedTraceInfos, string tracePath, Regex traceRegex, Action<EmittedTraceInfo> setTracePath)
        {
            var fileName = Path.GetFileName(tracePath);
            var m = traceRegex.Match(fileName);
            if (!m.Success)
                return false;

            var testingProcessId = int.Parse(m.Groups[TraceRegexTestingProcessId].Value);
            var traceIndex = int.Parse(m.Groups[TraceRegexTraceIndex].Value);
            var key = Tuple.Create(testingProcessId, traceIndex);
            if (!tmpEmittedTraceInfos.ContainsKey(key))
                tmpEmittedTraceInfos.Add(key, new EmittedTraceInfo());
            setTracePath(tmpEmittedTraceInfos[key]);

            return true;
        }

        public EmittedTraceInfo[] EmittedTraceInfos { get; private set; }
    }
}
