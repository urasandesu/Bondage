/* 
 * File: InitializerIntegrationTest.cs
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
using NUnit.Framework;
using Urasandesu.Bondage;
using Urasandesu.Enkidu;
using ST = System.Threading;

namespace Test.Urasandesu.Bondage
{
    [TestFixture]
    public class InitializerIntegrationTest
    {
        public class E1 : ApplicationEvent { }

        public class M1
        {
            public interface ISender : IMethodizedMachineSender { }

            public interface IReceiver : IMethodizedMachineReceiver
            {
                void EnterInitialized();
            }

            public class Receiver : MethodizedMachineReceiver<IBundler>, IReceiver
            {
                public void EnterInitialized()
                {
                    Logger.WriteLine("*** EnterInitialized ***");
                    ST::Thread.Sleep(500);
                }
            }

            public interface IBundler : ISender, IReceiver, IMethodizedMachineStatus
            {
                [Initializer]
                [OnEnterInvoke(nameof(EnterInitialized))]
                void Initialized();
            }
        }

        public class E2 : ApplicationEvent { }

        public class M2
        {
            public interface ISender : IMethodizedMachineSender { }

            public interface IReceiver : IMethodizedMachineReceiver
            {
                void EnterInitialized();
            }

            public class Receiver : MethodizedMachineReceiver<IBundler>, IReceiver
            {
                public void EnterInitialized()
                {
                    Logger.WriteLine("+++ EnterInitialized +++");
                }
            }

            public interface IBundler : ISender, IReceiver, IMethodizedMachineStatus
            {
                [Initializer]
                [OnEnterInvoke(nameof(EnterInitialized))]
                void Initialized();
            }
        }

        [Test]
        public void Initializer_and_OnEnterInvoke_can_specify_to_the_same_place()
        {
            // Arrange
            var configuration = Configuration.Create().WithVerbosityEnabled(2);
            var runtime = PSharpRuntime.Create(configuration);
            var runtimeHost = HostInfo.NewRuntimeHost(runtime);

            var logger = new SynchronizedLogger(new InMemoryLogger());
            var waitIfEnteredInitialized1 = logger.MachineActionHandledWait((machineId, _, actionName) => machineId.Name.Contains("M1") && actionName == "EnterInitialized");
            var waitIfEnteredInitialized2 = logger.MachineActionHandledWait((machineId, _, actionName) => machineId.Name.Contains("M2") && actionName == "EnterInitialized");
            logger.ApplySynchronization(waitIfEnteredInitialized2.Then(waitIfEnteredInitialized1));
            runtimeHost.SetLogger(logger);


            // Act
            runtimeHost.New(MachineInterface.Sender<M1.ISender>().Bundler<M1.IBundler>().Receiver<M1.Receiver>());
            runtimeHost.New(MachineInterface.Sender<M2.ISender>().Bundler<M2.IBundler>().Receiver<M2.Receiver>());


            // Assert
            Assert.DoesNotThrow(() => logger.WaitForWriting(1000));
        }
    }
}
