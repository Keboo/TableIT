using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TableIT.Client;
using TableIT.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddHttpClient("TableIT.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("TableIT.ServerAPI"));

//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration.GetSection("ServerApi")["Scopes"]);
//});

builder.Services.AddSingleton<ITableViewerConnection>(x => new TableViewerConnection(x.GetRequiredService<NavigationManager>().ToAbsoluteUri("/TableHub")));
builder.Services.AddSingleton<ITableRemoteConnection>(x => new TableRemoteConnection(x.GetRequiredService<NavigationManager>().ToAbsoluteUri("/TableHub")));
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddHttpClient<IImageService, ImageService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
