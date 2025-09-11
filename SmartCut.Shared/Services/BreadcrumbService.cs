using SmartCut.Shared.Models.ViewModels;

namespace SmartCut.Shared.Services
{
    public class BreadcrumbService
    {
        public event Action? OnBreadcrumbChanged;

        private List<BreadcrumbItem> _items = new();

        public IReadOnlyList<BreadcrumbItem> Items => _items;

        public void SetBreadcrumbs(IEnumerable<BreadcrumbItem> items)
        {
            _items = items.ToList();
            OnBreadcrumbChanged?.Invoke();
        }
    }
}
