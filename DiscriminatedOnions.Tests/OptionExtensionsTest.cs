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
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class OptionExtensionsTest
{
    private static readonly Dictionary<int, string> TheDictionary = new() { [42] = "The answer" };

    [Test]
    public static void Dictionary_TryGetValue_Present() => TheDictionary.TryGetValue(42).Should().Be(Option.Some("The answer"));

    [Test]
    public static void Dictionary_TryGetValue_Missing() => TheDictionary.TryGetValue(0).Should().Be(Option.None<string>());

    [Test]
    public static void ConcurrentDictionary_TryGetValue_Present() => new ConcurrentDictionary<int, string> { [42] = "The answer" }.TryGetValue(42).Should().Be(Option.Some("The answer"));

    [Test]
    public static void ConcurrentDictionary_TryGetValue_Missing() => new ConcurrentDictionary<int, string> { [42] = "The answer" }.TryGetValue(0).Should().Be(Option.None<string>());

    [Test]
    public static void IDictionary_TryGetValue_Present() => ((IDictionary<int, string>)TheDictionary).TryGetValue(42).Should().Be(Option.Some("The answer"));

    [Test]
    public static void IDictionary_TryGetValue_Missing() => ((IDictionary<int, string>)TheDictionary).TryGetValue(0).Should().Be(Option.None<string>());

    [Test]
    public static void IReadOnlyDictionary_TryGetValue_Present() => ((IReadOnlyDictionary<int, string>)TheDictionary).TryGetValue(42).Should().Be(Option.Some("The answer"));

    [Test]
    public static void IReadOnlyDictionary_TryGetValue_Missing() => ((IReadOnlyDictionary<int, string>)TheDictionary).TryGetValue(0).Should().Be(Option.None<string>());
}