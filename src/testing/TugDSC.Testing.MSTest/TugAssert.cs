// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TugDSC.Testing.MSTest
{
    public static class TugAssert
    {
        public static void ThrowsAny(this Assert assert, Action action, string message = null,
                params object[] parameters)
        {
            if (message == null)
                message = string.Empty;
            message = string.Format(message, parameters);

            try
            {
                action();
                message = string.Format("Any exception was expected but not thrown. {0}", message);
                throw new AssertFailedException(message);
            }
            catch (Exception)
            { }
        }

        public static void ThrowsAny<T>(this Assert assert, Action action, string message = null,
                params object[] parameters) where T : Exception
        {
            if (message == null)
                message = string.Empty;
            message = string.Format(message, parameters);

            try
            {
                action();
                message = string.Format("Any exception was expected but not thrown. {0}", message);
                throw new AssertFailedException(message);
            }
            catch (Exception ex)
            {
                if (ex as T == null)
                {
                    message = string.Format(
                            "An exception assignable to {0} was expected, but caught {1}. {2}",
                            typeof(T).Name, ex.GetType().Name, message);
                    throw new AssertFailedException(message);
                }
            }
        }

        public static void ThrowsWhen<T>(this Assert assert, Func<T, bool> condition, Action action,
                string message = null, params object[] parameters) where T : Exception
        {
            Exception expected = null;
            Assert.ThrowsException<T>(() => {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    expected = ex;
                    throw;
                }
            }, message, parameters);

            Assert.IsTrue(condition((T)expected), message, parameters);
        }

        public static void ThrowsAnyWhen<T>(this Assert assert, Func<T, bool> condition,
                Action action, string message = null, params object[] parameters) where T : Exception
        {
            if (message == null)
                message = string.Empty;
            message = string.Format(message, parameters);

            Exception expected = null;
            try
            {
                action();
                message = string.Format("Any exception was expected but not thrown. {0}", message);
                throw new AssertFailedException(message);
            }
            catch (Exception ex)
            {
                if (ex as T == null)
                {
                    message = string.Format(
                            "An exception assignable to {0} was expected, but caught {1}. {2}",
                            typeof(T).Name, ex.GetType().Name, message);
                    throw new AssertFailedException(message);
                }
                expected = ex;
            }

            Assert.IsTrue(condition((T)expected), message, parameters);
        }
    }
}