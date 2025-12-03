using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Models.Entities;
using QLDatVeMayBay.Models.ViewModels;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLDatVeMayBay.Controllers
{
    [Authorize]
    public class TheThanhToanController : Controller
    {
        private readonly QLDatVeMayBayContext _context;

        public TheThanhToanController(QLDatVeMayBayContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? showForm)
        {
            try
            {
                var tenDangNhap = User.Identity?.Name;
                var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

                if (nguoiDung == null)
                    return RedirectToAction("DangNhap", "TaiKhoan");

                var danhSach = await _context.TheThanhToan
                    .Where(t => t.NguoiDungId == nguoiDung.IDNguoiDung)
                    .ToListAsync();

                // ✅ Gán đúng kiểu nếu có showForm, ngược lại giữ null (ẩn form)
                LoaiTheLoaiVi? loai = null;
                if (showForm == "The")
                    loai = LoaiTheLoaiVi.TheNganHang;
                else if (showForm == "Vi")
                    loai = LoaiTheLoaiVi.ViDienTu;

                var vm = new TheThanhToanViewModel
                {
                    NguoiDungId = nguoiDung.IDNguoiDung,
                    Loai = loai, // ✅ null nếu không chọn gì, dùng để ẩn form
                    DanhSach = danhSach,
                    NgayLienKet = DateTime.Today
                };

                return View("~/Views/TheThanhToan/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["Debug"] = $"Lỗi Index: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTheNganHang(string SoThe, string TenTrenThe, string HieuLuc, string CVV)
        {
            var tenDangNhap = User.Identity?.Name;
            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);
            if (nguoiDung == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            // ========== VALIDATE CHẶT ==========

            // 1. Số thẻ: bắt buộc & CHỈ được là số
            if (string.IsNullOrWhiteSpace(SoThe))
                ModelState.AddModelError("SoThe", "Số thẻ không được để trống.");
            else if (!Regex.IsMatch(SoThe, @"^\d+$"))
                ModelState.AddModelError("SoThe", "Số thẻ chỉ được chứa chữ số, không được có chữ cái hoặc ký tự đặc biệt.");

            // 2. Tên in trên thẻ: bắt buộc
            if (string.IsNullOrWhiteSpace(TenTrenThe))
                ModelState.AddModelError("TenTrenThe", "Tên trên thẻ không được để trống.");

            // 3. CVV: bắt buộc, từ 3–4 số, không ký tự khác
            if (string.IsNullOrWhiteSpace(CVV))
                ModelState.AddModelError("CVV", "CVV không được để trống.");
            else if (!Regex.IsMatch(CVV, @"^\d{3,4}$"))
                ModelState.AddModelError("CVV", "CVV chỉ được phép từ 3 đến 4 chữ số, không chứa chữ cái hoặc ký tự đặc biệt.");

            // 4. Hạn dùng: bắt buộc, đúng định dạng dd/MM/yyyy
            DateTime expiry = DateTime.MinValue;   // ✅ khởi tạo giá trị mặc định

            if (string.IsNullOrWhiteSpace(HieuLuc))
            {
                ModelState.AddModelError("HieuLuc", "Ngày hiệu lực không được để trống.");
            }
            else if (!DateTime.TryParseExact(
                         HieuLuc,
                         "dd/MM/yyyy",
                         CultureInfo.InvariantCulture,
                         DateTimeStyles.None,
                         out expiry))
            {
                ModelState.AddModelError("HieuLuc", "Ngày hiệu lực phải có dạng dd/MM/yyyy.");
            }


            // Nếu có lỗi -> KHÔNG LƯU, quay lại trang Index, mở sẵn form thẻ
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Thông tin thẻ không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(Index), new { showForm = "The" });
            }

            // ========== HỢP LỆ MỚI CHO LƯU ==========

            var model = new TheThanhToan
            {
                Id = Guid.NewGuid().ToString(),
                NguoiDungId = nguoiDung.IDNguoiDung,
                Loai = LoaiTheLoaiVi.TheNganHang,
                SoThe = SoThe,
                TenTrenThe = TenTrenThe,
                // Lưu lại theo đúng format ngày/tháng/năm
                HieuLuc = expiry.ToString("dd/MM/yyyy"),
                CVV = CVV,
                NgayLienKet = DateTime.Now
            };

            _context.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm thẻ ngân hàng thành công!";
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        public async Task<IActionResult> CreateViDienTu(string TenVi, string EmailLienKet, string TenHienThi, string SoDienThoai)
        {
            var tenDangNhap = User.Identity?.Name;
            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

            if (nguoiDung == null)
                return RedirectToAction("DangNhap", "TaiKhoan");

            if (string.IsNullOrWhiteSpace(TenVi))
                ModelState.AddModelError("TenVi", "Tên ví không được để trống.");
            if (string.IsNullOrWhiteSpace(EmailLienKet))
                ModelState.AddModelError("EmailLienKet", "Email liên kết không được để trống.");
            if (string.IsNullOrWhiteSpace(TenHienThi))
                ModelState.AddModelError("TenHienThi", "Tên hiển thị không được để trống.");
            if (string.IsNullOrWhiteSpace(SoDienThoai))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại không được để trống.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin ví điện tử.";
                return RedirectToAction(nameof(Index), new { showForm = "Vi" });
            }

            var model = new TheThanhToan
            {
                Id = Guid.NewGuid().ToString(),
                NguoiDungId = nguoiDung.IDNguoiDung,
                Loai = LoaiTheLoaiVi.ViDienTu,
                TenVi = TenVi,
                EmailLienKet = EmailLienKet,
                TenHienThi = TenHienThi,
                SoDienThoai = SoDienThoai,
                NgayLienKet = DateTime.Now // ✅ Gán tự động ngày liên kết
            };

            _context.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm ví điện tử thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var model = await _context.TheThanhToan.FindAsync(id);
            if (model == null) return NotFound();

            var vm = new TheThanhToanViewModel
            {
                Id = model.Id,
                NguoiDungId = model.NguoiDungId,
                Loai = model.Loai,
                SoThe = model.SoThe,
                HieuLuc = model.HieuLuc,
                CVV = model.CVV,
                TenTrenThe = model.TenTrenThe,
                TenVi = model.TenVi,
                EmailLienKet = model.EmailLienKet,
                TenHienThi = model.TenHienThi,
                SoDienThoai = model.SoDienThoai,
                NgayLienKet = model.NgayLienKet
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TheThanhToanViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Debug"] = "Dữ liệu không hợp lệ";
                return View(vm);
            }

            var entity = await _context.TheThanhToan.FindAsync(vm.Id);
            if (entity == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin
            entity.Loai = vm.Loai.Value;
            entity.SoThe = vm.SoThe;
            entity.HieuLuc = vm.HieuLuc;
            entity.CVV = vm.CVV;
            entity.TenTrenThe = vm.TenTrenThe;
            entity.TenVi = vm.TenVi;
            entity.EmailLienKet = vm.EmailLienKet;
            entity.TenHienThi = vm.TenHienThi;
            entity.SoDienThoai = vm.SoDienThoai;
            entity.NgayLienKet = vm.NgayLienKet;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult GetChiTietThe(string id)
        {
            var the = _context.TheThanhToan.Find(id);
            if (the == null) return NotFound();

            return Json(new
            {
                soTaiKhoan = the.SoThe ?? the.TenVi,
                chuTaiKhoan = the.TenTrenThe ?? the.TenHienThi
            });
        }


        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var model = await _context.TheThanhToan.FindAsync(id);
            if (model == null) return NotFound();

            _context.TheThanhToan.Remove(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xoá thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}