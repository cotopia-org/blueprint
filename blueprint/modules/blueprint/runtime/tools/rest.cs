using Microsoft.ClearScript;

namespace blueprint.modules.blueprint.runtime.tools
{
    public class rest
    {
        public async Task get(string url, ScriptObject callback)
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

                    result.success = true;
                    result.data = responseBody;

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                    result.success = false;
                }

                callback.InvokeAsFunction(new object[] { result });
            }
        }

    }
    public class rest_response
    {
        public string data;
        public bool success;
        public int statusCode;
    }
}
