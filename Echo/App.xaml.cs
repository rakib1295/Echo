using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace Echo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //GeoCoordinateWatcher watcher;
    //    private void Application_Startup_1(object sender, StartupEventArgs e)
    //    {
    //        string mbInfo = String.Empty;
    //        ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
    //        scope.Connect();
    //        ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

    //        foreach (PropertyData propData in wmiClass.Properties)
    //        {
    //            if (propData.Name == "SerialNumber")
    //            {
    //                mbInfo = Convert.ToString(propData.Value);
    //                break;
    //            }
    //        }


    //        String strHostName = string.Empty;
    //        // Getting Ip address of local machine...
    //        // First get the host name of local machine.
    //        strHostName = Dns.GetHostName();


    //        //Console.WriteLine("Local Machine's Host Name: " + strHostName);
    //        //// Then using host name, get the IP address list..
    //        //IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
    //        //IPAddress[] addr = ipEntry.AddressList;

    //        //for (int i = 0; i < addr.Length; i++)
    //        //{
    //        //    Console.WriteLine("IP Address {0}: {1} ", i, addr[i].ToString());
    //        //}

    //        //string localIP;
    //        //using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
    //        //{
    //        //    socket.Connect("8.8.8.8", 65530);
    //        //    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
    //        //    localIP = endPoint.Address.ToString();
    //        //}

    //        //this.watcher = new GeoCoordinateWatcher();
    //        //this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
    //        //bool started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));

    //        //String mbInfo = "";


    //        //ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
    //        //ManagementObjectCollection moc = mos.Get();

    //        //foreach (ManagementObject mo in moc)
    //        //{
    //        //    mbInfo = mo["SerialNumber"].ToString();
    //        //}

    //        if (mbInfo != "/621YYY1/CN7016338H09RP/" && mbInfo != "/6VT2T92/CN701635AM01NB/"
    //            && mbInfo != "/FNGK382/CN701635AK0146/" && mbInfo != "6CR4514QGP") //sbn pc = /621YYY1/CN7016338H09RP/ sbn-SMSC= "6CR4514QGP" wmic baseboard get serialnumber moghbazar= /FNGK382/CN701635AK0146/
    //        {
    //            String UrlString = "http://bulksms.teletalk.com.bd/link_sms_send.php?op=SMS&user=noc&pass=Noc$1234&mobile=01917300427&sms=Someone installed Echo without your permission. Application is shutting down. Serial: " + 
    //                mbInfo + ", Hostname: " + strHostName;//#############################################################
    //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@UrlString);

    //            WebResponse response = request.GetResponse();
    //            MessageBox.Show("You are not an authorized user, please grant permission from programmer.", "Echo", MessageBoxButton.OK, MessageBoxImage.Warning);

    //            Application.Current.Shutdown();
    //        }                
    //    }

    //    //void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
    //    //{
    //    //    PrintPosition(e.Position.Location.Latitude, e.Position.Location.Longitude);
    //    //}

    //    //void PrintPosition(double Latitude, double Longitude)
    //    //{
    //    //    Console.WriteLine("Latitude: {0}, Longitude {1}", Latitude, Longitude);
    //    //}
    }
}
