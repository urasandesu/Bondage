/* 
 * File: ConfigurationMixinTest.cs
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
using System.IO;
using System.Reflection;
using Urasandesu.Bondage.Mixins.Microsoft.PSharp;

namespace Test.Urasandesu.Bondage.Mixins.Microsoft.PSharp
{

    [TestFixture]
    public class ConfigurationMixinTest
    {
        [Test]
        public void GetCurrentTestArtifactLocation_should_return_artifact_location_according_to_the_test()
        {
            // Arrange
            var configuration = Configuration.Create();

            // Act
            var testArtifact = configuration.GetCurrentTestArtifactLocation(new DateTime(2017, 10, 5, 12, 23, 34));

            // Assert
            Assert.That(testArtifact, Does.StartWith(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Output\20171005122334\Test.Urasandesu.Bondage.dll")));
            Assert.That(testArtifact, Does.Match(@"\\Test[0-9A-F]{8}_\d+"));
        }



        [Test]
        public void GetTestArtifactLocation_should_return_artifact_location_according_to_the_test()
        {
            // Arrange
            var configuration = Configuration.Create();

            // Act
            var testArtifact = configuration.GetTestArtifactLocation(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 5, 12, 23, 34));

            // Assert
            Assert.That(testArtifact, Does.StartWith(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Output\20171005122334\Test.Urasandesu.Bondage.dll")));
            Assert.That(testArtifact, Does.Match(@"\\Test[0-9A-F]{8}_\d+"));
        }



        [Test]
        public void CreateTestArtifact_should_create_the_directory_to_store_the_artifacts()
        {
            // Arrange
            var configuration = Configuration.Create();

            // Act
            var testArtifact = configuration.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 5, 12, 23, 34));

            // Assert
            Assert.IsTrue(Directory.Exists(testArtifact.Directory));
            Assert.That(testArtifact.Directory, Does.StartWith(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Output\20171005122334\Test.Urasandesu.Bondage.dll")));
            Assert.That(testArtifact.Directory, Does.Match(@"\\Test[0-9A-F]{8}_\d+"));
        }



        [Test]
        public void DeleteTestArtifact_should_delete_the_artifacts_of_the_test()
        {
            // Arrange
            var configuration = Configuration.Create();
            var testArtifact = configuration.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 5, 12, 23, 34));

            // Act
            configuration.DeleteTestArtifact(testArtifact);

            // Assert
            Assert.IsFalse(Directory.Exists(testArtifact.Directory));
        }



        [Test]
        public void GetTestArtifact_should_not_create_the_directory_immediately()
        {
            // Arrange
            var configuration = Configuration.Create();

            // Act
            var testArtifact = configuration.GetTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 5, 12, 23, 34));

            // Assert
            Assert.IsFalse(Directory.Exists(testArtifact.Directory));
        }

        [Test]
        public void GetTestArtifact_should_restore_same_test_info_if_it_has_already_been_created()
        {
            // Arrange
            var configuration = Configuration.Create();
            configuration.CreateTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 5, 12, 23, 34));

            // Act
            var testArtifact = configuration.GetTestArtifact(MethodBase.GetCurrentMethod(), new DateTime(2017, 10, 5, 12, 23, 34));

            // Assert
            Assert.IsNotNull(testArtifact);
            Assert.AreEqual(MethodBase.GetCurrentMethod(), testArtifact.Test);
        }
    }
}
