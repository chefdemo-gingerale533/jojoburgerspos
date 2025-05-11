using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace RestaurantPOS
{
    public partial class MainServer : Window
    {
        private TcpListener server;
        private List<TcpClient> connectedClients;
        private List<Order> orders;
        private bool isServerRunning;

        public MainServer()
        {
            InitializeComponent();
            connectedClients = new List<TcpClient>();
            orders = new List<Order>();
            isServerRunning = false;
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (isServerRunning)
            {
                AppendLog("Server is already running.");
                return;
            }

            try
            {
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();
                isServerRunning = true;
                AppendLog("Server started on port 5000...");

                Thread serverThread = new Thread(() =>
                {
                    while (isServerRunning)
                    {
                        try
                        {
                            var client = server.AcceptTcpClient();
                            connectedClients.Add(client);
                            UpdateConnectedClientsCount();
                            AppendLog("New client connected!");
                            Thread clientThread = new Thread(() => HandleClient(client));
                            clientThread.Start();
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"Server error: {ex.Message}");
                        }
                    }
                });

                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch (Exception ex)
            {
                AppendLog($"Error starting server: {ex.Message}");
            }
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            if (!isServerRunning)
            {
                AppendLog("Server is not running.");
                return;
            }

            try
            {
                isServerRunning = false;
                server.Stop();
                AppendLog("Server stopped.");
                connectedClients.Clear();
                UpdateConnectedClientsCount();
            }
            catch (Exception ex)
            {
                AppendLog($"Error stopping server: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        AppendLog($"Received: {message}");
                        ProcessMessage(message, client);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Client error: {ex.Message}");
            }
            finally
            {
                connectedClients.Remove(client);
                UpdateConnectedClientsCount();
                AppendLog("Client disconnected.");
            }
        }

        private void ProcessMessage(string message, TcpClient sender)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<ServerMessage>(message);

                switch (request.Type)
                {
                    case "new_order":
                        HandleNewOrder(request.Data);
                        break;
                    case "update_order":
                        HandleUpdateOrder(request.Data);
                        break;
                    default:
                        AppendLog("Unknown message type received.");
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Error processing message: {ex.Message}");
            }
        }

        private void HandleNewOrder(string orderData)
        {
            var order = JsonConvert.DeserializeObject<Order>(orderData);
            Dispatcher.Invoke(() =>
            {
                orders.Add(order);
                UpdateOrdersList();
                AppendLog($"New order added: Order ID {order.OrderId}");
            });
        }

        private void HandleUpdateOrder(string orderData)
        {
            var updatedOrder = JsonConvert.DeserializeObject<Order>(orderData);
            Dispatcher.Invoke(() =>
            {
                var existingOrder = orders.Find(o => o.OrderId == updatedOrder.OrderId);
                if (existingOrder != null)
                {
                    existingOrder.Status = updatedOrder.Status;
                    AppendLog($"Order {updatedOrder.OrderId} updated to {updatedOrder.Status}");
                    UpdateOrdersList();
                }
            });
        }

        private void MarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedIndex >= 0)
            {
                var order = orders[OrdersListBox.SelectedIndex];
                order.Status = "Completed";
                AppendLog($"Order {order.OrderId} marked as completed.");
                UpdateOrdersList();
            }
            else
            {
                MessageBox.Show("Please select an order to mark as completed.");
            }
        }

        private void RemoveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedIndex >= 0)
            {
                var order = orders[OrdersListBox.SelectedIndex];
                orders.Remove(order);
                AppendLog($"Order {order.OrderId} removed.");
                UpdateOrdersList();
            }
            else
            {
                MessageBox.Show("Please select an order to remove.");
            }
        }

        private void UpdateOrdersList()
        {
            OrdersListBox.Items.Clear();
            foreach (var order in orders)
            {
                OrdersListBox.Items.Add($"Order ID: {order.OrderId}, Status: {order.Status}, Items: {string.Join(", ", order.Items)}");
            }
        }

        private void AppendLog(string log)
        {
            Dispatcher.Invoke(() =>
            {
                ServerLogTextBox.AppendText($"{DateTime.Now}: {log}\n");
                ServerLogTextBox.ScrollToEnd();
            });
        }

        private void UpdateConnectedClientsCount()
        {
            Dispatcher.Invoke(() =>
            {
                ConnectedClientsText.Text = $"Connected Clients: {connectedClients.Count}";
            });
        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public List<string> Items { get; set; }
        public string Status { get; set; }
    }

    public class ServerMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
