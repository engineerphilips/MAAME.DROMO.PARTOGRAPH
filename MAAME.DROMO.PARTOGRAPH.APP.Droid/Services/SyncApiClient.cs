using System.Net.Http.Json;
using System.Text.Json;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// HTTP API client for sync operations with retry logic
/// </summary>
public class SyncApiClient : ISyncApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SyncApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private const int MaxRetries = 3;

    public SyncApiClient(HttpClient httpClient, ILogger<SyncApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Executes an HTTP request with exponential backoff retry logic
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> action)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var response = await action();

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
                await _httpClient.PostAsJsonAsync($"api/sync/pull/{request.TableName}", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<T>>(_jsonOptions);
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
                await _httpClient.PostAsJsonAsync($"api/sync/push/{tableName}", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPushResponse<T>>(_jsonOptions);
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
                await _httpClient.PostAsJsonAsync("api/sync/pull/patients", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<Patient>>(_jsonOptions);
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
            var response = await ExecuteWithRetryAsync(async () =>
                await _httpClient.PostAsJsonAsync("api/sync/push/patients", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPushResponse<Patient>>(_jsonOptions);
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
                await _httpClient.PostAsJsonAsync("api/sync/pull/partographs", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<Partograph>>(_jsonOptions);
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
                await _httpClient.PostAsJsonAsync("api/sync/push/partographs", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPushResponse<Partograph>>(_jsonOptions);
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
                await _httpClient.PostAsJsonAsync("api/sync/pull/staff", request, _jsonOptions)
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncPullResponse<Staff>>(_jsonOptions);
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
            var response = await _httpClient.GetAsync("api/sync/health");
            return response.IsSuccessStatusCode;
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
