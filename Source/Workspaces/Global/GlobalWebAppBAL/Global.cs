using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Web.Security;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Net;

namespace GlobalWebAppBAL
{
    public enum RecordFetchFlags
    {
        All,
        Active,
        Delivered,
        Cancelled
    }

    public enum AdminMenu
    {
        Preschedule,
        Address,
        ProvinceList,
        CityList,
        PrePrintedWBList,
        AdditionalServices,
        Customers,
        PrintWB,
        Employee,
        SetGST,
        SKDRates,
        ServiceRates,
        TimeTrack,
        ZoneRates,
        PostalCodeZone,
        CityRegions,
        CustomersList,
        ReCreateRePrintInv,
        MakeBillPayments,
        SalesCommission,
        ShowBillPay,
        BillCenter,
        CreateInvoice,
        InvoicePayment,
        InvOperations,
        SrchDelOrder,
        SrchInv,
        SrchWB,
        SrchWBF
    }

    public struct fetchResults
    {
        public int RecCount;
        public bool Result;
        public string ErrMsg;
    }

    public class Pendingnvoice
    {
        private Int64 _InvoiceID;
        private string _InvoiceDate;
        private decimal _InvAmt;
        private decimal _InvGST;
        private decimal _InvBalance;
        private int _TTLPendings = 0;

        public Int64 InvoiceID { get { return _InvoiceID; } set { _InvoiceID = value; } }
        public string InvoiceDate { get { return _InvoiceDate; } set { _InvoiceDate = value; } }
        public decimal InvAmt { get { return _InvAmt; } set { _InvAmt = value; } }
        public decimal InvGST { get { return _InvGST; } set { _InvGST = value; } }
        public decimal InvBalance { get { return _InvBalance; } set { _InvBalance = value; } }

    }

    public struct DriversLocationsInfo
    {
        public Guid PickUpLocationID;
        public Guid DeliveryLocationID;
        public string DriverNum;
    }

    public struct stCustomerInvoiceInfo
    {
        public string FileName;
        public string CustomerName;
        public string OnlyName;
        public long CustomerNum;
        public long InvoiceID;
    }

    public enum CustomersIndicator
    {
        MFI,
        Global,
        Als
    }

    public enum DeliveryMenusTypes
    {
        Dispatched,
        PPOD,
        POD,
        ReRouted,
        StorageOrder
    }

    public enum DeliveryAction
    {
        Dispatched,
        PickedUp,
        PassOff,
        Delivery,
        RDispatched,
        RPickedUp,
        RPassOff,
        RDelivery,
        Undo,
        RUndo,
        NoPickUp,
        RNoPickUp,
        DisplayOrder,
        DriverPortion
    }

    public enum statusCode
    {
        New,
        Update,
        Delete
    }

    public enum SerachFlag
    {
        WayBill,
        Invoice
    }

    public enum PaymentTypes
    {
        ORDPER,//Order Percentage
        FUELSUR,//Fuel Surcharge
        ORDOTH //Order's Other
    }

    public enum InvoiceDueDaysTypes
    {
        COD = 0,
        Due7Days = 1,
        Due15Days = 2,
        Due30Days = 3
    }

    public enum PayRollMode
    {
        OwnerOperator = 0,
        HourlyRate = 1,
        WeekLy = 2,
    }

    public enum DeliveryStatusType
    {
        I, // In Process
        D, // Dispatched
        P, // POD or Pass-on-POD
        O  //On Board
    }

    public enum RateTerm
    {
        Weekly,
        BiWeekly,
        Monthly
    }

    public enum DeliverySerrviceType
    {
        ON,
        SMD,
        RSH,
        DIR,
        AFT,
        SP,
        NP,
        ON1, //O/N By 9:00AM
        ON2, //O/N By 10:30AM 
        ON3 //O/N By 12:00PM 

    }

    public enum OrderSteps
    {
        SelectOrderType,
        SelectShipper,
        SelectConsignee,
        SelectConatinerDetails,
        SelectServiceAndPayment,
        SelectContactInfo,
        SelectDriver
    }

    public enum OrderStatusType
    {
        IPR, // Inprocess
        WDA, //Waiting for DriverAssignment,
        WPK, //Waiting For PickUp
        WDP, //Waiting Dispatched
        WFD, //Waiting for Delivery,
        WFB, //Waiting for Billing,
        WFP, // Waiting for payment
        RPK, //Return Delivery PickedUp
        RDP, //Return Delivery Dispatched
        RDD, //Return Delivery Delivery
        FIN //Finished,

    }

    public enum OrderType
    {
        Delivery,
        Storage,
        DeliveryStorage
    }

    public enum DriverType
    {
        A, //Agent
        O, //Owner Operator
        D //Drivers (Employee)
    }
    public enum ServiceType
    {
        D, //Direct Services
        S, // Same Day Delivery
        R //Rush Delivery
    }

    public enum PaymentMethod
    {
        P, //PrePaid
        C, //Collection
        T //Thrid Party  
    }

    public enum UsersPermissions
    {
        CreateDevileryOrders,
        ModifyStorageOrders,
        ViewBillingOrders,
        ViewInvoices,
        ChangeBillingDeliveryOrders,
        AddUsers,
        ModifyBillingOrders,
        CreateStorageOrders,
        ModifyDeliveryOrders,
        CreateInvoices
    }

    public enum FormMode
    {
        New,
        View,
        ReadOnly,
        NewStorageAlso,
        DeliveryFromStorage

    }
    public enum SQLCmdType
    {
        cmdInsert,
        cmdUpdate,
        cmdDelete
    }

    public static class Global
    {

        public struct StMonths
        {
            public string MonthName;
            public int MonthValue;
        }

        public static bool MoveOrderToDB(string sOrdNumber, string usrID)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "MoveToDispatch";
                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = sOrdNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UserID";
                parm.Value = usrID;
                Cmd.Parameters.Add(parm);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:MoveOrderToDB(); "));
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
                Cmd = null;
            }
            return bresult;

        }


        public static bool CancelOrder(string sOrdNumber, string usrID)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "CancelOrder";
                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = sOrdNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UserID";
                parm.Value = usrID;
                Cmd.Parameters.Add(parm);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:CancelOrder(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static DataTable GetOtherDrivers(string sDriverNum)
        {
            DataTable tbl = new DataTable();
            try
            {
                string sql = "select DriverNum,DriverName from Drivers Where IsNull(Active,1) = 1 and DriverNum <> '" + sDriverNum + "'";

                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = GlobalConnectionstring;
                        cn.Open();
                        cmd.CommandText = sql;
                        cmd.Connection = cn;
                        cmd.CommandType = CommandType.Text;
                        using (SqlDataAdapter ad = new SqlDataAdapter())
                        {
                            ad.SelectCommand = cmd;
                            ad.Fill(tbl);
                        }
                    }
                }
                DataRow rw = tbl.NewRow();
                rw["DriverNum"] = "-99";
                rw["DriverName"] = "";
                tbl.Rows.InsertAt(rw, 0);
            }
            catch (Exception)
            {

            }
            return tbl;
        }

        public static bool VoidPayroll(string sCommNum, string sUserID, string sRemarks)
        {
            bool bResult = true;

            try
            {
                string sql = "Update [dbo].[OrderDrivers] set PAIDForOrder = 0 where OrderNumber in (select Distinct OrderNum from MFIAP Where PayrollID = ";
                sql = sql + "(select PayRollID from PayRollTrack Where CommNumber = " + sCommNum + "))";


                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = GlobalConnectionstring;
                        cn.Open();
                        using (SqlTransaction tr = cn.BeginTransaction())
                        {
                            try
                            {
                                cmd.CommandText = sql;
                                cmd.Connection = cn;
                                cmd.CommandType = CommandType.Text;
                                cmd.Transaction = tr;
                                cmd.ExecuteNonQuery();

                                sql = "INSERT INTO [dbo].[VoidPayRolls]([PayRollID],[CreatedBy],[Remarks]) Select PayRollID,'" + sUserID + "','" + sRemarks + "' from PayRollTrack Where CommNumber = " + sCommNum.ToString();
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "DELETE FROM [dbo].[OwnerOperatorPayRoll] WHERE PayrollID = (select PayRollID from PayRollTrack Where CommNumber = " + sCommNum.ToString() + ")";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "DELETE FROM [dbo].HOURLYPayRoll WHERE PayrollID = (select PayRollID from PayRollTrack Where CommNumber = " + sCommNum.ToString() + ")";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "DELETE FROM [dbo].DailyPayRoll WHERE PayrollID = (select PayRollID from PayRollTrack Where CommNumber = " + sCommNum.ToString() + ")";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "DELETE FROM [dbo].DailyPayRoll WHERE PayrollID = (select PayRollID from PayRollTrack Where CommNumber = " + sCommNum.ToString() + ")";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "DELETE FROM [dbo].PayRollTrack WHERE CommNumber = " + sCommNum.ToString();
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "DELETE FROM [dbo].[MFIAP]  WHERE PayrollID = (select PayRollID from PayRollTrack Where CommNumber = " + sCommNum.ToString() + ")";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                tr.Commit();
                            }
                            catch
                            {
                                tr.Rollback();
                                bResult = false;
                            }

                        }
                    }
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
            }
            return bResult;
        }

        public static bool TogglePendingAdjustmentInv(string sInvNum, string sUserID)
        {
            bool bResult = true;

            try
            {
                string sql = "Update [InvoiceAdjustments] Set IsPending = Case IsNull(IsPending,0) When 0 Then 1 Else 0 end Where [InvoiceID] = '" + sInvNum + "'";

                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = GlobalConnectionstring;
                        cn.Open();
                        try
                        {
                            cmd.CommandText = sql;
                            cmd.Connection = cn;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();

                        }
                        catch
                        {
                            bResult = false;
                        }
                    }
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
            }
            return bResult;
        }

        public static string GetMFIWBHTML(MFIOrder myOrder)
        {
            string htmlText = string.Empty;
            try
            {

                htmlText = new WebClient().DownloadString("MFIWB.htm");

                //Replace Order values
                htmlText = htmlText.Replace("{INVOICE No}", myOrder.normalOrder.ordDetails.InvoiceNumber);
                htmlText = htmlText.Replace("{Date}", myOrder.normalOrder.ShipperLoc.DeliveryDate.ToString("MM/dd/yy"));
                htmlText = htmlText.Replace("{Account}", myOrder.normalOrder.biller.CustomerNum.ToString());
                htmlText = htmlText.Replace("{CustomerRef}", myOrder.normalOrder.ordDetails.CustomerReferenceNum);
                htmlText = htmlText.Replace("{WayBill}", myOrder.normalOrder.ordDetails.WayBillNumber);
                htmlText = htmlText.Replace("{CARGO}", myOrder.normalOrder.ordDetails.CaroContainerNum);
                htmlText = htmlText.Replace("{AWBNUM}", myOrder.normalOrder.ordDetails.AWBNum);

                htmlText = htmlText.Replace("{BillTo}", myOrder.normalOrder.biller.Name);
                htmlText = htmlText.Replace("{OrderedBy}", myOrder.normalOrder.ordDetails.OrderedBy);

                htmlText = htmlText.Replace("{Service}", myOrder.normalOrder.ordDetails.ServiceType);

                int count = 1;
                decimal adchrg = 0;
                foreach (GlobalAddService ads in myOrder.normalOrder.ordDetails.AddServices)
                {
                    if (!ads.Assign) continue;
                    htmlText = htmlText.Replace("{ASN" + count.ToString() + "}", ads.Code);
                    htmlText = htmlText.Replace("{AS" + count.ToString() + "}", ads.Charge.ToString());
                    adchrg = adchrg + ads.Charge;
                    count = count + 1;
                }

                for (int k = count; k <= 11; k++)
                {
                    htmlText = htmlText.Replace("{ASN" + k.ToString() + "}", string.Empty);
                    htmlText = htmlText.Replace("{AS" + k.ToString() + "}", string.Empty);
                }
                if (string.IsNullOrEmpty(myOrder.normalOrder.ShipperLoc.Suite))
                {
                    htmlText = htmlText.Replace("{ShipperAddress}", myOrder.normalOrder.ShipperLoc.StreetLocation);
                }
                else
                {
                    htmlText = htmlText.Replace("{ShipperAddress}", myOrder.normalOrder.ShipperLoc.StreetLocation + "," + myOrder.normalOrder.ShipperLoc.Suite);
                }

                if (string.IsNullOrEmpty(myOrder.normalOrder.ConsigneeLoc.Suite))
                {
                    htmlText = htmlText.Replace("{ConsigneeAddress}", myOrder.normalOrder.ConsigneeLoc.StreetLocation);
                }
                else
                {
                    htmlText = htmlText.Replace("{ConsigneeAddress}", myOrder.normalOrder.ConsigneeLoc.StreetLocation + ", " + myOrder.normalOrder.ConsigneeLoc.Suite);
                }

                htmlText = htmlText.Replace("{ShipperCityState}", myOrder.normalOrder.ShipperLoc.City + ", " + myOrder.normalOrder.ShipperLoc.Province);

                htmlText = htmlText.Replace("{ConsigneeCityState}", myOrder.normalOrder.ConsigneeLoc.City + ", " + myOrder.normalOrder.ConsigneeLoc.Province);

                htmlText = htmlText.Replace("{Pcs}", myOrder.normalOrder.ordDetails.QTY.ToString());
                htmlText = htmlText.Replace("{SKID}", myOrder.normalOrder.ordDetails.SKID.ToString());
                htmlText = htmlText.Replace("{Weight}", myOrder.normalOrder.ordDetails.ActualWeight.ToString());

                myOrder.setDeliveryAndDrivers();
                decimal lordCost;
                decimal ordGST;
                decimal ordBasic;
                if (myOrder.normalOrder.orderCost.OverRideOrderFinalCost > 0)
                {
                    lordCost = myOrder.normalOrder.orderCost.OverRideOrderFinalCost;
                    ordGST = myOrder.normalOrder.orderCost.OverRideOrderGSTcharge;
                    ordBasic = myOrder.normalOrder.orderCost.OverrideOrderCost;
                }
                else
                {
                    lordCost = myOrder.normalOrder.orderCost.OrderFinalCost;
                    ordGST = myOrder.normalOrder.orderCost.OrderGST;
                    ordBasic = myOrder.normalOrder.orderCost.OrderBasicCost;
                }

                htmlText = htmlText.Replace("{DelvDate}", GetASCGDATE(myOrder.DeliveryDate).ToString("mm/dd/yy"));
                htmlText = htmlText.Replace("{PickUpDriver}", myOrder.PUDriverNum);
                htmlText = htmlText.Replace("{DelvDriver}", myOrder.DelDriverNum);
                htmlText = htmlText.Replace("{ASTTL}", adchrg.ToString());
                htmlText = htmlText.Replace("{ORDHST}", ordGST.ToString());
                htmlText = htmlText.Replace("{TOTAL}", (lordCost + adchrg).ToString());
                htmlText = htmlText.Replace("{Base}", ordBasic.ToString());


            }
            catch (Exception ex1)
            {

            }
            return htmlText;
        }

        public static string GetMFIWBHTML(string WB)
        {
            string OrdNum = GetOrderNumFromWayBillNumber(WB);
            MFIOrder myOrder = new MFIOrder(OrdNum);
            return GetMFIWBHTML(myOrder);
        }

        public static DataTable GetCustomerAndLocationByNameParts(string Name)
        {
            DataTable tbl = new DataTable();
            try
            {

                string sql = "select c.CustomerNum,c.Name,c.Active,c.CityRegion,l.StreetLocation,l.Suite,l.City,l.Province,";
                sql = sql + "l.Country,l.Phone1,l.Phone2,l.Fax,l.Cell,l.PostalCode,l.Email1,l.Email2,c.ContactName1,";
                sql = sql + "c.ContactTitle1,c.Contact1Number,c.ContactName2,c.ContactTitle2,c.Contact2Number ";
                sql = sql + "from Customer c inner join Locations l on l.LocationID = c.MailingLocID where ";
                sql = sql + " Name = '" + Name.Trim() + "'";

                string[] parts = Name.Split(' ');

                for (int j = 0; j < parts.Length; j++)
                {
                    sql = sql + " or Name like '" + parts[j] + "%'";
                    sql = sql + " or Name like '%" + parts[j] + "'";
                    sql = sql + " or Name like '%" + parts[j] + "%'";
                }


                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = GlobalConnectionstring;
                        cn.Open();
                        cmd.CommandText = sql;
                        cmd.Connection = cn;
                        cmd.CommandType = CommandType.Text;
                        using (SqlDataAdapter ad = new SqlDataAdapter())
                        {
                            ad.SelectCommand = cmd;
                            ad.Fill(tbl);
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
            return tbl;
        }

        public static DataTable GetCustomerAndLocationByStreetParts(string street)
        {
            DataTable tbl = new DataTable();
            try
            {

                string sql = "select c.CustomerNum,c.Name,c.Active,c.CityRegion,l.StreetLocation,l.Suite,l.City,l.Province,";
                sql = sql + "l.Country,l.Phone1,l.Phone2,l.Fax,l.Cell,l.PostalCode,l.Email1,l.Email2,c.ContactName1,";
                sql = sql + "c.ContactTitle1,c.Contact1Number,c.ContactName2,c.ContactTitle2,c.Contact2Number ";
                sql = sql + "from Customer c inner join Locations l on l.LocationID = c.MailingLocID where ";
                sql = sql + " l.StreetLocation = '" + street.Trim() + "'";

                string[] parts = street.Split(' ');

                for (int j = 0; j < parts.Length; j++)
                {
                    sql = sql + " or l.StreetLocation like '" + parts[j] + "%'";
                    sql = sql + " or l.StreetLocation like '%" + parts[j] + "'";
                    sql = sql + " or l.StreetLocation like '%" + parts[j] + "%'";
                }

                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = GlobalConnectionstring;
                        cn.Open();
                        cmd.CommandText = sql;
                        cmd.Connection = cn;
                        cmd.CommandType = CommandType.Text;
                        using (SqlDataAdapter ad = new SqlDataAdapter())
                        {
                            ad.SelectCommand = cmd;
                            ad.Fill(tbl);
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
            return tbl;
        }

        public static bool swapOrderDriver(string OrdNum, string driverNum, string NewdriverNum)
        {
            bool bResult = true;
            try
            {
                string sql = "update OrderDrivers set DriverNum = d.DriverNum,DriverType = d.DriverType, ";
                sql = sql + "DriverName = d.DriverName,[Rate%] = d.[Rate%],FuelPayOff = d.FuelPayOff ";
                sql = sql + "From Drivers d where d.DriverNum = " + NewdriverNum + " and OrderDrivers.DriverNum = ";
                sql = sql + driverNum + " and OrderDrivers.OrderNumber = '" + OrdNum + "'";

                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cn.ConnectionString = GlobalConnectionstring;
                        cn.Open();

                        using (SqlTransaction tr = cn.BeginTransaction())
                        {
                            try
                            {
                                cmd.Transaction = tr;
                                cmd.CommandText = sql;
                                cmd.Connection = cn;
                                cmd.CommandType = CommandType.Text;
                                cmd.ExecuteNonQuery();

                                sql = "update OrderPod Set HandOverDriverNum = " + NewdriverNum + " Where OrderNum = '" + OrdNum + "' And HandOverDriverNum = " + driverNum;
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                sql = "update OrderPod Set ReceiverDriverNum = " + NewdriverNum + " Where OrderNum = '" + OrdNum + "' And ReceiverDriverNum = " + driverNum;
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();

                                tr.Commit();
                            }
                            catch
                            {
                                tr.Rollback();
                                bResult = false;
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                bResult = false;
            }
            return bResult;
        }

        public static bool CancelWebOrder(string sOrdNumber, string usrID)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "CancelWebOrder";
                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = sOrdNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UserID";
                parm.Value = usrID;
                Cmd.Parameters.Add(parm);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:CancelWebOrder(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static bool GetPickedUpDateTime(string sOrdNumber, ref string PickUpDate, ref string PickUpTime)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            DataTable tbl = new DataTable();
            SqlDataAdapter adp = new SqlDataAdapter();
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetPickedUpDateTime";
                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = sOrdNumber;
                Cmd.Parameters.Add(parm);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                adp.SelectCommand = Cmd;
                adp.Fill(tbl);

                PickUpDate = tbl.Rows[0]["PickUpDeliveryDate"].ToString();
                PickUpTime = tbl.Rows[0]["PickUpDeliveryTime"].ToString();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPickedUpDateTime(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static bool GetDispatchedDateTime(string sOrdNumber, ref string DispatchedDateTime)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            DataTable tbl = new DataTable();
            SqlDataAdapter adp = new SqlDataAdapter();
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetDispatchedDateTime";
                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = sOrdNumber;
                Cmd.Parameters.Add(parm);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                adp.SelectCommand = Cmd;
                adp.Fill(tbl);

                DispatchedDateTime = tbl.Rows[0][0].ToString();

                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPickedUpDateTime(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static List<string> GetAllCityRegions(DataTable cityTable)
        {
            List<string> cityRegions = new List<string>();

            try
            {
                foreach (DataRow myRw in cityTable.Rows)
                {
                    if (!string.IsNullOrEmpty(myRw["From"].ToString()) && !cityRegions.Contains(myRw["From"].ToString()))
                    {
                        cityRegions.Add(myRw["From"].ToString());
                    }
                    if (!string.IsNullOrEmpty(myRw["To"].ToString()) && !cityRegions.Contains(myRw["To"].ToString()))
                    {
                        cityRegions.Add(myRw["To"].ToString());
                    }
                }
            }
            catch (Exception ex1)
            {


            }
            return cityRegions;

        }

        public static bool ReInstateOrder(string sOrdNumber, string usrID)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "ReInstateOrder";
                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = sOrdNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UserID";
                parm.Value = usrID;
                Cmd.Parameters.Add(parm);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ReInstateOrder(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static DataSet WayBillsByCustomers(string custNum, string fromDate, string toDate)
        {
            DataSet myInfo = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@CustNum";
                parm.Value = custNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = fromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = toDate;
                Cmd.Parameters.Add(parm);

                myInfo = new DataSet();
                con.Open();

                Cmd.CommandText = "GetCustomerWayBills";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:WayBillsByCustomers(); "));
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
                Cmd = null;
            }
            return myInfo;

        }

        public static bool UpdateCustomerInfo(string myInformation)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = myInformation;

                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                bresult = (Cmd.ExecuteNonQuery() > 0);

                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:UpdateCustomerInfo(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static DataSet GetClientInfo()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet MyClient = null;
            try
            {
                MyClient = new DataSet();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetClientInformation";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(MyClient);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetClientInfo(); "));
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
                Cmd = null;
            }
            return MyClient;

        }

        public static string GetCityRegions()
        {
            string sResult = string.Empty;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cmd.CommandText = "select CityRegions from Config";
                        cmd.CommandType = CommandType.Text;
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        cmd.Connection = cn;
                        using (DataTable tbl = new DataTable())
                        {
                            sResult = cmd.ExecuteScalar().ToString();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {


            }

            return sResult;
        }

        public static string GetPostalCodesZones()
        {
            string sResult = string.Empty;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cmd.CommandText = "select PostalCodesZones from Config";
                        cmd.CommandType = CommandType.Text;
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        cmd.Connection = cn;
                        using (DataTable tbl = new DataTable())
                        {
                            sResult = cmd.ExecuteScalar().ToString();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {


            }

            return sResult;

        }

        public static string GetZonesRates()
        {
            string sResult = string.Empty;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cmd.CommandText = "select ZoneRates from Config";
                        cmd.CommandType = CommandType.Text;
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        cmd.Connection = cn;
                        using (DataTable tbl = new DataTable())
                        {
                            sResult = cmd.ExecuteScalar().ToString();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {


            }

            return sResult;

        }

        public static bool UpdateZoneRates(string sXML)
        {
            bool bResult = true;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cmd.CommandText = "Update Config set ZoneRates = '" + sXML + "'";
                        cmd.CommandType = CommandType.Text;
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        cmd.Connection = cn;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
            }

            return bResult;
        }

        public static bool UpdateCityRegions(string sXML)
        {
            bool bResult = true;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cmd.CommandText = "Update Config set CityRegions = '" + sXML + "'";
                        cmd.CommandType = CommandType.Text;
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        cmd.Connection = cn;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
            }

            return bResult;
        }

        public static bool UpdatePostalCodesZones(string sXML)
        {
            bool bResult = true;
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    using (SqlConnection cn = new SqlConnection())
                    {
                        cmd.CommandText = "Update Config set PostalCodesZones = '" + sXML + "'";
                        cmd.CommandType = CommandType.Text;
                        cn.ConnectionString = Global.GlobalConnectionstring;
                        cn.Open();
                        cmd.Connection = cn;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
            }

            return bResult;
        }

        public static DataSet GetEmployeeHoursSummary(string FromDate, string ToDate, string EmployeeNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet MyClient = null;
            try
            {
                MyClient = new DataSet();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetEmployeeHoursSummary";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(MyClient);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetClientInfo(); "));
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
                Cmd = null;
            }
            return MyClient;

        }

        public static DataTable GetASCGClientInfo()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myInfo = null;
            try
            {
                myInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SELECT * FROM [dbo].[CustomerInfo]";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetASCGClientInfo(); "));
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
                Cmd = null;
            }
            return myInfo;
        }

        public static DataTable GetSalesReport(string FromDate, string ToDate)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myInfo = null;
            try
            {
                myInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "RPTSales";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = FromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = ToDate;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetSalesReport();"));
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
                Cmd = null;
            }
            return myInfo;
        }

        public static DataTable GetHSTPayble(string FromDate, string ToDate)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myHSTInfo = null;
            try
            {
                myHSTInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "RPTPaybleHST";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = FromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = ToDate;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myHSTInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetHSTPayble(); "));
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
                Cmd = null;
            }
            return myHSTInfo;
        }

        public static DataTable GetHSTReceiveable(string FromDate, string ToDate)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myHSTInfo = null;
            try
            {
                myHSTInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "RPTReceivedHST";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = FromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = ToDate;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myHSTInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetHSTReceiveable(); "));
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
                Cmd = null;
            }
            return myHSTInfo;
        }

        public static DataTable GetMonthlyInvoices(int Month, int Year, long customerNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myinvInfo = null;
            try
            {
                myinvInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "RPTGetMonthlyTransactionsReport";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@Year";
                parm.Value = Year;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Month";
                parm.Value = Month;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerNum";
                parm.Value = customerNum;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myinvInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetMonthlyInvoices(); "));
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
                Cmd = null;
            }
            return myinvInfo;
        }

        public static DataTable GetReprintInv(ref string InvID)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myinvInfo = null;
            try
            {
                myinvInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetReprintInvInfo";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@InvID";
                parm.Value = InvID;
                Cmd.Parameters.Add(parm);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myinvInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetReprintInv(); "));
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
                Cmd = null;
            }
            return myinvInfo;
        }

        public static DataTable GetOrderDrivers(string orderNumber)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myInfo = null;
            try
            {
                myInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetOrderDrivers";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = orderNumber;
                Cmd.Parameters.Add(parm);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetOrderDrivers(); "));
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
                Cmd = null;
            }
            return myInfo;
        }

        public static DataTable GetTotalTransactions(string customerNum, string FromDate, string ToDate)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myOrders = null;
            int TTLRec = 0;
            try
            {
                myOrders = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetTotalTransactions";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@CustomerNum";
                parm.Value = customerNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = FromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = ToDate;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myOrders);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetTotalTransactions(); "));
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
                Cmd = null;
            }
            return myOrders;
        }
        public static DataTable GetOrderPrintInfo(string OrderNumber)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myinvInfo = null;
            try
            {
                myinvInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetInvoicePrintDataByOrder";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@ordNum";
                parm.Value = OrderNumber;
                Cmd.Parameters.Add(parm);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myinvInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetOrderPrintInfo(); "));
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
                Cmd = null;
            }
            return myinvInfo;
        }

        public static DataTable GetWebOrderStatus(Int64 wayBill)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable ordStatus = null;
            try
            {
                ordStatus = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetWebOrderStatus";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                parm = new SqlParameter();
                parm.ParameterName = "@waybill";
                parm.Value = wayBill;
                Cmd.Parameters.Add(parm);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(ordStatus);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetWebOrderStatus(); "));
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
                Cmd = null;
            }
            return ordStatus;
        }

        public static DataTable GetPreviousCustomers(string BillerNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myCustomers = null;
            StringBuilder myQuery = new StringBuilder();
            try
            {
                myCustomers = new DataTable();
                con.Open();
                Cmd = new SqlCommand();

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "GetPreviousCustomers";
                Cmd.Connection = con;
                Cmd.Parameters.AddWithValue("@CustomerNum", BillerNum);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myCustomers);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPreviousCustomers(); "));
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
                Cmd = null;
            }
            return myCustomers;
        }

        public static long AddNewAddress(string orderNum, string UsrID, string ordLocID, string customerName)
        {
            SqlCommand Cmd;
            SqlConnection con;
            long custNum = -9999;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myCustomers = null;
            StringBuilder myQuery = new StringBuilder();
            try
            {
                myCustomers = new DataTable();
                con.Open();
                Cmd = new SqlCommand();

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "AddNewAddress";
                Cmd.Connection = con;

                Cmd.Parameters.AddWithValue("@OrderNum", orderNum);
                Cmd.Parameters.AddWithValue("@CustomerID", custNum);
                Cmd.Parameters["@CustomerID"].Direction = ParameterDirection.InputOutput;
                Cmd.Parameters.AddWithValue("@CreatedBy", UsrID);
                Cmd.Parameters.AddWithValue("@OrdLocationID", ordLocID);
                Cmd.Parameters.AddWithValue("@CustomerName", customerName);

                Cmd.ExecuteNonQuery();
                custNum = long.Parse(Cmd.Parameters["@CustomerID"].Value.ToString());
                con.Close();
            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AddNewAddress(); "));
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
                Cmd = null;
            }
            return custNum;

        }

        public static void setOrdChargesCorrectly(string OrderNum)
        {
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandText = "Update DeliveryOrderCharges SET ACTIVE = 0 where OrdNum in (select OrderNumber from DeliveryOrders where ISNULL(Active,1) = 0 and OrderNumber = '" + OrderNum + "' )";
                    Cmd.CommandType = CommandType.Text;
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        con.Open();
                        Cmd.Connection = con;
                        Cmd.ExecuteNonQuery();
                        Cmd.CommandText = "Update DeliveryOrderCharges SET ACTIVE = 0 Where OrdNum = '" + OrderNum + "' And ReturnServiceID not in (select ReturnServiceID from DeliveryOrders Where OrderNumber = '" + OrderNum + "')";
                    }
                }

            }
            catch
            {
            }

        }

        public static DataTable GetCustomerList(string likeString, string lstbase, bool includeAddress)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myCustomers = null;
            StringBuilder myQuery = new StringBuilder();
            try
            {
                myCustomers = new DataTable();
                con.Open();
                Cmd = new SqlCommand();

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "GetCustsDynamically";
                Cmd.Connection = con;

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@likeString";
                param.Value = likeString;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@includeAddress";
                param.Value = includeAddress;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@lstbase";
                param.Value = lstbase;
                Cmd.Parameters.Add(param);



                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myCustomers);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomerList(); "));
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
                Cmd = null;
            }
            return myCustomers;
        }



        public static bool StoreImage(ref List<stCustomerInvoiceInfo> customerInvoices, bool IsStorageInvoice, string FileExtension)
        {
            FileStream oImg;
            BinaryReader oBinaryReader;
            byte[] oImgByteArray;

            SqlCommand Cmd = null;
            SqlConnection con = null;
            SqlTransaction tr = null;

            bool result = true;
            try
            {
                con = new SqlConnection();
                con.ConnectionString = Global.ASCGInvStorgeConnectionstring;
                con.Open();
                tr = con.BeginTransaction();
                Cmd = new SqlCommand();
                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;
                Cmd.CommandText = "SaveImage";
                Cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter parm = null;

                parm = new SqlParameter();
                parm.ParameterName = "@InvoiceID";
                Cmd.Parameters.Add(parm);

                Cmd.Parameters.Add(new SqlParameter("@ImageFile", SqlDbType.Image, 2147483647));

                parm = new SqlParameter();
                parm.ParameterName = "@StorageFlag";
                Cmd.Parameters.Add(parm);


                for (int kk = 0; kk < customerInvoices.Count; kk++)
                {
                    oImg = new FileStream(customerInvoices[kk].FileName, FileMode.Open, FileAccess.Read);

                    oBinaryReader = new BinaryReader(oImg);

                    oImgByteArray = oBinaryReader.ReadBytes((int)oImg.Length);

                    oBinaryReader.Close();

                    oImg.Close();

                    Cmd.Parameters["@InvoiceID"].Value = customerInvoices[kk].InvoiceID;
                    Cmd.Parameters["@ImageFile"].Value = oImgByteArray;
                    Cmd.Parameters["@StorageFlag"].Value = IsStorageInvoice;
                    Cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex1)
            {
                result = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:StoreImage(); "));
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
                if (tr != null)
                {
                    if (result == false)
                        tr.Rollback();
                    else
                        tr.Commit();
                }
                Cmd = null;
                tr = null;
                con = null;
            }
            return result;

        }

        public static string GetInvoiceImageFromDB(string InvoiceID, bool storageFlag)
        {
            DataTable dtImage = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con = null;
            byte[] oImgByteArray = null;
            string fName = string.Empty;
            if (storageFlag)
            {
                fName = @"c:\" + InvoiceID + "-" + DateTime.Now.Year.ToString() +
                                              DateTime.Now.Month.ToString() +
                                              DateTime.Now.Day.ToString() +
                                              DateTime.Now.Hour.ToString() +
                                              DateTime.Now.Minute.ToString() +
                                              DateTime.Now.Second.ToString() + ".jpg";
            }
            else
            {
                fName = @"c:\" + InvoiceID + "-" + DateTime.Now.Year.ToString() +
                                                  DateTime.Now.Month.ToString() +
                                                  DateTime.Now.Day.ToString() +
                                                  DateTime.Now.Hour.ToString() +
                                                  DateTime.Now.Minute.ToString() +
                                                  DateTime.Now.Second.ToString() + ".PDF";
            }
            try
            {
                con = new SqlConnection();
                con.ConnectionString = Global.ASCGInvStorgeConnectionstring;

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@InvoiceID";
                parm.Value = InvoiceID;
                dtImage = new DataTable();
                //con.Open();
                Cmd = new SqlCommand();
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@StorageFlag";
                if (storageFlag)
                {
                    parm.Value = 1;
                }
                else
                {
                    parm.Value = 0;
                }
                Cmd.Parameters.Add(parm);

                Cmd.CommandText = "GetDBImage";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                con.Open();
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(dtImage);
                foreach (DataRow oRow in dtImage.Rows)
                {
                    oImgByteArray = (byte[])oRow["ImageFile"];
                    FileStream oOutput = File.Create(fName, oImgByteArray.Length);
                    oOutput.Write(oImgByteArray, 0, oImgByteArray.Length);
                    oOutput.Close();
                }

                if (dtImage.Rows.Count <= 0)
                {
                    fName = string.Empty;
                }
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetInvoiceImageFromDB(); "));
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
                if (con != null)
                {
                    con.Close();
                }
                Cmd = null;
            }
            return fName;
        }

        public static string GetPayrollImageFromDB(string commNumber)
        {
            DataTable dtImage = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con = null;
            byte[] oImgByteArray = null;
            string fName = string.Empty;

            fName = @"c:\" + Guid.NewGuid().ToString() + "-" + DateTime.Now.Year.ToString() +
                                              DateTime.Now.Month.ToString() +
                                              DateTime.Now.Day.ToString() +
                                              DateTime.Now.Hour.ToString() +
                                              DateTime.Now.Minute.ToString() +
                                              DateTime.Now.Second.ToString() + ".PDF";

            try
            {
                con = new SqlConnection();
                con.ConnectionString = Global.ASCGInvStorgeConnectionstring;

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@commNumber";
                parm.Value = commNumber;
                dtImage = new DataTable();
                //con.Open();
                Cmd = new SqlCommand();
                Cmd.Parameters.Add(parm);
                Cmd.CommandText = "GetPayRollDBImage";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                con.Open();
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(dtImage);
                foreach (DataRow oRow in dtImage.Rows)
                {
                    oImgByteArray = (byte[])oRow["ImageFile"];
                    FileStream oOutput = File.Create(fName, oImgByteArray.Length);
                    oOutput.Write(oImgByteArray, 0, oImgByteArray.Length);
                    oOutput.Close();
                }


            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetPayrollImageFromDB(); "));
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
                if (con != null)
                {
                    con.Close();
                }
                Cmd = null;
            }
            return fName;
        }

        public static DataSet GetGlobalWebWayBill(string OrderNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet MyWayBill = null;
            try
            {
                MyWayBill = new DataSet();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalWebDeliveryWayBill";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                SqlParameter param = new SqlParameter();
                param.Value = OrderNum;
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(MyWayBill);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalWebWayBill(); "));
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
                Cmd = null;
            }
            return MyWayBill;

        }

        public static bool HandelNoPickUp(Int16 driverNum, SortedList<string, string> ordNums, string UsrID, DeliveryAction myDeliveryAct)
        {
            bool bResult = true;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            SqlTransaction tr = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool res = true;

            try
            {
                con.Open();
                tr = con.BeginTransaction();
                Cmd = new SqlCommand();
                Cmd.CommandText = "HandelNoPickUp";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = tr.Connection;
                Cmd.Transaction = tr;

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = UsrID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReturnFlag";

                if (myDeliveryAct == DeliveryAction.RNoPickUp)
                    param.Value = 1;
                else
                    param.Value = 0;

                Cmd.Parameters.Add(param);

                foreach (string ord in ordNums.Keys)
                {
                    Cmd.Parameters["@OrdNum"].Value = ord;
                    Cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch (Exception ex1)
            {
                tr.Rollback();
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:HandelNoPickUp(); "));
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
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                }
                if (tr != null)
                {
                    tr.Dispose();
                    tr = null;
                }

            }
            return bResult;
        }

        public static bool UndoDriverAssignment(Int16 driverNum, string ordNum, string UsrID, DeliveryAction myDeliveryAct)
        {
            bool bResult = true;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool res = true;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "UndoDriverAssignment";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = UsrID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                param.Value = ordNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:UndoDriverAssignment(); "));
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
                if (con != null) con.Close();

            }
            return bResult;
        }

        public static bool UndoDriverAssignment(Int16 driverNum, SortedList<string, string> myWBs, string UsrID, DeliveryAction myDeliveryAct)
        {
            bool bResult = true;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlTransaction tr = null;
            bool res = true;

            try
            {
                con.Open();
                tr = con.BeginTransaction();
                Cmd = new SqlCommand();

                Cmd.CommandText = "UndoDriverAssignment";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = tr.Connection;
                Cmd.Transaction = tr;

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = UsrID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);

                foreach (string ord in myWBs.Keys)
                {
                    Cmd.Parameters["@OrdNum"].Value = ord;
                    Cmd.ExecuteNonQuery();
                }


                tr.Commit();

            }
            catch (Exception ex1)
            {
                bResult = false;
                tr.Rollback();
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:UndoDriverAssignment(); "));
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
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }

            }
            return bResult;
        }

        public static bool HandleGlobalDispatchAction(Int16 driverNum, string ordNum, string UsrID, DeliveryAction myDeliveryAct)
        {
            bool bResult = true;
            SqlDataAdapter dataAdp;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool res = true;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "DispatchGlobalOrder";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = UsrID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                param.Value = ordNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReturnServiceFlag";
                param.Value = (myDeliveryAct == DeliveryAction.RDispatched);
                Cmd.Parameters.Add(param);

                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:HandleGlobalDispatchAction(); "));
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
                if (con != null) con.Close();
            }
            return bResult;
        }

        public static bool HandleGlobalDispatchAction(Int16 driverNum, List<string> ordNum, string UsrID)
        {
            bool bResult = true;
            SqlDataAdapter dataAdp;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool res = true;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "DispatchGlobalOrder";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = UsrID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReturnServiceFlag";
                param.Value = 0;
                Cmd.Parameters.Add(param);

                for (int k = 0; k < ordNum.Count; k++)
                {
                    Cmd.Parameters["@OrdNum"].Value = ordNum[k];
                    Cmd.ExecuteNonQuery();
                }
                con.Close();
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:HandleGlobalDispatchAction(); "));
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
                if (con != null) con.Close();
            }
            return bResult;
        }

        public static bool HandleGlobalPassOffAction(Int16 driverNum,
                                                    SortedList<string, string> ordNums, Int16 receivingDrvNum,
                                                    int waitTime, int adjustedTime, DateTime passedoffOn,
                                                    string userID, DeliveryAction podAction)
        {
            bool bResult = true;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            SqlTransaction tr = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            try
            {
                con.Open();
                tr = con.BeginTransaction();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GlobalPassOff";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = tr.Connection;
                Cmd.Transaction = tr;

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReceivingDrvNum";
                param.Value = receivingDrvNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@waitTime";
                param.Value = waitTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@adjustedTime";
                param.Value = adjustedTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@passedoffOn";
                param.Value = passedoffOn;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = userID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReturnServiceFlag";
                param.Value = (podAction == DeliveryAction.RPassOff);
                Cmd.Parameters.Add(param);
                foreach (string ord in ordNums.Keys)
                {
                    Cmd.Parameters["@OrdNum"].Value = ord;
                    Cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch (Exception ex1)
            {
                bResult = false;
                tr.Rollback();
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:HandleGlobalPassOffAction(); "));
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
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
                if (tr != null)
                {
                    tr.Dispose();
                }
            }
            return bResult;
        }


        public static bool SetUpGlobalPickedUP(Int16 driverNum, SortedList<string, string> ordNums,
                                                int waitTime, DateTime pickedUpDateTime, DateTime dispatchedDateTime,
                                                string userID, DeliveryAction pickedUpAct)
        {
            bool bResult = true;
            SqlParameter param;
            SqlCommand Cmd = null;
            SqlConnection con = null;
            SqlTransaction tr = null;
            try
            {
                con = new SqlConnection();
                con.ConnectionString = GlobalConnectionstring;
                con.Open();

                tr = con.BeginTransaction();

                Cmd = new SqlCommand();
                Cmd.CommandText = "GlobalPickedUp";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = tr.Connection;
                Cmd.Transaction = tr;

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@waitTime";
                param.Value = waitTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@PickedUpOn";
                param.Value = pickedUpDateTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DispatchDateTime";
                param.Value = dispatchedDateTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = userID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReturnServiceFlag";
                param.Value = (pickedUpAct == DeliveryAction.RPickedUp);
                Cmd.Parameters.Add(param);

                foreach (string ord in ordNums.Keys)
                {
                    Cmd.Parameters["@OrdNum"].Value = ord;
                    Cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch (Exception ex1)
            {
                tr.Rollback();
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SetUpGlobalPickedUP(); "));
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
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                }
                if (Cmd != null)
                {
                    Cmd.Dispose();
                    Cmd = null;
                }
            }
            return bResult;
        }

        public static bool SetUpGlobalDelivery(Int16 driverNum, SortedList<string, string> ordNums,
                                                       int waitTime, string PodRemarks, string approvedBy,
                                                       DateTime deliveredOn, string userID,
                                                        DeliveryAction podAction)
        {
            bool bResult = true;
            SqlParameter param;
            SqlCommand Cmd = null;
            SqlConnection con = null;
            SqlTransaction tr = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            try
            {
                con.Open();
                tr = con.BeginTransaction();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GlobalPOD";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Transaction = tr;

                param = new SqlParameter();
                param.ParameterName = "@DrvNum";
                param.Value = driverNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrdNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@waitTime";
                param.Value = waitTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@deliveredOn";
                param.Value = deliveredOn;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@PODRemarks";
                param.Value = PodRemarks;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ApprovedBy";
                param.Value = approvedBy;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@UserID";
                param.Value = userID;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ReturnServiceFlag";
                param.Value = (podAction == DeliveryAction.RDelivery);
                Cmd.Parameters.Add(param);

                foreach (string ord in ordNums.Keys)
                {
                    Cmd.Parameters["@OrdNum"].Value = ord;
                    Cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            catch (Exception ex1)
            {
                tr.Rollback();
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SetUpGlobalDelivery(); "));
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
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                }
                if (tr != null)
                {
                    tr.Dispose();
                    tr = null;
                }
                if (Cmd != null)
                {
                    Cmd.Dispose();
                    Cmd = null;
                }
            }
            return bResult;
        }

        public static List<StMonths> GetMonths()
        {
            List<StMonths> myMonths = new List<StMonths>(12);
            StMonths mn = new StMonths();
            mn.MonthName = "Jan";
            mn.MonthValue = 1;
            myMonths.Add(mn);

            mn = new StMonths();
            mn.MonthName = "Feb";
            mn.MonthValue = 2;
            myMonths.Add(mn);


            mn = new StMonths();
            mn.MonthName = "March";
            mn.MonthValue = 3;
            myMonths.Add(mn);

            mn = new StMonths();
            mn.MonthName = "Apr";
            mn.MonthValue = 4;
            myMonths.Add(mn);

            mn.MonthName = "May";
            mn.MonthValue = 5;
            myMonths.Add(mn);

            mn = new StMonths();
            mn.MonthName = "Jun";
            mn.MonthValue = 6;
            myMonths.Add(mn);


            mn = new StMonths();
            mn.MonthName = "July";
            mn.MonthValue = 7;
            myMonths.Add(mn);

            mn = new StMonths();
            mn.MonthName = "Aug";
            mn.MonthValue = 8;
            myMonths.Add(mn);

            mn.MonthName = "Sep";
            mn.MonthValue = 9;
            myMonths.Add(mn);

            mn = new StMonths();
            mn.MonthName = "Oct";
            mn.MonthValue = 10;
            myMonths.Add(mn);


            mn = new StMonths();
            mn.MonthName = "Nov";
            mn.MonthValue = 11;
            myMonths.Add(mn);

            mn = new StMonths();
            mn.MonthName = "Dec";
            mn.MonthValue = 12;
            myMonths.Add(mn);

            return myMonths;
        }
        public struct stOrdAmt
        {
            public decimal invOrdNetAmt;
            public decimal invOrdNetGST;
        }
        public static Guid mfiStorageID = new Guid("86c7faa0-a016-46b1-843c-b2af84003e12");
        public const int blankCuromerNum = -9999;
        public const int blankOrderNum = -9999;
       // private static ASCG.Library.ASCGUser mLoggedInUser;
        private static DataTable Provinces;
        private static DataTable AllDeliveryTypes;
        private static DataTable AllDeliveryRecords;
        private static DataTable AllDeliveriesBills;
        private static DataTable AllBillingDeliveries;
        private static DataTable AllStorageRecords;
        private static DataTable SameDayCitiesRates;
        private static DataTable CitiesRates;
        private static DataTable DeliveryRecForMain;
        private static DataTable Cities;
        private static StringBuilder mErrorInfo;
        private static string MFIGST = string.Empty;

        public static Boolean bAllowNewDelivery = false;
        public static Boolean bAllowModifyDelivery = false;
        public static Boolean bAllowNewStorage = false;
        public static Boolean bAllowModifyStorage = false;
        public static Boolean bAllowCustomerBilling = false;
        public static Boolean bAllowOwnerOperatorBilling = false;
        public static Boolean bAllowInvoiceHandling = false;
        public const string sDispatched = "Dispatched";
        public const string sOnBoard = "On-Board";
        public const string sToggleDispatched = "Toggle Dispatch";
        public const string sPPOD = "Pass On POD";
        public const string sPOD = "POD";
        public const string sReRouted = "Re-Routed";
        public const string sStorageOrder = "Create Storage";
        public const string sDeliveryAssignment = "Delivery Assigment";

        public const string sCreateDelivery = "Create Delivery";
        public const string sCreatePicUp = "Create PickUp";
        public const string sCreateInvoice = "Create Invoice";


        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;
            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }


        public static string DecimalValue(string svalue)
        {
            if (!Global.IsNumeric(svalue)) return string.Empty;
            if (!svalue.Contains("."))
            {
                svalue = svalue + ".00";
            }
            else
            {
                if (svalue.Substring(svalue.IndexOf(".") + 1).Length < 2)
                {
                    svalue = svalue + "0";
                }
            }
            return svalue;
        }

        public static string DecimalValueZero(string svalue)
        {
            if (!Global.IsNumeric(svalue)) return "0.00";
            if (!svalue.Contains("."))
            {
                svalue = svalue + ".00";
            }
            else
            {
                if (svalue.Substring(svalue.IndexOf(".") + 1).Length < 2)
                {
                    svalue = svalue + "0";
                }
            }
            return svalue;
        }

        public static int GetInvoiceDueIndex(int InvDueDays)
        {
            InvoiceDueDaysTypes res = InvoiceDueDaysTypes.COD;
            switch (InvDueDays)
            {
                case 0:
                    res = InvoiceDueDaysTypes.COD;
                    break;
                case 7:
                    res = InvoiceDueDaysTypes.Due7Days;
                    break;
                case 15:
                    res = InvoiceDueDaysTypes.Due15Days;
                    break;
                case 30:
                    res = InvoiceDueDaysTypes.Due30Days;
                    break;
            }
            return (int)res;
        }


        public static string GlobalConnectionstring
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ASCGDB"].ConnectionString;
            }

        }
        public static string AlsConnectionstring
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["AlsDB"].ConnectionString;
            }

        }

        public static string GlobalTempFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["TempFolder"];
            }

        }

        public static string ASCGInvStorgeConnectionstring
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ASCGDBInvStorage"].ConnectionString;
            }

        }
        public static string AlsInvStorgeConnectionstring
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["AlsDBInvStorage"].ConnectionString;
            }

        }

        public static string PrinterName
        {
            get
            {
                return ConfigurationManager.AppSettings["PrinterName"];
            }

        }

        //public static ASCG.Library.ASCGUser LogggedInUser
        //{
        //    get
        //    {
        //        return mLoggedInUser;
        //    }
        //    set
        //    {
        //        mLoggedInUser = value;
        //        SetUserAccess();
        //    }

        //}

        //public static bool PermissionExists(UsersPermissions perm)
        //{
        //    try
        //    {
        //        DataRow[] rows = LogggedInUser.UsersPrmissions.Select("PermissionName like '%" + perm.ToString() + "'");
        //        return (rows.Length > 0);
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}

        static public bool GetInvoices(ref DataSet ds, string InvID, string CustomerNum, object fromDt, object ToDate)
        {

            SqlConnection con = null;
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();
            SqlParameter parm = null;
            SqlDataAdapter dataAdp = null;
            try
            {
                con = new SqlConnection();
                con.ConnectionString = GlobalConnectionstring;

                con = new SqlConnection();
                con.ConnectionString = Global.GlobalConnectionstring;
                con.Open();

                if (InvID.Equals(string.Empty))
                {
                    parm = new SqlParameter("@InvID", DBNull.Value);
                    Cmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter("@InvID", InvID);
                    Cmd.Parameters.Add(parm);
                }

                if (CustomerNum.Equals(blankCuromerNum.ToString()))
                {
                    parm = new SqlParameter("@CustNum", DBNull.Value);
                    Cmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter("@CustNum", CustomerNum);
                    Cmd.Parameters.Add(parm);
                }


                parm = new SqlParameter("@DateFrom", fromDt);
                Cmd.Parameters.Add(parm);


                parm = new SqlParameter("@DateTo", ToDate);
                Cmd.Parameters.Add(parm);

                Cmd.Connection = con;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "GetInvoicesForSearch";


                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(ds);
                con.Close();


            }
            catch (Exception ex1)
            {
                bResult = false;

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetInvoices(); "));
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
                if (con != null) con.Close();
                Cmd = null;

            }
            return bResult;

        }

        static public DataRow GetWayBillCustInfo(string wayBillNum)
        {
            DataRow rw = null;
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable tbl = new DataTable();
            StringBuilder cmdText = new StringBuilder();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select WayBillNumber,Type,CustomerNum from PrePrintedWayBills where WayBillNumber = '");
                cmdText.Append(wayBillNum);
                cmdText.Append("' And Active = 1");
                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    rw = tbl.Rows[0];
                }
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetWayBillCustInfo(); "));
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
                Cmd = null;
            }
            return rw;
        }

        static public bool SetStorageLocation(ref DataTable dtStorages)
        {
            SqlCommand Cmd;
            SqlConnection con = null;
            SqlDataAdapter dataAdp;
            bool bResult = true;
            SqlCommand InsertCmd = new SqlCommand();
            SqlCommand UpdateCmd = new SqlCommand();
            SqlCommand DeleteCmd = new SqlCommand();

            try
            {
                dataAdp = new SqlDataAdapter();
                con = new SqlConnection();
                con.ConnectionString = GlobalConnectionstring;

                con = new SqlConnection();
                con.ConnectionString = Global.GlobalConnectionstring;
                con.Open();

                setStorageLocationsCommand(ref InsertCmd, SQLCmdType.cmdInsert);
                InsertCmd.Connection = con;
                InsertCmd.CommandType = CommandType.StoredProcedure;
                InsertCmd.CommandText = "SP_InsertStorageLocation";
                dataAdp.InsertCommand = InsertCmd;

                //this.setLocationsCommand(ref UpdateCmd, SQLCmdType.cmdUpdate);
                //UpdateCmd.Connection = con;
                //UpdateCmd.CommandType = CommandType.StoredProcedure;
                //UpdateCmd.CommandText = "SP_UpdateCustomerLocations";
                //dataAdp.UpdateCommand = UpdateCmd;

                //this.setLocationsCommand(ref DeleteCmd, SQLCmdType.cmdDelete);
                //DeleteCmd.Connection = con;
                //DeleteCmd.CommandType = CommandType.StoredProcedure;
                //DeleteCmd.CommandText = "SP_DeleteCustomerLocations";
                //dataAdp.DeleteCommand = DeleteCmd;

                dataAdp.Update(dtStorages);


            }
            catch (Exception ex1)
            {
                bResult = false;

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SetStorageLocation(); "));
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
                //con.Close();
                InsertCmd = null;
                UpdateCmd = null;
                DeleteCmd = null;
                if (con != null) con.Close();
            }
            return bResult;

        }

        static public void setStorageLocationsCommand(ref SqlCommand sqlCmd, SQLCmdType cmdType)
        {
            SqlParameter parm;
            try
            {
                #region "Params for Insert & Update"

                if (cmdType == SQLCmdType.cmdInsert || cmdType == SQLCmdType.cmdUpdate)
                {

                    parm = new SqlParameter();
                    parm.ParameterName = "@StreetLocation";
                    parm.SourceColumn = "StreetLocation";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Suite";
                    parm.SourceColumn = "Suite";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@City";
                    parm.SourceColumn = "City";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Province";
                    parm.SourceColumn = "Province";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Country";
                    parm.SourceColumn = "Country";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Fax";
                    parm.SourceColumn = "Fax";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Phone1";
                    parm.SourceColumn = "Phone1";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Phone2";
                    parm.SourceColumn = "Phone2";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Cell";
                    parm.SourceColumn = "Cell";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Email1";
                    parm.SourceColumn = "Email1";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@Email2";
                    parm.SourceColumn = "Email2";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@PostalCode";
                    parm.SourceColumn = "PostalCode";
                    sqlCmd.Parameters.Add(parm);

                }
                #endregion
                if (cmdType == SQLCmdType.cmdInsert)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.SourceColumn = "CreatedBy";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.SourceColumn = "CreatedDateTime";
                    sqlCmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.SourceColumn = "LastModifiedBy";
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.SourceColumn = "LastModifiedDateTime";
                    sqlCmd.Parameters.Add(parm);
                }

                parm = new SqlParameter();
                parm.ParameterName = "@StorageOwner";
                parm.SourceColumn = "StorageOwner";
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@StorageLocID";
                parm.SourceColumn = "StorageLocID";
                sqlCmd.Parameters.Add(parm);


            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:setStorageLocationsCommand(); "));
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                if (!(ex1.InnerException == null))
                {
                    mErrorInfo.Append(" ;");
                    mErrorInfo.Append("Inner Exception: ");
                    mErrorInfo.Append(ex1.InnerException.ToString());
                }

                Global.LogInEventLogger(mErrorInfo.ToString());
                throw new Exception(mErrorInfo.ToString());
                //mErrorInfo = null;

            }

        }

        public static DataTable GetPendingInvoices(string customerNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con = null;
            DataTable myInfo = new DataTable();
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                if (string.IsNullOrEmpty(customerNum) || !Global.IsNumeric(customerNum))
                {
                    customerNum = "-9999";
                }
                Provinces = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "RPTOutStanding";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Parameters.AddWithValue("@CustomerNum", customerNum);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPendingInvoices(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return myInfo;

        }

        public static DataTable ALLProvinces()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                Provinces = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_SelectProvinces";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(Provinces);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllProvinces(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return Provinces;

        }

        public static bool SetPrePrintedWBList(DataTable dtWBList)
        {
            bool bResult = true;
            string cmdDelete = "DELETE FROM [dbo].[PrePrintedWayBills] WHERE WayBillNumber = '<WayBillNumber>'";
            StringBuilder cmd = new StringBuilder();
            try
            {
                //DataRow[] rws = dtWBList.Select("Status <> 'NO CHANGE'");

                foreach (DataRow r in dtWBList.Rows)
                {

                    if (r["Status"].ToString().ToUpper().Equals("MARKED DELETED"))
                    {
                        cmd.Append(cmdDelete.Replace("<WayBillNumber>", r["WayBillNumber"].ToString()));
                    }
                    cmd.Append(Environment.NewLine.ToString());
                }

                using (SqlCommand c = new SqlCommand())
                {
                    using (SqlConnection con = new SqlConnection(Global.GlobalConnectionstring))
                    {
                        c.CommandText = cmd.ToString();
                        c.CommandType = CommandType.Text;
                        c.Connection = con;
                        con.Open();
                        c.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
            }
            return bResult;
        }

        public static bool SetProvinces(DataTable dtprovinces)
        {
            bool bResult = true;
            string cmdInsert = "INSERT INTO [dbo].[Province]([ProvinceID],[ProvinceName]) VALUES ('<ProvinceID>','<ProvinceName>')";
            string cmdUpdate = "UPDATE [dbo].[Province] SET [ProvinceName] = '<ProvinceName>' WHERE [ProvinceID] = '<ProvinceID>'";
            string cmdDelete = "Delete [dbo].[Province] WHERE [ProvinceID] = '<ProvinceID>'";
            StringBuilder cmd = new StringBuilder();
            try
            {
                DataRow[] rws = dtprovinces.Select("Status <> 'NO CHANGE'");

                foreach (DataRow r in rws)
                {
                    if (r["Status"].ToString().ToUpper().Equals("ADD NEW"))
                    {
                        cmd.Append(cmdInsert.Replace("<ProvinceID>", r["ProvinceID"].ToString()).Replace("<ProvinceName>", r["ProvinceName"].ToString()));
                        cmd.Append(Environment.NewLine.ToString());
                    }
                    else
                    {
                        if (r["Status"].ToString().ToUpper().Equals("MARKED DELETED"))
                        {
                            cmd.Append(cmdDelete.Replace("<ProvinceID>", r["ProvinceID"].ToString()));
                        }
                        else
                        {
                            cmd.Append(cmdUpdate.Replace("<ProvinceID>", r["ProvinceID"].ToString()).Replace("<ProvinceName>", r["ProvinceName"].ToString()));
                        }
                        cmd.Append(Environment.NewLine.ToString());
                    }
                }

                using (SqlCommand c = new SqlCommand())
                {
                    using (SqlConnection con = new SqlConnection(Global.GlobalConnectionstring))
                    {
                        c.CommandText = cmd.ToString();
                        c.CommandType = CommandType.Text;
                        c.Connection = con;
                        con.Open();
                        c.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
            }
            return bResult;
        }

        public static bool SetCities(DataTable dtprovinces)
        {
            bool bResult = true;
            string cmdInsert = "INSERT INTO [dbo].[Province]([ProvinceID],[ProvinceName]) VALUES ('<ProvinceID>','<ProvinceName>')";
            string cmdUpdate = "UPDATE [dbo].[CityRates] SET [CityName] = '<CityName>' WHERE [CityName] = '<CityName>'";
            string cmdDelete = "Delete [dbo].[CityRates] WHERE [CityName] = '<CityName>'";
            StringBuilder cmd = new StringBuilder();
            try
            {
                DataRow[] rws = dtprovinces.Select("Status <> 'NO CHANGE'");

                foreach (DataRow r in rws)
                {
                    if (r["Status"].ToString().ToUpper().Equals("ADD NEW"))
                    {
                        cmd.Append(cmdInsert.Replace("<ProvinceID>", r["ProvinceID"].ToString()).Replace("<ProvinceName>", r["ProvinceName"].ToString()));
                        cmd.Append(Environment.NewLine.ToString());
                    }
                    else
                    {
                        if (r["Status"].ToString().ToUpper().Equals("MARKED DELETED"))
                        {
                            cmd.Append(cmdDelete.Replace("<ProvinceID>", r["ProvinceID"].ToString()));
                        }
                        else
                        {
                            cmd.Append(cmdUpdate.Replace("<ProvinceID>", r["ProvinceID"].ToString()).Replace("<ProvinceName>", r["ProvinceName"].ToString()));
                        }
                        cmd.Append(Environment.NewLine.ToString());
                    }
                }

                using (SqlCommand c = new SqlCommand())
                {
                    using (SqlConnection con = new SqlConnection(Global.GlobalConnectionstring))
                    {
                        c.CommandText = cmd.ToString();
                        c.CommandType = CommandType.Text;
                        c.Connection = con;
                        con.Open();
                        c.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
            }
            return bResult;
        }

        public static DataTable ListCities()
        {
            DataTable prov = new DataTable();
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandText = "select distinct IsNull(CityName,'') as CityName,'NO CHANGE' as Status from dbo.CityRates";
                    Cmd.CommandType = CommandType.Text;
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        con.Open();
                        Cmd.Connection = con;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter(Cmd))
                        {
                            dataAdp.Fill(prov);
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ListCities(); "));
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

            return prov;

        }

        public static DataTable ListProvinces()
        {
            DataTable prov = new DataTable();
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandText = "select ProvinceID,ProvinceName,'NO CHANGE' as Status from dbo.Province";
                    Cmd.CommandType = CommandType.Text;
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        con.Open();
                        Cmd.Connection = con;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter(Cmd))
                        {
                            dataAdp.Fill(prov);
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ListProvinces(); "));
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

            return prov;

        }

        public static string RemoveEmployee(string EmployeeNum)
        {
            string msg = "Removed employee successfully.";
            object rwCount;
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandText = "SELECT count(1) FROM [dbo].[OrderDrivers] where DriverNum = " + EmployeeNum + " And IsNull(Active,1) = 1";
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        Cmd.CommandType = CommandType.Text;
                        con.Open();
                        Cmd.Connection = con;
                        rwCount = Cmd.ExecuteScalar();

                        if (rwCount != null && int.Parse(rwCount.ToString()) > 0)
                        {
                            msg = "Employee couldn't be removed as some orders are associated.";
                        }
                        else
                        {
                            Cmd.CommandText = "SELECT Count(*) FROM [dbo].[OrderPOD] Where [ReceiverDriverNum] = " + EmployeeNum;
                            rwCount = Cmd.ExecuteScalar();

                            if (rwCount != null && int.Parse(rwCount.ToString()) > 0)
                            {
                                msg = "Employee couldn't be removed as some orders are associated.";
                            }
                            else
                            {
                                Cmd.CommandText = "DELETE FROM [dbo].[Drivers] WHERE DriverNum = " + EmployeeNum;
                                if (Cmd.ExecuteNonQuery() <= 0)
                                {
                                    msg = "Employee couldn't be removed as some orders are associated.";
                                }

                            }
                        }



                    }
                }

            }
            catch (Exception ex1)
            {
                msg = "Couldn't be removed employee successfully.";
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:RemoveEmployee(); "));
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

            return msg;

        }

        public static DataTable GetPrePrintedWBCustomers()
        {
            DataTable tbl = new DataTable();
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    string sql = "select Distinct WB.CustomerNum,Name ";
                    sql = sql + "From PrePrintedWayBills WB Inner Join Customer c ";
                    sql = sql + " on c.CustomerNum = WB.CustomerNum Where WB.WayBillNumber ";
                    sql = sql + " not in (select WayBillNumber from DeliveryOrders Where IsNull(Active,1) = 1) ";
                    Cmd.CommandText = sql;
                    Cmd.CommandType = CommandType.Text;
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        con.Open();
                        Cmd.Connection = con;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter(Cmd))
                        {
                            dataAdp.Fill(tbl);
                            con.Close();
                        }
                    }
                }
                DataRow rw = tbl.NewRow();
                rw["CustomerNum"] = "-99";
                rw["Name"] = "";
                tbl.Rows.InsertAt(rw, 0);

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPrePrintedWBCustomers(); "));
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

            return tbl;

        }

        public static DataTable GetPrePrintedWBOfCustomers(string customerNum)
        {
            DataTable tbl = new DataTable();
            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    string sql = "SELECT WayBillNumber,Case [Type] When 'S' Then 'Shipper' Else 'Consignee' end as Type ";
                    sql = sql + ",'No Change' as Status FROM [dbo].[PrePrintedWayBills] ";
                    sql = sql + " Where WayBillNumber not in (select WayBillNumber from DeliveryOrders) ";
                    sql = sql + " And CustomerNum = " + customerNum;
                    Cmd.CommandText = sql;
                    Cmd.CommandType = CommandType.Text;
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        con.Open();
                        Cmd.Connection = con;
                        using (SqlDataAdapter dataAdp = new SqlDataAdapter(Cmd))
                        {
                            dataAdp.Fill(tbl);
                            con.Close();
                        }
                    }
                }
                DataRow rw = tbl.NewRow();
                rw["CustomerNum"] = "-99";
                rw["CustomerNum"] = "";
                tbl.Rows.InsertAt(rw, 0);

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPrePrintedWBCustomers(); "));
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

            return tbl;

        }

        public static DataTable SalesCommissionMan(bool AddBlankDriver, string BlankName)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable tbl = null;
            try
            {
                tbl = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SELECT * from Drivers where DriverType = 'S' and Active = 1 order by DriverName ASC";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(tbl);
                con.Close();
                if (AddBlankDriver)
                {
                    DataRow rw = tbl.NewRow();
                    rw["DriverName"] = BlankName;
                    rw["DriverNum"] = Global.blankCuromerNum;
                    rw["Active"] = false;
                    tbl.Rows.InsertAt(rw, 0);
                }
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SalesCommissionMan(); "));
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
                Cmd = null;
            }
            return tbl;

        }

        public static DataTable AllStorageOwners()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable Owners = null;

            try
            {
                Owners = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetAllStorageOwners";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(Owners);
                con.Close();
            }

            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllStorageOwners(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return Owners;

        }

        public static DataTable GetOrderDriver(string driverNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable ordDriver = new DataTable();
            StringBuilder cmdText = new StringBuilder();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select * from DRivers where DriverNum = '");
                cmdText.Append(driverNum);
                cmdText.Append("' And Active = 1");
                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(ordDriver);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetOrderDriver(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return ordDriver;

        }

        public static DataSet GetWayBillBackUP(string orderNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet ordDriver = new DataSet();
            StringBuilder cmdText = new StringBuilder();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetWayBillBackUp";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@OrderNum", orderNum);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(ordDriver);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetWayBillBackUP(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return ordDriver;

        }

        public static DataSet GetGlobalDelivery(string ordNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet order = new DataSet();
            StringBuilder cmdText = new StringBuilder();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalDelivery";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@OrderNum", ordNum);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(order);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalDelivery(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return order;
        }

        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }

        public static string GetGlobalDeliveryUser(string ordNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            string UserName = string.Empty;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalDeliveryUser";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@OrderNum", ordNum);
                Cmd.Connection = con;
                UserName = Cmd.ExecuteScalar().ToString();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalDeliveryUser(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return UserName;
        }


        public static DataTable GetGlobalDriverDeliveryInfo(string ordNum, string driverNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable delDetails = new DataTable();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetDriverDeliveryInfo";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@OrderNum", ordNum);
                Cmd.Parameters.AddWithValue("@DriverNum", driverNum);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(delDetails);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalDriverDeliveryInfo(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return delDetails;
        }

        public static DataTable GetGlobalPreschedule(string customerNum, string HistoryFlag, string PrescheduleDate)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable delDetails = new DataTable();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalPreschedule";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@PrescheduleDate", PrescheduleDate);
                Cmd.Parameters.AddWithValue("@customerNum", customerNum);
                Cmd.Parameters.AddWithValue("@HistoryFlag", HistoryFlag);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(delDetails);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalPreschedule(); "));
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
                Cmd = null;
            }
            return delDetails;
        }


        public static DataTable GetExtraCostVariables()
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable ordcost = new DataTable();
            StringBuilder cmdText = new StringBuilder();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "Select * from ExtraCostVariables";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(ordcost);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetExtraCostVariables(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return ordcost;

        }

        public static DataTable GetGlobalWebWayBill()
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable ordcost = new DataTable();
            StringBuilder cmdText = new StringBuilder();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "Select * from ExtraCostVariables";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(ordcost);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalWebWayBill(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return ordcost;

        }


        public static DataTable GetOrderDataByCustomer(string customerNum, string FromDate, string ToDate)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable _Data = new DataTable();
            SqlDataAdapter dataAdp;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "ReportOrderDataByCustomer";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@CustomerNum", customerNum);
                Cmd.Parameters.AddWithValue("@FromDate", FromDate);
                Cmd.Parameters.AddWithValue("@ToDate", ToDate);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(_Data);

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetOrderDataByCustomer(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return _Data;

        }
        public static DataTable GetWayBillReceipt(string orderNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable wayBillInfo = new DataTable();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetWayBillReceipt";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@OrderNum", orderNum);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(wayBillInfo);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetWayBillReceipt(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return wayBillInfo;

        }

        public static DataTable GetGlobalPresentDrvs()
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable drvs = new DataTable();
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalPresentDrvs";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(drvs);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalPresentDrvs(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return drvs;

        }

        public static bool AssignDriverToGlobalDelivery(string orderNum, string drvNum)
        {
            bool bResult = true;
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable drvs = new DataTable();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "AssignDrvToGlobalOrder";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = orderNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@DrvNum";
                parm.Value = drvNum;
                Cmd.Parameters.Add(parm);

                Cmd.ExecuteNonQuery();

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AssignDriverToGlobalDelivery(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;
        }

        public static bool PercentageForDrvOk(Int16 driverNum)
        {
            bool bResult = true;
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        using (DataTable tbl = new DataTable())
                        {
                            Cmd.CommandType = CommandType.Text;
                            Cmd.CommandText = "select [Rate%] as Rate,FuelPayOff from Drivers where DriverNum = " + driverNum.ToString();
                            using (SqlDataAdapter adp = new SqlDataAdapter())
                            {
                                Cmd.Connection = con;
                                adp.SelectCommand = Cmd;
                                con.ConnectionString = Global.GlobalConnectionstring;
                                con.Open();
                                adp.Fill(tbl);
                                if (tbl.Rows.Count > 0)
                                {
                                    if (tbl.Rows[0]["Rate"] == null || !Global.IsNumeric(tbl.Rows[0]["Rate"])
                                            || decimal.Parse(tbl.Rows[0]["Rate"].ToString()) <= 0 ||
                                        tbl.Rows[0]["FuelPayOff"] == null || !Global.IsNumeric(tbl.Rows[0]["FuelPayOff"])
                                            || decimal.Parse(tbl.Rows[0]["FuelPayOff"].ToString()) <= 0)
                                    {
                                        bResult = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
            }
            return bResult;
        }

        public static DataTable GetGlobalDeliveriesForDrv(Int16 driverNum)
        {
            SqlCommand Cmd = null;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable deliveries = new DataTable();
            SqlDataAdapter dataAdp = null;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetDriverGlobalDelivery";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Parameters.AddWithValue("@DriverNum", driverNum);
                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(deliveries);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetGlobalDeliveriesForDrv(); "));
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
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
                if (Cmd != null)
                {
                    Cmd.Dispose();
                    Cmd = null;
                }
                if (dataAdp != null)
                {
                    dataAdp.Dispose();
                    dataAdp = null;
                }
            }
            return deliveries;

        }


        //public static void SetUserAccess()
        //{

        //    for (int i = 0; i < mLoggedInUser.UsersPrmissions.Rows.Count; i++)
        //    {
        //        switch (mLoggedInUser.UsersPrmissions.Rows[i]["PermissionName"].ToString())
        //        {
        //            case "Create Devilery":
        //                bAllowNewDelivery = true;
        //                break;
        //            case "Modify Delivery":
        //                bAllowModifyDelivery = true;
        //                break;
        //            case "Create Devilery1":
        //                bAllowNewDelivery = true;
        //                break;
        //            case "Modify Delivery1":
        //                bAllowModifyDelivery = true;
        //                break;

        //        }
        //    }

        //}

        public static bool DoesGlobalACCExist(string AccNum, string custNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = false;
            StringBuilder cmdText = new StringBuilder();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select AccNum from Customer where AccNum = '");
                cmdText.Append(AccNum);
                cmdText.Append("'");
                cmdText.Append(" And CustomerNum <> ");
                cmdText.Append(custNum);
                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                if (Cmd.ExecuteScalar() != null)
                    bResult = true;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:DoesGlobalACCExist(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;
        }

        public static bool DriverNumExist(string driverNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = false;
            StringBuilder cmdText = new StringBuilder();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select DriverNum from Drivers where DriverNum = ");
                cmdText.Append(driverNum);
                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                if (Cmd.ExecuteScalar() != null)
                    bResult = true;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:DriverNumExist(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;

        }

        public static bool InvoiceNumExist(string InvoiceID)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = false;
            StringBuilder cmdText = new StringBuilder();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select InvoiceID from MFIAR where InvoiceID = ");
                cmdText.Append(InvoiceID);
                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                if (Cmd.ExecuteScalar() != null)
                    bResult = true;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:InvoiceNumExist(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;

        }


        public static bool WayNumExist(string wayBillNum, string OrdNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = false;
            StringBuilder cmdText = new StringBuilder();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select WayBillNumber from DeliveryOrders where WayBillNumber = '");
                cmdText.Append(wayBillNum);
                cmdText.Append("'");
                if (!string.IsNullOrEmpty(OrdNum) && !OrdNum.Equals(blankOrderNum.ToString()))
                {
                    cmdText.Append(" And OrderNumber <> '");
                    cmdText.Append(OrdNum);
                    cmdText.Append("'");
                }

                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                if (Cmd.ExecuteScalar() != null)
                    bResult = true;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:WayNumExist(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;

        }

        public static bool ISWayNumRangeValid(long startWB, long endWB)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = false;
            StringBuilder cmdText = new StringBuilder();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("SELECT WayBillNumber FROM [dbo].[PrePrintedWayBills] ");
                cmdText.Append("where Case IsNumeric([WayBillNumber]) When  1 Then Cast([WAYBillNumber] as decimal) else 0 end >= {w1} ");
                cmdText.Append("and Case IsNumeric([WayBillNumber]) When 1 Then Cast([WAYBillNumber] as decimal) else 0 end <= {w2} ");
                cmdText.Append("Union SELECT WayBillNumber ");
                cmdText.Append("FROM dbo.DeliveryOrders where Case IsNumeric([WayBillNumber]) When 1 Then Cast([WAYBillNumber] as decimal) else 0 end >= {w1} ");
                cmdText.Append("and Case IsNumeric([WayBillNumber]) When 1 Then Cast([WAYBillNumber] as decimal) else 0 end <= {w2} ");
                Cmd.CommandText = cmdText.ToString().Replace("{w1}", startWB.ToString()).Replace("{w2}", endWB.ToString());
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                if (Cmd.ExecuteScalar() == null)
                    bResult = true;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:WayNumRangeValidity(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;

        }

        public static bool CustomerRefNumExist(string CustRefNum, string OrdNum)
        {
            SqlCommand Cmd;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = false;
            StringBuilder cmdText = new StringBuilder();

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                cmdText.Append("Select CustomerReferenceNum from DeliveryOrders where CustomerReferenceNum = '");
                cmdText.Append(CustRefNum);
                cmdText.Append("'");
                if (!OrdNum.Equals(blankOrderNum.ToString()))
                {
                    cmdText.Append(" And OrderNumber <> '");
                    cmdText.Append(OrdNum);
                    cmdText.Append("'");
                }

                Cmd.CommandText = cmdText.ToString();
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                if (Cmd.ExecuteScalar() != null)
                    bResult = true;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:CustomerRefNumExist(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;

        }

        public static string GetNextInvoiceNumber(bool ForStorage)
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            Int64 nextInvoiceNum = 0;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                if (ForStorage)
                {
                    Cmd.CommandText = "select case isnumeric(Max(convert(bigint,InvoiceID))) when 0 then 1 else Max(convert(bigint,InvoiceID)) + 1 end from dbo.StorageInvoices";
                }
                else
                {
                    Cmd.CommandText = "select case isnumeric(Max(convert(bigint,InvoiceID))) when 0 then 1 else Max(convert(bigint,InvoiceID)) + 1 end from dbo.MFIAR";
                }
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                nextInvoiceNum = (long)Cmd.ExecuteScalar();
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetNextInvoiceNumber(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return nextInvoiceNum.ToString();
        }

        public static string GetNextPayCommisionNum()
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            Int64 nextInvoiceNum = 0;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "select case isnumeric(Max(CommNumber)) when 0 then 1 else Max(CommNumber) + 1 end from dbo.PayRollTrack";

                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                nextInvoiceNum = (long)Cmd.ExecuteScalar();
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetNextPayCommisionNum(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return nextInvoiceNum.ToString();
        }

        public static string GetNextDriverNum()
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            Int64 nextInvoiceNum = 0;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "select case isnumeric(Max(DriverNum)) when 0 then 1 else Max(DriverNum) + 1 end from dbo.Drivers";

                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                nextInvoiceNum = (long)Cmd.ExecuteScalar();
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetNextDriverNum(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return nextInvoiceNum.ToString();
        }

        public static bool InvReleaseOrVoid(string invID, string Reason,
                                            bool voidFlag, string createdBy, CustomersIndicator mycustomer)
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool bResult = true;
            try
            {
                con.Open();
                Cmd = new SqlCommand();
                if (mycustomer == CustomersIndicator.MFI)
                {
                    Cmd.CommandText = "SetInvReleasedOrVoid";
                }
                else
                {
                    Cmd.CommandText = "SetGlobalInvReleasedOrVoid";
                }
                Cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@InvID";
                parm.Value = invID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CreatedBy";
                parm.Value = createdBy;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Reason";
                parm.Value = Reason;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@voidFlag";
                parm.Value = voidFlag;
                Cmd.Parameters.Add(parm);

                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:InvReleaseOrVoid(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;
        }

        public static bool InvReleaseOrVoid(string invID, string Reason,
                                          bool voidFlag, string createdBy, CustomersIndicator mycustomer, ref SqlTransaction tr)
        {
            SqlCommand Cmd;

            bool bResult = true;
            try
            {

                Cmd = new SqlCommand();
                if (mycustomer == CustomersIndicator.MFI)
                {
                    Cmd.CommandText = "SetInvReleasedOrVoid";
                }
                else
                {
                    Cmd.CommandText = "SetGlobalInvReleasedOrVoid";
                }
                Cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@InvID";
                parm.Value = invID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CreatedBy";
                parm.Value = createdBy;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Reason";
                parm.Value = Reason;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@voidFlag";
                parm.Value = voidFlag;
                Cmd.Parameters.Add(parm);

                Cmd.Connection = tr.Connection;
                Cmd.Transaction = tr;
                Cmd.ExecuteNonQuery();

            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:InvReleaseOrVoid(); "));
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

        public struct structStorageInvoice
        {
            public string InvoiceID;
            public string OrderNumber;
            public DateTime InvoicePrintedDate;
            public Guid CreatedBy;
            public string InvUNIT;
            public string InvQTY;
            public string RatePerWeek;
            public string InvAmount;
            public string InvSKIDS;
            public DateTime InvDateFrom;
            public DateTime InvDateTo;
            public string InvGSTAmt;
            public string TermNums;
            public string OrderStorageCost;
            public DateTime ServiceDate;
            public string CustomerNum;
        }

        public static string GST()
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "select GST from CONFIG";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                MFIGST = Cmd.ExecuteScalar().ToString();
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GST(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return MFIGST;

        }

        public static string ClientName()
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "select Name from CustomerInfo";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                MFIGST = Cmd.ExecuteScalar().ToString();
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GST(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return MFIGST;

        }

        public static bool SaveCustomerWebInfo(ref DataTable departs, Int64 customerNum,
                                                    string userName, string pwd)
        {
            bool bResult = true;
            SqlDataAdapter dataAdp = new SqlDataAdapter();
            SqlCommand Cmd = null;
            SqlConnection con = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlTransaction trn = null;
            try
            {
                SqlParameter param = null;
                con.Open();
                trn = con.BeginTransaction();
                dataAdp = new SqlDataAdapter();
                Cmd = new SqlCommand();
                Cmd.Connection = con;
                Cmd.Transaction = trn;

                param = new SqlParameter();
                param.ParameterName = "@CustomerNum";
                param.Value = customerNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@LoginName";
                param.Value = userName;
                Cmd.Parameters.Add(param);


                param = new SqlParameter();
                param.ParameterName = "@WebPassword";
                param.Value = pwd;
                Cmd.Parameters.Add(param);
                Cmd.CommandText = "UpdateCustomerWebInfo";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.ExecuteNonQuery();

                Cmd.Parameters.Clear();

                param = new SqlParameter();
                param.ParameterName = "@CustomerNum";
                param.Value = customerNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DepartName";
                param.SourceColumn = "Department";
                Cmd.Parameters.Add(param);

                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "InsertWebDepartment";

                dataAdp.InsertCommand = Cmd;

                SqlCommand updateCmd = new SqlCommand();
                updateCmd.Connection = con;
                updateCmd.Transaction = trn;

                param = new SqlParameter();
                param.ParameterName = "@CustomerNum";
                param.Value = customerNum;
                updateCmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DepartName";
                param.SourceColumn = "Department";
                updateCmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Active";
                param.SourceColumn = "Active";
                updateCmd.Parameters.Add(param);

                updateCmd.CommandType = CommandType.StoredProcedure;
                updateCmd.CommandText = "UpdateWebDepartment";
                dataAdp.UpdateCommand = updateCmd;
                dataAdp.DeleteCommand = updateCmd;

                dataAdp.Update(departs);

                trn.Commit();
            }
            catch (Exception ex1)
            {
                bResult = false;
                if (con.State == ConnectionState.Open)
                {
                    trn.Rollback();
                }
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SaveCustomerWebInfo(); "));
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
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
                con = null;

                if (Cmd != null)
                {
                    Cmd = null;
                    //Cmd.Dispose();
                }

                if (dataAdp != null)
                {
                    dataAdp = null;
                    //dataAdp.Dispose();
                }
            }
            return bResult;
        }

        public static bool setInvoiceUnpiad(string invoiceID)
        {
            bool bResult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SetInvoiceUnpaid";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();

            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:setInvoiceUnpiad(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return bResult;

        }

        public static void SaveGST(string newGST)
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "Update CONFIG SET GST = '" + newGST + "'";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                Cmd.ExecuteNonQuery();
                con.Close();
                MFIGST = newGST;
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SaveGST(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            //            return MFIGST;

        }

        public static DataTable CustomerAllDeliveries(string custNum, string fromDate, string toDate)
        {
            DataTable AllCustDeliveries = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@CustNum";
                parm.Value = custNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = fromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = toDate;
                Cmd.Parameters.Add(parm);
                AllCustDeliveries = new DataTable();
                con.Open();
                Cmd.CommandText = "GetNonActiveDeliveries";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllCustDeliveries);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:CustomerAllDeliveries(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return AllCustDeliveries;

        }

        public static DataTable GetAllOrders(string custNum, DateTime fromDate, DateTime toDate, RecordFetchFlags fetchFlag)
        {
            DataTable dtGetAllOrders = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@CustNum";
                parm.Value = custNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = fromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = toDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FetchFlag";
                parm.Value = fetchFlag;
                Cmd.Parameters.Add(parm);


                con.Open();
                Cmd.CommandText = "GetAllOrders";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dtGetAllOrders = new DataTable();
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(dtGetAllOrders);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetAllOrders(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return dtGetAllOrders;

        }

        public static DataTable CustomerInvoicesForRecreate(string custNum, string fromDate, string toDate)
        {
            DataTable tblInfo = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@CustNum";
                parm.Value = custNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FromDate";
                parm.Value = fromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = toDate;
                Cmd.Parameters.Add(parm);

                tblInfo = new DataTable();
                con.Open();

                Cmd.CommandText = "GetCustomerInvoicesByDate";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(tblInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:CustomerInvoicesForRecreate(); "));
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
                Cmd = null;
            }
            return tblInfo;

        }

        public static DataTable CustomerAllInvoices(string custNum)
        {
            DataTable tblInfo = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@CustNum";
                parm.Value = custNum;
                Cmd.Parameters.Add(parm);


                tblInfo = new DataTable();
                con.Open();

                Cmd.CommandText = "select InvoiceDate,CustomerNum,InvoiceID,'Re-Print' as RePrint,'Re-Print With No Pending' as RePrintWithNoPending ";
                Cmd.CommandText = Cmd.CommandText + " from MFIAR	where ISNull(Void,0) = 0 and (CustomerNum = " + custNum + " or " + custNum + "= -9999)";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(tblInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:CustomerInvoicesForRecreate(); "));
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
                Cmd = null;
            }
            return tblInfo;

        }

        public static string GetInvoiceDate(string InvoiceID)
        {
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;
            string val = string.Empty;
            try
            {
                Cmd = new SqlCommand();
                con.Open();
                Cmd.CommandText = "select InvoiceDate from MFIAR Where InvoiceID = " + InvoiceID;
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                object o = Cmd.ExecuteScalar();
                if (o != null) val = o.ToString();
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetInvoiceDate(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return val;

        }


        public static DataTable GetAllOrdersForInvoice(string InvoiceID)
        {
            DataTable dtGetAllOrders = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();

                parm = new SqlParameter();
                parm.ParameterName = "@InvoiceID";
                parm.Value = InvoiceID;
                Cmd.Parameters.Add(parm);
                con.Open();
                Cmd.CommandText = "GetAllOrdersForInvoice";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dtGetAllOrders = new DataTable();
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(dtGetAllOrders);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetAllOrdersForInvoice(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return dtGetAllOrders;

        }

        public static DateTime GetASCGDATE(string sDate)
        {
            DateTime _date = DateTime.Today;
            try
            {
                string[] dtParts = sDate.Split('/');
                //MM/DD/YYYY fromat
                _date = new DateTime(int.Parse(dtParts[2]), int.Parse(dtParts[0]), int.Parse(dtParts[1]));
            }
            catch
            {

            }
            return _date;
        }

        public static DataTable CustomerWayBills(string custNum, string fromDate, string toDate)
        {
            DataTable CustomerWB = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@CustNum";
                parm.Value = custNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@FromDateTime";
                parm.Value = fromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDateTime";
                parm.Value = toDate;
                Cmd.Parameters.Add(parm);
                CustomerWB = new DataTable();
                con.Open();
                Cmd.CommandText = "RPTGetWayBillSummary";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(CustomerWB);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:CustomerWayBills(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return CustomerWB;

        }

        public static DataTable GetAllMyCheques(string _FromDate, string _ToDate)
        {

            DataTable dtCheques = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();

                parm = new SqlParameter();
                parm.ParameterName = "@Fromdate";
                parm.Value = _FromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = _ToDate;
                Cmd.Parameters.Add(parm);
                dtCheques = new DataTable();
                con.Open();

                Cmd.CommandText = "RPTCheques";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(dtCheques);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetAllMyCheques(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return dtCheques;
        }

        public static DataTable GetDriversDeliveries(string driverNum, string fromDate, string toDate)
        {
            DataTable drvDeliveries = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;

            try
            {
                Cmd = new SqlCommand();

                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@DriverNum";
                parm.Value = driverNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Fromdate";
                parm.Value = fromDate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ToDate";
                parm.Value = toDate;
                Cmd.Parameters.Add(parm);
                drvDeliveries = new DataTable();
                con.Open();
                Cmd.CommandText = "RPTGetDriverWBList";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(drvDeliveries);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("ASCGBilling:GetDriversDeliveries(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return drvDeliveries;

        }

        public static DataTable ALLGlobalDeliveryOrders()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            //SqlParameter parm = new SqlParameter();
            DataTable GlobalDeliveries = null; ;
            //parm.ParameterName = "@Flag";
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                GlobalDeliveries = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                //Cmd.Parameters.Add(parm);
                Cmd.CommandText = "GetGlobalDeliveries";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(GlobalDeliveries);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ALLGlobalDeliveryOrders(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return GlobalDeliveries;

        }

        private static DataTable ALLDispatchBoardDeliveryOrders(string SPName)
        {
            DataTable GlobalDeliveries = null;
            try
            {
                using (SqlDataAdapter dataAdp = new SqlDataAdapter())
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        using (SqlConnection con = new SqlConnection())
                        {
                            con.ConnectionString = GlobalConnectionstring;
                            GlobalDeliveries = new DataTable();
                            con.Open();
                            Cmd.CommandText = SPName; // "GetGlobalDispatchBoardDeliveries";
                            Cmd.CommandType = CommandType.StoredProcedure;
                            Cmd.Connection = con;
                            dataAdp.SelectCommand = Cmd;
                            dataAdp.Fill(GlobalDeliveries);
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ALLDispatchBoardDeliveryOrders(); "));
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
            return GlobalDeliveries;
        }

        public static DataTable ALLGlobalDispatchBoardDeliveryOrders()
        {
            return ALLDispatchBoardDeliveryOrders("GetGlobalDispatchBoardDeliveries");
        }

        public static DataTable ALLMFIDispatchBoardDeliveryOrders()
        {
            return ALLDispatchBoardDeliveryOrders("GetMFIDispatchBoardDeliveries");
        }

        public static DataTable GlobalCancelled(ref CustomersIndicator myCustomer)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllDeliveryRecords = null;
                AllDeliveryRecords = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetAllGlobalCancelledRecords";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllDeliveryRecords);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GlobalCancelled(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return AllDeliveryRecords;
        }

        public static DataTable ALLDeliveryOrders(bool bNormalView, ref CustomersIndicator myCustomer)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm = new SqlParameter();
            parm.ParameterName = "@Flag";
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllDeliveryRecords = null;
                AllDeliveryRecords = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                if (bNormalView)
                {
                    parm.Value = "1";
                }
                else
                {
                    parm.Value = "2";
                }
                Cmd.Parameters.Add(parm);
                if (myCustomer == CustomersIndicator.MFI)
                {
                    Cmd.CommandText = "SP_GetAllDeliverierRecords";
                }
                else
                {
                    Cmd.CommandText = "GetAllGlobalDeliveriesRecords";
                }
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllDeliveryRecords);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllDelievryOrders(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return AllDeliveryRecords;

        }


        public static DataTable ALLGlobalDeliveryOrders(string Flag, DateTime FromDate, DateTime ToDate, ref CustomersIndicator myCustomer)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter parm = new SqlParameter();
            parm.ParameterName = "@Flag";
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            try
            {
                AllDeliveryRecords = null;
                AllDeliveryRecords = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                parm.Value = Flag;
                parm.ParameterName = "@Flag";
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.Value = ToDate.ToString("MM/dd/yyyy");
                parm.ParameterName = "@ToDate";
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.Value = FromDate.ToString("MM/dd/yyyy");
                parm.ParameterName = "@FromDate";
                Cmd.Parameters.Add(parm);

                if (myCustomer == CustomersIndicator.MFI)
                {
                    Cmd.CommandText = "GetAllMFIDeliveriesRecords";
                }
                else
                {
                    Cmd.CommandText = "GetAllGlobalDeliveriesRecords";
                }
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllDeliveryRecords);
                con.Close();
            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllDelievryOrders(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return AllDeliveryRecords;
        }


        public static DataTable GetDeliveryOrderForMain(string OrderNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                DeliveryRecForMain = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetDeliveryRecForMain";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Parameters.Add("@OrderNum", SqlDbType.VarChar);
                Cmd.Parameters[0].Value = OrderNum;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(DeliveryRecForMain);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetDeliveryOrderForMain(); "));
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
                if (con != null) con.Close();
                Cmd = null;
            }
            return DeliveryRecForMain;

        }

        public static DataTable GetAllDeliveryTypes()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllDeliveryTypes = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetAllDeliveryType";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllDeliveryTypes);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetAllDeliveryTypes(); "));
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
                Cmd = null;
            }
            return AllDeliveryTypes;

        }

        public static string FormatASCGDate(string dateString, bool isInDDMMYYFormat, bool bIncludeTime)
        {
            string tmpDateStaring = string.Empty;
            int dtYear;
            int dtMonth;
            int dtDay;
            string yearPart = string.Empty;
            string timePart = string.Empty;
            try
            {
                string[] dtParts = dateString.Split('/');

                if (dtParts.Length == 3)
                {
                    if (isInDDMMYYFormat)
                    {
                        dtDay = int.Parse(dtParts[0]);
                        dtMonth = int.Parse(dtParts[1]);
                    }
                    else
                    {
                        dtDay = int.Parse(dtParts[1]);
                        dtMonth = int.Parse(dtParts[0]);
                    }
                    yearPart = dtParts[2];

                    if (yearPart.Trim().Length > 4)
                    {
                        dtYear = int.Parse(yearPart.Substring(0, 4));
                        try
                        {
                            timePart = DateTime.Parse(yearPart.Substring(4, yearPart.Length - 4)).ToString("hh:mm tt");
                        }
                        catch
                        {
                            timePart = string.Empty;
                        }

                    }
                    else
                    {
                        dtYear = int.Parse(yearPart);
                    }

                    tmpDateStaring = (new DateTime(dtYear, dtMonth, dtDay)).ToString("dd-MMM-yy");
                    if (bIncludeTime) tmpDateStaring = tmpDateStaring + " " + timePart;

                }
            }
            catch (Exception ex1)
            {
                tmpDateStaring = string.Empty;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:FormatASCGGate(); "));
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
            return tmpDateStaring.Trim();

        }

        public static DataSet GetWayBillFinderInfo(string WayBillNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet wayBillInfo = new DataSet();
            try
            {
                wayBillInfo = new DataSet();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalWBFinder";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.AddWithValue("@WayBillNum", WayBillNum);
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(wayBillInfo);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetWayBillFinderInfo(); "));
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
                Cmd = null;
            }
            return wayBillInfo;

        }


        public static DataTable GetCustomersOnly()
        {
            return GetCustomersOnly(false, string.Empty);

        }
        public static DataTable GetAddressesOnly()
        {
            return GetCustomersAddress(false, string.Empty, 2, 3);

        }

        public static DataTable GetCustomersOnly(bool AddBlankCustomer, string BlankName)
        {
            return GetCustomersAddress(AddBlankCustomer, BlankName, 1, 1);

        }

        public static DataTable GetCustomersAndAddress(bool AddBlankCustomer, string BlankName)
        {
            return GetCustomersAddress(AddBlankCustomer, BlankName, 3, 3);

        }

        private static DataTable GetCustomersAddress(bool AddBlankCustomer, string BlankName, int AddressFlag, int ActiveFlag)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myCustomers = null;
            try
            {
                myCustomers = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetCustomersNumName";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Parameters.AddWithValue("@AddressFlag", AddressFlag);
                Cmd.Parameters.AddWithValue("@ActiveFlag", ActiveFlag);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myCustomers);
                con.Close();
                if (AddBlankCustomer)
                {
                    DataRow rw = myCustomers.NewRow();
                    rw["Name"] = BlankName;
                    rw["CustomerNum"] = Global.blankCuromerNum;
                    //rw["Active"] = false;
                    myCustomers.Rows.InsertAt(rw, 0);
                }
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomersOnly(); "));
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
                Cmd = null;
            }
            return myCustomers;

        }

        public static DataRow GetCustomerMailingLoc(string customerNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myLoc = null;
            DataRow rw = null;
            try
            {
                if (!string.IsNullOrEmpty(customerNum))
                {
                    myLoc = new DataTable();
                    con.Open();
                    Cmd = new SqlCommand();
                    Cmd.CommandText = "GetCustomerMailingLoc";
                    SqlParameter parm = new SqlParameter();
                    parm.ParameterName = "@customerNum";
                    parm.Value = customerNum;
                    Cmd.Parameters.Add(parm);
                    Cmd.CommandType = CommandType.StoredProcedure;
                    Cmd.Connection = con;
                    dataAdp = new SqlDataAdapter(Cmd);
                    dataAdp.Fill(myLoc);
                    con.Close();
                    if (myLoc.Rows.Count > 0)
                    {
                        rw = myLoc.Rows[0];
                    }

                }

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomerMailingLoc(); "));
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
                Cmd = null;
            }
            return rw;

        }

        public static string GetOrderNumFromWayBillNumber(string wayBillNumber)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myData = null;
            string orderNum = blankOrderNum.ToString();

            try
            {
                myData = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetOrderNumFromWayBill";
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@wayBill";
                parm.Value = wayBillNumber;
                Cmd.Parameters.Add(parm);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myData);
                con.Close();
                if (myData.Rows.Count > 0)
                {
                    orderNum = myData.Rows[0][0].ToString();
                }

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetOrderWayBillNumber(); "));
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
                Cmd = null;
            }
            return orderNum;

        }


        public static DataRow GetPaybleRow(string customerNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myLoc = null;
            DataRow rw = null;
            try
            {
                if (!string.IsNullOrEmpty(customerNum))
                {
                    myLoc = new DataTable();
                    con.Open();
                    Cmd = new SqlCommand();
                    Cmd.CommandText = "GetPaybleRow";
                    SqlParameter parm = new SqlParameter();
                    parm.ParameterName = "@customerNum";
                    parm.Value = customerNum;
                    Cmd.Parameters.Add(parm);
                    Cmd.CommandType = CommandType.StoredProcedure;
                    Cmd.Connection = con;
                    dataAdp = new SqlDataAdapter(Cmd);
                    dataAdp.Fill(myLoc);
                    con.Close();
                    if (myLoc.Rows.Count > 0)
                    {
                        rw = myLoc.Rows[0];
                    }
                }

            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetPaybleRow(); "));
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
                Cmd = null;
            }
            return rw;

        }

        public static DataTable ALLDeliveriesBills(string fromdate, string todate, string customerNum)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter param;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllDeliveriesBills = null;
                AllDeliveriesBills = new DataTable();

                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetAllBillingDeliveries";
                Cmd.CommandType = CommandType.StoredProcedure;
                param = new SqlParameter();
                param.ParameterName = "@FromDate";
                param.Value = fromdate;
                Cmd.Parameters.Add(param);
                param = new SqlParameter();
                param.ParameterName = "@ToDate";
                param.Value = todate;
                Cmd.Parameters.Add(param);
                param = new SqlParameter();
                param.ParameterName = "@CustomerNum";
                param.Value = customerNum;
                Cmd.Parameters.Add(param);
                con.Open();
                Cmd.Connection = con;

                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllDeliveriesBills);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ALLDeliveriesBills(); "));
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
                Cmd = null;
            }
            return AllDeliveriesBills;

        }

        public static DataTable ALLBillingDrivers(bool reload)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllBillingDeliveries = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetAllBillingDrivers";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllBillingDeliveries);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ALLBillingDrivers(); "));
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
                Cmd = null;
            }
            return AllBillingDeliveries;

        }

        public static DataTable getALLBillingDeliveries()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllBillingDeliveries = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetAllBillingDeliveries";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllBillingDeliveries);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:getALLBillingDeliveries(); "));
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
                Cmd = null;
            }
            return AllDeliveryRecords;

        }

        public static DataTable GetAllPreschedule()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable preschedule = null;
            try
            {
                preschedule = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetAllPreschedule";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(preschedule);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetAllPreschedule(); "));
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
                Cmd = null;
            }
            return preschedule;

        }
        public static DataSet getPriorities(string usrID)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet priorities = null;

            try
            {
                priorities = new DataSet();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetPriorities";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Parameters.AddWithValue("@UserID", usrID);
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(priorities);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:getPriorities(); "));
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
                Cmd = null;
            }
            return priorities;

        }

        public static DataTable ALLStorageOrders()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllStorageRecords = null;
                AllStorageRecords = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetAllStorageRecords";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllStorageRecords);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllStorageRecords(); "));
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
                Cmd = null;
            }
            return AllStorageRecords;

        }

        public static DataTable ALLNewStorageOrders()
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                AllStorageRecords = null;
                AllStorageRecords = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetAllNewStorageRecords";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(AllStorageRecords);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllStorageRecords(); "));
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
                Cmd = null;
            }
            return AllStorageRecords;

        }

        public static DataTable AllSameDayCityRates()
        {

            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                SameDayCitiesRates = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_SelectSameDayRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(SameDayCitiesRates);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllSameDayCityRates(); "));
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
                Cmd = null;
            }
            return SameDayCitiesRates;

        }

        public static DataTable AllCitiesRates()
        {

            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                CitiesRates = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_SelectCityRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(CitiesRates);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllCitiesRates(); "));
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
                Cmd = null;
            }
            return CitiesRates;

        }

        public static DataSet GetCustomerWebInfo(Int64 customerNum)
        {
            SqlDataAdapter dataAdp = new SqlDataAdapter();
            SqlCommand Cmd = null;
            SqlConnection con = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataSet myInfo = null;
            try
            {

                myInfo = new DataSet();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetCustomerWebInfo";
                SqlParameter param = new SqlParameter();
                param.Value = customerNum;
                param.ParameterName = "@CustomerNum";
                Cmd.Parameters.Add(param);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomerWebInfo(); "));
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
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
                con = null;

                if (Cmd != null)
                {
                    Cmd = null;
                    //Cmd.Dispose();
                }

                if (dataAdp != null)
                {
                    dataAdp = null;
                    //dataAdp.Dispose();
                }
            }

            return myInfo;
        }

        public static DataTable GetCustomerDetails(string customerNum)
        {
            SqlDataAdapter dataAdp = new SqlDataAdapter();
            SqlCommand Cmd = null;
            SqlConnection con = null;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable myInfo = null;
            try
            {

                myInfo = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetWebCustomerDetails";
                SqlParameter param = new SqlParameter();
                param.Value = customerNum;
                param.ParameterName = "@CustomerNum";
                Cmd.Parameters.Add(param);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(myInfo);
                con.Close();
            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomerDetails(); "));
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
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
                con = null;

                if (Cmd != null)
                {
                    Cmd = null;
                    //Cmd.Dispose();
                }

                if (dataAdp != null)
                {
                    dataAdp = null;
                    //dataAdp.Dispose();
                }
            }

            return myInfo;
        }

        public static string NextGetAutoGenerateWB()
        {
            string WB = string.Empty;


            try
            {
                using (SqlCommand Cmd = new SqlCommand())
                {
                    Cmd.CommandText = "select Max(Cast(dbo.UDF_ParseAlphaChars(WayBillNumber) as integer)) from DeliveryOrders";
                    Cmd.CommandType = CommandType.Text;
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = GlobalConnectionstring;
                        Cmd.Connection = con;
                        Cmd.Connection.Open();
                        object val = Cmd.ExecuteScalar();
                        WB = (val == null || Int32.Parse(val.ToString()) < 300000) ? "300000" : (Int32.Parse(val.ToString()) + 1).ToString();
                    }
                }
            }

            catch (Exception ex1)
            {
                WB = "300000";
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:NextGetAutoGenerateWB(); "));
                mErrorInfo.Append("Exception Message: ");
                mErrorInfo.Append(ex1.Message);
                Global.LogInEventLogger(mErrorInfo.ToString());
                mErrorInfo = null;
            }
            return WB;

        }
        public static DataTable GetCustomerAddress(string customerNum)
        {
            using (DataTable myInfo = new DataTable())
            {
                try
                {
                    using (SqlCommand Cmd = new SqlCommand())
                    {
                        Cmd.CommandText = "GetAddressDetails";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlConnection con = new SqlConnection())
                        {
                            con.ConnectionString = GlobalConnectionstring;
                            Cmd.Connection = con;
                            using (SqlDataAdapter dataAdp = new SqlDataAdapter(Cmd))
                            {
                                SqlParameter param = new SqlParameter();
                                param.Value = customerNum;
                                param.ParameterName = "@CustomerNum";
                                Cmd.Parameters.Add(param);
                                Cmd.Connection.Open();
                                dataAdp.Fill(myInfo);
                            }
                        }
                    }
                }

                catch (Exception ex1)
                {
                    mErrorInfo = new StringBuilder("Error Information");
                    mErrorInfo.Append(Environment.NewLine);
                    mErrorInfo.Append("Function Name: ");
                    mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomerAddress(); "));
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
                return myInfo;
            }
        }

        public static SqlDataAdapter ServiceRates(ref DataTable SDRates)
        {

            SqlDataAdapter dataAdp = new SqlDataAdapter();
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter param;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_ServiceRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.SelectCommand = Cmd;

                //Insert Command
                Cmd = new SqlCommand();
                param = new SqlParameter();
                param.ParameterName = "@CityName";
                param.SourceColumn = "CityName";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@AfterFirstSkidRate";
                param.SourceColumn = "AfterFirstSkidRate";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@FirstSkidRate";
                param.SourceColumn = "FirstSkidRate";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Servicetype";
                param.SourceColumn = "Servicetype";
                Cmd.Parameters.Add(param);

                Cmd.CommandText = "SP_InsertServicesRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.InsertCommand = Cmd;

                //Update Command
                Cmd = new SqlCommand();
                param = new SqlParameter();
                param.ParameterName = "@CityName";
                param.SourceColumn = "CityName";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@AfterFirstSkidRate";
                param.SourceColumn = "AfterFirstSkidRate";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Servicetype";
                param.SourceColumn = "Servicetype";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@FirstSkidRate";
                param.SourceColumn = "FirstSkidRate";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ID";
                param.SourceColumn = "ID";
                Cmd.Parameters.Add(param);

                Cmd.CommandText = "SP_UpdateServicesRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.UpdateCommand = Cmd;


                //Delete Command
                Cmd = new SqlCommand();
                param = new SqlParameter();
                param.ParameterName = "@ID";
                param.SourceColumn = "ID";
                Cmd.Parameters.Add(param);

                Cmd.CommandText = "SP_DeleteServicesRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.DeleteCommand = Cmd;


                dataAdp.Fill(SDRates);
                //con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:ServiceRates(); "));
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
                //con.Close();
                //Cmd = null;
            }
            return dataAdp;

        }

        public static SqlDataAdapter SameDayRates(ref DataTable SDRates)
        {

            SqlDataAdapter dataAdp = new SqlDataAdapter();
            SqlCommand Cmd;
            SqlConnection con;
            SqlParameter param;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_SameDayRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.SelectCommand = Cmd;

                //Insert Command
                Cmd = new SqlCommand();
                param = new SqlParameter();
                param.ParameterName = "@Skids";
                param.SourceColumn = "Skids";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@CityName";
                param.SourceColumn = "CityName";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Rates";
                param.SourceColumn = "Rates";
                Cmd.Parameters.Add(param);

                Cmd.CommandText = "SP_InsertSameDayRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.InsertCommand = Cmd;

                //Update Command
                Cmd = new SqlCommand();
                param = new SqlParameter();
                param.ParameterName = "@Skids";
                param.SourceColumn = "Skids";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@CityName";
                param.SourceColumn = "CityName";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Rates";
                param.SourceColumn = "Rates";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@ID";
                param.SourceColumn = "ID";
                Cmd.Parameters.Add(param);

                Cmd.CommandText = "SP_UpdateSameDayRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.UpdateCommand = Cmd;


                //Delete Command
                Cmd = new SqlCommand();
                param = new SqlParameter();
                param.ParameterName = "@ID";
                param.SourceColumn = "ID";
                Cmd.Parameters.Add(param);

                Cmd.CommandText = "SP_DeleteSameDayRates";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp.DeleteCommand = Cmd;


                dataAdp.Fill(SDRates);
                //con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SameDayRates(); "));
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
                if (con != null) con.Close();
            }
            return dataAdp;

        }


        public static DataTable AllCities(bool reLoad)
        {
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                Cities = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_SelectAllCities";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(Cities);
                DataRow rw = Cities.NewRow();
                rw["CityName"] = string.Empty;
                rw["CityName"] = string.Empty;
                Cities.Rows.InsertAt(rw, 0);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllCities(); "));
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
                Cmd = null;
            }
            return Cities;

        }


        public static DataTable AllCities()
        {
            return AllCities(false);
        }

        public static void LogInEventLogger(string ErrMsg)
        {
            string sSource;
            string sLog;
            sSource = "Global";
            sLog = "Exception";
            try
            {

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                EventLog.WriteEntry(sSource, ErrMsg,
                   EventLogEntryType.Error, 234);

            }
            catch
            {

            }

        }

        public static void AddParameter(ref SqlCommand cmd, string parmName)
        {
            SqlParameter parm;
            parm = new SqlParameter();
            parm.ParameterName = "@" + parmName;
            parm.SourceColumn = parmName;
            cmd.Parameters.Add(parm);
        }

        public static DataTable GetInvoiceWayBills(string InvoiceID, string invType)
        {
            SqlDataAdapter dataAdp = null;
            SqlCommand Cmd = null;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            DataTable dtwayBills = null;
            try
            {
                dtwayBills = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetInvoiceWayBills";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                Cmd.Parameters.Add("@InvoiceID", SqlDbType.VarChar);
                Cmd.Parameters[0].Value = InvoiceID;
                Cmd.Parameters.Add("@InvType", SqlDbType.VarChar);
                Cmd.Parameters[1].Value = invType;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(dtwayBills);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetInvoiceWayBills(); "));
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
                Cmd = null;

            }
            return dtwayBills;

        }

        public static List<ClassPaymentType> GetPaymentTypes(int fordeductionFlag)
        {
            ClassPaymentType clsPayment;
            List<ClassPaymentType> lst = new List<ClassPaymentType>();
            //clsPayment = new ClassPaymentType();
            //clsPayment.PaymentTypeText = "Other";
            //clsPayment.PaymentTypeValue = "ORDOTH";
            //lst.Add(clsPayment);

            if (fordeductionFlag == 1)
            {
                clsPayment = new ClassPaymentType();
                clsPayment.PaymentTypeText = "Advance";
                clsPayment.PaymentTypeValue = "Advance";
                lst.Add(clsPayment);

                //clsPayment = new ClassPaymentType();
                //clsPayment.PaymentTypeText = "Insurance";
                //clsPayment.PaymentTypeValue = "Insurance";
                //lst.Add(clsPayment);

            }
            else
            {

                clsPayment = new ClassPaymentType();
                clsPayment.PaymentTypeText = "Order %";
                clsPayment.PaymentTypeValue = "ORDPER";
                lst.Add(clsPayment);

                clsPayment = new ClassPaymentType();
                clsPayment.PaymentTypeText = "Fuel Surchrg";
                clsPayment.PaymentTypeValue = "FUELSUR";
                lst.Add(clsPayment);

                clsPayment = new ClassPaymentType();
                clsPayment.PaymentTypeText = "Bonus";
                clsPayment.PaymentTypeValue = "Bonus";
                lst.Add(clsPayment);

                clsPayment = new ClassPaymentType();
                clsPayment.PaymentTypeText = "Service(s) Charge";
                clsPayment.PaymentTypeValue = "SERCHRG";
                lst.Add(clsPayment);
            }

            return lst;
        }

        public static DataTable AllSkidsWeights()
        {
            DataTable skdRates = null;
            SqlDataAdapter dataAdp;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;

            try
            {
                skdRates = new DataTable();
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SELECT [SkidNum],[UpperWt],LastModifiedBy,LastModifiedDateTime FROM [dbo].[SkidWeights]";
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = con;
                dataAdp = new SqlDataAdapter(Cmd);
                dataAdp.Fill(skdRates);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:AllSkidsWeights(); "));
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
                Cmd = null;
            }
            return skdRates;

        }

        public static DataTable GetDriverHours(DateTime pSelectedDate, string pDriverNum)
        {
            //bool bresult = true;
            DataTable drvHours = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter param;
            SqlDataAdapter adap;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetDriverHours";
                Cmd.CommandType = CommandType.StoredProcedure;
                param = new SqlParameter();
                param.ParameterName = "@SelDate";
                param.Value = pSelectedDate.ToString("MM/dd/yyyy");
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DriverNum";
                param.Value = pDriverNum;
                Cmd.Parameters.Add(param);

                Cmd.Connection = con;
                Cmd.Connection.Open();

                adap = new SqlDataAdapter();
                adap.SelectCommand = Cmd;
                adap.Fill(drvHours);
                con.Close();
            }
            catch (Exception ex1)
            {
                //bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetDriverHours(); "));
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
                Cmd = null;
                con.Dispose();

            }
            return drvHours;

        }

        public static DataTable GetDriversForTimeEntry()
        {

            DataTable drvHours = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            //SqlParameter param;
            SqlDataAdapter adap;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_GetDriversForTimeEntry";
                Cmd.CommandType = CommandType.StoredProcedure;

                Cmd.Connection = con;
                Cmd.Connection.Open();

                adap = new SqlDataAdapter();
                adap.SelectCommand = Cmd;
                adap.Fill(drvHours);
                con.Close();
            }
            catch (Exception ex1)
            {
                //bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetDriversForTimeEntry(); "));
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
                Cmd = null;
                con.Dispose();

            }
            return drvHours;

        }

        public static DataTable GetAllActiveEmployees()
        {

            DataTable dtEmployees = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            //SqlParameter param;
            SqlDataAdapter adap;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetAllActiveEmployees";
                Cmd.CommandType = CommandType.StoredProcedure;

                Cmd.Connection = con;
                Cmd.Connection.Open();

                adap = new SqlDataAdapter();
                adap.SelectCommand = Cmd;
                adap.Fill(dtEmployees);
                con.Close();
            }
            catch (Exception ex1)
            {
                //bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetAllActiveEmployees(); "));
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
                Cmd = null;
                con.Dispose();

            }
            return dtEmployees;

        }

        public static DataTable GetBillerDetails(string custNum)
        {
            DataTable custDetails = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            //SqlParameter param;
            SqlDataAdapter adap;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetInvoiceBillerDetails";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.Add(new SqlParameter("@custNum", custNum));
                Cmd.Connection = con;
                Cmd.Connection.Open();

                adap = new SqlDataAdapter();
                adap.SelectCommand = Cmd;
                adap.Fill(custDetails);
                con.Close();
            }
            catch (Exception ex1)
            {
                //bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetBillerDetails(); "));
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
                Cmd = null;
                con.Dispose();

            }
            return custDetails;

        }

        public static bool RemoveCustomers(string deleteSQL)
        {
            bool bResult = true;
            DataTable custDetails = new DataTable();
            SqlCommand Cmd = null;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlTransaction trn = null;
            try
            {
                con.Open();
                trn = con.BeginTransaction();

                Cmd = new SqlCommand();
                Cmd.CommandText = deleteSQL;
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = trn.Connection;

                Cmd.Transaction = trn;
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:RemoveCustomers(); "));
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
                if (trn != null)
                {
                    if (bResult) trn.Commit();
                    else trn.Rollback();
                    trn.Dispose();
                    Cmd.Dispose();

                }
            }
            return bResult;


        }
        public static DataTable GetCustomerList(string startingwith)
        {
            DataTable custDetails = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            //SqlParameter param;
            SqlDataAdapter adap;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetCustomersWithStartName";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Parameters.Add(new SqlParameter("@startingwith", startingwith));
                Cmd.Connection = con;
                Cmd.Connection.Open();

                adap = new SqlDataAdapter();
                adap.SelectCommand = Cmd;
                adap.Fill(custDetails);
                con.Close();
            }
            catch (Exception ex1)
            {
                //bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetCustomerList(); "));
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
                Cmd = null;
                con.Dispose();

            }
            return custDetails;

        }

        public static bool setDriverHours(ref DataTable drvHrs)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter param;
            SqlDataAdapter adap;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandType = CommandType.StoredProcedure;

                param = new SqlParameter();
                param.ParameterName = "@DriverNum";
                param.SourceColumn = "DriverNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@EntryDate";
                param.SourceColumn = "EntryDate";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Slot1Start";
                param.SourceColumn = "Slot1Start";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Slot2Start";
                param.SourceColumn = "Slot2Start";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Slot1End";
                param.SourceColumn = "Slot1End";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Slot2End";
                param.SourceColumn = "Slot2End";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@Remarks";
                param.SourceColumn = "Remarks";
                Cmd.Parameters.Add(param);

                adap = new SqlDataAdapter();

                if (drvHrs.Rows[0].RowState == DataRowState.Added)
                {
                    param = new SqlParameter();
                    param.ParameterName = "@CreatedDateTime";
                    param.SourceColumn = "CreatedDateTime";
                    Cmd.Parameters.Add(param);

                    param = new SqlParameter();
                    param.ParameterName = "@CreatedBy";
                    param.SourceColumn = "CreatedBy";
                    Cmd.Parameters.Add(param);

                    Cmd.CommandText = "InsertDriverTime";
                    adap.InsertCommand = Cmd;

                }
                else
                {
                    param = new SqlParameter();
                    param.ParameterName = "@LastModifiedDateTime";
                    param.SourceColumn = "LastModifiedDateTime";
                    Cmd.Parameters.Add(param);

                    param = new SqlParameter();
                    param.ParameterName = "@LastModifiedBy";
                    param.SourceColumn = "LastModifiedBy";
                    Cmd.Parameters.Add(param);

                    Cmd.CommandText = "UpdateDriverTime";
                    adap.UpdateCommand = Cmd;
                }


                Cmd.Connection = con;
                Cmd.Connection.Open();

                adap.Update(drvHrs);

                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:setDriverHours(); "));
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
                Cmd = null;
                con.Dispose();

            }
            return bresult;

        }


        public static bool SetOnBoardDateTime(string OnBoardDate, string OnBoardTime, string OrderNum, string DriverNum)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter param;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_SetOnBoardDateTime";
                Cmd.CommandType = CommandType.StoredProcedure;
                param = new SqlParameter();
                param.ParameterName = "@OnBoardDate";
                param.Value = OnBoardDate;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OnBoardTime";
                param.Value = OnBoardTime;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@OrderNum";
                param.Value = OrderNum;
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@DriverNum";
                param.Value = DriverNum;
                Cmd.Parameters.Add(param);


                Cmd.Connection = con;
                Cmd.Connection.Open();
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:SetOnBoardDateTime(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static bool InsertNewCity(string CityName)
        {
            bool bresult = true;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter param;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_InsertNewCity";
                Cmd.CommandType = CommandType.StoredProcedure;
                param = new SqlParameter();
                param.ParameterName = "@CityName";
                param.Value = CityName;
                Cmd.Parameters.Add(param);
                Cmd.Connection = con;
                Cmd.Connection.Open();
                Cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex1)
            {
                bresult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:InsertNewCity(); "));
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
                Cmd = null;
            }
            return bresult;

        }

        public static Int64 GetNewWebOrderNumber()
        {
            Int64 newOrdNum = 0;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            SqlParameter parm;
            try
            {
                Cmd = new SqlCommand();
                Cmd.CommandText = "InsertNewWebOrder";
                Cmd.CommandType = CommandType.StoredProcedure;
                parm = new SqlParameter();
                parm.ParameterName = "@RecNum";
                parm.DbType = DbType.Int64;
                parm.Direction = ParameterDirection.Output;
                Cmd.Parameters.Add(parm);
                Cmd.Connection = con;
                Cmd.Connection.Open();
                Cmd.ExecuteNonQuery();
                newOrdNum = Int64.Parse(Cmd.Parameters["@RecNum"].Value.ToString());
                con.Close();
            }
            catch (Exception ex1)
            {
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:GetNewWebOrderNumber(); "));
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
                Cmd = null;
            }
            return newOrdNum;

        }

        public static string CreateSalt(int size)
        {
            // Generate a cryptographic random number using the cryptographic
            // service provider
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            // Return a Base64 string representation of the random number
            return Convert.ToBase64String(buff);
        }

        public static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = String.Concat(pwd, salt);
            string hashedPwd =
                  FormsAuthentication.HashPasswordForStoringInConfigFile(
                                                       saltAndPwd, "SHA1");
            hashedPwd = String.Concat(hashedPwd, salt);
            return hashedPwd;
        }

        public static bool UpdateSkidsWeights(ref DataTable dtSkdWts)
        {
            SqlDataAdapter dataAdp;
            SqlParameter param;
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = GlobalConnectionstring;
            bool res = true;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "SP_UpdateSkidWeights";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;

                param = new SqlParameter();
                param.ParameterName = "@SkidNum";
                param.SourceColumn = "SkidNum";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@UpperWt";
                param.SourceColumn = "UpperWt";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@LastModifiedBy";
                param.SourceColumn = "LastModifiedBy";
                Cmd.Parameters.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@LastModifiedDateTime";
                param.SourceColumn = "LastModifiedDateTime";
                Cmd.Parameters.Add(param);

                dataAdp = new SqlDataAdapter();
                dataAdp.UpdateCommand = Cmd;
                dataAdp.Update(dtSkdWts);
                con.Close();
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("Global:UpdateSkidsWeights(); "));
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
                res = false;

            }
            finally
            {
                con.Close();
                Cmd = null;
            }
            return res;

        }


    }

    public class ClassPaymentType
    {
        private string mPaymentTypeText;
        private string mPaymentTypeValue;
        public string PaymentTypeValue
        {
            get
            {
                return mPaymentTypeValue;
            }
            set
            {
                mPaymentTypeValue = value;
            }
        }
        public string PaymentTypeText
        {
            get
            {
                return mPaymentTypeText;
            }
            set
            {
                mPaymentTypeText = value;
            }
        }

    }

}
