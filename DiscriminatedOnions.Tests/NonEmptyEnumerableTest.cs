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
using NUnit.Framework;
using Shouldly;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class NonEmptyEnumerableTest
{
    [Test]
    public static void Of_Single() =>
        NonEmptyEnumerable.Of(1)
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyEnumerable<int>>(),
                e => e.ShouldBe(new[] { 1 }));

    [Test]
    public static void Of_Multiple() =>
        NonEmptyEnumerable.Of(1, 2, 3)
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyEnumerable<int>>(),
                e => e.ShouldBe(new[] { 1, 2, 3 }));

    [Test]
    public static void Append() =>
        NonEmptyEnumerable.Of(1, 2).Append(3)
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyEnumerable<int>>(),
                e => e.ShouldBe(new[] { 1, 2, 3 }));

    [Test]
    public static void Concat_NonEmptyToEnumerable() =>
        NonEmptyEnumerable.Of(1, 2).Concat(new[] { 3, 4 })
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyEnumerable<int>>(),
                e => e.ShouldBe(new[] { 1, 2, 3, 4 }));

    [Test]
    public static void Concat_EnumerableToNonEmpty() =>
        new[] { 1, 2 }.Concat(NonEmptyEnumerable.Of(3, 4))
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyEnumerable<int>>(),
                e => e.ShouldBe(new[] { 1, 2, 3, 4 }));

    [Test]
    public static void Select() =>
        NonEmptyEnumerable.Of(1, 2).Select(e => (e + 1).ToString())
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyEnumerable<string>>(),
                e => e.ShouldBe(new[] { "2", "3" }));

    [Test]
    public static void ToNonEmptyCollection() =>
        NonEmptyEnumerable.Of(1, 2).ToNonEmptyCollection()
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyCollection<int>>(),
                e => e.ShouldBe(new[] { 1, 2 }));

    [Test]
    public static void ToNonEmptyList() =>
        NonEmptyEnumerable.Of(1, 2).ToNonEmptyList()
            .ShouldSatisfyAllConditions(
                e => e.ShouldBeAssignableTo<INonEmptyList<int>>(),
                e => e.ShouldBe(new[] { 1, 2 }));
}