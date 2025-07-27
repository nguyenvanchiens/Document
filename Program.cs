using Blazored.LocalStorage;
using Document.ApiClient;
using Document.Web;
using Document.Web.Service;
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(
        typeof(Program).Assembly
    );
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://chiennv.runasp.net/") });

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();



builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddScoped<UserWebService>();
builder.Services.AddScoped<AccountApiClient>();
builder.Services.AddScoped<AccountService>();

await builder.Build().RunAsync();
