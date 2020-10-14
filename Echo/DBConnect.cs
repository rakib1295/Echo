using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;
//Add MySql Library
using MySql.Data.MySqlClient;

namespace Echo
{
    class DBConnect
    {
        private MySqlConnection connection;
        public string Host_Name;
        public string Database;
        public string UID;
        public string PASSWORD;

        public String Title = "";

        //Constructor
        public DBConnect()
        {            
        }


        //open connection to database
        private bool OpenConnection()
        {
            string connectionString;
            connectionString = "SERVER=" + Host_Name + ";" + "DATABASE=" + Database + ";" + "UID=" + UID + ";" + "PASSWORD=" + PASSWORD + ";";

            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to DB server. Check userid/password/DB name.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool CheckDBConnection()
        {
            if (this.OpenConnection() == true)
            {
                this.CloseConnection();
                return true;
            }
            else
                return false;
        }

        public int SearchinCurrentDownNodes(string IPaddress)
        {
            string query = "select count(*) from CurrentDownPoPs where IPAddress = '" + IPaddress + "'";

            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        public int InsertDownNodes(string IPaddress, string Name, string Area, string DownTime)
        {
            int count = -1;

            string query = "INSERT INTO CurrentDownPoPs (IPAddress, Name, Area, DownTime) VALUES('" + IPaddress + "', '" + 
                Name + "', '" + Area + "', '" + DownTime + "')";

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                try
                {
                    count = cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //close connection
                this.CloseConnection();
            }
            return count;
        }

        public int InsertUpNodes(string IPaddress, string Name, string Area, string downtime, string uptime,
                        string DownDuration_ddhhmm, string Totalhour, string min, string monthCycle, string dateCycle)
        {
            int count = -1;
            string query = "INSERT INTO PoP_Status " +
                "(IPAddress, Name, Area, DownTime, UpTime, DownDuration_ddhhmm, Down_TotalHour, Down_Min, Month_Cycle, Date_Cycle) " +
                "VALUES('" + IPaddress + "', '" + Name + "','" + Area + "', '" + downtime + "','" + uptime + "', '" 
                + DownDuration_ddhhmm + "', '" + Totalhour + "', '" + min + "', '" + monthCycle + "', '" + dateCycle + "')";

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                try
                {
                    count = cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //close connection
                this.CloseConnection();
            }
            return count;
        }

        //Update statement
        //public void Update()
        //{
        //    string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

        //    //Open connection
        //    if (this.OpenConnection() == true)
        //    {
        //        //create mysql command
        //        MySqlCommand cmd = new MySqlCommand();
        //        //Assign the query using CommandText
        //        cmd.CommandText = query;
        //        //Assign the connection using Connection
        //        cmd.Connection = connection;

        //        //Execute query
        //        cmd.ExecuteNonQuery();

        //        //close connection
        //        this.CloseConnection();
        //    }
        //}


        //Select statement
        public string SelectDownTimefromDownTable(string IPaddress)
        {
            string query = "select DownTime from CurrentDownPoPs where IPAddress = '" + IPaddress + "'";

            //Create a list to store the result
            string data = "";

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    data = dataReader["DownTime"].ToString();
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return data;
            }
            else
            {
                return data;
            }
        }

        public void DeletefromDownTable(string IPaddress)
        {
            string query = "DELETE FROM CurrentDownPoPs where IPAddress = '" + IPaddress + "'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public List<string> SelectDownNodes()
        {
            string query = "select IPAddress from CurrentDownPoPs";

            //Create a list to store the result
            List<string> list = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list.Add(dataReader["IPAddress"].ToString() + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Select statement
        //public List<string>[] Select()
        //{
        //    string query = "SELECT * FROM tableinfo";

        //    //Create a list to store the result
        //    List<string>[] list = new List<string>[3];
        //    list[0] = new List<string>();
        //    list[1] = new List<string>();
        //    list[2] = new List<string>();

        //    //Open connection
        //    if (this.OpenConnection() == true)
        //    {
        //        //Create Command
        //        MySqlCommand cmd = new MySqlCommand(query, connection);
        //        //Create a data reader and Execute the command
        //        MySqlDataReader dataReader = cmd.ExecuteReader();

        //        //Read the data and store them in the list
        //        while (dataReader.Read())
        //        {
        //            list[0].Add(dataReader["id"] + "");
        //            list[1].Add(dataReader["name"] + "");
        //            list[2].Add(dataReader["age"] + "");
        //        }

        //        //close Data Reader
        //        dataReader.Close();

        //        //close Connection
        //        this.CloseConnection();

        //        //return list to be displayed
        //        return list;
        //    }
        //    else
        //    {
        //        return list;
        //    }
        //}

        //Backup
        //public void Backup()
        //{
        //    try
        //    {
        //        DateTime Time = DateTime.Now;
        //        int year = Time.Year;
        //        int month = Time.Month;
        //        int day = Time.Day;
        //        int hour = Time.Hour;
        //        int minute = Time.Minute;
        //        int second = Time.Second;
        //        int millisecond = Time.Millisecond;

        //        //Save file to C:\ with the current date as a filename
        //        string path;
        //        path = "C:\\" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
        //        StreamWriter file = new StreamWriter(path);

                
        //        ProcessStartInfo psi = new ProcessStartInfo();
        //        psi.FileName = "mysqldump";
        //        psi.RedirectStandardInput = false;
        //        psi.RedirectStandardOutput = true;
        //        psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
        //        psi.UseShellExecute = false;

        //        Process process = Process.Start(psi);

        //        string output;
        //        output = process.StandardOutput.ReadToEnd();
        //        file.WriteLine(output);
        //        process.WaitForExit();
        //        file.Close();
        //        process.Close();
        //    }
        //    catch (IOException ex)
        //    {
        //        MessageBox.Show("Error , unable to backup!");
        //    }
        //}

        //Restore
        //public void Restore()
        //{
        //    try
        //    {
        //        //Read file from C:\
        //        string path;
        //        path = "C:\\MySqlBackup.sql";
        //        StreamReader file = new StreamReader(path);
        //        string input = file.ReadToEnd();
        //        file.Close();


        //        ProcessStartInfo psi = new ProcessStartInfo();
        //        psi.FileName = "mysql";
        //        psi.RedirectStandardInput = true;
        //        psi.RedirectStandardOutput = false;
        //        psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
        //        psi.UseShellExecute = false;

                
        //        Process process = Process.Start(psi);
        //        process.StandardInput.WriteLine(input);
        //        process.StandardInput.Close();
        //        process.WaitForExit();
        //        process.Close();
        //    }
        //    catch (IOException ex)
        //    {
        //        MessageBox.Show("Error , unable to Restore!");
        //    }
        //}
    }
}
