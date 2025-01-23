using FluentAssertions;

namespace TodoAppMaui.Shared.Tests
{
    public class SharedDataTests
    {
        private static int _sharedData;

        [Fact]
        public void FirstCall_ShouldIncreasedValue()
        {
            _sharedData++;

            _sharedData.Should().Be(1);
        }

        [Fact]
        public void SecondCall_ShouldIncreasedValue()
        {
            _sharedData++;

            _sharedData.Should().Be(1);
        }
    }
}
