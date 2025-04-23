namespace BlazorApp1.Services
{
    public class CartService
    {
        public List<Degree> Selected { get; } = new();
        public event Action? Changed;
        public void Add(Degree d) { Selected.Add(d); Changed?.Invoke(); }
        public void Remove(Degree d) { Selected.Remove(d); Changed?.Invoke(); }
    }
}
