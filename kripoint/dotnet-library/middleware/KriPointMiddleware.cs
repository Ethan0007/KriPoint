using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KriPoint.Middleware;

public sealed class KriPointMiddleware
{
    private readonly RequestDelegate             _next;
    private readonly IKriPointEncryptionService  _encryption;
    private readonly KriPointOptions             _options;
    private readonly ILogger<KriPointMiddleware> _logger;

    public KriPointMiddleware(
        RequestDelegate next,
        IKriPointEncryptionService encryption,
        KriPointOptions options,
        ILogger<KriPointMiddleware> logger)
    {
        _next       = next;
        _encryption = encryption;
        _options    = options;
        _logger     = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldDecrypt(context))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        string rawBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            await _next(context);
            return;
        }

        KriPointPayload? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<KriPointPayload>(rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            context.Request.Body.Position = 0;
            await _next(context);
            return;
        }

        if (envelope is null ||
            string.IsNullOrWhiteSpace(envelope.Payload) ||
            string.IsNullOrWhiteSpace(envelope.Iv))
        {
            context.Request.Body.Position = 0;
            await _next(context);
            return;
        }

        string decryptedJson;
        try
        {
            decryptedJson = _encryption.DecryptToJson(envelope);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[KriPoint] Failed to decrypt request on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (_options.RejectOnDecryptFailure)
            {
                context.Response.StatusCode  = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("""{"error":"Invalid KriPoint payload"}""");
                return;
            }

            context.Request.Body.Position = 0;
            await _next(context);
            return;
        }

        _logger.LogDebug("[KriPoint] Decrypted request on {Method} {Path}",
            context.Request.Method, context.Request.Path);

        var decryptedBytes = Encoding.UTF8.GetBytes(decryptedJson);
        context.Request.Body          = new MemoryStream(decryptedBytes);
        context.Request.ContentLength = decryptedBytes.Length;
        context.Request.ContentType   = "application/json; charset=utf-8";
        context.Request.Headers.Remove("X-KriPoint");

        await _next(context);
    }

    private bool ShouldDecrypt(HttpContext ctx)
    {
        var method = ctx.Request.Method;

        if (HttpMethods.IsGet(method)    ||
            HttpMethods.IsHead(method)   ||
            HttpMethods.IsOptions(method))
            return false;

        var contentType = ctx.Request.ContentType ?? string.Empty;
        if (!contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            return false;

        var path = ctx.Request.Path.Value ?? string.Empty;
        foreach (var excluded in _options.ExcludePaths)
            if (path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase))
                return false;

        return true;
    }
}
