// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tug.UnitTesting
{
    public static class TugAssert
    {
        public static void ThrowsExceptionWhen<T>(Func<T, bool> condition,
                Action action, string message = null)
            where T : Exception
        {
            ThrowsExceptionWhen<T>(condition, action, message, null);
        }

        public static void ThrowsExceptionWhen<T>(Func<T, bool> condition,
                Action action, string message, params object[] parameters)
            where T : Exception
        {
            Exception expected = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                expected = ex;
            }

            Assert.ThrowsException<T>(() =>
            {
                if (expected != null) throw expected;
            }, message, parameters);
            
            Assert.IsTrue(condition((T)expected), message, parameters);
        }
    }
}