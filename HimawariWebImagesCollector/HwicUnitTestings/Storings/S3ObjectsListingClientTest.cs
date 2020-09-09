
namespace Hwic.UnitTestings.Storings
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hwic.Net;
    using Hwic.Storings;


    using Xunit;
    using Xunit.Abstractions;


    public class S3ObjectsListingClientTest
    {
        private ITestOutputHelper Output { get; }


        public S3ObjectsListingClientTest(ITestOutputHelper output)
            => this.Output = output;


        [Theory]
        [InlineData("play.min.io", "mymusic", "Q3AM3UQ867SPQQA43P2F", "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG")]
        public async Task ListObjects_ShouldHaveResult(
                string endpoint,
                string bucketName,
                string accessKey,
                string secretKey)
        {
            var s3config = new S3StorageConfig(
                provider: new S3Provider("play.min.io", endpoint),
                bucketName: bucketName,
                accessKey: accessKey,
                secretKey: secretKey,
                proxies: Enumerable.Empty<Socks5ProxyInfo>()
            );
            var queryClient = s3config.GetQueryClient();

            var items = await queryClient.GetObjectListAsync(bucketName);
            foreach (var item in items)
                this.Output.WriteLine(item.Key, item.Size);
        }
    }
}
