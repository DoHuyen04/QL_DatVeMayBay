using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Models;

namespace QLDatVeMayBay.Controllers
{
    public class HomeController : Controller
    {
        private readonly QLDatVeMayBayContext _context;

        //  Constructor duy nhất, inject DbContext
        public HomeController(QLDatVeMayBayContext context)
        {
            _context = context;
        }

        // Trang chủ - Hiển thị form tìm kiếm
        public IActionResult Index()
        {
            // Load danh sách sân bay cho combobox
            ViewBag.SanBayDi = new SelectList(_context.SanBay.ToList(), "IDSanBay", "TenSanBay");
            ViewBag.SanBayDen = new SelectList(_context.SanBay.ToList(), "IDSanBay", "TenSanBay");

            return View(new TimKiemChuyenBay());
        }

        //  POST: Tìm kiếm chuyến bay
        [HttpPost]
        public IActionResult TimKiem(TimKiemChuyenBay model)
        {
            var danhSach = _context.ChuyenBay
                .Include(cb => cb.MayBay)
                .Include(cb => cb.SanBayDiInfo)
                .Include(cb => cb.SanBayDenInfo)
                .Where(cb =>
                    cb.SanBayDi == model.SanBayDi &&
                    cb.SanBayDen == model.SanBayDen &&
                    cb.GioCatCanh.Date == model.NgayDi.Date)
                .ToList();

            // Trả lại dữ liệu hiển thị
            ViewBag.KetQua = danhSach;

            // RẤT QUAN TRỌNG: Truyền lại dropdowns
            ViewBag.SanBayDi = new SelectList(_context.SanBay.ToList(), "IDSanBay", "TenSanBay");
            ViewBag.SanBayDen = new SelectList(_context.SanBay.ToList(), "IDSanBay", "TenSanBay");

            return View("Index", model);
        }

        public IActionResult TrangChu()
        {
            return View();
        }

        public IActionResult HoTro()
        {
            return View();
        }
        public IActionResult TroGiup()
        {
            return View();
        }

        public IActionResult LienHe()
        {
            return View();
        }

        public IActionResult HopThu()
        {
            return View();
        }
        public IActionResult HuongDanDatCho()
        {
            return View();
        }
    }
}
                                                                                                                                                                                                                            