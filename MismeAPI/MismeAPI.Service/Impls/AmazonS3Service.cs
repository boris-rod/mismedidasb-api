using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MismeAPI.Services.Impls
{
    public class AmazonS3Service : IAmazonS3Service
    {
        private readonly IConfiguration _config;

        public AmazonS3Service(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(string key)
        {
            var client = GetAmazonClient();
            var bucketName = _config["AWS:BucketName"];

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await client.DeleteObjectAsync(deleteRequest);

            return response;
        }

        public AmazonS3Client GetAmazonClient()

        {
            var accessKey = _config["AWS:AccessKey"];
            var secretKey = _config["AWS:SecretKey"];
            if
             (String.IsNullOrWhiteSpace(accessKey) || String.IsNullOrWhiteSpace(secretKey))
            {
                return new AmazonS3Client();
            }
            var region = _config["AWS:S3Env"];
            return new AmazonS3Client(accessKey, secretKey, region == "dev" ? RegionEndpoint.USEast2 : RegionEndpoint.EUWest1);
        }

        public string GetPresignUrl(string key)
        {
            var client = GetAmazonClient();

            var bucketName = _config["AWS:BucketName"];

            var getRequest = new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddMinutes(100)
            };

            string url = client.GetPreSignedURL(getRequest);
            return url;
        }

        public string GetPublicUrl(string key)
        {
            var bucketName = _config["AWS:BucketName"];
            var awsUri = ".s3.amazonaws.com/";

            var url = "https://" + bucketName + awsUri + key;

            return url;
        }

        public async Task<PutObjectResponse> PutObjectAsync(string key, MemoryStream content)
        {
            //try
            //{
            var client = GetAmazonClient();

            var bucketName = _config["AWS:BucketName"];

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = content
            };

            var response = await client.PutObjectAsync(putRequest);
            return response;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
        }

        public async Task<CopyObjectResponse> CopyObjectAsync(string originKey, string destinationKey)
        {
            try
            {
                var client = GetAmazonClient();

                var bucketName = _config["AWS:BucketName"];
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    DestinationBucket = bucketName,
                    SourceKey = originKey,
                    DestinationKey = destinationKey
                };
                var response = await client.CopyObjectAsync(copyRequest);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}