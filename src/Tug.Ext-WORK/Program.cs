using System;
using System.Collections.Generic;
using System.Reflection;
using Tug.Ext;
using Tug.Ext.Provider;
using Tug.Ext.Util;
using static System.Console;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WriteLine("Hello World!");
      
            WriteLine("Found the following FooProviderXs:");
            var femX = new FooExtManagerX();
            foreach (var e in femX.FoundProviders)
                WriteLine($"  * {e}");
            WriteLine();

            WriteLine("Found the following FooProviders:");
            var fem = new FooExtManager();
            foreach (var e in fem.FoundExtensionNames)
                WriteLine($"  * {e}");
            WriteLine();
        }
    }

    public interface IFooExtension : IExtension
    { }

    public class FooExtensionAttribute : ExtensionAttribute
    {
        protected override string Name
        { get; }
    }

    public class FooExtManager : ExtManagerBase<IFooExtension, FooExtensionAttribute>
    {
        public FooExtManager()
        {
            base.AddBuiltIns(typeof(FooExtManagerX).GetTypeInfo().Assembly);
        }
    }

    [FooExtension]
    public class FooBarExtension : IFooExtension
    {
        public FooBarExtension()
        {
            WriteLine("FooBarProvider - CONSTRUCTED");
        }

        public IEnumerable<ExtParameterInfo> DescribeParameters()
        {
            return new ExtParameterInfo[0];
        }
    }

    public interface IThingy : IProviderProduct
    { }

    public interface IThingyProvider : IProviderExtension<IThingy>
    { }

    public class ThingyProviderAttribute : ExtensionAttribute
    {
        public ThingyProviderAttribute(string name)
        {
            Name = name;
        }

        protected override string Name
        { get; }
    }

    public class ThingyExtManager : ExtManagerBase<IThingyProvider, ThingyProviderAttribute>
    { }

    public class MyThingy : IThingy
    {
        bool IProviderProduct.IsDisposed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MyThingy() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    [ThingyProvider("MyThingy")]
    public class MyThingyProvider : IThingyProvider
    {
        public IEnumerable<ExtParameterInfo> DescribeParameters()
        {
            throw new NotImplementedException();
        }

        public IThingy Produce()
        {
            throw new NotImplementedException();
        }

        public void SetParameters(IDictionary<string, object> productParams)
        {
            throw new NotImplementedException();
        }
    }





    public interface IFooX : IExtension
    {
        void DoFoo();
    }

    public interface IFooProviderX : IExtProviderX<IFooX>
    { }

    public class FooExtManagerX : ExtManagerBaseX<IFooX, IFooProviderX>
    {
        public FooExtManagerX()
        {
            base.AddBuiltIns(typeof(FooExtManagerX).GetTypeInfo().Assembly);
        }
    }

    public class FooBarX : IFooX
    {
        public bool IsDisposed
        { get; private set; }

        public void DoFoo() => WriteLine("FooBar!");

        void IFooX.DoFoo()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FooBar() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
//        void IDisposable.Dispose()
//        {
//            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
//            Dispose(true);
//            // TODO: uncomment the following line if the finalizer is overridden above.
//            // GC.SuppressFinalize(this);
//        }

        #endregion
    }

    public class FooBarProviderX : IFooProviderX
    {
        ExtInfo IExtProviderX<IFooX>.Describe()
        { return new ExtInfo(nameof(FooBarX)); }

        IEnumerable<ExtParameterInfo> IExtProviderX<IFooX>.DescribeParameters()
        { return new ExtParameterInfo[0]; }

        IFooX IExtProviderX<IFooX>.Provide(IDictionary<string, object> initParams)
        {
            return new FooBarX();
        }
    }
}
