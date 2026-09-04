using System.Security.Cryptography;
using System.Text;

namespace Test____G1.Services
{
    /// <summary>
    /// แปลงรหัสผ่านเป็น SHA256 hash (hex string) ก่อนเก็บลงฐานข้อมูล
    /// เพื่อไม่ให้เก็บรหัสผ่านแบบ plain text
    /// </summary>
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
