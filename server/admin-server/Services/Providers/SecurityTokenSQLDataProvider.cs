using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
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
        public IAdminModel Load(IAdminModel iModel)
        {
            ISecurityToken token= (ISecurityToken)iModel;

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.SelectSecurityToken, connection))
                    {
                        command.Parameters.AddWithValue("@AccountName", token.AccountName);
                        command.Parameters.AddWithValue("@TokenType", token.TokenType);
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                token.Token = dataReader["Token"] as string;
                                token.ExpirationDate = (DateTime)dataReader["ExpirationDate"] ;
                                token.Expired = (bool)dataReader["Expired"];
                                token.ExistsOnDB = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return token;
            }

            return token;
           
        }

        //---------------------------------------------------------------------
        public OperationResult Save(IAdminModel iModel)
        {
            ISecurityToken token;
            OperationResult opRes = new OperationResult();
            opRes.Result = false;
            try
            {
                token = (ISecurityToken)iModel;
                if (String.IsNullOrEmpty(token.AccountName) || String.IsNullOrEmpty(token.Token))
                {
                    // Se il token è empty non salvo e non do errore.
                    opRes.Result = true;
                    return opRes;
                }
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    bool existSecurityToken = false;

                    using (SqlCommand command = new SqlCommand(Consts.ExistSecurityToken, connection))
                    {
                        command.Parameters.AddWithValue("@AccountName", token.AccountName);
                        command.Parameters.AddWithValue("@TokenType", token.TokenType);
                        existSecurityToken = (int)command.ExecuteScalar() > 0;
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = existSecurityToken ? Consts.UpdateSecurityToken : Consts.InsertSecurityToken;

                        command.Parameters.AddWithValue("@AccountName", token.AccountName);
                        command.Parameters.AddWithValue("@TokenType", token.TokenType);
                        command.Parameters.AddWithValue("@Token", token.Token);
                        command.Parameters.AddWithValue("@ExpirationDate", token.ExpirationDate);
                        command.Parameters.AddWithValue("@Expired", token.Expired);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               
                return opRes;
            }

            opRes.Result = true;
            return opRes;
        }

        //---------------------------------------------------------------------
        public bool Delete(IAdminModel iModel)
        {
            ISecurityToken token;

            try
            {
                token = (ISecurityToken)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.DeleteSecurityToken, connection))
                    {
                        command.Parameters.AddWithValue("@AccountName", token.AccountName);
                        command.Parameters.AddWithValue("@TokenType", token.TokenType);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();

			List<SecurityToken> tokensList = new List<SecurityToken>();

			string selectQuery = "SELECT * FROM MP_SecurityTokens WHERE ";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;

						foreach (QueryField field in qi.Fields)
						{
							string paramName = string.Format("@{0}", field.Name);
							selectQuery += string.Format("{0} = {1} AND ", paramName, field.Name);

							command.Parameters.AddWithValue(paramName, field.Value);
						}

						selectQuery = selectQuery.Substring(0, selectQuery.Length - 5);
						command.CommandText = selectQuery;

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								SecurityToken token = new SecurityToken();
								token.AccountName = dataReader["AccountName"] as string;
								token.Token = dataReader["Token"] as string;
								token.ExpirationDate = (DateTime)dataReader["ExpirationDate"];
								token.Expired = (bool)dataReader["Expired"];
								tokensList.Add(token);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = tokensList;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				opRes.Result = false;
				return opRes;
			}

			return opRes;
		}
	}
}
