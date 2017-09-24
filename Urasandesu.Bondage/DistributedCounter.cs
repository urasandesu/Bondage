/* 
 * File: DistributedCounter.cs
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



using Microsoft.PSharp.SharedObjects;
using System;
using System.Runtime.Serialization;

namespace Urasandesu.Bondage
{
    [DataContract]
    public class DistributedCounter : ApplicationContext, ISharedCounter
    {
        ISharedCounter m_counter;

        public DistributedCounter()
        {
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { IncrementCore, DecrementCore, GetValueCore, AddCore, ExchangeCore, CompareExchangeCore });
        }

        protected override void OnDeserializedCore(StreamingContext ctx)
        {
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { IncrementCore, DecrementCore, GetValueCore, AddCore, ExchangeCore, CompareExchangeCore });
        }

        protected override void OnLinkedTo(RuntimeHost runtimeHost, params object[] args)
        {
            if (args == null || args.Length == 0)
                m_counter = SharedCounter.Create(runtimeHost.Runtime);
            else if (args.Length == 1 && args[0] is int value)
                m_counter = SharedCounter.Create(runtimeHost.Runtime, value);
            else
                throw new ArgumentOutOfRangeException(nameof(args), "The value needs to translate in null, empty or the array that has just one element as int.");
        }

        public void Increment()
        {
            RuntimeHost.DoCommunication(Id, IncrementCore);
        }
        object IncrementCore(params object[] args)
        {
            m_counter.Increment();
            return null;
        }

        public void Decrement()
        {
            RuntimeHost.DoCommunication(Id, DecrementCore);
        }
        object DecrementCore(params object[] args)
        {
            m_counter.Decrement();
            return null;
        }

        public int GetValue()
        {
            return (int)RuntimeHost.DoCommunication(Id, GetValueCore);
        }
        object GetValueCore(params object[] args)
        {
            return m_counter.GetValue();
        }

        public int Add(int value)
        {
            return (int)RuntimeHost.DoCommunication(Id, AddCore, value);
        }
        object AddCore(params object[] args)
        {
            return m_counter.Add((int)args[0]);
        }

        public int Exchange(int value)
        {
            return (int)RuntimeHost.DoCommunication(Id, ExchangeCore, value);
        }
        object ExchangeCore(params object[] args)
        {
            return m_counter.Exchange((int)args[0]);
        }

        public int CompareExchange(int value, int comparand)
        {
            return (int)RuntimeHost.DoCommunication(Id, CompareExchangeCore, value, comparand);
        }
        object CompareExchangeCore(params object[] args)
        {
            return m_counter.CompareExchange((int)args[0], (int)args[1]);
        }
    }
}
