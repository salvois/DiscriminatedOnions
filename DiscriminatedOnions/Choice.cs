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

public abstract record Choice<T1, T2>
{
    public record Choice1(T1 Item) : Choice<T1, T2>;
    public record Choice2(T2 Item) : Choice<T1, T2>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2) =>
        this switch
        {
            Choice1 c => onChoice1(c.Item),
            Choice2 c => onChoice2(c.Item),
            _ => throw new ArgumentOutOfRangeException()
        };
}

public abstract record Choice<T1, T2, T3>
{
    public record Choice1(T1 Item) : Choice<T1, T2, T3>;
    public record Choice2(T2 Item) : Choice<T1, T2, T3>;
    public record Choice3(T3 Item) : Choice<T1, T2, T3>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2, Func<T3, U> onChoice3) =>
        this switch
        {
            Choice1 c => onChoice1(c.Item),
            Choice2 c => onChoice2(c.Item),
            Choice3 c => onChoice3(c.Item),
            _ => throw new ArgumentOutOfRangeException()
        };
}

public abstract record Choice<T1, T2, T3, T4>
{
    public record Choice1(T1 Item) : Choice<T1, T2, T3, T4>;
    public record Choice2(T2 Item) : Choice<T1, T2, T3, T4>;
    public record Choice3(T3 Item) : Choice<T1, T2, T3, T4>;
    public record Choice4(T4 Item) : Choice<T1, T2, T3, T4>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2, Func<T3, U> onChoice3, Func<T4, U> onChoice4) =>
        this switch
        {
            Choice1 c => onChoice1(c.Item),
            Choice2 c => onChoice2(c.Item),
            Choice3 c => onChoice3(c.Item),
            Choice4 c => onChoice4(c.Item),
            _ => throw new ArgumentOutOfRangeException()
        };
}

public abstract record Choice<T1, T2, T3, T4, T5>
{
    public record Choice1(T1 Item) : Choice<T1, T2, T3, T4, T5>;
    public record Choice2(T2 Item) : Choice<T1, T2, T3, T4, T5>;
    public record Choice3(T3 Item) : Choice<T1, T2, T3, T4, T5>;
    public record Choice4(T4 Item) : Choice<T1, T2, T3, T4, T5>;
    public record Choice5(T5 Item) : Choice<T1, T2, T3, T4, T5>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2, Func<T3, U> onChoice3, Func<T4, U> onChoice4, Func<T5, U> onChoice5) =>
        this switch
        {
            Choice1 c => onChoice1(c.Item),
            Choice2 c => onChoice2(c.Item),
            Choice3 c => onChoice3(c.Item),
            Choice4 c => onChoice4(c.Item),
            Choice5 c => onChoice5(c.Item),
            _ => throw new ArgumentOutOfRangeException()
        };
}

public abstract record Choice<T1, T2, T3, T4, T5, T6>
{
    public record Choice1(T1 Item) : Choice<T1, T2, T3, T4, T5, T6>;
    public record Choice2(T2 Item) : Choice<T1, T2, T3, T4, T5, T6>;
    public record Choice3(T3 Item) : Choice<T1, T2, T3, T4, T5, T6>;
    public record Choice4(T4 Item) : Choice<T1, T2, T3, T4, T5, T6>;
    public record Choice5(T5 Item) : Choice<T1, T2, T3, T4, T5, T6>;
    public record Choice6(T6 Item) : Choice<T1, T2, T3, T4, T5, T6>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2, Func<T3, U> onChoice3, Func<T4, U> onChoice4, Func<T5, U> onChoice5, Func<T6, U> onChoice6) =>
        this switch
        {
            Choice1 c => onChoice1(c.Item),
            Choice2 c => onChoice2(c.Item),
            Choice3 c => onChoice3(c.Item),
            Choice4 c => onChoice4(c.Item),
            Choice5 c => onChoice5(c.Item),
            Choice6 c => onChoice6(c.Item),
            _ => throw new ArgumentOutOfRangeException()
        };
}
    
public abstract record Choice<T1, T2, T3, T4, T5, T6, T7>
{
    public record Choice1(T1 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;
    public record Choice2(T2 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;
    public record Choice3(T3 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;
    public record Choice4(T4 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;
    public record Choice5(T5 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;
    public record Choice6(T6 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;
    public record Choice7(T7 Item) : Choice<T1, T2, T3, T4, T5, T6, T7>;

    public U Match<U>(Func<T1, U> onChoice1, Func<T2, U> onChoice2, Func<T3, U> onChoice3, Func<T4, U> onChoice4, Func<T5, U> onChoice5, Func<T6, U> onChoice6, Func<T7, U> onChoice7) =>
        this switch
        {
            Choice1 c => onChoice1(c.Item),
            Choice2 c => onChoice2(c.Item),
            Choice3 c => onChoice3(c.Item),
            Choice4 c => onChoice4(c.Item),
            Choice5 c => onChoice5(c.Item),
            Choice6 c => onChoice6(c.Item),
            Choice7 c => onChoice7(c.Item),
            _ => throw new ArgumentOutOfRangeException()
        };
}