using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Text.Json.Serialization;
using TableIT.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.RoleClaimType =
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    },
    options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSignalR(hubOptions => hubOptions.EnableDetailedErrors = true)
                .AddJsonProtocol(
                    options =>
                    {
                        options.PayloadSerializerOptions.PropertyNamingPolicy =
                            System.Text.Json.JsonNamingPolicy.CamelCase;
                        options.PayloadSerializerOptions.ReferenceHandler =
                            ReferenceHandler.IgnoreCycles;
                    })
                .AddHubOptions<TableHub>(
                    options =>
                    {
                        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                        options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
                    });
builder.Services.AddSingleton<ITableManager, InMemoryTableManager>();

builder.Services.AddSingleton<TokenCredential>(_ => new DefaultAzureCredential());
builder.Services.AddSingleton(x =>
{
    return new BlobContainerClient(new Uri("https://tableitstorage.blob.core.windows.net/resources/"), x.GetRequiredService<TokenCredential>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapHub<TableHub>($"/{nameof(TableHub)}");

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
