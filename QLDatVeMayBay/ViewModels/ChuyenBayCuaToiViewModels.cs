using System;

namespace QLDatVeMayBay.Models.ViewModels
{
    public class ChuyenBayCuaToiViewModels
    {
        public int IDVe { get; set; }
        public int  MaChuyenBay { get; set; }
        public DateTime GioCatCanh { get; set; }
        public DateTime GioHaCanh { get; set; }
        public string SanBayDi { get; set; }
        public string SanBayDen { get; set; }
        public string TenMayBay { get; set; }
        public string HangGhe { get; set; }
        public string LoaiVe { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public string TrangThaiVe { get; set; }
        public string TinhTrangChuyenBay { get; set; }

        public string TrangThaiHienThi =>
            TrangThaiThanhToan == "Đã hoàn" ? "💸 Đã hoàn tiền" :
            TrangThaiThanhToan == "Đã hủy" || TrangThaiVe == "Đã hủy" ? "❌ Đã hủy" :
            GioHaCanh < DateTime.Now ? "✅ Đã bay" : "🟠 Chưa bay";
    }
}