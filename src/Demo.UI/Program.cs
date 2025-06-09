var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationStateProvider>();

builder.Services.AddScoped<Settings>(_
    => new(builder.Configuration.GetSection("ApiUrl").Value!));

builder.Services.AddTransient<ApiService>();

builder.Services.AddSysinfocus();

await builder.Build().RunAsync();