using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Microarea.AdminServer.Services.Providers
{
    public class SecurityTokenSQLDataProvider : IDataProvider
    {
        string connectionString;

        //---------------------------------------------------------------------
        public SecurityTokenSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

        //---------------------------------------------------------------------
        public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

        //---------------------------------------------------------------------
        public void Load(IAdminModel iModel)
        {
            // ISecurityToken token;

            //try
            //{
            //    token = (ISecurityToken)iModel;
            //    using (SqlConnection connection = new SqlConnection(this.connectionString))
            //    {
            //        connection.Open();
            //        using (SqlCommand command = new SqlCommand(Consts.SelectISecurityTokenByAccountidAndType, connection))
            //        {
            //            command.Parameters.AddWithValue("@AccountID", token.AccountId);
            //            command.Parameters.AddWithValue("@type", token.TokenType);
            //            using (SqlDataReader dataReader = command.ExecuteReader())
            //            {
            //                while (dataReader.Read())
            //                {
            //                    token.ActivationToken = new Library.ActivationToken(dataReader["ActivationKey"] as string);
            //                    token.PurchaseId = dataReader["PurchaseId"] as string;
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return null;
            //}

            // return token;
            return;
        }

        //---------------------------------------------------------------------
        public bool Save(IAdminModel iModel)
        {
            //ISecurityToken token;

            //try
            //{
            //    token = (ISecurityToken)iModel;
            //    using (SqlConnection connection = new SqlConnection(this.connectionString))
            //    {
            //        connection.Open();

            //        bool existSubscription = false;

            //        using (SqlCommand command = new SqlCommand(Consts.ExistToken, connection))
            //        {
            //            command.Parameters.AddWithValue("@SubscriptionId", token.AccountId);
            //            command.Parameters.AddWithValue("@type", token.TokenType);
            //            existSubscription = (int)command.ExecuteScalar() > 0;
            //        }

            //        using (SqlCommand command = new SqlCommand())
            //        {
            //            command.Connection = connection;
            //            command.CommandText = existSubscription ? Consts.UpdateToken: Consts.InsertToken;

            //            command.Parameters.AddWithValue("@accountid", token.AccountId );
                      
            //            if (existSubscription)
            //                command.Parameters.AddWithValue("@accountid", token.AccountId);

            //            command.ExecuteNonQuery();
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return false;
            //}

            return true;
        }

        //---------------------------------------------------------------------
        public bool Delete(IAdminModel iModel)
        {
            //ISecurityToken token;

            //try
            //{
            //    token = (ISecurityToken)iModel;
            //    using (SqlConnection connection = new SqlConnection(this.connectionString))
            //    {
            //        connection.Open();
            //        using (SqlCommand command = new SqlCommand(Consts.DeleteToken, connection))
            //        {
            //            command.Parameters.AddWithValue("@SubscriptionId", token.SubscriptionId);
            //            command.ExecuteNonQuery();
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return false;
            //}

            return true;
        }
    }
}
