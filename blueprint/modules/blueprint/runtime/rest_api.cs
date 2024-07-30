using Microsoft.ClearScript;
using srtool;

namespace blueprint.modules.blueprint.runtime
{
    public class rest_api
    {
        public async Task get(string url, ScriptObject callback)
        {
            var result = new response();
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
        public class response
        {
            public string data;
            public bool success;
            public int statusCode;
        }
    }

    public class log
    {
        public void add(object item)
        {
            Debug.Log(item);
        }
    }
    //public async Task delay(double sec, ScriptObject action)
    //{
    //    try
    //    {
    //        await Task.Delay(TimeSpan.FromSeconds(sec));
    //        action.InvokeAsFunction(new object[] { });
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.Error(ex);
    //    }
    //}
}
