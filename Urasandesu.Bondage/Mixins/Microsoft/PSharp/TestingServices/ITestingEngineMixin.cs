/* 
 * File: ITestingEngineMixin.cs
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
using System.Reflection;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp.TestingServices
{
    public static class ITestingEngineMixin
    {
        public static ITestingEngine RunAndEmitTraces(this ITestingEngine @this)
        {
            var configuration = @this.TestReport.Configuration;
            var testArtifact = configuration.CreateCurrentTestArtifact();
            return @this.RunAndEmitTraces(testArtifact);
        }

        public static ITestingEngine RunAndEmitTraces(this ITestingEngine @this, DateTime dateTime)
        {
            var configuration = @this.TestReport.Configuration;
            var testArtifact = configuration.CreateCurrentTestArtifact(dateTime);
            return @this.RunAndEmitTraces(testArtifact);
        }

        public static ITestingEngine RunAndEmitTraces(this ITestingEngine @this, MethodBase testMethod, DateTime dateTime)
        {
            var configuration = @this.TestReport.Configuration;
            var testArtifact = configuration.CreateTestArtifact(testMethod, dateTime);
            return @this.RunAndEmitTraces(testArtifact);
        }

        public static ITestingEngine RunAndEmitTraces(this ITestingEngine @this, TestArtifact testArtifact)
        {
            var engine = @this.Run();
            @this.TryEmitTraces(testArtifact.Directory, testArtifact.TraceNameBase);
            return @this;
        }
    }
}
