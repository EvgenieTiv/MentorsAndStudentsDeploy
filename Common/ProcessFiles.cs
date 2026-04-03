using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public class ProcessFiles: IProcessFiles
    {
        private readonly IConfiguration _configuration;

        public ProcessFiles(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> UploadFile(string fileName, IFormFile File, string directory)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["FilesConnectionStrings:ConnectionString"]);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["FilesConnectionStrings:ContainerName"]);

                string blobName = $"{directory}/{fileName}";

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                await using (Stream stream = File.OpenReadStream())
                {
                    var uploadOptions = new Azure.Storage.Blobs.Models.BlobUploadOptions
                    {
                        HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                        {
                            ContentType = File.ContentType
                        }
                    };

                    await blobClient.UploadAsync(stream, uploadOptions);
                }

                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<FileStreamResult> DownloadFile(string fileName, string directory)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["FilesConnectionStrings:ConnectionString"]);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["FilesConnectionStrings:ContainerName"]);

                string blobName = $"{directory}/{fileName}";

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    throw new FileNotFoundException();
                }

                var stream = new MemoryStream();

                await blobClient.DownloadToAsync(stream);

                stream.Position = 0;

                string contentType = "application/otcet-stream";

                return new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = fileName
                };
            }

            catch (Exception ex)
            {
                throw new Exception("Downloading file failed");
            }
        }

        public async Task<bool> DeleteFile(string fileName, string directory)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["FilesConnectionStrings:ConnectionString"]);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["FilesConnectionStrings:ContainerName"]);

                string blobName = $"{directory}/{fileName}";

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    throw new FileNotFoundException();
                }

                var stream = new MemoryStream();

                await blobClient.DeleteAsync();

                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
