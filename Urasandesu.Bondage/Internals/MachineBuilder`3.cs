/* 
 * File: MachineBuilder`3.cs
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

namespace Urasandesu.Bondage.Internals
{
    class MachineBuilder<TSender, TBundler, TReceiver>
        where TSender : class, IMethodizedMachineSender
        where TBundler : class, IMethodizedMachineSender, IMethodizedMachineReceiver, IMethodizedMachineStatus
        where TReceiver : MethodizedMachineReceiver<TBundler>, IMethodizedMachineReceiver
    {
        readonly static TransitionAndBundlerStorage<TSender, TBundler, TReceiver> ms_transitionAndBundlerStorage = new MachineAndBundlerStorage<TSender, TBundler, TReceiver>();
        readonly static Lazy<(Type transType, Type bundlerType, Type userDefStartState)> ms_transitionAndBundlerType = 
                                                                            new Lazy<(Type, Type, Type)>(() => ms_transitionAndBundlerStorage.DefineTransitionAndBundlerType());

        public static TSender New(RuntimeHost runtimeHost, MachineInterface<TSender, TBundler, TReceiver> @interface)
        {
            (var transType, var bundlerType, var userDefStartState) = ms_transitionAndBundlerType.Value;
            return NewCore(runtimeHost, @interface, transType, bundlerType, userDefStartState);
        }

        static TSender NewCore(RuntimeHost runtimeHost, MachineInterface<TSender, TBundler, TReceiver> @interface, Type transitionType, Type bundlerType, Type userDefStartState)
        {
            var id = runtimeHost.NewMachine(transitionType);
            var bundler = (TBundler)Activator.CreateInstance(bundlerType, runtimeHost, id, @interface.Receiver);
            @interface.Receiver.Self = bundler;
            runtimeHost.BuildUp(bundler.GetType(), bundler);
            runtimeHost.SendEvent(id, new Construct(bundler, userDefStartState));
            return bundler as TSender;
        }
    }
}
