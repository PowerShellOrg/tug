using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Tug.Ext;

namespace Tug.Providers
{
    public class Sha256ChecksumAlgorithmProvider : IChecksumAlgorithmProvider
    {
        public const string PROVIDER_NAME = "SHA-256";

        private static readonly ProviderInfo INFO = new ProviderInfo(PROVIDER_NAME);

        private static readonly ProviderParameterInfo[] PARAMS = new ProviderParameterInfo[0];

        private IDictionary<string, object> _productParams;

        public ProviderInfo Describe() => INFO;

        public IEnumerable<ProviderParameterInfo> DescribeParameters() => PARAMS;

        public void SetParameters(IDictionary<string, object> productParams = null)
        {
            _productParams = productParams;
        }

        public IChecksumAlgorithm Produce()
        {
            return new Sha256ChecksumAlgorithm();
        }
    }
}
