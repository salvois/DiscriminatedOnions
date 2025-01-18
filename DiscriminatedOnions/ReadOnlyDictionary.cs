/*
DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022-2025 Salvatore ISAJA. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED THE COPYRIGHT HOLDER ``AS IS'' AND ANY EXPRESS
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY DIRECT,
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscriminatedOnions;

/// Helpers to work with IReadOnlyDictionary
public static class ReadOnlyDictionary
{
    private static class EmptyDictionary<TKey, TValue> where TKey : notnull
    {
        public static readonly IReadOnlyDictionary<TKey, TValue> Value = new Dictionary<TKey, TValue>();
    }

    /// Like Enumerable.ToDictionary() but casts the result to IReadOnlyDictionary
    public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<T, TKey, TValue>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull =>
        source.ToDictionary(keySelector, valueSelector);

    /// Like Enumerable.ToDictionary() but casts the result to IReadOnlyDictionary
    public static IReadOnlyDictionary<TKey, T> ToReadOnlyDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) where TKey : notnull =>
        source.ToDictionary(keySelector);

    /// Returns a singleton empty IReadOnlyDictionary
    public static IReadOnlyDictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull =>
        EmptyDictionary<TKey, TValue>.Value;
}