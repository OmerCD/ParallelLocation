using NUnit.Framework;
using NUnit;

namespace Parallel.Location.Tests
{
    public class GradientDescentTests
    {
        private IAnchor[] _anchors;
        [SetUp]
        public void Setup()
        {
            _anchors = new IAnchor[]
            {
                new Anchor(1,1,0, 1), 
                new Anchor(44,85,0, 2), 
                new Anchor(44,1,0, 3), 
                new Anchor(1,85,0, 4), 
            };
        }

        [Test]
        public void GetResult_With_Success_Inside_Anchor_Bounds_Three_Anchors()
        {
            var gradientDescent = new GradientDescent();
            gradientDescent.SetAnchors(_anchors);
            IDistance[] node = {
                new DistanceBase(1, 41.4),
                new DistanceBase(3, 37.58),
                new DistanceBase(4, 56.79)
            };
            ICoordinate result = gradientDescent.GetResult(0,node);
            Assert.That(result.X, Is.EqualTo(26).Within(1.5));
            Assert.That(result.Y, Is.EqualTo(0).Within(3));
            Assert.That(result.Z, Is.EqualTo(34).Within(1.5));
        }
        [Test]
        public void GetResult_With_Success_Inside_Anchor_Bounds_Four_Anchors()
        {
            var gradientDescent = new GradientDescent();
            gradientDescent.SetAnchors(_anchors);
            IDistance[] node = {
                new DistanceBase(1, 41.4),
                new DistanceBase(3, 37.58),
                new DistanceBase(4, 56.79),
                new DistanceBase(2,54.08),

            };
            ICoordinate result = gradientDescent.GetResult(0,node);
            Assert.That(result.X, Is.EqualTo(26).Within(1.5));
            Assert.That(result.Y, Is.EqualTo(0).Within(3));
            Assert.That(result.Z, Is.EqualTo(34).Within(1.5));
        }
        [Test]
        public void GetResult_With_Success_Outside_Anchor_Bounds_Four_Anchors()
        {
            var gradientDescent = new GradientDescent();
            gradientDescent.SetAnchors(_anchors);
            IDistance[] node = {
                new DistanceBase(1, 101.11),
                new DistanceBase(3, 89.14),
                new DistanceBase(4, 48.25),
                new DistanceBase(2,7.07),

            };
            ICoordinate result = gradientDescent.GetResult(0,node);
            Assert.That(result.X, Is.EqualTo(44).Within(4));
            Assert.That(result.Y, Is.EqualTo(0).Within(3));
            Assert.That(result.Z, Is.EqualTo(85).Within(4));
        }
    }
}