# Deployment


1. Tải xuống bản phát hành mới nhất

- Link tại đây: https://github.com/hsdt/ICloudAdapter/releases
- Giải nén file tải về: CloudSyncDist.zip

2. Chạy với chế dộ Console
- Chuyển vào thư mục đã giải nén bước 1: `cd CloudSyncDist`
- Chạy ở chế độ console: `./CloudSync.exe run`

2. Đăng ký windows service, khởi động cùng hệ thống

- Mở CMD và chạy với quyền Administrator
- Lệnh đăng ký: `./CloudSync.exe install`
- Khởi động dịch vụ: `./CloudSync.exe start`

3. Truy cập Swagger/test
- Truy cập link: http://localhost:5002/swagger/index.html
- Thực hiện các hàm gửi dữ liệu test với license test.

# Cấu hình: NodeForwarderPubSub.json

```json
{
  "Enabled": true,
  "CloudGetUrl": "",
  "CloudToken": "",
  "NodeURL": "",
  "NodePubSubURL": "http://10.2.x.100:8000",
  "ForwardAuthKey": "token-abc-def-xyz-changeme",
  "Requests": [
    {
      "Topic": "api/{topic}/pending",
      "ExecuteApi": "his-api_list?lane=hs&msgToken={token}",
      "ForwardUrl": "https://httpbin.org/post",
      "ForwardAuthKey": ""
    }
  ]
}
```

Trong đó:
- Enabled: Khởi tạo dịch vụ chạy cùng hệ thống.
- NodeURL: Api service node (nếu cần thiết)
- NodePubSubURL: Pubsub service node (nếu cần thiết)
- CloudToken: Token được cấp phát với HUB Cloud, dùng để khởi tạo và ExecuteApi
- ForwardAuthKey: Access key của hệ thống được forward data tới, nếu không được thiết lập trong Requests sẽ lấy giá trị được cấu hình ngoài.

# Cấu hình trong file Config/GetPhieuDonTiepPubSub.json
```json
{
  "Enabled": true,
  "ApiHisUrl": "https://httpbin.org",
  "ApiPostPhieuDonTiep": "post",
  "ApiHisToken": "changeme",
  "CloudGetUrl": "",
  "CloudToken": "",
  "Channel": "lichkham/pending",
  "Lane": ""
}
```

Trong đó:
- `ApiHisUrl`: Đường dẫn của HIS
- `ApiPostPhieuDonTiep`: Api hàm đón tiếp của HIS (nhận dữ liệu phiếu đón tiếp từ Cloud), Method: Post
- `ApiHisToken`: Token key của HIS (thêm vào Header của Authorization)
- `CloudGetUrl`: Địa chỉ Api nhận dữ liệu phiếu đón tiếp (do Cloud cung cấp), Vd: `xxx-lichkham_list`
- `CloudToken`: Token truy cập dữ liệu vào Cloud (do Cloud cung cấp)
- `Channel`: Tên channel nhận dữ liệu phiếu đón tiếp (mặc định do Cloud cung cấp)
- `Lane`: Tên kênh lane nhận dữ liệu, khi cấu hình nhiều kênh (mặc định để trống)

# Thứ tự các cấu hình được ưu tiên:

1. Environment Variables
2. Process config: ./Config/GetPhieuDonTiepPubSub.json
3. App config: ./appsettings.json & ./appsettings.Production.json

Trong quá trình phát triển, cấu hình có thể được đặt trong file: `appsettings.Development.json`

  
# Build:

### + Windows: 
```bash
dotnet publish -r win-x64 -c Release --self-contained -o CloudSyncDist
```

### + Linux: 
```bash
dotnet publish -r linux-64 -c Release --self-contained -o CloudSyncDist
```

# Dev Requirement

- Cài đặt Visual studio 2022
- .NET 7
