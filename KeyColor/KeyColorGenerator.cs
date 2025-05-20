#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace KeyColor;

public class KeyColorGenerator {

    public GeneratedColor this[string label] {
        get { return GetUniqueColor(label); }
    }

    public GeneratedColor this[ReadOnlySpan<byte> key] {
        get { return GetUniqueColor(key); }
    }

    public GeneratedColor GetUniqueColor<T>(T key) where T : struct {
        Span<byte> span = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref key, 1));
        return GetUniqueColor(span);
    }

    public GeneratedColor GetUniqueColor(string label) {
        return GenerateColor(label);
    }

    public GeneratedColor GetUniqueColor(ReadOnlySpan<byte> key) {
        return GenerateColor(key);
    }


    /// <summary>
    /// Range of HLS saturation. Range between 0 and 1.
    /// </summary>
    public readonly Range<double> Saturation = new(0, 1) { Min = 0, Max = 1 };

    /// <summary>
    /// Range of HLS lightness. Range between 0 and 1.
    /// </summary>
    public readonly Range<double> Lightness = new(0, 1) { Min = 0.2, Max = 0.8 };

    /// <summary> 
    /// The range of human eyes perceived brightness. Range between 0 and 255.
    /// </summary>
    public readonly Range<int> Brightness = new(0, 255) { Min = 80, Max = 200 };


    public int Seed {
        get { return MemoryMarshal.Read<int>(_hueSeed); }
        set {
#if NET8_0_OR_GREATER
            MemoryMarshal.Write(_hueSeed, value);
            MemoryMarshal.Write(_saturationSeed, value + 1);
            MemoryMarshal.Write(_lightnessSeed, value + 2);
#else
            MemoryMarshal.Write(_hueSeed, ref value);
            value++;
            MemoryMarshal.Write(_saturationSeed, ref value);
            value++;
            MemoryMarshal.Write(_lightnessSeed, ref value);
#endif
        }
    }

    private readonly byte[] _hueSeed = new byte[sizeof(int)];
    private readonly byte[] _saturationSeed = new byte[sizeof(int)];
    private readonly byte[] _lightnessSeed = new byte[sizeof(int)];


    private readonly Range<double> _hueRange = new(0, 1);

    private GeneratedColor GenerateColor(string label) {
        return GenerateColor(MemoryMarshal.AsBytes(label.AsSpan()));
    }

    private GeneratedColor GenerateColor(ReadOnlySpan<byte> keySpan) {

        // Compute SHA256 hash of the label
        Span<byte> hueHash = stackalloc byte[32];
        Span<byte> saturationHash = stackalloc byte[32];
        Span<byte> lightnessHash = stackalloc byte[32];

        HMACSHA256.HashData(_hueSeed, keySpan, hueHash);
        HMACSHA256.HashData(_saturationSeed, keySpan, saturationHash);
        HMACSHA256.HashData(_lightnessSeed, keySpan, lightnessHash);

        // Map the hash value to a hue
        double hue = _hueRange.Map(hueHash); //(hashValue / (double)ulong.MaxValue) * 360.0;

        double lightness = Lightness.Map(lightnessHash);
        double saturation = Saturation.Map(saturationHash);

        // Convert HSL to RGB
        Color color = Hsl2Rgb(hue, saturation, lightness);

        // Compute brightness
        int brightness = GetColorBrightness(color.R, color.G, color.B);

        if (brightness < Brightness.Min) {
            while (brightness < Brightness.Min) {
                lightness += 0.1;
                color = Hsl2Rgb(hue, saturation, lightness);
                brightness = GetColorBrightness(color.R, color.G, color.B);
            }
        } else if (brightness > Brightness.Max) {
            while (brightness > Brightness.Min) {
                lightness -= 0.1;
                color = Hsl2Rgb(hue, saturation, lightness);
                brightness = GetColorBrightness(color.R, color.G, color.B);
            }
        }


        return new GeneratedColor() {
            R = color.R,
            G = color.G,
            B = color.B
        };
    }

    public static int GetColorBrightness(byte r, byte g, byte b) {
        return (int)Math.Sqrt(
            r * r * .241 +
            g * g * .691 +
            b * b * .068);
    }

    private static Color Hsl2Rgb(double h, double s, double l) {
        double r, g, b;

        if (s == 0) {
            r = g = b = l; // Achromatic color (gray scale)
        } else {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = Hue2Rgb(p, q, h + 1.0 / 3.0);
            g = Hue2Rgb(p, q, h);
            b = Hue2Rgb(p, q, h - 1.0 / 3.0);
        }

        return Color.FromArgb(
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }

    private static double Hue2Rgb(double p, double q, double t) {
        if (t < 0) t += 1.0;
        if (t > 1) t -= 1.0;
        return t switch {
            < 1.0 / 6.0 => p + (q - p) * 6.0 * t,
            < 1.0 / 2.0 => q,
            < 2.0 / 3.0 => p + (q - p) * (2.0 / 3.0 - t) * 6.0,
            _           => p
        };
    }

    public class Range<T> where T : struct, INumber<T>, IMinMaxValue<T>, IConvertible {
        private readonly T _minMin;
        private readonly T _maxMax;
        private T _min;
        private T _max;

        public T Min {
            get { return _min; }
            set {
                if (value < _minMin) return;
                if (value > Max) return;
                _min = value;

            }
        }

        public T Max {
            get { return _max; }
            set {
                if (value > _maxMax) return;
                if (value < Min) return;
                _max = value;
            }
        }

        public Range(T minMin, T maxMax) {
            _minMin = minMin;
            _maxMax = maxMax;
            Min = minMin;
            Max = maxMax;
        }

        public double Map(ReadOnlySpan<byte> hash) {
            ulong value = MemoryMarshal.Read<ulong>(hash);

            // Convert all values to double for calculation
            double valueDouble = value;
            const double tMinDouble = ulong.MinValue;
            const double tMaxDouble = ulong.MaxValue;
            double minDouble = Min.ToDouble(null);
            double maxDouble = Max.ToDouble(null);

            // Map the value to the [Min, Max] range
            double mappedValueDouble =
                minDouble + (valueDouble - tMinDouble) * (maxDouble - minDouble) / (tMaxDouble - tMinDouble);

            return mappedValueDouble;
        }

    }
}

#endif