/* 
 * File: SynchronizedLogger.cs
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
using System;
using System.Runtime.ExceptionServices;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp.IO;
using Urasandesu.Enkidu;
using ST = System.Threading;

namespace Urasandesu.Bondage
{
    public sealed partial class SynchronizedLogger : PublishableLogger, ILogger
    {
        readonly ILogger m_logger;

        public SynchronizedLogger(ILogger logger)
        {
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LoggingVerbosity = logger.LoggingVerbosity;
            Configuration = logger.Configuration;
        }

        int ILogger.LoggingVerbosity
        {
            get => m_logger.LoggingVerbosity;
            set
            {
                LoggingVerbosity = value;
                m_logger.LoggingVerbosity = value;
            }
        }

        Configuration ILogger.Configuration
        {
            get => m_logger.Configuration;
            set
            {
                Configuration = value;
                m_logger.Configuration = value;
            }
        }

        public override void Write(string value)
        {
            lock (m_logger)
                m_logger.Write(value);
        }

        public override void Write(string format, params object[] args)
        {
            lock (m_logger)
                m_logger.Write(format, args);
        }

        public override void WriteLine(string value)
        {
            lock (m_logger)
                m_logger.WriteLine(value);
        }

        public override void WriteLine(string format, params object[] args)
        {
            lock (m_logger)
                m_logger.WriteLine(format, args);
        }

        ISynchronizer m_synchronizer;

        Exception m_exception;
        ST::CancellationTokenSource m_cts = new ST::CancellationTokenSource();

        public override void OnFailure(Exception ex)
        {
            ST::Interlocked.Exchange(ref m_exception, ex);
            m_cts.Cancel();
        }

        public void ApplySynchronization(ISynchronizable synchronizable)
        {
            if (synchronizable == null)
                throw new ArgumentNullException(nameof(synchronizable));

            ClearSynchronization();
            m_synchronizer = synchronizable.GetSynchronizer();
        }

        public void ClearSynchronization()
        {
            m_synchronizer?.Dispose();
            m_synchronizer = null;
        }

        public string WaitForWriting()
        {
            return WaitForWriting(-1, false);
        }

        public string WaitForWriting(TimeSpan timeout, bool throwsExceptionWhenTimeout = true)
        {
            return WaitForWriting((int)timeout.TotalMilliseconds, throwsExceptionWhenTimeout);
        }

        public string WaitForWriting(int millisecondsTimeout, bool throwsExceptionWhenTimeout = true)
        {
            return WaitForWriting(millisecondsTimeout, ST::CancellationToken.None, throwsExceptionWhenTimeout);
        }

        public string WaitForWriting(ST::CancellationToken cancellationToken)
        {
            return WaitForWriting(-1, cancellationToken, false);
        }

        public string WaitForWriting(int millisecondsTimeout, ST::CancellationToken cancellationToken, bool throwsExceptionWhenTimeout = true)
        {
            try
            {
                using (var cts = ST::CancellationTokenSource.CreateLinkedTokenSource(m_cts.Token, cancellationToken))
                    if (m_synchronizer?.NotifyAll(false).Wait(millisecondsTimeout, cts.Token) == false)
                        if (throwsExceptionWhenTimeout)
                            lock (m_logger)
                                throw new TimeoutException("The operation has timed out. Current logger: " + Environment.NewLine + m_logger);
                        else
                            return null;
            }
            catch (OperationCanceledException)
            { }

            if (m_exception != null)
                ExceptionDispatchInfo.Capture(m_exception).Throw();

            lock (m_logger)
                return m_logger.ToString();
        }

        public override string ToString()
        {
            return WaitForWriting();
        }

        bool m_disposed;

        void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    m_cts?.Dispose();
                    m_synchronizer?.Dispose();
                    m_logger.Dispose();
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