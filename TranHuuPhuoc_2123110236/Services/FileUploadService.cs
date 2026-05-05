using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Model;

namespace TranHuuPhuoc_2123110236.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IAmazonS3? _s3Client;
        private readonly bool _useS3;
        private readonly string _uploadFolder;
        private readonly string[] _allowedExtensions;
        private readonly long _maxFileSize;

        public FileUploadService(IConfiguration configuration, IWebHostEnvironment environment, IAmazonS3? s3Client = null)
        {
            _configuration = configuration;
            _environment = environment;
            _s3Client = s3Client;

            // Lấy cấu hình từ appsettings.json
            _uploadFolder = _configuration["FileUpload:UploadFolder"] ?? "uploads/images";
            _maxFileSize = long.Parse(_configuration["FileUpload:MaxFileSize"] ?? "5242880");
            _useS3 = bool.Parse(_configuration["FileUpload:UseS3"] ?? "false");

            var allowedExtensionsString = _configuration["FileUpload:AllowedExtensions"];
            if (allowedExtensionsString != null)
            {
                _allowedExtensions = allowedExtensionsString.Split(',')
                    .Select(x => x.Trim().ToLower())
                    .ToArray();
            }
            else
            {
                _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Kiểm tra file
                if (file == null || file.Length == 0)
                {
                    throw new Exception("File không được trống");
                }

                // Kiểm tra kích thước
                if (file.Length > _maxFileSize)
                {
                    throw new Exception($"Kích thước file vượt quá giới hạn {_configuration["FileUpload:MaxFileSizeMB"]}MB");
                }

                // Kiểm tra phần mở rộng
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    throw new Exception($"Định dạng file không được hỗ trợ. Các định dạng hỗ trợ: {string.Join(", ", _allowedExtensions)}");
                }

                if (_useS3 && _s3Client != null)
                {
                    return await UploadToS3Async(file, fileExtension);
                }
                else
                {
                    return UploadToLocal(file, fileExtension);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload ảnh: {ex.Message}");
            }
        }

        private async Task<string> UploadToS3Async(IFormFile file, string fileExtension)
        {
            try
            {
                var bucketName = _configuration["AWS:S3:BucketName"];
                var folderPath = _configuration["AWS:S3:FolderPath"] ?? "product-images";

                if (string.IsNullOrWhiteSpace(bucketName))
                {
                    throw new Exception("Bucket name không được cấu hình");
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var s3Key = $"{folderPath}/{fileName}";

                // Upload lên S3
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = s3Key,
                        InputStream = memoryStream,
                        ContentType = file.ContentType,
                        Metadata =
                        {
                            { "original-name", file.FileName }
                        }
                    };

                    await _s3Client!.PutObjectAsync(putRequest);
                }

                // Trả về S3 URL
                var s3Url = $"https://{bucketName}.s3.amazonaws.com/{s3Key}";
                return s3Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload lên S3: {ex.Message}");
            }
        }

        private string UploadToLocal(IFormFile file, string fileExtension)
        {
            // Tạo thư mục nếu chưa tồn tại
            var uploadPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", _uploadFolder);
            Directory.CreateDirectory(uploadPath);

            // Tạo tên file unique
            var fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Trả về URL (đường dẫn để truy cập)
            var imageUrl = $"/{_uploadFolder}/{fileName}".Replace("\\", "/");
            return imageUrl;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return false;
                }

                if (_useS3 && _s3Client != null)
                {
                    return await DeleteFromS3Async(imageUrl);
                }
                else
                {
                    return DeleteFromLocal(imageUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa ảnh: {ex.Message}");
            }
        }

        private async Task<bool> DeleteFromS3Async(string imageUrl)
        {
            try
            {
                var bucketName = _configuration["AWS:S3:BucketName"];
                var folderPath = _configuration["AWS:S3:FolderPath"] ?? "product-images";

                if (string.IsNullOrWhiteSpace(bucketName))
                {
                    throw new Exception("Bucket name không được cấu hình");
                }

                // Lấy tên file từ URL
                var fileName = Path.GetFileName(imageUrl);
                var s3Key = $"{folderPath}/{fileName}";

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key
                };

                await _s3Client!.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa từ S3: {ex.Message}");
            }
        }

        private bool DeleteFromLocal(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", _uploadFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        public string GetImagePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new Exception("Tên file không được trống");
            }

            if (_useS3)
            {
                var bucketName = _configuration["AWS:S3:BucketName"];
                var folderPath = _configuration["AWS:S3:FolderPath"] ?? "product-images";
                return $"https://{bucketName}.s3.amazonaws.com/{folderPath}/{fileName}";
            }
            else
            {
                return Path.Combine(_environment.WebRootPath ?? "wwwroot", _uploadFolder, fileName);
            }
        }
    }
}
