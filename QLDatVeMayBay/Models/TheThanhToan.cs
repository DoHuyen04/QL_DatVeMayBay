using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatVeMayBay.Models.Entities
{
    public class TheThanhToan
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Người dùng là bắt buộc")]
        public int NguoiDungId { get; set; }

        [ForeignKey("NguoiDungId")]
        public NguoiDung? NguoiDung { get; set; }

        [Required(ErrorMessage = "Loại thẻ/ví là bắt buộc")]
        public LoaiTheLoaiVi Loai { get; set; }

        // 🎴 Thẻ ngân hàng (Loại = 0)
        public string? TenNganHang { get; set; }
        [StringLength(20)]
        public string? SoThe { get; set; }

        [StringLength(10)]
        public string? HieuLuc { get; set; }

        [StringLength(4)]
        public string? CVV { get; set; }

        [StringLength(100)]
        public string? TenTrenThe { get; set; }

        // 🧧 Ví điện tử (Loại = 1)
        [StringLength(50)]
        public string? TenVi { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? EmailLienKet { get; set; }

        public string? TenHienThi { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        public DateTime? NgayLienKet { get; set; }
        [NotMapped]
        public List<TheThanhToan> DanhSach { get; set; } = new();
    }

    public enum LoaiTheLoaiVi
    {
        TheNganHang = 0,
        ViDienTu = 1
    }
}
