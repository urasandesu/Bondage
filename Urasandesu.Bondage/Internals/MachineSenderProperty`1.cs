/* 
 * File: MachineSenderProperty`1.cs
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
using Urasandesu.Bondage.Infrastructures;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;

namespace Urasandesu.Bondage.Internals
{
    class MachineSenderProperty<TSender>
        where TSender : class, IMethodizedMachineSender
    {
        readonly static WeakReferenceKeyValueTable<MachineId, TSender> ms_senders = new WeakReferenceKeyValueTable<MachineId, TSender>();
        readonly static SenderTypeBuilder<TSender> ms_senderStorage = new MachineSenderTypeBuilder<TSender>();
        readonly static Lazy<Type> ms_senderType = new Lazy<Type>(() => ms_senderStorage.DefineSenderType());
        public static TSender Get(CommunicationId key, MachineId id)
        {
            return ms_senders.GetOrAdd(id, _ => New(RuntimeHostReferences.Get(key.RuntimeHostId), id));
        }

        static TSender New(RuntimeHost runtimeHost, MachineId id)
        {
            var senderType = ms_senderType.Value;
            return NewCore(runtimeHost, id, senderType);
        }

        static TSender NewCore(RuntimeHost runtimeHost, MachineId id, Type senderType)
        {
            return (TSender)Activator.CreateInstance(senderType, runtimeHost, id);
        }

        public static void Set(ref MachineId id, TSender value)
        {
            id = value.Id;
            ms_senders.AddOrUpdate(id, value, (_1, _2) => value);
        }
    }
}
