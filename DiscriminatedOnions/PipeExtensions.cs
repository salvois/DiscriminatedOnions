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
using System.Threading.Tasks;

namespace DiscriminatedOnions
{
    public static class PipeExtensions
    {
        public static TOut Pipe<TIn, TOut>(this TIn previous, Func<TIn, TOut> next) =>
            next(previous);

        public static async Task<TOut> Pipe<TIn, TOut>(this TIn previous, Func<TIn, Task<TOut>> next) =>
            await next(previous);

        public static async Task<TOut> Pipe<TIn, TOut>(this Task<TIn> previous, Func<TIn, TOut> next) =>
            next(await previous);

        public static async Task<TOut> Pipe<TIn, TOut>(this Task<TIn> previous, Func<TIn, Task<TOut>> next) =>
            await next(await previous);

        public static T PipeIf<T>(this T previous, Func<T, bool> predicate, Func<T, T> next) =>
            predicate(previous) ? next(previous) : previous;

        public static async Task<T> PipeIf<T>(this T previous, Func<T, bool> predicate, Func<T, Task<T>> next) =>
            predicate(previous) ? await next(previous) : previous;

        public static async Task<T> PipeIf<T>(this Task<T> previous, Func<T, bool> predicate, Func<T, T> next) =>
            PipeIf(await previous, predicate, next);

        public static async Task<T> PipeIf<T>(this Task<T> previous, Func<T, bool> predicate, Func<T, Task<T>> next) =>
            await PipeIf(await previous, predicate, next);
    }
}