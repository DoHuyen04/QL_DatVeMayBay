                                            using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Models;
using OfficeOpenXml;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using QLDatVeMayBay.Models.ViewModels;

namespace QLDatVeMayBay.Controllers
{
    [Authorize(Roles = "KhachHang")]
    public class VeMayBayController : Controller
    {
        private readonly QLDatVeMayBayContext _context;
        private const int PageSize = 10;

        public VeMayBayController(QLDatVeMayBayContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string tuKhoa, string trangThai, int? idChuyenBay, string hangGhe, DateTime? ngayDat, int page = 1)
        {
            var query = _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.ToLower();
                query = query.Where(v => v.NguoiDung.HoTen.ToLower().Contains(tuKhoa) ||
                                         v.NguoiDung.Email.ToLower().Contains(tuKhoa) ||
                                         v.ChuyenBay.IDChuyenBay.ToString().Contains(tuKhoa));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(v => v.TrangThaiVe == trangThai);
            }

            if (idChuyenBay.HasValue)
            {
                query = query.Where(v => v.IDChuyenBay == idChuyenBay);
            }

            if (!string.IsNullOrEmpty(hangGhe))
            {
                query = query.Where(v => v.Ghe.HangGhe == hangGhe);
            }

            if (ngayDat.HasValue)
            {
                var dateOnly = ngayDat.Value.Date;
                query = query.Where(v => v.ThoiGianDat.Date == dateOnly);
            }

            int totalItems = await query.CountAsync();

            var danhSachVe = await query
                .OrderByDescending(v => v.ThoiGianDat)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.IDChuyenBay = idChuyenBay;
            ViewBag.HangGhe = hangGhe;
            ViewBag.NgayDat = ngayDat?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);

            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Text = "✅ Đã thanh toán", Value = "Đã thanh toán" },
                new SelectListItem { Text = "⌛ Chưa thanh toán", Value = "Chưa thanh toán" },
                new SelectListItem { Text = "❌ Đã huỷ", Value = "Đã huỷ" }
            };

            ViewBag.ChuyenBayList = _context.ChuyenBay
                .Include(cb => cb.MayBay)
                .Select(cb => new SelectListItem
                {
                    Value = cb.IDChuyenBay.ToString(),
                    Text = cb.IDChuyenBay + " - " + cb.MayBay.TenHangHK
                }).ToList();

            ViewBag.HangGheList = await _context.GheNgoi
                .Select(g => g.HangGhe)
                .Distinct()
                .ToListAsync();

            return View(danhSachVe);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var ve = await _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .FirstOrDefaultAsync(v => v.IDVe == id);

            if (ve == null) return NotFound();
            return View(ve);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ve = await _context.VeMayBay.FindAsync(id);
            if (ve == null) return NotFound();

            ViewBag.TrangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Text = "✅ Đã thanh toán", Value = "Đã thanh toán" },
                new SelectListItem { Text = "⌛ Chưa thanh toán", Value = "Chưa thanh toán" },
                new SelectListItem { Text = "❌ Đã huỷ", Value = "Đã huỷ" }
            };

            return View(ve);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string trangThaiVe)
        {
            var ve = await _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .FirstOrDefaultAsync(v => v.IDVe == id);

            if (ve == null) return NotFound();

            if (string.IsNullOrEmpty(trangThaiVe))
            {
                ModelState.AddModelError("trangThaiVe", "Vui lòng chọn trạng thái.");
            }

            // Trạng thái hợp lệ để Admin chỉnh sửa
            var validStatuses = new List<string> { "Đã thanh toán", "Chưa thanh toán", "Đã huỷ" };
            if (!validStatuses.Contains(trangThaiVe))
            {
                ModelState.AddModelError("trangThaiVe", "Trạng thái không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TrangThaiList = validStatuses.Select(s => new SelectListItem
                {
                    Text = s,
                    Value = s,
                    Selected = s == ve.TrangThaiVe
                }).ToList();

                return View(ve);
            }

            ve.TrangThaiVe = trangThaiVe;
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Cập nhật trạng thái vé thành công.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> GuiEmail(int id)
        {
            var ve = await _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .FirstOrDefaultAsync(v => v.IDVe == id);

            if (ve == null) return NotFound();

            var message = new MailMessage("your_email@example.com", ve.NguoiDung.Email)
            {
                Subject = "Thông tin vé máy bay của bạn",
                Body = $"Chào {ve.NguoiDung.HoTen},\n\nĐây là thông tin vé của bạn: Mã vé: {ve.IDVe}, Chuyến bay: {ve.IDChuyenBay}, Ghế: {ve.Ghe.HangGhe}, Trạng thái: {ve.TrangThaiVe}, Thời gian đặt: {ve.ThoiGianDat}",
            };

            using var smtp = new SmtpClient("smtp.yourserver.com")
            {
                Credentials = new NetworkCredential("your_email@example.com", "your_password"),
                EnableSsl = true
            };
            await smtp.SendMailAsync(message);

            TempData["Message"] = "Đã gửi email vé thành công.";
            return RedirectToAction("ChiTiet", new { id });
        }

        public async Task<IActionResult> XuatPDF(int id)
        {
            var ve = await _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .FirstOrDefaultAsync(v => v.IDVe == id);

            if (ve == null) return NotFound();

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            var a5Size = new PageSize(420, 595); // Kích thước A5 đơn vị point (1pt = 1/72 inch)
            var doc = new Document(pdf, a5Size);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            doc.Add(new Paragraph("VÉ MÁY BAY").SetFontSize(20).SetFont(boldFont).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            doc.Add(new Paragraph($"Mã vé: {ve.IDVe}"));
            doc.Add(new Paragraph($"Họ tên: {ve.NguoiDung?.HoTen}"));
            doc.Add(new Paragraph($"Chuyến bay: {ve.ChuyenBay?.IDChuyenBay}"));
            doc.Add(new Paragraph($"Hãng: {ve.ChuyenBay?.MayBay?.TenHangHK}"));
            doc.Add(new Paragraph($"Ghế: {ve.Ghe?.HangGhe}"));
            doc.Add(new Paragraph($"Trạng thái: {ve.TrangThaiVe}"));
            doc.Add(new Paragraph($"Thời gian đặt: {ve.ThoiGianDat:dd/MM/yyyy HH:mm}"));

            doc.Close();
            stream.Position = 0;

            return File(stream, "application/pdf", $"Ve_{ve.IDVe}.pdf");
        }

        public async Task<IActionResult> XacNhanXoa(int id)
        {
            var ve = await _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .FirstOrDefaultAsync(v => v.IDVe == id);

            if (ve == null) return NotFound();
            return View(ve);
        }

        [HttpPost, ActionName("XacNhanXoa")]
        public async Task<IActionResult> XoaConfirmed(int id)
        {
            var ve = await _context.VeMayBay.FindAsync(id);
            if (ve == null) return NotFound();

            _context.VeMayBay.Remove(ve);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Xoá vé thành công.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> XuatExcel(string tuKhoa, string trangThai, int? idChuyenBay, string hangGhe, DateTime? ngayDat)
        {
            var query = _context.VeMayBay
                .Include(v => v.NguoiDung)
                .Include(v => v.ChuyenBay).ThenInclude(cb => cb.MayBay)
                .Include(v => v.Ghe)
                .AsQueryable();

            var list = await query.ToListAsync();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("VeMayBay");
            ws.Cells[1, 1].Value = "ID";
            ws.Cells[1, 2].Value = "Người đặt";
            ws.Cells[1, 3].Value = "Chuyến bay";
            ws.Cells[1, 4].Value = "Hạng ghế";
            ws.Cells[1, 5].Value = "Thời gian đặt";
            ws.Cells[1, 6].Value = "Trạng thái";

            for (int i = 0; i < list.Count; i++)
            {
                var v = list[i];
                ws.Cells[i + 2, 1].Value = v.IDVe;
                ws.Cells[i + 2, 2].Value = v.NguoiDung?.HoTen;
                ws.Cells[i + 2, 3].Value = v.ChuyenBay?.IDChuyenBay;
                ws.Cells[i + 2, 4].Value = v.Ghe?.HangGhe;
                ws.Cells[i + 2, 5].Value = v.ThoiGianDat.ToString("dd/MM/yyyy HH:mm");
                ws.Cells[i + 2, 6].Value = v.TrangThaiVe;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VeMayBay.xlsx");
        }
        public IActionResult ChuyenBayCuaToi()
{
    var idNguoiDung = HttpContext.Session.GetInt32("IDNguoiDung");
    if (idNguoiDung == null)
    {
        return RedirectToAction("DangNhap", "TaiKhoan");
    }

            var veMayBays = (
            from ve in _context.VeMayBay
            join chuyen in _context.ChuyenBay
                on ve.IDChuyenBay equals chuyen.IDChuyenBay
            join mb in _context.MayBay
                on chuyen.IDMayBay equals mb.IDMayBay
            join ghe in _context.GheNgoi
                on ve.IDGhe equals ghe.IDGhe
            join tt in _context.ThanhToan
                on ve.IDVe equals tt.IDVe into ttGroup
            from tt in ttGroup.DefaultIfEmpty()
            where ve.IDNguoiDung == idNguoiDung
            select new ChuyenBayCuaToiViewModels
            {
                IDVe = ve.IDVe,
                MaChuyenBay = chuyen.IDChuyenBay,
                GioCatCanh = chuyen.GioCatCanh,
                GioHaCanh = chuyen.GioHaCanh,
                // ✅ Lấy TÊN sân bay từ navigation property
                SanBayDi = chuyen.SanBayDiInfo != null
                 ? chuyen.SanBayDiInfo.TenSanBay
                 : "Không rõ",
                SanBayDen = chuyen.SanBayDenInfo != null
                 ? chuyen.SanBayDenInfo.TenSanBay
                 : "Không rõ",

                TenMayBay = mb.TenHangHK,

                HangGhe = ve.HangGhe,

                LoaiVe = !string.IsNullOrEmpty(ve.LoaiVe)
                ? ve.LoaiVe
                : ve.HangGhe,

                PhuongThucThanhToan = tt != null ? tt.PhuongThuc : "Chưa thanh toán",
                TrangThaiThanhToan = tt != null ? tt.TrangThaiThanhToan : "Chưa thanh toán",

                TrangThaiVe = ve.TrangThaiVe ?? "Chưa thanh toán",
                TinhTrangChuyenBay = chuyen.TinhTrang
            }
        ).ToList();


            return View(veMayBays);
}
        // Xử lý hủy vé
        [HttpPost]
        public IActionResult HuyVe(int id, string lyDo)
        {
            var ve = _context.VeMayBay.FirstOrDefault(v => v.IDVe == id);
            var tt = _context.ThanhToan.FirstOrDefault(v => v.IDVe == id);

            if (ve != null && ve.TrangThaiVe != "Đã hủy" && tt != null)
            {
                ve.TrangThaiVe = "Đã hủy";

                HoanTien ht = new HoanTien();
                ht.IDThanhToan = tt.IDThanhToan;
                ht.SoTienHoan = tt.SoTien;
                ht.NgayHoanTien = DateTime.Now;

                // ⭐ đây là chỗ quan trọng:
                ht.LyDo = lyDo;   // lấy đúng lý do bạn nhập trong form

                _context.HoanTien.Add(ht);
                _context.SaveChanges();
            }

            return RedirectToAction("ChuyenBayCuaToi");
        }

    }
}