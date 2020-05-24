using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MismeAPI.Services.Impls
{
    public class FileService : IFileService
    {
        private readonly IAmazonS3Service _s3Service;
        private readonly IConfiguration _config;

        public FileService(IAmazonS3Service s3Service, IConfiguration config)
        {
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task CopyFileAsync(string sourceKey, string destinationKey)
        {
            await _s3Service.CopyObjectAsync(sourceKey, destinationKey);
        }

        public async Task DeleteFileAsync(string guid)
        {
            await _s3Service.DeleteObjectAsync(guid);
        }

        public async Task UploadFileAsync(IFormFile file, string guid)
        {
            if (file == null)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "File");
            }

            if (file.Length == 0)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "File");
            }
            if (file.Length > 2 * 1024 * 1024)
            {
                throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "File");
            }

            using (Stream stream = file.OpenReadStream())
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    var mime = file.ContentType;
                    var fileContent = binaryReader.ReadBytes((int)file.Length);

                    if (!mime.Equals("image/png") && !mime.Equals("image/jpg") && !mime.Equals("image/jpeg"))
                    {
                        throw new MismeAPI.Common.Exceptions.InvalidDataException(ExceptionConstants.INVALID_DATA, "File");
                    }

                    //upload the file
                    await _s3Service.PutObjectAsync(guid, new MemoryStream(fileContent));
                }
            }
        }
    }
}