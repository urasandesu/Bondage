/* 
 * File: DistributedObjectsIntegrationTest.cs
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
using System;
using Test.Urasandesu.Bondage.ReferenceImplementations;
using Urasandesu.Bondage;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;
using Urasandesu.Bondage.Mixins.System;
using Urasandesu.NAnonym.Mixins.System;

namespace Test.Urasandesu.Bondage
{
    [TestFixture]
    public class DistributedObjectsIntegrationTest
    {
        [Test]
        public void Dictionary_can_communicate_remote_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var expected_Assign = new MarshalByRefAction<int>(i => expected = i);
            var actual_Assign = new MarshalByRefAction<int>(i => actual = i);

            // Act
            AppDomain.CurrentDomain.RunAtIsolatedDomain((expected_Assign_, actual_Assign_) =>
            {
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);
                using (var monitorsNetworkProvider = new DomainCommunicationProvider(runtimeHost, "monitors"))
                {
                    var parameter = default(string);
                    {
                        runtimeHost.SetNetworkProvider(monitorsNetworkProvider);

                        var dic = runtimeHost.NewDistributedDictionary<int, string>();
                        dic.TryAdd(1, "a");
                        dic.TryAdd(2, "b");
                        expected_Assign_.Invoke(dic.Count);
                        parameter = dic.ToJson();
                    }

                    AppDomain.CurrentDomain.RunAtIsolatedDomain((actual_Assign__, parameter_) =>
                    {
                        var dic = parameter_.FromJson<DistributedDictionary<int, string>>();
                        actual_Assign__.Invoke(dic.Count);
                    }, actual_Assign_, parameter);
                }
            }, expected_Assign, actual_Assign);


            // Assert
            Assert.AreEqual(expected, actual);
        }



        [Test]
        public void Register_can_communicate_remote_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var expected_Assign = new MarshalByRefAction<int>(i => expected = i);
            var actual_Assign = new MarshalByRefAction<int>(i => actual = i);

            // Act
            AppDomain.CurrentDomain.RunAtIsolatedDomain((expected_Assign_, actual_Assign_) =>
            {
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);
                using (var monitorsNetworkProvider = new DomainCommunicationProvider(runtimeHost, "monitors"))
                {
                    var parameter = default(string);
                    {
                        runtimeHost.SetNetworkProvider(monitorsNetworkProvider);

                        var reg = runtimeHost.NewDistributedRegister(42);
                        reg.Update(prev => prev + 1);
                        expected_Assign_.Invoke(reg.GetValue());
                        parameter = reg.ToJson();
                    }

                    AppDomain.CurrentDomain.RunAtIsolatedDomain((actual_Assign__, parameter_) =>
                    {
                        var reg = parameter_.FromJson<DistributedRegister<int>>();
                        actual_Assign__.Invoke(reg.GetValue());
                    }, actual_Assign_, parameter);
                }
            }, expected_Assign, actual_Assign);


            // Assert
            Assert.AreEqual(expected, actual);
        }



        [Test]
        public void Counter_can_communicate_remote_application()
        {
            // Arrange
            var expected = default(int);
            var actual = default(int);
            var expected_Assign = new MarshalByRefAction<int>(i => expected = i);
            var actual_Assign = new MarshalByRefAction<int>(i => actual = i);

            // Act
            AppDomain.CurrentDomain.RunAtIsolatedDomain((expected_Assign_, actual_Assign_) =>
            {
                var configuration = Configuration.Create().WithMonitorsInProductionEnabled();
                var runtime = PSharpRuntime.Create(configuration);
                var runtimeHost = HostInfo.NewRuntimeHost(runtime);
                using (var monitorsNetworkProvider = new DomainCommunicationProvider(runtimeHost, "monitors"))
                {
                    var parameter = default(string);
                    {
                        runtimeHost.SetNetworkProvider(monitorsNetworkProvider);

                        var counter = runtimeHost.NewDistributedCounter(42);
                        counter.Increment();
                        counter.Increment();
                        expected_Assign_.Invoke(counter.GetValue());
                        parameter = counter.ToJson();
                    }

                    AppDomain.CurrentDomain.RunAtIsolatedDomain((actual_Assign__, parameter_) =>
                    {
                        var counter = parameter_.FromJson<DistributedCounter>();
                        counter.Decrement();
                        actual_Assign__.Invoke(counter.GetValue());
                    }, actual_Assign_, parameter);
                }
            }, expected_Assign, actual_Assign);


            // Assert
            Assert.AreEqual(expected, actual + 1);
        }
    }
}
