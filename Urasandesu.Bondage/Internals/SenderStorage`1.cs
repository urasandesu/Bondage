/* 
 * File: SenderStorage`1.cs
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Urasandesu.NAnonym.Mixins.System;

namespace Urasandesu.Bondage.Internals
{
    abstract class SenderStorage<TSender> : SenderStorage
    {
        protected override Type DefineSenderType(ModuleBuilder modBldr)
        {
            var parentType = SenderParentType;
            var interfaceTypes = new[] { typeof(TSender) }.Concat(typeof(TSender).GetInterfaces()).ToArray();
            var name = "<Sender>" + typeof(TSender).FullNameWithoutNestedTypeQualification();
            var typeAttr = TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            var senderBldr = modBldr.DefineType(name, typeAttr, parentType, interfaceTypes);

            DefineSenderConstructor(senderBldr, parentType);
            DefineSenderSenderMethods(senderBldr, parentType);
            return senderBldr.CreateType();
        }

        protected abstract Type SenderParentType { get; }

        public override Type GetSenderType()
        {
            if (!typeof(TSender).IsInterface)
                throw new NotSupportedException($"The generic parameter '{ nameof(TSender) }' must be an interface.");

            var senderTypeBase = SenderTypeBase;
            var senderTypes = new[] { typeof(TSender) }.Concat(typeof(TSender).GetInterfaces().Except(new[] { senderTypeBase })).ToArray();
            if (senderTypes.Length != 1)
                throw new NotSupportedException($"The generic parameter '{ nameof(TSender) }' must inherit the base type '{ senderTypeBase.FullName }' only one time.");

            return senderTypes[0];
        }

        protected abstract Type SenderTypeBase { get; }
    }
}
