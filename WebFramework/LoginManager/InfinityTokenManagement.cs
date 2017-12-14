using System;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.LoginManager
{
    //----------------------------------------------------------------------------
    public static class InfinityHelper
    {

        //----------------------------------------------------------------------------
        public static string GetRandomMMMENUID()
        {
            Random random = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(32);
            for (int i = 0; i < 32; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            return result.ToString();
        }

    }
    /// <summary>
    /// Classe per gestire il token criptato di infinity
    /// riesce a decriptarlo e criptarlo utilizzando direttamente le classi di zucchetti che sono contenute in un jar che è  wrappato da una dll c# grazie ad un tool (IKVM)
    /// la cui licenza pare open
    /// quindi includendo la dll creata e tutte le dll  del tool è possibile quindi utilizzare direttamente i metodi java.
    /// il token in arrivo viene decriptato e scisso nelle sue parti 
    /// il token in partenza viene criptato partendo dai dati conservati in memoria negli slot di login e formattandolo secondo le regole zucchetti.
    /// </summary>
    //----------------------------------------------------------------------------
    public class TokenInfinity
    {
        /*
		// Recupero il contenuto del token
		String[] v = getsTokenUncyph().split(SEPARATOR);
		// sSSOID := v[0] // Codice di autenticazione
		setsSSOID(v[0]);
		// dUSERAHR := Val(v[1]) // Codice utente AHR
		setdUSERAHR(CPLib.Round(CPLib.Val(v[1]), 0));
		// sAPPREG := v[2] // Codice applicazione registrata
		setsAPPREG(v[2]);
		// sIDATESYS := v[3] // Data di sistema
		setsIDATESYS(v[3]);
		// sTIMESTAMP := v[4] // Timestamp
		setsTIMESTAMP(v[4]);
		// sMATRICOLA := v[5] // Matricola
		setsAHR_MATRICOLA(v[5]);
		// sPARIVA := v[6] // Partita iva
		setsAHR_COIVA(v[6]);
		// sCODFIS := v[7] // Codice fiscale
		setsAHR_COFISCALCODE(v[7]);
		// sRAGSOC := v[8] // Ragione sociale
		setsAHR_COTITLE(v[8]);
        */
        public string ssoid;
        public string userCode;
        public string sysDate;
        public string timeStamp;
        public string appRegCode;
        public int companyToLogin;
        public string pIva;
        public string ragSoc;
        public string codFisc;
        public string matricola;
        private const string Separator = "|||";
        private const string CompanySeparator = "-_-";
        //----------------------------------------------------------------------------
        public TokenInfinity(string token)
        {
            if (!token.IsNullOrWhiteSpace() && token.Length > 17)
            {
                string initialChar = token.Substring(0, 1);
                string key = token.Substring(1, 16);
                string cryptedText = Replace(false, token.Substring(17));
                string decrypted = Microarea.InfoBusinessLicenseDomain.AesCryptingAlgorithm.DeCrypt(cryptedText, key);
               //decrypted = "ognuacrxikptxxvgftbrestpibvfuigogcnhujcu|||1|||MAGO-net-_-1|||20160531|||20160531162708|||123456|||01402000994|||01402000994|||MICROAREA SPA";
                string[] ss = decrypted.Split(new string[] { Separator }, StringSplitOptions.None);
                
                if (ss.Length == 9)
                {
                    ssoid = ss[0];
                    userCode = ss[1];
                    ParseAppReg(ss[2]);
                    sysDate = ss[3];
                    timeStamp = ss[4];
                    matricola = ss[5];
                    pIva = ss[6];
                    codFisc = ss[7];
                    ragSoc = ss[8];
                }
            }
        }

        //Mi aspetto che nella sezione appreg del token criptato ci sia anche espressa la company che l'utente ha già indicato nel wizard di configurazione, separata dalla vera e epropria appreg con uno specifico set di caratteri 
        //Questa funzione quindi parsa la stringa aspettandosi il separatore seguito da un numero, se non esiste separatore o se esso non è seguito da un numero tutto il contenuto si questa sezione è considerato appreg e non vien valorizzato il compnayId
        //tale companyid se valorizzato con un numero maggiore di -1 viene poi paragonato al valore di companyID che l'utente ha scelto in login, se diversi viene impedita la login e viene dato un errore.
        //----------------------------------------------------------------------------
        private void ParseAppReg(string v)
        {
            
            if (v.IsNullOrWhiteSpace())
            {
                companyToLogin = -1;
                appRegCode = null;
                return;
            }
            int index = v.LastIndexOf(CompanySeparator);
            if (index > 0)
            {
                string companyVal = v.Substring(index+ CompanySeparator.Length);
                if (!Int32.TryParse(companyVal, out companyToLogin))
                {
                    companyToLogin = -1;
                    appRegCode = v;
                }
                else
                appRegCode = v.Substring(0, index);
            }
            else
            {
                companyToLogin = -1;
                appRegCode = v;
            }
        }

        //----------------------------------------------------------------------------
        public TokenInfinity()
        {

        }

        //----------------------------------------------------------------------------
        public void FillTokenInfinity(string data, string ssoid)
        {
            if (data.IsNullOrWhiteSpace()) return;
            string[] ss = data.Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            if (ss == null || ss.Length != 6) return;
            userCode = ss[0];
            appRegCode = ss[1];
            matricola = ss[2];
            pIva = ss[3];
            codFisc = ss[4];
            ragSoc = ss[5];
            this.ssoid = ssoid;
        }

        /// <summary>
        /// metodo scopiazzato online per generare un key random di 16 caratteri
        /// </summary>
        /// <returns></returns>
        //----------------------------------------------------------------------------
        private string GetUniqueKey()
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            byte[] data = new byte[1];
            using (System.Security.Cryptography.RNGCryptoServiceProvider crypto = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[16];
                crypto.GetNonZeroBytes(data);
            }
            System.Text.StringBuilder result = new System.Text.StringBuilder(16);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        //----------------------------------------------------------------------------
        internal string GetCryptedToken()
        {
            if (string.IsNullOrWhiteSpace(ssoid))
                return null;
            string tokenPatternToCrypt = "{0}{9}{1}{9}{2}{9}{3}{9}{4}{9}{5}{9}{6}{9}{7}{9}{8}";
            string tokenPattern = "M{0}{1}";
            string key = GetUniqueKey();
            string crypted = Microarea.InfoBusinessLicenseDomain.AesCryptingAlgorithm.Crypt
                (String.Format(tokenPatternToCrypt, ssoid, userCode, appRegCode, GetSysDate(), GetNewTimeStamp(), "IMAGO", pIva, codFisc, ragSoc, Separator), key);

            return String.Format(tokenPattern, key, Replace(true, crypted));
        }

        //----------------------------------------------------------------------------
        internal string GetInfoForDb()
        {
            if (string.IsNullOrWhiteSpace(ssoid))
                return null;
            string tokenPatternToStore = "{0}{6}{1}{6}{2}{6}{3}{6}{4}{6}{5}";

            return String.Format(tokenPatternToStore, userCode, appRegCode, matricola, pIva, codFisc, ragSoc, Separator);
        }


        //----------------------------------------------------------------------------
        private string GetNewTimeStamp()
        {
            //"20160324095529" ore in formato 24??
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        //----------------------------------------------------------------------------
        private string GetSysDate()//sysdate cosa sarebbe? la data del sistema operativo o la data di applicazione?
        {
            //"20160324"
            return DateTime.Now.ToString("yyyyMMdd");
        }

        /// <summary>
        /// esegue il replace delle stringhe secondo le regole zucchetti precrypt\postdecrypt
        /// </summary>
        /// <param name="toCrypt">toCrypt se il token sta per essere criptato, altrimenti  se è stato decriptato</param>
        /// <returns></returns>
        //----------------------------------------------------------------------------
        private string Replace(bool toCrypt, string token)
        {
            if (toCrypt)//il token sta per essere criptato
            {
                token = token.Replace("+", "!");
                token = token.Replace("/", "$");
            }
            else//decrypto
            {
                token = token.Replace("!", "+");
                token = token.Replace("$", "/");
            }
            return token;
        }


        /*
Num.: 37/IM
Data apertura: 09-06-2016 17:42
Argomento: Richieste generiche di sviluppo (Cod.: INTMA0000000012)
Ciao Ilaria,
nella nostra implementazione il token è valido per 60 minuti, per entrambe le login: AHR e Infinity.
*/
        //----------------------------------------------------------------------------
        internal bool CheckTime()
        {
            //"20160531162708"
            DateTime tocheck = DateTime.Now;
            try
            {
                tocheck = DateTime.ParseExact(timeStamp, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { return false; }
            bool ok = tocheck.AddMinutes(60) >= DateTime.Now;
            if (ok) return true;
            System.Diagnostics.Debug.WriteLine("TOKEN INFINITY SCADUTO, ATTENZIONE! SEI IN DEBUG QUINDI NON VIENI BLOCCATO!");

#if DEBUG
            return true;
#else
            return false;
#endif
        }

        //----------------------------------------------------------------------------
        internal bool IsValid()
        {
            return !ssoid.IsNullOrWhiteSpace();
        }
    }
}