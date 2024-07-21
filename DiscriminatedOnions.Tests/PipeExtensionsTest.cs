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
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace DiscriminatedOnions.Tests;

[TestFixture]
public static class PipeExtensionsTest
{
    private static int Double(int v) => v * 2;
    private static Task<int> DoubleAsync(int v) => Task.FromResult(Double(v));
    private static int StrDouble(string v) => Double(int.Parse(v));
    private static Task<int> StrDoubleAsync(string v) => Task.FromResult(StrDouble(v));
    private static bool IsOdd(int v) => v % 2 != 0;

    [Test]
    public static void Pipe_Action()
    {
        var list = new List<int>();
        21.Pipe(list.Add).Should().Be(Unit.Value);
        list.Should().BeEquivalentTo(new[] { 21 });
    }

    [Test]
    public static void Pipe_SyncSync() => "21".Pipe(StrDouble).Should().Be(42);

    [Test]
    public static async Task Pipe_SyncAsync() => (await "21".Pipe(StrDoubleAsync)).Should().Be(42);

    [Test]
    public static async Task Pipe_AsyncSync() => (await Task.FromResult("21").Pipe(StrDouble)).Should().Be(42);

    [Test]
    public static async Task Pipe_AsyncAsync() => (await Task.FromResult("21").Pipe(StrDoubleAsync)).Should().Be(42);

    [Test]
    public static void PipeIf_SyncSync_True() => 21.PipeIf(IsOdd, Double).Should().Be(42);

    [Test]
    public static void PipeIf_SyncSync_False() => 20.PipeIf(IsOdd, Double).Should().Be(20);

    [Test]
    public static async Task PipeIf_SyncAsync_True() => (await 21.PipeIf(IsOdd, DoubleAsync)).Should().Be(42);

    [Test]
    public static async Task PipeIf_SyncAsync_False() => (await 20.PipeIf(IsOdd, DoubleAsync)).Should().Be(20);

    [Test]
    public static async Task PipeIf_AsyncSync_True() => (await Task.FromResult(21).PipeIf(IsOdd, Double)).Should().Be(42);

    [Test]
    public static async Task PipeIf_AsyncSync_False() => (await Task.FromResult(20).PipeIf(IsOdd, Double)).Should().Be(20);

    [Test]
    public static async Task PipeIf_AsyncAsync_True() => (await Task.FromResult(21).PipeIf(IsOdd, DoubleAsync)).Should().Be(42);

    [Test]
    public static async Task PipeIf_AsyncAsync_False() => (await Task.FromResult(20).PipeIf(IsOdd, DoubleAsync)).Should().Be(20);
}