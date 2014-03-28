using ApprovalTests;
using FurlStrong.Generation;
using FurlStrong.Internal;
using NUnit.Framework;

namespace FurlStrong.Tests
{
    [TestFixture]
    public class BigPictureSpecs
    {
        [Test]
        public void When_generating_it_should_look_right()
        {
            var urlmap = ManifestResourceHelper.ExtractResourceToString("FurlStrong.Tests.sample.urls");

            var generator = new Generator();

            var result = generator.GenerateStrongUrls(urlmap);

            Approvals.Verify(result);
        }
    }
}
