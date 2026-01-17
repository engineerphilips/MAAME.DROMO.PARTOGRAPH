using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services.Helper
{
    public class ServiceRequestProvider : IServiceRequestProvider
    {
        private static string WEB_API_URL => WebApiConstants.WEB_API_URL;
        public async Task<T?> GetAsync<T>(string uri)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{WEB_API_URL}{uri}");
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ?  await SerializeDeserialize.DeserializeJsonFromStreamAsync<T>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }
        public async Task<TResult?> GetAsync<TResult, T>(string uri, T data)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{WEB_API_URL}{uri}");
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }
        public async Task<TResult?> GetExtAsync<TResult, T>(string uri, T data)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{WEB_API_URL}{uri}");
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }
        public async Task<TResult?> PostAsync<TResult, T>(string uri, T data)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{WEB_API_URL}{uri}")
                {
                    Content = SerializeDeserialize.CreateHttpContent(data)
                };
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }
        public async Task PostAsync(string uri)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{WEB_API_URL}{uri}");
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task<TResult?> PostAsync<TResult>(string uri)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{WEB_API_URL}{uri}");
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task<TResult?> PostAsync<TResult, T>(string uri, T data, TResult result)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{WEB_API_URL}{uri}")
                {
                    Content = SerializeDeserialize.CreateHttpContent(data)
                };
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task<TResult?> PutAsync<TResult, T>(string uri, T data)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, $"{WEB_API_URL}{uri}")
                {
                    Content = SerializeDeserialize.CreateHttpContent(data)
                };
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task<TResult?> PutAsync<TResult, T>(string uri, T data, TResult result)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, $"{WEB_API_URL}{uri}")
                {
                    Content = SerializeDeserialize.CreateHttpContent(data)
                };
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
                var stream = await response.Content.ReadAsStreamAsync();
                return response.IsSuccessStatusCode
                    ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
                    : default;
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
                return default;
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task PutAsync<T>(string uri, T? data)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, $"{WEB_API_URL}{uri}")
                {
                    Content = SerializeDeserialize.CreateHttpContent(data)
                };
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task PutAsync(string uri)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, $"{WEB_API_URL}{uri}");
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        public async Task PostAsync<T>(string uri, T? data)
        {
            var cancelTokenSource = new CancellationTokenSource();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{WEB_API_URL}{uri}")
                {
                    Content = SerializeDeserialize.CreateHttpContent(data)
                };
                using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
                using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
                await HandleResponse(response);
            }
            catch (OperationCanceledException e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            catch (Exception e)
            {
                cancelTokenSource?.Cancel();
                ErrorHandler.HandleException(e);
            }
            finally
            {
                cancelTokenSource?.Dispose();
            }
        }

        //public async Task<TResult> PostBLOBAsync<TResult, T>(string uri, FileInfo attachment)
        //{
        //    var cancelTokenSource = new CancellationTokenSource();
        //    try
        //    {
        //        using var content = new MultipartFormDataContent();

        //        if (attachment.Exists)
        //        {
        //            //var fileContent = new StreamContent(File.OpenRead(file.FullName));
        //            using var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(attachment.FullName));
        //            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

        //            // here it is important that second parameter matches with name given in API.
        //            content.Add(fileContent, "file", System.IO.Path.GetFileName(attachment.FullName));
                    
        //            using var request = new HttpRequestMessage(HttpMethod.Post, $"{WEB_API_URL}{uri}")
        //            {
        //                Content = content 
        //            };

        //            using var httpclient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        //            cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(60));
        //            using var response = await httpclient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelTokenSource.Token);
        //            await HandleResponse(response);
        //            var stream = await response.Content.ReadAsStreamAsync();
        //            return response.IsSuccessStatusCode
        //                ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
        //                : default;
        //        }

        //        return default;
        //    }
        //    catch (OperationCanceledException e)
        //    {
        //        cancelTokenSource?.Cancel();
        //        ErrorHandler.HandleException(e);
        //        return default;
        //    }
        //    catch (Exception e)
        //    {
        //        cancelTokenSource?.Cancel();
        //        ErrorHandler.HandleException(e);
        //        return default;
        //    }
        //    finally
        //    {
        //        cancelTokenSource?.Dispose();
        //    }
        //}

        //public async Task<TResult> PostBLOBAsync<TResult, T>(string uri, IEnumerable<FileInfo> data)
        //{
        //    var cancelTokenSource = new CancellationTokenSource();
        //    try
        //    {
        //        using var content = new MultipartFormDataContent();
        //        foreach (var attachment in data)
        //        {
        //            if (attachment.Exists)
        //            {
        //                //var fileContent = new StreamContent(File.OpenRead(file.FullName));
        //                using var fileContent =
        //                    new ByteArrayContent(System.IO.File.ReadAllBytes(attachment.FullName));
        //                //content.Add(fileContent, "file", file.Name);

        //                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

        //                // here it is important that second parameter matches with name given in API.
        //                content.Add(fileContent, "file", System.IO.Path.GetFileName(attachment.FullName));
        //            }
        //        }

        //        using var httpclient = new HttpClient();
        //        httpclient.BaseAddress = new Uri($"{WEB_API_URL}");
        //        var response = await httpclient.PostAsync(uri, content, cancelTokenSource.Token);

        //        //response.EnsureSuccessStatusCode();

        //        //var responseContent = await response.Content.ReadAsStringAsync();

        //        var stream = await response.Content.ReadAsStreamAsync();
        //        return response.IsSuccessStatusCode
        //            ? await SerializeDeserialize.DeserializeJsonFromStreamAsync<TResult>(stream)
        //            : default;
        //    }
        //    catch (OperationCanceledException e)
        //    {
        //        cancelTokenSource?.Cancel();
        //        ErrorHandler.HandleException(e);
        //        return default;
        //    }
        //    catch (Exception e)
        //    {
        //        cancelTokenSource?.Cancel();
        //        ErrorHandler.HandleException(e);
        //        return default;
        //    }
        //    finally
        //    {
        //        cancelTokenSource?.Dispose();
        //    }
        //}

        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                {
                    throw new ServiceAuthenticationException(content);
                }

                throw new HttpRequestExceptionEx(response.StatusCode, content);
            }
        }

        public static class ErrorHandler
        {
            public static void HandleException(OperationCanceledException ex)
            {
                LogException(ex);
                
                ShowErrorMessage(ex);
            }

            public static void HandleException(Exception ex)
            {
                LogException(ex);
                
                ShowErrorMessage(ex);
            }
            
            private static void LogException(OperationCanceledException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            private static void LogException(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            private static void ShowErrorMessage(OperationCanceledException ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            private static void ShowErrorMessage(Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
