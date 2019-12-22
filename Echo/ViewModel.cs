using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace Echo
{
    class ViewModel : INotifyPropertyChanged
    {
        private bool _checkAll = true;
        private bool _checkConnectedOnly = false;
        private bool _checkDisconnectedOnly = false;
        bool _UILoadingAnimation = false; //for logo

        public String User_Name_String = "";
        public String Password_String = "";
        public String Message_Header = "";
        public String Message_Footer = "";
        public String AllLinksUpMessage = "";
        public bool SMSActive = true;
        public bool SMSEvenAllUp = true;
        public String Title = "";

        private String _logviewer = "";
        public string Destination_Excel_url = "";///////////////////////////////////////////


        public ViewModel()
        {
            this.PropertyChanged += ViewModel_PropertyChanged;
            TimerforUIupdate();
            TimerforStatusResetAndSMS();
            TimerforNetChecking();

            _nodes = new ObservableCollection<Entity>();
            _nodeslist = new List<Entity>();
            _phonenumberlist = new List<int>();
            _downrouterslist = new List<Entity>();
            //_zonelist = new List<Zone>();
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public bool UILoadingAnimation //for logo
        {
            get { return _UILoadingAnimation; }
            set
            {
                _UILoadingAnimation = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("UILoadingAnimation");
            }
        }

        private String _nextSMSTime = "";
        public String NextSMSTime
        {
            get { return _nextSMSTime; }
            set
            {
                _nextSMSTime = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("NextSMSTime");
            }
        }

        private String _pingStatusText = "";
        public String PingStatusText
        {
            get { return _pingStatusText; }
            set
            {
                _pingStatusText = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("PingStatusText");
            }
        }

        private String _accountStatusText = "";
        public String AccountStatusText
        {
            get { return _accountStatusText; }
            set
            {
                _accountStatusText = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("AccountStatusText");
            }
        }

        private int _UpDownIndicator;
        public int UpDownIndicator
        {
            get { return _UpDownIndicator; }
            set
            {
                _UpDownIndicator = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("UpDownIndicator");
            }
        }

        private bool _ExcelLoaded = false;

        public bool ExcelLoaded
        {
            get { return _ExcelLoaded; }
            set
            {
                _ExcelLoaded = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ExcelLoaded");
            }
        }

        private bool _RunPingFunctionality = false;

        public bool RunPingFunctionality
        {
            get { return _RunPingFunctionality; }
            set
            {
                _RunPingFunctionality = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("StartPingFunctionality");
            }
        }

        private bool _StartSMSFunctionality = false;
        public bool StartSMSFunctionality
        {
            get { return _StartSMSFunctionality; }
            set
            {
                _StartSMSFunctionality = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("StartSMSFunctionality");
            }
        }


        public String LogViewer
        {
            get { return _logviewer; }
            set
            {
                _logviewer = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("LogViewer");
            }
        }

        public bool CheckAll
        {
            get { return _checkAll; }
            set
            {
                _checkAll = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("CheckAllorNot");
            }
        }

        protected void OnPropertyChanged(string data)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(data));
            }
        }



        private ObservableCollection<Entity> _nodes;
        private IList<Entity> _nodeslist;
        private IList<Entity> _downrouterslist;
        private IList<int> _phonenumberlist;
        //private IList<Zone> _zonelist;

        //public IList<Zone> ZoneList
        //{
        //    get { return _zonelist; }
        //    set
        //    {
        //        _zonelist = value;
        //        OnPropertyChanged("ZonelistChanged");
        //    }
        //}
        public IList<Entity> NodesList
        {
            get { return _nodeslist; }
            set
            {
                _nodeslist = value;
                OnPropertyChanged("NodeslistChanged");
            }
        }

        public IList<Entity> DownRoutersList
        {
            get { return _downrouterslist; }
            set
            {
                _downrouterslist = value;
                OnPropertyChanged("DownRoutersListChanged");
            }
        }

        public IList<int> PhoneNumberList
        {
            get { return _phonenumberlist; }
            set { _phonenumberlist = value; }
        }


        public ObservableCollection<Entity> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                OnPropertyChanged("NodesChanged");
            }
        }


        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CheckAllorNot")
            {
                Dispatcher dispatcher = Application.Current.Dispatcher;

                dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        Nodes.Clear();
                        if (this.CheckAll)
                        {
                            foreach (var item in NodesList)
                            {
                                item.Serial = Nodes.Count + 1;
                                Nodes.Add(item);
                            }
                        }
                        else if (this.CheckConnectedOnly)
                        {
                            foreach (var item in NodesList)
                            {
                                if (item.Status != "Down" && item.Status != "Unknown")
                                {
                                    item.Serial = Nodes.Count + 1;
                                    Nodes.Add(item);
                                }
                            }
                        }
                        else if (this.CheckDisconnectedOnly)
                        {
                            foreach (var item in NodesList)
                            {
                                if (item.Status == "Down" || item.Status == "Unknown")
                                {
                                    item.Serial = Nodes.Count + 1;
                                    Nodes.Add(item);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.LogViewer = "Error in inserting observable collection: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                        Write_logFile("Error in inserting observable collection: " + ex.Message + " <" + ex.GetType().ToString() + ">");
                    }
                }));
            }
            else if (e.PropertyName == "NodeslistChanged")
            {

            }
            else if (e.PropertyName == "DownRoutersListChanged")
            {

            }
            else if (e.PropertyName == "NodesChanged")
            {

            }
            else if (e.PropertyName == "StartPingFunctionality")
            {
                StarOrStopPingRequest();
            }
            else if (e.PropertyName == "UpDownIndicator")
            {
                Entity.UpDownIndicator = this.UpDownIndicator;
            }
        }



        private void StarOrStopPingRequest()
        {
            if (RunPingFunctionality)
            {
                RequestPing();
                UIupdateTimer.Start();
                StatusResetAndSMSTimer.Start();
            }
            else
            {
                PingStatusText = "Ping paused.";

                UILoadingAnimation = false; //for logo
                UIupdateTimer.Stop();
                StatusResetAndSMSTimer.Stop();
            }

        }


        public int SMSInterval = 120; //min
        public int PingSensePeriodForSMS = 5; //min
        public int RefreshPeriod = 10; //min

        private static System.Timers.Timer StatusResetAndSMSTimer = new System.Timers.Timer();

        void TimerforStatusResetAndSMS()
        {
            StatusResetAndSMSTimer.Interval = 60000;//1 minute/////////////////////////////////////////////consider always////////////////////////////////////////////////////
            StatusResetAndSMSTimer.AutoReset = true;
            StatusResetAndSMSTimer.Elapsed += StatusResetAndSMSTimer_Elapsed;
        }


        int timeCounter = 0;

        private void StatusResetAndSMSTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timeCounter++;
            if (timeCounter == SMSInterval - PingSensePeriodForSMS) //reset before sms
            {
                ResetStatus();
                LogViewer = "Status of all nodes has been reset.";
                Write_logFile(LogViewer);
            }

            if (timeCounter == SMSInterval)
            {
                timeCounter = 0;
                NextSMSTime = DateTime.Now.AddMinutes(SMSInterval).ToLongTimeString();

                if (SMSActive)
                {
                    SMSThreadMethod();
                }
            }
            else if (timeCounter % RefreshPeriod == 0) //reset each reset interval
            {
                if (SMSInterval - timeCounter > PingSensePeriodForSMS)
                {
                    ResetStatus();
                }
            }
        }


        bool SleepBeforePing = false;
        public void ResetStatus()
        {
            this.RunPingFunctionality = false;
            SentMsgCount = 0;
            NumberofDestination = 0;
            NumberofFailtoSendSMS = 0;
            foreach (var item in NodesList)
            {
                item.PingCount = 0;
                item.PingFailed = 0;
                item.SuccessPingCount = 0;
                item.PercentageLoss = 0;
                item.AverageRoundTripTime = 0;
                item.MaxRoundTripTime = 0;
                item.MinRoundTripTime = 999999;
            }

            SleepBeforePing = true;
            this.RunPingFunctionality = true;
        }

        public async void SMSThreadMethod()
        {
            Task tsk = SMSThreadMethodAsync();
            await Task.WhenAll(tsk);
            tsk.Dispose();
        }


        private Task SMSThreadMethodAsync()
        {
            return Task.Run(() => CheckNetBeforeSMS());
        }

        private void CheckNetBeforeSMS()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                SMSFunction();
            }
            else
            {
                LogViewer = "Error: Network cable may be unplugged. SMS cannot be triggered. Please fix it as soon as possible.";
                Write_logFile(LogViewer);
                timeCounter = SMSInterval / 2;
                NextSMSTime = DateTime.Now.AddMinutes(SMSInterval - timeCounter).ToLongTimeString();
                Write_logFile("Next SMS time for network Error: " + NextSMSTime);
            }
        }

        private void SMSFunction()
        {
            int downnodescount = NodesList.Where(s => (s.Status == "Down" || s.Status == "Unknown")).ToList<Entity>().Count;
            if (downnodescount != NodesList.Count)
            {
                String SMSContentString = "";
                SMSContentString = BuildSMSContent();

                SMSTrigger(SMSContentString);

            }
            else
            {
                LogViewer = "Error: All links are down, may be something is wrong. Please check internet connection of this Computer.";
                Write_logFile(LogViewer);
                timeCounter = SMSInterval / 2;
                NextSMSTime = DateTime.Now.AddMinutes(SMSInterval - timeCounter).ToLongTimeString();
                Write_logFile("Next SMS time for Error: " + NextSMSTime);
            }
        }


        void SMSTrigger(String SMSContentString)
        {
            //StartPingFunctionality = false;
            //Thread.Sleep(1000);


            NumberofDestination = PhoneNumberList.Count;
            NumberofFailtoSendSMS = 0;
            int contentlen = SMSContentString.Length;

            if (SMSContentString != "")
            {
                if (SMSContentString[contentlen - 1] == ' ')
                {
                    SMSContentString = SMSContentString.Substring(0, contentlen - 2);
                }

                if(DownRoutersList.Count > 1)
                {
                    SMSContentString = SMSContentString + " links are down.";
                }
                else if(DownRoutersList.Count == 1)
                {
                    SMSContentString = SMSContentString + " link is down.";
                }
            }
            else
            {
                if (SMSEvenAllUp)
                {
                    SMSContentString = AllLinksUpMessage;
                }
                else
                {
                    LogViewer = "All links are up now, so message will not be sent.";
                }
            }

            if (SMSContentString != "")
            {
                LogViewer = SMSContentString;
                Write_logFile(SMSContentString);

                //contentlen = SMSContentString.Length;
                //int headerlen = Message_Header.Length;
                //int footerlen = Message_Footer.Length;

                SMSContentString = Message_Header + "\n" + SMSContentString + " \n" + Message_Footer;

                //if (contentlen <= 160)
                //{
                //    if (contentlen + footerlen <= 159 && Message_Footer != "")
                //    {
                //        if (headerlen + contentlen + footerlen <= 158 && Message_Header != "")
                //        {
                //            SMSContentString = Message_Header + "\n" + SMSContentString + " \n" + Message_Footer;
                //        }
                //        else
                //        {
                //            SMSContentString = SMSContentString + "\n" + Message_Footer;
                //        }
                //    }
                //}
                //else
                //{
                //    SMSContentString = SMSContentString.Substring(0, 160);
                //    contentlen = SMSContentString.Length;
                //    HttpCallforSMS(1917300427, SMSContentString);
                //    SentMsgCount = 0;
                //}

                LogViewer = "Sending SMS, please wait ... .. .";
                Write_logFile(LogViewer);
                SentMsgCount = 0;
                foreach (var phnNum in PhoneNumberList)
                {
                    if (NumberofFailtoSendSMS == 0)
                    {
                        HttpCalltoTeletalk(phnNum, SMSContentString);
                    }
                }
            }
        }



        int NumberofDestination = 0;
        
        string CreditStatus_Text = "";
        int SentMsgCount = 0, NumberofFailtoSendSMS = 0;
        public int CreditStatus_Today = 0, CreditDeducted_Yesterday = 0, CreditStatus_Yesterday = 0;
        String _creditStatus = "";

        public String CreditStatus
        {
            get { return _creditStatus; }
            set
            {
                _creditStatus = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("CreditStatus");
            }
        }

        public async void AccountTestTask()
        {
            await Task.Run(() => HttpCalltoTeletalk());
        }

        private void HttpCalltoTeletalk()
        {
            string responseFromHttpWeb = "";

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    String UrlString = "http://bulksms.teletalk.com.bd/link_sms_send.php?op=SMS&user=" + User_Name_String + "&pass=" + Password_String;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@UrlString);
                    request.AllowWriteStreamBuffering = false;


                    WebResponse response = request.GetResponse();
                    // Display the status.
                    //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.

                    responseFromHttpWeb = reader.ReadToEnd();
                    // Display the content.
                    //Console.WriteLine(responseFromServer);

                    if (((HttpWebResponse)response).StatusDescription == "OK")
                    {
                        responseFromHttpWeb = responseFromHttpWeb.ToUpper();
                        if (responseFromHttpWeb.Contains("INVALID USER") || responseFromHttpWeb.Contains("WRONG USER"))
                        {
                            AccountStatusText = "Sorry! Account is invalid :(";
                        }
                        else if(responseFromHttpWeb.Contains("EMPTY SMS"))
                        {
                            AccountStatusText = "Congrats! Account is valid :)";
                        }
                        else
                        {
                            AccountStatusText = "Not sure!! :(";
                            LogViewer = responseFromHttpWeb;
                            Write_logFile("Not sure!! " + LogViewer);
                        }
                    }
                    else
                    {
                        AccountStatusText = "Server not OK!";
                        LogViewer = responseFromHttpWeb;
                        Write_logFile("Server not OK! " + LogViewer);
                    }

                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();

                    reader.Dispose();
                    dataStream.Dispose();
                    response.Dispose();
                }
                catch (Exception ex)
                {
                    AccountStatusText = "Network error! :(";
                }
            }
            else
            {
                AccountStatusText = "Network unplugged! :(";
            }
        }

        private void HttpCalltoTeletalk(int PhnNum, String SMSContentString)
        {
            string responseFromHttpWeb = "";

            try
            {
                if (SMSContentString.Contains('&'))
                {
                    SMSContentString = SMSContentString.Replace("&", "%26");
                }
                String UrlString = "http://bulksms.teletalk.com.bd/link_sms_send.php?op=SMS&user=" + User_Name_String + "&pass=" + Password_String + "&mobile=0" + PhnNum.ToString() + "&sms=" + SMSContentString;//#############################################################
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@UrlString);
                request.AllowWriteStreamBuffering = false;

                
                WebResponse response = request.GetResponse();
                // Display the status.
                //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                
                responseFromHttpWeb = reader.ReadToEnd();
                // Display the content.
                //Console.WriteLine(responseFromServer);


                CreditStatus_Text = "";
                CreditStatus_Today = 0;

                if (((HttpWebResponse)response).StatusDescription == "OK")
                {
                    SentMsgCount++;
                    NumberofFailtoSendSMS = 0;
                    if (SentMsgCount == NumberofDestination)
                    {
                        string[] data = new string[7];
                        data = responseFromHttpWeb.Split(',');
                        foreach (var x in data)
                        {
                            if (x.Contains("CURRENT CREDIT"))
                            {
                                CreditStatus_Text = x;
                                break;
                            }
                            if (x.Contains("FAILED"))
                            {
                                throw new Exception("ERROR: FROM TELETALK SERVER");
                            }
                        }
                        

                        string str = "Sent SMS today, total number of SMS: " + NumberofDestination.ToString() + ", " + DateTime.Now.ToLongDateString();
                        LogViewer = str;

                        string[] data2 = new string[2];
                        data2 = CreditStatus_Text.Split('=');


                        double d = 0;
                        try
                        {
                            d = Convert.ToDouble(data2[1]);
                        }
                        catch (Exception ex)
                        {
                            Write_logFile("Error in HTTP response conversion: " + ex.Message + " <" + ex.GetType().ToString() + ">");
                        }


                        if (d > 0)
                        {
                            CreditStatus_Today = Convert.ToInt32(d);

                            if (CreditStatus_Yesterday == 0)
                            {
                                CreditStatus_Yesterday = CreditStatus_Today + NumberofDestination;
                            }

                            string replystring = "CURRENT CREDIT = ";
                            replystring += CreditStatus_Today.ToString();

                            if (CreditDeducted_Yesterday != 0)
                            {
                                replystring += "\tCREDIT deducted yesterday = ";
                                replystring += CreditDeducted_Yesterday.ToString();
                            }

                            CreditStatus = replystring;
                        }
                        else
                        {
                            CreditStatus = responseFromHttpWeb;
                        }

                        Write_logFile("Sent SMS today, total number of SMS: " + NumberofDestination.ToString() + "."); ///////////////////////////////////////////////////////////////

                        SentMsgCount = 0;
                        timeCounter = 0;
                        NextSMSTime = DateTime.Now.AddMinutes(SMSInterval).ToLongTimeString();
                        Write_logFile("Next SMS time: " + NextSMSTime);

                        //SleepBeforePing = true;
                        //StartPingFunctionality = true;
                    }
                }
                else
                {
                    timeCounter = SMSInterval / 2;
                    NextSMSTime = DateTime.Now.AddMinutes(SMSInterval - timeCounter).ToLongTimeString();
                    Write_logFile("Next SMS time for Error due to web response not OK: " + NextSMSTime);
                }

                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();

                reader.Dispose();
                dataStream.Dispose();
                response.Dispose();
            }
            catch (Exception ex)
            {
                if (ex.Message == "ERROR: FROM TELETALK SERVER")
                {
                    //MessageBox.Show("Failed to send SMS, Invalid Phone Number.");
                    LogViewer = "ERROR: FROM TELETALK SERVER, " + responseFromHttpWeb;
                    Write_logFile("ERROR: FROM TELETALK SERVER, " + responseFromHttpWeb);

                    timeCounter = SMSInterval / 2;
                    NextSMSTime = DateTime.Now.AddMinutes(SMSInterval - timeCounter).ToLongTimeString();
                    Write_logFile("Next SMS time for Error at Teletalk server: " + NextSMSTime);

                    //StartPingFunctionality = true;
                }
                else
                {
                    NumberofFailtoSendSMS++;
                    String s = "";
                    if (ex.InnerException == null)
                    {
                        s = "Error in SMS sending to: " + PhnNum.ToString() + ", " + ex.Message + " <" + ex.GetType().ToString() + ">" + ", Number of Attemt: " + NumberofFailtoSendSMS;
                    }
                    else
                    {
                        s = "Error in SMS sending to: " + PhnNum.ToString() + ", " + ex.Message + " <" + ex.GetType().ToString() + ": " + ex.InnerException.ToString() + ">" + ", Number of Attemt: " + NumberofFailtoSendSMS;
                    }

                    Write_logFile(s);/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    LogViewer = "Error in SMS sending to: " + PhnNum.ToString() + ", " + ex.Message + " <" + ex.GetType().ToString() + ">" + ", Number of Attemt: " + NumberofFailtoSendSMS;
                    //LogViewer = "Please make sure your PC is connected to internet.";


                    if (NumberofFailtoSendSMS < 7)
                    {
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        timeCounter = SMSInterval / 2;
                        NextSMSTime = DateTime.Now.AddMinutes(SMSInterval - timeCounter).ToLongTimeString();
                        Write_logFile("Next SMS time for Error (catch 7 times): " + NextSMSTime);

                        //StartPingFunctionality = true;
                    }
                }
            }
            finally
            {
                if (NumberofFailtoSendSMS > 0 && NumberofFailtoSendSMS < 7)
                {
                    HttpCalltoTeletalk(PhnNum, SMSContentString);
                }
                else if (NumberofFailtoSendSMS >= 7)
                {
                    LogViewer = "Retried seven times but failed. SMS sending cancel due to internet disconnection.";
                    Write_logFile("Retried seven times but failed. SMS sending cancel due to internet disconnection.");
                }
            }
        }


        private string BuildSMSContent()
        {
            string SMSContentString = "";
            DownRoutersList.Clear();
            DownRoutersList = NodesList.Where(s => (s.Status == "Down" && s.Action_Type == NodeType.SMSENABLED.ToString())).ToList<Entity>();

            if(DownRoutersList.Count > 0)
            {
                foreach (var item in DownRoutersList)
                {
                    SMSContentString = item.Area + ", " + SMSContentString;
                }
            }

            return SMSContentString;
        }

        //private String BuildSMSContent()
        //{
        //    DownRoutersList.Clear();
        //    DownRoutersList = NodesList.Where(s => (s.AvgPingStatus == "Down" && s.Node_Type == NodeType.Router.ToString())).ToList<Entity>();


        //    IEnumerable<String> duplicatesZone = from item in DownRoutersList
        //                                         select item.Zone;


        //    IEnumerable<String> nonDuplicatedZone = duplicatesZone.Distinct();

        //    String SMSContentString = "";

        //    foreach (var item in nonDuplicatedZone)
        //    {
        //        int count = (from _itm in duplicatesZone
        //                     where _itm == item
        //                     select _itm).Count();

        //        int countinZonelist = 0;

        //        foreach (var _itm in ZoneList)
        //        {
        //            if (_itm.ZoneName == item)
        //            {
        //                countinZonelist = _itm.ZoneCount;
        //                break;
        //            }
        //        }

        //        if (count == countinZonelist) //////from zone
        //        {
        //            if (!SMSContentString.Contains(item))
        //            {
        //                Entity _ent = null;
        //                try
        //                {
        //                    IEnumerable<Entity> zoneObj = from obj in NodesList
        //                                                  where obj.Node_Type == "Router" && obj.Area == item
        //                                                  select obj;
        //                    _ent = zoneObj.ElementAt(0);
        //                }
        //                catch (Exception e)
        //                {
        //                    LogViewer = "Error: " + item + "- no router found with this name. " + e.Message + " <" + e.GetType().ToString() + ">";
        //                    Write_logFile("Error: " + item + "- no router found with this name. " + e.Message + " <" + e.GetType().ToString() + ">");
        //                    _ent = null;
        //                }


        //                if (_ent != null && _ent.AvgPingStatus != "Down")
        //                {
        //                    foreach (var _itm in DownRoutersList)
        //                    {
        //                        if (item == _itm.Zone)
        //                        {
        //                            if (!SMSContentString.Contains(_itm.Area))
        //                            {
        //                                if (nonDuplicatedZone.Contains(_itm.Area))
        //                                {
        //                                    SMSContentString = _itm.Area + " " + Zone_Footer + ", " + SMSContentString;
        //                                }
        //                                else
        //                                {
        //                                    SMSContentString = _itm.Area + ", " + SMSContentString;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    string superzone = "";
        //                    foreach (var _newitem in DownRoutersList)
        //                    {
        //                        if (_newitem.Area == item)
        //                        {
        //                            superzone = _newitem.Zone;
        //                            break;
        //                        }
        //                    }
        //                    if (!SMSContentString.Contains(superzone) || superzone == "")  //checks if superzone is already added or not
        //                    {
        //                        if (superzone == "")
        //                        {
        //                            SMSContentString = item + " " + Zone_Footer + ", " + SMSContentString;//need to add 'zone'
        //                        }
        //                        else
        //                        {
        //                            int _newcount = (from _itm in duplicatesZone
        //                                             where _itm == superzone
        //                                             select _itm).Count();


        //                            int _countinZonelist = 0;

        //                            foreach (var _itm in ZoneList) //checks if superzone will be added or not
        //                            {
        //                                if (_itm.ZoneName == superzone)
        //                                {
        //                                    _countinZonelist = _itm.ZoneCount;
        //                                    break;
        //                                }
        //                            }
        //                            if (_newcount == _countinZonelist)
        //                            {
        //                                if (_newcount == 1)
        //                                {
        //                                    SMSContentString = superzone + ", " + SMSContentString;
        //                                }
        //                                else
        //                                    SMSContentString = superzone + " " + Zone_Footer + ", " + SMSContentString;//need to add 'zone'
        //                            }
        //                            else
        //                            {
        //                                SMSContentString = item + " " + Zone_Footer + ", " + SMSContentString;//need to add 'zone'
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else   //////from area
        //        {
        //            foreach (var _itm in DownRoutersList)
        //            {
        //                if (item == _itm.Zone)
        //                {
        //                    if (!SMSContentString.Contains(_itm.Area))
        //                    {
        //                        if (nonDuplicatedZone.Contains(_itm.Area))
        //                        {
        //                            SMSContentString = _itm.Area + " " + Zone_Footer + ", " + SMSContentString;
        //                        }
        //                        else
        //                        {
        //                            SMSContentString = _itm.Area + ", " + SMSContentString;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return SMSContentString;
        //}



        private static System.Timers.Timer UIupdateTimer = new System.Timers.Timer();

        void TimerforUIupdate()
        {
            UIupdateTimer.Interval = 1000;  // 1 sec
            UIupdateTimer.AutoReset = true;
            UIupdateTimer.Elapsed += UIupdateTimer_Tick;
        }

        private void UIupdateTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateUINodes();
        }

        public async void RequestPing()
        {
            Task tsk = PingThreadAsync();
            await Task.WhenAll(tsk);
            tsk.Dispose();
        }

        public Task PingThreadAsync()
        {
            return Task.Run(() => PingThread());
        }


        private void PingThread()
        {
            if (SleepBeforePing)
            {
                SleepBeforePing = false;
                Thread.Sleep(5000);
            }

            PingStatusText = "Ping running.";

            UILoadingAnimation = true; //for logo

            if (this.NodesList.Count > 0)
            {
                var _nodelist = this.NodesList;
                TryToPingNodes();
            }
            else
            {
                LogViewer = "No IP address found. Ping stopped.";
                Write_logFile("No IP address found. Ping stopped.");
                UILoadingAnimation = false; //for logo
            }
        }


        public bool CheckConnectedOnly
        {
            get { return _checkConnectedOnly; }
            set
            {
                _checkConnectedOnly = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("CheckAllorNot");
            }
        }

        public bool CheckDisconnectedOnly
        {
            get { return _checkDisconnectedOnly; }
            set
            {
                _checkDisconnectedOnly = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("CheckAllorNot");
            }
        }



        public async void LoadingThread()
        {
            Task tsk = LoadExcelDataAsync();
            await Task.WhenAll(tsk);
            tsk.Dispose();
        }

        public Task LoadExcelDataAsync()
        {
            return Task.Run(() => LoadExcelData());
        }

        void LoadExcelData()
        {
            UILoadingAnimation = true; //for logo
            var _nodelist = this.NodesList;
            var _phonelist = this.PhoneNumberList;
            ImportExcelFile();

            if (this.NodesList.Count > 0 && ExcelLoaded)
            {
                LogViewer = "Excel file imported. Total number of nodes: " + NodesList.Count.ToString();
                Write_logFile(LogViewer);

                int cnt = (from _itm in NodesList
                           where _itm.Action_Type == NodeType.SMSENABLED.ToString()
                           select _itm).Count();
                LogViewer = "Number of links which should be notified through SMS: " + cnt.ToString();
                Write_logFile(LogViewer);

                cnt = (from _itm in NodesList
                       where _itm.Action_Type == NodeType.PINGONLY.ToString()
                       select _itm).Count();
                LogViewer = "Number of links which will ping only: " + cnt.ToString();
                Write_logFile(LogViewer);

                LogViewer = "Number of Phone numbers: " + PhoneNumberList.Count.ToString();
                Write_logFile(LogViewer);

                if (timeCounter == 0)
                {
                    NextSMSTime = DateTime.Now.AddMinutes(SMSInterval).ToLongTimeString();
                    Write_logFile("Next SMS time: " + NextSMSTime);
                }

                UILoadingAnimation = false; //for logo

                Thread.Sleep(5000);
                RunPingFunctionality = true;
            }
            else
            {
                UILoadingAnimation = false; //for logo
            }
        }

        void UpdateUINodes()
        {
            if (this.CheckAll)
            {
                this.CheckAll = true;
            }
            else if (this.CheckConnectedOnly)
            {
                this.CheckConnectedOnly = true;
            }
            else if (this.CheckDisconnectedOnly)
            {
                this.CheckDisconnectedOnly = true;
            }
        }

        private void ImportExcelFile()
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet1;
            xlWorkBook = xlApp.Workbooks.Open(Destination_Excel_url, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet1 = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            Excel.Worksheet xlWorkSheet2;
            xlWorkSheet2 = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);

            try
            {
                Excel.Range last = xlWorkSheet1.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);


                int lastUsedRow = last.Row;
                int lastUsedColumn = last.Column;

                //
                NodesList.Clear();

                for (int i = 2; i <= lastUsedRow; i++)
                {
                    Entity _nd = new Entity();
                    _nd.PingCount = 0;
                    _nd.PingFailed = 0;
                    _nd.SuccessPingCount = 0;
                    _nd.PercentageLoss = 0;
                    _nd.AverageRoundTripTime = 0;
                    _nd.MaxRoundTripTime = 0;
                    _nd.MinRoundTripTime = 999999;


                    string str = xlWorkSheet1.Cells[i, 1].Value2.ToString();
                    _nd.IpAddress = str;

                    _nd.Name = xlWorkSheet1.Cells[i, 2].Value2.ToString();

                    str = xlWorkSheet1.Cells[i, 3].Value2.ToString();
                    if (str.ToUpper().Contains("SMSENABLED"))
                    {
                        _nd.Action_Type = NodeType.SMSENABLED.ToString();
                    }
                    else if (!str.ToUpper().Contains("SMSENABLED"))
                    {
                        _nd.Action_Type = NodeType.PINGONLY.ToString();
                    }

                    _nd.Area = xlWorkSheet1.Cells[i, 4].Value2.ToString();

                    //_nd.Zone = xlWorkSheet1.Cells[i, 5].Value2.ToString();
                    NodesList.Add(_nd);
                }


                //IEnumerable<String> duplicates = from item in NodesList
                //                                 select item.Zone;



                //IEnumerable<String> noduplicates = duplicates.Distinct();

                //ZoneList.Clear();
                //foreach (var item in noduplicates)
                //{
                //    Zone zn = new Zone();
                //    zn.ZoneName = item;

                //    zn.ZoneCount = (from _itm in duplicates
                //                    where _itm == item
                //                    select _itm).Count();
                //    ZoneList.Add(zn);
                //}


                last = xlWorkSheet2.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);

                lastUsedRow = last.Row;
                lastUsedColumn = last.Column;

                PhoneNumberList.Clear();
                for (int i = 2; i <= lastUsedRow; i++)
                {
                    string str = xlWorkSheet2.Cells[i, 1].Value2.ToString();
                    PhoneNumberList.Add(Convert.ToInt32(str));
                }
                ExcelLoaded = true;
            }
            catch (Exception ex)
            {
                this.LogViewer = "Error in importing excel: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                Write_logFile("Error in importing excel: " + ex.Message + " <" + ex.GetType().ToString() + ">");
                MessageBox.Show("There may be wrong data in excel file. Excel may be partially loaded.\nTo load fully, please correct the excel and load again.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                object misValue = System.Reflection.Missing.Value;
                xlWorkBook.Close(false, misValue, misValue);
                xlApp.Quit();
                releaseObject(xlWorkSheet1);
                releaseObject(xlWorkSheet2);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                this.LogViewer = "Error in releasing object: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                Write_logFile("Error in releasing object: " + ex.Message + " <" + ex.GetType().ToString() + ">");
            }
            finally
            {
                GC.Collect();
            }
        }



        private void TryToPingNodes()
        {
            while (RunPingFunctionality)
            {
                Parallel.For(0, _nodeslist.Count, async (index, loopstate) =>
                {
                    Ping pingSender = new Ping();
                    try
                    {
                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            PingReply reply = await pingSender.SendPingAsync(NodesList[index].IpAddress);

                            if (RunPingFunctionality)
                            {
                                NodesList[index].PingCount++;//////////resetable

                                if (reply.Status == IPStatus.Success)
                                {
                                    NodesList[index].LastPingStatus = IPStatus.Success.ToString();
                                    NodesList[index].PercentageLoss = Math.Round(NodesList[index].PercentageLoss * (NodesList[index].PingCount - 1) / NodesList[index].PingCount, 3);
                                    if (NodesList[index].MinRoundTripTime > reply.RoundtripTime)
                                    {
                                        NodesList[index].MinRoundTripTime = reply.RoundtripTime;
                                    }
                                    NodesList[index].SuccessPingCount++;
                                    NodesList[index].AverageRoundTripTime = (NodesList[index].AverageRoundTripTime * (NodesList[index].SuccessPingCount - 1) + reply.RoundtripTime) / NodesList[index].SuccessPingCount;
                                }
                                else
                                {
                                    NodesList[index].LastPingStatus = reply.Status.ToString();
                                    NodesList[index].PercentageLoss = Math.Round((NodesList[index].PercentageLoss * (NodesList[index].PingCount - 1) + 100) / NodesList[index].PingCount, 3);
                                    NodesList[index].PingFailed++;
                                }
                                NodesList[index].LastRoundTripTime = reply.RoundtripTime;


                                if (NodesList[index].MaxRoundTripTime < reply.RoundtripTime)
                                {
                                    NodesList[index].MaxRoundTripTime = reply.RoundtripTime;
                                }

                                if (NodesList[index].PercentageLoss >= 0 && NodesList[index].PercentageLoss <= 20)
                                {
                                    NodesList[index].Color_Type2 = Colors.Green;
                                    NodesList[index].Status = "Up";
                                }
                                else if (NodesList[index].PercentageLoss > 20 && NodesList[index].PercentageLoss <= 50)
                                {
                                    NodesList[index].Color_Type2 = Colors.Blue;
                                    NodesList[index].Status = "Moderate";
                                }
                                else if (NodesList[index].PercentageLoss > 50 && NodesList[index].PercentageLoss < Entity.UpDownIndicator)
                                {
                                    NodesList[index].Color_Type2 = Colors.DarkOrange;
                                    NodesList[index].Status = "Poor";
                                }
                                else if (NodesList[index].PercentageLoss >= Entity.UpDownIndicator)
                                {
                                    NodesList[index].Color_Type2 = Colors.Red;
                                    NodesList[index].Status = "Down";
                                }
                            }
                            else
                            {
                                loopstate.Stop();
                                return;
                            }
                        }
                        else
                        {
                            if (this.RunPingFunctionality)
                            {
                                this.RunPingFunctionality = false;
                                LogViewer = "Error in Network adapter: " + "Network cable may be unplugged. Please fix it as soon as possible.";
                            }


                            Write_logFile("Error in Network adapter: node number: " + index.ToString() + ". " + "Network cable may be unplugged. No connection is found.");

                            lock (thisLock)
                            {
                                if (!NetCheckingTimer.Enabled)
                                {
                                    NetCheckingTimer.Interval = 60000;
                                    LogViewer = "Ping Paused for 60 seconds.";
                                    Write_logFile(LogViewer);
                                    NetCheckingTimer.Start();
                                }
                            }
                        }

                        index++;
                    }
                    catch (Exception ex)
                    {
                        if (this.RunPingFunctionality)
                        {
                            this.RunPingFunctionality = false;
                            LogViewer = "Ping Error: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                        }


                        if (ex.InnerException == null)
                        {
                            Write_logFile("Ping Error: Device count: " + NodesList.Count.ToString() + ", node number: " + index.ToString() + ". " + ex.Message + " <" + ex.GetType().ToString() + ">");
                        }
                        else
                        {
                            Write_logFile("Ping Error: Device count: " + NodesList.Count.ToString() + ", node number: " + index.ToString() + ". " + ex.Message +
                                " <" + ex.GetType().ToString() + ": " + ex.InnerException.ToString() + ">");
                        }
                        // }
                        NodesList[index].Color_Type2 = Colors.Red;
                        NodesList[index].Status = "Unknown";


                        lock (thisLock)
                        {
                            if (!NetCheckingTimer.Enabled)
                            {
                                NetCheckingTimer.Interval = 180000;
                                LogViewer = "Ping Paused for 180 seconds.";
                                Write_logFile(LogViewer);
                                NetCheckingTimer.Start();
                            }
                        }
                    }
                    finally
                    {
                        pingSender.Dispose();
                    }
                });
            }
        }

        private Object thisLock = new Object();


        private static System.Timers.Timer NetCheckingTimer = new System.Timers.Timer();

        private void TimerforNetChecking()
        {
            //NetCheckingTimer.Interval = TimeSpan.FromSeconds(60);///////////////////////////////////////////////consider always////////////////////////////////////////////////////
            NetCheckingTimer.AutoReset = true;
            NetCheckingTimer.Elapsed += NetCheckingTimer_Tick;
        }

        private void NetCheckingTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.RunPingFunctionality = true;
                NetCheckingTimer.Stop();

                LogViewer = "Network connection is OK now.";
                Write_logFile(LogViewer);
                NextSMSTime = DateTime.Now.AddMinutes(SMSInterval - timeCounter).ToLongTimeString();
                Write_logFile("Next SMS time: " + NextSMSTime);
            }
            else
            {
                LogViewer = "Error: Network connection is still unavailable *** *** ***";
                Write_logFile(LogViewer);
            }
        }

        private Object logFileLock = new Object();

        public void Write_logFile(String str)///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            string _dir = "C:\\Users\\Public\\" + Title + " Log\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MMMM");
            try
            {
                lock (logFileLock)
                {
                    if (!Directory.Exists(_dir))
                    {
                        Directory.CreateDirectory(_dir);
                    }

                    System.IO.StreamWriter Logfile = new System.IO.StreamWriter(_dir + "\\" + Title + "_" + DateTime.Now.ToString("dd") + "_" + 
                        DateTime.Now.ToString("MMM") + "_" + DateTime.Now.ToString("yy") + ".log", true);

                    Logfile.WriteLine(DateTime.Now.ToString() + ":- " + str);
                    Logfile.Close();
                }
            }
            catch (Exception ex)
            {
                this.LogViewer = ex.Message + " <" + ex.GetType().ToString() + ">";
            }
        }

        public void DisposeTimers()
        {
            if(NetCheckingTimer.Enabled)
                NetCheckingTimer.Stop();
            if(StatusResetAndSMSTimer.Enabled)
                StatusResetAndSMSTimer.Stop();
            if(UIupdateTimer.Enabled)
                UIupdateTimer.Stop();

            NetCheckingTimer.Dispose();
            StatusResetAndSMSTimer.Dispose();
            UIupdateTimer.Dispose();
        }
    }
}