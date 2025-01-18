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
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class ReadMeExamples
{
    private abstract record Shape
    {
        public record Rectangle(double Width, double Height) : Shape;
        public record Circle(double Radius) : Shape;

        public U Match<U>(Func<Rectangle, U> onRectangle, Func<Circle, U> onCircle) =>
            this switch
            {
                Rectangle r => onRectangle(r),
                Circle c => onCircle(c),
                _ => throw new ArgumentOutOfRangeException()
            };

        private Shape() { }
    }

    private static double ComputeAreaUsingSwitch(this Shape shape) =>
        shape switch
        {
            Shape.Rectangle(var width, var height) => width * height,
            Shape.Circle(var radius) => radius * radius * Math.PI,
            _ => throw new ArgumentOutOfRangeException() // This is one point where this approach is inferior to real discriminated unions
        };

    private static double ComputeAreaUsingMatch(this Shape shape) =>
        shape.Match(
            onRectangle: r => r.Width * r.Height,
            onCircle: c => c.Radius * c.Radius * Math.PI);

    [Test]
    public static void SwitchRectangle() => new Shape.Rectangle(10.0, 1.3).ComputeAreaUsingSwitch().ShouldBe(10.0 * 1.3);

    [Test]
    public static void SwitchCircle() => new Shape.Circle(1.0).ComputeAreaUsingSwitch().ShouldBe(1.0 * 1.0 * Math.PI);

    [Test]
    public static void MatchRectangle() => new Shape.Rectangle(10.0, 1.3).ComputeAreaUsingMatch().ShouldBe(10.0 * 1.3);

    [Test]
    public static void MatchCircle() => new Shape.Circle(1.0).ComputeAreaUsingMatch().ShouldBe(1.0 * 1.0 * Math.PI);

    [Test]
    public static void Option_Map_Some()
    {
        Option<string> someString = Option.Some("I have a value");
        Option<string> mapped = someString.Map(v => v + " today");
        mapped.ShouldBe(Option.Some("I have a value today"));
    }

    [Test]
    public static void Option_DefaultValue()
    {
        Option<string> noString = Option.None<string>();
        string defaulted = noString.DefaultValue("default value");
        defaulted.ShouldBe("default value");
    }

    [Test]
    public static void Option_Nothing_DefaultValue()
    {
        Option<string> noString = Option.Nothing;
        string defaulted = noString.DefaultValue("default value");
        defaulted.ShouldBe("default value");
    }

    [Test]
    public static async Task Option_BindAsync_Some()
    {
        Option<string> someString = Option.Some("I have a value");
        Option<string> asyncBound = await someString
            .BindAsync(v => Task.FromResult(Option.Some(v + " altered")))
            .Pipe(o => o.BindAsync(v => Task.FromResult(Option.Some(v + " two times"))));
        asyncBound.ShouldBe(Option.Some("I have a value altered two times"));
    }

    [Test]
    public static void Option_Choose()
    {
        IEnumerable<int> chosen = new[] { 1, 2, 3, 4, 5 }.Choose(
            i => i % 2 == 0
                ? Option.Some(i)
                : Option.None<int>());
        chosen.ShouldBe(new[] { 2, 4 });
    }

    [Test]
    public static void Option_TryFind()
    {
        Option<int> found = new[] { 1, 2, 3, 4, 5 }.TryFind(i => i % 2 == 0);
        found.ShouldBe(Option.Some(2));
    }

    [Test]
    public static void OptionExtension_TryGetValue()
    {
        var dict = new Dictionary<int, string> { [42] = "The answer" };
        Option<string> value = dict.TryGetValue(42);
        value.ShouldBe(Option.Some("The answer"));
    }

    [Test]
    public static void Result_Bind_Ok()
    {
        Result<string, int> ok = Result.Ok<string, int>("result value");
        Result<string, int> boundOk = ok.Bind(v => Result.Ok<string, int>("beautiful " + v));
        boundOk.ShouldBe(Result.Ok<string, int>("beautiful result value"));
    }

    [Test]
    public static void Result_Bind_Error()
    {
        Result<string, int> error = Result.Error<string, int>(42);
        Result<string, int> boundError = error.Bind(v => Result.Ok<string, int>("beautiful " + v));
        boundError.ShouldBe(Result.Error<string, int>(42));
    }

    [Test]
    public static void Result_Implicit_Bind_Ok()
    {
        Result<string, int> ok = Result.Ok("result value");
        Result<string, int> boundOk = ok.Bind(v => Result.Ok<string, int>("beautiful " + v));
        boundOk.ShouldBe(Result.Ok<string, int>("beautiful result value"));
    }

    [Test]
    public static void Result_Implicit_Bind_Error()
    {
        Result<string, int> error = Result.Error(42);
        Result<string, int> boundError = error.Bind(v => Result.Ok<string, int>("beautiful " + v));
        boundError.ShouldBe(Result.Error<string, int>(42));
    }

    [Test]
    public static async Task Result_BindAsync_Ok()
    {
        Result<string, int> ok = Result.Ok<string, int>("result value");
        Result<string, int> asyncBoundOk = await ok
            .BindAsync(v => Task.FromResult(Result.Ok<string, int>("beautiful " + v)))
            .Pipe(o => o.BindAsync(v => Task.FromResult(Result.Ok<string, int>("very " + v))));
        asyncBoundOk.ShouldBe(Result.Ok<string, int>("very beautiful result value"));
    }

    [Test]
    public static void Pipe()
    {
        const int twenty = 20;
        int piped = twenty.Pipe(v => v + 1).Pipe(v => v * 2);
        piped.ShouldBe(42);
    }

    [Test]
    public static void PipeIf()
    {
        const int twenty = 20;
        int maybePiped = twenty.PipeIf(v => v < 10, v => v * 2);
        maybePiped.ShouldBe(20);
    }

    [Test]
    public static void ReturningUnit()
    {
        new Shape.Circle(1.0).Match(
                onRectangle: r =>
                {
                    Console.WriteLine($"Rectangle with width {r.Width} and height {r.Height}.");
                    return Unit.Value; // unfortunately you have to return it explicitly
                },
                onCircle: c =>
                {
                    Console.WriteLine($"Circle with radius {c.Radius}.");
                    return Unit.Value;
                })
            .ShouldBe(Unit.Value);
    }

    [Test]
    public static void Unit_Call()
    {
        new Shape.Circle(1.0).Match(
                onRectangle: r => Unit.Call(() => Console.WriteLine($"Rectangle with width {r.Width} and height {r.Height}.")),
                onCircle: c => Unit.Call(() => Console.WriteLine($"Circle with radius {c.Radius}.")))
            .ShouldBe(Unit.Value);
    }
}