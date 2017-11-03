/* 
 * File: MonitorId.cs
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
using System.Linq;
using System.Runtime.Serialization;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{

    [DataContract]
    public sealed class MonitorId
    {
        [DataMember]
        public string Endpoint { get; private set; }

        [DataMember]
        public string AssemblyQualifiedName { get; private set; }

        Type m_monitorType;
        public Type MonitorType
        {
            get
            {
                if (m_monitorType == null)
                    m_monitorType = Type.GetType(AssemblyQualifiedName, asmName => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(_ => _.FullName == asmName.FullName), null, true);
                return m_monitorType;
            }
        }

        public MonitorId(PSharpRuntime runtime, Type type)
        {
            Endpoint = runtime.NetworkProvider.GetLocalEndpoint();
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            m_monitorType = type;
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            hashCode ^= Endpoint == null ? 0 : Endpoint.GetHashCode();
            hashCode ^= AssemblyQualifiedName.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var other = default(MonitorId);
            if ((other = obj as MonitorId) == null)
                return false;

            return Endpoint == other.Endpoint && 
                   AssemblyQualifiedName == other.AssemblyQualifiedName;
        }

        public override string ToString()
        {
            return string.Join("/", new[] { Endpoint, MonitorType.FullName });
        }
    }
}
