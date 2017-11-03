/* 
 * File: TestArtifact.cs
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
using System.Reflection;
using System.Runtime.Serialization;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    [DataContract]
    public class TestArtifact
    {
        public TestArtifact(string testArtifactDirectory, TestArtifactProperty testArtifactProp)
        {
            Directory = testArtifactDirectory;
            Property = testArtifactProp;

            if (!Directory.EndsWith(@"\"))
                Directory += @"\";
        }

        [DataMember]
        public MethodBase Test { get; private set; }

        // Directory location is always made dynamically because it should be a user can move it anywhere.
        public string Directory { get; private set; }

        string m_fullName;
        public string FullName
        {
            get
            {
                if (Directory == null && m_fullName == null)
                    return null;

                if (m_fullName == null)
                    m_fullName = Path.Combine(Directory, typeof(TestArtifact).Name + ".xml");
                return m_fullName;
            }
        }

        TestArtifactProperty m_prop;
        public TestArtifactProperty Property
        {
            get
            {
                if ((Directory == null || Test == null) && m_prop == null)
                    return null;

                if (m_prop == null)
                    m_prop = TestArtifactProperty.Parse(Directory, Test);
                return m_prop;
            }
            private set
            {
                m_prop = value;
                if (m_prop != null)
                    Test = m_prop.Test;
            }
        }

        [DataMember]
        public string TraceNameBase { get; set; } = "Trace";

        public void CopyNonDataMemberTo(TestArtifact dst)
        {
            dst.Directory = Directory;
            dst.Property = Property;
        }
    }
}
