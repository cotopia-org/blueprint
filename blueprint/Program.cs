using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using blueprint.core;
using blueprint.modules.config;
using blueprint.modules._global;
using blueprint.modules.blueprintProcess.logic;
using blueprint.modules.schedule;


await SystemModule.Instance.RunAsync();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(
    jsonOptions => jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.
               Add(new JsonStringEnumConverter());
    })
    .AddNewtonsoftJson();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddRazorPages();
#region Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c) =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "blueprint",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 789234hgfwfhgdsghfs2425fdskmfsdf=\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});
#endregion
builder.Services.AddScoped<AuthRequire>();
builder.WebHost.UseUrls(ConfigModule.GetString("net.host"));
var app = builder.Build();
app.UseMiddleware<IAsyncErrorMiddleware>();
app.UseCors();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwaggerUI(c =>
{
    c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
});

if (ConfigModule.GetBool("swagger.active", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapRazorPages();
app.UseRouting();
app.UseWebSockets();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();
