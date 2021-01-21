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
        //public String NodeIdentifier = "link";
        public bool RepetitiveSMSActive = true;
        public bool SMS_ON = true;
        public bool SMSEvenAllUp = true;
        public String SMS_Server = "";
        public String Title = "";
        private bool AppLoadingFlag = true;

        private String _logviewer = "";
        public string Destination_Excel_url = "";///////////////////////////////////////////

        private DBConnect DB = new DBConnect();

        public ViewModel()
        {
            this.PropertyChanged += ViewModel_PropertyChanged;
            TimerforUIupdate();
            TimerforStatusResetAndSMS();
            NetCheckingTimer.Elapsed += NetCheckingTimer_Tick;


            Nodes = new ObservableCollection<Entity>();
            NodesList = new List<Entity>();
            PhoneNumbersList = new List<PhoneNumber>();
            DownNodesList = new List<Entity>();
            TempDownNodesList = new List<Entity>();
            UPNodesList = new List<Entity>();
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
                        Write_logFile(LogViewer);
                    }
                }));
            }

            else if (e.PropertyName == "StartPingFunctionality")
            {
                StarOrStopPingRequest();
            }
            else if (e.PropertyName == "DB_Host_Name")
            {
                DB.Host_Name = this.DB_Host_Name;
            }
            else if (e.PropertyName == "DatabaseName")
            {
                DB.Database = this.DatabaseName;
            }
            else if (e.PropertyName == "DB_UID")
            {
                DB.UID = this.DB_UID;
            }
            else if (e.PropertyName == "DB_PASSWORD")
            {
                DB.PASSWORD = this.DB_PASSWORD;
            }
            else if (e.PropertyName == "DB_DownTable_Name")
            {
                DB.CurrentDownTableName = this.DB_DownTable_Name;
            }
            else if (e.PropertyName == "DB_UpTable_Name")
            {
                DB.NodeStatusTableName = this.DB_UpTable_Name;
            }
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

        private string _db_host_name = "";
        private string _dbname = "";
        private string _db_uid;
        private string _db_pw;
        private string _db_downtable_name;
        private string _db_uptable_name;

        public string DB_Host_Name
        {
            get { return _db_host_name; }
            set
            {
                _db_host_name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DB_Host_Name");
            }
        }
        public string DatabaseName
        {
            get { return _dbname; }
            set
            {
                _dbname = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DatabaseName");
            }
        }
        public string DB_UID
        {
            get { return _db_uid; }
            set
            {
                _db_uid = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DB_UID");
            }
        }
        public string DB_PASSWORD
        {
            get { return _db_pw; }
            set
            {
                _db_pw = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DB_PASSWORD");
            }
        }

        public string DB_DownTable_Name
        {
            get { return _db_downtable_name; }
            set
            {
                _db_downtable_name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DB_DownTable_Name");
            }
        }

        public string DB_UpTable_Name
        {
            get { return _db_uptable_name; }
            set
            {
                _db_uptable_name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("DB_UpTable_Name");
            }
        }

        private string _nextSMSTime = "";
        public string NextSMSTime
        {
            get { return _nextSMSTime; }
            set
            {
                _nextSMSTime = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("NextSMSTime");
            }
        }

        private string _pingStatusText = "";
        public string PingStatusText
        {
            get { return _pingStatusText; }
            set
            {
                _pingStatusText = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("PingStatusText");
            }
        }

        private string _accountStatusText = "";
        public string AccountStatusText
        {
            get { return _accountStatusText; }
            set
            {
                _accountStatusText = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("AccountStatusText");
            }
        }

        public int NodesCount { get; set; } = 0;
        public int UpDownIndicator { get; set; } = 90;


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


        public string LogViewer
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(data));
        }

        private IList<Entity> NodesList { get; set; }

        private IList<Entity> DownNodesList { get; set; }

        private List<Entity> TempDownNodesList { get; set; }

        private List<Entity> UPNodesList { get; set; }

        private List<PhoneNumber> PhoneNumbersList { get; set; }

        public ObservableCollection<Entity> Nodes { get; set; }

        public void AppLoaded_Event()
        {
            DB.Title = Title;
        }


        private static System.Timers.Timer AppLoadingTimer = new System.Timers.Timer();

        public async void CheckforSyncDB()
        {
            if (!AppLoadingFlag)
            {
                await Task.Run(() => SyncDBAsync());
                Thread.Sleep(10000);

                TimerforSyncing();
            }

            if (!RunPingFunctionality)
                RunPingFunctionality = true;
        }

        private void TimerforSyncing()
        {
            AppLoadingFlag = true;
            AppLoadingTimer.Interval = 60000;
            AppLoadingTimer.AutoReset = true;
            AppLoadingTimer.Elapsed -= AppLoadingTimer_Tick;
            AppLoadingTimer.Elapsed += AppLoadingTimer_Tick;
            AppLoadingTimer.Start();
        }

        private async void AppLoadingTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (RunningDBSync)
                return;
            if (!RunPingFunctionality)
                return;
            foreach (var item in NodesList)
            {
                if (item.PingCount < 4)
                {
#if DEBUG
                    LogViewer = "App loading returned due to ping count less than 4.";
#endif
                    Write_logFile("App loading returned due to ping count less than 4.");
                    return;
                }
            }

            AppLoadingFlag = false;

            AppLoadingTimer.Stop();
            List<Entity> downlist = NodesList.Where(s => (s.Status == "Down")).ToList<Entity>();

            DownNodesList.Clear();
            foreach (var item in downlist)
            {
                item.DownTime = DateTime.Now;
                item.UpTime = null;
                int count = -1;
                count = await SearchinDBDownListAsync(item);
                if (count == 0)
                {
                    DownNodesList.Add(item);
                }
            }
            if (DownNodesList.Count > 0)
            {
                await InsertDBAsync();
            }
            LogViewer = "Completed MySQL Database sync in application side. System is stable now.";
            Write_logFile(LogViewer);            
        }

        private static System.Timers.Timer PingSenseTimer = new System.Timers.Timer();

        private void TimerforPingSenseMethod()
        {
            LogViewer = "Started monitoring ping for at least " + (PingSensePeriodForSMS + 1).ToString() + " min(s).";
            Write_logFile(LogViewer);

            PingSenseTimer.Interval = 60000;
            PingSenseTimer.AutoReset = true;
            PingSenseTimer.Elapsed -= PingSenseFlagTimer_Tick;
            PingSenseTimer.Elapsed += PingSenseFlagTimer_Tick;
            PingSenseTimer.Start();
        }

        public int CheckDBConnection()
        {
            int stat;
            lock (DB)
            {
                stat = DB.CheckDBConnection();
            }

            return stat;
        }

        private async void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status" && !AppLoadingFlag)
            {
                Entity en = (Entity)sender;
                if(en.Status == "Down")
                {
                    if (!TempDownNodesList.Contains(en))
                    {
                        int count = -1;
                        count = await SearchinDBDownListAsync(en);


                        if (count == 0)
                        {
                            LogViewer = "Monitoring '" + en.Name + "' for [Down] Status.";
                            Write_logFile(LogViewer);

                            TempDownNodesList.Add(en);
                            
                            if (!PingSenseTimer.Enabled)
                            {
                                TimerforPingSenseMethod();
                            }
                        }
                        else if (count > 1)
                        {
                            MessageBox.Show("Down Table in MySQL has duplicate data, remove one manually.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if(en.Status == "Up")
                {
                    if(!UPNodesList.Contains(en))
                    {
                        int count = -1;
                        count = await SearchinDBDownListAsync(en);


                        if (count == 1)
                        {
                            LogViewer = "Monitoring '" + en.Name + "' for [Up] Status.";
                            Write_logFile(LogViewer);
                            
                            UPNodesList.Add(en);
                            
                            if (!PingSenseTimer.Enabled)
                            {
                                TimerforPingSenseMethod();
                            }
                        }
                        else if(count > 1)
                        {
                            MessageBox.Show("Down Table in MySQL has duplicate data, remove one manually.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private Task<int> SearchinDBDownListAsync(Entity en)
        {
            return Task.Run(() => SearchinDBDownList(en));
        }

        private int SearchinDBDownList(Entity en)
        {
            int count = 0;

            lock (DB)
            {
                count = DB.SearchinCurrentDownNodes(en.IPAddress);
            }

            return count;
        }

        private async Task InsertDBAsync()
        {
            int inserted_downtable = 0, inserted_uptable = 0;

            foreach (var item in UPNodesList)
            {
                int count = -1, row = 0;
                count = await SearchinDBDownListAsync(item);

                if (count == 1)
                {
                    row = await InsertUpNodesDBAsync(item);
                    if(row == -1)
                    {
                        inserted_uptable = -1;
                        break;
                    }
                    else
                    {
                        inserted_uptable++;
                    }
                }
                else if(count == -1)
                {
                    inserted_uptable = -1;
                    break;
                }
            }

            if(inserted_uptable != -1)
            {
                if (inserted_uptable > 0)
                {
                    LogViewer = "Number of rows inserted into '" + DB_UpTable_Name + "' table in DB: " + inserted_uptable + ".";
                    Write_logFile(LogViewer);
                }
                UPNodesList.Clear();
            }

            foreach (var item in DownNodesList)
            {
                int count = -1, row = 0;
                count = await SearchinDBDownListAsync(item);

                if (count == 0)
                {
                    row = await InsertDownNodesDBAsync(item);
                    if (row == -1)
                    {
                        inserted_downtable = -1;
                        break;
                    }
                    else
                    {
                        inserted_downtable++;
                    }
                }
                else if (count == -1)
                {
                    inserted_downtable = -1;
                    break;
                }
            }

            if (inserted_downtable != -1)
            {
                if (inserted_downtable > 0)
                {
                    LogViewer = "Number of rows inserted into '" + DB_DownTable_Name + "' table in DB: " + inserted_downtable + ".";
                    Write_logFile(LogViewer);
                }
                DownNodesList.Clear();
            }
        }


        public Task<int> InsertUpNodesDBAsync(Entity en)
        {
            return Task.Run(() => DBInsertion4UpNodes(en));
        }

        public Task<int> InsertDownNodesDBAsync(Entity en)
        {
            return Task.Run(() => DBInsertion4DownNodes(en));
        }

        private int DBInsertion4UpNodes(Entity en)
        {
            int count = -1;
            lock (DB)
            {
                string downtime = "";
                downtime = DB.SelectDownTimefromDownTable(en.IPAddress);


                if (downtime != "")
                {
                    DateTime dt = DateTime.Parse(downtime);
                    TimeSpan ts = en.UpTime.Value.Subtract(dt);

                    string duration_ddhhmm = ts.Days.ToString() + "-" + ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00");
                    int Totalhours = (int)ts.TotalHours;
                    int min = ts.Minutes;
                    
                    string monthCycle = en.UpTime.Value.Year.ToString() + en.UpTime.Value.Month.ToString("00");
                    string dateCycle = en.UpTime.Value.Day.ToString();
                    count = DB.InsertUpNodes(en.IPAddress, en.Name, en.Area, dt.ToString("yyyy-MM-dd HH:mm:ss"), en.UpTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), 
                        duration_ddhhmm, Totalhours.ToString(), min.ToString(), monthCycle, dateCycle);

                    if(count == 1)
                    {
                        DB.DeletefromDownTable(en.IPAddress);
                    }
                }
            }
            return count;
        }

        private int DBInsertion4DownNodes(Entity en)
        {
            int count = -1;
            lock (DB)
            {
                count = DB.InsertDownNodes(en.IPAddress, en.Name, en.Area, en.DownTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            return count;
        }

        bool RunningDBSync = false;

        private async void SyncDBAsync()
        {
            LogViewer = "Syncing MySQL Database......";
            Write_logFile(LogViewer);
            RunningDBSync = true;
            int status = 0;
            List<string> down_ip_list_fromDB = new List<string>();
            lock (DB)
            {
                down_ip_list_fromDB = DB.SelectDownNodes();
            }

            List<String> ip_list = NodesList.Select(o => o.IPAddress).ToList();

            UPNodesList.Clear();
            foreach (var item in down_ip_list_fromDB)
            {
                if (ip_list.Contains(item))
                {
                    int i = 0;
                    while(status != -1)
                    {
                        status = await TryToPingNodesAync(item);
                        if (status == 1) break;
                        i++;
                        if (i == 4) break;
                    }

                    if(status == 1)
                    {
                        NodesList.FirstOrDefault(s => (s.IPAddress == item)).UpTime = DateTime.Now;
                        NodesList.FirstOrDefault(s => (s.IPAddress == item)).DownTime = null;
                        UPNodesList.Add(NodesList.FirstOrDefault(s => (s.IPAddress == item)));
                    }
                    else if(status == -1)
                    {
                        break;
                    }
                }
                else
                {
                    lock (DB)
                    {
                        DB.DeletefromDownTable(item);
                    }
                }
            }

            if(status != -1)
            {
                if (UPNodesList.Count > 0)
                {
                    await InsertDBAsync();
                }
                RunningDBSync = false;
                LogViewer = "Completed MySQL Database sync in DB side.";
                Write_logFile(LogViewer);
            }
            else
            {
                LogViewer = "Failed to sync. It will retry after sometimes";
                Write_logFile(LogViewer);
            }
        }


        private void StarOrStopPingRequest()
        {
            if (RunPingFunctionality)
            {
                RequestPingAsync();
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


        public int SMSInterval = 180; //min
        public int PingSensePeriodForSMS = 4; //min
        //public int RefreshPeriod = 6; //min

        private static System.Timers.Timer StatusResetAndSMSTimer = new System.Timers.Timer();

        void TimerforStatusResetAndSMS()
        {
            StatusResetAndSMSTimer.Interval = 60000;//1 minute (60000) is fixed for release/////////////////////////////////////////////consider always////////////////////////////////////////////////////
            StatusResetAndSMSTimer.AutoReset = true;
            StatusResetAndSMSTimer.Elapsed -= StatusResetAndSMSTimer_Elapsed;
            StatusResetAndSMSTimer.Elapsed += StatusResetAndSMSTimer_Elapsed;
        }


        int timeCounter = 0;

        private void StatusResetAndSMSTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timeCounter++;
#if DEBUG
            LogViewer = "timeCounter " + timeCounter.ToString();
#endif

            if (timeCounter == SMSInterval - 1 && RepetitiveSMSActive)
            {
                ResetStatus();
                LogViewer = "Status of all nodes refreshed.";
                Write_logFile(LogViewer);
            }
            else if (timeCounter == SMSInterval)
            {
                if (RepetitiveSMSActive)
                {
                    foreach (var item in NodesList)
                    {
                        if (item.PingCount < 4)
                        {
                            timeCounter--;
#if DEBUG
                            LogViewer = "TimeCounter returned from SMS trigger due to ping count less than 4.";
#endif
                            Write_logFile("TimeCounter returned from SMS trigger due to ping count less than 4.");
                            return;
                        }
                    }
                    timeCounter = 0;
                    NextSMSTime = DateTime.Now.AddMinutes(SMSInterval).ToLongTimeString();
                    if (RunPingFunctionality)
                        this.RunPingFunctionality = false;
                    SMSThreadMethod();
                }
                else
                {
                    timeCounter = 0;
                    ResetStatus();
#if DEBUG
                    LogViewer = "Refreshed.";
#endif
                    Write_logFile("Refreshed.");
                }
            }
            else if (timeCounter % PingSensePeriodForSMS == 0 && !PingSenseTimer.Enabled) //refresh each refresh interval
            {
                ResetStatus();
#if DEBUG
                LogViewer = "Refreshed.";
#endif
                Write_logFile("Refreshed.");                
            }
        }


        bool sleepBeforePingAfterRefresh = false;
        public void ResetStatus()
        {
            if (RunPingFunctionality)
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

            sleepBeforePingAfterRefresh = true;
            if (!RunPingFunctionality)
                this.RunPingFunctionality = true;
        }

        public async void SMSThreadMethod()
        {
            Task tsk = SMSThreadMethodAsync();
            await Task.WhenAll(tsk);
            tsk.Dispose();
        }


        private async Task SMSThreadMethodAsync()
        {
            await Task.Run(() => CheckNetBeforeSMS());
            await Task.Run(() => InsertDBAsync());
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


        int pingsensetimecounter = 0;

        private void PingSenseFlagTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            pingsensetimecounter++;
            if (pingsensetimecounter < PingSensePeriodForSMS)
                return;

            if (pingsensetimecounter == PingSensePeriodForSMS)
            {
                ResetStatus();
                LogViewer = "Status of all nodes refreshed before status change event fire.";
                Write_logFile(LogViewer);
            }
            else if (pingsensetimecounter > PingSensePeriodForSMS)
            {
                foreach (var item in NodesList)
                {
                    if (item.PingCount < 4)
                    {
#if DEBUG
                        LogViewer = "Ping sense returned due to ping count less than 4.";
#endif
                        Write_logFile("Ping sense returned due to ping count less than 4.");
                        return;
                    }
                }

                pingsensetimecounter = 0;

#if DEBUG
                LogViewer = "Ping sense ticked.";
#endif
                Write_logFile("Ping sense ticked.");
                if (RunPingFunctionality)
                    this.RunPingFunctionality = false;
                Thread.Sleep(5000);

                PingSenseTimer.Stop();
                //List<String> down_ip_list = NodesList.Where(s => (s.Status == "Down")).ToList<Entity>().Select(o => o.IPAddress).ToList();

                int downnodescount = NodesList.Where(s => (s.Status == "Down" || s.Status == "Unknown")).ToList<Entity>().Count;
                if (downnodescount != NodesList.Count)
                {
                    bool fluctuationFlag4down = true, fluctuationFlag4up = true;
                    bool shouldsendSMS4up = false, shouldsendSMS4down = false;
                    if (TempDownNodesList.Count > 0)
                    {
                        TempDownNodesList.RemoveAll(item => item.Status != "Down");

                        if (TempDownNodesList.Count > 0)
                        {
                            shouldsendSMS4down = true;
                        }
                    }
                    else
                    {
                        fluctuationFlag4down = false;
                    }

                    if (UPNodesList.Count > 0)
                    {
                        UPNodesList.RemoveAll(item => item.Status != "Up");

                        if (UPNodesList.Count > 0)
                        {
                            shouldsendSMS4up = true;
                        }
                    }
                    else
                    {
                        fluctuationFlag4up = false;
                    }

                    if (shouldsendSMS4down && !shouldsendSMS4up)
                    {
                        LogViewer = "Firing SMS for node changing to Down status.";
                        Write_logFile(LogViewer);
                    }
                    else if (!shouldsendSMS4down && shouldsendSMS4up)
                    {
                        TempDownNodesList.Clear();
                        LogViewer = "Firing SMS for node changing to Up status.";

                        Write_logFile(LogViewer);
                    }
                    else if (shouldsendSMS4down && shouldsendSMS4up)
                    {
                        LogViewer = "Firing SMS for both Status changes for some links.";
                        Write_logFile(LogViewer);
                    }
                    else
                    {
                        TempDownNodesList.Clear();
                        if (!fluctuationFlag4down && !fluctuationFlag4up)
                        {
                            LogViewer = "SMS may be sent already, so it halted from status changing trigger.";
                            Write_logFile(LogViewer);
                        }
                        else
                        {
                            LogViewer = "SMS halted due to status reverse (fluctuation).";
                            Write_logFile(LogViewer);
                        }
                    }

                    if (shouldsendSMS4up || shouldsendSMS4down)
                    {
                        SMSThreadMethod();
                    }
                    else
                    {
                        if (!RunPingFunctionality)
                            this.RunPingFunctionality = true;
                    }
                }
                else
                {
                    LogViewer = "Please check internet connection.";
                    Write_logFile("Network down when firing SMS due to status change.");
                }
            }
        }



        //int DCount = 0, UCount = 0;
        private void BuildDownNodesList()
        {
            //DCount = 0;
            DownNodesList.Clear();
            List<Entity> _downNodesList = NodesList.Where(s => (s.Status == "Down")).ToList<Entity>();

            if (_downNodesList.Count > 0)
            {
                if (TempDownNodesList.Count > 0)
                {
                    foreach (var item in TempDownNodesList)
                    {
                        if(_downNodesList.Contains(item))
                        {
                            DownNodesList.Add(item);
                            //SMSContentString = SMSContentString + ", " + item.Area;
                            //DCount++;
                        }
                        else
                        {
                            LogViewer = "Fluctuation found during send SMS for '" + item.Name + "'";
                            Write_logFile(LogViewer);
                        }
                    }
                }
                foreach (var item in _downNodesList)
                {
                    if (item.DownTime == null)
                    {
                        item.DownTime = DateTime.Now;
                    }
                    item.UpTime = null;

                    if (/*item.Action_Type == NodeType.SMSENABLED.ToString() && */!TempDownNodesList.Contains(item))
                    {
                        DownNodesList.Add(item);
                        //SMSContentString = SMSContentString + ", " + item.Area;
                        //DCount++;
                    }
                }
            }
            TempDownNodesList.Clear();
        }

        private void BuildUpNodesList()
        {
            //UCount = 0;

            if (UPNodesList.Count > 0)
            {
                UPNodesList.RemoveAll(item => item.Status != "Up");

                if (UPNodesList.Count > 0)
                {
                    foreach (var item in UPNodesList)
                    {
                        if (item.UpTime == null)
                        {
                            item.UpTime = DateTime.Now;
                        }
                        item.DownTime = null;
                        //if (item.Action_Type == NodeType.SMSENABLED.ToString())
                        //{
                        //    //SMSContentString = SMSContentString + ", " + item.Area;
                        //    UCount++;
                        //}
                    }
                }
                else
                {
                    LogViewer = "Fluctuation found during send SMS for Up nodes.";
                    Write_logFile(LogViewer);
                }
            }
        }



        private async void SMSFunction()
        {
            int downnodescount = NodesList.Where(s => (s.Status == "Down" || s.Status == "Unknown")).ToList<Entity>().Count;
            if (downnodescount != NodesList.Count)
            {
                //String SMSContentString_downNodes = "";
                //String SMSContentString_UpNodes = "";

                BuildDownNodesList();
                BuildUpNodesList();

                if(SMS_ON)
                {
                    IEnumerable<int> phonenumbersforSMS = new List<int>();

                    foreach (var item in DownNodesList)
                    {
                        phonenumbersforSMS = phonenumbersforSMS.Union(item.PhoneNumbersList);
                    }

                    foreach (var item in UPNodesList)
                    {
                        phonenumbersforSMS = phonenumbersforSMS.Union(item.PhoneNumbersList);
                    }

                    phonenumbersforSMS = phonenumbersforSMS.Distinct();

                    PhoneNumbersList.Clear();

                    foreach(var phn in phonenumbersforSMS)
                    {
                        PhoneNumber phoneNumber = new PhoneNumber();

                        phoneNumber.Phone = phn;

                        phoneNumber.DownEntities = DownNodesList.Where(x => x.PhoneNumbersList.Contains(phn)).ToList();

                        phoneNumber.UpEntities = UPNodesList.Where(x => x.PhoneNumbersList.Contains(phn)).ToList();

                        PhoneNumbersList.Add(phoneNumber);
                    }

                    NumberofDestination = PhoneNumbersList.Count;
                    NumberofFailtoSendSMS = 0;
                    SentMsgCount = 0;

                    LogViewer = "Sending SMS, please wait ... .. .";
                    Write_logFile(LogViewer);

                    foreach (var phn in PhoneNumbersList)
                    {

                        SMSTrigger(phn);
                    }
                }
                else
                {
                    //DCount = 0;
                    //UCount = 0;
                    timeCounter = 0;
                    if (!RunPingFunctionality)
                        this.RunPingFunctionality = true;
                }
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


        void SMSTrigger(PhoneNumber phone)
        {
            String SMSContentString = "";

            string SMSContentString_DownNodes = "";

            foreach(var item in phone.DownEntities)
            {
                if (item.Action_Type == NodeType.SMSENABLED.ToString())
                {
                    SMSContentString_DownNodes = SMSContentString_DownNodes + ", " + item.Area; /////////////////sms column from excel
                }
            }

            int contentlen = SMSContentString_DownNodes.Length;

            if (SMSContentString_DownNodes != "")
            {
                if (SMSContentString_DownNodes[0] == ',')
                {
                    SMSContentString_DownNodes = SMSContentString_DownNodes.Substring(2, contentlen - 2);
                }
            }


            string SMSContentString_UpNodes = "";

            foreach (var item in phone.UpEntities)
            {
                if (item.Action_Type == NodeType.SMSENABLED.ToString())
                {
                    SMSContentString_UpNodes = SMSContentString_UpNodes + ", " + item.Area; /////////////////sms column from excel
                }
            }

            contentlen = SMSContentString_UpNodes.Length;

            if (SMSContentString_UpNodes != "")
            {
                if (SMSContentString_UpNodes[0] == ',')
                {
                    SMSContentString_UpNodes = SMSContentString_UpNodes.Substring(2, contentlen - 2);
                }
            }


            if (phone.UpEntities.Count + phone.DownEntities.Count > 0)
            {
                if (phone.UpEntities.Count > 1)
                {
                    SMSContentString = SMSContentString_UpNodes + " are up.\n";
                }
                else if (phone.UpEntities.Count == 1)
                {
                    SMSContentString = SMSContentString_UpNodes + " is up.\n";
                }

                if (phone.DownEntities.Count > 1)
                {
                    SMSContentString = SMSContentString + SMSContentString_DownNodes + " are down.\n";
                }
                else if (phone.DownEntities.Count == 1)
                {
                    SMSContentString = SMSContentString + SMSContentString_DownNodes + " is down.\n";
                }
                //else if(phone.DownEntities.Count == 0)
                //{
                //    SMSContentString = AllLinksUpMessage + "\n";
                //}
            }
            //else
            //{
            //    if (SMSEvenAllUp)
            //    {
            //        SMSContentString = AllLinksUpMessage + "\n";
            //    }
            //    else
            //    {
            //        LogViewer = "All " + NodeIdentifier + "s are up now, so message will not be sent.";
            //    }
            //}

            if (SMSContentString != "")
            {
                Write_logFile("Phone: " + phone.Phone.ToString() + ", SMS :" + 
                    SMSContentString.Substring(0, SMSContentString.Length - 1).Replace("\n" , " "));

                SMSContentString = Message_Header + "\n" + SMSContentString + Message_Footer;


                if (NumberofFailtoSendSMS == 0)
                {
                    HttpCalltoTeletalk(phone.Phone, SMSContentString);
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
                    String UrlString = "http://" + SMS_Server + "/link_sms_send.php?op=SMS&user=" + User_Name_String + "&pass=" + Password_String;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@UrlString);
                    request.AllowWriteStreamBuffering = false;


                    WebResponse response = request.GetResponse();
                    // Display the Status.
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
                catch (Exception)
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
                String UrlString = "http://" + SMS_Server + "/link_sms_send.php?op=SMS&user=" + User_Name_String + "&pass=" + Password_String + "&mobile=0" + PhnNum.ToString() + "&sms=" + SMSContentString;//#############################################################
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@UrlString);
                request.AllowWriteStreamBuffering = false;

                
                WebResponse response = request.GetResponse();
                // Display the Status.
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
                        

                        string str = "Sent SMS today, " + DateTime.Now.ToShortDateString() + ", total number of SMS: " + NumberofDestination.ToString() + ".";
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

                        //DCount = 0;
                        //UCount = 0;
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
                        s = "Error in SMS sending to: 0" + PhnNum.ToString() + ", " + ex.Message + " <" + ex.GetType().ToString() + ">" + ", Number of Attemt: " + NumberofFailtoSendSMS;
                    }
                    else
                    {
                        s = "Error in SMS sending to: 0" + PhnNum.ToString() + ", " + ex.Message + " <" + ex.GetType().ToString() + ": " + ex.InnerException.ToString() + ">" + ", Number of Attemt: " + NumberofFailtoSendSMS;
                    }

                    Write_logFile(s);/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    LogViewer = "Error in SMS sending to: 0" + PhnNum.ToString() + ", " + ex.Message + " <" + ex.GetType().ToString() + ">" + ", Number of Attemt: " + NumberofFailtoSendSMS;
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
                if (!RunPingFunctionality)
                    this.RunPingFunctionality = true;
            }
        }


        private static System.Timers.Timer UIupdateTimer = new System.Timers.Timer();

        void TimerforUIupdate()
        {
            UIupdateTimer.Interval = 1000;  // 1 sec
            UIupdateTimer.AutoReset = true;
            UIupdateTimer.Elapsed -= UIupdateTimer_Tick;
            UIupdateTimer.Elapsed += UIupdateTimer_Tick;
        }

        private void UIupdateTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateUINodes();
        }

        public async void RequestPingAsync()
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
            if (sleepBeforePingAfterRefresh)
            {
                sleepBeforePingAfterRefresh = false;
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

        public async Task LoadExcelData()
        {
            UILoadingAnimation = true; //for logo
            bool load_or_not = await Task.Run(() => ImportExcelFile());


            UILoadingAnimation = false; //for logo   
            if (!load_or_not)
            {
                NodesList.Clear();
                PhoneNumbersList.Clear();
                NodesCount = 0;
                ExcelLoaded = false;
                return;
            }

            ExcelLoaded = true;

            if (this.NodesList.Count > 0)
            {
                NodesCount = NodesList.Count;
                LogViewer = "Excel file imported. Total number of nodes: " + NodesList.Count.ToString();
                Write_logFile(LogViewer);

                int cnt = (from _itm in NodesList
                           where _itm.Action_Type == NodeType.SMSENABLED.ToString()
                           select _itm).Count();
                LogViewer = "Number of nodes which should be notified through SMS: " + cnt.ToString();
                Write_logFile(LogViewer);

                cnt = (from _itm in NodesList
                       where _itm.Action_Type == NodeType.PINGONLY.ToString()
                       select _itm).Count();
                LogViewer = "Number of nodes which will ping only: " + cnt.ToString();
                Write_logFile(LogViewer);

                if (timeCounter == 0)
                {
                    NextSMSTime = DateTime.Now.AddMinutes(SMSInterval).ToLongTimeString();
                    Write_logFile("Next SMS time: " + NextSMSTime);
                }                  

                await Task.Run(() => SyncDBAsync());
                Thread.Sleep(10000);

                TimerforSyncing();

                if (!RunPingFunctionality)
                    RunPingFunctionality = true;
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

        private bool ImportExcelFile()
        {
            bool load_or_not = false;
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet1;
            xlWorkBook = xlApp.Workbooks.Open(Destination_Excel_url, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet1 = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            Excel.Worksheet xlWorkSheet2;
            xlWorkSheet2 = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);

            try
            {
                NodesList.Clear();

                Excel.Range last = xlWorkSheet1.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);


                int lastUsedRow = last.Row;
                int lastUsedColumn = last.Column;

                int rowcount = 0;
                foreach (Excel.Range row in xlWorkSheet1.Rows)
                {
                    rowcount++;
                    if (rowcount == lastUsedRow + 1)
                        break;

                    if (rowcount > 1)
                    {
                        Entity _nd = new Entity();
                        _nd.PingCount = 0;
                        _nd.PingFailed = 0;
                        _nd.SuccessPingCount = 0;
                        _nd.PercentageLoss = 0;
                        _nd.AverageRoundTripTime = 0;
                        _nd.MaxRoundTripTime = 0;
                        _nd.MinRoundTripTime = 999999;

                        foreach (Excel.Range cell in row.Cells)
                        {
                            if(cell.Address.Contains("A"))
                                _nd.IPAddress = cell.Value2.ToString();
                            else if (cell.Address.Contains("B"))
                                _nd.Name = cell.Value2.ToString();
                            else if (cell.Address.Contains("C"))
                            {
                                string str = cell.Value2.ToString();
                                if (str.ToUpper().Contains("SMSENABLED"))
                                {
                                    _nd.Action_Type = NodeType.SMSENABLED.ToString();
                                }
                                else
                                {
                                    _nd.Action_Type = NodeType.PINGONLY.ToString();
                                }
                            }
                            else if (cell.Address.Contains("D"))
                                _nd.Area = cell.Value2.ToString();

                            else if (cell.Value2 != null)
                            {
                                _nd.PhoneNumbersList.Add(Convert.ToInt32(cell.Value2.ToString()));
                            }                            
                            else
                                break;
                        }


                        _nd.PropertyChanged -= Node_PropertyChanged;
                        _nd.PropertyChanged += Node_PropertyChanged;

                        NodesList.Add(_nd);
                    }
                }

                //for (int i = 2; i <= lastUsedRow; i++)
                //{

                //}

                //last = xlWorkSheet2.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);

                //lastUsedRow = last.Row;
                //lastUsedColumn = last.Column;

                //PhoneNumberList.Clear();
                //for (int i = 2; i <= lastUsedRow; i++)
                //{
                //    string str = xlWorkSheet2.Cells[i, 1].Value2.ToString();
                //    PhoneNumberList.Add(Convert.ToInt32(str));
                //}

                load_or_not = true;
            }
            catch (Exception ex)
            {
                load_or_not = false;
                this.LogViewer = "Error in importing excel: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                Write_logFile("Error in importing excel: " + ex.Message + " <" + ex.GetType().ToString() + ">");
                MessageBox.Show("There may be wrong data in excel file.\nPlease correct the excel file and load again.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
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
            return load_or_not;
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

        private async Task<int> TryToPingNodesAync(string ipaddress)
        {
            int replyStatus = -1;
            Ping pingSender = new Ping();
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    PingReply reply = await pingSender.SendPingAsync(ipaddress);
                    if(reply.Status == IPStatus.Success)
                    {
                        replyStatus = 1;
                    }
                    else
                    {
                        replyStatus = 0;
                    }
                }
                else
                {
                    LogViewer = "Network cable may be unplugged. Please fix it as soon as possible.";
                    MessageBox.Show(LogViewer, Title, MessageBoxButton.OK, MessageBoxImage.Error);

                    EnablingTimerforNetChecking(60);
                }
            }
            catch(Exception ex)
            {
                LogViewer = "Exception in DBSync: " + ex.Message;
                Write_logFile(LogViewer);

                EnablingTimerforNetChecking(180);
            }
            finally
            {
                pingSender.Dispose();
            }
            return replyStatus;
        }

        private void TryToPingNodes()
        {
            while (RunPingFunctionality)
            {
                Parallel.For(0, NodesList.Count, async (index, loopStatus) =>
                {
                    Ping pingSender = new Ping();
                    try
                    {
                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            PingReply reply = await pingSender.SendPingAsync(NodesList[index].IPAddress);

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

                                if (NodesList[index].PercentageLoss >= 0 && NodesList[index].PercentageLoss < 20 && NodesList[index].PingCount >= 4)
                                {
                                    NodesList[index].Color_Type2 = Colors.Green;
                                    if(NodesList[index].Status != "Up")
                                        NodesList[index].Status = "Up";
                                }
                                else if (NodesList[index].PercentageLoss >= 20 && NodesList[index].PercentageLoss < 50 && NodesList[index].PingCount >= 4)
                                {
                                    NodesList[index].Color_Type2 = Colors.Blue;
                                    if (NodesList[index].Status != "Sufficient")
                                        NodesList[index].Status = "Sufficient";
                                }
                                else if (NodesList[index].PercentageLoss >= 50 && NodesList[index].PercentageLoss < this.UpDownIndicator && NodesList[index].PingCount >= 4)
                                {
                                    NodesList[index].Color_Type2 = Colors.DarkOrange;
                                    if (NodesList[index].Status != "Poor")
                                        NodesList[index].Status = "Poor";
                                }
                                else if (NodesList[index].PercentageLoss >= this.UpDownIndicator && NodesList[index].PingCount >= 4)
                                {
                                    NodesList[index].Color_Type2 = Colors.Red;
                                    if (NodesList[index].Status != "Down")
                                        NodesList[index].Status = "Down";
                                }
                            }
                            else
                            {
                                loopStatus.Stop();
                                return;
                            }
                        }
                        else
                        {
                            if (this.RunPingFunctionality)
                            {
                                this.RunPingFunctionality = false;
                                LogViewer = "Error in Network adapter: " + "Network cable may be unplugged. Please fix it as soon as possible.";
                                MessageBox.Show(LogViewer, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                            Write_logFile("Error in Network adapter: node number: " + index.ToString() + ". " + "Network cable may be unplugged. No connection is found.");

                            EnablingTimerforNetChecking(60);
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

                        EnablingTimerforNetChecking(180);

                    }
                    finally
                    {
                        pingSender.Dispose();
                    }
                });
            }
        }

        private Object netcheckerLock = new Object();


        private static System.Timers.Timer NetCheckingTimer = new System.Timers.Timer();

        private void EnablingTimerforNetChecking(int i)
        {
            lock (netcheckerLock)
            {
                if (!NetCheckingTimer.Enabled)
                {
                    NetCheckingTimer.AutoReset = true;
                    NetCheckingTimer.Interval = i * 1000;
                    LogViewer = "Ping Paused for " + i.ToString() + " seconds.";
                    Write_logFile(LogViewer);
                    NetCheckingTimer.Start();
                }
            }
        }

        private async void NetCheckingTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!RunPingFunctionality)
                    this.RunPingFunctionality = true;
                NetCheckingTimer.Stop();
                if(RunningDBSync)
                {
                    await Task.Run(() => SyncDBAsync());
                }

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