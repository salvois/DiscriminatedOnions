/*
DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022-2024 Salvatore ISAJA. All rights reserved.

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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DiscriminatedOnions;

/// Utility functions augmenting common functionality with the Option type
public static class OptionExtensions
{
    /// Returns Some(v) if dict contains key or None if it doesn't
    public static Option<TValue> TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull =>
        ((IReadOnlyDictionary<TKey, TValue>)dict).TryGetValue(key);

    /// Returns Some(v) if dict contains key or None if it doesn't
    public static Option<TValue> TryGetValue<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : notnull =>
        ((IReadOnlyDictionary<TKey, TValue>)dict).TryGetValue(key);

    /// Returns Some(v) if dict contains key or None if it doesn't
    public static Option<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) =>
        dict.TryGetValue(key, out var value) ? Option.Some(value) : Option.None<TValue>();

    /// Returns Some(v) if dict contains key or None if it doesn't
    public static Option<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key) =>
        dict.TryGetValue(key, out var value) ? Option.Some(value) : Option.None<TValue>();
}