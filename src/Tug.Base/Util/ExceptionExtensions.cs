using System;

namespace Tug.Util
{
    public static class ExceptionExtensions
    {
        public static T WithData<T>(this T exception, object key, object value)
            where T : Exception
        {
            exception.Data.Add(key, value);
            return exception;
        }
    }
}