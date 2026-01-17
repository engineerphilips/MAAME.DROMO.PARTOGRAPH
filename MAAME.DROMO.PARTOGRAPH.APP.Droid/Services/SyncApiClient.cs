using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services.Helper;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// HTTP API client for sync operations with retry logic and JWT authentication
/// </summary>
public class SyncApiClient : ISyncApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService; 
    private readonly ILogger<SyncApiClient> _logger;
    private readonly IServiceRequestProvider _serviceRequestProvider;
    private readonly JsonSerializerOptions _jsonOptions;
    private const int MaxRetries = 3;

    public SyncApiClient(
        HttpClient httpClient,
        IAuthenticationService authService,
        IServiceRequestProvider serviceRequestProvider,
        ILogger<SyncApiClient> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _serviceRequestProvider = serviceRequestProvider;
        _logger = logger;

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Sets the Authorization header with the current access token
    /// </summary>
    private async Task<bool> SetAuthorizationHeaderAsync()
    {
        try
        {
            var token = await _authService.GetValidAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                return true;
            }

            _logger.LogWarning("No valid access token available for sync request");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting authorization header");
            return false;
        }
    }

    /// <summary>
    /// Executes an HTTP request with exponential backoff retry logic and token refresh
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<Task<HttpResponseMessage>> action,
        bool requiresAuth = false)
    {
        Exception? lastException = null;

        // Set authorization header before first attempt
        if (requiresAuth)
        {
            await SetAuthorizationHeaderAsync();
        }

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var response = await action();

                // Handle 401 Unauthorized - try to refresh token and retry
                if (response.StatusCode == HttpStatusCode.Unauthorized && requiresAuth && attempt == 0)
                {
                    _logger.LogInformation("Received 401, attempting token refresh");

                    if (await _authService.RefreshAccessTokenAsync())
                    {
                        await SetAuthorizationHeaderAsync();
                        // Retry the request with new token
                        response = await action();
                    }
                }

                // Retry on server errors (5xx) only
                if (response.IsSuccessStatusCode || (int)response.StatusCode < 500)
                {
                    return response;
                }

                lastException = new HttpRequestException($"Server error: {response.StatusCode}");
                _logger.LogWarning("Retry {Attempt}/{MaxRetries} due to server error: {StatusCode}",
                    attempt + 1, MaxRetries, response.StatusCode);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Retry {Attempt}/{MaxRetries} due to exception", attempt + 1, MaxRetries);
            }

            // Don't delay on the last attempt
            if (attempt < MaxRetries - 1)
            {
                var delaySeconds = Math.Pow(2, attempt); // Exponential backoff: 1s, 2s, 4s
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        throw new SyncException($"Failed after {MaxRetries} retries", lastException!);
    }

    /// <inheritdoc/>
    public string BaseUrl => _httpClient.BaseAddress?.ToString() ?? "Not configured";

    /// <inheritdoc/>
    public async Task<SyncPullResponse<T>> PullAsync<T>(SyncPullRequest request) where T : BasePartographMeasurement
    {
        try
        {
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync($"api/sync/pull/{request.TableName}", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<T>>();
            return result ?? new SyncPullResponse<T> { Records = new List<T>(), ServerTimestamp = 0, HasMore = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling data for table {TableName}", request.TableName);
            throw new SyncException($"Failed to pull data for {request.TableName}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncPushResponse<T>> PushAsync<T>(SyncPushRequest<T> request) where T : BasePartographMeasurement
    {
        try
        {
            var tableName = typeof(T).Name;
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync($"api/sync/push/{tableName}", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPushResponse<T>>();
            return result ?? new SyncPushResponse<T>
            {
                SuccessIds = new List<string>(),
                Conflicts = new List<ConflictRecord<T>>(),
                Errors = new List<SyncError>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing data for type {TypeName}", typeof(T).Name);
            throw new SyncException($"Failed to push data for {typeof(T).Name}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncPullResponse<Patient>> PullPatientsAsync(SyncPullRequest request)
    {
        try
        {
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync("api/sync/pull/patients", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<Patient>>();
            return result ?? new SyncPullResponse<Patient> { Records = new List<Patient>(), ServerTimestamp = 0, HasMore = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling patient data");
            throw new SyncException("Failed to pull patient data", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncPushResponse<Patient>> PushPatientsAsync(SyncPushRequest<Patient> request)
    {
        try
        {
            //await _serviceRequestProvider.PostAsync<SyncPushRequest<Patient>>("api/sync/push/patients", request)
            var response = await ExecuteWithRetryAsync(async () =>
            await _httpClient.PostAsJsonAsync("api/sync/push/patients", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPushResponse<Patient>>();
            return result ?? new SyncPushResponse<Patient>
            {
                SuccessIds = new List<string>(),
                Conflicts = new List<ConflictRecord<Patient>>(),
                Errors = new List<SyncError>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing patient data");
            throw new SyncException("Failed to push patient data", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncPullResponse<Partograph>> PullPartographsAsync(SyncPullRequest request)
    {
        try
        {
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync("api/sync/pull/partographs", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<Partograph>>();
            return result ?? new SyncPullResponse<Partograph> { Records = new List<Partograph>(), ServerTimestamp = 0, HasMore = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling partograph data");
            throw new SyncException("Failed to pull partograph data", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncPushResponse<Partograph>> PushPartographsAsync(SyncPushRequest<Partograph> request)
    {
        try
        {
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync("api/sync/push/partographs", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPushResponse<Partograph>>();
            return result ?? new SyncPushResponse<Partograph>
            {
                SuccessIds = new List<string>(),
                Conflicts = new List<ConflictRecord<Partograph>>(),
                Errors = new List<SyncError>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing partograph data");
            throw new SyncException("Failed to push partograph data", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SyncPullResponse<Staff>> PullStaffAsync(SyncPullRequest request)
    {
        try
        {
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync("api/sync/pull/staff", request)
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Sync failed: {response.StatusCode} - {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<Staff>>();
            return result ?? new SyncPullResponse<Staff> { Records = new List<Staff>(), ServerTimestamp = 0, HasMore = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling staff data");
            throw new SyncException("Failed to pull staff data", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            //Health check doesn't require authentication
            var response = await ExecuteWithRetryAsync(
                async () => await _httpClient.GetAsync("api/sync/health"),
                requiresAuth: false
            );
            return response.IsSuccessStatusCode;

            //var data = await _provider.GetAsync<bool>($"api/sync/health");
            //return data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed");
            return false;
        }
    }
}

/// <summary>
/// Exception thrown when sync operations fail
/// </summary>
public class SyncException : Exception
{
    public SyncException(string message) : base(message) { }
    public SyncException(string message, Exception innerException) : base(message, innerException) { }
}
