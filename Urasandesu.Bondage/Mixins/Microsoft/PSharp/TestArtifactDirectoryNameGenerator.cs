/* 
 * File: TestArtifactDirectoryNameGenerator.cs
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



using System.IO;
using System.Text.RegularExpressions;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    public class TestArtifactDirectoryNameGenerator
    {
        readonly TestArtifactProperty m_testArtifactProp;
        int m_maxBranchNumber = -1;

        public TestArtifactDirectoryNameGenerator(TestArtifactProperty testArtifactProp)
        {
            m_testArtifactProp = testArtifactProp;
        }

        string m_directory;
        public string Directory
        {
            get
            {
                if (m_directory == null)
                    m_directory = MakeDirectory(m_maxBranchNumber, m_testArtifactProp);
                return m_directory;
            }
        }

        static string MakeDirectory(int maxBranchNumber, TestArtifactProperty testArtifactProp)
        {
            var testNameSuffix = "_" + (maxBranchNumber + 1);
            return Path.Combine(testArtifactProp.TestDllDirectory, testArtifactProp.TestName + testNameSuffix);
        }

        const string TestNameRegexBranchNumber = "BranchNumber";
        readonly static Regex TestNameRegex = new Regex(@"Test[0-9A-F]{8}_(?<" + TestNameRegexBranchNumber + @">\d+)", RegexOptions.Compiled);

        public bool TryUpdate(string name)
        {
            if (m_directory != null)
                return false;

            if (!name.StartsWith(m_testArtifactProp.TestName))
                return false;

            var m = TestNameRegex.Match(name);
            if (!m.Success)
                return false;

            var branchNumber = int.Parse(m.Groups[TestNameRegexBranchNumber].Value);
            if (m_maxBranchNumber < branchNumber)
                m_maxBranchNumber = branchNumber;
            return true;
        }
    }
}
