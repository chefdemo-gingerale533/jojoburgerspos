using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace RestaurantPOS
{
    public partial class KDSInterface : Window
    {
        private TcpClient client;
        private List<Order> orders;

        public KDSInterface()
        {
            InitializeComponent();
            orders = new List<Order>();
        }

        private void KDSInterface_Load(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            RefreshOrders();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 5000); // Connect to the server on localhost:5000
                MessageBox.Show("Connected to server!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void RefreshOrders()
        {
            try
            {
                // Send request to server to get all orders
                var message = new ServerMessage
                {
                    Type = "get_orders",
                    Data = ""
                };

                SendMessageToServer(message);

                // Read response from server
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var serverMessage = JsonConvert.DeserializeObject<ServerMessage>(response);

                    if (serverMessage.Type == "orders_list")
                    {
                        orders = JsonConvert.DeserializeObject<List<Order>>(serverMessage.Data);
                        DisplayOrders();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing orders: {ex.Message}");
            }
        }

        private void DisplayOrders()
        {
            lstOrders.Items.Clear();
            foreach (var order in orders)
            {
                var orderPanel = new StackPanel();
                orderPanel.Children.Add(new TextBlock { Text = $"Order ID: {order.OrderId}", FontWeight = FontWeights.Bold });
                orderPanel.Children.Add(new TextBlock { Text = $"Items: {string.Join(", ", order.Items)}" });
                orderPanel.Children.Add(new TextBlock { Text = $"Status: {order.Status}", Foreground = GetStatusColor(order.Status) });

                lstOrders.Items.Add(new ListBoxItem { Content = orderPanel });
            }
        }

        private System.Windows.Media.Brush GetStatusColor(string status)
        {
            switch (status.ToLower())
            {
                case "pending": return System.Windows.Media.Brushes.Red;
                case "in_progress": return System.Windows.Media.Brushes.Orange;
                case "completed": return System.Windows.Media.Brushes.Green;
                default: return System.Windows.Media.Brushes.Black;
            }
        }

        private void MarkInProgress_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedOrderStatus("in_progress");
        }

        private void MarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedOrderStatus("completed");
        }

        private void UpdateSelectedOrderStatus(string newStatus)
        {
            if (lstOrders.SelectedItem is ListBoxItem selectedItem)
            {
                var selectedOrder = orders[lstOrders.SelectedIndex];
                selectedOrder.Status = newStatus;

                // Send update to server
                var message = new ServerMessage
                {
                    Type = "update_order",
                    Data = JsonConvert.SerializeObject(selectedOrder)
                };
                SendMessageToServer(message);

                // Refresh the local display
                DisplayOrders();
            }
            else
            {
                MessageBox.Show("Please select an order to update.");
            }
        }

        private void SendMessageToServer(ServerMessage message)
        {
            try
            {
                var stream = client.GetStream();
                var jsonMessage = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

                stream.Write(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error communicating with server: {ex.Message}");
            }
        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public List<string> Items { get; set; }
        public string Status { get; set; } // pending, in_progress, completed
    }

    public class ServerMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
