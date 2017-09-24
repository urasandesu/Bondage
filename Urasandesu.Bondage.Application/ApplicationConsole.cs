/* 
 * File: ApplicationConsole.cs
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
using System;

namespace Urasandesu.Bondage.Application
{
    public abstract class ApplicationConsole : IDisposable
    {
        bool m_disposed = false;

        protected abstract ApplicationViewModel ApplicationViewModel { get; }

        public IUnityContainer Container { get; private set; }

        public void RegisterController<TController>() where TController : ApplicationController
        {
            Container.RegisterType<TController, TController>();
        }

        [InjectionMethod]
        public void Initialize(IUnityContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            Container = container.CreateChildContainer();
            Container.BuildUp(ApplicationViewModel);

            InitializeCore(container);
        }

        protected abstract void InitializeCore(IUnityContainer container);

        public int Run(params string[] args)
        {
            return RunCore(args);
        }

        protected abstract int RunCore(params string[] args);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
                Container?.Dispose();
            }

            // Free any unmanaged objects here. 
            //
            m_disposed = true;
        }

        ~ApplicationConsole()
        {
            Dispose(false);
        }
    }
}
