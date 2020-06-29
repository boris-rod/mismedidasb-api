using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IAmazonS3Service
    {
        AmazonS3Client GetAmazonClient();

        string GetPresignUrl(string key);

        string GetPublicUrl(string key);

        Task<PutObjectResponse> PutObjectAsync(string key, MemoryStream content);

        Task<DeleteObjectResponse> DeleteObjectAsync(string key);

        Task<CopyObjectResponse> CopyObjectAsync(string originKey, string destinationKey);
    }
}
