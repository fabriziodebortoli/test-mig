using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Core.MenuManagerLoader
{

    public class LoggerMenu : IProviderLogWriter
    {
        private static object _locker = "";
        private static IProviderLogWriter _instance;
        public static IProviderLogWriter Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new LoggerMenu();
                    return _instance;
                }
            }
        }

        protected LoggerMenu() { }

        public void WriteToLog(string companyName, string username, string exceptionMsg, string methodName, string extendedInfo = "")
        {
            int fileNumber = 0;

            string dirPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyLogPath(companyName), "ImagoMenu");

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string filePath = Path.Combine(dirPath, string.Format("{0}_{1}_({2}).txt", username, DateTime.Now.ToString("yyyy-MM-dd"), fileNumber));

            StreamWriter writer;

            lock (_locker)
            {
                writer = File.AppendText(filePath);

                // if the  file size is > than 3.5 MB  i'll create another log file 
                while (writer.BaseStream.Length >= 3500 * 1024)
                {
                    writer.Close();
                    fileNumber++;
                    filePath = Path.Combine(dirPath, string.Format("{0}_{1}_({2}).txt", username, DateTime.Now.ToString("yyyy-MM-dd"), fileNumber));
                    writer = File.AppendText(filePath);
                }

                writer.Write("\r\nLog Entry : ");
                writer.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                writer.WriteLine("  :");
                writer.WriteLine("  :{0}", methodName);
                if (!string.IsNullOrEmpty(extendedInfo))
                    writer.WriteLine("  :{0}", extendedInfo);
                writer.WriteLine("  :{0}", exceptionMsg);
                writer.WriteLine("-------------------------------");
                writer.Flush();
                writer.Close();
            }
        }

        public void WriteToLog(string message, Exception e)
        {
            WriteToLog("", "", $"{message}, Exception message: {e.Message}", "");
        }
    }

    public class MagoMenuParser : IDisposable
    {
        private XmlDocument _doc = null;
        private string _SQLConnection = string.Empty; // "Data Source={0}; Initial Catalog='{1}';User ID='{2}';Password='{3}'";
        private string _SQLConnectionString = string.Empty;
        //private string sqlInsert = "insert into MSD_IMagoMenu (MMMENUID, MMLSTUPD, MM__VOCE, MM_LEVEL, MMDIRECT, MMINFMOD, MMLVLKEY, MMELEMEN, MM_PROGR, MMINFPRO) values ('{0}', '{1}', '{2}', {3}, {4}, 1, '{5}',{6}, {7}, '{8}')";
        private string sqlInsert = $@"insert into MSD_IMagoMenu (MMMENUID, MMLSTUPD, MM__VOCE, MM_LEVEL, MMDIRECT, MMINFMOD, MMLVLKEY, MMELEMEN, MM_PROGR, MMINFPRO) 
                                        values (@mmmenuid, @mmlstupd, @mm__voce, @mm_level, @mmdirect, 1, @mm_levelkey,@mmelemen, @mm_progr, @mminfpro)";
        private string sqlDelete = "delete from MSD_IMagoMenu where MMMENUID='{0}' AND MMLSTUPD='{1}'";
        private string sqlSelect = "select count(*) from MSD_IMagoMenu where MMMENUID='{0}' AND MMLSTUPD='{1}'";
        private string sqlSelCompUsr = "select MMMENUID from MSD_CompanyLogins where LoginId={0} AND CompanyId={1}";
        private string magofunc = "function:openMago(\"{0}\", \"\",\"\",\"{1}\")";
        private string prefix = "001.";
        private char pad = '0';
        private int nIdx = 0;
        private string _mmenuid;
        private string _mmlstupid;
        //private string _logfile;
        private int _loginId;
        private int _CompanyId;
        SqlConnection connection = null;
        SqlCommand sqlRowInsert = null;
        //StreamWriter sw = null;
        SqlTransaction transaction = null;
        private string _companyname = string.Empty;
        private string _username = string.Empty;
        int nrows;
        private string _caller = string.Empty;

        //-----------------------------------------------------------------
        public MagoMenuParser(
                                string connection,
                                string username,
                                string companyname,
                                string caller
                               )
        {
            _SQLConnection = InstallationData.ServerConnectionInfo.SysDBConnectionString;
            // _SQLConnection = connection;

            string[] tk = _SQLConnection.Split(';');

            _SQLConnectionString += tk[0];

            try
            {
                for (int i = 1; i < tk.Length - 1; i++)
                {
                    string[] items = tk[i].Split('=');
                    if (items[0] != "Password")
                        _SQLConnectionString += ';' + tk[i];
                }
            }
            catch (Exception e)
            {
                LoggerMenu.Instance.WriteToLog("Parsing connection string", e);
                _SQLConnectionString = string.Empty;
            }

            //_logfile = logfilename;
            _username = username;
            _companyname = companyname;
            _caller = caller;
        }

        //-----------------------------------------------------------------
        public MagoMenuParser(
                                string connectstr,
                                string xmldoc,
                                string loginid,
                                string companyid,
                                string username,
                                string companyname,
                                string caller
                                )
        {

            _SQLConnection = InstallationData.ServerConnectionInfo.SysDBConnectionString;

            string[] tk = _SQLConnection.Split(';');

            _SQLConnectionString += tk[0];
            try
            {
                for (int i = 1; i < tk.Length - 1; i++)
                {
                    string[] items = tk[i].Split('=');
                    if (items[0] != "Password")
                        _SQLConnectionString += ';' + tk[i];
                }
            }
            catch (Exception e)
            {
                LoggerMenu.Instance.WriteToLog("Parsing connection string", e);
                _SQLConnectionString = string.Empty;
            }

            //_logfile = logfilename;

            _loginId = Int32.Parse(loginid);
            _CompanyId = Int32.Parse(companyid);

            _username = username;
            _companyname = companyname;
            _caller = caller;

            _doc = new XmlDocument();
            _doc.LoadXml(xmldoc);

        }

        //--------------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (sqlRowInsert != null)
                sqlRowInsert.Dispose();
            if (transaction != null)
                transaction.Dispose();
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
            //if (sw != null)
            //{
            //    sw.Flush();
            //    sw.Close();
            //    sw.Dispose();
            //}
        }

        //--------------------------------------------------------------------------------------------------------
        private bool RetrieveMMENUID(int LoginID, int CompanyId)
        {

            try
            {
                if (connection == null)
                {
                    connection = new SqlConnection(_SQLConnection);
                    connection.Open();
                }

                string SqlSelect = string.Format(sqlSelCompUsr, LoginID, CompanyId);
                SqlCommand sqlSel = new SqlCommand(SqlSelect, connection);
                SqlDataReader reader;
                reader = sqlSel.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        _mmenuid = (string)reader["MMMENUID"];
                        _mmlstupid = "";

                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _mmenuid = "";
                _mmlstupid = "";
                LoggerMenu.Instance.WriteToLog(_companyname, _username, ex.Message, "RetrieveMMENUID()");
                LoggerMenu.Instance.WriteToLog(_companyname, _username, _SQLConnectionString, "RetrieveMMENUID()");
                return false;
            }
            return true;
        }

        //-----------------------------------------------------------------
        public int ExistMenu(int LoginID, int CompanyId)
        {
            int rownum = -1;

            //if (sw == null)
            //    try
            //    {
            //        string dirPath = Path.GetDirectoryName(_logfile);
            //        if (!Directory.Exists(dirPath))
            //            Directory.CreateDirectory(dirPath);
            //        sw = File.AppendText(_logfile);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(ex.Message);
            //    }

            bool bOk = RetrieveMMENUID(LoginID, CompanyId);

            if (bOk)
            {
                try
                {
                    string SqlSelect = string.Format(sqlSelect, _mmenuid, _mmlstupid);
                    SqlCommand sqlSel = new SqlCommand(SqlSelect, connection);
                    rownum = (int)sqlSel.ExecuteScalar();

                    if (rownum > 0)
                        connection.Close();

                }
                catch (Exception ex)
                {
                    LoggerMenu.Instance.WriteToLog(_companyname, _username, ex.Message, "ExistMenu()");

                }
            }

            return rownum;
        }

        //-----------------------------------------------------------------
        public void menu_insert(XmlNodeList nodeList, string prefix, int lvl, ref int nIdx)
        {
            int mylvl = lvl + 1;
            int menu_idx = 1;
            string proc = "function:javascript(0)";
            int mm_direct = 1;
            int mm_elem = 0;
            string padded_prefix;

            foreach (XmlNode xn in nodeList)
            {
                if (xn.Name != "Title")
                {
                    if (xn.Name == "Menu" && xn.ChildNodes.Count == 0)
                        continue;

                    //string sql;

                    padded_prefix = menu_idx.ToString();
                    string str_lvlkey = prefix + '.' + padded_prefix.PadLeft(3, pad);
                    XmlAttributeCollection Attributes = xn.Attributes;
                    if (xn.Attributes["title"] == null)
                        return;
                    if (xn.Name == "Object")
                    {
                        if (xn.Attributes["objectType"].Value == "Report")
                            proc = string.Format(magofunc, xn.Attributes["target"].Value, "report");
                        else if (xn.Attributes["objectType"].Value == "Function")
                            proc = string.Format(magofunc, xn.Attributes["target"].Value, "function");
                        else
                            proc = string.Format(magofunc, xn.Attributes["target"].Value, "document");
                        mm_direct = 1;
                        mm_elem = 0;
                    }
                    else
                    {
                        mm_direct = 2;
                        mm_elem = 1;
                    }

                    //string escaped_cmd = xn.Attributes["title"].Value.Replace("'", "''");
                    string escaped_cmd = xn.Attributes["title"].Value;
                    if (xn.Name == "Object" && xn.Attributes["objectType"].Value == "Report")
                        escaped_cmd += " (Report)";
                    //sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, escaped_cmd, mylvl, mm_direct, str_lvlkey, mm_elem, nIdx, proc);

                    try
                    {
                        //                  private string sqlInsert = $@"insert into MSD_IMagoMenu (MMMENUID, MMLSTUPD, MM__VOCE, MM_LEVEL, MMDIRECT, MMINFMOD, MMLVLKEY, MMELEMEN, MM_PROGR, MMINFPRO) 
                        //                values (@mmmenuid, @mmlstupd, @mm__voce, @mm_level, @mmdirect, 1, @mm_levelkey,@mmelemen, @mm_progr, @mminfpro)";

                        sqlRowInsert = new SqlCommand(sqlInsert, connection, transaction);
                        sqlRowInsert.Parameters.AddWithValue("@mmmenuid", _mmenuid);
                        sqlRowInsert.Parameters.AddWithValue("@mmlstupd", _mmlstupid);
                        sqlRowInsert.Parameters.AddWithValue("@mm__voce", escaped_cmd);
                        sqlRowInsert.Parameters.AddWithValue("@mm_level", mylvl);
                        sqlRowInsert.Parameters.AddWithValue("@mmdirect", mm_direct);
                        sqlRowInsert.Parameters.AddWithValue("@mm_levelkey", str_lvlkey);
                        sqlRowInsert.Parameters.AddWithValue("@mmelemen", mm_elem);
                        sqlRowInsert.Parameters.AddWithValue("@mm_progr", nIdx);
                        sqlRowInsert.Parameters.AddWithValue("@mminfpro", proc);

                        nrows = sqlRowInsert.ExecuteNonQuery();
                        nIdx += 1;
                        menu_idx += 1;
                        if (xn.ChildNodes.Count > 0)
                            menu_insert(xn.ChildNodes, str_lvlkey, mylvl, ref nIdx);
                    }
                    catch (Exception ex)
                    {
                        LoggerMenu.Instance.WriteToLog(_companyname, _username, ex.Message, "menu_insert()");
                    }
                }
            }
        }
        //-----------------------------------------------------------------
        public bool Parse()
        {
            //if (sw == null)
            //    try
            //    {
            //        string dirPath = Path.GetDirectoryName(_logfile);
            //        if (!Directory.Exists(dirPath))
            //            Directory.CreateDirectory(dirPath);
            //        sw = File.AppendText(_logfile);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(ex.Message);
            //    }

            LoggerMenu.Instance.WriteToLog(_companyname, _username, "start Parse", "Parse()");

            //LoggerMenu.Instance.WriteToLog(_companyname, _username, "start Parse", _doc.InnerXml);

            RetrieveMMENUID(_loginId, _CompanyId);

            string SqlDelete = string.Format(sqlDelete, _mmenuid, _mmlstupid);
            SqlCommand sqlDel = new SqlCommand(SqlDelete, connection);
            nrows = sqlDel.ExecuteNonQuery();

            transaction = connection.BeginTransaction();

            try
            {

                XmlNodeList nodeList;
                XmlElement root = _doc.DocumentElement;

                string initprefix = "001";

                //string sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, "Menu Principale", 1, 1, initprefix, 0, nIdx, "function:javascript(0)");
                sqlRowInsert = new SqlCommand(sqlInsert, connection, transaction);
                sqlRowInsert.Parameters.AddWithValue("@mmmenuid", _mmenuid);
                sqlRowInsert.Parameters.AddWithValue("@mmlstupd", _mmlstupid);
                sqlRowInsert.Parameters.AddWithValue("@mm__voce", "Menu Principale");
                sqlRowInsert.Parameters.AddWithValue("@mm_level", 1);
                sqlRowInsert.Parameters.AddWithValue("@mmdirect", 1);
                sqlRowInsert.Parameters.AddWithValue("@mm_levelkey", initprefix);
                sqlRowInsert.Parameters.AddWithValue("@mmelemen", 0);
                sqlRowInsert.Parameters.AddWithValue("@mm_progr", nIdx);
                sqlRowInsert.Parameters.AddWithValue("@mminfpro", "function:javascript(0)");

                nrows = sqlRowInsert.ExecuteNonQuery();
                nIdx += 1;

                XmlNodeList nodeAppList = root.SelectNodes("/Root/ApplicationMenu/AppMenu/Application");
                int macro_menu_idx = 1;
                string macro_menu_prefix = string.Empty;
                int macro_level;
                string padded_prefix;
                string lvl_prefix;

                foreach (XmlNode xnApp in nodeAppList)
                {
                    if (xnApp.Attributes["name"].Value != "ERP")
                    {
                        padded_prefix = macro_menu_idx.ToString();
                        macro_menu_prefix = prefix + padded_prefix.PadLeft(3, pad);
                        // sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xnApp.Attributes["name"].Value, 2, 2, macro_menu_prefix, 1, nIdx, "function:javascript(0)");
                        sqlRowInsert = new SqlCommand(sqlInsert, connection, transaction);
                        sqlRowInsert.Parameters.AddWithValue("@mmmenuid", _mmenuid);
                        sqlRowInsert.Parameters.AddWithValue("@mmlstupd", _mmlstupid);
                        sqlRowInsert.Parameters.AddWithValue("@mm__voce", xnApp.Attributes["name"].Value);
                        sqlRowInsert.Parameters.AddWithValue("@mm_level", 2);
                        sqlRowInsert.Parameters.AddWithValue("@mmdirect", 2);
                        sqlRowInsert.Parameters.AddWithValue("@mm_levelkey", macro_menu_prefix);
                        sqlRowInsert.Parameters.AddWithValue("@mmelemen", 1);
                        sqlRowInsert.Parameters.AddWithValue("@mm_progr", nIdx);
                        sqlRowInsert.Parameters.AddWithValue("@mminfpro", "function:javascript(0)");

                        nrows = sqlRowInsert.ExecuteNonQuery();
                        nIdx += 1;
                    }

                    nodeList = root.SelectNodes("/Root/ApplicationMenu/AppMenu/Application[@name='" + xnApp.Attributes["name"].Value + "']/Group");

                    if (xnApp.Attributes["name"].Value != "ERP")
                        lvl_prefix = macro_menu_prefix + ".";
                    else
                        lvl_prefix = prefix;
                    if (nodeList.Count == 0)
                    {
                        nIdx += 1;
                        macro_menu_idx += 1;
                    }
                    else
                    {
                        foreach (XmlNode xn in nodeList)
                        {
                            XmlAttributeCollection Attributes = xn.Attributes;
                            XmlNode title = xn.SelectSingleNode(".//Title");

                            macro_level = 2;

                            padded_prefix = macro_menu_idx.ToString();
                            macro_menu_prefix = lvl_prefix + padded_prefix.PadLeft(3, pad);
                            if (xn.ChildNodes.Count > 0)
                            {
                                // sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xn.Attributes["title"].Value, macro_level, 2, macro_menu_prefix, 1, nIdx, "");
                                sqlRowInsert = new SqlCommand(sqlInsert, connection, transaction);
                                sqlRowInsert.Parameters.AddWithValue("@mmmenuid", _mmenuid);
                                sqlRowInsert.Parameters.AddWithValue("@mmlstupd", _mmlstupid);
                                sqlRowInsert.Parameters.AddWithValue("@mm__voce", xn.Attributes["title"].Value);
                                sqlRowInsert.Parameters.AddWithValue("@mm_level", macro_level);
                                sqlRowInsert.Parameters.AddWithValue("@mmdirect", 2);
                                sqlRowInsert.Parameters.AddWithValue("@mm_levelkey", macro_menu_prefix);
                                sqlRowInsert.Parameters.AddWithValue("@mmelemen", 1);
                                sqlRowInsert.Parameters.AddWithValue("@mm_progr", nIdx);
                                sqlRowInsert.Parameters.AddWithValue("@mminfpro", "");

                                nrows = sqlRowInsert.ExecuteNonQuery();
                                menu_insert(xn.ChildNodes, macro_menu_prefix, macro_level, ref nIdx);
                                nIdx += 1;
                                macro_menu_idx += 1;
                            }
                        }
                    }
                }
                //nodeList = root.SelectNodes("/Root/ApplicationMenu/AppMenu/Application[@name='TBS']/Group");

                //foreach (XmlNode xn in nodeList)
                //{
                //    XmlAttributeCollection Attributes = xn.Attributes;
                //    XmlNode title = xn.SelectSingleNode(".//Title");
                //    macro_level = 2;
                //    padded_prefix = macro_menu_idx.ToString();
                //    macro_menu_prefix = prefix + padded_prefix.PadLeft(3, pad);
                //    if (xn.ChildNodes.Count > 0)
                //    {
                //        sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xn.Attributes["title"].Value, macro_level, 2, macro_menu_prefix, 1, nIdx, "");
                //        sqlRowInsert = new SqlCommand(sql, connection, transaction);
                //        nrows = sqlRowInsert.ExecuteNonQuery();
                //        menu_insert(xn.ChildNodes, macro_menu_prefix, macro_level, ref nIdx);
                //        nIdx += 1;
                //        macro_menu_idx += 1;
                //    }
                //}
                nodeList = root.SelectNodes("/Root/EnvironmentMenu/AppMenu/Application[@name='Framework']");
                if (nodeList.Count > 0)
                {
                    padded_prefix = macro_menu_idx.ToString();
                    macro_menu_prefix = prefix + padded_prefix.PadLeft(3, pad);
                    //sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, "TBF", 2, 2, macro_menu_prefix, 1, nIdx, "function:javascript(0)");
                    sqlRowInsert = new SqlCommand(sqlInsert, connection, transaction);
                    sqlRowInsert.Parameters.AddWithValue("@mmmenuid", _mmenuid);
                    sqlRowInsert.Parameters.AddWithValue("@mmlstupd", _mmlstupid);
                    sqlRowInsert.Parameters.AddWithValue("@mm__voce", "TBF");
                    sqlRowInsert.Parameters.AddWithValue("@mm_level", 2);
                    sqlRowInsert.Parameters.AddWithValue("@mmdirect", 2);
                    sqlRowInsert.Parameters.AddWithValue("@mm_levelkey", macro_menu_prefix);
                    sqlRowInsert.Parameters.AddWithValue("@mmelemen", 1);
                    sqlRowInsert.Parameters.AddWithValue("@mm_progr", nIdx);
                    sqlRowInsert.Parameters.AddWithValue("@mminfpro", "function:javascript(0)");

                    nrows = sqlRowInsert.ExecuteNonQuery();
                    nIdx += 1;
                }
                nodeList = root.SelectNodes("/Root/EnvironmentMenu/AppMenu/Application[@name='Framework']/Group");

                macro_menu_idx = 1;
                string tbf_prefix = macro_menu_prefix + ".";

                foreach (XmlNode xn in nodeList)
                {
                    XmlAttributeCollection Attributes = xn.Attributes;
                    XmlNode title = xn.SelectSingleNode(".//Title");
                    macro_level = 2;

                    padded_prefix = macro_menu_idx.ToString();
                    macro_menu_prefix = tbf_prefix + padded_prefix.PadLeft(3, pad);
                    if (xn.ChildNodes.Count > 0)
                    {
                        //sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xn.Attributes["title"].Value, macro_level, 2, macro_menu_prefix, 1, nIdx, "");
                        sqlRowInsert = new SqlCommand(sqlInsert, connection, transaction);
                        sqlRowInsert.Parameters.AddWithValue("@mmmenuid", _mmenuid);
                        sqlRowInsert.Parameters.AddWithValue("@mmlstupd", _mmlstupid);
                        sqlRowInsert.Parameters.AddWithValue("@mm__voce", xn.Attributes["title"].Value);
                        sqlRowInsert.Parameters.AddWithValue("@mm_level", macro_level);
                        sqlRowInsert.Parameters.AddWithValue("@mmdirect", 2);
                        sqlRowInsert.Parameters.AddWithValue("@mm_levelkey", macro_menu_prefix);
                        sqlRowInsert.Parameters.AddWithValue("@mmelemen", 1);
                        sqlRowInsert.Parameters.AddWithValue("@mm_progr", nIdx);
                        sqlRowInsert.Parameters.AddWithValue("@mminfpro", "");
                        nrows = sqlRowInsert.ExecuteNonQuery();
                        menu_insert(xn.ChildNodes, macro_menu_prefix, macro_level, ref nIdx);
                        nIdx += 1;
                        macro_menu_idx += 1;
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                LoggerMenu.Instance.WriteToLog(_companyname, _username, ex.Message, "Parse(): Exception");
                LoggerMenu.Instance.WriteToLog(_companyname, _username, "Parse(): Exception", _doc.InnerXml);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred
                    // on the server that would cause the rollback to fail, such as
                    // a closed connection.
                    LoggerMenu.Instance.WriteToLog(_companyname, _username, ex2.Message, "Parse(): Rollback");
                }
                connection.Close();
                return false;
            }
            connection.Close();
            LoggerMenu.Instance.WriteToLog(_companyname, _username, "End Parse", "Parse()");
            return true;
        }

    }
}
