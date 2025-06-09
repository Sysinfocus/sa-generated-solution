namespace Demo.API.Settings;

public static class StartupSettings
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors();

        builder.Services.AddDbContext<AppDbContext>(o =>
        {
            o.UseSqlite(builder.Configuration.GetConnectionString("Default")!);
            o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //o.EnableSensitiveDataLogging();
        });

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddIdentityApiEndpoints<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.AddDependenciesFor(typeof(IServices<,>));
        builder.Services.AddDependenciesFor(typeof(IEndpoints));
        builder.Services.AddSingleton<INotifications, NotificationService>();
        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();
        builder.Services.AddHybridCache();        
    }

    public static void UseServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapGet("/", () => Results.Content(
            """
            <body style='background-color:#333;color:#eee'>
                <p style='font-family:system-ui;padding:2rem;font-size:1.5rem;text-align:center'>
                    Welcome to <b>Demo</b> landing endpoint.<br/>
                    Click to open <a style='color:gold' href="/scalar">Scalar API Reference</a>
                </p>
            </body>
            """,
            "text/html"));        

        app.UseCors(c => c.AllowAnyOrigin()
            .AllowAnyMethod().AllowAnyHeader()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        var group = app.MapGroup("/api/v1/Auth");
        group.MapIdentityApi<IdentityUser>().WithTags("AuthEndpoints");
        app.MapEndpoints();
    }
}
