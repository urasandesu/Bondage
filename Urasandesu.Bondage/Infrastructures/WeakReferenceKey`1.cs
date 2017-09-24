/* 
 * File: WeakReferenceKey`1.cs
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

namespace Urasandesu.Bondage.Infrastructures
{
    sealed class WeakReferenceKey<TKey> where TKey : class
    {
        readonly int m_hashCode;
        readonly WeakReference<TKey> m_reference;
        public WeakReferenceKey(TKey key)
        {
            m_hashCode = key.GetHashCode();
            m_reference = new WeakReference<TKey>(key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = default(WeakReferenceKey<TKey>);
            if ((other = obj as WeakReferenceKey<TKey>) == null)
                return false;

            if (m_hashCode != other.m_hashCode)
                return false;

            if (!TryGetTarget(out var thisTarget) || !other.TryGetTarget(out var otherTarget))
                return false;
            else
                return thisTarget.Equals(otherTarget);
        }

        public override int GetHashCode()
        {
            return m_hashCode;
        }

        public bool TryGetTarget(out TKey target)
        {
            return m_reference.TryGetTarget(out target);
        }

        public bool IsAlive => TryGetTarget(out var _);
    }
}
