/* 
 * File: IMethodizedMonitorStatus.cs
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



using Microsoft.Practices.Unity;
using Microsoft.PSharp;
using Microsoft.PSharp.IO;
using System;
using System.ComponentModel;

namespace Urasandesu.Bondage
{
    public interface IMethodizedMonitorStatus
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        RuntimeHost RuntimeHost { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IUnityContainer Container { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Func<ILogger> LoggerGet { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        ILogger Logger { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Func<string> Name { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Func<Type> CurrentStateGet { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Type CurrentState { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Func<Event> ReceivedEventGet { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Action<bool> AssertBool { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Action<bool, string, object[]> AssertBoolStringObjectArray { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Func<int> GetHashedState { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Action<Type> GotoType { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        Action<Event> RaiseEvent { get; set; }
    }
}
