using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Relay.Application.Services;

namespace Relay.Infrastructure.Storage;

public sealed class S3ObjectStorage(IConfiguration configuration) : IObjectStorage
{
    private AmazonS3Client CreateClient()
    {
        var config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["Storage:Region"] ?? "auto") };
        var serviceUrl = configuration["Storage:ServiceUrl"];
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            config.ServiceURL = serviceUrl;
            config.ForcePathStyle = true;
        }
        return new AmazonS3Client(configuration["Storage:AccessKey"], configuration["Storage:SecretKey"], config);
    }

    public async Task<string> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        await client.PutObjectAsync(new PutObjectRequest { BucketName = configuration["Storage:Bucket"], Key = key, InputStream = content, ContentType = contentType }, cancellationToken);
        return BuildPublicUrl(key);
    }

    public Task<string> CreateReadUrlAsync(string key, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        var url = client.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = configuration["Storage:Bucket"], Key = key, Expires = DateTime.UtcNow.Add(lifetime) });
        return Task.FromResult(url);
    }

    private string BuildPublicUrl(string key)
    {
        var baseUrl = configuration["Storage:PublicBaseUrl"];
        return string.IsNullOrWhiteSpace(baseUrl) ? key : $"{baseUrl.TrimEnd('/')}/{key.TrimStart('/')}";
    }
}
