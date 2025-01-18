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
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

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
            .ShouldBe(GoodValue);

    [Test]
    public static void None() =>
        Option.None<string>()
            .Match(onNone: () => DummyValue, onSome: v => v)
            .ShouldBe(DummyValue);

    [Test]
    public static void Some_Statement()
    {
        var actual = DummyValue;
        Option.Some(GoodValue)
            .Match(
                onNone: () => { actual = DummyValue; },
                onSome: v => { actual = v; });
        actual.ShouldBe(GoodValue);
    }

    [Test]
    public static void None_Statement()
    {
        var actual = DummyValue;
        Option.None<string>()
            .Match(
                onNone: () => { actual = GoodValue; },
                onSome: v => { actual = v; });
        actual.ShouldBe(GoodValue);
    }

    [Test]
    public static void ToString_Some() => Option.Some(42).ToString().ShouldBe("Some(42)");

    [Test]
    public static void ToString_None() => Option.None<int>().ToString().ShouldBe("None");

    [Test]
    public static void Implicit_None() =>
        ((Option<string>)Option.Nothing)
            .Match(onNone: () => DummyValue, onSome: v => v)
            .ShouldBe(DummyValue);

    [Test]
    public static void Equality_Some() => Option.Some("value").ShouldBe(Option.Some("value"));

    [Test]
    public static void Equality_None() => Option.None<string>().ShouldBe(Option.None<string>());

    [Test]
    public static void Inequality_Some_None() => Option.Some("value").ShouldNotBe(Option.None<string>());

    [Test]
    public static void Inequality_Some() => Option.Some("value").ShouldNotBe(Option.Some("anotherValue"));

    [Test]
    public static void SystemTextJson_None() =>
        System.Text.Json.JsonSerializer.Serialize(Option.None<string>())
            .Pipe(s => System.Text.Json.JsonSerializer.Deserialize<Option<string>>(s))
            .ShouldBe(Option.None<string>());

    [Test]
    public static void SystemTextJson_Some() =>
        System.Text.Json.JsonSerializer.Serialize(Option.Some("value"))
            .Pipe(s => System.Text.Json.JsonSerializer.Deserialize<Option<string>>(s))
            .ShouldBe(Option.Some("value"));

    [Test]
    public static void Bind_Some() =>
        Option.Some(GoodValue)
            .Bind(v => Option.Some(v + "Altered"))
            .Bind(v => Option.Some(v + " two times"))
            .Match(onNone: () => DummyValue, onSome: v => v)
            .ShouldBe(GoodValue + "Altered two times");

    [Test]
    public static void Bind_None() =>
        Option.None<string>()
            .Bind(v => Option.Some(v + "Altered"))
            .Bind(v => Option.Some(v + " two times"))
            .Match(onNone: () => DummyValue, onSome: v => v)
            .ShouldBe(DummyValue);

    [Test]
    public static async Task BindAsync_Some() =>
        (await Option.Some(GoodValue)
            .BindAsync(v => Task.FromResult(Option.Some(v + "Altered")))
            .Pipe(o => o.BindAsync(v => Task.FromResult(Option.Some(v + " two times"))))
            .Pipe(o => o.Match(onNone: () => DummyValue, onSome: v => v)))
        .ShouldBe(GoodValue + "Altered two times");

    [Test]
    public static async Task BindAsync_None() =>
        (await Option.None<string>()
            .BindAsync(v => Task.FromResult(Option.Some(v + "Altered")))
            .Pipe(o => o.BindAsync(v => Task.FromResult(Option.Some(v + " two times"))))
            .Pipe(o => o.Match(onNone: () => DummyValue, onSome: v => v)))
        .ShouldBe(DummyValue);

    [Test]
    public static void Contains_Some_Matching() => Option.Some("value").Contains("value").ShouldBeTrue();

    [Test]
    public static void Contains_Some_NotMatching() => Option.Some("value").Contains("anotherValue").ShouldBeFalse();

    [Test]
    public static void Contains_None() => Option.None<string>().Contains("value").ShouldBeFalse();

    [Test]
    public static void Count_Some() => Option.Some(GoodValue).Count().ShouldBe(1);

    [Test]
    public static void Count_None() => Option.None<string>().Count().ShouldBe(0);

    [Test]
    public static void DefaultValue_Some() => Option.Some("value").DefaultValue("defaultValue").ShouldBe("value");

    [Test]
    public static void DefaultValue_None() => Option.None<string>().DefaultValue("defaultValue").ShouldBe("defaultValue");

    [Test]
    public static void DefaultWith_Some() => Option.Some("value").DefaultWith(() => "defaultValue").ShouldBe("value");

    [Test]
    public static void DefaultWith_None() => Option.None<string>().DefaultWith(() => "defaultValue").ShouldBe("defaultValue");

    [Test]
    public static async Task DefaultWithAsync_Some() => (await Option.Some("value").DefaultWithAsync(() => Task.FromResult("defaultValue"))).ShouldBe("value");

    [Test]
    public static async Task DefaultWithAsync_None() => (await Option.None<string>().DefaultWithAsync(() => Task.FromResult("defaultValue"))).ShouldBe("defaultValue");

    [Test]
    public static void Exists_Some_Matching() => Option.Some("value").Exists(v => v == "value").ShouldBeTrue();

    [Test]
    public static void Exists_Some_NotMatching() => Option.Some("value").Exists(v => v == "anotherValue").ShouldBeFalse();

    [Test]
    public static void Exists_None() => Option.None<string>().Exists(v => v == "value").ShouldBeFalse();

    [Test]
    public static void Filter_Some_Matching() => Option.Some("value").Filter(v => v == "value").ShouldBe(Option.Some("value"));

    [Test]
    public static void Filter_Some_NotMatching() => Option.Some("value").Filter(v => v == "anotherValue").ShouldBe(Option.None<string>());

    [Test]
    public static void Filter_None() => Option.None<string>().Filter(v => v == "value").ShouldBe(Option.None<string>());

    [Test]
    public static void Flatten_Some_Some() => Option.Some(Option.Some(GoodValue)).Flatten().ShouldBe(Option.Some(GoodValue));

    [Test]
    public static void Flatten_Some_None() => Option.Some(Option.None<string>()).Flatten().ShouldBe(Option.None<string>());

    [Test]
    public static void Flatten_None() => Option.None<Option<string>>().Flatten().ShouldBe(Option.None<string>());

    [Test]
    public static void Fold_None() => Option.None<int>().Fold(10, (acc, v) => acc + v * 2).ShouldBe(10);

    [Test]
    public static void ForAll_Some_Matching() => Option.Some(42).ForAll(v => v >= 5).ShouldBeTrue();

    [Test]
    public static void ForAll_Some_NotMatching() => Option.Some(4).ForAll(v => v >= 5).ShouldBeFalse();

    [Test]
    public static void ForAll_None() => Option.None<int>().ForAll(v => v >= 5).ShouldBeTrue();

    [Test]
    public static void Fold_Some() => Option.Some(1).Fold(10, (acc, v) => acc + v * 2).ShouldBe(12);

    [Test]
    public static void Get_Some() => Option.Some(GoodValue).Get().ShouldBe(GoodValue);

    [Test]
    public static void Get_None() => Should.Throw<InvalidOperationException>(() => Option.None<string>().Get());

    [Test]
    public static void IsSome_Some() => Option.Some(GoodValue).IsSome().ShouldBeTrue();

    [Test]
    public static void IsSome_None() => Option.None<string>().IsSome().ShouldBeFalse();

    [Test]
    public static void IsNone_Some() => Option.Some(GoodValue).IsNone().ShouldBeFalse();

    [Test]
    public static void IsNone_None() => Option.None<string>().IsNone().ShouldBeTrue();

    [Test]
    public static void Iter_Some()
    {
        string? found = null;
        Option.Some(GoodValue).Iter(v => found = v);
        found.ShouldBe(GoodValue);
    }

    [Test]
    public static void Iter_None()
    {
        string? found = null;
        Option.None<string>().Iter(v => found = v);
        found.ShouldBeNull();
    }

    [Test]
    public static async Task IterAsync_Some()
    {
        string? found = null;
        await Option.Some(GoodValue).IterAsync(v =>
        {
            found = v;
            return Task.CompletedTask;
        });
        found.ShouldBe(GoodValue);
    }

    [Test]
    public static async Task IterAsync_None()
    {
        string? found = null;
        await Option.None<string>().IterAsync(v =>
        {
            found = v;
            return Task.CompletedTask;
        });
        found.ShouldBeNull();
    }

    [Test]
    public static void Map_Some() =>
        Option.Some(GoodValue)
            .Map(v => v + "Altered")
            .Match(onNone: () => DummyValue, onSome: v => v)
            .ShouldBe(GoodValue + "Altered");

    [Test]
    public static void Map_None() =>
        Option.None<string>()
            .Map(v => v + "Altered")
            .Match(onNone: () => DummyValue, onSome: v => v)
            .ShouldBe(DummyValue);

    [Test]
    public static async Task MapAsync_Some() =>
        (await Option.Some(GoodValue)
            .MapAsync(v => Task.FromResult(v + "Altered"))
            .Pipe(o => o.Match(onNone: () => DummyValue, onSome: v => v)))
        .ShouldBe(GoodValue + "Altered");

    [Test]
    public static async Task MapAsync_None() =>
        (await Option.None<string>()
            .MapAsync(v => Task.FromResult(v + "Altered"))
            .Pipe(o => o.Match(onNone: () => DummyValue, onSome: v => v)))
        .ShouldBe(DummyValue);

    [Test]
    public static void Map2_Some_Some() =>
        (Option.Some(42), Option.Some(8)).Map2((v1, v2) => v1 + v2).ShouldBe(Option.Some(50));

    [Test]
    public static void Map2_Some_None() =>
        (Option.Some(42), Option.None<int>()).Map2((v1, v2) => v1 + v2).ShouldBe(Option.None<int>());

    [Test]
    public static void Map2_None_Some() =>
        (Option.None<int>(), Option.Some(42)).Map2((v1, v2) => v1 + v2).ShouldBe(Option.None<int>());

    [Test]
    public static void Map2_None_Nome() =>
        (Option.None<int>(), Option.None<int>()).Map2((v1, v2) => v1 + v2).ShouldBe(Option.None<int>());

    [Test]
    public static void Map3_Some_Some_Some() =>
        (Option.Some(42), Option.Some(8), Option.Some(50)).Map3((v1, v2, v3) => v1 + v2 + v3).ShouldBe(Option.Some(100));

    [Test]
    public static void Map3_Some_Some_None() =>
        (Option.Some(42), Option.Some(8), Option.None<int>()).Map3((v1, v2, v3) => v1 + v2 + v3).ShouldBe(Option.None<int>());

    [Test]
    public static void OfNullable_NotNull() => Option.OfNullable((int?)42).ShouldBe(Option.Some(42));

    [Test]
    public static void OfNullable_Null() => Option.OfNullable((int?)null).ShouldBe(Option.None<int>());

    [Test]
    public static void OfObj_NotNull() => Option.OfObj(GoodValue).ShouldBe(Option.Some(GoodValue));

    [Test]
    public static void OfObj_Null() => Option.OfObj((string?)null).ShouldBe(Option.None<string>());

    [Test]
    public static void OrElse_Some() => Option.Some("value").OrElse(Option.Some("defaultValue")).ShouldBe(Option.Some("value"));

    [Test]
    public static void OrElse_None() => Option.None<string>().OrElse(Option.Some("defaultValue")).ShouldBe(Option.Some("defaultValue"));

    [Test]
    public static void OrElseWith_Some() => Option.Some("value").OrElseWith(() => Option.Some("defaultValue")).ShouldBe(Option.Some("value"));

    [Test]
    public static void OrElseWith_None() => Option.None<string>().OrElseWith(() => Option.Some("defaultValue")).ShouldBe(Option.Some("defaultValue"));

    [Test]
    public static async Task OrElseWithAsync_Some() => (await Option.Some("value").OrElseWithAsync(() => Task.FromResult(Option.Some("defaultValue")))).ShouldBe(Option.Some("value"));

    [Test]
    public static async Task OrElseWithAsync_None() => (await Option.None<string>().OrElseWithAsync(() => Task.FromResult(Option.Some("defaultValue")))).ShouldBe(Option.Some("defaultValue"));

    [Test]
    public static void ToNullable_NotNull() => Option.Some(42).ToNullable().ShouldBe(42);

    [Test]
    public static void ToNullable_Null() => Option.None<int>().ToNullable().ShouldBeNull();

    [Test]
    public static void ToArray_Some() => Option.Some(42).ToArray().ShouldBe(new[] { 42 });

    [Test]
    public static void ToArray_None() => Option.None<int>().ToArray().ShouldBe(Array.Empty<int>());

    [Test]
    public static void ToEnumerable_Some() => Option.Some(42).ToEnumerable().ShouldBe(new[] { 42 });

    [Test]
    public static void ToEnumerable_None() => Option.None<int>().ToEnumerable().ShouldBe(Array.Empty<int>());

    [Test]
    public static void ToObj_NotNull() => Option.Some(GoodValue).ToObj().ShouldBe(GoodValue);

    [Test]
    public static void ToObj_Null() => Option.None<string>().ToObj().ShouldBeNull();

    [Test]
    public static void ToOption_ValueType_NotNull() => ((int?)42).ToOption().ShouldBe(Option.Some(42));

    [Test]
    public static void ToOption_ValueType_Null() => ((int?)null).ToOption().ShouldBe(Option.None<int>());

    [Test]
    public static void ToOption_ReferenceType_NotNull() => GoodValue.ToOption().ShouldBe(Option.Some(GoodValue));

    [Test]
    public static void ToOption_ReferenceType_Null() => ((string?)null).ToOption().ShouldBe(Option.None<string>());

    [Test]
    public static void TryGet_ValueType_Some()
    {
        Option.Some(42).TryGet(out var value).ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Test]
    public static void TryGet_ValueType_None()
    {
        Option.None<int>().TryGet(out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Test]
    public static void TryGet_ReferenceType_Some()
    {
        Option.Some("value").TryGet(out var value).ShouldBeTrue();
        value.ShouldBe("value");
    }

    [Test]
    public static void TryGet_ReferenceType_None()
    {
        Option.None<string>().TryGet(out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }
}