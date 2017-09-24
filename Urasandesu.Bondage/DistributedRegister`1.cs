/* 
 * File: DistributedRegister`1.cs
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
    public class DistributedRegister<T> : ApplicationContext, ISharedRegister<T> where T : struct
    {
        ISharedRegister<T> m_reg;

        public DistributedRegister()
        {
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { UpdateCore, GetValueCore, SetValueCore });
        }

        protected override void OnDeserializedCore(StreamingContext ctx)
        {
            RuntimeHost.RegisterCommunication(Id, new Func<object[], object>[] { UpdateCore, GetValueCore, SetValueCore });
        }

        protected override void OnLinkedTo(RuntimeHost runtimeHost, params object[] args)
        {
            if (args == null || args.Length == 0)
                m_reg = SharedRegister.Create<T>(runtimeHost.Runtime);
            else if (args.Length == 1 && args[0] is T value)
                m_reg = SharedRegister.Create<T>(runtimeHost.Runtime, value);
            else
                throw new ArgumentOutOfRangeException(nameof(args), "The value needs to translate in null, empty or the array that has just one element as T.");
        }

        public T Update(Func<T, T> func)
        {
            return (T)RuntimeHost.DoCommunication(Id, UpdateCore, func);
        }
        object UpdateCore(params object[] args)
        {
            return m_reg.Update((Func<T, T>)args[0]);
        }

        public T GetValue()
        {
            return (T)RuntimeHost.DoCommunication(Id, GetValueCore);
        }
        object GetValueCore(params object[] args)
        {
            return m_reg.GetValue();
        }

        public void SetValue(T value)
        {
            RuntimeHost.DoCommunication(Id, SetValueCore, value);
        }
        object SetValueCore(params object[] args)
        {
            m_reg.SetValue((T)args[0]);
            return null;
        }
    }
}
