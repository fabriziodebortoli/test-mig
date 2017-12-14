using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;

using System.ServiceModel;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.TaskBuilderNet.UI.InifnityMailer
{
    //=========================================================================
    public class InfinityMailer
    {
        private string infinityPOSTUrl = "/servlet/gsmg_bentrypoint_infinity"; // "http:/localhost:8080/iMago/servlet/gsmg_bentrypoint_infinity";
        private string infinityResponseUrl = "http://localhost:8080/iMago";
        private const string CRMInfinity = "CRMInfinity";
        private string infinityAutenticationToken = String.Empty;
        private string connectionString = String.Empty;

        //---------------------------------------------------------------------
        public InfinityMailer(string infinityToken, string companyConnection) 
        {
            infinityAutenticationToken = infinityToken;
            LoginManager loginMng = new LoginManager();
            connectionString = companyConnection;
            infinityResponseUrl = GeProviderUrlString();
            infinityPOSTUrl = infinityResponseUrl + infinityPOSTUrl;
        }

        //---------------------------------------------------------------------
        private string GeProviderUrlString()
        {
            SqlDataReader myDataReader = null;
            string end = @"/services/InfinitySyncro";
            try
            {
                string myQuery = @"SELECT * FROM DS_Providers
        	  WHERE Name= '" + CRMInfinity + "'";

                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                SqlCommand myCommand = new SqlCommand(myQuery, sqlConnection);
                myDataReader = myCommand.ExecuteReader();

                if (myDataReader == null)
                    return string.Empty;

                while (myDataReader.Read())
                {
                   
                    return myDataReader["ProviderUrl"].ToString().Substring(0, myDataReader["ProviderUrl"].ToString().Length - end.Length);
                }
            }
            catch (SqlException)
            {

            }
            finally
            {
                if (myDataReader != null && !myDataReader.IsClosed)
                {
                    myDataReader.Close();
                    myDataReader.Dispose();
                }
            }

            return myDataReader["ProviderUrl"].ToString().Substring(0, myDataReader["ProviderUrl"].ToString().Length - end.Length);
        }


        //---------------------------------------------------------------------
        public byte[] UploadMailAttachments(string[] strarrayAttach)
        {
            string zipFile = Core.NameSolver.BasePathFinder.BasePathFinderInstance.GetAppDataPath(true) + "/" + "result.zip";

            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress endPoint = new EndpointAddress(infinityResponseUrl + @"/services/InfinitySyncro"); // da database

            InfinityServices.InfinitySyncroClient client = new InfinityServices.InfinitySyncroClient(binding, endPoint);
            string contextId = client.connectRevInfinityWithToken(infinityAutenticationToken);

            try
            {
                if (File.Exists(zipFile))
                    File.Delete(zipFile);

                using (ZipFile zip = new ZipFile(zipFile))
                {
                    foreach (string attachmentPath in strarrayAttach)
                        zip.AddFile(attachmentPath, string.Empty); // Il 2o param è string.Empty per evitare che venga replicato il path del file nello zip

                    zip.Save(); // salvo il file zip
                    return File.ReadAllBytes(zipFile);
                    
                }
            }
            catch (Exception exx)
            {
                string a = exx.Message;
                return null;
            }

        }
        //---------------------------------------------------------------------
        public string UploadMailAttachmentsbyString(string[] strarrayAttach)
        {
            try
            {
                string zipFile = Core.NameSolver.BasePathFinder.BasePathFinderInstance.GetAppDataPath(true) + "/" + "result.zip";

                BasicHttpBinding binding = new BasicHttpBinding();
                EndpointAddress endPoint = new EndpointAddress(infinityResponseUrl + @"/services/InfinitySyncro"); // da database

                InfinityServices.InfinitySyncroClient client = new InfinityServices.InfinitySyncroClient(binding, endPoint);
                string contextId = client.connectRevInfinityWithToken(infinityAutenticationToken);

                byte [] rawBytes = UploadMailAttachments(strarrayAttach);
                return  client.uploadMailAttachments(contextId, rawBytes);
            }
            catch (Exception exx)
            {
                string a = exx.Message;
                return string.Empty;
            }
        }

        //---------------------------------------------------------------------
        public bool OpenInfinityMailer(string subject, string mailText, string sFrom, string[] sTo, string[] sCCN, string[] sCC, string[] strarrayAttach, bool deferrer)
        {

            // TOKEN		= token infiniti
            // RESOURCE	= 'Uri da lanciare - In caso di DMSEasy può valere : nuovo_documento - acquisizione_file - acquisizione_scanner - ricerca ; In caso di Mail può valere : nuova_mail
            // TEXT		    = Il testo della mail
            // SUBJECT		= Oggetto della mail

            // ATATTRIBID	= Lista codici attributi
            // CLASSEDOC	= Classe documetale
            // ATVALUE		= Lista valori attributi
            // TYPESEARCH	= Possibili valori : 0->Ricerca account aziendale, 1-> Ricerca account Aziendale PEC, 2->Ricerca account personale, 3-> Ricerca account personale PEC, 4->Ricerca per indirizzo email specificato in sMailSender
            // MAILSENDER	= Passare sempre vuoto tranne nel caso in cui iTypeSearch=4; In tal caso specificare la mail dell'account di invio

            // MAILTO		= Lista di indirizzi (separati da virgola) in TO
            // MAILCC		= Lista di indirizzi (separati da virgola) in CC
            // MAILCCN		= Lista di indirizzi (separati da virgola) in CCN
            // ISHTML		= Specifica se TEXT è html oppure testo normale (0->Normale, 1->html)
            // SAVEMAIL	= Specifica se salvare la mail nella cartella posta inviata (0->Non salva, 1->Salva)
            // ATTACHMENTS = Lista di attachments

            //SE ho il token vuoto allora redirect su login
            //TODO LARA
            //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(infinityPOSTUrl);
            //webRequest.AllowAutoRedirect = false;
            //webRequest.Method = "POST";
            //webRequest.ContentType = "application/x-www-form-urlencoded";

            //var data = Encoding.ASCII.GetBytes(@"SUBJECT=" + subject + "&TOKEN=" + infinityAutenticationToken + "&RESOURCE=nuova_mail&TEXT=" + mailText + "&MAILTO=" + sTo + "&MAILCC=" + sCC + "&MAILCCN" + sCCN + "&ATTACHMENTS=" + a);

            //using (var stream = webRequest.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}

            //HttpWebResponse webResponse;
            //using (webResponse = (HttpWebResponse)webRequest.GetResponse())
            //{
            //    if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
            //    {
            //        string uriString = webResponse.Headers["Location"];
            //        uriString = infinityResponseUrl + uriString.Substring(2);
            //        webResponse.Close();
            //        System.Diagnostics.Process.Start(uriString);
            //    }
            //}

            string to = string.Empty;
            for (int i = 0; i < sTo.Length; i++)
            {
                to = to + sTo[i] + "|N;";
            }

            string ccn = string.Empty;
            for (int i = 0; i < sCCN.Length; i++)
            {
               ccn = ccn + sCCN[i]+ "|N;" ;
            }
                

            string cc = string.Empty;
            for (int i = 0; i < sCC.Length; i++)
            {
                cc = cc + sCC[i] + "|N;";
            }

            if (deferrer)
            {
                byte[] attach = null;
                if (strarrayAttach != null && strarrayAttach.Length > 0)
                    attach = UploadMailAttachments(strarrayAttach);

                BasicHttpBinding binding = new BasicHttpBinding();
                EndpointAddress endPoint = new EndpointAddress(infinityResponseUrl + @"/services/InfinitySyncro"); // da database

                InfinityServices.InfinitySyncroClient client = new InfinityServices.InfinitySyncroClient(binding, endPoint);
                string contextId = client.connectRevInfinityWithToken(infinityAutenticationToken);
                string result = client.sendMail(contextId, 2,string.Empty, mailText, subject, to, cc, ccn, 1, 0, attach); // x fatturazione differita
                return true;
            }

            string a = "";
            if (strarrayAttach != null && strarrayAttach.Length > 0)
                a = UploadMailAttachmentsbyString(strarrayAttach);

            string htmlFile = @"<html>
                                    <script type=""text/javascript"" src=""" +infinityResponseUrl+ @"/visualweb/mootools-core.js""></script>
                                    <script>window.addEvent('domready', function()
                                            {
                                                window.document.forms[0].submit();
                                            });
                                    </script>
                                    <body>
                                    <h1>Loading form mail.......<h1>
                                    <form action = """ + infinityResponseUrl + @"/servlet/gsmg_bentrypoint_infinity"" method=""POST"">
                                      <input type = ""hidden"" name=""TOKEN"" value=""" + infinityAutenticationToken + @""">
                                      <input type = ""hidden"" name=""RESOURCE"" value=""nuova_mail"">
                                      <input type = ""hidden"" name=""TYPESEARCH"" value=""2"">
                                      <input type = ""hidden"" name=""MAILSENDER"" value="""">
                                      <input type = ""hidden"" name=""SUBJECT"" value=""" + subject + @""">
                                      <input type = ""hidden"" name=""MAILTO"" value=""" + to + @""" >
                                      <input type = ""hidden"" name=""TEXT"" value=""" + mailText + @""">
                                      <input type = ""hidden"" name=""MAILCC"" value=""" + cc + @""">
                                      <input type = ""hidden"" name=""MAILCCN"" value=""" + ccn + @""">
                                      <input type = ""hidden"" name=""ISHTML"" value=""1"">
                                      <input type = ""hidden"" name=""ATTACHMENTS"" value=""" + a + @""">  
                                      <input type = ""submit"" value=""Submit"" style=""display:none;"">
                                    </form>
                                    </body>
                                    </html>";

            htmlFile = htmlFile.Replace("\r\n", " ");
            string path = Core.NameSolver.BasePathFinder.BasePathFinderInstance.GetAppDataPath(true) + @"\\result.htm";
            // This text is added only once to the file.
            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllText(path, htmlFile);
          

            try
            {
                string keyValue = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\http\shell\open\command", "", null) as string;
                if (string.IsNullOrEmpty(keyValue) == false)
                {
                    string browserPath = keyValue.Replace("%1", path);
                    System.Diagnostics.Process.Start(browserPath);
                    return true;
                }
            }
            catch { }

            // try open browser as default command
            try
            {
                System.Diagnostics.Process.Start(path); //browserPath, argUrl);
                return true;
            }
            catch { }

 
            return true;
        }
    }
}
