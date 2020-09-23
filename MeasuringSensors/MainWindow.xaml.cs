using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;

namespace MeasuringSensors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BpmGauge.DataContext = new GaugeViewModel();
            BpmGauge.Slider.Visibility = Visibility.Hidden;
            BPM.Visibility = Visibility.Hidden;
            Temp.Visibility = Visibility.Hidden;
        }

        private void BPM_TextChanged(object sender, TextChangedEventArgs e)
        {
            double value;
            if (BPM.Text == string.Empty)
            {
                value = 0;
            }
            else
            {
                value = Double.Parse(BPM.Text);
            }
            this.BpmGauge.Slider.Value = value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BluetoothClient client = new BluetoothClient();
            List<string> items = new List<string>();
            BluetoothDeviceInfo[] devices = client.DiscoverDevices();

            foreach (BluetoothDeviceInfo d in devices)
            {
                if (d.DeviceName == "itead \r\n")
                {
                    Connect(d, client);
                    break;
                }
            }
        }

        private void Connect(BluetoothDeviceInfo device, BluetoothClient client)
        {
            var serviceClass = BluetoothService.SerialPort;

            if (device == null)
            {
                return;
            }

            var ep = new BluetoothEndPoint(device.DeviceAddress, serviceClass);

            try
            {
                if (!device.Connected)
                {
                    client.Connect(ep);

                    System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += (sender, e) => dispatcherTimer_Tick(sender, e, client);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
                    dispatcherTimer.Start();

                    btn.IsEnabled = false;
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                MessageBox.Show("Connection failed!");
                client.Close();
                return;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e, BluetoothClient client)
        {
            ReadStream(client);
        }

        private void ReadStream(BluetoothClient client)
        {
            string[] data = new string[4];
            while (client.GetStream().DataAvailable)
            {
                if (client.GetStream().ReadByte().ToString() == "33")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[i] = client.GetStream().ReadByte().ToString();

                    }
                    if (data[3] != "47")
                    {
                        MessageBox.Show("Error while reading...");
                    }
                    else
                    {
                        Temp.Text = data[1] + "." + data[2];
                        BPM.Text = data[0];
                    }
                }
            }
        }
    }
}
