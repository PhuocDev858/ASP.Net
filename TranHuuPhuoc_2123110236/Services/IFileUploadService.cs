namespace TranHuuPhuoc_2123110236.Services
{
    public interface IFileUploadService
    {
        /// <summary>
        /// Upload một file ảnh và trả về URL của file
        /// </summary>
        /// <param name="file">File được upload</param>
        /// <returns>URL của file hoặc tên file đã lưu</returns>
        Task<string> UploadImageAsync(IFormFile file);

        /// <summary>
        /// Xóa một file ảnh từ server
        /// </summary>
        /// <param name="imageUrl">URL hoặc tên file</param>
        Task<bool> DeleteImageAsync(string imageUrl);

        /// <summary>
        /// Lấy đường dẫn tuyệt đối của file
        /// </summary>
        /// <param name="fileName">Tên file</param>
        /// <returns>Đường dẫn tuyệt đối</returns>
        string GetImagePath(string fileName);
    }
}
