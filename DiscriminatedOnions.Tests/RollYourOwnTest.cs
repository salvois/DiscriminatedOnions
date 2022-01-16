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
public static class RollYourOwnTest
{
    private abstract record Shape
    {
        public record Rectangle(double Width, double Height) : Shape;
        public record Circle(double Radius) : Shape;

        public U Match<U>(Func<double, double, U> onRectangle, Func<double, U> onCircle) =>
            this switch
            {
                Rectangle(var width, var height) => onRectangle(width, height),
                Circle(var radius) => onCircle(radius),
                _ => throw new ArgumentOutOfRangeException()
            };
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
            onRectangle: (w, h) => w * h,
            onCircle: r => r * r * Math.PI);

    [Test]
    public static void SwitchRectangle() => new Shape.Rectangle(10.0, 1.3).ComputeAreaUsingSwitch().Should().Be(10.0 * 1.3);

    [Test]
    public static void SwitchCircle() => new Shape.Circle(1.0).ComputeAreaUsingSwitch().Should().Be(1.0 * 1.0 * Math.PI);

    [Test]
    public static void MatchRectangle() => new Shape.Rectangle(10.0, 1.3).ComputeAreaUsingMatch().Should().Be(10.0 * 1.3);

    [Test]
    public static void MatchCircle() => new Shape.Circle(1.0).ComputeAreaUsingMatch().Should().Be(1.0 * 1.0 * Math.PI);
}