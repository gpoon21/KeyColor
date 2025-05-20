using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

#if NETSTANDARD2_0

namespace KeyColor.Standard {

    public class KeyColorGenerator {
        public GeneratedColor this[string label] {
            get { return GetUniqueColor(label); }
        }

        public GeneratedColor this[byte[] key] { // Changed from ReadOnlySpan<byte> to byte[]
            get { return GetUniqueColor(key); }
        }

        public GeneratedColor GetUniqueColor<T>(T key) where T : struct {
            byte[] data = StructToByteArray(key);
            return GetUniqueColor(data);
        }

        private static byte[] StructToByteArray<T>(T structData) where T : struct {
            int size = Marshal.SizeOf(structData);
            byte[] array = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.StructureToPtr(structData, ptr, true);
                Marshal.Copy(ptr, array, 0, size);
            } finally {
                Marshal.FreeHGlobal(ptr);
            }
            return array;
        }


        public GeneratedColor GetUniqueColor(string label) {
            return GenerateColor(label);
        }

        public GeneratedColor GetUniqueColor(byte[] key) { // Changed from ReadOnlySpan<byte> to byte[]
            return GenerateColor(key);
        }


        /// <summary>
        /// Range of HLS saturation. Range between 0 and 1.
        /// </summary>
        public readonly RangeDouble Saturation = new RangeDouble(0, 1) { Min = 0, Max = 1 };

        /// <summary>
        /// Range of HLS lightness. Range between 0 and 1.
        /// </summary>
        public readonly RangeDouble Lightness = new RangeDouble(0, 1) { Min = 0.2, Max = 0.8 };

        /// <summary> 
        /// The range of human eyes perceived brightness. Range between 0 and 255.
        /// </summary>
        public readonly RangeInt Brightness = new RangeInt(0, 255) { Min = 80, Max = 200 };


        public int Seed {
            get { return _seed; }
            set {
                _seed = value;
                BitConverter.GetBytes(value).CopyTo(_hueSeed, 0);
                BitConverter.GetBytes(value).CopyTo(_saturationSeed, 0);
                BitConverter.GetBytes(value).CopyTo(_lightnessSeed, 0);
            }
        }

        private int _seed;

        private readonly byte[] _hueSeed = new byte[sizeof(int)];
        private readonly byte[] _saturationSeed = new byte[sizeof(int)];
        private readonly byte[] _lightnessSeed = new byte[sizeof(int)];


        private readonly RangeDouble _hueRange = new RangeDouble(0, 1);

        private GeneratedColor GenerateColor(string label) {
            return GenerateColor(Encoding.Unicode.GetBytes(label));
        }

        private GeneratedColor GenerateColor(byte[] keySpan) { // Changed from ReadOnlySpan<byte> to byte[]

            // Compute SHA256 hash of the label
            byte[] hueHash;
            byte[] saturationHash;
            byte[] lightnessHash;

            using (HMACSHA256 hmac = new HMACSHA256(_hueSeed)) {
                hueHash = hmac.ComputeHash(keySpan, 0, keySpan.Length);
            }
            using (HMACSHA256 hmac = new HMACSHA256(_saturationSeed)) {
                saturationHash = hmac.ComputeHash(keySpan, 0, keySpan.Length);
            }
            using (HMACSHA256 hmac = new HMACSHA256(_lightnessSeed)) {
                lightnessHash = hmac.ComputeHash(keySpan, 0, keySpan.Length);
            }


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


            return new GeneratedColor(color.R, color.G, color.B);
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
            if (t < 1.0 / 6.0)
                return p + (q - p) * 6.0 * t;
            if (t < 1.0 / 2.0)
                return q;
            if (t < 2.0 / 3.0)
                return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            return p;
        }

        #region Ranges

        public class RangeDouble {
            private readonly double _minMin;
            private readonly double _maxMax;
            private double _min;
            private double _max;

            public double Min {

                get { return _min; }
                set {
                    if (value < _minMin) return;
                    if (value > Max) return;
                    _min = value;
                }
            }

            public double Max {
                get { return _max; }
                set {
                    if (value > _maxMax) return;
                    if (value < Min) return;
                    _max = value;
                }
            }

            public RangeDouble(double minMin, double maxMax) {
                _minMin = minMin;
                _maxMax = maxMax;
                Min = minMin;
                Max = maxMax;
            }

            public double Map(byte[] hash) {
                ulong value = BitConverter.ToUInt64(hash, 0);
                // Convert all values to double for calculation
                double valueDouble = value;
                const double tMinDouble = ulong.MinValue;
                const double tMaxDouble = ulong.MaxValue;
                double minDouble = Min;
                double maxDouble = Max;

                return (valueDouble - tMinDouble) * (maxDouble - minDouble) / (tMaxDouble - tMinDouble) + minDouble;
            }
        }

        public class RangeInt {
            private readonly int _minMin;
            private readonly int _maxMax;
            private int _min;
            private int _max;

            public int Min {

                get { return _min; }
                set {
                    if (value < _minMin) return;
                    if (value > Max) return;
                    _min = value;
                }
            }

            public int Max {
                get { return _max; }
                set {
                    if (value > _maxMax) return;
                    if (value < Min) return;
                    _max = value;
                }
            }

            public RangeInt(int minMin, int maxMax) {
                _minMin = minMin;
                _maxMax = maxMax;
                Min = minMin;
                Max = maxMax;
            }

            public int Map(byte[] hash) {
                ulong value = BitConverter.ToUInt64(hash, 0);
                // Convert all values to int for calculation
                int valueInt = (int)(value % int.MaxValue);
                const int tMinInt = 0;
                const int tMaxInt = int.MaxValue - 1;
                int minInt = Min;
                int maxInt = Max;

                return (valueInt - tMinInt) * (maxInt - minInt) / (tMaxInt - tMinInt) + minInt;
            }
        }

        #endregion

    }
}

#endif