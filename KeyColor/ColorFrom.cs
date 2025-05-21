#if NET7_0_OR_GREATER

using System;
using System.Runtime.InteropServices;

namespace KeyColor;

public static class ColorFrom {

    private static readonly KeyColorGenerator _generator = new();

    public static GeneratedColor Key<T>(T key) where T : struct {
        return _generator.GetUniqueColor(key);
    }

    public static GeneratedColor String(string key) {
        return _generator.GetUniqueColor(key);
    }

    public static GeneratedColor Span<T>(ReadOnlySpan<T> span) where T : struct {
        return _generator.GetUniqueColor(MemoryMarshal.AsBytes(span));
    }
    
    public static GeneratedColor Rng() {
        return _generator.GetUniqueColor(Random.Shared.Next());
    }
}
#endif