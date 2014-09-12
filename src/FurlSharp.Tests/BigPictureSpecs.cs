using ApprovalTests;
using FurlSharp.Generation;
using FurlSharp.Internal;
using NUnit.Framework;

namespace FurlSharp.Tests
{
    [TestFixture]
    public class BigPictureSpecs
    {
        [Test]
        public void When_generating_it_should_look_right()
        {
            var urlmap = ManifestResourceHelper.ExtractResourceToString("FurlSharp.Tests.sample.urls");

            var generator = new Generator();

            var result = generator.GenerateStrongUrls(urlmap);

            Approvals.Verify(result);
        }
    }
}
