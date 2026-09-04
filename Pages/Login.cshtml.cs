using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using Test____G1.Services;

namespace Test____G1.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _config;

        public LoginModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string ErrorMessage { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Username = (Username ?? "").Trim();

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน";
                return Page();
            }

            string hashedPassword = PasswordHelper.Hash(Password);
            string connStr = _config.GetConnectionString("PGConnection");

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT a.\"Account_Status\", u.\"Name\", u.\"Urname\", u.\"User_ID\" " +
                "FROM \"Use_Account\" a " +
                "JOIN \"User\" u ON a.\"User_ID\" = u.\"User_ID\" " +
                "WHERE a.\"Account_User\" = @username AND a.\"Account_Password\" = @password", conn);
            cmd.Parameters.AddWithValue("username", Username);
            cmd.Parameters.AddWithValue("password", hashedPassword);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                bool? status = reader["Account_Status"] as bool?;
                if (status.HasValue && status.Value == false)
                {
                    ErrorMessage = "บัญชีนี้ถูกระงับการใช้งาน กรุณาติดต่อผู้ดูแลระบบ";
                    return Page();
                }

                // เข้าสู่ระบบสำเร็จ - เก็บข้อมูลลง Session
                HttpContext.Session.SetString("Username", Username);
                HttpContext.Session.SetString("FullName", reader["Name"] + " " + reader["Urname"]);
                HttpContext.Session.SetString("UserId", reader["User_ID"].ToString() ?? "");

                return RedirectToPage("Welcome");
            }
            else
            {
                ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
                return Page();
            }
        }
    }
}
