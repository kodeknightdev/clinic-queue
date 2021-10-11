using ClinicQueue.Web.Services;
using Xunit;

namespace ClinicQueue.Tests
{
    public class LookupCodeTests
    {
        [Fact]
        public void Generate_AvoidsConfusableCharacters()
        {
            for (var i = 0; i < 500; i++)
            {
                var code = LookupCode.Generate();

                Assert.Equal(LookupCode.Length, code.Length);
                Assert.DoesNotContain(code, c => c == '0' || c == 'O' || c == '1' || c == 'I' || c == 'L');
            }
        }

        [Theory]
        [InlineData(" k7tq2m ", "K7TQ2M")]
        [InlineData("ABC123", "ABC123")]
        [InlineData(null, null)]
        public void Normalize(string input, string expected)
        {
            Assert.Equal(expected, LookupCode.Normalize(input));
        }
    }

    public class PhoneNumberTests
    {
        [Theory]
        [InlineData("(555) 010-2030", "5550102030")]
        [InlineData("555.010.2030", "5550102030")]
        [InlineData("+44 20 7946 0000", "+442079460000")]
        [InlineData("   ", null)]
        [InlineData("n/a", null)]
        [InlineData(null, null)]
        public void Normalize(string input, string expected)
        {
            Assert.Equal(expected, PhoneNumber.Normalize(input));
        }
    }
}
