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

namespace DiscriminatedOnions;

public readonly record struct Result<T, TError>
{
    public bool IsOk { get; private init; }
    public T ResultValue { get; private init; }
    public TError ErrorValue { get; private init; }

    public U Match<U>(Func<TError, U> onError, Func<T, U> onOk) =>
        this switch
        {
            { IsOk: false, ErrorValue: var v } => onError(v),
            { IsOk: true, ResultValue: var v } => onOk(v)
        };

    internal Result(bool isOk, T value, TError errorValue)
    {
        IsOk = isOk;
        ResultValue = value;
        ErrorValue = errorValue;
    }
}

public static class Result
{
    public static Result<T, TError> Ok<T, TError>(T resultValue) =>
        new(true, resultValue, default!);

    public static Result<T, TError> Error<T, TError>(TError errorValue) =>
        new(false, default!, errorValue);

    public static Result<U, TError> Bind<T, TError, U>(this Result<T, TError> result, Func<T, Result<U, TError>> binder) =>
        result.Match(Error<U, TError>, binder);

    public static Result<U, TError> Map<T, TError, U>(this Result<T, TError> result, Func<T, U> mapping) =>
        result.Match(Error<U, TError>, v => Ok<U, TError>(mapping(v)));

    public static Result<T, U> MapError<T, TError, U>(this Result<T, TError> result, Func<TError, U> mapping) =>
        result.Match(e => Error<T, U>(mapping(e)), Ok<T, U>);
}