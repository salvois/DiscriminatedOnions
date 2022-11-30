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

using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class ReadMeExamples
{
    [Test]
    public static void Option_Map_Some()
    {
        Option<string> someString = Option.Some("I have a value");
        Option<string> mapped = someString.Map(v => v + " today");
        mapped.Should().Be(Option.Some("I have a value today"));
    }

    [Test]
    public static void Option_DefaultValue()
    {
        Option<string> noString = Option.None<string>();
        string defaulted = noString.DefaultValue("default value");
        defaulted.Should().Be("default value");
    }

    [Test]
    public static void Option_Choose()
    {
        IEnumerable<int> chosen = new[] { 1, 2, 3, 4, 5 }.Choose(
            i => i % 2 == 0
                ? Option.Some(i)
                : Option.None<int>());
        chosen.Should().BeEquivalentTo(new[] { 2, 4 });
    }

    [Test]
    public static void Option_TryFind()
    {
        Option<int> found = new[] { 1, 2, 3, 4, 5 }.TryFind(i => i % 2 == 0);
        found.Should().Be(Option.Some(2));
    }

    [Test]
    public static void OptionExtension_TryGetValue()
    {
        var dict = new Dictionary<int, string> { [42] = "The answer" };
        Option<string> value = dict.TryGetValue(42);
        value.Should().Be(Option.Some("The answer"));
    }

    [Test]
    public static void Result_Bind_Ok()
    {
        Result<string, int> ok = Result.Ok<string, int>("result value");
        Result<string, int> boundOk = ok.Bind(v => Result.Ok<string, int>("beautiful " + v));
        boundOk.Should().Be(Result.Ok<string, int>("beautiful result value"));
    }

    [Test]
    public static void Result_Bind_Error()
    {
        Result<string, int> error = Result.Error<string, int>(42);
        Result<string, int> boundError = error.Bind(v => Result.Ok<string, int>("beautiful " + v));
        boundError.Should().Be(Result.Error<string, int>(42));
    }

    [Test]
    public static void Pipe()
    {
        const int twenty = 20;
        int piped = twenty.Pipe(v => v + 1).Pipe(v => v * 2);
        piped.Should().Be(42);
    }

    [Test]
    public static void PipeIf()
    {
        const int twenty = 20;
        int maybePiped = twenty.PipeIf(v => v < 10, v => v * 2);
        maybePiped.Should().Be(20);
    }
}