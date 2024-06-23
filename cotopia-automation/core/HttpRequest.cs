using Newtonsoft.Json.Linq;
using srtool;
using System.Text;

namespace blueprint.core
{
    public class HttpRequest
    {
        public static async Task<HttpResponse> RequestAsync(HttpRequestType httpRequestType, string url, RequestData data)
        {
            HttpResponse httpResponse = new HttpResponse();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (data == null)
                        data = new RequestData();



                    if (data.queryParam != null)
                        foreach (var i in data.queryParam)
                            url = Utility.AppendUtrParam(url, i.Key, i.Value);

                    HttpRequestMessage request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;

                    if (httpRequestType != HttpRequestType.Get && httpRequestType != HttpRequestType.Delete)
                    {
                        if (data.json != null)
                        {
                            string json = data.json.ToString();
                            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        }
                        else
                        {
                            request.Content = new FormUrlEncodedContent(data.contentParam);
                        }
                    }

                    if (data.headerParam != null)
                        foreach (var i in data.headerParam)
                            client.DefaultRequestHeaders.Add(i.Key, i.Value);

                    switch (httpRequestType)
                    {
                        case HttpRequestType.Get:
                            {
                                var response = await client.GetAsync(url);

                                httpResponse.StatusCode = (int)response.StatusCode;
                                var responseString = await response.Content.ReadAsStringAsync();

                                httpResponse.Data = responseString;
                            }
                            break;
                        case HttpRequestType.Post:
                            {
                                var response = await client.PostAsync(url, request.Content);

                                httpResponse.StatusCode = (int)response.StatusCode;
                                var responseString = await response.Content.ReadAsStringAsync();

                                httpResponse.Data = responseString;
                            }
                            break;
                        case HttpRequestType.Put:
                            {
                                var response = await client.PutAsync(url, request.Content);

                                httpResponse.StatusCode = (int)response.StatusCode;
                                var responseString = await response.Content.ReadAsStringAsync();

                                httpResponse.Data = responseString;
                            }
                            break;
                        case HttpRequestType.Delete:
                            {
                                var response = await client.DeleteAsync(url);

                                httpResponse.StatusCode = (int)response.StatusCode;
                                var responseString = await response.Content.ReadAsStringAsync();

                                httpResponse.Data = responseString;
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                httpResponse.HttpError = true;
                //
                httpResponse.Error = e.Message;
            }

            return httpResponse;
        }
    }
    public enum HttpRequestType
    {
        Get, Post, Delete, Put
    }
    public class RequestData
    {

        public Dictionary<string, string> contentParam = new Dictionary<string, string>();
        public Dictionary<string, string> headerParam = new Dictionary<string, string>();
        public Dictionary<string, string> queryParam = new Dictionary<string, string>();


        public string Authorization
        {
            set
            {
                AddHeaderParam("Authorization", value);
            }
            get
            {
                if (headerParam.TryGetValue("Authorization", out var _out))
                    return _out;
                else
                    return null;
            }
        }
        public void AddContent(string key, string value)
        {
            contentParam.Remove(key);
            contentParam.Add(key, value);
        }
        public void AddHeaderParam(string key, string value)
        {
            headerParam.Remove(key);
            headerParam.Add(key, value);
        }

        public void AddQueryParam(string key, string value)
        {
            queryParam.Remove(key);
            queryParam.Add(key, value);
        }
        public JObject json { get; set; }
        public void SetJson(JObject json)
        {
            this.json = json;
        }
        public void AddJsonParam(string key, string value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
        public void AddJsonParam(string key, int value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
        public void AddJsonParam(string key, bool value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
        public void AddJsonParam(string key, DateTime value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
        public void AddJsonParam(string key, float value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
        public void AddJsonParam(string key, JObject value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
        public void AddJsonParam(string key, JArray value)
        {
            if (json == null)
                json = new JObject();

            json[key] = value;
        }
    }
    public class HttpResponse
    {
        public bool HttpError { get; set; }
        public int StatusCode { get; set; }
        public string Error { get; set; }
        public string Data { get; set; }
    }

}
