using MongoDB.Bson;
using System.Security.Cryptography;

namespace srtool
{
    public static class ObjectIdExtension
    {
        public static ObjectId GenerateBySeed(string seed)
        {
            // Hash the seed string to create a 12-byte array using MD5
            using (MD5 md5 = MD5.Create())
            {
                // Generate a 12-byte byte array from the seed string
                byte[] seedBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));

                // Take the first 12 bytes (MD5 produces a 16-byte hash, but ObjectId needs 12 bytes)
                byte[] objectIdBytes = new byte[12];
                Array.Copy(seedBytes, objectIdBytes, 12);

                // Create ObjectId from the byte array
                return new ObjectId(objectIdBytes);
            }
        }
    }
}