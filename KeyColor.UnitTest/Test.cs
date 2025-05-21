
using Xunit;
#if !NET7_0_OR_GREATER
using KeyColor.Standard;
#endif

namespace KeyColor.UnitTest {


    public class KeyColorGeneratorTests {
        [Fact]
        public void TestGetUniqueColorFromString() {
            KeyColorGenerator generator = new KeyColorGenerator();
            const string label = "testLabel";
            GeneratedColor color1 = generator[label];
            GeneratedColor color2 = generator[label];

            Assert.Equal(color1.R, color2.R);
            Assert.Equal(color1.G, color2.G);
            Assert.Equal(color1.B, color2.B);
        }

        [Fact]
        public void TestGetUniqueColorFromSpan() {
            KeyColorGenerator generator = new KeyColorGenerator();
            byte[] key = { 1, 2, 3 };
            GeneratedColor color1 = generator[key];
            GeneratedColor color2 = generator[key];

            Assert.Equal(color1.R, color2.R);
            Assert.Equal(color1.G, color2.G);
            Assert.Equal(color1.B, color2.B);
        }

        [Fact]
        public void TestGetUniqueColorFromDifferentInputs() {
            KeyColorGenerator generator = new KeyColorGenerator();
            const string label = "testLabel";
            byte[] key = { 1, 2, 3 };

            GeneratedColor colorFromLabel = generator[label];
            GeneratedColor colorFromKey = generator[key];

            Assert.NotEqual(colorFromLabel.R, colorFromKey.R);
            Assert.NotEqual(colorFromLabel.G, colorFromKey.G);
            Assert.NotEqual(colorFromLabel.B, colorFromKey.B);
        }

        [Fact]
        public void TestBrightnessRange() {
            KeyColorGenerator generator = new KeyColorGenerator();
            const string label = "testLabel";
            GeneratedColor color = generator[label];

            int brightness = KeyColorGenerator.GetColorBrightness(color.R, color.G, color.B);

            Assert.True(brightness >= generator.Brightness.Min && brightness <= generator.Brightness.Max);
        }

    }
}