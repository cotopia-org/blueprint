using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;

namespace blueprint.modules.blueprint.core
{
    public static class util
    {
        private static string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString().ToLower();
        }
        public static string GenerateId()
        {
            return CalculateMD5Hash(Guid.NewGuid().ToString())[..10].ToLower();
        }

    }
}
