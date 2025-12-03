using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatVeMayBay.Models
{
    public class NguoiDung
    {
        [Key]
        public int IDNguoiDung { get; set; }

        [Required]
        [ForeignKey("TaiKhoan")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [RegularExpression(@"^[^@\s]+@gmail\.com$",
            ErrorMessage = "Email phải có đuôi @gmail.com")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        [RegularExpression(@"^[0-9]{10,11}$",
            ErrorMessage = "Số điện thoại phải là 10-11 số và không chứa chữ cái")]
        public string? SoDienThoai { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(50)]
        public string? QuocTich { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^[0-9]+$",
            ErrorMessage = "CCCD chỉ được chứa số và không có chữ cái")]
        public string? CCCD { get; set; }

        // ✅ Navigation đến tài khoản
        public TaiKhoan? TaiKhoan { get; set; }

        // ✅ Navigation đến vé máy bay đã đặt
        public List<VeMayBay> VeMayBays { get; set; } = new List<VeMayBay>();
    }
}
