/* 
 * File: MethodizedMachineReceiver`1.cs
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
using Microsoft.PSharp.IO;

namespace Urasandesu.Bondage
{
    public class MethodizedMachineReceiver<TBundler>
        where TBundler : class, IMethodizedMachineSender, IMethodizedMachineReceiver, IMethodizedMachineStatus
    {
        protected internal TBundler Self { get; internal set; }

        public MachineId Id { get { return Self.Id; } }

        public ILogger Logger { get => Self.Logger; }

        protected void Halt()
        {
            Self.RaiseEvent(new Halt());
        }

        protected void Assert(bool predicate)
        {
            Self.AssertBool(predicate);
        }

        protected void Assert(bool predicate, string s, params object[] args)
        {
            Self.AssertBoolStringObjectArray(predicate, s, args);
        }

        protected virtual bool Random()
        {
            return Self.Random();
        }

        protected virtual bool Random(int maxValue)
        {
            return Self.RandomInt32(maxValue);
        }

        protected virtual int RandomInteger(int maxValue)
        {
            return Self.RandomIntegerInt32(maxValue);
        }
    }
}
