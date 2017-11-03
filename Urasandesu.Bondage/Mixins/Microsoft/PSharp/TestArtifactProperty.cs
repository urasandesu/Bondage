/* 
 * File: TestArtifactProperty.cs
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



using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    public class TestArtifactProperty
    {
        public TestArtifactProperty(string baseDirectory, MethodBase testMethod, DateTime dateTime)
        {
            BaseDirectory = baseDirectory;
            Test = testMethod;
            DateTime = dateTime;
        }

        public string BaseDirectory { get; }
        public MethodBase Test { get; }
        public DateTime DateTime { get; }

        string m_testDllDirectory;
        public string TestDllDirectory
        {
            get
            {
                if (m_testDllDirectory == null)
                    m_testDllDirectory = MakeTestDllDirectory(BaseDirectory, DateTime, Test);
                return m_testDllDirectory;
            }
        }

        static string MakeTestDllDirectory(string baseDirectory, DateTime dateTime, MethodBase testMethod)
        {
            var outputDir = Path.Combine(baseDirectory, "Output");
            var historyDir = Path.Combine(outputDir, dateTime.ToString("yyyyMMddHHmmss"));
            var testDllName = testMethod.DeclaringType.Module.Name;
            return Path.Combine(historyDir, testDllName);
        }

        string m_testName;
        public string TestName
        {
            get
            {
                if (m_testName == null)
                    m_testName = "Test" + (Test.DeclaringType.FullName + "." + Test.ToString()).GetHashCode().ToString("X8");
                return m_testName;
            }
        }

        public TestArtifactDirectoryNameGenerator GetDirectoryNameGenerator()
        {
            return new TestArtifactDirectoryNameGenerator(this);
        }

        public static TestArtifactProperty Parse(string testArtifactDirectory, MethodBase testMethod)
        {
            var testDllDir = Path.GetDirectoryName(testArtifactDirectory);
            var historyDir = Path.GetDirectoryName(testDllDir);
            var historyDirName = Path.GetFileName(historyDir);
            if (!DateTime.TryParseExact(historyDirName, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                throw new FormatException(Resources.GetString("TestArtifactProperty_Parse_InvalidFormat"));

            var outputDir = Path.GetDirectoryName(historyDir);
            var baseDir = Path.GetDirectoryName(outputDir);
            return new TestArtifactProperty(baseDir, testMethod, dateTime);
        }
    }
}
