using System.ComponentModel.DataAnnotations;

namespace QLDatVeMayBay.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 6 đến 20 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái và số, không dấu")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ hoa, một chữ thường, một số và một ký tự đặc biệt")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("MatKhau", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "Số điện thoại phải là số và từ 9 đến 12 chữ số")]
        public string? SoDienThoai { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string? GioiTinh { get; set; } // "Nam", "Nữ", "Khác"

        [StringLength(20)]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "CCCD phải là số và từ 9 đến 12 chữ số")]
        [Required(ErrorMessage = "CCCD không được để trống")]
        public string? CCCD { get; set; }
        [Required(ErrorMessage = "Quốc tịch không được để trống")]
        [StringLength(50, ErrorMessage = "Quốc tịch tối đa 50 ký tự")]
        public string? QuocTich { get; set; }
    }
}