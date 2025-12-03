using System.ComponentModel.DataAnnotations;

namespace QLDatVeMayBay.ViewModels.TaiKhoan
{
    public class ChinhSuaHoSoViewModel
    {
        public int IDNguoiDung { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [RegularExpression(@"^[^@\s]+@gmail\.com$",
            ErrorMessage = "Email phải có đuôi @gmail.com")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^[0-9]{10,11}$",
            ErrorMessage = "Số điện thoại phải là 10-11 số và không chứa chữ cái")]
        [StringLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giới tính không được để trống")]
        [StringLength(10)]
        public string GioiTinh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quốc tịch không được để trống")]
        [StringLength(50)]
        public string QuocTich { get; set; } = string.Empty;

        [Required(ErrorMessage = "CCCD không được để trống")]
        [RegularExpression(@"^[0-9]+$",
            ErrorMessage = "CCCD chỉ được chứa số và không chứa chữ cái")]
        [StringLength(20)]
        public string CCCD { get; set; } = string.Empty;
    }
}
