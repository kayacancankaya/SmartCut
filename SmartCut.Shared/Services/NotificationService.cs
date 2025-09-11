

namespace SmartCut.Shared.Services
{
    public class NotificationService
    {
        public string? SuccessMessage { get; private set; }
        public string? ErrorMessage { get; private set; }

        public event Action? OnChange;
        public void ShowSuccess(string message)
        {
            SuccessMessage = message;
            ErrorMessage = null;
            OnChange?.Invoke();
        }

        public void ShowError(string message)
        {
            ErrorMessage = message;
            SuccessMessage = null;
            OnChange?.Invoke();
        }

        public void Clear()
        {
            SuccessMessage = null;
            ErrorMessage = null;
            OnChange?.Invoke();
        }
    }
}
