using System;
using System.Security.Cryptography;
using System.Text;

namespace Consultorios
{
    public static class PasswordResetHelper
    {
        public static string GenerarToken()
        {
            byte[] bytes =
                new byte[32];

            using (RNGCryptoServiceProvider rng =
                new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            return Convert
                .ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }


        public static byte[] ObtenerHashToken(
            string token)
        {
            using (SHA256 sha =
                SHA256.Create())
            {
                return sha.ComputeHash(
                    Encoding.UTF8.GetBytes(token));
            }
        }
    }
}
