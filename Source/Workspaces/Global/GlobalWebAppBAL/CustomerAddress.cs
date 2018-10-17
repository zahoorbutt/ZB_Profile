using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace GlobalWebAppBAL
{
    public class CustomerAddress
    {
            public  Int64 mCustomerNum;
            public Int64 mAccNum;
            private StringBuilder mErrorInfo;
            public  String mName;
            public  Boolean mActive;
            public  DateTime mCreatedDateTime;
            public  DateTime mLastModifiedDateTime;
            public  Guid mCreatedBy;
            public  string mLastModifiedBy = "";
            public string mMailingLocID = "";
            public  string mStreetLocation = "";
            public  string mSuite = "";
            public  string mProvince = "";
            public  string mCity = "";
            public  string mCityRegion = "";
            public  string mPostalCode = "";
            public  string mCountry = "";
            public  string mPhone = "";
            public  string mFax="";
            public  string mCell = "";
            public  string mPhone2 = "";
            public  string mEmail1 = "";
            public  string mEmail2 = "";
            public  string mContact1Name = "";
            public  string mContact1Title = "";
            public  string mContact1Phone = "";
            
            public  string mContact2Name = "";
            public  string mContact2Title = "";
            public  string mContact2Phone = "";
        public Guid mloc;
            
            public CustomerAddress(string CustomerNum)
            {
                if (CustomerNum.Equals(Global.blankCuromerNum.ToString()))
                {
                    mMailingLocID = Guid.Empty.ToString();
                    mCountry = "CANADA";
                }
                else
                { 
                     DataTable tbl= Global.GetCustomerAddress(CustomerNum);
                     if(tbl != null && tbl.Rows.Count>0)
                     {
                            this.mActive = (bool)tbl.Rows[0]["Active"];
                            this.mCell = tbl.Rows[0]["Cell"].ToString();
                            this.mCity = tbl.Rows[0]["City"].ToString();
                            this.mCityRegion = tbl.Rows[0]["CityRegion"].ToString();
                            this.mContact1Name = tbl.Rows[0]["ContactName1"].ToString();
                            this.mContact1Phone = tbl.Rows[0]["Contact1Number"].ToString();
                            this.mContact1Title = tbl.Rows[0]["ContactTitle1"].ToString();
                            this.mContact2Name = tbl.Rows[0]["ContactName2"].ToString();
                            this.mContact2Phone = tbl.Rows[0]["Contact2Number"].ToString();
                            this.mContact2Title = tbl.Rows[0]["ContactTitle2"].ToString();
                            this.mCountry = tbl.Rows[0]["Country"].ToString();
                            this.mCustomerNum = long.Parse(tbl.Rows[0]["CustomerNum"].ToString());
                            this.mEmail1 = tbl.Rows[0]["Email1"].ToString();
                            this.mEmail2 = tbl.Rows[0]["Email2"].ToString();
                            this.mFax = tbl.Rows[0]["Fax"].ToString();
                            this.mName = tbl.Rows[0]["Name"].ToString();
                            this.mPhone = tbl.Rows[0]["Phone1"].ToString();
                            this.mPhone2 = tbl.Rows[0]["Phone2"].ToString();
                            this.mPostalCode = tbl.Rows[0]["PostalCode"].ToString();
                            this.mProvince = tbl.Rows[0]["Province"].ToString();
                            this.mStreetLocation = tbl.Rows[0]["StreetLocation"].ToString();
                            this.mSuite = tbl.Rows[0]["Suite"].ToString();
                            this.mMailingLocID = tbl.Rows[0]["MailingLocID"].ToString();
                             this.mAccNum = (Global.IsNumeric( tbl.Rows[0]["AccNum"].ToString()))?long.Parse(tbl.Rows[0]["AccNum"].ToString()):this.mCustomerNum;
                     }
                }
            }

            public bool Save()
            {
                bool bResult = true;
                try  
	            {
                    SetAddressOnly();
	            }
	            catch (Exception)
	            {
                    bResult = false;
	            }
                return bResult;
            }
        public long SaveCustomerAddressOnly()
        {
            long cID = 0;
            try
            {
                cID=SaveAddressOnly();
            }
            catch (Exception)
            {
                cID = 0;
            }
            return cID;
        }
        private bool SetAddressOnly()
            {
                bool bResult = true;
                try
                {
                    if (this.mMailingLocID.Equals(Guid.Empty.ToString())) this.mMailingLocID = Guid.NewGuid().ToString();
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandType = CommandType.StoredProcedure;
                        Cmd.CommandText = "SetAddressOnly";
                        Cmd.Parameters.AddWithValue("@Name", mName);
                        Cmd.Parameters.AddWithValue("@Active", mActive);
                        Cmd.Parameters.AddWithValue("@MailingLocID", mMailingLocID);
                        Cmd.Parameters.AddWithValue("@ContactName1", mContact1Name);
                        Cmd.Parameters.AddWithValue("@ContactName2", mContact2Name);
                        Cmd.Parameters.AddWithValue("@ContactTitle1", mContact1Title);
                        Cmd.Parameters.AddWithValue("@ContactTitle2", mContact2Title);
                        Cmd.Parameters.AddWithValue("@Contact1Number", mContact1Phone);
                        Cmd.Parameters.AddWithValue("@Contact2Number", mContact2Phone);
                        Cmd.Parameters.AddWithValue("@CityRegion", mCityRegion);
                        Cmd.Parameters.AddWithValue("@CustomerNum", mCustomerNum);
                        Cmd.Parameters.AddWithValue("@UserID", mLastModifiedBy);
                        Cmd.Parameters["@CustomerNum"].Direction = ParameterDirection.InputOutput;
                        using (SqlConnection cn = new SqlConnection())
                        {
                            cn.ConnectionString = Global.GlobalConnectionstring;
                            cn.Open();
                            Cmd.Connection = cn;
                            using (SqlTransaction tr = cn.BeginTransaction())
                            {
                                try
                                {
                                    Cmd.Transaction = tr;
                                    Cmd.ExecuteNonQuery();
                                    mCustomerNum = (Int64)Cmd.Parameters["@CustomerNum"].Value;
                                    //Session["ReceverNum"] = mCustomerNum.ToString();
                                    Cmd.Parameters.Clear();
                                    Cmd.CommandText = "SetLocation";
                                    Cmd.Parameters.AddWithValue("@StreetLocation", mStreetLocation);
                                    Cmd.Parameters.AddWithValue("@Active", mActive);
                                    Cmd.Parameters.AddWithValue("@LocationID", mMailingLocID.ToString());
                                    Cmd.Parameters.AddWithValue("@Suite", mSuite);
                                    Cmd.Parameters.AddWithValue("@City", mCity);
                                    Cmd.Parameters.AddWithValue("@Province", mProvince);
                                    Cmd.Parameters.AddWithValue("@Country", mCountry);
                                    Cmd.Parameters.AddWithValue("@Fax", mFax);
                                    Cmd.Parameters.AddWithValue("@Phone1", mPhone);
                                    Cmd.Parameters.AddWithValue("@Phone2", mPhone2);
                                    Cmd.Parameters.AddWithValue("@Cell", mCell);
                                    Cmd.Parameters.AddWithValue("@UserID", mLastModifiedBy);
                                    Cmd.Parameters.AddWithValue("@Email1", mEmail1);
                                    Cmd.Parameters.AddWithValue("@Email2", mEmail2);
                                    Cmd.Parameters.AddWithValue("@PostalCode", mPostalCode);
                                    Cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex2)
                                {
                                    tr.Rollback();
                                    throw ex2;
                                }
                                tr.Commit();
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
                    mErrorInfo.Append(mErrorInfo.Append("ASCGGLOBAL :SetAddressOnly(); "));
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

        private long SaveAddressOnly()
        {
            
            try
            {
                if (this.mMailingLocID.Equals(Guid.Empty.ToString())) this.mloc = Guid.NewGuid();
                mMailingLocID = mloc.ToString();
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandType = CommandType.StoredProcedure;
                    Cmd.CommandText = "SetAddressOnly";
                    Cmd.Parameters.AddWithValue("@Name", mName);
                    Cmd.Parameters.AddWithValue("@Active", mActive);
                    Cmd.Parameters.AddWithValue("@MailingLocID", mloc);
                    Cmd.Parameters.AddWithValue("@ContactName1", mContact1Name);
                    Cmd.Parameters.AddWithValue("@ContactName2", mContact2Name);
                    Cmd.Parameters.AddWithValue("@ContactTitle1", mContact1Title);
                    Cmd.Parameters.AddWithValue("@ContactTitle2", mContact2Title);
                    Cmd.Parameters.AddWithValue("@Contact1Number", mContact1Phone);
                    Cmd.Parameters.AddWithValue("@Contact2Number", mContact2Phone);
                    Cmd.Parameters.AddWithValue("@CityRegion", mCityRegion);
                    Cmd.Parameters.AddWithValue("@CustomerNum", mCustomerNum);
                    Cmd.Parameters.AddWithValue("@UserID", mLastModifiedBy);
                    Cmd.Parameters["@CustomerNum"].Direction = ParameterDirection.InputOutput;
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        Cmd.Connection = cn;
                        using (SqlTransaction tr = cn.BeginTransaction())
                        {
                            try
                            {
                                Cmd.Transaction = tr;
                                Cmd.ExecuteNonQuery();
                                mCustomerNum = (Int64)Cmd.Parameters["@CustomerNum"].Value;                                
                                Cmd.Parameters.Clear();
                                Cmd.CommandText = "SetLocation";
                                Cmd.Parameters.AddWithValue("@StreetLocation", mStreetLocation);
                                Cmd.Parameters.AddWithValue("@Active", mActive);
                                Cmd.Parameters.AddWithValue("@LocationID", mloc);
                                Cmd.Parameters.AddWithValue("@Suite", mSuite);
                                Cmd.Parameters.AddWithValue("@City", mCity);
                                Cmd.Parameters.AddWithValue("@Province", mProvince);
                                Cmd.Parameters.AddWithValue("@Country", mCountry);
                                Cmd.Parameters.AddWithValue("@Fax", mFax);
                                Cmd.Parameters.AddWithValue("@Phone1", mPhone);
                                Cmd.Parameters.AddWithValue("@Phone2", mPhone2);
                                Cmd.Parameters.AddWithValue("@Cell", mCell);
                                Cmd.Parameters.AddWithValue("@UserID", mLastModifiedBy);
                                Cmd.Parameters.AddWithValue("@Email1", mEmail1);
                                Cmd.Parameters.AddWithValue("@Email2", mEmail2);
                                Cmd.Parameters.AddWithValue("@PostalCode", mPostalCode);
                                Cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex2)
                            {
                                tr.Rollback();
                                throw ex2;
                            }
                            tr.Commit();
                        }

                    }
                }

            }
            catch (Exception ex1)
            {
                
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGGLOBAL :SetAddressOnly(); "));
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

            return mCustomerNum;
        }



    }
}
