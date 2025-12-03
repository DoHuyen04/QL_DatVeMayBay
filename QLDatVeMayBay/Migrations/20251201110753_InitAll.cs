using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDatVeMayBay.Migrations
{
    /// <inheritdoc />
    public partial class InitAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiMayBay",
                columns: table => new
                {
                    LoaiMayBayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TongSoGhe = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiMayBay", x => x.LoaiMayBayId);
                });

            migrationBuilder.CreateTable(
                name: "MaXacNhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianHetHan = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaXacNhan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanBay",
                columns: table => new
                {
                    IDSanBay = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSanBay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanBay", x => x.IDSanBay);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThaiTK = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SoLanDangNhapSai = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBiKhoa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.TenDangNhap);
                });

            migrationBuilder.CreateTable(
                name: "MayBay",
                columns: table => new
                {
                    IDMayBay = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHangHK = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiMayBayId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MayBay", x => x.IDMayBay);
                    table.ForeignKey(
                        name: "FK_MayBay_LoaiMayBay_LoaiMayBayId",
                        column: x => x.LoaiMayBayId,
                        principalTable: "LoaiMayBay",
                        principalColumn: "LoaiMayBayId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    IDNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QuocTich = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.IDNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDung_TaiKhoan_TenDangNhap",
                        column: x => x.TenDangNhap,
                        principalTable: "TaiKhoan",
                        principalColumn: "TenDangNhap",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenBay",
                columns: table => new
                {
                    IDChuyenBay = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDMayBay = table.Column<int>(type: "int", nullable: false),
                    SanBayDi = table.Column<int>(type: "int", nullable: false),
                    SanBayDen = table.Column<int>(type: "int", nullable: false),
                    GioCatCanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioHaCanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GiaVe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TinhTrang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenBay", x => x.IDChuyenBay);
                    table.ForeignKey(
                        name: "FK_ChuyenBay_MayBay_IDMayBay",
                        column: x => x.IDMayBay,
                        principalTable: "MayBay",
                        principalColumn: "IDMayBay",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuyenBay_SanBay_SanBayDen",
                        column: x => x.SanBayDen,
                        principalTable: "SanBay",
                        principalColumn: "IDSanBay",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuyenBay_SanBay_SanBayDi",
                        column: x => x.SanBayDi,
                        principalTable: "SanBay",
                        principalColumn: "IDSanBay",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TheThanhToan",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    Loai = table.Column<int>(type: "int", nullable: false),
                    TenNganHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoThe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HieuLuc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CVV = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    TenTrenThe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailLienKet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenHienThi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayLienKet = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheThanhToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TheThanhToan_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "IDNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GheNgoi",
                columns: table => new
                {
                    IDGhe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDChuyenBay = table.Column<int>(type: "int", nullable: false),
                    HangGhe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GheNgoi", x => x.IDGhe);
                    table.ForeignKey(
                        name: "FK_GheNgoi_ChuyenBay_IDChuyenBay",
                        column: x => x.IDChuyenBay,
                        principalTable: "ChuyenBay",
                        principalColumn: "IDChuyenBay",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoanTien",
                columns: table => new
                {
                    IDHoanTien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDThanhToan = table.Column<int>(type: "int", nullable: false),
                    SoTienHoan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayHoanTien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoanTien", x => x.IDHoanTien);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToan",
                columns: table => new
                {
                    IDThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDVe = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThoiGianGiaoDich = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.IDThanhToan);
                });

            migrationBuilder.CreateTable(
                name: "VeMayBay",
                columns: table => new
                {
                    IDVe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDNguoiDung = table.Column<int>(type: "int", nullable: false),
                    IDChuyenBay = table.Column<int>(type: "int", nullable: false),
                    IDGhe = table.Column<int>(type: "int", nullable: false),
                    ThoiGianDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiVe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HangGhe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiVe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheThanhToanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ThanhToanIDThanhToan = table.Column<int>(type: "int", nullable: true),
                    IDTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    TaiKhoanTenDangNhap = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    NguoiDungIDNguoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeMayBay", x => x.IDVe);
                    table.ForeignKey(
                        name: "FK_VeMayBay_ChuyenBay_IDChuyenBay",
                        column: x => x.IDChuyenBay,
                        principalTable: "ChuyenBay",
                        principalColumn: "IDChuyenBay",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VeMayBay_GheNgoi_IDGhe",
                        column: x => x.IDGhe,
                        principalTable: "GheNgoi",
                        principalColumn: "IDGhe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VeMayBay_NguoiDung_IDNguoiDung",
                        column: x => x.IDNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "IDNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VeMayBay_NguoiDung_NguoiDungIDNguoiDung",
                        column: x => x.NguoiDungIDNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "IDNguoiDung");
                    table.ForeignKey(
                        name: "FK_VeMayBay_TaiKhoan_TaiKhoanTenDangNhap",
                        column: x => x.TaiKhoanTenDangNhap,
                        principalTable: "TaiKhoan",
                        principalColumn: "TenDangNhap");
                    table.ForeignKey(
                        name: "FK_VeMayBay_ThanhToan_ThanhToanIDThanhToan",
                        column: x => x.ThanhToanIDThanhToan,
                        principalTable: "ThanhToan",
                        principalColumn: "IDThanhToan");
                    table.ForeignKey(
                        name: "FK_VeMayBay_TheThanhToan_TheThanhToanId",
                        column: x => x.TheThanhToanId,
                        principalTable: "TheThanhToan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenBay_IDMayBay",
                table: "ChuyenBay",
                column: "IDMayBay");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenBay_SanBayDen",
                table: "ChuyenBay",
                column: "SanBayDen");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenBay_SanBayDi",
                table: "ChuyenBay",
                column: "SanBayDi");

            migrationBuilder.CreateIndex(
                name: "IX_GheNgoi_IDChuyenBay",
                table: "GheNgoi",
                column: "IDChuyenBay");

            migrationBuilder.CreateIndex(
                name: "IX_HoanTien_IDThanhToan",
                table: "HoanTien",
                column: "IDThanhToan");

            migrationBuilder.CreateIndex(
                name: "IX_MayBay_LoaiMayBayId",
                table: "MayBay",
                column: "LoaiMayBayId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_IDVe",
                table: "ThanhToan",
                column: "IDVe");

            migrationBuilder.CreateIndex(
                name: "IX_TheThanhToan_NguoiDungId",
                table: "TheThanhToan",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_IDChuyenBay",
                table: "VeMayBay",
                column: "IDChuyenBay");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_IDGhe",
                table: "VeMayBay",
                column: "IDGhe");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_IDNguoiDung",
                table: "VeMayBay",
                column: "IDNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_NguoiDungIDNguoiDung",
                table: "VeMayBay",
                column: "NguoiDungIDNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_TaiKhoanTenDangNhap",
                table: "VeMayBay",
                column: "TaiKhoanTenDangNhap");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_ThanhToanIDThanhToan",
                table: "VeMayBay",
                column: "ThanhToanIDThanhToan");

            migrationBuilder.CreateIndex(
                name: "IX_VeMayBay_TheThanhToanId",
                table: "VeMayBay",
                column: "TheThanhToanId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoanTien_ThanhToan_IDThanhToan",
                table: "HoanTien",
                column: "IDThanhToan",
                principalTable: "ThanhToan",
                principalColumn: "IDThanhToan",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToan_VeMayBay_IDVe",
                table: "ThanhToan",
                column: "IDVe",
                principalTable: "VeMayBay",
                principalColumn: "IDVe",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenBay_MayBay_IDMayBay",
                table: "ChuyenBay");

            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenBay_SanBay_SanBayDen",
                table: "ChuyenBay");

            migrationBuilder.DropForeignKey(
                name: "FK_ChuyenBay_SanBay_SanBayDi",
                table: "ChuyenBay");

            migrationBuilder.DropForeignKey(
                name: "FK_GheNgoi_ChuyenBay_IDChuyenBay",
                table: "GheNgoi");

            migrationBuilder.DropForeignKey(
                name: "FK_VeMayBay_ChuyenBay_IDChuyenBay",
                table: "VeMayBay");

            migrationBuilder.DropForeignKey(
                name: "FK_VeMayBay_ThanhToan_ThanhToanIDThanhToan",
                table: "VeMayBay");

            migrationBuilder.DropTable(
                name: "HoanTien");

            migrationBuilder.DropTable(
                name: "MaXacNhan");

            migrationBuilder.DropTable(
                name: "MayBay");

            migrationBuilder.DropTable(
                name: "LoaiMayBay");

            migrationBuilder.DropTable(
                name: "SanBay");

            migrationBuilder.DropTable(
                name: "ChuyenBay");

            migrationBuilder.DropTable(
                name: "ThanhToan");

            migrationBuilder.DropTable(
                name: "VeMayBay");

            migrationBuilder.DropTable(
                name: "GheNgoi");

            migrationBuilder.DropTable(
                name: "TheThanhToan");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
