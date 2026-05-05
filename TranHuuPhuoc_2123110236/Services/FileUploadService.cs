using Microsoft.Extensions.Configuration;

namespace TranHuuPhuoc_2123110236.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadFolder;
        private readonly string[] _allowedExtensions;
        private readonly long _maxFileSize;

        public FileUploadService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            // Lấy cấu hình từ appsettings.json
            _uploadFolder = _configuration["FileUpload:UploadFolder"] ?? "uploads/images";
            _maxFileSize = long.Parse(_configuration["FileUpload:MaxFileSize"] ?? "5242880"); // 5MB default

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

                // Tạo thư mục nếu chưa tồn tại
                var uploadPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", _uploadFolder);
                Directory.CreateDirectory(uploadPath);

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về URL (đường dẫn để truy cập)
                // Ví dụ: /uploads/images/guid_timestamp.jpg
                var imageUrl = $"/{_uploadFolder}/{fileName}".Replace("\\", "/");

                return imageUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload ảnh: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return false;
                }

                // Lấy tên file từ URL
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", _uploadFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa ảnh: {ex.Message}");
            }
        }

        public string GetImagePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new Exception("Tên file không được trống");
            }

            return Path.Combine(_environment.WebRootPath ?? "wwwroot", _uploadFolder, fileName);
        }
    }
}
