using System;
using System.Security.Cryptography;
using System.Text;

namespace PlaylistRandomizer
{
    public static class Hmac
    {
        public static bool VerifyHash(string hash, string content, string secret)
        {
            string resultHash;
            resultHash = Generate(content, secret);
            return resultHash == hash;
        }

        /// <summary>
        /// SHA256 Hex format
        /// </summary>
        /// <param name="content"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string Generate(string content, string secret)
        {
            var hashMsg = GenerateSha256Raw(content, secret);
            return FormatAsHex(hashMsg);
        }

        private static byte[] GenerateSha256Raw(string content, string secret)
        {
            var keyBytes = Encoding.ASCII.GetBytes(secret);
            var messageBytes = Encoding.ASCII.GetBytes(content);
            using (var hmac = new HMACSHA256(keyBytes))
            {               
                return hmac.ComputeHash(messageBytes);
            }
        }

        private static string FormatAsHex(byte[] hashMessage)
        {
            var sb = new StringBuilder();
            for (var i = 0; i <= hashMessage.Length - 1; i++)
            {
                sb.Append(hashMessage[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }
}