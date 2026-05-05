# Image Upload API Documentation

## Endpoints

### 1. Upload Image
**POST** `/api/products/upload-image`

Upload a single image file to the server.

**Request:**
- Content-Type: `multipart/form-data`
- Body: `file` (IFormFile)

**Example:**
```bash
curl -X POST http://localhost:5000/api/products/upload-image \
  -F "file=@image.jpg"
```

**Response (Success - 200 OK):**
```json
{
  "message": "Upload ảnh thành công",
  "imageUrl": "/uploads/images/guid_timestamp.jpg",
  "fileName": "guid_timestamp.jpg"
}
```

**Response (Error - 400 Bad Request):**
```json
{
  "message": "Kích thước file vượt quá giới hạn 5MB"
}
```

### 2. Delete Image
**DELETE** `/api/products/delete-image?imageUrl=/uploads/images/guid_timestamp.jpg`

Delete an image from the server.

**Parameters:**
- `imageUrl` (query string): The URL of the image to delete

**Example:**
```bash
curl -X DELETE "http://localhost:5000/api/products/delete-image?imageUrl=/uploads/images/guid_timestamp.jpg"
```

**Response (Success - 200 OK):**
```json
{
  "message": "Xóa ảnh thành công"
}
```

**Response (Not Found - 404):**
```json
{
  "message": "Ảnh không tồn tại"
}
```

## Configuration

Settings are located in `appsettings.json`:

```json
"FileUpload": {
  "UploadFolder": "uploads/images",
  "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".gif", ".webp" ],
  "MaxFileSize": 5242880,
  "MaxFileSizeMB": 5
}
```

## Usage Example with Product

### 1. Upload an image
```bash
POST /api/products/upload-image
Form-data: file=image.jpg
Response: { "imageUrl": "/uploads/images/guid_123.jpg" }
```

### 2. Create a product with the image
```bash
POST /api/products
Content-Type: application/json
{
  "productId": "P001",
  "productName": "Sample Product",
  "categoryId": "C001",
  "price": 100000,
  "description": "Sample Description",
  "stock": 50,
  "imageUrl": "/uploads/images/guid_123.jpg"
}
```

### 3. Update product image
```bash
PUT /api/products/P001
Content-Type: application/json
{
  "productName": "Sample Product",
  "categoryId": "C001",
  "price": 100000,
  "description": "Sample Description",
  "stock": 50,
  "imageUrl": "/uploads/images/guid_new.jpg"
}
```

## File Location

Uploaded files are stored in:
```
{ProjectRoot}/wwwroot/uploads/images/
```

The files are served as static files at:
```
http://localhost:5000/uploads/images/filename.jpg
```

## Constraints

- **Allowed formats**: .jpg, .jpeg, .png, .gif, .webp
- **Maximum file size**: 5 MB
- **Unique naming**: Files are named with GUID + timestamp to prevent conflicts
- **Access**: Static files are publicly accessible

## Error Handling

| Error | Status | Message |
|-------|--------|---------|
| Empty file | 400 | File không được trống |
| File too large | 400 | Kích thước file vượt quá giới hạn 5MB |
| Invalid extension | 400 | Định dạng file không được hỗ trợ |
| Missing imageUrl | 400 | URL ảnh không được trống |
| Image not found (delete) | 404 | Ảnh không tồn tại |
