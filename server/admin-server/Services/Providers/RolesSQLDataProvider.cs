using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Microarea.AdminServer.Services.Providers
{
    //================================================================================
    public class RolesSQLDataProvider : IDataProvider
    {
        string connectionString;

        //---------------------------------------------------------------------
        public RolesSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }
        //---------------------------------------------------------------------
        public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

        //---------------------------------------------------------------------
        public IAdminModel Load(IAdminModel iModel)
        {
            IRoles role;

            try
            {
                role = (IRoles)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Queries.SelectRole, connection))
                    {
                        command.Parameters.AddWithValue("@RoleId", role.RoleId);
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                role.Description = dataReader["Description"] as string;
                                role.Disabled= (bool)dataReader["Disabled"];
                                role.RoleName= dataReader["RoleName"] as string;
                                role.ExistsOnDB = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            return role;
        }

        //---------------------------------------------------------------------
        public bool Delete(IAdminModel iModel)
        {
            IRoles role;

            try
            {
                role = (IRoles)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Queries.DeleteRole, connection))
                    {
                        command.Parameters.AddWithValue("@RoleId", role.RoleId);
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

            List<IRoles> list = new List<IRoles>();

            string selectQuery = "SELECT * FROM MP_Roles WHERE ";

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
                                IRoles role = new Roles();
                                role.RoleName= dataReader["RoleName"] as string;
                                role.Description = dataReader["Description"] as string;
                                role.Disabled = (bool)dataReader["Disabled"];
                                role.RoleId = (int)dataReader["RoleId"];
                                list.Add(role);
                            }
                        }
                    }
                    opRes.Result = true;
                    opRes.Content = list;
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

        //---------------------------------------------------------------------
        public OperationResult Save(IAdminModel iModel)
        {
            IRoles role;
            OperationResult opRes = new OperationResult();

            try
            {
                role = (IRoles)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    bool existRole= false;

                    using (SqlCommand command = new SqlCommand(Queries.ExistRole, connection))
                    {
                        command.Parameters.AddWithValue("@RoleId", role.RoleId);
                        existRole = (int)command.ExecuteScalar() > 0;
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = existRole ? Queries.UpdateRole : Queries.InsertRole;

                        command.Parameters.AddWithValue("@Description", role.Description);
                        command.Parameters.AddWithValue("@RoleName", role.RoleName);
                        command.Parameters.AddWithValue("@Disabled", role.Disabled);
                        command.ExecuteNonQuery();
                    }

                    opRes.Result = true;
                    opRes.Content = role;
                }
            }
            catch (Exception e)
            {
                opRes.Result = false;
                opRes.Message = String.Concat("An error occurred while saving Role: ", e.Message);
                return opRes;
            }

            return opRes;
        }
    }
}
