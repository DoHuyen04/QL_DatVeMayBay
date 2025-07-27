using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QLDatVeMayBay.Controllers;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Models;
using QLDatVeMayBay.Models.Entities;
using QLDatVeMayBay.Services;
using System.Text;
using System.Text.Json;
using TestDatVeMayBay.Tests.Controller;

public class DatVeControllerTest
{
    private readonly DatVeController _controller;
    private readonly QLDatVeMayBayContext _context;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<EmailService> _mockEmailService;

    private static DatVeController CreateController(ISession session)
    {
        // Tạo context giả lập bằng InMemory DB
        var options = new DbContextOptionsBuilder<QLDatVeMayBayContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new QLDatVeMayBayContext(options);

        // Tạo IConfiguration giả lập
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["EmailSettings:Host"]).Returns("smtp.test.com");

        // Tạo EmailService giả
        var emailService = new Mock<EmailService>(mockConfig.Object).Object;

        // Gán session vào HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var controller = new DatVeController(context, mockConfig.Object, emailService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public void ThanhToan_RedirectsToLogin_WhenSessionInvalid()
    {
        var session = TestSession.CreateSession(); // không có IDNguoiDung
        var controller = CreateController(session);

        var result = controller.ThanhToan(1, 1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("DangNhap", redirectResult.ActionName);
        Assert.Equal("TaiKhoan", redirectResult.ControllerName);
    }

    [Fact]
    public void ThanhToan_RedirectsToChonGhe_WhenNoSeatSelected()
    {
        var session = TestSession.CreateSession(new Dictionary<string, string>
        {
            { "IDNguoiDung", "1" }
        });
        var controller = CreateController(session);

        var result = controller.ThanhToan(1, 0); // idGhe = 0 giả lập không chọn

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ChonGhe", redirectResult.ActionName);
    }
    [Fact]
    public async Task ChonGhe_Returns_View_With_Correct_Model()
    {
        var loaiMayBay = new LoaiMayBay { LoaiMayBayId = 1, TongSoGhe = 150 };
        var mayBay = new MayBay { IDMayBay = 1, LoaiMayBay = loaiMayBay };
        var chuyenBay = new ChuyenBay { IDChuyenBay = 1, MayBay = mayBay };
        _context.LoaiMayBay.Add(loaiMayBay);
        _context.MayBay.Add(mayBay);
        _context.ChuyenBay.Add(chuyenBay);
        await _context.SaveChangesAsync();

        var result = await _controller.ChonGhe(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<DatGhe>(viewResult.Model);
        Assert.Equal(150, model.TongSoGhe);
    }
    [Fact]
    public void XacNhanVe_WithValidSession_ReturnsViewResult()
    {
        // Arrange
        var session = TestSession.CreateSession(new Dictionary<string, string>
    {
        { "TenDangNhap", "testuser" },
        { "VaiTro", "KhachHang" }
    });

        var httpContext = new DefaultHttpContext
        {
            Session = session
        };

        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var mockContext = new Mock<QLDatVeMayBayContext>().Object;
        var mockConfig = new Mock<IConfiguration>().Object;
        var mockEmail = new Mock<EmailService>(mockConfig).Object;

        var controller = new DatVeController(mockContext, mockConfig, mockEmail)
        {
            ControllerContext = controllerContext
        };

        // Act
        var result = controller.XacNhanVe(1, 5);

        // Assert
        Assert.IsType<ViewResult>(result);
    }


    //[Fact]
    //public void ThanhToan_ReturnsRedirectToAction_WhenSessionInvalid()
    //{
    //    // Arrange
    //    var options = new DbContextOptionsBuilder<QLDatVeMayBayContext>()
    //        .UseInMemoryDatabase(databaseName: "TestDatabase_ThanhToan")
    //        .Options;

    //    using var context = new QLDatVeMayBayContext(options);

    //    var controller = new DatVeController(context);

    //    var httpContext = new DefaultHttpContext();
    //    httpContext.Session = TestSession.CreateSession(); // ✅ Sử dụng session giả lập

    //    controller.ControllerContext = new ControllerContext
    //    {
    //        HttpContext = httpContext
    //    };

    //    // Act
    //    var result = controller.ThanhToan(1, 2);

    //    // Assert
    //    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
    //    Assert.Equal("DangNhap", redirectResult.ActionName);
    //    Assert.Equal("TaiKhoan", redirectResult.ControllerName);
    //}

    //[Fact]
    //public void ThanhToan_ChoosesView_WhenSessionValidAndGheIsNull()
    //{
    //    // Arrange
    //    var options = new DbContextOptionsBuilder<QLDatVeMayBayContext>()
    //        .UseInMemoryDatabase(databaseName: "TestDatabase_ThanhToan_GheNull")
    //        .Options;

    //    using var context = new QLDatVeMayBayContext(options);

    //    var controller = new DatVeController(context);

    //    var session = TestSession.CreateSession(new Dictionary<string, string>
    //    {
    //        { "IDNguoiDung", "123" }
    //    });

    //    var httpContext = new DefaultHttpContext();
    //    httpContext.Session = session;

    //    controller.ControllerContext = new ControllerContext
    //    {
    //        HttpContext = httpContext
    //    };

    //    // Act
    //    var result = controller.ThanhToan(1, 0); // idGhe null giả lập bằng 0

    //    // Assert
    //    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
    //    Assert.Equal("ChonGhe", redirectResult.ActionName);
    //}

    [Fact]
    public async Task KiemTraOTP_Returns_ThanhToanThanhCong_View_When_OTP_Valid()
    {
        var contextOptions = new DbContextOptionsBuilder<QLDatVeMayBayContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new QLDatVeMayBayContext(contextOptions);
        var config = new Mock<IConfiguration>();
        var emailService = new Mock<EmailService>(config.Object);
        var controller = new DatVeController(db, config.Object, emailService.Object);

        var ve = new VeMayBay
        {
            IDVe = 1,
            IDNguoiDung = 1,
            IDChuyenBay = 1,
            IDGhe = 12,
            HangGhe = "Phổ thông",
            LoaiVe = "Thường",
            TrangThaiVe = "Chưa đặt"
        };
        var model = new ThongTinThanhToan
        {
            MaOTP = "123456",
            SelectedTheId = "1",
            SoTien = 2000000,
            PhuongThuc = "Thẻ ngân hàng",
            Ve = ve
        };

        db.NguoiDung.Add(new NguoiDung { IDNguoiDung = 1, Email = "test@example.com", HoTen = "Nguyen Van A" });
        db.SanBay.AddRange(
            new SanBay { IDSanBay = 1, TenSanBay = "Nội Bài" },
            new SanBay { IDSanBay = 2, TenSanBay = "Tân Sơn Nhất" }
        );
        db.ChuyenBay.Add(new ChuyenBay
        {
            IDChuyenBay = 1,
            MayBay = new MayBay { TenHangHK = "Vietnam Airlines", LoaiMayBayId = 1 },
            SanBayDi = 1,
            SanBayDen = 2,
            GioCatCanh = DateTime.Now,
            GioHaCanh = DateTime.Now.AddHours(2)
        });
        db.TheThanhToan.Add(new TheThanhToan { Id = "1", Loai = LoaiTheLoaiVi.TheNganHang, TenNganHang = "Vietcombank" });
        await db.SaveChangesAsync();

        var session = TestSession.CreateSession(new Dictionary<string, string>
        {
            { "OTP", "123456" },
            { "OTP_Expires", DateTime.Now.AddMinutes(5).ToString() }
        },
        new Dictionary<string, byte[]>
        {
            { "VeTemp", JsonSerializer.SerializeToUtf8Bytes(model) }
        });

        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            Session = session
        };

        var result = await controller.KiemTraOTP(new ThongTinThanhToan { MaOTP = "123456" });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ThanhToanThanhCong", viewResult.ViewName);
        var resultModel = Assert.IsAssignableFrom<ThongTinThanhToan>(viewResult.Model);
        Assert.Equal("Thẻ ngân hàng", resultModel.PhuongThuc);
    }
}
