/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tug.Ext
{
    [TestClass]
    public class Tests
    {
        public static readonly string AUX_TEST_LIB_PATH;

        static Tests()
        {
            var thisPath = Path.GetDirectoryName(typeof(Tests).GetTypeInfo().Assembly.Location);

            // Weird the relative path behavior differs across platforms!?!?!            
#if DOTNET_FRAMEWORK
            var auxPath = Path.GetFullPath(Path.Combine(thisPath, "../../../../../Tug.Ext-tests-aux/bin/Debug/net452"));
#else
            var auxPath = Path.GetFullPath(Path.Combine(thisPath, "../../../../../test/Tug.Ext-tests-aux/bin/Debug/netcoreapp1.0"));
#endif

            AUX_TEST_LIB_PATH = auxPath;
            System.Console.WriteLine("*** Computed AUX_TEST_LIB_PATH:  " + AUX_TEST_LIB_PATH);
        }

        [TestMethod]
        public void TestProviderModel_SimpleManager_FoundProviders()
        {
            var manager = new Tug.TestExt.SimpleThingyProviderManager();
            var names = manager.FoundProvidersNames.ToArray();

            Assert.AreEqual(1, names.Length,
                    message: "found names length");
            CollectionAssert.Contains(names, "basic",
                    message: "found names contains 'basic'");
        }

        [TestMethod]
        public void TestProviderModel_SimpleManager_BasicProviderDetails()
        {
            var manager = new Tug.TestExt.SimpleThingyProviderManager();
            var prov = manager.GetProvider(manager.FoundProvidersNames.First());

            Assert.IsNotNull(prov, message: "get first provider");
            
            var provInfo = prov.Describe();
            Assert.IsNotNull(provInfo,
                    message: "get provider details");
            var provParamsInfo = prov.DescribeParameters();
            Assert.IsNotNull(provParamsInfo,
                    message:  "get provider parameter details");
            Assert.IsTrue(provParamsInfo.Count() > 0,
                    message:  "get provider parameter details has at least one");
            Assert.IsFalse(provParamsInfo.All(x => string.IsNullOrEmpty(x.Name)),
                    message:  "get provider parameter details all have names");
        }

        [TestMethod]
        public void TestProviderModel_SimpleManager_BasicProductBehavior1()
        {
            var manager = new Tug.TestExt.SimpleThingyProviderManager();
            var prov = manager.GetProvider(manager.FoundProvidersNames.First());
            var prod = prov.Produce();

            Assert.IsFalse(prod.IsDisposed);
            prod.Dispose();
            Assert.IsTrue(prod.IsDisposed);
        }

        [TestMethod]
        public void TestProviderModel_SimpleManager_BasicProductBehavior2()
        {
            var value = "foobar";
            var manager = new Tug.TestExt.SimpleThingyProviderManager();
            var prov = manager.GetProvider(manager.FoundProvidersNames.First());
            var prod = prov.Produce();

            Assert.IsFalse(prod.IsDisposed);
            prod.SetThing(value);
            Assert.AreEqual(value, prod.GetThing());
            prod.Dispose();
            Assert.IsTrue(prod.IsDisposed);
        }

        [TestMethod]
        public void TestProviderModel_SimpleManager_BasicProductBehavior3()
        {
            var value = "foobar";
            var prefix = "Before<";
            var suffix = ">After";
            var manager = new Tug.TestExt.SimpleThingyProviderManager();
            var prov = manager.GetProvider(manager.FoundProvidersNames.First());

            prov.SetParameters(new Dictionary<string, object>
            {
                ["Prefix"] = prefix,
                ["Suffix"] = suffix,
            });

            var prod = prov.Produce();
            

            Assert.IsFalse(prod.IsDisposed);
            prod.SetThing(value);
            Assert.AreEqual($"{prefix}{value}{suffix}", prod.GetThing());
            prod.Dispose();
            Assert.IsTrue(prod.IsDisposed);
        }

        [TestMethod]
        public void TestProviderModel_DynamicManager_WithoutDynamicProvider()
        {
            var manager = new Tug.TestExt.DynamicThingyProviderManager();
            var names = manager.FoundProvidersNames.ToArray();

            Assert.AreEqual(1, names.Length,
                    message: "found names length");
            CollectionAssert.Contains(names, "basic",
                    message: "found names contains 'basic'");
        }

        [TestMethod]
        public void TestProviderModel_DynamicManager_WithDynamicProvider_SearchPath()
        {
            var path = Path.GetFullPath(AUX_TEST_LIB_PATH);

            var manager = new Tug.TestExt.DynamicThingyProviderManager(
                    searchPaths: new[] { path });
            var names = manager.FoundProvidersNames.ToArray();

            Assert.AreEqual(2, names.Length,
                    message: "found names length");
            CollectionAssert.Contains(names, "basic",
                    message: "found names contains 'basic'");
            CollectionAssert.Contains(names, "func",
                    message: "found names contains 'func'");
        }


        [TestMethod]
        public void TestProviderModel_DynamicManager_WithDynamicProvider_SearchAssembly()
        {
            var asmPath = Path.GetFullPath(AUX_TEST_LIB_PATH + "/Tug.Ext-tests-aux.dll");
            var asm = Tug.Ext.Util.MefExtensions.LoadFromAssembly(asmPath);

            var manager = new Tug.TestExt.DynamicThingyProviderManager(
                    // resetBuiltIns: true,
                    resetSearchAssemblies: true,
                    searchAssemblies: new[] { asm });
            var names = manager.FoundProvidersNames.ToArray();

            Assert.AreEqual(2, names.Length,
                    message: "found names length");
            CollectionAssert.Contains(names, "basic",
                    message: "found names contains 'basic'");
            CollectionAssert.Contains(names, "func",
                    message: "found names contains 'func'");
        }
    }
}
