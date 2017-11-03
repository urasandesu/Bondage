/* 
 * File: MarshalByRefTestingEngine.cs
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



using Microsoft.PSharp.TestingServices;
using System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices
{
    class MarshalByRefTestingEngine : MarshalByRefObject, ITestingEngine
    {
        readonly ITestingEngine m_engine;

        public MarshalByRefTestingEngine(ITestingEngine engine)
        {
            m_engine = engine;
        }

        public TestReport TestReport => m_engine.TestReport;

        public IRegisterRuntimeOperation Reporter => m_engine.Reporter;

        public ITestingEngine Run()
        {
            m_engine.Run();
            return this;
        }

        public void Stop()
        {
            m_engine.Stop();
        }

        public void TryEmitTraces(string directory, string file)
        {
            m_engine.TryEmitTraces(directory, file);
        }

        public void RegisterPerIterationCallBack(Action<int> callback)
        {
            m_engine.RegisterPerIterationCallBack(callback);
        }

        public string Report()
        {
            return m_engine.Report();
        }
    }
}
