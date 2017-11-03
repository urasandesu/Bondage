/* 
 * File: CompositeReplayEngine.cs
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
using System.Threading.Tasks;
using Urasandesu.Bondage.Mixins.System;
using Urasandesu.NAnonym.Mixins.System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices
{
    public class CompositeReplayEngine : ITestingEngine
    {
        readonly Configuration m_configuration;
        readonly Action<RuntimeHost> m_action;
        readonly TestingEngineCoordinator m_coordinator;

        public CompositeReplayEngine(Configuration configuration, Action<RuntimeHost> action)
        {
            m_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_action = action ?? throw new ArgumentNullException(nameof(action));
            m_coordinator = new TestingEngineCoordinator(1, configuration);
        }

        public TestReport TestReport { get => m_coordinator.TestReport; }

        public IRegisterRuntimeOperation Reporter => throw new NotImplementedException();

        public ITestingEngine Run()
        {
            m_coordinator.Initialize();
            NewBugFindingTask().Wait();
            return this;
        }

        Task NewBugFindingTask() =>
            Task.Run(() => AppDomain.CurrentDomain.RunAtIsolatedProcess(FindBug, m_configuration, m_action, m_coordinator));

        static void FindBug(Configuration configuration, Action<RuntimeHost> action, TestingEngineCoordinator coordinator)
        {
            //global::System.Diagnostics.Debugger.Launch();

            var runtimeHost = default(RuntimeHost);
            var engine = TestingEngineFactory.CreateReplayEngine(configuration, runtime =>
            {
                runtimeHost = HostInfo.NewRuntimeHost(runtime);
                action(runtimeHost);
            });
            coordinator.AddStopEvent(0, new MarshalByRefTestingEngine(engine));

            engine.Run();

            if (0 < engine.TestReport.NumOfFoundBugs)
                coordinator.NotifyStopEventAllImmediately();

            coordinator.SetTestReport(0, engine.TestReport.ToJson());
        }


        public void Stop() => m_coordinator.EnableStopEventAllNotification();

        public void RegisterPerIterationCallBack(Action<int> callback)
        { }

        public void TryEmitTraces(string directory, string file)
        { }

        public string Report() => m_coordinator.Report();
    }
}
