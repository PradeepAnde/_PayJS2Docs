using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace payjs_csharp_sample.PaymentsJS
{
    public static class Encryption
    {
        public static Nonces GetRandomData(int i = 16)
        {
            byte[] nonceBytes = new byte[i];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(nonceBytes);
            }
            var result = new Nonces();
            result.IV = nonceBytes;
            result.Salt = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(BytesToHex(nonceBytes)));
            return result;
        }

        public static string Encrypt(string message, string password, string salt, byte[] iv)
        {
            message = UTF8Encoding.UTF8.GetString(UTF8Encoding.UTF8.GetBytes(message));
            byte[] encryptedResult;
            using (Aes aesAlg = Aes.Create())
            {
                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, UTF8Encoding.UTF8.GetBytes(salt), 1500))
                {
                    aesAlg.Key = pbkdf2.GetBytes(32);
                }
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }
                        encryptedResult = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encryptedResult);
        }

        private static string BytesToHex(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

    public class Nonces
    {
        public string Salt;
        public byte[] IV;
    }
}