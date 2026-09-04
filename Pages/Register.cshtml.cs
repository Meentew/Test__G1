using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using Test____G1.Services;

namespace Test____G1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _config;

        public RegisterModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public string Name { get; set; } = "";

        [BindProperty]
        public string Surname { get; set; } = "";

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Phone { get; set; } = "";

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        [BindProperty]
        public string ConfirmPassword { get; set; } = "";

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Name = (Name ?? "").Trim();
            Surname = (Surname ?? "").Trim();
            Email = (Email ?? "").Trim();
            Phone = (Phone ?? "").Trim();
            Username = (Username ?? "").Trim();

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "กรุณากรอกชื่อ, ชื่อผู้ใช้ และรหัสผ่านให้ครบถ้วน";
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "รหัสผ่านและการยืนยันรหัสผ่านไม่ตรงกัน";
                return Page();
            }

            long phoneNumber = 0;
            if (!string.IsNullOrEmpty(Phone) && !long.TryParse(Phone, out phoneNumber))
            {
                ErrorMessage = "เบอร์โทรศัพท์ต้องเป็นตัวเลขเท่านั้น";
                return Page();
            }

            string connStr = _config.GetConnectionString("PGConnection");

            NpgsqlConnection conn;
            try
            {
                conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "เชื่อมต่อฐานข้อมูลไม่สำเร็จ: " + ex.Message;
                return Page();
            }

            await using (conn)
            {
                await using var tx = await conn.BeginTransactionAsync();
                try
                {
                // ตรวจสอบว่า username นี้ถูกใช้ไปแล้วหรือยัง
                await using (var checkCmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM \"Use_Account\" WHERE \"Account_User\" = @username", conn, tx))
                {
                    checkCmd.Parameters.AddWithValue("username", Username);
                    long count = (long)(await checkCmd.ExecuteScalarAsync())!;
                    if (count > 0)
                    {
                        ErrorMessage = "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว กรุณาเลือกชื่ออื่น";
                        await tx.RollbackAsync();
                        return Page();
                    }
                }

                // เพิ่มข้อมูลลงตาราง User และรับ User_ID ที่ถูกสร้างขึ้น
                long newUserId;
                await using (var insertUserCmd = new NpgsqlCommand(
                    "INSERT INTO \"User\" (\"Name\", \"Urname\", \"E-Mail\", \"phone_number\") " +
                    "VALUES (@name, @surname, @email, @phone) RETURNING \"User_ID\"", conn, tx))
                {
                    insertUserCmd.Parameters.AddWithValue("name", Name);
                    insertUserCmd.Parameters.AddWithValue("surname", (object)Surname ?? DBNull.Value);
                    insertUserCmd.Parameters.AddWithValue("email", (object)Email ?? DBNull.Value);
                    insertUserCmd.Parameters.AddWithValue("phone", phoneNumber);
                    newUserId = (long)(await insertUserCmd.ExecuteScalarAsync())!;
                }

                // เพิ่มข้อมูลบัญชีเข้าสู่ระบบลงตาราง Use_Account
                string hashedPassword = PasswordHelper.Hash(Password);
                await using (var insertAccountCmd = new NpgsqlCommand(
                    "INSERT INTO \"Use_Account\" (\"Account_User\", \"Account_Password\", \"User_ID\", \"Account_Status\") " +
                    "VALUES (@username, @password, @userId, @status)", conn, tx))
                {
                    insertAccountCmd.Parameters.AddWithValue("username", Username);
                    insertAccountCmd.Parameters.AddWithValue("password", hashedPassword);
                    insertAccountCmd.Parameters.AddWithValue("userId", newUserId);
                    insertAccountCmd.Parameters.AddWithValue("status", true);
                    await insertAccountCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                    SuccessMessage = "ลงทะเบียนสำเร็จ! กำลังพาไปหน้าเข้าสู่ระบบ...";
                    Response.Headers.Append("Refresh", "2;url=/Login");
                    return Page();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    ErrorMessage = "เกิดข้อผิดพลาด: " + ex.Message;
                    return Page();
                }
            }
        }
    }
}
