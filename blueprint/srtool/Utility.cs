using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace srtool
{
    public static class Utility
    {
        #region String
        /// <summary>
        /// Encode text to utf8 base 64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64_Encoder_utf8(string str)
        {
            byte[] e_utf8_bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(e_utf8_bytes);
        }
        /// <summary>
        /// Decode text of base 64 utf8 text
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64_Decoder_utf8(string str)
        {
            byte[] d_base64_bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(d_base64_bytes);
        }
        /// <summary>
        /// Return true if english text
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEnglish(string inputstring)
        {
            Regex regex = new Regex(@"[A-Za-z0-9 .,-=+(){}\[\]\\]");
            MatchCollection matches = regex.Matches(inputstring);

            if (matches.Count.Equals(inputstring.Length))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Return Max date time of d1 or d2
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        #endregion
        #region BASE_IO_FUNCS
        private const int rwBuffer = 1024 * 100;
        public static async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: rwBuffer, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[0x1000];
                int numRead;
                bool f = true;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                    sb.Append(text);
                    f = false;
                }
                if (f && numRead == 0)
                    return null;

                sourceStream.Close();
                return sb.ToString();
            }
        }
        public static async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.UTF8.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
               File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: rwBuffer, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                sourceStream.Close();
            };
        }
        #endregion
        #region HTTP_FUNCS
        public static async Task<string> GetWebPageAsync(string url)
        {
            return await new HttpClient().GetStringAsync(url);
        }
        #endregion
        #region File
        public static string[] FindFiles(string path, string pattern)
        {
            return Directory.GetFiles(path, pattern);
        }
        #endregion
        #region Math
        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }
        #endregion
        #region Net
        public static IPEndPoint IPEndPointFromString(string ipep)
        {
            string[] split = ipep.Split(':');

            return new IPEndPoint(IPAddress.Parse(split[0]), int.Parse(split[1]));
        }
        #endregion
        #region Compression
        public static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
        #endregion
        #region BYTES
        public static string FormatBytes(long bytes)
        {
            if (bytes >= 0x1000000000000000) { return ((double)(bytes >> 50) / 1024).ToString("000.0000 EB"); }
            if (bytes >= 0x4000000000000) { return ((double)(bytes >> 40) / 1024).ToString("000.0000 PB"); }
            if (bytes >= 0x10000000000) { return ((double)(bytes >> 30) / 1024).ToString("000.0000 TB"); }
            if (bytes >= 0x40000000) { return ((double)(bytes >> 20) / 1024).ToString("000.0000 GB"); }
            if (bytes >= 0x100000) { return ((double)(bytes >> 10) / 1024).ToString("000.000 MB"); }
            if (bytes >= 0x400) { return ((double)bytes / 1024).ToString("000.000 KB"); }
            return bytes.ToString("000 Bytes ");
        }
        public static string FormatBits(long bytes)
        {
            bytes *= 8;
            if (bytes >= 0x1000000000000000) { return ((double)(bytes >> 50) / 1024).ToString("000.0000 Eb"); }
            if (bytes >= 0x4000000000000) { return ((double)(bytes >> 40) / 1024).ToString("000.0000 Pb"); }
            if (bytes >= 0x10000000000) { return ((double)(bytes >> 30) / 1024).ToString("000.0000 Tb"); }
            if (bytes >= 0x40000000) { return ((double)(bytes >> 20) / 1024).ToString("000.0000 Gb"); }
            if (bytes >= 0x100000) { return ((double)(bytes >> 10) / 1024).ToString("000.000 Mb"); }
            if (bytes >= 0x400) { return ((double)bytes / 1024).ToString("000.000 Kb"); }
            return bytes.ToString("000 Bits  ");
        }
        public static string FormatCount(long count)
        {
            if (count >= 0x1000000000000000) { return ((double)(count >> 50) / 1024).ToString("0.00 E"); }
            if (count >= 0x4000000000000) { return ((double)(count >> 40) / 1024).ToString("0.00 P"); }
            if (count >= 0x10000000000) { return ((double)(count >> 30) / 1024).ToString("0.00 T"); }
            if (count >= 0x40000000) { return ((double)(count >> 20) / 1024).ToString("0.00 G"); }
            if (count >= 0x100000) { return ((double)(count >> 10) / 1024).ToString("0.00 M"); }
            if (count >= 0x400) { return ((double)count / 1024).ToString("0.00") + " K"; }
            return count.ToString("0");
        }
        #endregion
        #region HASH
        public static string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString().ToLower();

        }

        public static int CalculateHash(string input)
        {
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            return BitConverter.ToInt32(hash, 0);
        }
        #endregion
        #region JSON
        public static object ConvertToJson(this object obj)
        {
            try
            {
                var json = JObject.FromObject(obj);
                return json;
            }
            catch
            {
                return obj.ToString();
            }
        }
        public static JObject ToJObject(this object _object)
        {
            if (_object == null)
                return null;
            else
                return JObject.FromObject(_object);
        }
        public static JObject ToJObject(this string jsonData)
        {
            if (jsonData == null)
                return null;
            else
            {
                try
                {
                    return JObject.Parse(jsonData);
                }
                catch
                {
                    return null;
                }
            }
        }
        public static string AsStringDef(this JObject jObject, string name, string def = null)
        {
            if (jObject == null)
                return def;
            else
            {
                try
                {
                    if (jObject[name] != null)
                        return (string)jObject[name];
                    else
                        return def;
                }
                catch
                {
                    return def;
                }
            }
        }
        public static int AsIntDef(this JObject jObject, string name, int def = 0)
        {
            if (jObject == null)
                return def;
            else
            {
                try
                {
                    if (jObject[name] != null)
                        return (int)jObject[name];
                    else
                        return def;
                }
                catch
                {
                    return def;
                }
            }
        }
        public static bool AsBoolDef(this JObject jObject, string name, bool def = false)
        {
            if (jObject == null)
                return def;
            else
            {
                try
                {
                    if (jObject[name] != null)
                        return (bool)jObject[name];
                    else
                        return def;
                }
                catch
                {
                    return def;
                }
            }
        }
        public static bool? AsNullableBoolDef(this JObject jObject, string name)
        {
            if (jObject == null)
                return null;
            else
            {
                try
                {
                    if (jObject[name] != null)
                        return (bool)jObject[name];
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static List<string> ArrayShared(List<string> array_a, List<string> array_b)
        {
            List<string> result = new List<string>();
            foreach (var i in array_a)
            {
                if (array_b.Contains(i))
                    result.Add(i);
            }
            return result;
        }
        public static List<string> ArrayCombine(List<string> array_a, List<string> array_b)
        {
            List<string> result = array_a.ToList();
            foreach (var i in array_b)
            {
                result.Add(i);
            }
            return result.Distinct().ToList();
        }
        public static JObject CombineJson(JObject object1, JObject object2)
        {
            object1.Merge(object2, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
            return object1;
        }
        #endregion
        #region DATE_TIME
        public static string TimeSpanFormat(TimeSpan timeSpan)
        {

            if (timeSpan.TotalDays >= 1)
                return timeSpan.Days + " Days";
            else
            if (timeSpan.TotalHours >= 1)
                return timeSpan.Hours + "h " + timeSpan.Minutes + "m " + timeSpan.Seconds + "s";
            else
                return timeSpan.Minutes + "m " + timeSpan.Seconds + "s";
        }
        public static DateTime Max(DateTime d1, DateTime d2)
        {
            if (d1 > d2)
                return d1;
            else
                return d2;
        }
        public static DateTime EndOfWeek(DateTime dateTime, CallenderType callenderType)
        {
            return StartOfWeek(dateTime, callenderType).AddDays(7);
        }
        public static DateTime StartOfWeek(DateTime dateTime, CallenderType callenderType)
        {
            DayOfWeek firstDayOfWeek = callenderType == CallenderType.Shamsi ? DayOfWeek.Saturday : DayOfWeek.Monday;

            int diff = (7 + (dateTime.DayOfWeek - firstDayOfWeek)) % 7;
            return dateTime.AddDays(-1 * diff).Date;
        }
        public enum CallenderType
        {
            Shamsi,
            Gregorian,
        }
        #endregion
        #region List
        //public static List<T> CloneList<T>(List<T> oldList)
        //{
        //    BinaryFormatter formatter = new BinaryFormatter();
        //    MemoryStream stream = new MemoryStream();
        //    formatter.Serialize(stream, oldList);
        //    stream.Position = 0;
        //    return (List<T>)formatter.Deserialize(stream);
        //}
        #endregion
        #region WEB
        public static string HtmlToText(string HTMLCode)
        {
            if (HTMLCode == null)
                return "";
            // Remove new lines since they are not visible in HTML
            HTMLCode = HTMLCode.Replace("\n", " ");

            // Remove tab spaces
            HTMLCode = HTMLCode.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

            // Remove HEAD tag
            HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", ""
                                , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", ""
              , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(HTMLCode);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = {"&nbsp;", "&amp;", "&quot;", "&lt;",
   "&gt;", "&reg;", "&copy;", "&bull;", "&trade;","&#39;"};
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "\'" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("<p ", "\n<p ");

            // Finally, remove_by_ids all HTML tags and return plain text
            return Regex.Replace(
              sbHTML.ToString(), "<[^>]*>", "");
        }
        public static string SlugMaker(string text)
        {
            if (text == null)
                text = "";

            return text.Trim()
                .Replace(' ', '-')
                .Replace("/", "")
                .Replace("\\", "").ToLower();
        }
        public static string SlugMaker(string text, string id)
        {
            return $"{SlugMaker(text)}-{CalculateMD5Hash(id).Substring(0, 6)}";
        }
        public static string AppendUtrParam(string link, string param, string value)
        {
            string longurl = link;
            var uriBuilder = new UriBuilder(longurl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[param] = value;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
        public static string AppendUtrParam(string link, string param1, string value1, string param2, string value2)
        {
            string longurl = link;
            var uriBuilder = new UriBuilder(longurl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[param1] = value1;
            query[param2] = value2;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
        public static string AppendUtrParam(string link, string param1, string value1, string param2, string value2, string param3, string value3)
        {
            string longurl = link;
            var uriBuilder = new UriBuilder(longurl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[param1] = value1;
            query[param2] = value2;
            query[param3] = value3;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
        #endregion
        #region MongoDB
        public static ObjectId ToObjectId(this string id)
        {
            if (id == null)
                return ObjectId.Empty;

            if (ObjectId.TryParse(id, out var _id))
                return _id;
            else
                return ObjectId.Empty;

        }
        public static ObjectId? ToObjectIdNullable(this string id)
        {
            if (id == null)
                return null;

            if (ObjectId.TryParse(id, out var _id))
                return _id;
            else
                return null;

        }
        public static async Task<List<TSource>> PaginationAsync<TSource>(this IMongoQueryable<TSource> source, Pagination pagination)
        {
            return await source.Skip(pagination.Skip).Take(pagination.Take).ToListAsync();
        }
        #endregion
        #region Email
        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[\w\.-]+@[\w\.-]+\.\w+$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(email);
        }
        #endregion
    }
}
