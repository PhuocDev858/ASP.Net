# VNPay QR Code Payment Integration Guide

## Overview
Backend đã được cấu hình để thanh toán qua VNPay với QR Code. Khách hàng có thể scan QR code hoặc click vào payment link để thanh toán.

## Tính Năng

✅ **Tạo Payment URL** - Generate VNPay payment link  
✅ **QR Code Generation** - Tạo QR code từ payment URL  
✅ **Callback Handler** - Xử lý callback từ VNPay  
✅ **Transaction Status** - Kiểm tra trạng thái giao dịch  
✅ **Refund** - Hoàn tiền

## Setup VNPay

### Step 1: Đăng ký VNPay Merchant Account
1. Vào https://vnpayment.vn
2. Đăng ký merchant account
3. Chờ phê duyệt (thường 1-2 ngày)
4. Nhận **Tmn Code** và **Hash Secret**

### Step 2: Cấu Hình Sandbox (Test)
1. Vào VNPay sandbox: https://sandbox.vnpayment.vn
2. Đăng nhập bằng merchant account
3. Lấy test credentials

### Step 3: Cập Nhật Configuration

#### Local Development
Sửa `appsettings.Development.json`:
```json
{
  "VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paygate",
    "ReturnUrl": "http://localhost:5000/api/payment/vnpay-return",
    "ApiUrl": "https://sandbox.vnpayment.vn/merchant_webapi/merchant.html"
  }
}
```

#### Production (Render)
Thêm environment variables vào Render:
```
VNPay__TmnCode=YOUR_TMN_CODE
VNPay__HashSecret=YOUR_HASH_SECRET
VNPay__PaymentUrl=https://sandbox.vnpayment.vn/paygate
VNPay__ReturnUrl=https://yourdomain.com/api/payment/vnpay-return
VNPay__ApiUrl=https://sandbox.vnpayment.vn/merchant_webapi/merchant.html
```

**Production Mode** (khi đã sẵn sàng):
```
VNPay__PaymentUrl=https://pay.vnpayment.vn/paygate
VNPay__ApiUrl=https://api.vnpayment.vn/merchant_webapi/merchant.html
```

## API Endpoints

### 1. Tạo Payment & QR Code
**POST** `/api/payment/create-vnpay-payment`

**Request:**
```json
{
  "orderId": "ORD123456789",
  "amount": 500000,
  "orderInfo": "Thanh toán đơn hàng ORD123456789"
}
```

**Response:**
```json
{
  "paymentUrl": "https://sandbox.vnpayment.vn/paygate?vnp_Version=2.1.0&...",
  "qrCodeBase64": "iVBORw0KGgoAAAANSUhEUgAA...",
  "orderId": "ORD123456789",
  "amount": 500000,
  "message": "Tạo liên kết thanh toán thành công"
}
```

### 2. Callback từ VNPay
**GET** `/api/payment/vnpay-return?vnp_Amount=...&vnp_ResponseCode=...`

VNPay sẽ redirect tới endpoint này sau khi customer hoàn tất payment.
Backend tự động update payment status trong database.

### 3. Kiểm Tra Trạng Thái Thanh Toán
**POST** `/api/payment/check-status`

**Request:**
```json
{
  "orderId": "ORD123456789",
  "transactionDate": "2026-05-05T00:00:00"
}
```

**Response:**
```json
{
  "orderId": "ORD123456789",
  "status": "Success",
  "amount": 500000,
  "transactionId": "123456789",
  "paymentDate": "2026-05-05T10:30:00",
  "completedAt": "2026-05-05T10:31:00",
  "message": "Kiểm tra trạng thái thành công"
}
```

### 4. Hoàn Tiền
**POST** `/api/payment/refund`

**Request:**
```json
{
  "orderId": "ORD123456789",
  "amount": 500000,
  "transactionDate": "2026-05-05T00:00:00",
  "transactionNo": "123456789",
  "reason": "Khách hủy đơn"
}
```

**Response:**
```json
{
  "message": "Hoàn tiền thành công",
  "orderId": "ORD123456789",
  "amount": 500000,
  "status": "Refunded"
}
```

### 5. Lấy Thông Tin Thanh Toán
**GET** `/api/payment/{orderId}`

## Frontend Integration

### Ví Dụ React Component

```jsx
import { useState } from 'react'

export default function PaymentComponent({ orderId, amount }) {
  const [qrCode, setQrCode] = useState('')
  const [paymentUrl, setPaymentUrl] = useState('')
  const [loading, setLoading] = useState(false)

  const handleCreatePayment = async () => {
    setLoading(true)
    try {
      const response = await fetch('/api/payment/create-vnpay-payment', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId, amount })
      })
      const data = await response.json()
      
      if (data.qrCodeBase64) {
        setQrCode(`data:image/png;base64,${data.qrCodeBase64}`)
        setPaymentUrl(data.paymentUrl)
      }
    } catch (error) {
      console.error('Payment error:', error)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="payment-container">
      <h2>Thanh Toán Qua VNPay</h2>
      <button onClick={handleCreatePayment} disabled={loading}>
        {loading ? 'Đang tạo...' : 'Tạo Mã QR'}
      </button>

      {qrCode && (
        <div>
          <h3>Scan QR Code để thanh toán:</h3>
          <img src={qrCode} alt="VNPay QR Code" />
          
          <p>hoặc</p>
          
          <a href={paymentUrl} target="_blank" rel="noopener noreferrer">
            <button>Thanh Toán Qua Link</button>
          </a>
        </div>
      )}
    </div>
  )
}
```

## Trạng Thái Giao Dịch

| Status | Ý Nghĩa |
|--------|---------|
| Pending | Đang chờ thanh toán |
| Success | Thanh toán thành công |
| Failed | Thanh toán thất bại |
| Refunded | Đã hoàn tiền |

## Response Code VNPay

| Code | Ý Nghĩa |
|------|---------|
| 00 | Giao dịch thành công |
| 01 | Lỗi không xác định |
| 02 | Lỗi yêu cầu từ merchant |
| 04 | Giao dịch bị từ chối |
| 05 | Tài khoản khách hàng không hợp lệ |

## Troubleshooting

**Lỗi: "Chữ ký không hợp lệ"**
- Kiểm tra `TmnCode` và `HashSecret` có chính xác không
- Ensure hash calculation sử dụng correct secret

**QR Code không scan được**
- Kiểm tra `PaymentUrl` có chứa tất cả required parameters
- Thử tạo lại QR code

**Callback không nhận được**
- Verify `ReturnUrl` đã set đúng trong configuration
- Check firewall/ngrok nếu test locally

**Giao dịch không được update**
- Ensure database connection đang hoạt động
- Check logs để xem có error gì

## Best Practices

✅ **Luôn verify callback** - Kiểm tra signature trước khi update status  
✅ **Log transactions** - Log tất cả giao dịch cho audit trail  
✅ **Idempotent callbacks** - Handle duplicate callbacks  
✅ **Test sandbox trước** - Thử nghiệm kỹ trước production  
✅ **Secure credentials** - Không commit credentials vào git  

## Testing

### Test Payment Link
1. Tạo payment link qua API
2. Mở link trong browser
3. VNPay sẽ redirect tới sandbox payment page
4. Nhập test card: `9704198526191432198` (thẻ test VNPay)
5. OTP: `123456`
6. Xác nhận thanh toán

### Test Callback
1. Backend sẽ nhận callback từ VNPay
2. Update payment status tự động
3. Verify qua `check-status` endpoint

## Migration Steps

Nếu chuyển từ payment method khác:
1. Backup payment data
2. Migrate existing records
3. Update Order model nếu cần
4. Test toàn bộ payment flow
5. Deploy to production

## Chi Phí

- **Commission**: 0.5% - 2% (tùy gói)
- **Setup**: Miễn phí
- **Monthly**: Miễn phí nếu không có giao dịch
- Tham khảo giá tại: https://vnpayment.vn/pricing
