/*
DiscriminatedOnions - A stinky but tasty hack to emulate F#-like discriminated unions in C#

Copyright 2022 Salvatore ISAJA. All rights reserved.

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
public static class OptionTest
{
    private const string GoodValue = nameof(GoodValue);
    private const string DummyValue = nameof(DummyValue);

    [Test]
    public static void Some() =>
        Option.Some(GoodValue)
            .Match(onNone: () => DummyValue, onSome: v => v)
            .Should().Be(GoodValue);

    [Test]
    public static void None() =>
        Option.None<string>()
            .Match(onNone: () => DummyValue, onSome: v => v)
            .Should().Be(DummyValue);

    [Test]
    public static void Bind_Some() =>
        Option.Some(GoodValue)
            .Bind(v => Option.Some(v + "Altered"))
            .Match(onNone: () => DummyValue, onSome: v => v)
            .Should().Be(GoodValue + "Altered");

    [Test]
    public static void Bind_None() =>
        Option.None<string>()
            .Bind(v => Option.Some(v + "Altered"))
            .Match(onNone: () => DummyValue, onSome: v => v)
            .Should().Be(DummyValue);

    [Test]
    public static void Contains_Some_Matching() => Option.Some("value").Contains("value").Should().BeTrue();

    [Test]
    public static void Contains_Some_NotMatching() => Option.Some("value").Contains("anotherValue").Should().BeFalse();

    [Test]
    public static void Contains_None() => Option.None<string>().Contains("value").Should().BeFalse();

    [Test]
    public static void DefaultValue_Some() => Option.Some("value").DefaultValue("defaultValue").Should().Be("value");

    [Test]
    public static void DefaultValue_None() => Option.None<string>().DefaultValue("defaultValue").Should().Be("defaultValue");

    [Test]
    public static void DefaultWith_Some() => Option.Some("value").DefaultWith(() => "defaultValue").Should().Be("value");

    [Test]
    public static void DefaultWith_None() => Option.None<string>().DefaultWith(() => "defaultValue").Should().Be("defaultValue");

    [Test]
    public static void Exists_Some_Matching() => Option.Some("value").Exists(v => v == "value").Should().BeTrue();

    [Test]
    public static void Exists_Some_NotMatching() => Option.Some("value").Exists(v => v == "anotherValue").Should().BeFalse();

    [Test]
    public static void Exists_None() => Option.None<string>().Exists(v => v == "value").Should().BeFalse();

    [Test]
    public static void Filter_Some_Matching() => Option.Some("value").Filter(v => v == "value").Should().Be(Option.Some("value"));

    [Test]
    public static void Filter_Some_NotMatching() => Option.Some("value").Filter(v => v == "anotherValue").Should().Be(Option.None<string>());

    [Test]
    public static void Filter_None() => Option.None<string>().Filter(v => v == "value").Should().Be(Option.None<string>());

    [Test]
    public static void Flatten_Some_Some() => Option.Some(Option.Some(GoodValue)).Flatten().Should().Be(Option.Some(GoodValue));

    [Test]
    public static void Flatten_Some_None() => Option.Some(Option.None<string>()).Flatten().Should().Be(Option.None<string>());

    [Test]
    public static void Flatten_None() => Option.None<Option<string>>().Flatten().Should().Be(Option.None<string>());

    [Test]
    public static void Get_Some() => Option.Some(GoodValue).Get().Should().Be(GoodValue);

    [Test]
    public static void Get_None() => ((Func<string>)(() => Option.None<string>().Get())).Should().Throw<InvalidOperationException>();

    [Test]
    public static void IsSome_Some() => Option.Some(GoodValue).IsSome().Should().BeTrue();

    [Test]
    public static void IsSome_None() => Option.None<string>().IsSome().Should().BeFalse();

    [Test]
    public static void IsNone_Some() => Option.Some(GoodValue).IsNone().Should().BeFalse();

    [Test]
    public static void IsNone_None() => Option.None<string>().IsNone().Should().BeTrue();

    [Test]
    public static void Iter_Some()
    {
        string? found = null;
        Option.Some(GoodValue).Iter(v => found = v);
        found.Should().Be(GoodValue);
    }

    [Test]
    public static void Iter_None()
    {
        string? found = null;
        Option.None<string>().Iter(v => found = v);
        found.Should().BeNull();
    }

    [Test]
    public static void Map_Some() =>
        Option.Some(GoodValue)
            .Map(v => v + "Altered")
            .Match(onNone: () => DummyValue, onSome: v => v)
            .Should().Be(GoodValue + "Altered");

    [Test]
    public static void Map_None() =>
        Option.None<string>()
            .Map(v => v + "Altered")
            .Match(onNone: () => DummyValue, onSome: v => v)
            .Should().Be(DummyValue);

    [Test]
    public static void OfNullable_NotNull() => Option.OfNullable((int?)42).Should().Be(Option.Some(42));

    [Test]
    public static void OfNullable_Null() => Option.OfNullable((int?)null).Should().Be(Option.None<int>());

    [Test]
    public static void OfObj_NotNull() => Option.OfObj(GoodValue).Should().Be(Option.Some(GoodValue));

    [Test]
    public static void OfObj_Null() => Option.OfObj((string?)null).Should().Be(Option.None<string>());

    [Test]
    public static void OrElse_Some() => Option.Some("value").OrElse(Option.Some("defaultValue")).Should().Be(Option.Some("value"));

    [Test]
    public static void OrElse_None() => Option.None<string>().OrElse(Option.Some("defaultValue")).Should().Be(Option.Some("defaultValue"));

    [Test]
    public static void OrElseWith_Some() => Option.Some("value").OrElseWith(() => Option.Some("defaultValue")).Should().Be(Option.Some("value"));

    [Test]
    public static void OrElseWith_None() => Option.None<string>().OrElseWith(() => Option.Some("defaultValue")).Should().Be(Option.Some("defaultValue"));

    [Test]
    public static void ToNullable_NotNull() => Option.Some(42).ToNullable().Should().Be(42);

    [Test]
    public static void ToNullable_Null() => Option.None<int>().ToNullable().Should().BeNull();

    [Test]
    public static void ToObj_NotNull() => Option.Some(GoodValue).ToObj().Should().Be(GoodValue);

    [Test]
    public static void ToObj_Null() => Option.None<string>().ToObj().Should().BeNull();
}