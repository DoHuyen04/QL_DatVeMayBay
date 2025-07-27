using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Models;
using QLDatVeMayBay.Helper;
using QLDatVeMayBay.Services;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using QLDatVeMayBay.Models.Entities;

namespace QLDatVeMayBay.Controllers
{
    public class DatVeController : Controller
    {
        private readonly QLDatVeMayBayContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public DatVeController(QLDatVeMayBayContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<IActionResult> ChonGhe(int idChuyenBay)
        {
            var chuyenBay = await _context.ChuyenBay
                .Include(cb => cb.MayBay).ThenInclude(mb => mb.LoaiMayBay)
                .FirstOrDefaultAsync(cb => cb.IDChuyenBay == idChuyenBay);

            var gheDaDat = await _context.VeMayBay
                .Where(v => v.IDChuyenBay == idChuyenBay)
                .Select(v => v.IDGhe)
                .ToListAsync();

            var model = new DatGhe
            {
                IDChuyenBay = idChuyenBay,
                TongSoGhe = chuyenBay.MayBay.LoaiMayBay.TongSoGhe,
                GheDaDat = gheDaDat
            };

            return View(model);
        }

        public IActionResult XacNhanVe(int idChuyenBay, int idGhe)
        {
            var idNguoiDung = HttpContext.Session.GetInt32("IDNguoiDung");
            if (idNguoiDung == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var nguoiDung = _context.NguoiDung.FirstOrDefault(x => x.IDNguoiDung == idNguoiDung);
            var chuyenBay = _context.ChuyenBay.Include(x => x.MayBay).FirstOrDefault(x => x.IDChuyenBay == idChuyenBay);
            var sanBayDi = _context.SanBay.FirstOrDefault(x => x.IDSanBay == chuyenBay.SanBayDi);
            var sanBayDen = _context.SanBay.FirstOrDefault(x => x.IDSanBay == chuyenBay.SanBayDen);

            // ✅ Lấy danh sách thẻ/ví đã lưu của người dùng
            var danhSachThe = _context.TheThanhToan
                .Where(t => t.NguoiDungId == idNguoiDung.Value)
                .ToList();

            var thongTinVe = new ThongTinVe
            {
                IDNguoiDung = nguoiDung.IDNguoiDung,
                HoTen = nguoiDung.HoTen,
                GioiTinh = nguoiDung.GioiTinh,
                IDChuyenBay = chuyenBay.IDChuyenBay,
                TenHangHK = chuyenBay.MayBay.TenHangHK,
                GioCatCanh = chuyenBay.GioCatCanh,
                GioHaCanh = chuyenBay.GioHaCanh,
                SanBayDi = sanBayDi.IDSanBay,
                SanBayDen = sanBayDen.IDSanBay,
                TenSanBayDi = sanBayDi.TenSanBay,
                TenSanBayDen = sanBayDen.TenSanBay,
                IDGhe = idGhe.ToString(),
                GiaVe = chuyenBay.GiaVe,

                // ✅ Gán danh sách thẻ vào ViewModel
                DanhSachThe = danhSachThe
            };

            return View("XacNhanVe", thongTinVe);
        }


        [HttpGet]
        public IActionResult ThanhToan(int idChuyenBay, int idGhe)
        {
            var idNguoiDung = HttpContext.Session.GetInt32("IDNguoiDung");
            if (idNguoiDung == null) return RedirectToAction("DangNhap", "TaiKhoan");
            if (idGhe <= 0)
            {
                TempData["LoiChonGhe"] = "Bạn chưa chọn ghế nào!";
                return RedirectToAction("ChonGhe", new { id = idChuyenBay });
            }
            // Lấy danh sách ghế đã đặt của chuyến bay
            var gheDaDat = _context.VeMayBay
                .Where(v => v.IDChuyenBay == idChuyenBay)
                .Select(v => v.IDGhe)
                .ToList();

            if (gheDaDat.Contains(idGhe))
            {
                TempData["LoiChonGhe"] = $"Ghế G{idGhe} đã được người khác đặt. Vui lòng chọn ghế khác.";
                return RedirectToAction("ChonGhe", new { id = idChuyenBay });
            }
            var chuyenBay = _context.ChuyenBay.Find(idChuyenBay);
            if (chuyenBay == null)
            {
                TempData["LoiChonGhe"] = "Chuyến bay không tồn tại.";
                return RedirectToAction("ChonGhe", new { id = idChuyenBay });
            }

            var giaVe = chuyenBay?.GiaVe ?? 0;
            var danhSachThe = _context.TheThanhToan
      .Where(t => t.NguoiDungId == idNguoiDung)
      .ToList();
            var model = new ThongTinThanhToan
            {
                Ve = new VeMayBay
                {
                    IDNguoiDung = idNguoiDung.Value,
                    IDChuyenBay = idChuyenBay,
                    IDGhe = idGhe,
                    ThoiGianDat = DateTime.Now,
                    TrangThaiVe = "Chưa thanh toán"
                },
                SoTien = giaVe,
                DanhSachThe = danhSachThe
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ThanhToan(ThongTinThanhToan model)
        {
            var nguoiDung = await _context.NguoiDung.FindAsync(model.Ve.IDNguoiDung);
            if (nguoiDung == null) return NotFound();
            model.Ve.TheThanhToanId = model.SelectedTheId;

            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expires", DateTime.Now.AddMinutes(2).ToString());
            HttpContext.Session.Set("VeTemp", JsonSerializer.SerializeToUtf8Bytes(model));

            // Tạo nội dung HTML email xác nhận OTP
            string htmlEmail = $@"
<div style='font-family:Segoe UI, sans-serif; background-color:#ffffff; padding:30px; border:1px solid #e0e0e0; border-radius:10px; max-width:600px; margin:auto;'>
    <div style='text-align:center; margin-bottom:20px;'>
        <h2 style='color:#0d6efd; margin-bottom:5px;'>Xác nhận thanh toán vé máy bay</h2>
        <p style='font-size:14px; color:#6c757d;'>QLĐặtVé Máy Bay</p>
    </div>

    <p>Xin chào <strong>{model.ChuTaiKhoan}</strong>,</p>

    <p style='font-size:15px; color:#333;'>Bạn đang thực hiện thanh toán vé trên hệ thống <strong>QLĐặtVé Máy Bay</strong>.</p>

    <p style='margin-top:20px; font-weight:500;'>Mã xác nhận thanh toán (OTP) của bạn:</p>
    <div style='font-size:32px; font-weight:bold; letter-spacing:6px; color:#198754; margin:20px 0; text-align:center;'>{otp}</div>

    <p style='color:#555;'>⚠️ <strong>Lưu ý:</strong> Không chia sẻ mã xác nhận với bất kỳ ai. Mã sẽ hết hạn sau <strong>2 phút</strong> kể từ khi được gửi.</p>

    <p style='margin-top:30px; font-size:14px; color:#888;'>Nếu bạn không thực hiện thanh toán, vui lòng bỏ qua email này.</p>

    <hr style='margin:30px 0;' />

    <p style='text-align:center; font-size:12px; color:#999;'>© {DateTime.Now.Year} QLĐặtVé Máy Bay. Mọi quyền được bảo lưu.</p>
</div>";

            // Gửi email HTML
            await _emailService.SendEmailAsync(nguoiDung.Email, "Xác nhận thanh toán vé máy bay", htmlEmail);
            model.DanhSachThe = _context.TheThanhToan
    .Where(t => t.NguoiDungId == model.Ve.IDNguoiDung)
    .ToList();

            return View("NhapOTP", model);
        }

        [HttpPost]
        public async Task<IActionResult> KiemTraOTP(ThongTinThanhToan model)
        {


            // Lấy mã OTP và thời gian hết hạn từ session
            var otp = HttpContext.Session.GetString("OTP");
            var otpExpStr = HttpContext.Session.GetString("OTP_Expires");

            if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(otpExpStr))
            {
                ModelState.AddModelError("", "Phiên OTP không hợp lệ. Vui lòng thử lại.");
                return View("NhapOTP", model);
            }

            var otpExp = DateTime.Parse(otpExpStr);

            if (model.MaOTP != otp || DateTime.Now > otpExp)
            {
                ModelState.AddModelError("MaOTP", "OTP không hợp lệ hoặc đã hết hạn.");
                return View("NhapOTP", model);
            }

            // Deserialize dữ liệu ThongTinThanhToan từ session
            var veData = HttpContext.Session.Get("VeTemp");
            var fullModel = JsonSerializer.Deserialize<ThongTinThanhToan>(veData);
            var ve = fullModel.Ve;
            ve.TrangThaiVe = "Đã đặt";
            ve.TheThanhToanId = fullModel.SelectedTheId;
            // Lưu vé vào DB
            _context.VeMayBay.Add(ve);
            await _context.SaveChangesAsync();


            // Lưu thông tin thanh toán
            var thanhToan = new ThanhToan
            {
                IDVe = ve.IDVe,
                SoTien = fullModel.SoTien,
                PhuongThuc = fullModel.PhuongThuc,
                ThoiGianGiaoDich = DateTime.Now,
                TrangThaiThanhToan = "Thành công"
            };
            _context.ThanhToan.Add(thanhToan);
            await _context.SaveChangesAsync();

            // Lấy thông tin chuyến bay
            var chuyenBay = await _context.ChuyenBay
                .Include(cb => cb.MayBay)
                .Include(cb => cb.SanBayDiInfo)
                .Include(cb => cb.SanBayDenInfo)
                .FirstOrDefaultAsync(cb => cb.IDChuyenBay == ve.IDChuyenBay);

            // Lấy thông tin người dùng
            var nguoiDung = await _context.NguoiDung.FindAsync(ve.IDNguoiDung);
            if (nguoiDung == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            var the = await _context.TheThanhToan.FindAsync(fullModel.SelectedTheId);
            if (the == null)
            {
                ModelState.AddModelError("", "Thẻ thanh toán không tồn tại.");
                return View("NhapOTP", fullModel);
            }
            string hinhThuc = the?.Loai == LoaiTheLoaiVi.TheNganHang ? "Thẻ ngân hàng" : "Ví điện tử";
            string tenChu = the?.Loai == LoaiTheLoaiVi.TheNganHang ? the.TenNganHang : the.TenVi;

            // Tạo mã QR
            var qrText = $"""
👤 Mã KH: {ve.IDNguoiDung}
✈ Chuyến bay: {ve.IDChuyenBay}
📍 {chuyenBay?.SanBayDiInfo?.TenSanBay} → {chuyenBay?.SanBayDenInfo?.TenSanBay}
🛫 Cất cánh: {chuyenBay?.GioCatCanh:dd/MM/yyyy HH:mm}
🛬 Hạ cánh: {chuyenBay?.GioHaCanh:dd/MM/yyyy HH:mm}
🛩 Loại máy bay: {chuyenBay?.MayBay?.LoaiMayBay}
💺 Ghế: G{ve.IDGhe} | Hạng: {ve.HangGhe ?? "Không rõ"}
🎫 Loại vé: {ve.LoaiVe ?? "Thường"}
🆔 Mã vé: {ve.IDVe}
""";

            var qrBase64 = QRCodeHelper.GenerateQRCodeBase64(qrText);

            // Gửi email xác nhận
            string emailHtml = $"""
    <h2>✅ Đặt vé thành công!</h2>
    <p>Chào <strong>{nguoiDung.HoTen}</strong>,</p>
    <p>Bạn đã đặt vé thành công. Dưới đây là thông tin vé:</p>
    <ul>
        <li><strong>Chuyến bay:</strong> {ve.IDChuyenBay} - {chuyenBay?.MayBay?.TenHangHK}</li>
        <li><strong>Ghế:</strong> G{ve.IDGhe} | Hạng: {ve.HangGhe ?? "Không rõ"}</li>
        <li><strong>Loại vé:</strong> {ve.LoaiVe ?? "Thường"}</li>
        <li><strong>Điểm đi:</strong> {chuyenBay?.SanBayDiInfo?.TenSanBay}</li>
        <li><strong>Điểm đến:</strong> {chuyenBay?.SanBayDenInfo?.TenSanBay}</li>
        <li><strong>Cất cánh:</strong> {chuyenBay?.GioCatCanh:dd/MM/yyyy HH:mm}</li>
        <li><strong>Hạ cánh:</strong> {chuyenBay?.GioHaCanh:dd/MM/yyyy HH:mm}</li>
    </ul>
    <p><strong>Mã QR của bạn:</strong></p>
    <img src='data:image/png;base64,{qrBase64}' width='220' />
    """;

            await _emailService.SendEmailAsync(
        nguoiDung.Email,
        "✅ Xác nhận đặt vé thành công",
        emailHtml
    );

            // Gán dữ liệu cho ViewBag
            ViewBag.QRBase64 = qrBase64;
            ViewBag.Ve = ve;
            ViewBag.ThanhToan = thanhToan;
            ViewBag.ChuyenBay = chuyenBay;
            ViewBag.NguoiDung = nguoiDung;

            return View("ThanhToanThanhCong", fullModel);
        }

        [HttpGet]
        public IActionResult NhapOTP()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GuiLaiOTP()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuiLaiOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ThongBao"] = "Vui lòng nhập email hợp lệ.";
                return RedirectToAction("NhapOTP");
            }

            // Tìm người dùng
            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(n => n.Email == email);
            if (nguoiDung == null)
            {
                TempData["ThongBao"] = "Không tìm thấy người dùng với email này.";
                return RedirectToAction("NhapOTP");
            }

            // Tạo OTP mới
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expires", DateTime.UtcNow.AddMinutes(2).ToString("O"));

            // Gửi email
            string emailHtml = $"""
        <h2>Mã OTP mới</h2>
        <p>Mã xác nhận thanh toán của bạn là:</p>
        <div style='font-size:28px;font-weight:bold;color:#198754'>{otp}</div>
        <p>Mã có hiệu lực trong 2 phút.</p>
        """;

            await _emailService.SendEmailAsync(
                nguoiDung.Email,
                "Mã OTP thanh toán mới",
                emailHtml
            );

            TempData["ThongBao"] = "Mã OTP mới đã được gửi vào email.";
            return RedirectToAction("NhapOTP");
        }

    }
}