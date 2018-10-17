using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace GlobalWebAppBAL
{
    public class GlobalUser
    {
        private bool mIsAutheticated;
        private DataTable DTUser = new DataTable();
        private DataTable DTUesrPermissions = new DataTable();
        private SqlDataAdapter dataAdp;
        private SqlCommand selCmd;
        SqlConnection con;
        private StringBuilder mErrorInfo;
        public GlobalUser(string pUserName, string pPWD)
        {
            try
            {
                con = new SqlConnection(Global.GlobalConnectionstring);
                selCmd = new SqlCommand();
                selCmd.CommandText = "SP_Login";
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@UserName";
                parm.Value = pUserName;
                selCmd.Parameters.Add(parm);
                parm = new SqlParameter();
                parm.ParameterName = "@PWD";
                parm.Value = pPWD;
                selCmd.Parameters.Add(parm);
                selCmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();
                selCmd.Connection = con;
                dataAdp = new SqlDataAdapter(selCmd);
                DataSet ds = new DataSet();
                dataAdp.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    mIsAutheticated = false;
                }
                else
                {
                    DTUser = ds.Tables[0];
                    DTUesrPermissions = ds.Tables[1];

                    string dbPasswordHash = DTUser.Rows[0]["Password"].ToString();
                    int size = 5;
                    string salt =
                             Global.CreateSalt(size);

                    string hashedPasswordAndSalt =
                                Global.CreatePasswordHash(pPWD, dbPasswordHash.Substring(dbPasswordHash.Length - salt.Length));
                    // Now verify them.
                    mIsAutheticated = hashedPasswordAndSalt.Equals(dbPasswordHash);



                }
            }
            catch (Exception ex1)
            {
                mIsAutheticated = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :ASCGUser(); "));
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
            finally
            {
                con.Close();
            }

        }

        public bool IsAuthenticated
        {
            get
            {
                return mIsAutheticated;
            }
        }

        public string Name
        {
            get { return DTUser.Rows[0]["Name"].ToString(); }
        }

        public Guid UserID
        {
            get { return (Guid)DTUser.Rows[0]["UserID"]; }
        }

        public bool MustChangePassword
        {
            get { return (bool)DTUser.Rows[0]["MustChangePassword"]; }
        }

        public DataTable UsersPrmissions
        {
            get
            {
                return this.DTUesrPermissions;
            }

        }

    }
}
