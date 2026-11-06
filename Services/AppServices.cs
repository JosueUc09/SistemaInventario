using Microsoft.Extensions.Configuration;

namespace InventarioEscolar.Services;

public static class AppServices
{
    public static SupabaseApiService Supabase { get; private set; } = null!;

    public static void Configure()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var url = configuration["Supabase:Url"];
        var key = configuration["Supabase:PublishableKey"];
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key) || url.Contains("TU-PROYECTO"))
            throw new InvalidOperationException("Configure Supabase:Url y Supabase:PublishableKey en appsettings.json.");
        Supabase = new SupabaseApiService(url, key);
    }
}
