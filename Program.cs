using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace urb_netfr
{
    class Program
    {
        static string x_remoteAddr;
        static BluetoothRadio x_radio;
        static BluetoothAddress x_localAddr;
        static ArrayList x_deviceInfos = new ArrayList();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("please enter the remote device address --> xx:xx:xx:xx:xx:xx");
                return;
            }

            // get local address
            x_remoteAddr = args[0];
            x_radio = BluetoothRadio.PrimaryRadio;
            if (x_radio == null)
            {
                Console.WriteLine("system has no bluetooth device");
                return;
            }
            x_localAddr = x_radio.LocalAddress;

            // scanning
            BluetoothEndPoint endPoint = new BluetoothEndPoint(x_localAddr, BluetoothService.SerialPort);
            BluetoothClient client = new BluetoothClient(endPoint);
            BluetoothComponent component = new BluetoothComponent(client);
            component.DiscoverDevicesAsync(255, true, true, true, true, null);
            component.DiscoverDevicesProgress += new EventHandler<DiscoverDevicesEventArgs>(XProgress);
            component.DiscoverDevicesComplete += new EventHandler<DiscoverDevicesEventArgs>(XComplete);
            Thread.Sleep(6000); // 4 period (1.28 x 4 = 5.12 sec)

            BluetoothAddress addr = BluetoothAddress.Parse(x_remoteAddr);
            BluetoothDeviceInfo dev = null;
            BluetoothDeviceInfo[] paired = client.DiscoverDevices(255, false, true, false, false);

            foreach (BluetoothDeviceInfo devInfo in x_deviceInfos)
            {
                var devAddr = devInfo.DeviceAddress;
                if (devAddr == addr)
                {
                    dev = devInfo;
                    Console.WriteLine("target device found {0}", devAddr);
                    //break;
                }
            }
            if (dev == null)
            {
                Console.WriteLine("target device not found");
                return;
            }

            // pairing
            if (!dev.Authenticated)
            {
                if (!BluetoothSecurity.PairRequest(dev.DeviceAddress, "1234"))
                {
                    Console.WriteLine("pairing failed");
                    return;
                }
                Console.WriteLine("pairing success");
            }

        }

        static void XProgress(object sender, DiscoverDevicesEventArgs eventArgs)
        {
            for (int i = 0; i < eventArgs.Devices.Length; i++)
            {
                x_deviceInfos.Add(eventArgs.Devices[i]);
            }
        }

        static void XComplete(object sender, DiscoverDevicesEventArgs eventArgs)
        {
            return;
        }

    }
}
