using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services.Helper
{
    public interface IServiceRequestProvider
    {
        Task<T?> GetAsync<T>(string uri);
        Task<TResult?> GetAsync<TResult, T>(string uri, T data);
        Task<TResult?> GetExtAsync<TResult, T>(string uri, T data);
        Task<TResult?> PostAsync<TResult, T>(string uri, T data);
        Task<TResult?> PostAsync<TResult, T>(string uri, T data, TResult result);
        Task PostAsync<T>(string uri, T data);
        Task PostAsync(string uri);
        Task<TResult?> PostAsync<TResult>(string uri);
        Task<TResult?> PutAsync<TResult, T>(string uri, T data);
        Task<TResult?> PutAsync<TResult, T>(string uri, T data, TResult result);
        Task PutAsync<T>(string uri, T data);
        Task PutAsync(string uri);
        //Task<TResult> PostBLOBAsync<TResult, T>(string uri, FileInfo data);
        //Task<TResult> PostBLOBAsync<TResult, T>(string uri, IEnumerable<FileInfo> data);
    }
}
