/* 
 * File: DistributedDictionary`2.cs
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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Urasandesu.Bondage
{
    [DataContract]
    public class DistributedDictionary<TKey, TValue> : ApplicationContext, ISharedDictionary<TKey, TValue>
    {
        ISharedDictionary<TKey, TValue> m_dic;

        public DistributedDictionary()
        {
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { TryAddCore, TryUpdateCore, thisGetCore, thisSetCore, TryRemoveCore, CountGetCore });
        }

        protected override void OnDeserializedCore(StreamingContext ctx)
        {
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { TryAddCore, TryUpdateCore, thisGetCore, thisSetCore, TryRemoveCore, CountGetCore });
        }

        protected override void OnLinkedTo(RuntimeHost runtimeHost, params object[] args)
        {
            if (args == null || args.Length == 0)
                m_dic = SharedDictionary.Create<TKey, TValue>(runtimeHost.Runtime);
            else if (args.Length == 1 && args[0] is IEqualityComparer<TKey> comparer)
                m_dic = SharedDictionary.Create<TKey, TValue>(comparer, runtimeHost.Runtime);
            else
                throw new ArgumentOutOfRangeException(nameof(args), "The value needs to translate in null, empty or the array that has just one element as IEqualityComparer<TKey>.");
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return (bool)RuntimeHost.DoCommunication(Id, TryAddCore, key, value);
        }
        object TryAddCore(params object[] args)
        {
            return m_dic.TryAdd((TKey)args[0], (TValue)args[1]);
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            return (bool)RuntimeHost.DoCommunication(Id, TryUpdateCore, key, newValue, comparisonValue);
        }
        object TryUpdateCore(params object[] args)
        {
            return m_dic.TryUpdate((TKey)args[0], (TValue)args[1], (TValue)args[2]);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var tmp = new object[1];
            var result = (bool)RuntimeHost.DoCommunication(Id, TryGetValue, key, tmp);
            value = (TValue)tmp[0];
            return result;
        }
        object TryGetValue(params object[] args)
        {
            var result = m_dic.TryGetValue((TKey)args[0], out var value);
            ((object[])args[1])[0] = value;
            return result;
        }

        public TValue this[TKey key]
        {
            get => (TValue)RuntimeHost.DoCommunication(Id, thisGetCore, key);
            set => RuntimeHost.DoCommunication(Id, thisSetCore, key, value);
        }
        object thisGetCore(params object[] args)
        {
            return m_dic[(TKey)args[0]];
        }
        object thisSetCore(params object[] args)
        {
            m_dic[(TKey)args[0]] = (TValue)args[1];
            return null;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var tmp = new object[1];
            var result = (bool)RuntimeHost.DoCommunication(Id, TryRemoveCore, key, tmp);
            value = (TValue)tmp[0];
            return result;
        }
        object TryRemoveCore(params object[] args)
        {
            var result = m_dic.TryRemove((TKey)args[0], out var value);
            ((object[])args[1])[0] = value;
            return result;
        }

        public int Count => (int)RuntimeHost.DoCommunication(Id, CountGetCore);
        object CountGetCore(params object[] args)
        {
            return m_dic.Count;
        }
    }
}
