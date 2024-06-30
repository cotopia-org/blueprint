using Amazon.Runtime.Internal;
using blueprint.core;
using Microsoft.AspNetCore.Http;
using srtool;
using System;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

public class IAsyncErrorMiddleware
{
    private readonly RequestDelegate _next;

    public IAsyncErrorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch
        {
            throw;
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, AppException exception)
    {
        context.Response.StatusCode = (int)exception.data.status;
        var jsonErrorResponse = JsonSerializer.Serialize(exception.data);
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(jsonErrorResponse);
    }
}

public class AppException : Exception
{
    public AppExceptionData data;
    public AppException(HttpStatusCode httpStatusCode)
    {
        data = new AppExceptionData();
        data.type = "blueprint";
        data.title = "One or more errors have occurred.";
        data.traceId = Utility.CalculateMD5Hash(Guid.NewGuid().ToString()).ToLower();
        data.status = httpStatusCode;
        data.statusName = httpStatusCode.ToString();

        data.hints = new List<AppExceptionParam>();
    }
    public void AddHint(string name, string message, object value = null)
    {
        data.hints.Add(new AppExceptionParam() { name = name, message = message, value = value });
    }


    public static AppException ForbiddenAccessObject()
    {
        var appE = new AppException(HttpStatusCode.Forbidden);
        appE.AddHint("access", "You do not have permission to access the requested object.");
        return appE;
    }
    public static AppException NotFoundObject()
    {
        var appE = new AppException(HttpStatusCode.NotFound);
        appE.AddHint("object", "Not found object.");
        return appE;
    }
}
public class AppExceptionData
{
    public string type { get; set; }
    public string title { get; set; }
    public string traceId { get; set; }
    public HttpStatusCode status { get; set; }
    public string statusName { get; set; }

    public List<AppExceptionParam> hints { get; set; }
}
public class AppExceptionParam
{
    public string name { get; set; }
    public string message { get; set; }
    public object value { get; set; }
}