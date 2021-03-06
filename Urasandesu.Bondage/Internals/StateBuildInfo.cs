﻿/* 
 * File: StateBuildInfo.cs
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
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Urasandesu.Bondage.Internals
{
    public class StateBuildInfo
    {
        public StateBuildInfo(TypeBuilder transBldr, TypeBuilder currentStateBldr, IEnumerable<MethodizedStateAttribute> methodizedStateAttrs)
        {
            TransitionTypeBuilder = transBldr ?? throw new ArgumentNullException(nameof(transBldr));
            CurrentStateBuilder = currentStateBldr ?? throw new ArgumentNullException(nameof(currentStateBldr));
            MethodizedStateAttributes = methodizedStateAttrs ?? throw new ArgumentNullException(nameof(methodizedStateAttrs));
        }

        public TypeBuilder TransitionTypeBuilder { get; }
        public TypeBuilder CurrentStateBuilder { get; }
        public IEnumerable<MethodizedStateAttribute> MethodizedStateAttributes { get; }
        public TypeBuilder AutoDefinedStartStateBuilder { get; set; }

        public Type CreateTypeAndGetUserDefinedStartState()
        {
            if (AutoDefinedStartStateBuilder == null)
            {
                CurrentStateBuilder.CreateType();
                return null;
            }
            else
            {
                AutoDefinedStartStateBuilder.CreateType();
                return CurrentStateBuilder.CreateType();
            }
        }
    }
}
