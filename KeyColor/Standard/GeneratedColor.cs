using System;

#if !NET8_0_OR_GREATER

namespace KeyColor.Standard {
    public readonly struct GeneratedColor {
        public GeneratedColor(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public byte[] ToArray() {
            return new byte[] { R, G, B };
        }

        public string ToCssColor() {
            byte[] bytes = { R, G, B };
            return $"#{BitConverter.ToString(bytes).Replace("-", "")}";
        }
    }
}


#endif