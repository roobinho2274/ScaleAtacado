using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ScaleAtacado.Blazor;
using ScaleAtacado.Blazor.Auth;
using ScaleAtacado.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
var baseUri = string.IsNullOrWhiteSpace(apiUrl)
    ? new Uri(builder.HostEnvironment.BaseAddress)  // produção: usa URL do nginx
    : new Uri(apiUrl);                               // desenvolvimento: usa appsettings.json
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = baseUri });

builder.Services.AddMudServices();

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ApiHttpClient>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<PaymentMethodService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddSingleton<CompanyStateService>();

await builder.Build().RunAsync();
