using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Text;
using System.Threading;
using System.Timers;
using System.Diagnostics;

using Microarea.Library.TBTalkieCommon;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TBTalkieManager
{
    //=========================================================================
    public class Global : System.Web.HttpApplication
    {
        private const double usersTimerInterval = 210000.0;//3 minuti e mezzo.
        private const double messagesTimerInterval = 3600000.0;//1 ora.

        private static Hashtable usersList;
        private static Hashtable messageQueue;

        private static IAuthProvider authProv;

        private static string loginManagerURL;

        private BasePathFinder pathFinder = null;
        private System.Timers.Timer usersPurgerTimer = new System.Timers.Timer();
        private System.Timers.Timer messagesPurgerTimer = new System.Timers.Timer();

        //---------------------------------------------------------------------
        public static IAuthProvider AuthProv
        {
            get
            {
                if (authProv == null)
                   authProv = new DummyAuthenticationProvider();
                //authProv = new MagoAuthenticationProvider(loginManagerURL);

                return authProv;
            }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value", "authentication provider cannot be null");

				authProv = value;
			}
		}

        //---------------------------------------------------------------------
        public static string LoginManagerURL
        {
            get
            {
                return loginManagerURL;
            }
        }

		#region UsersList management

		//---------------------------------------------------------------------
		public static Hashtable UsersList 
        { 
            get 
            {
                if (usersList == null)
                {
                    usersList = new Hashtable();
                    usersList = Hashtable.Synchronized(usersList);
                }

                return usersList; 
            }
		}

		//---------------------------------------------------------------------
		public static void AddUserToUsersList(string userName, UserInfo newUser)
		{
			if (userName == null)
				throw new ArgumentNullException("userName", "Unable to add 'userName' because is null");

			if (userName.Length == 0)
				throw new ArgumentException("Unable to add 'userName' because is empty", "userName");

			if (newUser == null)
				throw new ArgumentNullException("newUser", "Unable to add 'newUser' because is null");

			IDictionary temp = UsersList;
			if (!temp.Contains(userName))
				temp.Add(userName, newUser.Clone());
			else
				temp[userName] = newUser.Clone();
		}

		//---------------------------------------------------------------------
		public static void RemoveUserFromUsersList(string userName)
		{
			if (userName == null)
				throw new ArgumentNullException("userName", "Unable to add 'userName' because is null");

			if (userName.Length == 0)
				throw new ArgumentException("Unable to add 'userName' because is empty", "userName");

			UsersList.Remove(userName);
		}

		//---------------------------------------------------------------------
		public static bool IsUserInUsersList(string userName)
		{
			if (userName == null)
				throw new ArgumentNullException("userName", "Unable to add 'userName' because is null");

			if (userName.Length == 0)
				throw new ArgumentException("Unable to add 'userName' because is empty", "userName");

			return UsersList.ContainsKey(userName);
		}

		//---------------------------------------------------------------------
		public static string UsersListToString()
		{
			StringBuilder strBuilder = new StringBuilder();

			foreach (object val in usersList.Values)
				strBuilder.Append(val.ToString());

			return strBuilder.ToString();
		}

		#endregion

		#region MessageQueue management

		//---------------------------------------------------------------------
		public static Hashtable MessageQueue
        {
            get
            {
                if (messageQueue == null)
                {
                    messageQueue = new Hashtable();
                    messageQueue = Hashtable.Synchronized(messageQueue);
                }

                return messageQueue;
            }
        }

		//---------------------------------------------------------------------
		public static void PumpMessageToQueue(string receiverName, string message)
		{
			if (receiverName == null)
				throw new ArgumentNullException("receiverName", "Unable to add 'receiverName' because is null");

			if (receiverName.Length == 0)
				throw new ArgumentException("Unable to add 'receiverName' because is empty", "receiverName");

			if (message == null)
				throw new ArgumentNullException("message", "Unable to add 'message' because is null");

			IDictionary temp = MessageQueue;
			if (temp.Contains(receiverName))
			{
				string tempMsgs = temp[receiverName] as string;
				temp[receiverName] = String.Concat(tempMsgs, message);
			}
			else
			{
				temp.Add(receiverName, message);
			}         
		}

		//---------------------------------------------------------------------
		public static string ConsumeMessageFromQueue(string receiverName)
		{
			if (receiverName == null)
				throw new ArgumentNullException("receiverName", "Unable to add 'receiverName' because is null");

			if (receiverName.Length == 0)
				throw new ArgumentException("Unable to add 'receiverName' because is empty", "receiverName");

			IDictionary temp = MessageQueue;
			string messages = string.Empty;

			if (temp.Contains(receiverName))
			{
				messages = temp[receiverName] as string;
				temp.Remove(receiverName);
			}

			return messages;
		}

		#endregion

        //---------------------------------------------------------------------
        protected void Application_Start(object sender, EventArgs e)
        {

            //Get from WebConfig AppSettings section Login Manager URL
            
            //loginManagerURL = System.Configuration.ConfigurationManager.AppSettings["LoginManagerURL"];

            pathFinder = BasePathFinder.BasePathFinderInstance;

            loginManagerURL = pathFinder.LoginManagerUrl;

            if (loginManagerURL == String.Empty)
                loginManagerURL = "IDE";
                      
            //TODO Diventato inutile?!?
			usersPurgerTimer.Interval = usersTimerInterval;
            usersPurgerTimer.Elapsed += new ElapsedEventHandler(UsersPurgerTimer_Elapsed);
            usersPurgerTimer.Start();

            /*messagesPurgerTimer.Interval = messagesTimerInterval;
            messagesPurgerTimer.Elapsed += new ElapsedEventHandler(MessagesPurgerTimer_Elapsed);
            messagesPurgerTimer.Start();*/

			ICollection<string> users = AuthProv.GetAllUsers();
			if (users != null )
				foreach (string user in users)
					AddUserToUsersList(user, new UserInfo(user, UserState.Offline, "", user));
        }

        //---------------------------------------------------------------------
        private void UsersPurgerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
			IList<UserInfo> toBeUpdated = new List<UserInfo>();
            string tempToken = null;
			UserInfo tempUserInfo = null;

            IEnumerator enumerator = usersList.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
				tempUserInfo = (UsersList[enumerator.Current] as UserInfo);
				tempToken = tempUserInfo.AuthToken;
                if (!authProv.IsTokenValid(tempToken))
					toBeUpdated.Add(tempUserInfo);
            }

			foreach (UserInfo userInfo in toBeUpdated)
			{
				userInfo.UserState = UserState.Offline;
				userInfo.AuthToken = string.Empty; 
			}
        }

        //---------------------------------------------------------------------
        private void MessagesPurgerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Thread messagesPurgerThread = new Thread(new ThreadStart(PurgeMessages));
            messagesPurgerThread.Priority = ThreadPriority.BelowNormal;
            messagesPurgerThread.Start();
        }

        //---------------------------------------------------------------------
        private void PurgeMessages()
        {
            IList<string> toBeDeletedUsers = new List<string>();

            foreach (string receiver in MessageQueue.Keys)
        		 if (!UsersList.ContainsValue(receiver))
                     toBeDeletedUsers.Add(receiver);

            foreach (string toBeDeletedUser in toBeDeletedUsers)
                MessageQueue.Remove(toBeDeletedUsers);
        }

        //---------------------------------------------------------------------
        protected void Application_End(object sender, EventArgs e)
        {
            usersPurgerTimer.Stop();
            usersPurgerTimer.Dispose();
            usersPurgerTimer = null;

            messagesPurgerTimer.Stop();
            messagesPurgerTimer.Dispose();
            messagesPurgerTimer = null;

            pathFinder = null;
            
        }
    }
}