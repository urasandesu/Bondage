﻿/* 
 * File: CommunicationId.cs
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
using System.Runtime.Serialization;

namespace Urasandesu.Bondage.Mixins.Microsoft.PSharp
{
    [DataContract]
    public abstract class CommunicationId
    {
        [DataMember]
        public Guid Value { get; private set; }

        [DataMember]
        public RuntimeHostId RuntimeHostId { get; private set; }

        public string Endpoint
        {
            get => RuntimeHostId == null ? string.Empty : RuntimeHostId.Endpoint;
        }



        protected CommunicationId()
        {
            Value = Guid.NewGuid();
        }

        internal void LinkTo(RuntimeHostId runtimeHostId)
        {
            if (RuntimeHostId != null)
                return;

            RuntimeHostId = runtimeHostId;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CommunicationId;
            if (other == null)
                return false;

            return Value == other.Value;
        }

        public override string ToString()
        {
            return $"{{\"Value\":\"{ Value }\",\"RuntimeHostId\":{ RuntimeHostId }}}";
        }
    }
}
