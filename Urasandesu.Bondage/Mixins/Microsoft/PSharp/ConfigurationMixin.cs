/* 
 * File: ConfigurationMixin.cs
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
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    public static class ConfigurationMixin
    {
        public static Configuration WithMonitorsInProductionEnabled(this Configuration @this, bool enabled = true)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            @this.EnableMonitorsInProduction = enabled;
            return @this;
        }

        public static Configuration WithMaxSchedulingSteps(this Configuration @this, int unfairSteps, double fairStepsRatio = 10, bool tryUpdateLivenessTemperatureThreshold = true)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            @this.MaxUnfairSchedulingSteps = unfairSteps;
            @this.MaxFairSchedulingSteps = (int)(unfairSteps * fairStepsRatio);
            if (tryUpdateLivenessTemperatureThreshold)
                if (@this.LivenessTemperatureThreshold == 0)
                    if (@this.EnableCycleDetection)
                        @this.LivenessTemperatureThreshold = 100;
                    else if (@this.MaxFairSchedulingSteps > 0)
                        @this.LivenessTemperatureThreshold = @this.MaxFairSchedulingSteps / 2;
            return @this;
        }

        public static Configuration WithRandomSchedulingSeed(this Configuration @this, int? seed = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (@this.RandomSchedulingSeed == null && seed == null)
                @this.RandomSchedulingSeed = DateTime.Now.Millisecond;
            else if (seed != null)
                @this.RandomSchedulingSeed = seed;
            return @this;
        }

        public static Configuration WithParallelBugFindingTasks(this Configuration @this, uint num = 1)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            @this.ParallelBugFindingTasks = num;
            return @this;
        }

        public static Configuration WithNextRandomSchedulingSeed(this Configuration @this, uint? num = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (@this.RandomSchedulingSeed != null)
                @this.RandomSchedulingSeed = (int)(@this.RandomSchedulingSeed + (673 * num ?? @this.TestingProcessId));
            return @this;
        }

        public static Configuration WithAttachingDebuggerEnabled(this Configuration @this, bool enabled = true)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            @this.AttachDebugger = enabled;
            return @this;
        }

        public static Predicate<MethodBase> IsTestMethod { get; set; } = new Predicate<MethodBase>(meth =>
        {
            return meth.GetCustomAttributes(true).Any(_ => _.GetType().FullName == "NUnit.Framework.TestAttribute");    // TODO: Enhance supported test framework.
        });

        static MethodBase IdentifyTestMethod(StackTrace trace)
        {
            var testMethod = default(MethodBase);
            foreach (var frame in trace.GetFrames())
            {
                var meth = frame.GetMethod();
                if (!IsTestMethod(meth))
                    continue;

                testMethod = meth;
                break;
            }
            if (testMethod == null)
                throw new InvalidOperationException($"This method can only call in the method that matches to '{ nameof(IsTestMethod) }' result.");

            return testMethod;
        }

        public static string GetCurrentTestArtifactLocation(this Configuration @this)
        {
            return @this.GetCurrentTestArtifactLocation(DateTime.Now);
        }

        public static string GetCurrentTestArtifactLocation(this Configuration @this, DateTime dateTime)
        {
            return @this.GetTestArtifactLocation(IdentifyTestMethod(new StackTrace()), dateTime);
        }

        public static string GetTestArtifactLocation(this Configuration @this, MethodBase testMethod, DateTime dateTime)
        {
            return @this.GetTestArtifact(testMethod, dateTime).Directory;
        }

        public static TestArtifact CreateCurrentTestArtifact(this Configuration @this)
        {
            return @this.CreateCurrentTestArtifact(DateTime.Now);
        }

        public static TestArtifact CreateCurrentTestArtifact(this Configuration @this, DateTime dateTime)
        {
            return @this.CreateTestArtifact(IdentifyTestMethod(new StackTrace()), dateTime);
        }

        public static TestArtifact CreateTestArtifact(this Configuration @this, MethodBase testMethod, DateTime dateTime)
        {
            var testArtifact = @this.GetTestArtifact(testMethod, dateTime);
            if (Directory.Exists(testArtifact.Directory))
                @this.DeleteTestArtifact(testArtifact);
            Directory.CreateDirectory(testArtifact.Directory);
            using (var sw = new StreamWriter(Path.Combine(testArtifact.Directory, testArtifact.FullName)))
            using (var xw = new XmlTextWriter(sw))
            {
                xw.Formatting = Formatting.Indented;
                var ndcs = new NetDataContractSerializer();
                ndcs.WriteObject(xw, testArtifact);
            }
            return testArtifact;
        }

        public static TestArtifact GetCurrentTestArtifact(this Configuration @this)
        {
            return @this.GetCurrentTestArtifact(DateTime.Now);
        }

        public static TestArtifact GetCurrentTestArtifact(this Configuration @this, DateTime dateTime)
        {
            return @this.GetTestArtifact(IdentifyTestMethod(new StackTrace()), dateTime);
        }

        public static TestArtifact GetTestArtifact(this Configuration @this, MethodBase testMethod, DateTime dateTime)
        {
            var testArtifactProp = new TestArtifactProperty(AppDomain.CurrentDomain.BaseDirectory, testMethod, dateTime);
            var testDllDirInfo = new DirectoryInfo(testArtifactProp.TestDllDirectory);
            var testArtifactDirNameGen = testArtifactProp.GetDirectoryNameGenerator();
            if (testDllDirInfo.Exists)
            {
                foreach (var dirInfo in testDllDirInfo.EnumerateDirectories())
                {
                    if (!testArtifactDirNameGen.TryUpdate(dirInfo.Name))
                        continue;

                    var tmpTestArtifact = new TestArtifact(dirInfo.FullName, testArtifactProp);
                    using (var sr = new StreamReader(tmpTestArtifact.FullName))
                    using (var xr = new XmlTextReader(sr))
                    {
                        var ndcs = new NetDataContractSerializer();
                        var testArtifact = (TestArtifact)ndcs.ReadObject(xr);
                        if (testArtifact.Test == testMethod)
                        {
                            tmpTestArtifact.CopyNonDataMemberTo(testArtifact);
                            return testArtifact;
                        }
                    }
                }
            }
            return new TestArtifact(testArtifactDirNameGen.Directory, testArtifactProp);
        }

        public static void DeleteTestArtifact(this Configuration @this, TestArtifact testArtifact)
        {
            Directory.Delete(testArtifact.Directory, true);
        }
    }
}
