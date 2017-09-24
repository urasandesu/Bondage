/* 
 * File: InMemoryLogger.cs
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
using System.IO;

namespace Urasandesu.Bondage
{
    public sealed class InMemoryLogger : StateMachineLogger
    {
        readonly StringWriter m_writer = new StringWriter();

        public InMemoryLogger(int loggingVerbosity = 2) :
            base(loggingVerbosity)
        {
            Configuration = Configuration.Create();
        }

        public override void Write(string value)
        {
            m_writer.Write(value);
        }

        public override void Write(string format, params object[] args)
        {
            m_writer.Write(format, args);
        }

        public override void WriteLine(string value)
        {
            m_writer.WriteLine(value);
        }

        public override void WriteLine(string format, params object[] args)
        {
            m_writer.WriteLine(format, args);
        }

        public override string ToString()
        {
            return m_writer.ToString();
        }

        bool m_disposed;

        void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    m_writer.Dispose();
                }

                m_disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }

}
