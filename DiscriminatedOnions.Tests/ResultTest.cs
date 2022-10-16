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
using FluentAssertions;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class ResultTest
{
    private const string DummyResultValue = nameof(DummyResultValue);
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
}