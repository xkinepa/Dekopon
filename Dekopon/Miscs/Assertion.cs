using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Dekopon.Miscs
{
    public static class Assertion
    {
        public static void IsPositive(int data, string message = null)
        {
            IsPositive(Convert.ToInt64(data), message);
        }

        public static void IsPositive(long data, string message = null)
        {
            if (data <= 0)
            {
                throw new AssertionException(message);
            }
        }

        public static void IsZero(int data, string message = null)
        {
            IsZero(Convert.ToInt64(data), message);
        }

        public static void IsZero(long data, string message = null)
        {
            if (data != 0)
            {
                throw new AssertionException(message);
            }
        }

        [ContractAnnotation("expression: false => halt")]
        public static void IsTrue(bool expression, string message = null)
        {
            if (!expression)
            {
                throw new AssertionException(message);
            }
        }

        [ContractAnnotation("obj: notnull => halt")]
        public static void IsNull(object obj, string message = null)
        {
            if (obj != null)
            {
                throw new AssertionException(message);
            }
        }

        [ContractAnnotation("obj: null => halt")]
        public static void NotNull(object obj, string message = null)
        {
            if (obj == null)
            {
                throw new AssertionException(message);
            }
        }

        //[ContractAnnotation("text: null => halt")]
        public static void HasLength(string text, string message = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new AssertionException(message);
            }
        }

        //[ContractAnnotation("text: null => halt")]
        public static void HasText(string text, string message = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new AssertionException(message);
            }
        }

        public static void All<T>(IEnumerable<T> collection, Func<T, bool> predicate, Func<T, string> messageFactory = null)
        {
            foreach (var it in collection)
            {
                IsTrue(predicate(it), messageFactory?.Invoke(it));
            }
        }

        [ContractAnnotation("=> halt")]
        public static Exception Fail(string message = null)
        {
            throw new AssertionException(message);
        }
    }

    [Serializable]
    public class AssertionException : Exception
    {
        public AssertionException()
            : base()
        {
        }

        public AssertionException(string message)
            : base(message)
        {
        }

        public AssertionException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        public AssertionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AssertionException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }

        protected AssertionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class WillNeverReachHereException : Exception
    {
    }
}