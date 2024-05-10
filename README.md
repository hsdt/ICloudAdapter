# Môi trường DEV
## - Cài đặt Visual studio 2022
## - .NET 7

# Cấu hình Deployment
## - Cấu hình trong file Config/GetPhieuDonTiepPubSub.json
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
- `ApiHisUrl`: Đường dẫn của HIS
- `ApiPostPhieuDonTiep`: Api hàm đón tiếp của HIS (nhận dữ liệu phiếu đón tiếp từ Cloud), Method: Post
- `ApiHisToken`: Token key của HIS (thêm vào Header của Authorization)
- `CloudGetUrl`: Địa chỉ Api nhận dữ liệu phiếu đón tiếp (do Cloud cung cấp), Vd: `xxx-lichkham_list`
- `CloudToken`: Token truy cập dữ liệu vào Cloud (do Cloud cung cấp)
- `Channel`: Tên channel nhận dữ liệu phiếu đón tiếp (mặc định do Cloud cung cấp)
- `Lane`: Tên kênh lane nhận dữ liệu, khi cấu hình nhiều kênh (mặc định để trống)
