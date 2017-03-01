using Microarea.Common.Generic;
namespace Microarea.AccountManager.Library
{
    public class LoginEngine
    {
        public string Crypt(string val)
        {
            return Crypto.Encrypt(val);

        }
        public string Decrypt(string val)
        {
            return Crypto.Decrypt(val);

        }
    }
}
