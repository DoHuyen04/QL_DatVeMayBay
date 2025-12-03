// Controllers/NguoiDungController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Models;
using System.Drawing;
using System.IO;
using System.Text;

namespace QLDatVeMayBay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NguoiDungController : Controller
    {
        private readonly QLDatVeMayBayContext _context;
        private const int PageSize = 10;

        public NguoiDungController(QLDatVeMayBayContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string tuKhoa, string trangThai, string vaiTro, int page = 1)
        {
            var query = _context.NguoiDung
                .Include(n => n.TaiKhoan)
                .Include(n => n.VeMayBays)
                    .ThenInclude(v => v.ChuyenBay)
                .Include(n => n.VeMayBays)
                    .ThenInclude(v => v.Ghe)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.ToLower();
                query = query.Where(n => n.HoTen.ToLower().Contains(tuKhoa)
                                      || n.Email.ToLower().Contains(tuKhoa)
                                      || n.SoDienThoai.Contains(tuKhoa));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(n => n.TaiKhoan.TrangThaiTK == trangThai);
            }

            if (!string.IsNullOrEmpty(vaiTro))
            {
                query = query.Where(n => n.TaiKhoan.VaiTro == vaiTro);
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            var nguoiDungs = await query
                .OrderByDescending(n => n.TenDangNhap)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.VaiTro = vaiTro;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Text = "🟢 Hoạt động", Value = "HoatDong", Selected = trangThai == "HoatDong" },
                new SelectListItem { Text = "🔴 Bị khóa", Value = "BiKhoa", Selected = trangThai == "BiKhoa" }
            };

            ViewBag.VaiTroList = new List<SelectListItem>
            {
                new SelectListItem { Text = "🛡️ Admin", Value = "Admin", Selected = vaiTro == "Admin" },
                new SelectListItem { Text = "👤 Khách hàng", Value = "KhachHang", Selected = vaiTro == "KhachHang" }
            };

            return View(nguoiDungs);
        }

        [HttpPost]
        public async Task<IActionResult> DoiTrangThai(string tenDangNhap)
        {
            var taiKhoan = await _context.TaiKhoan.FindAsync(tenDangNhap);
            if (taiKhoan == null) return NotFound();

            taiKhoan.TrangThaiTK = taiKhoan.TrangThaiTK == "HoatDong" ? "BiKhoa" : "HoatDong";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: /NguoiDung/ChiTiet/tenDangNhap
        public async Task<IActionResult> ChiTiet(string tenDangNhap)
        {
            if (string.IsNullOrEmpty(tenDangNhap)) return NotFound();

            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(n => n.TenDangNhap == tenDangNhap);
            if (nguoiDung == null) return NotFound();

            var taiKhoan = await _context.TaiKhoan.FindAsync(tenDangNhap);
            ViewBag.TrangThaiTK = taiKhoan?.TrangThaiTK ?? "";

            var lichSuVe = await _context.VeMayBay
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .Where(v => v.IDNguoiDung == nguoiDung.IDNguoiDung)
                .OrderByDescending(v => v.ThoiGianDat)
                .ToListAsync();

            ViewBag.LichSuVe = lichSuVe;

            return View(nguoiDung);
        }

        [HttpGet]
        public async Task<IActionResult> XacNhanXoa(string tenDangNhap)
        {
            if (string.IsNullOrEmpty(tenDangNhap)) return NotFound();

            var nguoiDung = await _context.NguoiDung
                .Include(n => n.TaiKhoan)
                .FirstOrDefaultAsync(n => n.TenDangNhap == tenDangNhap);

            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(string tenDangNhap)
        {
            var nguoiDung = await _context.NguoiDung.FindAsync(tenDangNhap);
            var taiKhoan = await _context.TaiKhoan.FindAsync(tenDangNhap);

            if (nguoiDung == null || taiKhoan == null)
                return NotFound();

            _context.NguoiDung.Remove(nguoiDung);
            _context.TaiKhoan.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var nguoiDung = await _context.NguoiDung
                .Include(n => n.TaiKhoan)
                .FirstOrDefaultAsync(n => n.IDNguoiDung == id);

            if (nguoiDung == null) return NotFound();

            return View(nguoiDung);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NguoiDung model, string VaiTro)
        {
            var nguoiDung = await _context.NguoiDung
                .Include(n => n.TaiKhoan)
                .FirstOrDefaultAsync(n => n.IDNguoiDung == model.IDNguoiDung);

            if (nguoiDung == null) return NotFound();

            if (ModelState.IsValid)
            {
                nguoiDung.HoTen = model.HoTen;
                nguoiDung.Email = model.Email;
                nguoiDung.SoDienThoai = model.SoDienThoai;
                nguoiDung.GioiTinh = model.GioiTinh;
                nguoiDung.QuocTich = model.QuocTich;
                nguoiDung.CCCD = model.CCCD;

                if (User.IsInRole("Admin") && nguoiDung.TaiKhoan != null)
                {
                    nguoiDung.TaiKhoan.VaiTro = VaiTro;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> XuatExcel(string tuKhoa, string trangThai, string vaiTro)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var query = _context.NguoiDung.Include(n => n.TaiKhoan).AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.ToLower();
                query = query.Where(n => n.HoTen.ToLower().Contains(tuKhoa)
                                      || n.Email.ToLower().Contains(tuKhoa)
                                      || n.SoDienThoai.Contains(tuKhoa));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(n => n.TaiKhoan.TrangThaiTK == trangThai);
            }

            if (!string.IsNullOrEmpty(vaiTro))
            {
                query = query.Where(n => n.TaiKhoan.VaiTro == vaiTro);
            }

            var nguoiDungs = await query.ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("NguoiDung");

            worksheet.Cells["A1"].Value = "Tên đăng nhập";
            worksheet.Cells["B1"].Value = "Họ tên";
            worksheet.Cells["C1"].Value = "Email";
            worksheet.Cells["D1"].Value = "SĐT";
            worksheet.Cells["E1"].Value = "Giới tính";
            worksheet.Cells["F1"].Value = "Vai trò";
            worksheet.Cells["G1"].Value = "Trạng thái";

            using (var range = worksheet.Cells["A1:G1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            int row = 2;
            foreach (var n in nguoiDungs)
            {
                worksheet.Cells[row, 1].Value = n.TenDangNhap;
                worksheet.Cells[row, 2].Value = n.HoTen;
                worksheet.Cells[row, 3].Value = n.Email;
                worksheet.Cells[row, 4].Value = n.SoDienThoai;
                worksheet.Cells[row, 5].Value = n.GioiTinh;
                worksheet.Cells[row, 6].Value = n.TaiKhoan?.VaiTro ?? "";
                worksheet.Cells[row, 7].Value = n.TaiKhoan?.TrangThaiTK ?? "";
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = "DanhSachNguoiDung.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(stream, contentType, fileName);
        }
    }
}