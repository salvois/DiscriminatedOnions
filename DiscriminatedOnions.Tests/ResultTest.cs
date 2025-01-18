﻿/*
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
using FluentAssertions;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class ResultTest
{
    private const string GoodResultValue = nameof(GoodResultValue);
    private const string DummyResultValue = nameof(DummyResultValue);
    private const int GoodErrorValue = 42;
    private const int DummyErrorValue = int.MinValue;

    [Test]
    public static void Ok() =>
        Result.Ok<string, int>("resultValue")
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((DummyErrorValue, "resultValue"));

    [Test]
    public static void Error() =>
        Result.Error<string, int>(42)
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((42, DummyResultValue));

    [Test]
    public static void Ok_Statement()
    {
        var actualResultValue = DummyResultValue;
        var actualErrorValue = DummyErrorValue;
        Result.Ok<string, int>("resultValue")
            .Match(
                onError: e => { actualErrorValue = e; },
                onOk: v => { actualResultValue = v; });
        actualResultValue.Should().Be("resultValue");
        actualErrorValue.Should().Be(DummyErrorValue);
    }

    [Test]
    public static void Error_Statement()
    {
        var actualResultValue = DummyResultValue;
        var actualErrorValue = DummyErrorValue;
        Result.Error<string, int>(42)
            .Match(
                onError: e => { actualErrorValue = e; },
                onOk: v => { actualResultValue = v; });
        actualResultValue.Should().Be(DummyResultValue);
        actualErrorValue.Should().Be(42);
    }

    [Test]
    public static void ToString_Ok() => Result.Ok<string, int>("resultValue").ToString().Should().Be("Ok(resultValue)");

    [Test]
    public static void ToString_Error() => Result.Error<string, int>(42).ToString().Should().Be("Error(42)");


    [Test]
    public static void Implicit_Ok() =>
        ((Result<string, int>)Result.Ok("resultValue"))
        .Match(
            onError: e => (e, DummyResultValue),
            onOk: v => (DummyErrorValue, v))
        .Should().Be((DummyErrorValue, "resultValue"));

    [Test]
    public static void Implicit_Error() =>
        ((Result<string, int>)Result.Error(42))
        .Match(
            onError: e => (e, DummyResultValue),
            onOk: v => (DummyErrorValue, v))
        .Should().Be((42, DummyResultValue));

    [Test]
    public static void Implicit_SameType_Ok() =>
        ((Result<string, string>)Result.Ok("resultValue"))
        .Match(
            onError: e => (e, "x"),
            onOk: v => ("x", v))
        .Should().Be(("x", "resultValue"));

    [Test]
    public static void Implicit_SameType_Error() =>
        ((Result<string, string>)Result.Error("errorValue"))
        .Match(
            onError: e => (e, "x"),
            onOk: v => ("x", v))
        .Should().Be(("errorValue", "x"));

    [Test]
    public static void Bind_Ok() =>
        Result.Ok<string, int>("resultValue")
            .Bind(v => Result.Ok<string, int>(v + "Altered"))
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((DummyErrorValue, "resultValueAltered"));

    [Test]
    public static void Bind_Error() =>
        Result.Error<string, int>(42)
            .Bind(v => Result.Ok<string, int>(v + "Altered"))
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((42, DummyResultValue));

    [Test]
    public static async Task BindAsync_Ok() =>
        (await Result.Ok<string, int>("resultValue")
            .BindAsync(v => Task.FromResult(Result.Ok<string, int>(v + "Altered")))
            .Pipe(r => r.Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))))
        .Should().Be((DummyErrorValue, "resultValueAltered"));

    [Test]
    public static async Task BindAsync_Error() =>
        (await Result.Error<string, int>(42)
            .BindAsync(v => Task.FromResult(Result.Ok<string, int>(v + "Altered")))
            .Pipe(r => r.Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))))
        .Should().Be((42, DummyResultValue));

    [Test]
    public static void Contains_Ok_Matching() => Result.Ok<string, int>("value").Contains("value").Should().BeTrue();

    [Test]
    public static void Contains_Ok_NotMatching() => Result.Ok<string, int>("value").Contains("anotherValue").Should().BeFalse();

    [Test]
    public static void Contains_Error() => Result.Error<string, int>(42).Contains("value").Should().BeFalse();

    [Test]
    public static void Count_Ok() => Result.Ok<string, int>(GoodResultValue).Count().Should().Be(1);

    [Test]
    public static void Count_Error() => Result.Error<string, int>(42).Count().Should().Be(0);

    [Test]
    public static void DefaultValue_Ok() => Result.Ok<string, int>("value").DefaultValue("defaultValue").Should().Be("value");

    [Test]
    public static void DefaultValue_Error() => Result.Error<string, int>(42).DefaultValue("defaultValue").Should().Be("defaultValue");

    [Test]
    public static void DefaultWith_Ok() => Result.Ok<string, int>("value").DefaultWith(e => $"defaultValue{e}").Should().Be("value");

    [Test]
    public static void DefaultWith_Error() => Result.Error<string, int>(42).DefaultWith(e => $"defaultValue{e}").Should().Be("defaultValue42");

    [Test]
    public static async Task DefaultWithAsync_Ok() => (await Result.Ok<string, int>("value").DefaultWithAsync(e => Task.FromResult($"defaultValue{e}"))).Should().Be("value");

    [Test]
    public static async Task DefaultWithAsync_Error() => (await Result.Error<string, int>(42).DefaultWithAsync(e => Task.FromResult($"defaultValue{e}"))).Should().Be("defaultValue42");

    [Test]
    public static void Exists_Ok_Matching() => Result.Ok<string, int>("value").Exists(v => v == "value").Should().BeTrue();

    [Test]
    public static void Exists_Ok_NotMatching() => Result.Ok<string, int>("value").Exists(v => v == "anotherValue").Should().BeFalse();

    [Test]
    public static void Exists_Error() => Result.Error<string, int>(42).Exists(v => v == "value").Should().BeFalse();

    [Test]
    public static void Get_Ok() => Result.Ok<string, int>(GoodResultValue).Get().Should().Be(GoodResultValue);

    [Test]
    public static void Get_Error() => ((Func<string>)(() => Result.Error<string, int>(GoodErrorValue).Get())).Should().Throw<InvalidOperationException>();

    [Test]
    public static void GetError_Ok() => ((Func<int>)(() => Result.Ok<string, int>(GoodResultValue).GetError())).Should().Throw<InvalidOperationException>();

    [Test]
    public static void GetError_Error() => Result.Error<string, int>(GoodErrorValue).GetError().Should().Be(GoodErrorValue);

    [Test]
    public static void Fold_Ok() => Result.Ok<int, string>(1).Fold(10, (acc, v) => acc + v * 2).Should().Be(12);

    [Test]
    public static void Fold_Error() => Result.Error<int, string>("error").Fold(10, (acc, v) => acc + v * 2).Should().Be(10);

    [Test]
    public static void ForAll_Ok_Matching() => Result.Ok<int, string>(42).ForAll(v => v >= 5).Should().BeTrue();

    [Test]
    public static void ForAll_Ok_NotMatching() => Result.Ok<int, string>(4).ForAll(v => v >= 5).Should().BeFalse();

    [Test]
    public static void ForAll_Error() => Result.Error<int, string>("error").ForAll(v => v >= 5).Should().BeTrue();

    [Test]
    public static void IsOk_Ok() => Result.Ok<string, int>(GoodResultValue).IsOk().Should().BeTrue();

    [Test]
    public static void IsOk_Error() => Result.Error<string, int>(GoodErrorValue).IsOk().Should().BeFalse();

    [Test]
    public static void IsError_Ok() => Result.Ok<string, int>(GoodResultValue).IsError().Should().BeFalse();

    [Test]
    public static void IsError_Error() => Result.Error<string, int>(GoodErrorValue).IsError().Should().BeTrue();

    [Test]
    public static void Iter_Ok()
    {
        string? found = null;
        Result.Ok<string, int>("resultValue").Iter(v => found = v);
        found.Should().Be("resultValue");
    }

    [Test]
    public static void Iter_Error()
    {
        string? found = null;
        Result.Error<string, int>(42).Iter(v => found = v);
        found.Should().BeNull();
    }

    [Test]
    public static async Task IterAsync_Ok()
    {
        string? found = null;
        await Result.Ok<string, int>("resultValue").IterAsync(v =>
        {
            found = v;
            return Task.CompletedTask;
        });
        found.Should().Be("resultValue");
    }

    [Test]
    public static async Task IterAsync_Error()
    {
        string? found = null;
        await Result.Error<string, int>(42).IterAsync(v =>
        {
            found = v;
            return Task.CompletedTask;
        });
        found.Should().BeNull();
    }

    [Test]
    public static void IterError_Ok()
    {
        int? found = null;
        Result.Ok<string, int>("resultValue").IterError(v => found = v);
        found.Should().BeNull();
    }

    [Test]
    public static void IterError_Error()
    {
        int? found = null;
        Result.Error<string, int>(42).IterError(v => found = v);
        found.Should().Be(42);
    }

    [Test]
    public static async Task IterErrorAsync_Ok()
    {
        int? found = null;
        await Result.Ok<string, int>("resultValue").IterErrorAsync(v =>
        {
            found = v;
            return Task.CompletedTask;
        });
        found.Should().BeNull();
    }

    [Test]
    public static async Task IterErrorAsync_Error()
    {
        int? found = null;
        await Result.Error<string, int>(42).IterErrorAsync(v =>
        {
            found = v;
            return Task.CompletedTask;
        });
        found.Should().Be(42);
    }

    [Test]
    public static void Map_Ok() =>
        Result.Ok<string, int>("resultValue")
            .Map(v => v + "Altered")
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((DummyErrorValue, "resultValueAltered"));

    [Test]
    public static void Map_Error() =>
        Result.Error<string, int>(42)
            .Map(v => v + "Altered")
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((42, DummyResultValue));

    [Test]
    public static async Task MapAsync_Ok() =>
        (await Result.Ok<string, int>("resultValue")
            .MapAsync(v => Task.FromResult(v + "Altered"))
            .Pipe(r => r.Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))))
        .Should().Be((DummyErrorValue, "resultValueAltered"));

    [Test]
    public static async Task MapAsync_Error() =>
        (await Result.Error<string, int>(42)
            .MapAsync(v => Task.FromResult(v + "Altered"))
            .Pipe(r => r.Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))))
        .Should().Be((42, DummyResultValue));

    [Test]
    public static void MapError_Ok() =>
        Result.Ok<string, int>("resultValue")
            .MapError(e => e + 1)
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((DummyErrorValue, "resultValue"));

    [Test]
    public static void MapError_Error() =>
        Result.Error<string, int>(42)
            .MapError(e => e + 1)
            .Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))
            .Should().Be((43, DummyResultValue));

    [Test]
    public static async Task MapErrorAsync_Ok() =>
        (await Result.Ok<string, int>("resultValue")
            .MapErrorAsync(e => Task.FromResult(e + 1))
            .Pipe(r => r.Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))))
        .Should().Be((DummyErrorValue, "resultValue"));

    [Test]
    public static async Task MapErrorAsync_Error() =>
        (await Result.Error<string, int>(42)
            .MapErrorAsync(e => Task.FromResult(e + 1))
            .Pipe(r => r.Match(
                onError: e => (e, DummyResultValue),
                onOk: v => (DummyErrorValue, v))))
        .Should().Be((43, DummyResultValue));

    [Test]
    public static void ToArray_Ok() => Result.Ok<int, string>(42).ToArray().Should().BeEquivalentTo(new[] { 42 });

    [Test]
    public static void ToArray_Error() => Result.Error<int, string>("error").ToArray().Should().BeEquivalentTo(Array.Empty<int>());

    [Test]
    public static void ToEnumerable_Ok() => Result.Ok<int, string>(42).ToEnumerable().Should().BeEquivalentTo(new[] { 42 });

    [Test]
    public static void ToEnumerable_Error() => Result.Error<int, string>("error").ToEnumerable().Should().BeEquivalentTo(Array.Empty<int>());

    [Test]
    public static void ToOption_Ok() => Result.Ok<string, int>(GoodResultValue).ToOption().Should().Be(Option.Some(GoodResultValue));

    [Test]
    public static void ToOption_Error() => Result.Error<string, int>(GoodErrorValue).ToOption().Should().Be(Option.None<string>());

    [Test]
    public static void ToOptionError_Ok() => Result.Ok<string, int>(GoodResultValue).ToOptionError().Should().Be(Option.None<int>());

    [Test]
    public static void ToOptionError_Error() => Result.Error<string, int>(GoodErrorValue).ToOptionError().Should().Be(Option.Some(GoodErrorValue));

    [Test]
    public static void TryGet_ValueType_Ok()
    {
        Result.Ok<int, string>(42).TryGet(out var value).Should().BeTrue();
        value.Should().Be(42);
    }

    [Test]
    public static void TryGet_ValueType_Error()
    {
        Result.Error<int, string>("error").TryGet(out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Test]
    public static void TryGet_ReferenceType_Ok()
    {
        Result.Ok<string, int>("value").TryGet(out var value).Should().BeTrue();
        value.Should().Be("value");
    }

    [Test]
    public static void TryGet_ReferenceType_Error()
    {
        Result.Error<string, int>(42).TryGet(out var value).Should().BeFalse();
        value.Should().BeNull();
    }
    [Test]
    public static void TryGetError_ValueType_Ok()
    {
        Result.Ok<string, int>("value").TryGetError(out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Test]
    public static void TryGetError_ValueType_Error()
    {
        Result.Error<string, int>(42).TryGetError(out var value).Should().BeTrue();
        value.Should().Be(42);
    }

    [Test]
    public static void TryGetError_ReferenceType_Ok()
    {
        Result.Ok<int, string>(42).TryGetError(out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Test]
    public static void TryGetError_ReferenceType_Error()
    {
        Result.Error<int, string>("error").TryGetError(out var value).Should().BeTrue();
        value.Should().Be("error");
    }
}