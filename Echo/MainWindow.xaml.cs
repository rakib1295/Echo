using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Deployment.Application;

namespace Echo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CollectionView view;
        ViewModel VM = new ViewModel();

        //private static System.Windows.Forms.NotifyIcon iconInstance;
        //public static System.Windows.Forms.NotifyIcon IconInstance  //tray
        //{
        //    get
        //    {
        //        if (iconInstance == null)
        //        {
        //            iconInstance = new System.Windows.Forms.NotifyIcon();
        //        }
        //        return iconInstance;
        //    }
        //}

        public MainWindow()
        {
            InitializeComponent();

            timerforPopup.Tick += timer_TickForPopup;
            timerforPopupEdit.Tick += timer_TickForPopupEdit;
            Browse_Btn_Animation();
            DispatcherTimerClock();
            VM.PropertyChanged += View_PropertyChanged;

            DispatcherTimerLogoAnimation();   //for logo

            c11.CellTemplate = (DataTemplate)this.Resources["dTemplate_c11"];
            c12.CellTemplate = (DataTemplate)this.Resources["dTemplate_c12"];
            c13.CellTemplate = (DataTemplate)this.Resources["dTemplate_c13"];
            c6.CellTemplate = (DataTemplate)this.Resources["dTemplate_c6"];


            IPListView.ItemsSource = VM.Nodes;
            view = (CollectionView)CollectionViewSource.GetDefaultView(IPListView.ItemsSource);

            view.Filter = UserFilter;
            VM.Title = this.Title;

            //IPListView.SelectionChanged += IPListView_SelectionChanged;

            //Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/echo_crop_5qo_icon.ico")).Stream; /////tray
            //IconInstance.Icon = new System.Drawing.Icon(iconStream);

            //IconInstance.Visible = true;
            //this.Closing += MainWindow_Closing;
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            Application.Current.MainWindow.Loaded += MainWindow_Loaded;

#if !DEBUG
            versionNumber.Text = "Version: " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4);
#endif
            //IconInstance.Text = "Echo";


            //IconInstance.DoubleClick +=
            //    delegate(object sender, EventArgs args)
            //    {
            //        this.Show();
            //        this.WindowState = WindowState.Maximized;
            //    };

            //IconInstance.MouseClick += ni_MouseClick;
        }

        int _popupcounter;
        Entity _itm = new Entity();
        DispatcherTimer timerforPopupEdit = new DispatcherTimer();

        private void timer_TickForPopupEdit(object sender, EventArgs e)
        {
            _popupcounter--;
            PopcounterTxtblk.Text = _popupcounter.ToString();
            if (_popupcounter == 0)
            {
                Popup_Edit.IsOpen = false;
                PopcounterTxtblk.Text = "15";
                timerforPopupEdit.Stop();
            }
        }


        private void Popup_Edit_MouseMove(object sender, MouseEventArgs e)
        {
            PopcounterTxtblk.Text = "15";
            _popupcounter = 15;
        }


        private void IPListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Popup_Edit.IsOpen = true;
            timerforPopupEdit.Interval = TimeSpan.FromSeconds(1);
            _popupcounter = 15;
            PopcounterTxtblk.Text = "15";
            timerforPopupEdit.Start();
            try
            {
                _itm = (Entity)IPListView.SelectedItem;
                _NodeName.Text = _itm.Name;
                _NodeIP.Text = _itm.IpAddress;
                _NodeStatus.Text = _itm.Status;

                if (_itm.Action_Type == NodeType.SMSENABLED.ToString())
                {
                    _NodeSMSStatus.Text = "SMS Enabled";
                    _NodeSMSStatus.Foreground = Brushes.Green;
                    Edit_btn.Content = "Temporarily disable SMS for this link?";
                }
                else
                {
                    _NodeSMSStatus.Text = "Ping Only";
                    _NodeSMSStatus.Foreground = Brushes.Red;
                    Edit_btn.Content = "Enable SMS for this link ->";
                }
            }
            catch (Exception ex)
            {
                Show_LogTextblock("Please double click again.");
                VM.Write_logFile("Exception while double clicking on a link: " + ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }



        private void ConfigureEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_itm.Action_Type == NodeType.SMSENABLED.ToString())
            {
                _itm.Action_Type = NodeType.PINGONLY.ToString();
                _NodeSMSStatus.Text = "Ping Only";
                _NodeSMSStatus.Foreground = Brushes.Red;
                Edit_btn.Content = "Enable SMS for this link ->";
                Show_LogTextblock(_itm.Area + " link has been configured as " + _itm.Action_Type + ". No SMS will be sent for this link.");
            }
            else
            {
                _itm.Action_Type = NodeType.SMSENABLED.ToString();
                _NodeSMSStatus.Text = "SMS Enabled";
                _NodeSMSStatus.Foreground = Brushes.Green;
                Edit_btn.Content = "Temporarily disable SMS for this link?";
                Show_LogTextblock(_itm.Area + " link has been configured as " + _itm.Action_Type + ".");
            }
        }

        private void ConfigureCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Popup_Edit.IsOpen = false;
            PopcounterTxtblk.Text = "15";
            timerforPopupEdit.Stop();
        }

        private void exit_function_Click_1(object sender, RoutedEventArgs e)
        {
            //if (MessageBox.Show("Do you really want to close Echo?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    //IconInstance.Dispose(); ///////////////////////////////////////////////////////////////////////////////////////////////////////////tray

            //}
            this.Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) //tary
        {
            bool flag = true;
            if (VM.ExcelLoaded)
            {
                if (VM.RunPingFunctionality == false)
                {
                    flag = false;
                }
                VM.RunPingFunctionality = false;
            }
            if (MessageBox.Show("Do you really want to close the application?", VM.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                e.Cancel = true;
                if (VM.ExcelLoaded && flag == true)
                    VM.RunPingFunctionality = true;
            }
            else if (MessageBox.Show("Are you sure?", VM.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                e.Cancel = true;
                if (VM.ExcelLoaded && flag == true)
                    VM.RunPingFunctionality = true;
            }
            else
            {
                VM.DisposeTimers();
                SaveSettings();
            }

            //IconInstance.Dispose();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.SMS_Checkbox_Data = (bool)SMS_Checkbox.IsChecked;
            Properties.Settings.Default.User_Name_String = user_name.Text;
            Properties.Settings.Default.Password_String = acc_psw.Password;
            Properties.Settings.Default.ParcentLoss = ParcentLoss_txtbox.Text;
            Properties.Settings.Default.SMS_Interval = SMSInterval_txtbox.Text;
            Properties.Settings.Default.Refresh_Interval = RefreshInterval_txtbox.Text;
            Properties.Settings.Default.PingSenseTime = PingSenseTime_txtbox.Text;
            Properties.Settings.Default.MsgHeader = MsgHeader_txtbox.Text;
            Properties.Settings.Default.MsgFooter = MsgFooter_txtbox.Text;
            Properties.Settings.Default.SMSIfAllUp = (bool)SMSIfAllUp_Checkbox.IsChecked;
            Properties.Settings.Default.AllLinksUp_txt = AllLinksUp_txtbox.Text;

            Properties.Settings.Default.Save();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SMS_Checkbox.IsChecked = Properties.Settings.Default.SMS_Checkbox_Data;
            user_name.Text = Properties.Settings.Default.User_Name_String;
            acc_psw.Password = Properties.Settings.Default.Password_String;
            Properties.Settings.Default.ParcentLoss = ParcentLoss_txtbox.Text;
            SMSInterval_txtbox.Text = Properties.Settings.Default.SMS_Interval;
            RefreshInterval_txtbox.Text = Properties.Settings.Default.Refresh_Interval;
            PingSenseTime_txtbox.Text = Properties.Settings.Default.PingSenseTime;
            MsgHeader_txtbox.Text = Properties.Settings.Default.MsgHeader;
            MsgFooter_txtbox.Text = Properties.Settings.Default.MsgFooter;
            SMSIfAllUp_Checkbox.IsChecked = Properties.Settings.Default.SMSIfAllUp;
            AllLinksUp_txtbox.Text = Properties.Settings.Default.AllLinksUp_txt;
        }


        private void Default_btn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to reset settings data to default value?", VM.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SMS_Checkbox.IsChecked = true;
                Properties.Settings.Default.ParcentLoss = "90";
                SMSInterval_txtbox.Text = "120";
                RefreshInterval_txtbox.Text = "10";
                PingSenseTime_txtbox.Text = "5";
                MsgHeader_txtbox.Text = "Dear Sir,";
                MsgFooter_txtbox.Text = "NOC\nMoghbazar\nT-0258312345";
                SMSIfAllUp_Checkbox.IsChecked = true;
                AllLinksUp_txtbox.Text = "All links are up now.";
                Show_LogTextblock("Settings data has been reset to default.");
            }
        }



        //private void ni_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Button == System.Windows.Forms.MouseButtons.Right)
        //    {
        //        ContextMenu menu = (ContextMenu)this.FindResource("NotifierContextMenu");

        //        menu.IsOpen = true;
        //    }
        //}

        //private void Menu_Open(object sender, RoutedEventArgs e)
        //{
        //    this.Show();
        //    this.Activate();

        //    this.WindowState = WindowState.Maximized;
        //}


        //private void Menu_Close(object sender, RoutedEventArgs e)
        //{
        //    if (MessageBox.Show("Do you really want to close Echo?", "Warning", MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
        //    {
        //        IconInstance.Dispose();

        //        Application.Current.Shutdown();
        //    }
        //}

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    if (MessageBox.Show("Do you really want to close Echo?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
        //    {
        //        e.Cancel = true;
        //    }
        //    base.OnClosing(e);
        //}

        private void View_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogViewer")
            {
                Show_LogTextblock(VM.LogViewer);
            }
            else if (e.PropertyName == "UILoadingAnimation") //for logo
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!AnimationTimer.IsEnabled)
                        AnimationTimer.Start();
                }));
            }
            else if (e.PropertyName == "ExcelLoaded")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (VM.ExcelLoaded)
                    {
                        LoadExcel_btn.ClearValue(Button.BackgroundProperty);
                        Ping_btn.IsEnabled = true;
                        Ping_btn.Content = "Pause System";
                        Send_btn.IsEnabled = true;
                        Reset_btn.IsEnabled = true;
                    }
                    else
                    {
                        Browse_Btn_Animation();
                    }
                }));
            }
            else if (e.PropertyName == "CreditStatus")
            {
                if (VM.CreditStatus_Today != 0)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        _WebReplyRun1.Text = "Balance in Teletalk account: " + DateTime.Now.ToString() + "\n";
                        _WebReplyRun2.Text = VM.CreditStatus;
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        _WebReplyRun1.Text = "Reply from Teletalk Server: " + DateTime.Now.ToString() + "\n";
                        _WebReplyRun2.Text = VM.CreditStatus;
                    }));
                }
            }
            else if (e.PropertyName == "NextSMSTime" && VM.SMSActive)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    NextSMSAlart_txtblk.Text = "Next SMS will be sent at: " + VM.NextSMSTime;
                }));
            }
            else if (e.PropertyName == "PingStatusText")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    PingStatus_txtblk.Text = "Ping status: " + VM.PingStatusText;
                    if (VM.PingStatusText == "Ping paused.")
                    {
                        Ping_btn.Content = "Start Ping";
                    }
                    else
                    {
                        Ping_btn.Content = "Pause System";
                    }
                }));
            }
            else if (e.PropertyName == "AccountStatusText")
            { 
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    AccTest_Txtblk.Text = VM.AccountStatusText;
                }));
            }
        } 


        DispatcherTimer AnimationTimer = new DispatcherTimer();  //for logo

        void DispatcherTimerLogoAnimation()
        {
            AnimationTimer.Interval = TimeSpan.FromMilliseconds(250);
            AnimationTimer.Tick += timer_TickLogoAnimation;
        }

        private void timer_TickLogoAnimation(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (VM.UILoadingAnimation == true)
                {
                    if (EchoLogo1.Visibility == Visibility.Visible)
                    {
                        EchoLogo1.Visibility = Visibility.Collapsed;
                        EchoLogo2.Visibility = Visibility.Visible;
                        EchoLogo3.Visibility = Visibility.Collapsed;
                    }
                    else if (EchoLogo2.Visibility == Visibility.Visible)
                    {
                        EchoLogo1.Visibility = Visibility.Collapsed;
                        EchoLogo2.Visibility = Visibility.Collapsed;
                        EchoLogo3.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        EchoLogo1.Visibility = Visibility.Visible;
                        EchoLogo2.Visibility = Visibility.Collapsed;
                        EchoLogo3.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    EchoLogo1.Visibility = Visibility.Collapsed;
                    EchoLogo2.Visibility = Visibility.Collapsed;
                    EchoLogo3.Visibility = Visibility.Collapsed;
                    AnimationTimer.Stop();
                }
            }));
        }

        private Object thisLock = new Object();

        void Show_LogTextblock(String str)
        {
            try
            {
                lock (thisLock)
                {

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        log_textblock.Text = log_textblock.Text + "# " + DateTime.Now.ToLongTimeString() + ":- " + str + "\n";
                        _scrollbar_log.ScrollToBottom();
                    }));
                }
            }
            catch (Exception ex)
            {
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }


        void DispatcherTimerClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        string currentTime = "";

        private void timer_Tick(object sender, EventArgs e)
        {
            currentTime = DateTime.Now.ToLongTimeString();

            Dispatcher.BeginInvoke((Action)(() =>
            {
                clock_textblock.Text = currentTime; //time showing
            }));

            
            if (currentTime == "12:00:00 AM") //###################################################CONSIDER ALWAYS########################################################
            {
                if (VM.CreditStatus_Yesterday - VM.CreditStatus_Today >= 0)
                    VM.CreditDeducted_Yesterday = VM.CreditStatus_Yesterday - VM.CreditStatus_Today;

                VM.CreditStatus_Yesterday = VM.CreditStatus_Today;

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    log_textblock.Text = "";
                }));
            }
        }

        string SearchCase = "";

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(Search_Textbox.Text))
                return true;
            else
                switch(SearchCase)
                {
                    case "Serial":
                        return ((item as Entity).Serial.ToString().IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "Name":
                        return ((item as Entity).Name.IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "IP":
                        return ((item as Entity).IpAddress.IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "Area":
                        return ((item as Entity).Area.IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "Type":
                        return ((item as Entity).Action_Type.IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "LastPing":
                        return ((item as Entity).LastPingStatus.IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "Loss":
                        return ((item as Entity).PercentageLoss.ToString().IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "Time":
                        return ((item as Entity).AverageRoundTripTime.ToString().IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    case "Status":
                        return ((item as Entity).Status.IndexOf(Search_Textbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                    default:
                        return true;
                }
        }

        private void RadioButton_Click_Serial(object sender, RoutedEventArgs e)
        {
            SearchCase = "Serial";
        }

        private void RadioButton_Click_Name(object sender, RoutedEventArgs e)
        {
            SearchCase = "Name";
        }

        private void RadioButton_Click_IP(object sender, RoutedEventArgs e)
        {
            SearchCase = "IP";
        }

        private void RadioButton_Click_Area(object sender, RoutedEventArgs e)
        {
            SearchCase = "Area";
        }


        private void RadioButton_Click_LastPing(object sender, RoutedEventArgs e)
        {
            SearchCase = "LastPing";
        }
        private void RadioButton_Click_Status(object sender, RoutedEventArgs e)
        {
            SearchCase = "Status";
        }

        private void RadioButton_Click_Type(object sender, RoutedEventArgs e)
        {
            SearchCase = "Type";
        }

        private void RadioButton_Click_Loss(object sender, RoutedEventArgs e)
        {
            SearchCase = "Loss";
        }

        private void RadioButton_Click_Time(object sender, RoutedEventArgs e)
        {
            SearchCase = "Time";
        }

        private void Search_Textbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(IPListView.ItemsSource).Refresh();
        }


        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;  

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(IPListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription();//(sortBy.ToString(), direction);  
            sd.PropertyName = sortBy;
            sd.Direction = direction;

            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;
            try
            {
                if (headerClicked != null)
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != _lastHeaderClicked)
                        {
                            direction = ListSortDirection.Ascending;
                        }
                        else
                        {
                            if (_lastDirection == ListSortDirection.Ascending)
                            {
                                direction = ListSortDirection.Descending;
                            }
                            else
                            {
                                direction = ListSortDirection.Ascending;
                            }
                        }

                        var sortBy = "";
                        if ((String)headerClicked.Column.Header == "Last Ping")
                        {
                            sortBy = "LastPingStatus";
                        }
                        else if ((String)headerClicked.Column.Header == "Pkt Loss (%)")
                        {
                            sortBy = "PercentageLoss";
                        }
                        else if ((String)headerClicked.Column.Header == "Action")
                        {
                            sortBy = "Action_Type";
                        }
                        else if ((String)headerClicked.Column.Header == "Status")
                        {
                            sortBy = "Status";
                        }
                        else 
                        {
                            var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                            sortBy = (string)((Binding)((GridViewColumnHeader)e.OriginalSource).Column.DisplayMemberBinding).Path.Path;
                            //var sortBy = headerClicked.Column.Header as string;
                        }



                        Sort(sortBy, direction);

                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
            catch (Exception ex)
            {
                Show_LogTextblock(ex.Message + " <" + ex.GetType().ToString() + ">");
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }

        private void Browse_Btn_Animation()
        {
            try
            {
                SolidColorBrush Browse_Btn_Brush = new SolidColorBrush();
                ColorAnimation colorAnimation = new ColorAnimation(Colors.Red, TimeSpan.FromMilliseconds(500));


                colorAnimation.RepeatBehavior = RepeatBehavior.Forever;
                colorAnimation.AutoReverse = true;
                Browse_Btn_Brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                LoadExcel_btn.Background = Browse_Btn_Brush;
            }
            catch (Exception ex)
            {
                Show_LogTextblock(ex.Message + " <" + ex.GetType().ToString() + ">");
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }

        private void SelectFile_function_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xls";
            dlg.Filter = "Excel Worksheets|*.xls;*.xlsx";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                VM.Destination_Excel_url = filename;
                Show_LogTextblock("File has been selected successfully. Path is " + VM.Destination_Excel_url);
                VM.Write_logFile("File has been selected successfully. Path is " + VM.Destination_Excel_url); 
            }
        }

        private void RadioButton_Click_ShowAll(object sender, RoutedEventArgs e)
        {
            VM.CheckAll = true;
            VM.CheckConnectedOnly = false;
            VM.CheckDisconnectedOnly = false;
        }

        private void RadioButton_Click_ConnectedOnly(object sender, RoutedEventArgs e)
        {
            VM.CheckAll = false;
            VM.CheckConnectedOnly = true;
            VM.CheckDisconnectedOnly = false;
        }

        private void RadioButton_Click_DisconnectedOnly(object sender, RoutedEventArgs e)
        {
            VM.CheckAll = false;
            VM.CheckConnectedOnly = false;
            VM.CheckDisconnectedOnly = true;
        }        


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Show_LogTextblock(ex.Message + " <" + ex.GetType().ToString() + ">");
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }

        private void Reset_MouseEnter_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = true;
            Popup_Common_textblock.Text = "Click here to reset app data.";
            timerforPopup.Interval = TimeSpan.FromSeconds(5);
            timerforPopup.Start();
        }

        private void Reset_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = false;
            timerforPopup.Stop();
        }

        DispatcherTimer timerforPopup = new DispatcherTimer();

        private void timer_TickForPopup(object sender, EventArgs e)
        {
            timerforPopup.Stop();
            AllPopupClose();
        }

        private void LoadBtn_MouseEnter_1(object sender, MouseEventArgs e)
        {
            if (VM.NodesList.Count == 0)
            {
                if (VM.Destination_Excel_url == "")
                    Popup_Common_textblock.Text = "Please browse an excel file for IP Address & phone number list.";
                else
                    Popup_Common_textblock.Text = "Please click here to load the Excel file";
            }
            else
                Popup_Common_textblock.Text = "Excel file path is " + VM.Destination_Excel_url;

            Popup_Common.IsOpen = true;
            timerforPopup.Interval = TimeSpan.FromSeconds(5);
            timerforPopup.Start();
        }


        private void LoadBtn_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = false;
            timerforPopup.Stop();
        }        


        private void LoadExcel_btn_Click(object sender, RoutedEventArgs e)
        {
            if (VM.Destination_Excel_url != "")
            {
                if (VM.SMSActive)
                {
                    if (this.user_name.Text == "" || this.acc_psw.Password == "")
                    {
                        if (MessageBox.Show("Please give user name & password for bulk sms, otherwise deactivate sms sending. Do you want to activate sms?", VM.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            this.Popup_Settings.IsOpen = true;
                        }
                    }
                    else
                    {
                        if (VM.RunPingFunctionality)
                        {
                            MessageBox.Show("Please at first pause ping function, then load excel.", VM.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            VM.ExcelLoaded = false;
                            Show_LogTextblock("Trying to load excel file ... .. .");
                            VM.LoadingThread();
                        }
                    }
                }
                else
                {
                    if (VM.RunPingFunctionality)
                    {
                        MessageBox.Show("Please at first pause ping function, then load excel.", VM.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        VM.ExcelLoaded = false;
                        Show_LogTextblock("Trying to load excel file ... .. .");
                        VM.LoadingThread();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please browse Excel file from File menu.", VM.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void Ping_btn_Click(object sender, RoutedEventArgs e)
        {
            Show_LogTextblock("Ping button clicked.");
            VM.Write_logFile("Ping button clicked.");
            if (VM.ExcelLoaded)
            {
                if (VM.RunPingFunctionality)
                {
                    VM.RunPingFunctionality = false;
                    Ping_btn.Content = "Start Ping";                    
                }
                else
                {
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        VM.RunPingFunctionality = true;
                        Ping_btn.Content = "Pause System";
                    }
                    else
                    {
                        MessageBox.Show("Network connection is unavailable. Please fix it.", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                Show_LogTextblock("Excel not loaded yet, please wait.....");
            }
           
        }


        private void PingBtn_MouseEnter_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = true;
            if (!VM.RunPingFunctionality)
            {
                Popup_Common_textblock.Text = "Click here to ping IP addresses now.";
            }
            else
            {
                Popup_Common_textblock.Text = "Click here if you want to pause pinging.";
            }
            timerforPopup.Interval = TimeSpan.FromSeconds(5);
            timerforPopup.Start();
        }

        private void PingBtn_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = false;
            timerforPopup.Stop();
        }


        private void AllPopupClose()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Popup_Common.IsOpen = false;
                //Popup_Edit.IsOpen = false;
            }));
        }

        private void Reset_btn_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to reset ping status?", VM.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Show_LogTextblock("Reset button clicked.");
                VM.Write_logFile("Reset button clicked.");
                VM.ResetStatus();
            }
        }


        
        private void Send_btn_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to send SMS manually?", VM.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Show_LogTextblock("Send button clicked.");
                VM.Write_logFile("Send button clicked.");
                if (VM.NodesList.Count > 0)
                {
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        VM.SMSThreadMethod();
                    }
                    else
                    {
                        MessageBox.Show("Network connection is unavailable. Please fix it.", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No Routers found for Ping Status.", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void SendBtn_MouseEnter_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = true;
            Popup_Common_textblock.Text = "Click here to send SMS now";
            timerforPopup.Interval = TimeSpan.FromSeconds(5);
            timerforPopup.Start();
        }

        private void SendBtn_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Common.IsOpen = false;
            timerforPopup.Stop();
        }




        private void Settings_function_Click_1(object sender, RoutedEventArgs e)
        {
            Popup_Settings.IsOpen = true;

        }

        private void acc_psw_PasswordChanged_1(object sender, RoutedEventArgs e)
        {
            VM.Password_String = acc_psw.Password;
        }

        private void user_name_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            VM.User_Name_String = user_name.Text;
        }

        private void Settings_OK_btn_Click_1(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Popup_Settings.IsOpen = false;
        }


        private void ParcentLoss_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (ParcentLoss_txtbox.Text == "")
                    VM.UpDownIndicator = 0;
                else
                    VM.UpDownIndicator = Convert.ToInt32(this.ParcentLoss_txtbox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " <" + ex.GetType().ToString() + ">", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }

        private void SMSInterval_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (SMSInterval_txtbox.Text == "")
                    VM.SMSInterval = 120;
                else
                    VM.SMSInterval = Convert.ToInt32(this.SMSInterval_txtbox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " <" + ex.GetType().ToString() + ">", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                VM.Write_logFile(ex.Message);
            }
        }

        private void RefreshInterval_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (RefreshInterval_txtbox.Text == "")
                    VM.RefreshPeriod = 10;
                else
                    VM.RefreshPeriod = Convert.ToInt32(this.RefreshInterval_txtbox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " <" + ex.GetType().ToString() + ">", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                VM.Write_logFile(ex.Message);
            }
        }

        private void PingSenseTime_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (PingSenseTime_txtbox.Text == "")
                    VM.PingSensePeriodForSMS = 5;
                else
                    VM.PingSensePeriodForSMS = Convert.ToInt32(this.PingSenseTime_txtbox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " <" + ex.GetType().ToString() + ">", VM.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                VM.Write_logFile(ex.Message);
            }
        }

        private void MsgHeader_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            VM.Message_Header = MsgHeader_txtbox.Text;
        }

        private void MsgFooter_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            VM.Message_Footer = MsgFooter_txtbox.Text;
        }

        private void AllLinksUp_txtbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            VM.AllLinksUpMessage = AllLinksUp_txtbox.Text;
        }

        private void SMS_Checkbox_Checked_1(object sender, RoutedEventArgs e)
        {
            VM.SMSActive = true;
            if (user_name != null)
            {
                user_name.IsEnabled = true;
                acc_psw.IsEnabled = true;
                SMSIfAllUp_Checkbox.IsEnabled = true;
                AllLinksUp_txtbox.IsEnabled = true;
                Show_LogTextblock("SMS is enabled.");
                NextSMSAlart_txtblk.Text = "Next SMS will be sent at: " + VM.NextSMSTime;
                WebReply_textblock.Visibility = Visibility.Visible;
                if(VM.ExcelLoaded)
                    Send_btn.IsEnabled = true;
            }
        }

        private void SMS_Checkbox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            VM.SMSActive = false;
            if (user_name != null)
            {
                user_name.IsEnabled = false;
                acc_psw.IsEnabled = false;
                SMSIfAllUp_Checkbox.IsEnabled = false;
                AllLinksUp_txtbox.IsEnabled = false;
                Show_LogTextblock("SMS is disabled. Only Pinging will continue.");
                NextSMSAlart_txtblk.Text = "SMS disabled.";
                Send_btn.IsEnabled = false;
                WebReply_textblock.Visibility = Visibility.Collapsed;
            }
        }

        private string Show_Instructions()
        {
            return
                "\n  1. Please at first browse excel file from file menu, if 'Load Excel' button blinks. There should be two sheets in excel file: A) 1st sheet will contain router or switch info. B) 2nd sheet will contain phone number list for sending message." +
                "\n  2. In first sheet of excel, there will be 4 columns: i)IP ii)Link name iii)Link type iv)Area. First row will be column headers." +
                "\n  3. In second sheet of excel, there will be phone numbers in first column. Again first row will be column headers. Phone numbers must be in 10 digits (starts with '1' not '0')." +
                "\n  4. After browsing the excel file, give username and password for Teletalk account in settings then click the 'Load Excel' button." +
                "\n  5. If you need to adjust SMS time interval, refresh interval and ping sense period, do it from settings." +
                "\n  6. You can add message header or footer from settings, but be careful about message size." +
                "\n  7. You can adjust minimum packet loss value (in percent) from settings, which will indicate the node is down." +
                "\n  8. SMS will be sent automatically after definite time interval, if you need to send message manually, then click on 'Send SMS' button." +
                "\n  9. If you need to pause ping, click on 'Pause Ping' button." +
                "\n  10. Router or switch status will be reset automatically. If you need to reset manually, then click 'Reset' button." +
                "\n  11. After any change in excel file, click on 'Load Excel' button. But before that you need to pause the ping functionality." +
                "\n  12. Next SMS time is shown at lower left corner of app." +
                "\n  13. Each log data will be saved to this directory:- C:\\Users\\Public\\" + VM.Title + " Log" +
                "\n  14. You can click on column name to sort by ascending or descending." +
                "\n  15. You can search any data from the list by writing any search entry at search box. At first select from 'Search by' by which you want to search." +
                "\n  16. Add text in 'All links up' message box in settings, which will be the message if all links are up." +
                "\n  17. You can disable SMS sytem from settings. It will disable SMS system only, but pinging will still be continuing." +
                "\n  18. You can stop sending SMS when all nodes are up by unchecking 'Send SMS even all nodes are up' at SMS Settings." +
                "\n  19. You can stop sending SMS for a particular link temporarily. To do this, double click on a link and disable its SMS.";
        }

        private void Instructions_MouseEnter_1(object sender, MouseEventArgs e)
        {
            _InstructRun1.Text = "Instructions of using this app:";
            _InstructRun2.Text = Show_Instructions();
            Popup_Instruct.IsOpen = true;            
        }

        private void Instructions_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Instruct.IsOpen = false;
        }


        private void SearchClear_Click_1(object sender, RoutedEventArgs e)
        {
            Search_Textbox.Text = "";
        }

        private void SMSIfAllUp_Checkbox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (!VM.SMSEvenAllUp)
            {
                VM.SMSEvenAllUp = true;
                AllLinksUp_txtbox.IsEnabled = true;
                Show_LogTextblock("SMS will be sent even all links are up.");
            }
        }


        private void SMSIfAllUp_Checkbox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            VM.SMSEvenAllUp = false;
            AllLinksUp_txtbox.IsEnabled = false;
            Show_LogTextblock("SMS will not be sent when all links are up.");
        }

        private void AccTest_btn_Click(object sender, RoutedEventArgs e)
        {
            VM.AccountStatusText = "Please wait......";
            VM.AccountTestTask();
        }
    }
}