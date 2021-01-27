using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Echo
{
    class Entity : INotifyPropertyChanged
    {
        public Entity()
        {
            PhoneNumbersList = new List<uint>();
        }
        public int SuccessPingCount
        { get; set; }

        public int PingCount
        { get; set; }

        public int PingFailed
        { get; set; }
        

        public long MinRoundTripTime
        { get; set; }

        public long MaxRoundTripTime
        { get; set; }

        public long LastRoundTripTime
        { get; set; }

        public long AverageRoundTripTime
        { get; set; }

        public int Serial
        { get; set; }

        public double PercentageLoss
        { get; set; }

        
        public String Action_Type { get; set; } = "Unknown";

        public Color Color_Type1
        {
            get
            {
                if (this.LastPingStatus == IPStatus.Success.ToString())
                {
                    return Colors.Green;
                }
                else
                {
                    return Colors.Red;
                }
            }
        }

        private Color color_type2 = Colors.Red;
        public Color Color_Type2
        {
            get { return color_type2; }
            set
            {
                color_type2 = value;
                // Call OnPropertyChanged whenever the property is updated
                //OnPropertyChanged("IpAddress");
            }
        }

        public Color Color_Type3
        {
            get
            {
                if (this.Action_Type == NodeType.SMSENABLED.ToString())
                {
                    return Colors.Teal;
                }
                else
                {
                    return Colors.Red;
                }
            }
        }


        public String IPAddress { get; set; }

        public String Name { get; set; }


        public String Area { get; set; }

        public DateTime? UpTime = null;

        public DateTime? DownTime = null;

        public String LastPingStatus { get; set; } = "Unknown";

        private String _Status = "Unknown";
        public String Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Status");
            }
        }

        public List<uint> PhoneNumbersList 
        { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string data)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(data));
            }
        }
    }

    enum NodeType
    {
        Unknown = 0,
        SMSENABLED = 1,
        PINGONLY = 2,
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      