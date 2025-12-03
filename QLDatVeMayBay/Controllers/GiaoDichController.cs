using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.ViewModels;
using System;

namespace QLDatVeMayBay.Controllers
{
    public class GiaoDichController : Controller
    {
        private readonly QLDatVeMayBayContext _context;

        public GiaoDichController(QLDatVeMayBayContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy ID người dùng từ session (giống ChuyenBayCuaToi)
            var idNguoiDung = HttpContext.Session.GetInt32("IDNguoiDung");
            if (idNguoiDung == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }
            var giaoDichList = _context.ThanhToan
                .Include(t => t.VeMayBay)
                    .ThenInclude(v => v.NguoiDung)
                     .Where(t => t.VeMayBay != null
                    && t.VeMayBay.NguoiDung != null
                    && t.VeMayBay.NguoiDung.IDNguoiDung == idNguoiDung)
        .Select(t => new GiaoDichViewModel
        {
                    IDThanhToan = t.IDThanhToan,
                    IDVe = t.IDVe,
                    HoTenNguoiDung = t.VeMayBay!.NguoiDung!.HoTen,
                    TenDangNhap = t.VeMayBay!.NguoiDung!.TenDangNhap,
                    SoTien = t.SoTien,
            PhuongThuc = t.PhuongThuc ?? string.Empty,

            ThoiGianGiaoDich = t.ThoiGianGiaoDich,
                    TrangThaiThanhToan = t.TrangThaiThanhToan ?? "Không rõ"
                })
                .OrderByDescending(t => t.ThoiGianGiaoDich)
                .ToList();

            return View(giaoDichList);
        }
    }
}
