/* 
 * File: WeakReferenceKeyValueTable`2.cs
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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ST = System.Threading;

namespace Urasandesu.Bondage.Infrastructures
{

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The Timer should continue to run until the timing that garbage collector collects this instance.")]
    public class WeakReferenceKeyValueTable<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        readonly ConcurrentDictionary<WeakReferenceKey<TKey>, WeakReference<TValue>> m_entries;
        readonly ST::Timer m_gc;

        public WeakReferenceKeyValueTable()
        {
            m_entries = new ConcurrentDictionary<WeakReferenceKey<TKey>, WeakReference<TValue>>();
            m_gc = new ST::Timer(CollectGarbage, m_entries, 0, 1000);
        }

        static void CollectGarbage(object state)
        {
            var entries = (ConcurrentDictionary<WeakReferenceKey<TKey>, WeakReference<TValue>>)state;
            var deadKeys = entries.Where(_ => !_.Key.IsAlive || !_.Value.TryGetTarget(out var __)).Select(_ => _.Key).ToArray();
            foreach (var deadKey in deadKeys)
                entries.TryRemove(deadKey, out var _);
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return m_entries.AddOrUpdate(new WeakReferenceKey<TKey>(key),
                                         _ => _.TryGetTarget(out var rawKey) ? new WeakReference<TValue>(addValueFactory(rawKey)) : default,
                                         (_1, _2) => _1.TryGetTarget(out var rawKey) ? new WeakReference<TValue>(updateValueFactory(rawKey, _2.TryGetTarget(out var rawValue1) ? rawValue1 : default)) : default).
                             TryGetTarget(out var rawValue) ? rawValue : default;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return m_entries.AddOrUpdate(new WeakReferenceKey<TKey>(key),
                                         new WeakReference<TValue>(addValue),
                                         (_1, _2) => _1.TryGetTarget(out var target) ? new WeakReference<TValue>(updateValueFactory(target, _2.TryGetTarget(out var rawValue1) ? rawValue1 : default)) : default).
                             TryGetTarget(out var rawValue) ? rawValue : default;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return m_entries.GetOrAdd(new WeakReferenceKey<TKey>(key), _ => _.TryGetTarget(out var target) ? new WeakReference<TValue>(valueFactory(target)) : default).
                             TryGetTarget(out var rawValue) ? rawValue : default;
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return m_entries.GetOrAdd(new WeakReferenceKey<TKey>(key), new WeakReference<TValue>(value)).
                             TryGetTarget(out var rawValue) ? rawValue : default;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return m_entries.TryAdd(new WeakReferenceKey<TKey>(key), new WeakReference<TValue>(value));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var result = m_entries.TryGetValue(new WeakReferenceKey<TKey>(key), out var weakValue);
            if (!result)
                return false;

            return weakValue.TryGetTarget(out value);
        }
    }
}
