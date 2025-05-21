using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER

namespace KeyColor;

public readonly struct GeneratedColor {
    public required byte R { get; init; }
    public required byte G { get; init; }
    public required byte B { get; init; }

    public byte[] ToArray() {
        return new byte[] { R, G, B };
    }

    public string ToCssColor() {
#if NET8_0_OR_GREATER
        const int expectedStructSize = 3;
        Debug.Assert(Unsafe.SizeOf<GeneratedColor>() == expectedStructSize);
        Span<byte> bytes = stackalloc byte[expectedStructSize];
        MemoryMarshal.Write(bytes, in this);
#else
        GeneratedColor copyOfThis = this;
        Span<byte> bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref copyOfThis, 1));
#endif
        return $"#{Convert.ToHexString(bytes).Replace("-", "")}";
    }
}

#endif