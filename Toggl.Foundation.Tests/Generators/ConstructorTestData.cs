using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Toggl.Foundation.Tests.Generators
{
    public abstract class ConstructorTestData : IEnumerable<object[]>
    {
        private readonly List<object[]> data;

        protected ConstructorTestData(int parameterCount)
        {
            data = Enumerable.Range(0, parameterCount)
                    .Select(i => Enumerable.Range(0, parameterCount)
                        .Select(j => (object)(i != j))
                        .ToArray())
                    .ToList();
        }

        public IEnumerator<object[]> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class TwoParameterConstructorTestData : ConstructorTestData
    {
        public TwoParameterConstructorTestData() : base(2) { }
    }

    public sealed class ThreeParameterConstructorTestData : ConstructorTestData
    {
        public ThreeParameterConstructorTestData() : base(3) { }
    }

    public sealed class FourParameterConstructorTestData : ConstructorTestData
    {
        public FourParameterConstructorTestData() : base(4) { }
    }

    public sealed class FiveParameterConstructorTestData : ConstructorTestData
    {
        public FiveParameterConstructorTestData() : base(5) { }
    }

    public sealed class SixParameterConstructorTestData : ConstructorTestData
    {
        public SixParameterConstructorTestData() : base(6) { }
    }

    public sealed class SevenParameterConstructorTestData : ConstructorTestData
    {
        public SevenParameterConstructorTestData() : base(7) { }
    }

    public sealed class EightParameterConstructorTestData : ConstructorTestData
    {
        public EightParameterConstructorTestData() : base(8) { }
    }

    public sealed class NineParameterConstructorTestData : ConstructorTestData
    {
        public NineParameterConstructorTestData() : base(9) { }
    }

    public sealed class TenParameterConstructorTestData : ConstructorTestData
    {
        public TenParameterConstructorTestData() : base(10) { }
    }

    public sealed class ElevenParameterConstructorTestData : ConstructorTestData
    {
        public ElevenParameterConstructorTestData() : base(11) { }
    }

    public sealed class TwelveParameterConstructorTestData : ConstructorTestData
    {
        public TwelveParameterConstructorTestData() : base(12) { }
    }

    public sealed class FourteenParameterConstructorTestData : ConstructorTestData
    {
        public FourteenParameterConstructorTestData() : base(14) { }
    }

    public sealed class ConstructorTestDataTests
    {
        private sealed class ConstructorTestDataTest : ConstructorTestData
        {
            public ConstructorTestDataTest(int n) : base(n)
            {
            }
        }

        public sealed class TheGeneratedSequence
        {
            [Property]
            public void ReturnsSequenceWithExactlyOneFalseItem(byte n)
            {
                if (n == 0) return;

                var testData = new ConstructorTestDataTest(n);
                testData.All(sequence => sequence.Count(x => (bool)x == false) == 1).Should().BeTrue();
            }
        }
    }
}
