/* 
 * File: TestingHandle.cs
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
    class TestingHandle
    {
        public Action<int> PerIterationCallBack { get; set; }

        public void DoPerIterationCallBack(int iteration)
        {
            PerIterationCallBack?.Invoke(iteration);
        }

        public event EventHandler Stop;

        public void NotifyStopEvent()
        {
            Stop?.Invoke(this, EventArgs.Empty);
        }

        public TestReport TestReport { get; set; }

        public static TestingHandle[] NewHandles(uint numOfHandles)
        {
            var handles = new TestingHandle[numOfHandles];
            for (var testingProcessId = 0; testingProcessId < handles.Length; testingProcessId++)
                handles[testingProcessId] = new TestingHandle();
            return handles;
        }
    }
}
