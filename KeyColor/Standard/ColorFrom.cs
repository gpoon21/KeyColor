#if !NET8_0_OR_GREATER

using System;

namespace KeyColor.Standard {

    public static class ColorFrom {

        private static readonly KeyColorGenerator _generator = new KeyColorGenerator();
        private static readonly Random _random = new Random();

        public static GeneratedColor Key<T>(T key) where T : struct {
            return _generator.GetUniqueColor(key);
        }

        public static GeneratedColor String(string key) {
            return _generator.GetUniqueColor(key);
        }

        public static GeneratedColor Span<T>(byte[] data) where T : struct {
            return _generator.GetUniqueColor(data);
        }

        public static GeneratedColor Rng() {
            int random;
            lock (_random) {
                random = _random.Next();
            }
            return _generator.GetUniqueColor(random);
        }
    }
}

#endif