using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    public class Encryptor
    {
        EncryptEngine engin;
        public byte[] IV;
        public Encryptor(string key)
        {
            engin = new EncryptEngine(key);
        }

        //---------------------------------------------------------------------
        public EncryptEngine EncryptEngine
        {
            get
            {
                return engin;
            }
            set
            {
                engin = value;
            }
        }

        //---------------------------------------------------------------------
        public string Encrypt(string MainString)
        {
            MemoryStream memory = new MemoryStream();
            CryptoStream stream = new CryptoStream(memory, engin.GetCryptTransform(), CryptoStreamMode.Write);
            StreamWriter streamwriter = new StreamWriter(stream);
            streamwriter.WriteLine(MainString);
            streamwriter.Close();
            stream.Close();
            IV = engin.Vector;
            byte[] buffer = memory.ToArray();
            memory.Close();
            return Convert.ToBase64String(buffer);

        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////
    public class EncryptEngine
    {
        bool bWithKey = false;
        string keyword = "";
        public byte[] Vector;
        public EncryptEngine(string Key)
        {
            if (Key.Length == 0)
                bWithKey = false;
            else
                bWithKey = true;

            keyword = Key;
        }

        //---------------------------------------------------------------------
        public ICryptoTransform GetCryptTransform()
        {
            byte[] key = Encoding.ASCII.GetBytes(keyword);

            Rijndael rj = new RijndaelManaged();
            rj.Mode = CipherMode.CBC;
            if (bWithKey) rj.Key = key;
            Vector = rj.IV;
            return rj.CreateEncryptor();
        }

        //---------------------------------------------------------------------
        public static bool ValidateKeySize(int Lenght)
        {
            Rijndael rj = new RijndaelManaged();
            return rj.ValidKeySize(Lenght);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////// 
    public class DecryptTransformer
    {
        string SecurityKey = "";
        Byte[] IV;
        bool bHasIV = false;

        //---------------------------------------------------------------------
        public DecryptTransformer(byte[] iv)
        {
            IV = iv;
            bHasIV = true;
        }

        //---------------------------------------------------------------------
        public void SetSecurityKey(string Key)
        {
            SecurityKey = Key;
        }
        //---------------------------------------------------------------------
        public ICryptoTransform GetCryptoTransform()
        {
            bool bHasSecuityKey = false;
            if (SecurityKey.Length != 0)
                bHasSecuityKey = true;

            byte[] key = Encoding.ASCII.GetBytes(SecurityKey);

            Rijndael rj = new RijndaelManaged();
            if (bHasSecuityKey) rj.Key = key;
            if (bHasIV) rj.IV = IV; ;
            return rj.CreateDecryptor();
        }

    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////// 
    public class Decryptor
    {
        byte[] IV;

        //---------------------------------------------------------------------
        public Decryptor(byte[] iv)
        {
            IV = iv;
        }

        //---------------------------------------------------------------------
        public string Decrypt(string MainString, string key)
        {
            DecryptTransformer dt = new DecryptTransformer(IV);
            dt.SetSecurityKey(key);

            byte[] buffer = Convert.FromBase64String(MainString.Trim());
            MemoryStream ms = new MemoryStream(buffer);

            // Create a CryptoStream using the memory stream and the 
            // CSP DES key. 
            CryptoStream encStream = new CryptoStream(ms, dt.GetCryptoTransform(), CryptoStreamMode.Read);

            // Create a StreamReader for reading the stream.
            StreamReader sr = new StreamReader(encStream);

            // Read the stream as a string.
            string val = sr.ReadLine();

            // Close the streams.
            sr.Close();
            encStream.Close();
            ms.Close();

            return val;
        }
    }

}
