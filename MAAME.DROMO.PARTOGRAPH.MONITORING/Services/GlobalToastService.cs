using System;
using System.Timers;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public interface IGlobalToastService
    {
        event Action<string, string, ToastType> OnShow;
        void ShowToast(string title, string message, ToastType type);
        void ShowSuccess(string title, string message);
        void ShowError(string title, string message);
        void ShowInfo(string title, string message);
        void ShowWarning(string title, string message);
    }

    public class GlobalToastService : IGlobalToastService
    {
        public event Action<string, string, ToastType>? OnShow;

        public void ShowToast(string title, string message, ToastType type)
        {
            OnShow?.Invoke(title, message, type);
        }

        public void ShowSuccess(string title, string message) => ShowToast(title, message, ToastType.Success);
        public void ShowError(string title, string message) => ShowToast(title, message, ToastType.Error);
        public void ShowInfo(string title, string message) => ShowToast(title, message, ToastType.Info);
        public void ShowWarning(string title, string message) => ShowToast(title, message, ToastType.Warning);
    }

    public enum ToastType
    {
        Success,
        Error,
        Info,
        Warning
    }
}
