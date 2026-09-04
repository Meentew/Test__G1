var builder = WebApplication.CreateBuilder(args);

// รองรับ platform ที่กำหนด port ผ่าน environment variable "PORT"
// (เช่น Render.com ซึ่ง default คือ 10000 - ถ้าไม่อ่านค่านี้ แอปจะไปฟังผิด port แล้ว deploy timeout)
// ถ้าไม่มีตัวแปรนี้ (เช่นรันในเครื่องตัวเอง หรือ Railway ที่ใช้ ASPNETCORE_HTTP_PORTS อยู่แล้ว) จะ fallback ไปที่ 8080 ตามเดิม
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// เพิ่ม Razor Pages
builder.Services.AddRazorPages();

// เพิ่ม Session (ใช้เก็บสถานะ login แทน System.Web.Session แบบเดิม)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
