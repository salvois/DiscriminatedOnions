
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
using System;
using FluentAssertions;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class NonEmptyListTest
{
    [Test]
    public static void TryCreateNonEmptyList_Empty() =>
        Array.Empty<int>().TryCreateNonEmptyList()
            .Should().Be(Option.None<INonEmptyList<int>>());

    [Test]
    public static void TryCreateNonEmptyList_NonEmpty() =>
        new[] { 1, 2 }.TryCreateNonEmptyList()
            .Should().BeAssignableTo<Option<INonEmptyList<int>>>().And.BeEquivalentTo(Option.Some(new[] { 1, 2 }), o => o.WithStrictOrdering());

    [Test]
    public static void Count() =>
        NonEmptyEnumerable.Of(1, 2, 3).ToNonEmptyList().Count
            .Should().Be(3);

    [TestCase(0, 'a')]
    [TestCase(1, 'b')]
    [TestCase(2, 'c')]
    public static void Index(int index, char expected) =>
        NonEmptyEnumerable.Of('a', 'b', 'c').ToNonEmptyList()[index]
            .Should().Be(expected);

    [Test]
    public static void Index_OutOfBound()
    {
        var act = () => NonEmptyEnumerable.Of('a', 'b', 'c').ToNonEmptyList()[4];
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}