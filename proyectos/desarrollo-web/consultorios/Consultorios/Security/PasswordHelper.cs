using System;
using System.Security.Cryptography;

namespace Consultorios
{
    public static class PasswordHelper
    {
        private const int Iteraciones = 100000;
        private const int TamanioSalt = 32;
        private const int TamanioHash = 64;


        public static void CrearPasswordHash(
            string password,
            out byte[] passwordHash,
            out byte[] passwordSalt)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(
                    "La contraseña no puede estar vacía.");
            }

            passwordSalt = new byte[TamanioSalt];

            using (RNGCryptoServiceProvider rng =
                new RNGCryptoServiceProvider())
            {
                rng.GetBytes(passwordSalt);
            }

            using (Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(
                    password,
                    passwordSalt,
                    Iteraciones))
            {
                passwordHash =
                    pbkdf2.GetBytes(TamanioHash);
            }
        }


        public static bool VerificarPassword(
            string password,
            byte[] passwordHashGuardado,
            byte[] passwordSaltGuardado)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (passwordHashGuardado == null ||
                passwordSaltGuardado == null)
            {
                return false;
            }

            byte[] hashCalculado;

            using (Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(
                    password,
                    passwordSaltGuardado,
                    Iteraciones))
            {
                hashCalculado =
                    pbkdf2.GetBytes(TamanioHash);
            }

            if (hashCalculado.Length !=
                passwordHashGuardado.Length)
            {
                return false;
            }

            bool iguales = true;

            for (int i = 0;
                 i < hashCalculado.Length;
                 i++)
            {
                if (hashCalculado[i] !=
                    passwordHashGuardado[i])
                {
                    iguales = false;
                }
            }

            return iguales;
        }
    }
}
