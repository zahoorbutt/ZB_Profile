using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace GlobalWebAppBAL
{
    public class GlobalAdditionalServices
    {
        private string _OrdNumber = string.Empty;
        private List<GlobalAddService> _Services = new List<GlobalAddService>();
        private List<GlobalAddService> _PickUPServices = new List<GlobalAddService>();
        private List<GlobalAddService> _StorageServices = new List<GlobalAddService>();
        private StringBuilder mErrorInfo;
        public GlobalAdditionalServices(string OrderNumber)
        {
            _OrdNumber = OrderNumber;
        }

        public List<GlobalAddService> GlobalAddServices { get { return _Services; } }

        public List<GlobalAddService> StorageGlobalAddServices { get { return _StorageServices; } }
        public List<GlobalAddService> PickupGlobalAddServices { get { return _PickUPServices; } }

        public void PopulatePickUPServices(string PickUpID)
        {
            Dictionary<string, GlobalAddService> lOrdServices = new Dictionary<string, GlobalAddService>();
            _PickUPServices.Clear();
            try
            {
                using (DataTable tbl = new DataTable())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "Sp_SelectAllServices";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                        {
                            using (SqlConnection con = new SqlConnection())
                            {
                                con.ConnectionString = Global.GlobalConnectionstring;
                                Cmd.Connection = con;
                                Cmd.Connection.Open();
                                dataAdp.SelectCommand = Cmd;
                                dataAdp.SelectCommand.Connection = con;
                                dataAdp.Fill(tbl);
                                con.Close();
                            }
                        }

                        foreach (DataRow rw in tbl.Rows)
                        {
                            if (!bool.Parse(rw["ForStorageFlag"].ToString())) continue;

                            GlobalAddService ser = new GlobalAddService();
                            ser.ApplyGST = bool.Parse(rw["GSTApplicable"].ToString());
                            ser.Assign = false;
                            ser.Charge = 0;
                            ser.Code = rw["Code"].ToString();
                            ser.DriverPercentage = 0;
                            ser.ForStorage = true;
                            ser.Name = rw["ServiceDescription"].ToString();
                            ser.ServiceID = rw["ServiceID"].ToString();
                            ser.PayToDriver = bool.Parse(rw["PayToDriver"].ToString());
                            lOrdServices.Add(ser.ServiceID, ser);

                        }
                    }
                }

                using (DataTable tbl = new DataTable())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "GetPickUPServices";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                        {
                            using (SqlConnection con = new SqlConnection())
                            {
                                con.ConnectionString = Global.GlobalConnectionstring;
                                Cmd.Connection = con;
                                Cmd.Connection.Open();
                                Cmd.Parameters.AddWithValue("@ordNum", _OrdNumber);
                                Cmd.Parameters.AddWithValue("@PickUpID", PickUpID);
                                dataAdp.SelectCommand = Cmd;
                                dataAdp.SelectCommand.Connection = con;
                                dataAdp.Fill(tbl);
                                con.Close();
                            }
                        }

                        foreach (DataRow rw in tbl.Rows)
                        {
                            lOrdServices[rw["ServiceID"].ToString()].ApplyGST = bool.Parse(rw["GSTApplicable"].ToString());
                            lOrdServices[rw["ServiceID"].ToString()].Assign = true;
                            if (!rw["ServiceCharge"].Equals(DBNull.Value) && Global.IsNumeric(rw["ServiceCharge"].ToString()))
                            {
                                lOrdServices[rw["ServiceID"].ToString()].Charge = decimal.Parse(rw["ServiceCharge"].ToString());
                            }
                        }
                    }
                }

                foreach (GlobalAddService en in lOrdServices.Values)
                {
                    this._PickUPServices.Add(en);
                }

            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(this.GetType().Name + "PolulateServices(); ");
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;

            }
        }

        public void PopulateStorageServices()
        {
            Dictionary<string, GlobalAddService> lOrdServices = new Dictionary<string, GlobalAddService>();
            _StorageServices.Clear();
            try
            {
                using (DataTable tbl = new DataTable())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "Sp_SelectAllServices";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                        {
                            using (SqlConnection con = new SqlConnection())
                            {
                                con.ConnectionString = Global.GlobalConnectionstring;
                                Cmd.Connection = con;
                                Cmd.Connection.Open();
                                dataAdp.SelectCommand = Cmd;
                                dataAdp.SelectCommand.Connection = con;
                                dataAdp.Fill(tbl);
                                con.Close();
                            }
                        }

                        foreach (DataRow rw in tbl.Rows)
                        {
                            if (!bool.Parse(rw["ForStorageFlag"].ToString())) continue;

                            GlobalAddService ser = new GlobalAddService();
                            ser.ApplyGST = bool.Parse(rw["GSTApplicable"].ToString());
                            ser.Assign = false;
                            ser.Charge = 0;
                            ser.Code = rw["Code"].ToString();
                            ser.DriverPercentage = 0;
                            ser.ForStorage = true;
                            ser.Name = rw["ServiceDescription"].ToString();
                            ser.ServiceID = rw["ServiceID"].ToString();
                            ser.PayToDriver = bool.Parse(rw["PayToDriver"].ToString());
                            lOrdServices.Add(ser.ServiceID, ser);

                        }
                    }
                }

                using (DataTable tbl = new DataTable())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "GetStorageServices";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                        {
                            using (SqlConnection con = new SqlConnection())
                            {
                                con.ConnectionString = Global.GlobalConnectionstring;
                                Cmd.Connection = con;
                                Cmd.Connection.Open();
                                Cmd.Parameters.AddWithValue("@ordNum", _OrdNumber);
                                Cmd.Parameters.AddWithValue("@OnlyCurrent", 1);
                                dataAdp.SelectCommand = Cmd;
                                dataAdp.SelectCommand.Connection = con;
                                dataAdp.Fill(tbl);
                                con.Close();
                            }
                        }

                        foreach (DataRow rw in tbl.Rows)
                        {
                            lOrdServices[rw["ServiceID"].ToString()].ApplyGST = bool.Parse(rw["GSTApplicable"].ToString());
                            lOrdServices[rw["ServiceID"].ToString()].Assign = true;
                            if (!rw["ServiceCharge"].Equals(DBNull.Value) && Global.IsNumeric(rw["ServiceCharge"].ToString()))
                            {
                                lOrdServices[rw["ServiceID"].ToString()].Charge = decimal.Parse(rw["ServiceCharge"].ToString());
                            }
                        }
                    }
                }

                foreach (GlobalAddService en in lOrdServices.Values)
                {
                    this._StorageServices.Add(en);
                }

            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(this.GetType().Name + "PolulateServices(); ");
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;

            }

        }


        public void PopulateServices(bool ForReturnService)
        {
            Dictionary<string, GlobalAddService> lOrdServices = new Dictionary<string, GlobalAddService>();
            _Services.Clear();
            try
            {
                using (DataTable tbl = new DataTable())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "Sp_SelectAllServices";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                        {
                            using (SqlConnection con = new SqlConnection())
                            {
                                con.ConnectionString = Global.GlobalConnectionstring;
                                Cmd.Connection = con;
                                Cmd.Connection.Open();
                                dataAdp.SelectCommand = Cmd;
                                dataAdp.SelectCommand.Connection = con;
                                dataAdp.Fill(tbl);
                                con.Close();
                            }
                        }

                        foreach (DataRow rw in tbl.Rows)
                        {
                            GlobalAddService ser = new GlobalAddService();
                            ser.ApplyGST = bool.Parse(rw["GSTApplicable"].ToString());
                            ser.Assign = false;
                            ser.Charge = 0;
                            ser.Code = rw["Code"].ToString();
                            ser.DriverPercentage = 0;
                            ser.Name = rw["ServiceDescription"].ToString();
                            ser.ServiceID = rw["ServiceID"].ToString();
                            ser.PayToDriver = bool.Parse(rw["PayToDriver"].ToString());
                            lOrdServices.Add(ser.ServiceID, ser);

                        }
                    }
                }

                using (DataTable tbl = new DataTable())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "GetOrderAssignedServices";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                        {
                            using (SqlConnection con = new SqlConnection())
                            {
                                con.ConnectionString = Global.GlobalConnectionstring;
                                Cmd.Connection = con;
                                Cmd.Connection.Open();
                                Cmd.Parameters.AddWithValue("@ordNum", _OrdNumber);
                                Cmd.Parameters.AddWithValue("@ReturnServiceFlag", ForReturnService);
                                dataAdp.SelectCommand = Cmd;
                                dataAdp.SelectCommand.Connection = con;
                                dataAdp.Fill(tbl);
                                con.Close();
                            }
                        }

                        foreach (DataRow rw in tbl.Rows)
                        {
                            lOrdServices[rw["ServiceID"].ToString()].ApplyGST = bool.Parse(rw["GSTApplicable"].ToString());
                            lOrdServices[rw["ServiceID"].ToString()].PayToDriver = rw["PayToDriver"].Equals(DBNull.Value) ? false : (rw["PayToDriver"].ToString().Equals("1")) ? true : false;
                            lOrdServices[rw["ServiceID"].ToString()].Assign = true;
                            if (!rw["DriverPercentage"].Equals(DBNull.Value) && Global.IsNumeric(rw["DriverPercentage"].ToString()))
                            {
                                lOrdServices[rw["ServiceID"].ToString()].DriverPercentage = decimal.Parse(rw["DriverPercentage"].ToString());
                            }
                            if (!rw["ServiceCharge"].Equals(DBNull.Value) && Global.IsNumeric(rw["ServiceCharge"].ToString()))
                            {
                                lOrdServices[rw["ServiceID"].ToString()].Charge = decimal.Parse(rw["ServiceCharge"].ToString());
                            }
                        }
                    }
                }

                foreach (GlobalAddService en in lOrdServices.Values)
                {
                    this._Services.Add(en);
                }

            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(this.GetType().Name + "PolulateServices(); ");
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;

            }

        }


        public bool SaveStorageOrderServices(SqlTransaction tr, List<GlobalAddService> services, string OrdNumber, string userID)
        {
            bool bResult = true;
            string sql = "INSERT INTO [dbo].[StorageServices]([OrderNumber],[ServiceID],[ServiceDescription],[GSTApplicable],[ServiceCharge],[Code])";
            sql = sql + " values ({OrderNumber},'{ServiceID}','{ServiceDescription}',{GSTApplicable},{ServiceCharge},'{Code}')";
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandType = CommandType.Text;
                    Cmd.Connection = tr.Connection;
                    Cmd.Transaction = tr;

                    Cmd.CommandText = "Delete from StorageServices where OrderNumber = " + OrdNumber + " AND StorageInvoiceID is Null";
                    Cmd.ExecuteNonQuery();


                    foreach (GlobalAddService ser in services)
                    {
                        if (ser.Assign)
                        {
                            Cmd.CommandText = sql.Replace("{OrderNumber}", OrdNumber);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceID}", ser.ServiceID);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceDescription}", ser.Name);
                            Cmd.CommandText = Cmd.CommandText.Replace("{GSTApplicable}", (ser.ApplyGST) ? "1" : "0");
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceCharge}", ser.Charge.ToString());
                            Cmd.CommandText = Cmd.CommandText.Replace("{Code}", ser.Code);
                            Cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(this.GetType().Name + "SaveOrderServices(); ");
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;

            }
            return bResult;

        }

        public bool SaveStoragePicUpServices(SqlTransaction tr, List<GlobalAddService> services, string OrdNumber, string userID, string PickUpID)
        {
            bool bResult = true;
            string sql = "INSERT INTO [dbo].[StorageServices]([OrderNumber],[ServiceID],[ServiceDescription],[GSTApplicable],[ServiceCharge],[Code],[PickUpID])";
            sql = sql + " values ({OrderNumber},'{ServiceID}','{ServiceDescription}',{GSTApplicable},{ServiceCharge},'{Code}',{PickUpID})";
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandType = CommandType.Text;
                    Cmd.Connection = tr.Connection;
                    Cmd.Transaction = tr;

                    Cmd.CommandText = "Delete from StorageServices where OrderNumber = " + OrdNumber + " AND StorageInvoiceID is Null and PickUpID = " + PickUpID.ToString();
                    Cmd.ExecuteNonQuery();


                    foreach (GlobalAddService ser in services)
                    {
                        if (ser.Assign)
                        {
                            Cmd.CommandText = sql.Replace("{OrderNumber}", OrdNumber);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceID}", ser.ServiceID);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceDescription}", ser.Name);
                            Cmd.CommandText = Cmd.CommandText.Replace("{GSTApplicable}", (ser.ApplyGST) ? "1" : "0");
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceCharge}", ser.Charge.ToString());
                            Cmd.CommandText = Cmd.CommandText.Replace("{Code}", ser.Code);
                            Cmd.CommandText = Cmd.CommandText.Replace("{PickUpID}", PickUpID);
                            Cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(this.GetType().Name + "SaveOrderServices(); ");
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;

            }
            return bResult;

        }

        public bool SaveOrderServices(ref SqlTransaction tr, List<GlobalAddService> services, string OrdNumber, string userID, bool ForReturnService)
        {
            bool bResult = true;
            string sql = "INSERT INTO [OrdersServices]([OrderNumber],[ServiceID],[ServiceDescription],[GSTApplicable],[ServiceCharge],[CreatedBy],[Active],[DriverPercentage],[PayToDriver],[Code],[ReturnService])";
            sql = sql + " values ('{OrderNumber}','{ServiceID}','{ServiceDescription}',{GSTApplicable},{ServiceCharge},'{CreatedBy}',{Active},{DriverPercentage},{PayToDriver},'{Code}',{ReturnService})";
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandType = CommandType.Text;
                    Cmd.Connection = tr.Connection;
                    Cmd.Transaction = tr;
                    if (ForReturnService == false)
                    {
                        Cmd.CommandText = "Delete from OrdersServices where OrderNumber = '" + OrdNumber + "'";
                        Cmd.ExecuteNonQuery();
                    }

                    foreach (GlobalAddService ser in services)
                    {
                        if (ser.Assign)
                        {
                            Cmd.CommandText = sql.Replace("{OrderNumber}", OrdNumber);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceID}", ser.ServiceID);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceDescription}", ser.Name);
                            Cmd.CommandText = Cmd.CommandText.Replace("{GSTApplicable}", (ser.ApplyGST) ? "1" : "0");
                            Cmd.CommandText = Cmd.CommandText.Replace("{ServiceCharge}", ser.Charge.ToString());
                            Cmd.CommandText = Cmd.CommandText.Replace("{CreatedBy}", userID);
                            Cmd.CommandText = Cmd.CommandText.Replace("{Active}", "1");
                            Cmd.CommandText = Cmd.CommandText.Replace("{DriverPercentage}", ser.DriverPercentage.ToString());
                            Cmd.CommandText = Cmd.CommandText.Replace("{PayToDriver}", (ser.PayToDriver) ? "1" : "0");
                            Cmd.CommandText = Cmd.CommandText.Replace("{Code}", ser.Code);
                            Cmd.CommandText = Cmd.CommandText.Replace("{ReturnService}", (ser.ForReturnService) ? "1" : "0");
                            Cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(this.GetType().Name + "SaveOrderServices(); ");
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;

            }
            return bResult;

        }


    }

    public class GlobalAddService : ICloneable
    {
        private string _ServiceID = string.Empty;
        private string _Code = string.Empty;
        private string _Name = string.Empty;
        private bool _Assign = false;
        private bool _PayToDriver = false;
        private bool _ApplyGST = false;
        private decimal _Charge = 0;
        private decimal _DriverPercentage = 0;
        public bool ForReturnService = false;
        public bool ForStorage = false;

        public string ServiceID { get { return _ServiceID; } set { _ServiceID = value; } }
        public string Code { get { return _Code; } set { _Code = value; } }
        public string Name { get { return _Name; } set { _Name = value; } }

        public decimal Charge { get { return _Charge; } set { _Charge = value; } }
        public decimal DriverPercentage { get { return _DriverPercentage; } set { _DriverPercentage = value; } }

        public Boolean Assign { get { return _Assign; } set { _Assign = value; } }
        public Boolean PayToDriver { get { return _PayToDriver; } set { _PayToDriver = value; } }
        public Boolean ApplyGST { get { return _ApplyGST; } set { _ApplyGST = value; } }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }


        #endregion
    }
}
