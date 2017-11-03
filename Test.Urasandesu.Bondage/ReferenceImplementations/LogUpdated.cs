/* 
 * File: LogUpdated.cs
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
using System.Runtime.Serialization;
using Urasandesu.Bondage;

namespace Test.Urasandesu.Bondage.ReferenceImplementations
{
    [DataContract]
    public class LogUpdated : ApplicationEvent
    {
        [DataMember]
        MachineId m_storageNodeId;
        public IStorageNodeSender StorageNode
        {
            get => GetSender<IStorageNodeSender>(m_storageNodeId);
            set => SetSender(ref m_storageNodeId, value);
        }

        [DataMember]
        public long Log { get; private set; }

        public LogUpdated(IStorageNodeSender storageNode, long log)
        {
            StorageNode = storageNode;
            Log = log;
        }
    }
}
