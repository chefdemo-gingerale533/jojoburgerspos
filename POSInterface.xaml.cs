using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantPOS
{
    public partial class POSInterface : Window
    {
        private List<MenuItem> menuItems;
        private List<OrderItem> currentOrder;

        public POSInterface()
        {
            InitializeComponent();
            InitializeMenuItems();
            currentOrder = new List<OrderItem>();
        }

        private void InitializeMenuItems()
        {
            // Example menu items
            menuItems = new List<MenuItem>
            {
                new MenuItem { Name = "Burger", Price = 5.99m, Category = "Meals" },
                new MenuItem { Name = "Pizza", Price = 7.49m, Category = "Meals" },
                new MenuItem { Name = "Coke", Price = 1.99m, Category = "Drinks" },
                new MenuItem { Name = "Fries", Price = 2.99m, Category = "Snacks" },
                new MenuItem { Name = "Ice Cream", Price = 3.49m, Category = "Desserts" }
            };
        }

        private void MenuCategory_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            string category = clickedButton.Content.ToString();
            LoadMenuItemsByCategory(category);
        }

        private void LoadMenuItemsByCategory(string category)
        {
            MenuItemsPanel.Children.Clear();
            foreach (var item in menuItems)
            {
                if (item.Category == category)
                {
                    Button menuItemButton = new Button
                    {
                        Content = $"{item.Name} - ${item.Price:F2}",
                        Width = 120,
                        Height = 120,
                        Margin = new Thickness(5),
                        Tag = item
                    };
                    menuItemButton.Click += MenuItemButton_Click;
                    MenuItemsPanel.Children.Add(menuItemButton);
                }
            }
        }

        private void MenuItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            MenuItem menuItem = clickedButton.Tag as MenuItem;

            // Add the selected menu item to the order
            currentOrder.Add(new OrderItem { Name = menuItem.Name, Price = menuItem.Price });
            RefreshOrderSummary();
        }

        private void RefreshOrderSummary()
        {
            OrderItemsListBox.Items.Clear();
            decimal subtotal = 0;

            foreach (var orderItem in currentOrder)
            {
                OrderItemsListBox.Items.Add($"{orderItem.Name} - ${orderItem.Price:F2}");
                subtotal += orderItem.Price;
            }

            decimal tax = subtotal * 0.1m; // 10% tax
            decimal total = subtotal + tax;

            SubtotalText.Text = $"Subtotal: ${subtotal:F2}";
            TaxText.Text = $"Tax: ${tax:F2}";
            TotalText.Text = $"Total: ${total:F2}";
        }

        private void ClearOrder_Click(object sender, RoutedEventArgs e)
        {
            currentOrder.Clear();
            RefreshOrderSummary();
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            // Handle order checkout logic here
            MessageBox.Show("Order has been sent to the server for processing.");
            ClearOrder_Click(sender, e);
        }
    }

    public class MenuItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
