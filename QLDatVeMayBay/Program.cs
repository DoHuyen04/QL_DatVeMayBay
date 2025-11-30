using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QLDatVeMayBay.Data;
using QLDatVeMayBay.Services;

var builder = WebApplication.CreateBuilder(args);

// ❗ Loại bỏ EventLog để tránh lỗi ghi log vào Windows
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// builder.Logging.AddFile("Logs/log-{Date}.txt"); // nếu muốn ghi file

// ✅ Cấu hình xác thực Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.LogoutPath = "/TaiKhoan/DangXuat";
        options.AccessDeniedPath = "/TaiKhoan/AccessDenied";
    });

// ✅ Cấu hình EF DbContext
builder.Services.AddDbContext<QLDatVeMayBayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLDatVeMayBayContext")));

// ✅ Email service (gọn gàng)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailService>();

// ✅ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ✅ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto-migrate
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<QLDatVeMayBayContext>();
    context.Database.Migrate();
}

app.Run();