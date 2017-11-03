/* 
 * File: MainServersViewModel.cs
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



using Reactive.Bindings;
using System;
using System.Collections.Specialized;
using Test.Urasandesu.Bondage.ReferenceImplementations;
using Urasandesu.Bondage.Application;
using Urasandesu.Bondage.Application.Mixins.System;

namespace Test.Urasandesu.Bondage.Application.ReferenceImplementations.Servers
{
    class MainServersViewModel : ApplicationViewModel
    {
        MessageCollection m_messages;
        public MessageCollection Messages
        {
            get { return m_messages; }
            set
            {
                DisplayMessages?.Dispose();
                SetProperty(ref m_messages, value);
                DisplayMessages = value?.ToCollectionChanged().
                                         DelayUntilNextSchedule(_ => _.Action == NotifyCollectionChangedAction.Reset ? TimeSpan.Zero : TimeSpan.FromMilliseconds(50)).
                                         ToReadOnlyReactiveCollection(true);
            }
        }

        ReadOnlyReactiveCollection<Message> m_displayMessages;
        public ReadOnlyReactiveCollection<Message> DisplayMessages
        {
            get { return m_displayMessages; }
            private set { SetProperty(ref m_displayMessages, value); }
        }

        public ApplicationProperty<int> ExitCode { get; } = new ApplicationProperty<int>(-1);

        ReactiveCommand m_loadedCommand;
        public ReactiveCommand LoadedCommand
        {
            get
            {
                if (m_loadedCommand == null)
                    m_loadedCommand = new LoadedCommand(this);
                return m_loadedCommand;
            }
        }

        public DistributedStorageContext Context { get; set; }
    }
}
