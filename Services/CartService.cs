using System.Collections.Generic;
using System.Linq;
using static BlazorApp1.Components.Pages.Checkout;

namespace BlazorApp1.Services
{
    public class CartService
    {
        public List<CartItem> Items { get; private set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            // Check if item already exists
            var existingItem = Items.FirstOrDefault(i => i.Name == item.Name && i.Type == item.Type);
            if (existingItem == null)
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Name == item.Name && i.Type == item.Type);
            if (existingItem != null)
            {
                Items.Remove(existingItem);
            }
        }

        public void Clear()
        {
            Items.Clear();
        }

        public int TotalCredits => Items.Sum(i => i.Credits);
    }
}
