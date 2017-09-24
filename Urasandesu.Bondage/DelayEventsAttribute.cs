﻿/* 
 * File: DelayEventsAttribute.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Urasandesu.Bondage.Internals;

namespace Urasandesu.Bondage
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class DelayEventsAttribute : MethodizedStateAttribute
    {
        public DelayEventsAttribute(params string[] events)
        {
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public IReadOnlyCollection<string> Events { get; private set; }

        internal override void SetStateAttributeTo<TSender, TReceiver, TBundler>(StateBuildInfo stateBuildInfo, IEnumerable<TypeBuilder> allStateBldrs)
        {
            var eventTypes = Events.Select(GetEventType<TSender, TBundler>).ToArray();
            var ctor = typeof(DeferEvents).GetConstructor(new[] { typeof(Type[]) });
            stateBuildInfo.CurrentStateBuilder.SetCustomAttribute(new CustomAttributeBuilder(ctor, new object[] { eventTypes }));
        }
    }
}