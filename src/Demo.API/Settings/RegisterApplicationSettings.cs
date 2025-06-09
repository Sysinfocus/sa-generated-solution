namespace Demo.API.Settings;

public static class RegisterApplicationSettings
{    
    public static IServiceCollection AddDependenciesFor(this IServiceCollection services,
        Type serviceType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var isGenericType = serviceType.Name.Contains('`');

        if (isGenericType)
        {
            var registerServices = typeof(Program).Assembly.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType)
                    .Select(i => new { Interface = i, Implementation = t }))
                .ToList();

            foreach (var service in registerServices)
            {
                if (serviceLifetime == ServiceLifetime.Transient) services.AddTransient(service.Interface, service.Implementation);
                else if (serviceLifetime == ServiceLifetime.Scoped) services.AddScoped(service.Interface, service.Implementation);
                else if (serviceLifetime == ServiceLifetime.Singleton) services.AddSingleton(service.Interface, service.Implementation);
            }
        }
        else
        {
            var registerServices = typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && t.IsAssignableTo(serviceType))
                .SelectMany(t => t.GetInterfaces()
                    .Select(i => new { Interface = i, Implementation = t }))
                .ToList();
                        
            foreach (var service in registerServices)
            {
                if (serviceLifetime == ServiceLifetime.Transient) services.AddTransient(service.Interface, service.Implementation);
                else if (serviceLifetime == ServiceLifetime.Scoped) services.AddScoped(service.Interface, service.Implementation);
                else if (serviceLifetime == ServiceLifetime.Singleton) services.AddSingleton(service.Interface, service.Implementation);
            }
        }

        return services;
    }

    public static void MapEndpoints(this WebApplication app)
    {
        var types = typeof(Program).Assembly.GetTypes()
            .Where(a => a.IsAssignableTo(typeof(IEndpoints)) && !a.IsInterface);

        var scope = app.Services.CreateScope().ServiceProvider;
        var services = scope.GetServices<IEndpoints>();
        foreach (var service in services)
            service.Register(app);
    }
}