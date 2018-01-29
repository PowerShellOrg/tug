/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TugDSC.Ext;

namespace TugDSC.Providers
{
    public class Sha256ChecksumAlgorithm : IChecksumAlgorithm
    {
        private SHA256 _sha256;

        public Sha256ChecksumAlgorithm()
        {
            _sha256 = SHA256.Create();
        }

        public string AlgorithmName
        { get; } = Sha256ChecksumAlgorithmProvider.PROVIDER_NAME;

        public bool IsDisposed
        { get; private set; }

        public string ComputeChecksum(Stream stream)
        {
            return Escape(_sha256.ComputeHash(stream));
        }

        public string ComputeChecksum(byte[] bytes)
        {
            return Escape(_sha256.ComputeHash(bytes));
        }

        public static string Escape(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        #region -- IDisposable Support --

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _sha256.Dispose();
                    _sha256 = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Sha256ChecksumAlgorithm() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion -- IDisposable Support --

    }
}
