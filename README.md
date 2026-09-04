# Test____G1 - ASP.NET Core (Razor Pages) + PostgreSQL + Docker

โปรเจกต์นี้ migrate มาจาก ASP.NET Web Forms (.NET Framework) เดิม ให้เป็น **ASP.NET Core 8 (Razor Pages)**
เพื่อให้ build/run ด้วย Docker (Dockerfile ที่แนบมา) ได้จริง และ deploy บน Railway ได้

## โครงสร้างไฟล์
```
Test____G1/
├── Test____G1.csproj
├── Program.cs
├── appsettings.json
├── Dockerfile
├── .dockerignore
├── Services/
│   └── PasswordHelper.cs     # hash รหัสผ่านด้วย SHA256
├── Pages/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml # header/nav/footer ของทุกหน้า
│   ├── Index.cshtml(.cs)     # redirect ไป /Login
│   ├── Register.cshtml(.cs)
│   ├── Login.cshtml(.cs)
│   └── Welcome.cshtml(.cs)
└── wwwroot/css/site.css
```

## รันทดสอบในเครื่องตัวเอง (ไม่ใช้ Docker)

1. ติดตั้ง [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. แก้ connection string ใน `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "PGConnection": "Host=localhost;Port=5432;Database=yourdb;Username=youruser;Password=yourpassword;"
   }
   ```
3. รันคำสั่งในโฟลเดอร์โปรเจกต์:
   ```bash
   dotnet restore
   dotnet run
   ```
4. เปิดเบราว์เซอร์ตามพอร์ตที่ terminal แจ้ง (ปกติ `https://localhost:5001` หรือ `http://localhost:5000`)

## รันทดสอบด้วย Docker ในเครื่องตัวเอง

```bash
docker build -t test-g1 .
docker run -p 8080:8080 -e ConnectionStrings__PGConnection="Host=xxx;Port=5432;Database=xxx;Username=xxx;Password=xxx;" test-g1
```
แล้วเปิด `http://localhost:8080`

## Deploy บน Railway

1. Push โค้ดทั้งหมด (รวม `Dockerfile` ที่ root ของรีโป) ขึ้น GitHub — Railway จะเจอ `Dockerfile` และ build ให้อัตโนมัติ
2. ไปที่ Railway > โปรเจกต์ > **Variables** เพิ่ม environment variable ดังนี้ (ห้ามเก็บรหัสผ่านฐานข้อมูลไว้ใน `appsettings.json` ตอน deploy จริง):
   ```
   ConnectionStrings__PGConnection = Host=<host>;Port=5432;Database=<db>;Username=<user>;Password=<password>;
   ```
   ASP.NET Core จะอ่านค่านี้มาทับ `appsettings.json` อัตโนมัติ (ใช้ `__` แทนจุดของ key ที่ซ้อนกัน)
3. ถ้าใช้ Railway PostgreSQL plugin สามารถ copy connection string จากแท็บ **Connect** ของฐานข้อมูลมาวางได้เลย
4. Railway จะ map พอร์ต 8080 ที่ `EXPOSE`/`ASPNETCORE_HTTP_PORTS` ให้อัตโนมัติ ไม่ต้องตั้งอะไรเพิ่ม

## หมายเหตุสำคัญ

- **ชื่อ .dll ต้องตรงกับ ENTRYPOINT**: `ENTRYPOINT ["dotnet", "Test____G1.dll"]` ต้องตรงกับชื่อไฟล์ `Test____G1.csproj` (ชื่อโปรเจกต์ = ชื่อ .dll ที่ publish ออกมา) ถ้าเปลี่ยนชื่อโปรเจกต์ต้องแก้บรรทัดนี้ด้วย
- **`User` เป็นคำสงวนใน PostgreSQL** โค้ดครอบชื่อตาราง/คอลัมน์ด้วย `"..."` ให้แล้ว เหมือนโปรเจกต์เดิม
- **รหัสผ่าน**: hash ด้วย SHA256 ก่อนบันทึกและก่อนเปรียบเทียบตอน login เหมือนเดิม
- **Session**: ใช้ ASP.NET Core Session (in-memory) เก็บสถานะ login แทน `System.Web.Session` แบบเดิม ถ้า deploy แบบมีหลาย instance (scale > 1) ต้องเปลี่ยนไปใช้ distributed cache เช่น Redis แทน `AddDistributedMemoryCache()`
