# AWS S3 Image Upload Integration Guide

## Overview
Backend đã được cấu hình để upload ảnh lên AWS S3 thay vì lưu trên local server. Điều này giải quyết vấn đề frontend local không thể truy cập ảnh từ backend deployed.

## Setup AWS S3

### Step 1: Tạo AWS Account
1. Vào https://aws.amazon.com
2. Tạo tài khoản AWS (nếu chưa có)
3. Đăng nhập vào AWS Management Console

### Step 2: Tạo S3 Bucket
1. Vào **S3 service** → **Buckets**
2. Nhấp **Create Bucket**
3. Điền tên bucket (vd: `phuoc-shop-images`)
   - Tên phải unique toàn cầu
   - Không có space hoặc ký tự đặc biệt
4. Chọn region (vd: `ap-southeast-1` cho VN)
5. Nhấp **Create Bucket**

### Step 3: Cấu hình Bucket Policy
1. Mở bucket vừa tạo
2. Vào tab **Permissions**
3. Tìm **Bucket Policy** → nhấp **Edit**
4. Dán policy sau:
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::phuoc-shop-images/*"
        }
    ]
}
```
(Thay `phuoc-shop-images` bằng tên bucket của bạn)

5. Nhấp **Save Changes**

### Step 4: Tạo IAM User (Access Key & Secret Key)
1. Vào **IAM** service
2. Vào **Users** → **Create User**
3. Điền username (vd: `phuoc-shop-app`)
4. Nhấp **Next**
5. Chọn **Attach policies directly**
6. Tìm và chọn `AmazonS3FullAccess`
7. Nhấp **Create User**
8. Vào user vừa tạo → **Security Credentials** tab
9. Nhấp **Create Access Key**
10. Chọn **Application running outside AWS** → **Next**
11. Copy **Access Key ID** và **Secret Access Key**
    - Lưu lại 2 cái này ở nơi an toàn!

## Cấu Hình Backend

### Option 1: Local Development
Sửa `appsettings.Development.json`:

```json
{
  "FileUpload": {
    "UseS3": true,
    "UploadFolder": "uploads/images",
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".gif", ".webp" ],
    "MaxFileSize": 5242880,
    "MaxFileSizeMB": 5
  },
  "AWS": {
    "S3": {
      "BucketName": "phuoc-shop-images",
      "Region": "ap-southeast-1",
      "AccessKey": "YOUR_ACCESS_KEY_HERE",
      "SecretKey": "YOUR_SECRET_KEY_HERE",
      "FolderPath": "product-images"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Option 2: Production (Render)
Thêm Environment Variables vào Render:
1. Vào dashboard Render.com → Select service
2. Vào **Environment** tab
3. Thêm environment variables:
```
FileUpload__UseS3=true
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS__S3__BucketName=phuoc-shop-images
AWS__S3__Region=ap-southeast-1
AWS__S3__FolderPath=product-images
```

## Cách Hoạt Động

### Upload Flow:
```
Frontend select file
    ↓
Frontend upload to backend API
    ↓
Backend validates file
    ↓
Backend uploads to S3 bucket
    ↓
S3 returns file URL
    ↓
Backend returns: { imageUrl: "https://bucket.s3.amazonaws.com/..." }
    ↓
Frontend displays image + saves URL to database
```

### Image Access:
- URL format: `https://phuoc-shop-images.s3.amazonaws.com/product-images/filename.jpg`
- Accessible từ bất kỳ đâu (frontend local, deployed backend, etc.)

## Testing

### Local Testing:
1. Cập nhật `appsettings.Development.json` với AWS credentials
2. Run backend: `dotnet run`
3. Upload ảnh qua admin panel
4. Kiểm tra:
   - Console log có hiển thị S3 upload success không
   - URL trả về có phải S3 URL không
   - Frontend có hiển thị ảnh không

### Troubleshooting:
- **"Bucket name không được cấu hình"** → Check `BucketName` trong appsettings.json
- **403 Forbidden** → Check bucket policy và IAM permissions
- **Access denied** → Check Access Key & Secret Key
- **Ảnh không hiển thị** → Check bucket policy cho public read access

## Chuyên Đổi Giữa S3 và Local

Để dùng local upload thay S3:
```json
"FileUpload": {
  "UseS3": false,  // ← Thay true thành false
  ...
}
```

Khi `UseS3 = false`, backend sẽ lưu ảnh vào `wwwroot/uploads/images/` như trước.

## Xóa Ảnh

Khi xóa sản phẩm hoặc sửa sản phẩm (thay ảnh):
- Nếu dùng S3: Delete request được gửi tới S3
- Nếu dùng local: File được xóa từ wwwroot folder

## Cost Estimate (AWS)
- **S3 Storage**: $0.023/GB/month (trong 1 năm)
- **Data Transfer Out**: $0.09/GB (trafic từ S3 xuống client)
- Với ảnh sản phẩm thường ~100-500KB, chi phí rất thấp (< $1/month)

## Best Practices

✅ **Luôn dùng S3 cho production** - Tránh phụ thuộc vào storage của server
✅ **Sử dụng CDN** - CloudFront trước S3 để tăng tốc độ load
✅ **Backup ảnh** - S3 có redundancy nhưng vẫn nên backup định kỳ
✅ **Organize folder** - Dùng folder path phù hợp (vd: `product-images`, `user-avatars`)
✅ **Set lifecycle policy** - Xóa ảnh cũ sau 1 năm để tiết kiệm chi phí

## Tổng Kết

| Aspect | Local | S3 |
|--------|-------|-----|
| Truy cập từ local | ❌ | ✅ |
| Truy cập từ deployed | ❌ | ✅ |
| Chi phí | 💰 | 💵 (rất rẻ) |
| Scalability | ❌ | ✅ |
| Maintenance | 📝 | 🤖 |

Để giải quyết bài toán frontend local + backend deployed, **S3 là giải pháp tốt nhất!**
