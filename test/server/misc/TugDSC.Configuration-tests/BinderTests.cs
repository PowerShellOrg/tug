using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TugDSC.Configuration.Binder;
using TugDSC.Testing.MSTest;

namespace TugDSC.Configuration.Tests
{
    [TestClass]
    public class OfTypeTests
    {
        public abstract class Type1
        {
            public string S1 { get; set; }

            public int I1 { get; set; }

            public bool B1 { get; set; }

            public IValueGetter ValueGetter
            { get; set; }
        }

        public class Type2 : Type1 {}

        public interface IValueGetter
        {
            string GetValue();
        }

        public class BasicValueGetter : IValueGetter
        {
            public string Value
            { get; set; }

            public string GetValue()
            {
                return Value;
            }
        }

        public class ToUpperValueGetter : IValueGetter
        {
            public string Value
            { get; set; }

            public string GetValue()
            {
                return Value?.ToUpper();
            }
        }

        [TestMethod]
        public void TestConfigurationExtendedBinder()
        {
            var dirPath = Path.GetDirectoryName(typeof(OfTypeTests).Assembly.Location);
            var jsonPath = Path.Combine(dirPath, "test1.json");

            var config = new ConfigurationBuilder()
                    .AddJsonFile(jsonPath)
                    .Build();
            
            
            Assert.That.ThrowsAny(() => config.GetSection("Type1Key0").GetExt<Type1>(),
                    "cannot bind to interface or abstract without type hint");

            var key1 = config.GetSection("Type1Key1").GetExt<Type1>();
            var key2 = config.GetSection("Type1Key2").GetExt<Type1>();

            Assert.AreEqual("FooBar", key1.ValueGetter.GetValue());
            Assert.AreEqual("FOOBAR", key2.ValueGetter.GetValue());
        }
    }
}