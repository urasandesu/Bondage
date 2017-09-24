/* 
 * File: MonitorAndBundlerStorage`3.cs
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
using System.Reflection;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.NAnonym.Mixins.System;

namespace Urasandesu.Bondage.Internals
{
    class MonitorAndBundlerStorage<TSender, TBundler, TReceiver> : TransitionAndBundlerStorage<TSender, TBundler, TReceiver>
        where TSender : class, IMethodizedMonitorSender
        where TBundler : class, IMethodizedMonitorSender, IMethodizedMonitorReceiver, IMethodizedMonitorStatus
        where TReceiver : MethodizedMonitorReceiver<TBundler>, IMethodizedMonitorReceiver
    {
        protected override SenderStorage<TSender> SenderStorage { get; } = new MonitorSenderStorage<TSender>();

        protected override Type ReceiverTypeBase => typeof(IMethodizedMonitorReceiver);

        protected override string TransitionTypeName => "<Monitor>" + typeof(TBundler).FullNameWithoutNestedTypeQualification();

        protected override Type TransitionParentType => typeof(ApplicationMonitor<TBundler>);

        protected override Type TransitionStateParentType => typeof(MonitorState);

        protected override Type TransitionType => typeof(Monitor);

        protected override Type BundlerGenericParentTypeDefinition => typeof(MethodizedMonitorBundler<>);

        protected override Type TransitionIdType => typeof(MonitorId);

        protected override MethodInfo GetHandledLogMethod(Type baseType)
        {
            return baseType.GetMethod("MonitorHandledLog", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
