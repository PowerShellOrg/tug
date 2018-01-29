using System;

namespace TugDSC.Util
{
    public static class ExceptionExtensions
    {
        /// This extension method provides a fluent method of appending various
        /// meta data to an exception, useful when debugging and trying to
        /// isolate more contextual information from a thrown exception.
        public static T WithData<T>(this T exception, object key, object value)
            where T : Exception
        {
            exception.Data.Add(key, value);
            return exception;
        }
    }
}