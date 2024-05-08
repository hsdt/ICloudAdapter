using System;

namespace CloudSync.Process.Dto
{
    public class PhieuDonTiepDto
    {
        public string Domain { get; set; }
        public Guid Id { get; set; }
        public string CCCD { get; set; }
        public string BHYT { get; set; }
        public string HoTen { get; set; }
        public string MaDangKy { get; set; }
        public string PhongKham { get; set; }
        public DateTime? ThoiGianDatLich { get; set; }
        public DateTime ThoiGianLichKham { get; set; }
        public string LoaiKham { get; set; }
        public string NoiDungKham { get; set; }
        public bool KhamCachLy { get; set; }
        public string GiaTriHuong { get; set; }
        public string DanToc { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string QuocTich { get; set; }
        public byte GioiTinh { get; set; }

        public string ChanDoan { get; set; }
        public string NoiLamViec { get; set; }
        public string MaSoThue { get; set; }
        public string DiaChiThe { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string NgheNghiep { get; set; }
        public long Updated { get; set; }
        public NoiODto NoiO { get; set; }
        public HanTheDto HanThe { get; set; }
    }

    public class NoiODto
    {
        public string TinhThanh { get; set; }
        public string PhuongXa { get; set; }
        public string ThonXomSoNha { get; set; }
        public string QuanHuyen { get; set; }
    }
    public class HanTheDto
    {
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
    }
}
