using IdentityModel.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SignalR.ExampleClient
{
    public sealed class Consumer
    {
        private HubConnection _hubConnection;
        private MemoryCache _cache;
        private readonly string _cacheKey = "TOKEN_CACHE_KEY";
        private static HttpClient _httpClient = new HttpClient();
        public Consumer()
        {
            _cache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromMinutes(3) });
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri($"https://dev-dis.egki.dk/caseHub"), o =>
                {
                    o.AccessTokenProvider = async () =>
                    {
                        return await _cache.GetOrCreateAsync(_cacheKey, async (cacheEntry) =>
                        {
                            if (cacheEntry != null && cacheEntry?.Value != null)
                                return cacheEntry.Value!.ToString()!;

                            var response = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                            {
                                ClientId = "CLIENT_ID",
                                ClientSecret = "CLIENT_SECRET",
                                Address = "https://dev-sts.egki.dk/auth/realms/egki/protocol/openid-connect/token",
                            }, CancellationToken.None);

                            cacheEntry!.SetValue(response.AccessToken);

                            return response.AccessToken;
                        });
                    };
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>(
                "CaseCreatedNotificationAsync", OnCaseCreatedNotificationReceivedAsync);

            _hubConnection.On<string>(
                "CaseUpdatedNotificationAsync", OnCaseUpdatedNotificationReceivedAsync);

            _hubConnection.On<string>(
                "CaseDeletedNotificationAsync", OnCaseDeletedNotificationReceivedAsync);
        }

        public async Task StartNotificationConnectionAsync()
        {
            await _hubConnection.StartAsync();

            Console.WriteLine("Connected to SignalR hub");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task OnCaseCreatedNotificationReceivedAsync(string caseId)
        {
            Console.WriteLine($"Created: {caseId}");
        }

        private async Task OnCaseUpdatedNotificationReceivedAsync(string caseId)
        {
            Console.WriteLine($"Updated: {caseId}");
        }

        private async Task OnCaseDeletedNotificationReceivedAsync(string caseId)
        {
            Console.WriteLine($"Deleted: {caseId}");
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
