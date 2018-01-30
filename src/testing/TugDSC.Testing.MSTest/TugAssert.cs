/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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
            try
            {
                action();
            }
            catch (Exception ex)
            {
                expected = ex;
            }

            Assert.ThrowsException<T>(() => expected != null
                    ? throw expected : 0, message, parameters);
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