using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KriPoint.Middleware;

namespace KriPoint;

public sealed class KriPointOptions
{
    public string   AesKey                 { get; set; } = string.Empty;
    public bool     RejectOnDecryptFailure { get; set; } = true;
    public string[] ExcludePaths           { get; set; } = [];
}

public static class KriPointExtensions
{
    public static IServiceCollection AddKriPoint(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<KriPointOptions>? configure = null)
    {
        var options = new KriPointOptions();
        configuration.GetSection("KriPoint").Bind(options);
        configure?.Invoke(options);

        Validate(options);

        services.AddSingleton(options);
        services.AddSingleton<IKriPointEncryptionService>(
            _ => new KriPointEncryptionService(options.AesKey));

        return services;
    }

    public static IServiceCollection AddKriPoint(
        this IServiceCollection services,
        Action<KriPointOptions> configure)
    {
        var options = new KriPointOptions();
        configure(options);

        Validate(options);

        services.AddSingleton(options);
        services.AddSingleton<IKriPointEncryptionService>(
            _ => new KriPointEncryptionService(options.AesKey));

        return services;
    }

    public static IApplicationBuilder UseKriPoint(this IApplicationBuilder app)
        => app.UseMiddleware<KriPointMiddleware>();

    private static void Validate(KriPointOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AesKey))
            throw new InvalidOperationException(
                "[KriPoint] AesKey is not configured. " +
                "Set KriPoint:AesKey in appsettings.json or the KriPoint__AesKey environment variable.");
    }
}
