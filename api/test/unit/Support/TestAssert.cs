namespace Checknote.Api.UnitTests.Support;

using System;
using System.Collections.Generic;

internal static class TestAssert
{
    public static void Equal<T>(T expected, T actual, string description)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{description}: expected {expected}, got {actual}");
        }
    }

    public static void True(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException(description);
        }
    }

    public static void Same<T>(T expected, T actual, string description)
        where T : class
    {
        if (!ReferenceEquals(expected, actual))
        {
            throw new InvalidOperationException(description);
        }
    }
}
