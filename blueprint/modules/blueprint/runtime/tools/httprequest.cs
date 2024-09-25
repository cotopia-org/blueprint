using Microsoft.ClearScript;
using System.Net;

namespace blueprint.modules.blueprint.runtime.tools
{
    public class httprequest
    {
        public static async void get(string url, Action<rest_response> callback)
        {
            var result = new rest_response();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var res = await client.GetAsync(url);
                    result.statusCode = (int)res.StatusCode;
                    res.EnsureSuccessStatusCode(); // Ensure success status code (200-299)
                    var responseBody = await res.Content.ReadAsStringAsync();
                    result.content = responseBody;
                }
                catch (HttpRequestException e)
                {
                    result.statusCode = (int)HttpStatusCode.Conflict;
                }

                callback(result);
            }
        }
        public async void delete(string url, Action<rest_response> callback)
        {
            var result = new rest_response();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var res = await client.DeleteAsync(url);
                    result.statusCode = (int)res.StatusCode;
                    res.EnsureSuccessStatusCode(); // Ensure success status code (200-299)
                    var responseBody = await res.Content.ReadAsStringAsync();
                    result.content = responseBody;
                }
                catch (HttpRequestException e)
                {
                    result.statusCode = (int)e.StatusCode;
                    Console.WriteLine($"Error: {e.Message}");
                }

                callback(result);
            }
        }
    }
    public class rest_response
    {
        public string content;
        public int statusCode;
    }
}
