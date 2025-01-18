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
using NUnit.Framework;
using Shouldly;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class OptionEnumerableExtensionsTest
{
    [Test]
    public static void Choose() =>
        new[] { 1, 2, 3, 4, 5 }
            .Choose(i => i % 2 == 0 ? Option.Some(i) : Option.None<int>())
            .ShouldBe(new[] { 2, 4 });

    [Test]
    public static void Pick_Matching() =>
        new[] { 1, 2, 3, 4, 5 }
            .Pick(i => i % 2 == 0 ? Option.Some(i.ToString()) : Option.None<string>())
            .ShouldBe("2");

    [Test]
    public static void Pick_NotMatching() =>
        Should.Throw<KeyNotFoundException>(() => new[] { 1, 2, 3, 4, 5 }
            .Pick(i => i > 10 ? Option.Some(i.ToString()) : Option.None<string>()));

    [Test]
    public static void TryExactlyOne_None() =>
        Array.Empty<int>().TryExactlyOne().ShouldBe(Option.None<int>());

    [Test]
    public static void TryExactlyOne_One() =>
        new[] { 42 }.TryExactlyOne().ShouldBe(Option.Some(42));

    [Test]
    public static void TryExactlyOne_Two() =>
        new[] { 1, 2 }.TryExactlyOne().ShouldBe(Option.None<int>());

    [Test]
    public static void TryFind_Matching() =>
        new[] { 1, 2, 3, 4, 5 }.TryFind(i => i % 2 == 0).ShouldBe(Option.Some(2));

    [Test]
    public static void TryFind_NotMatching() =>
        new[] { 1, 2, 3, 4, 5 }.TryFind(i => i > 10).ShouldBe(Option.None<int>());

    [Test]
    public static void TryFindBack_Matching() =>
        new[] { 1, 2, 3, 4, 5 }.TryFindBack(i => i % 2 == 0).ShouldBe(Option.Some(4));

    [Test]
    public static void TryFindBack_NotMatching() =>
        new[] { 1, 5, 3 }.TryFindBack(i => i % 2 == 0).ShouldBe(Option.None<int>());

    [Test]
    public static void TryFindIndex_Matching() =>
        new[] { 1, 2, 3, 4, 5 }.TryFindIndex(i => i % 2 == 0).ShouldBe(Option.Some(1));

    [Test]
    public static void TryFindIndex_NotMatching() =>
        new[] { 1, 2, 3, 4, 5 }.TryFindIndex(i => i > 10).ShouldBe(Option.None<int>());

    [Test]
    public static void TryFindIndexBack_Matching() =>
        new[] { 1, 2, 3, 4, 5 }.TryFindIndexBack(i => i % 2 == 0).ShouldBe(Option.Some(3));

    [Test]
    public static void TryFindIndexBack_NotMatching() =>
        new[] { 1, 5, 3 }.TryFindIndexBack(i => i % 2 == 0).ShouldBe(Option.None<int>());

    [Test]
    public static void TryHead_Matching() =>
        new[] { "banana", "pear" }.TryHead().ShouldBe(Option.Some("banana"));

    [Test]
    public static void TryHead_NotMatching() =>
        Array.Empty<string>().TryHead().ShouldBe(Option.None<string>());

    [Test]
    public static void TryItem_Matching() =>
        new[] { "a", "b", "c", "d" }.TryItem(1).ShouldBe(Option.Some("b"));

    [Test]
    public static void TryItem_NotMatching() =>
        new[] { "a", "b", "c", "d" }.TryItem(5).ShouldBe(Option.None<string>());

    [Test]
    public static void TryLast_Matching() =>
        new[] { "pear", "banana" }.TryLast().ShouldBe(Option.Some("banana"));

    [Test]
    public static void TryLast_NotMatching() =>
        Array.Empty<string>().TryLast().ShouldBe(Option.None<string>());

    [Test]
    public static void TryPick_Matching() =>
        new[] { 1, 2, 3, 4, 5 }
            .TryPick(i => i % 2 == 0 ? Option.Some(i.ToString()) : Option.None<string>())
            .ShouldBe(Option.Some("2"));

    [Test]
    public static void TryPick_NotMatching() =>
        new[] { 1, 2, 3, 4, 5 }
            .TryPick(i => i > 10 ? Option.Some(i.ToString()) : Option.None<string>())
            .ShouldBe(Option.None<string>());

    [Test]
    public static void Unfold_Finite() =>
        1
            .Unfold(state => state > 100 ? Option.None<(string, int)>() : Option.Some((state.ToString(), state * 2)))
            .ShouldBe(new[] { "1", "2", "4", "8", "16", "32", "64" });

    [Test]
    public static void Unfold_Infinite() =>
        1
            .Unfold(state => Option.Some((state.ToString(), state * 2)))
            .Take(4)
            .ShouldBe(new[] { "1", "2", "4", "8" });
}