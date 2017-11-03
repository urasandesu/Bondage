/* 
 * File: MyRetryAttribute.cs
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



using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Runtime.ExceptionServices;

namespace Test.Urasandesu.Bondage.Application
{
    // This is the workaround for [NUnit Issues#2325](https://github.com/nunit/nunit/issues/2325).
    // TODO: Migrate to the version that the above issue is fixed.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MyRetryAttribute : PropertyAttribute, IWrapSetUpTearDown
    {
        int m_count;

        public MyRetryAttribute(int count) :
            base(count)
        {
            m_count = count;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new MyRetryCommand(command, m_count);
        }

        public class MyRetryCommand : DelegatingTestCommand
        {
            int m_retryCount;

            public MyRetryCommand(TestCommand innerCommand, int retryCount) :
                base(innerCommand)
            {
                m_retryCount = retryCount;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var count = m_retryCount;
                var nunitEx = default(NUnitException);
                while (0 < count--)
                {
                    var resultState = default(ResultState);
                    try
                    {
                        nunitEx = null;
                        context.CurrentResult = innerCommand.Execute(context);
                        resultState = context.CurrentResult.ResultState;
                    }
                    catch (NUnitException ex)
                    {
                        resultState = ResultState.Failure;
                        nunitEx = ex;
                    }

                    if (resultState != ResultState.Failure)
                        break;
                }

                if (nunitEx != null)
                    ExceptionDispatchInfo.Capture(nunitEx).Throw();

                return context.CurrentResult;
            }
        }
    }
}
