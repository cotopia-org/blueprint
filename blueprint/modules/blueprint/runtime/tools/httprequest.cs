using Microsoft.ClearScript;

namespace blueprint.modules.blueprint.runtime.tools
{
    public static class httprequest
    {
        public static async Task get(string url, ScriptObject callback)
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
                    result.statusCode = (int)e.StatusCode;
                    Console.WriteLine($"Error: {e.Message}");
                }

                callback.InvokeAsFunction(new object[] { result });
            }
        }
    }
    public class rest_response
    {
        public string content;
        public int statusCode;
    }
}
