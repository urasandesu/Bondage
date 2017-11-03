/* 
 * File: RuntimeHostId.cs
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



using Microsoft.PSharp.Net;
using System;
using System.Runtime.Serialization;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.Net;
using Urasandesu.NAnonym.Mixins.System;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    [DataContract]
    public sealed class RuntimeHostId
    {
        [DataMember]
        public Guid Value { get; private set; }

        [DataMember]
        public AdditionalConfiguration AdditionalConfiguration { get; internal set; }

        [DataMember]
        public string Endpoint { get; private set; }

        [DataMember]
        public string AssemblyQualifiedName { get; private set; }

        internal RuntimeHostId()
        {
            Value = Guid.NewGuid();
        }

        internal void SetNetworkInformation(INetworkProvider networkProvider)
        {
            Endpoint = networkProvider.GetLocalEndpoint();
            if (networkProvider is ICommunicationProvider networkProvider2)
                AssemblyQualifiedName = networkProvider2.RemoteNetworkProviderType.AssemblyQualifiedName;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as RuntimeHostId;
            if (other == null)
                return false;

            return Value == other.Value;
        }

        public override string ToString()
        {
            return $"{{\"AdditionalConfiguration\":{ AdditionalConfiguration },\"AssemblyQualifiedName\":{ AssemblyQualifiedName.ToNullVisibleString() },\"Endpoint\":{ Endpoint.ToNullVisibleString() },\"Value\":\"{ Value }\"}}";
        }
    }
}
