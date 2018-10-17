using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace GlobalWebAppBAL
{
    public struct stCustomer
    {
        public Guid orderCustID;
        public long CustomerNum;
        public string Name;
        public statusCode status;
        public string customerType;
        public string paymentBase;
        public decimal SurchargeRate;
        public decimal SpecialDiscount;
        public bool GSTApplicable;
        public int InvoiceDueDays;
        public string ContactName;
        public string ContactPhone;
    }
    public struct stLocation
    {
        public Guid LocationID;
        public string StreetLocation;
        public string Suite;
        public string City;
        public string Province;
        public string Country;
        public string PostalCode;
        public statusCode status;
        public DateTime DeliveryDate;
        public DateTime DeliveryTime;
        public string cityRegion;
    }
    public struct stOrderDetails
    {
        public string dispatchedAt;
        public string RdispatchedAt;
        public string OrderNumber;
        public string WayBillNumber;
        public List<GlobalAddService> AddServices;
        public string InvoiceNumber;
        public string InvoiceDate;
        public OrderStatusType OrderStatus;
        public statusCode status;
        public string CustomerReferenceNum;
        public string Department;
        public int QTY;
        public int waitTime;
        public string UNIT;
        public int SKID;
        public decimal ActualWeight;
        public string OrderedBy;
        public Guid ConsigneeLocationID;
        public Guid ShipperLocationID;
        public Guid BillingLocationID;
        public Guid CreatedBy;
        public Guid LastModifiedBy;
        public string ServiceType;
        public string PaymentMethod;
        public string PaymentMethodDescription;
        public string Memo;
        public bool PrintMemoOnInvoice;
        public int InvoiceDueDays;
        public Guid ReturnServiceID;
        public Int32 EmployeeNum;
        public decimal EmployeeCommission;
        public Guid ReturnSrvLocationID;
        public string VehicleMode;
        public decimal OrderSurchargeRate;
        public decimal OrderPercentageDiscount;
        public decimal GSTValue;
        public bool IsWebOrder;
        public string createdDateTime;
        public string AWBNum;
        public string CaroContainerNum;
        public string AppointeeName;
        public string ContactPhone1;
    }
    public struct stDeliveryDetails
    {
        public string pickedUpAt;
        public string passedoffAt;
        public string pod1At;
        public string pod2At;
        public string driverNum;
        public string driverName;
        public string driverPhone;
        public string rpickedUpAt;
        public string rpassedoffAt;
        public string rpod1At;
        public string rpod2At;
        public bool ReturnFlag;
        public string deliveryType;
    }
    public class Order
    {
        public stCustomer shipper;
        public stCustomer ReturnServiceConsignee;
        public stCustomer consignee;
        public stCustomer biller;
        public GlobalOrderCost orderCost;
        public GlobalOrderCost returnorderCost;
        public stOrderDetails ordDetails;
        public stOrderDetails returnordDetails;
        public stLocation ShipperLoc;
        public stLocation ConsigneeLoc;
        public stLocation BillerLoc;
        public stLocation ReturnServiceLoc;
        
        public Order()
        {
            shipper = new stCustomer();
            shipper.customerType = "S";
            consignee = new stCustomer();
            consignee.customerType = "C";

            biller = new stCustomer();
            biller.customerType = "B";
            ReturnServiceConsignee = new stCustomer();

            ordDetails = new stOrderDetails();
            ordDetails.VehicleMode = "C";
            ordDetails.IsWebOrder = false;

            returnordDetails = new stOrderDetails();
            returnordDetails.VehicleMode = "Z";
            returnordDetails.IsWebOrder = false;

            returnorderCost = new GlobalOrderCost();
            ShipperLoc = new stLocation();
            ConsigneeLoc = new stLocation();
            BillerLoc = new stLocation();
            ReturnServiceLoc = new stLocation();
            orderCost = new GlobalOrderCost();
            returnorderCost = new GlobalOrderCost();
        }
    }
    public class GlobalOrder
    {
        private StringBuilder mErrorInfo;
        public statusCode OrderStatuscode;
        public Order normalOrder;
        public Guid UserID;
        public bool bHasReturnService = false;

        string _RetDelDriverNum = string.Empty;
        string _DelDriverNum = string.Empty;
        string _RetPUDriverNum = string.Empty;
        string _PUDriverNum = string.Empty;
        string _RetPassOffDriverNum = string.Empty;
        string _PassOffDriverNum = string.Empty;

        string _RetDeliveryDate = string.Empty;
        string _RetDeliverySignature = string.Empty;

        string _DeliveryDate = string.Empty;
        string _DeliverySignature = string.Empty;

        public string RetDelDriverNum { get { return _RetDelDriverNum; } }
        public string DelDriverNum { get { return _DelDriverNum; } }
        public string RetPUDriverNum { get { return _RetPUDriverNum; } }
        public string PUDriverNum { get { return _PUDriverNum; } }

        public string RetPassOffDriverNum { get { return _RetPassOffDriverNum; } }
        public string PassOffDriverNum { get { return _PassOffDriverNum; } }

        public string RetDeliveryDate { get { return _RetDeliveryDate; } }
        public string RetDeliverySignature { get { return _RetDeliverySignature; } }


        public string DeliveryDate { get { return _DeliveryDate; } }
        public string DeliverySignature { get { return _DeliverySignature; } }

        public void runOrderCalculator()
        {

            RunOrderCost(ref this.normalOrder.orderCost, ref this.normalOrder.ordDetails, false);
            RunOrderCost(ref this.normalOrder.returnorderCost, ref this.normalOrder.returnordDetails, true);
            this.SaveGlobalOrder();
        }

        private void RunOrderCost(ref GlobalOrderCost ordCost, ref stOrderDetails ordDetails, bool bReturnService)
        {
            try
            {
                ordCost.myCustomer = CustomersIndicator.Global;
                ordCost.weight = ordDetails.ActualWeight;
                ordCost.wait = ordDetails.waitTime;
                ordCost.GSTValue = ordDetails.GSTValue;
                ordCost.agreedSurcharge = ordDetails.OrderSurchargeRate; // decimal.Parse(paybaleDataRw["SurchargeRate"].ToString());
                ordCost.SpecialDiscount = 0;
                ordCost.TotalSkids = 1;
                ordCost.ServiceType = ordDetails.ServiceType; // cmbService.SelectedValue.ToString().ToUpper();
                ordCost.ApplyGST = this.normalOrder.biller.GSTApplicable;
                ordCost.customerPaymentBase = this.normalOrder.biller.paymentBase; // paybaleDataRw["PaymentBase"].ToString();

                if (ordCost.OverrideOrderCost <= 0)
                {
                    ordCost.OverrideOrderCost = Global.blankOrderNum;
                }

                if (bReturnService)
                {
                    ordCost.shipperCityRegion = this.normalOrder.ConsigneeLoc.cityRegion; // cmbSCityRegion.SelectedValue.ToString();
                    ordCost.consigneeCityRegion = this.normalOrder.ReturnServiceLoc.cityRegion;
                    if (Global.IsNumeric(ordDetails.QTY) && ordDetails.VehicleMode.Equals("T"))
                    {
                        ordCost.TotalSkids = ordDetails.QTY;
                    }

                    ordCost.ConsigneeCity = this.normalOrder.ShipperLoc.City;  //txtCCity.Text;
                    ordCost.ShipperCity = this.normalOrder.ReturnServiceLoc.City; //txtSCity.Text;
                    ordCost.consigneePostalCode = this.normalOrder.ReturnServiceLoc.PostalCode;  //txtCPostalCode.Text;
                    ordCost.shipperPostalCode = this.normalOrder.ConsigneeLoc.PostalCode; //txtSPostalCode.Text;

                }
                else
                {
                    ordCost.shipperCityRegion = this.normalOrder.ShipperLoc.cityRegion;
                    ordCost.consigneeCityRegion = this.normalOrder.ConsigneeLoc.cityRegion; // cmbCCityRegion.SelectedValue.ToString();
                    if (Global.IsNumeric(ordDetails.QTY) && ordDetails.VehicleMode.Equals("T"))
                    {
                        ordCost.TotalSkids = ordDetails.QTY;
                    }

                    ordCost.ConsigneeCity = this.normalOrder.ConsigneeLoc.City;  //txtCCity.Text;
                    ordCost.ShipperCity = this.normalOrder.ShipperLoc.City; //txtSCity.Text;
                    ordCost.consigneePostalCode = this.normalOrder.ConsigneeLoc.PostalCode;  //txtCPostalCode.Text;
                    ordCost.shipperPostalCode = this.normalOrder.ShipperLoc.PostalCode; //txtSPostalCode.Text;

                }
                if (ordDetails.VehicleMode.Equals("V"))
                {
                    ordCost.vanChargesFlag = true;
                }
                else if (ordDetails.VehicleMode.Equals("C"))
                {
                    ordCost.carChargesFlag = true;
                }

                if (ordDetails.VehicleMode.Equals("T"))
                {
                    ordCost.CalculateCost("C");
                }
                else
                {
                    ordCost.CalculateCost("Z");
                }


            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

        }


        public DataTable OrderPOD()
        {
            DataTable myPOD = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalPOD";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = this.normalOrder.ordDetails.OrderNumber;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(myPOD);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("GloberOrder:OrderPOD(); "));
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
            return myPOD;

        }

        public void setWebOrder()
        {
            //normalOrder.ordDetails.PaymentMethod = 
        }

        public stDeliveryDetails GetDriversDeliveryInfo(ref string driverNum)
        {
            stDeliveryDetails myDetails = new stDeliveryDetails();
            myDetails.driverNum = string.Empty;
            myDetails.driverName = string.Empty;
            myDetails.deliveryType = string.Empty;
            myDetails.driverPhone = string.Empty;
            myDetails.passedoffAt = string.Empty;
            myDetails.pickedUpAt = string.Empty;
            myDetails.pod1At = string.Empty;
            myDetails.pod2At = string.Empty;
            myDetails.rpassedoffAt = string.Empty;
            myDetails.rpickedUpAt = string.Empty;
            myDetails.rpod1At = string.Empty;
            myDetails.rpod2At = string.Empty;
            try
            {
                DataTable myInfo = Global.GetGlobalDriverDeliveryInfo(this.normalOrder.ordDetails.OrderNumber, driverNum);
                if (myInfo != null && myInfo.Rows.Count > 0)
                {
                    DataRow myRw = myInfo.Rows[0];
                    if (myRw["DriverName"] != null)
                    {
                        myDetails.driverName = myRw["DriverName"].ToString();
                    }
                    if (myRw["PrimaryPhone"] != null)
                    {
                        myDetails.driverPhone = myRw["PrimaryPhone"].ToString();
                    }
                    if (myRw["ReturnServiceFlag"] != null)
                    {
                        myDetails.ReturnFlag = bool.Parse(myRw["ReturnServiceFlag"].ToString());
                    }
                    if (myRw["DeliveryType"] != null)
                    {
                        myDetails.deliveryType = myRw["DeliveryType"].ToString();
                    }
                    if (myRw["PodDate"] != null)
                    {
                        myDetails.pod1At = myRw["PodDate"].ToString();
                    }

                    if (myDetails.ReturnFlag)
                    {
                        if (myDetails.deliveryType.ToUpper().Equals("O"))
                        {

                            if (myRw["RActivityDateTime"] != null)
                            {
                                myDetails.rpickedUpAt = myRw["RActivityDateTime"].ToString();
                            }
                        }
                        else
                        {
                            if (myRw["RActivityDateTime"] != null)
                            {
                                myDetails.rpassedoffAt = myRw["RActivityDateTime"].ToString();
                            }

                        }

                        if (myRw["RPodDate"] != null)
                        {
                            myDetails.rpod1At = myRw["RPodDate"].ToString();
                        }
                    }
                    else
                    {
                        if (myDetails.deliveryType.ToUpper().Equals("O"))
                        {

                            if (myRw["ActivityDateTime"] != null)
                            {
                                myDetails.pickedUpAt = myRw["ActivityDateTime"].ToString();
                            }
                        }
                        else
                        {
                            if (myRw["ActivityDateTime"] != null)
                            {
                                myDetails.passedoffAt = myRw["ActivityDateTime"].ToString();
                            }

                        }
                    }
                }
            }
            catch (Exception ex1)
            {

                throw ex1;
            }
            return myDetails;
        }

        public GlobalOrder(string mOrderNum)
        {
            normalOrder = new Order();
            if (mOrderNum.Equals(Global.blankOrderNum.ToString()))
            {
                normalOrder.ordDetails.OrderStatus = OrderStatusType.WDP;
            }
            else
            {
                HydrateOrder(mOrderNum);
            }
        }

        public bool CheckAndSetWebWayBillNumber()
        {
            bool bResult = true;
            try
            {
                if (this.normalOrder.ordDetails.WayBillNumber == null ||
                    this.normalOrder.ordDetails.WayBillNumber.Equals(string.Empty) ||
                    !Global.IsNumeric(this.normalOrder.ordDetails.WayBillNumber))
                {
                    this.normalOrder.ordDetails.WayBillNumber = Global.GetNewWebOrderNumber().ToString();
                    this.SaveGlobalOrder();
                }
            }
            catch
            {
                bResult = false;

            }
            return bResult;
        }
        public void CopyFrom(ref GlobalOrder pGlobalOrder)
        {
            //Copy normal order's Customers
            this.normalOrder.biller = pGlobalOrder.normalOrder.biller;
            this.normalOrder.consignee = pGlobalOrder.normalOrder.consignee;
            this.normalOrder.shipper = pGlobalOrder.normalOrder.shipper;

            //Copy normal order's Locations
            this.normalOrder.BillerLoc = pGlobalOrder.normalOrder.BillerLoc;
            this.normalOrder.ConsigneeLoc = pGlobalOrder.normalOrder.ConsigneeLoc;
            this.normalOrder.ShipperLoc = pGlobalOrder.normalOrder.ShipperLoc;
            this.normalOrder.ordDetails = pGlobalOrder.normalOrder.ordDetails;
            this.normalOrder.returnordDetails = pGlobalOrder.normalOrder.returnordDetails;
            this.normalOrder.ReturnServiceLoc = pGlobalOrder.normalOrder.ReturnServiceLoc;
            this.bHasReturnService = pGlobalOrder.bHasReturnService;
        }

        public void CopyFrom(ref GlobalOrder pGlobalOrder, bool MarkedNew)
        {
            CopyFrom(ref pGlobalOrder);

            this.normalOrder.biller.status = statusCode.New;
            this.normalOrder.consignee.status = statusCode.New;
            this.normalOrder.shipper.status = statusCode.New;

            this.normalOrder.BillerLoc.LocationID = Guid.Empty;
            this.normalOrder.BillerLoc.status = statusCode.New;
            this.normalOrder.ConsigneeLoc.LocationID = Guid.Empty;
            this.normalOrder.ConsigneeLoc.status = statusCode.New;
            this.normalOrder.ShipperLoc.LocationID = Guid.Empty;
            this.normalOrder.ShipperLoc.status = statusCode.New;

            this.normalOrder.ordDetails.status = statusCode.New;
            this.normalOrder.returnordDetails.status = statusCode.New;
            this.normalOrder.ReturnServiceLoc.status = statusCode.New;
            this.bHasReturnService = pGlobalOrder.bHasReturnService;

        }
        private void FillOrderDetails(ref DataRow dtMain, ref stOrderDetails pordDetails)
        {
            pordDetails.ActualWeight = 0;
            if (dtMain["ActualWeight"] != null && Global.IsNumeric(dtMain["ActualWeight"].ToString()))
            {
                pordDetails.ActualWeight = decimal.Parse(dtMain["ActualWeight"].ToString());
            }
            pordDetails.ServiceType = dtMain["ServiceType"].ToString();
            pordDetails.VehicleMode = dtMain["VehicleMode"].ToString();
            if (dtMain["QTY"] != null && Global.IsNumeric(dtMain["QTY"].ToString()))
            {
                pordDetails.QTY = int.Parse(dtMain["QTY"].ToString());
            }

            if (dtMain["waitTime"] != null && Global.IsNumeric(dtMain["waitTime"].ToString()))
            {
                pordDetails.waitTime = int.Parse(dtMain["waitTime"].ToString());
            }

            if (dtMain.Table.Columns.Contains("OrderGST") && dtMain["OrderGST"] != null && Global.IsNumeric(dtMain["OrderGST"].ToString()))
            {
                pordDetails.GSTValue = decimal.Parse(dtMain["OrderGST"].ToString());
            }

            pordDetails.UNIT = dtMain["UNIT"].ToString();
            pordDetails.SKID = 0;
            if (dtMain["SKID"] != null && Global.IsNumeric(dtMain["SKID"].ToString()))
            {
                pordDetails.SKID = int.Parse(dtMain["SKID"].ToString());
            }

            pordDetails.status = statusCode.Update;
            pordDetails.ReturnServiceID = new Guid(dtMain["ReturnServiceID"].ToString());

        }

        private void HydrateOrder(string ordNum)
        {
            this.OrderStatuscode = statusCode.Update;

            DataSet globalOrderSet = Global.GetGlobalDelivery(ordNum);

            //Fill Order Details
            DataRow dtMain = globalOrderSet.Tables[0].Rows[0];

            if (!dtMain["InvoiceID"].Equals(DBNull.Value))
            {
                normalOrder.ordDetails.InvoiceNumber = dtMain["InvoiceID"].ToString();
            }
            if (!dtMain["InvoiceDate"].Equals(DBNull.Value) && !string.IsNullOrEmpty(dtMain["InvoiceDate"].ToString()))
            {
                normalOrder.ordDetails.InvoiceDate = Global.GetASCGDATE(dtMain["InvoiceDate"].ToString()).ToString("MM/dd/yyyy");
            }

            normalOrder.ordDetails.AppointeeName = string.Empty;
            normalOrder.ordDetails.ContactPhone1 = string.Empty;
            normalOrder.ordDetails.CaroContainerNum = string.Empty;
            normalOrder.ordDetails.AWBNum = string.Empty;
            normalOrder.ordDetails.WayBillNumber = dtMain["WayBillNumber"].ToString();
            normalOrder.ordDetails.CustomerReferenceNum = dtMain["CustomerReferenceNum"].ToString();
            normalOrder.ordDetails.Department = dtMain["Department"].ToString();
            normalOrder.ordDetails.OrderedBy = dtMain["OrderedBy"].ToString();
            normalOrder.ordDetails.Memo = dtMain["Memo"].ToString();
            normalOrder.ordDetails.OrderNumber = dtMain["OrderNumber"].ToString();
            normalOrder.ordDetails.PaymentMethod = dtMain["PaymentMethod"].ToString();
            normalOrder.ordDetails.PaymentMethodDescription = "Prepaid";
            normalOrder.ordDetails.PaymentMethodDescription = (normalOrder.ordDetails.PaymentMethod.ToUpper().Equals("T")) ? "THIRD PARTY" : "COLLECTION";
            normalOrder.ordDetails.PrintMemoOnInvoice = bool.Parse(dtMain["PrintMemoOnInvoice"].ToString());
            normalOrder.ordDetails.ReturnServiceID = new Guid(dtMain["ReturnServiceID"].ToString());
            normalOrder.ordDetails.dispatchedAt = dtMain["DispatchedDateTime"].ToString();
            normalOrder.ordDetails.RdispatchedAt = dtMain["RDispatchedDateTime"].ToString();
            normalOrder.ordDetails.createdDateTime = dtMain["CreatedDateTime"].ToString();
            switch (dtMain["OrderStatus"].ToString().ToUpper())
            {
                case "IPR":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.IPR;
                    break;
                case "RDD":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.RDD;
                    break;
                case "RDP":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.RDP;
                    break;
                case "RPK":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.RPK;
                    break;
                case "WDA":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WDA;
                    break;
                case "WDP":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WDP;
                    break;
                case "WFB":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WFB;
                    break;
                case "WFD":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WFD;
                    break;
                case "WFP":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WFP;
                    break;
                case "WPK":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WPK;
                    break;
            }



            if (dtMain["WebIndicator"] == null)
            {
                normalOrder.ordDetails.IsWebOrder = false;
            }
            else
            {
                normalOrder.ordDetails.IsWebOrder = bool.Parse(dtMain["WebIndicator"].ToString());
            }

            if (dtMain["OrderPercentageDiscount"] != null && Global.IsNumeric(dtMain["OrderPercentageDiscount"].ToString()))
            {
                normalOrder.ordDetails.OrderPercentageDiscount = decimal.Parse(dtMain["OrderPercentageDiscount"].ToString());
            }
            if (dtMain["OrderSurchargeRate"] != null && Global.IsNumeric(dtMain["OrderSurchargeRate"].ToString()))
            {
                normalOrder.ordDetails.OrderSurchargeRate = decimal.Parse(dtMain["OrderSurchargeRate"].ToString());
            }

            FillOrderDetails(ref dtMain, ref normalOrder.ordDetails);

            //Fill Locations
            DataRow[] rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["ConsigneeLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.ConsigneeLoc);
            }

            rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["ShipperLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.ShipperLoc);
            }

            rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["BillingLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.BillerLoc);
            }

            rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["ReturnSrvLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.ReturnServiceLoc);
            }

            //Fill Customers
            foreach (DataRow dtrw in globalOrderSet.Tables[2].Rows)
            {
                switch (dtrw["CustomerIndictor"].ToString())
                {
                    case "C":
                        FillCustomer(dtrw, ref normalOrder.consignee);
                        break;
                    case "S":
                        FillCustomer(dtrw, ref normalOrder.shipper);
                        break;
                    case "R":
                        FillCustomer(dtrw, ref normalOrder.ReturnServiceConsignee);
                        break;
                    case "B":
                        FillCustomer(dtrw, ref normalOrder.biller);
                        break;
                }
            }

            //Fill Return Service
            this.bHasReturnService = false;
            if (globalOrderSet.Tables[3].Rows.Count > 0)
            {
                DataRow rwReturn = globalOrderSet.Tables[3].Rows[0];
                FillOrderDetails(ref rwReturn, ref normalOrder.returnordDetails);
                normalOrder.returnordDetails.GSTValue = normalOrder.ordDetails.GSTValue;
                normalOrder.returnordDetails.OrderSurchargeRate = normalOrder.ordDetails.OrderSurchargeRate;
                normalOrder.returnordDetails.OrderNumber = normalOrder.ordDetails.OrderNumber;
                this.bHasReturnService = bool.Parse(globalOrderSet.Tables[3].Rows[0]["Active"].ToString());

            }


            //Fill Cost
            if (this.bHasReturnService)
            {
                rw = globalOrderSet.Tables[4].Select("ReturnServiceID = '" + this.normalOrder.returnordDetails.ReturnServiceID.ToString() + "'");
                if (rw.Length > 0)
                {
                    FillCost(rw[0], ref normalOrder.returnorderCost);
                }

                rw = globalOrderSet.Tables[4].Select("ReturnServiceID <> '" + this.normalOrder.returnordDetails.ReturnServiceID.ToString() + "'");
                if (rw.Length > 0)
                {
                    FillCost(rw[0], ref normalOrder.orderCost);
                }
                normalOrder.returnordDetails.status = statusCode.Update;
            }
            else
            {
                foreach (DataRow ordcost in globalOrderSet.Tables[4].Rows)
                {
                    if (ordcost["ReturnServiceID"].Equals(Guid.Empty))
                    {
                        FillCost(ordcost, ref normalOrder.orderCost);
                    }
                }
            }

        }

        private void FillCost(DataRow rw, ref GlobalOrderCost ordCost)
        {
            ordCost.agreedSurcharge = this.normalOrder.biller.SurchargeRate;
            ordCost.ApplyGST = normalOrder.biller.GSTApplicable;
            ordCost.carCharges = decimal.Parse(rw["CarCharges"].ToString());
            ordCost.loadUnloadCharges = decimal.Parse(rw["LoadUnloadCharge"].ToString());
            ordCost.OrdChargeID = new Guid(rw["OrdChargeID"].ToString());
            ordCost.ordCostStatus = statusCode.Update;
            ordCost.OrderBasicCost = decimal.Parse(rw["OrdValue"].ToString()); //decimal.Parse(rw["OrdBasicValue"].ToString());
            ordCost.OrderCost = decimal.Parse(rw["OrdValue"].ToString());
            ordCost.OrderFinalCost = decimal.Parse(rw["OrdFinalNetValue"].ToString());
            ordCost.OrderGST = decimal.Parse(rw["OrdGST"].ToString());
            ordCost.WaitCarChargeUnit = decimal.Parse(rw["WaitingCarUnitCharge"].ToString());
            ordCost.WaitVanChargeUnit = decimal.Parse(rw["WaitingVanUnitCharge"].ToString());
            ordCost.WeightChargeUnit = decimal.Parse(rw["WeigthUnitCharge"].ToString());

            ordCost.WaitCarChargeFreeLimit = decimal.Parse(rw["WaitingVANFreeLimit"].ToString());
            ordCost.WaitVanChargeFreeLimit = decimal.Parse(rw["WaitingCarFreeLimit"].ToString());


            ordCost.SurchargeAmtFromOrder = decimal.Parse(rw["OrdSysFuel"].ToString());
            ordCost.OrderGST = decimal.Parse(rw["OrdSysGst"].ToString());

            //ordCost.orderPaymentBase = normalOrder.biller
            ordCost.OrderSaving = decimal.Parse(rw["OrdSaving"].ToString());
            if (rw["OrdOverRideValue"] != DBNull.Value)
            {
                ordCost.OverrideOrderCost = decimal.Parse(rw["OrdOverRideValue"].ToString());
            }
            if (ordCost.OverrideOrderCost > 0)
            {
                ordCost.OverRideOrderFinalCost = decimal.Parse(rw["OrdFinalNetValue"].ToString());
            }
            if (rw["OrdOverRideGST"] != DBNull.Value)
            {
                ordCost.OverRideOrderGSTcharge = decimal.Parse(rw["OrdOverRideGST"].ToString());
            }
            //ordCost.OverRideOrderSaving = 0;
            if (rw["OrdOverRideFuel"] != DBNull.Value)
            {
                ordCost.OverRideSurchargeAmtFromOrder = decimal.Parse(rw["OrdOverRideFuel"].ToString());
            }
            ordCost.ReturnServiceID = new Guid(rw["ReturnServiceID"].ToString());
            //ordCost.ServiceType = n;
            // ordCost.SpecialDiscount = 0;
            //ordCost.SurchargeAmtFromOrder = decimal.Parse(rw["OrdFuel"].ToString()); 
            //ordCost.TotalMileage = 0;
            ordCost.loadUnloadCharges = decimal.Parse(rw["LoadUnloadCharge"].ToString());
            ordCost.vanCharges = decimal.Parse(rw["VanCharge"].ToString());
            ordCost.waitCharges = decimal.Parse(rw["WaitCharges"].ToString());
            ordCost.weightCharges = decimal.Parse(rw["WeightCharges"].ToString());

            ordCost.status = statusCode.Update;
        }

        private void FillCustomer(DataRow rw, ref stCustomer cust)
        {
            cust.CustomerNum = short.Parse(rw["CustomerNum"].ToString().ToString());
            cust.customerType = rw["CustomerIndictor"].ToString();

            cust.GSTApplicable = bool.Parse(rw["GSTApplicable"].ToString());
            if (Global.IsNumeric(rw["SpecialDiscount"].ToString()))
            {
                cust.SpecialDiscount = decimal.Parse(rw["SpecialDiscount"].ToString());
            }
            cust.Name = rw["CustomerName"].ToString();
            cust.ContactName = rw["ContactName1"].ToString();
            cust.ContactPhone = rw["Contact1Number"].ToString();
            cust.orderCustID = new Guid(rw["OrdCustID"].ToString());
            cust.paymentBase = rw["PaymentBase"].ToString();
            cust.status = statusCode.Update;
        }

        private void FillLocation(ref DataRow rw, ref stLocation loc)
        {
            loc.City = rw["City"].ToString();
            loc.Country = rw["Country"].ToString();
            string[] datePart = rw["PickUpDeliveryDate"].ToString().Split('/');
            loc.DeliveryDate = new DateTime(int.Parse(datePart[2]), int.Parse(datePart[0]), int.Parse(datePart[1]));

            //datePart = rw["PickUpDeliveryTime"].ToString().Split('/');
            loc.DeliveryTime = DateTime.Parse(rw["PickUpDeliveryTime"].ToString());

            loc.LocationID = new Guid(rw["LocationID"].ToString());
            loc.PostalCode = rw["PostalCode"].ToString();
            loc.Province = rw["Province"].ToString();
            loc.status = statusCode.Update;
            loc.StreetLocation = rw["StreetLocation"].ToString();
            loc.Suite = rw["Suite"].ToString();
            loc.cityRegion = rw["CityRegion"].ToString();
            loc.status = statusCode.Update;

        }

        public void setDeliveryAndDrivers()
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection();
            DataTable tbl;
            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();

            try
            {
                con.ConnectionString = Global.GlobalConnectionstring;
                cmd.CommandText = "GetGlobalDeliveryDrivers";
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = this.normalOrder.ordDetails.OrderNumber;
                cmd.Parameters.Add(parm);
                con.Open();
                cmd.Connection = con;
                adp.SelectCommand = cmd;
                adp.Fill(ds);
                tbl = ds.Tables[0];
                bool bForReturn = false;

                foreach (DataRow podRw in ds.Tables[1].Rows)
                {
                    if (bool.Parse(podRw["ReturnServiceFlag"].ToString()))
                    {
                        _RetDelDriverNum = podRw["PodDriver"].ToString();
                        _RetDeliveryDate = podRw["PodDate"].ToString();
                        _RetDeliverySignature = podRw["ApprovedBy"].ToString();
                    }
                    else
                    {
                        _DelDriverNum = podRw["PodDriver"].ToString();
                        _DeliveryDate = podRw["PodDate"].ToString();
                        _DeliverySignature = podRw["ApprovedBy"].ToString();
                        //if this is return delivery then return delivery is being picked up by 
                        //same driver
                        if (!this.normalOrder.ordDetails.ReturnServiceID.Equals(Guid.Empty))
                        {
                            _RetPUDriverNum = _DelDriverNum;
                        }
                    }
                }

                //Calculate 
                foreach (DataRow rw in tbl.Rows)
                {
                    bForReturn = (bool)rw["ReturnServiceFlag"];

                    //PickUp deriver
                    if (rw["DeliveryType"].ToString().ToUpper().Equals("O"))
                    {
                        _PUDriverNum = rw["DriverNum"].ToString();
                    }
                    else if (rw["DeliveryType"].ToString().ToUpper().Equals("P"))
                    {
                        if (!rw["DriverNum"].ToString().Equals(_RetPUDriverNum))
                        {
                            if (bForReturn)
                            {
                                _RetPassOffDriverNum = rw["DriverNum"].ToString();
                            }
                            else
                            {
                                _PassOffDriverNum = rw["DriverNum"].ToString();
                            }
                        }
                    }

                }


            }

            catch
            {

            }
            finally
            {
                if (con != null) con.Close();
            }


        }



        #region "Save Order"

        public bool SaveGlobalOrder()
        {
            bool bResult = true;
            SqlParameter parm = new SqlParameter();
            SqlCommand Cmd = new SqlCommand();
            Cmd.CommandType = CommandType.StoredProcedure;
            SqlTransaction tr = null;
            SqlConnection con = new SqlConnection(Global.GlobalConnectionstring);
            try
            {
                con.Open();
                Cmd.Connection = con;
                tr = con.BeginTransaction();
                if (this.OrderStatuscode == statusCode.New)
                {
                    normalOrder.ordDetails.status = statusCode.New;

                    normalOrder.biller.orderCustID = Guid.NewGuid();
                    normalOrder.consignee.orderCustID = Guid.NewGuid();
                    normalOrder.shipper.orderCustID = Guid.NewGuid();

                    normalOrder.ConsigneeLoc.LocationID = Guid.NewGuid();
                    normalOrder.ShipperLoc.LocationID = Guid.NewGuid();
                    normalOrder.BillerLoc.LocationID = Guid.NewGuid();

                    normalOrder.orderCost.OrdChargeID = Guid.NewGuid();
                    //add update address
                   // CustomerAddress custadd = new CustomerAddress(custNumber);

                    //    custadd.mName = myOrder.normalOrder.consignee.Name;
                    //    custadd.mActive = true;
                    //    custadd.mCustomerNum = myOrder.normalOrder.consignee.CustomerNum;
                    //    custadd.mMailingLocID = myOrder.normalOrder.ConsigneeLoc.LocationID.ToString();
                    //    custadd.mStreetLocation = myOrder.normalOrder.ConsigneeLoc.StreetLocation;
                    //    custadd.mSuite = myOrder.normalOrder.ConsigneeLoc.Suite;
                    //    custadd.mCity = myOrder.normalOrder.ConsigneeLoc.City;
                    //    custadd.mProvince = myOrder.normalOrder.ConsigneeLoc.Province;
                    //    custadd.mCountry = myOrder.normalOrder.ConsigneeLoc.Country;
                    //    custadd.mContact1Name = myOrder.normalOrder.consignee.ContactName;
                    //    custadd.mContact1Phone = myOrder.normalOrder.consignee.ContactPhone;
                    //    custadd.mLastModifiedBy = DateTime.Now.ToString();
                    //    custadd.Save();
                    //
                    if (this.bHasReturnService)
                    {
                        normalOrder.ReturnServiceLoc.LocationID = Guid.NewGuid();
                        normalOrder.ReturnServiceLoc.status = statusCode.New;
                        normalOrder.returnordDetails.ReturnServiceID = Guid.NewGuid();
                        normalOrder.returnordDetails.status = statusCode.New;
                        normalOrder.ordDetails.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                        normalOrder.returnorderCost.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                        normalOrder.ReturnServiceConsignee.orderCustID = Guid.NewGuid();
                        normalOrder.ReturnServiceConsignee.status = statusCode.New;
                        normalOrder.returnorderCost.OrdChargeID = Guid.NewGuid();
                        normalOrder.returnorderCost.OrdChargeID = normalOrder.returnordDetails.ReturnServiceID;

                    }
                }
                else
                {
                    if (this.bHasReturnService)
                    {
                        if (normalOrder.ReturnServiceLoc.LocationID == null ||
                            normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                        {
                            normalOrder.ReturnServiceLoc.LocationID = Guid.NewGuid();
                            normalOrder.ReturnServiceLoc.status = statusCode.New;

                            normalOrder.returnordDetails.ReturnServiceID = Guid.NewGuid();
                            normalOrder.returnordDetails.status = statusCode.New;
                            normalOrder.ordDetails.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                            normalOrder.returnorderCost.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                            normalOrder.ReturnServiceConsignee.orderCustID = Guid.NewGuid();
                            normalOrder.ReturnServiceConsignee.status = statusCode.New;

                            normalOrder.returnorderCost.OrdChargeID = Guid.NewGuid();

                        }
                        else
                        {
                            normalOrder.ReturnServiceLoc.status = statusCode.Update;
                            normalOrder.returnordDetails.status = statusCode.Update;
                            normalOrder.ReturnServiceConsignee.status = statusCode.Update;
                            normalOrder.orderCost.status = statusCode.Update;
                        }

                    }
                    else
                    {
                        if (!normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                        {
                            normalOrder.ReturnServiceLoc.status = statusCode.Delete;
                            normalOrder.returnordDetails.status = statusCode.Delete;
                            normalOrder.ReturnServiceConsignee.status = statusCode.Delete;
                            normalOrder.orderCost.status = statusCode.Delete;
                        }
                    }
                }

                if (this.normalOrder.ordDetails.ServiceType.Equals(DeliverySerrviceType.ON1.ToString()))
                {
                    this.normalOrder.ShipperLoc.DeliveryTime = DateTime.Parse("09:00 AM");
                }
                else if (this.normalOrder.ordDetails.ServiceType.Equals(DeliverySerrviceType.ON2.ToString()))
                {
                    this.normalOrder.ShipperLoc.DeliveryTime = DateTime.Parse("10:30 AM");
                }
                else if (this.normalOrder.ordDetails.ServiceType.Equals(DeliverySerrviceType.ON3.ToString()))
                {
                    this.normalOrder.ShipperLoc.DeliveryTime = DateTime.Parse("12:00 PM");
                }

                bResult = SaveOrder(ref tr);

                if (bResult)
                {
                    tr.Commit();
                }
                else
                {
                    tr.Rollback();
                    throw new Exception("The order couldn't save");
                }
                try
                {
                    Global.setOrdChargesCorrectly(this.normalOrder.ordDetails.OrderNumber);
                }
                catch
                {

                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                if (tr.Connection != null) tr.Rollback();
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :SaveGlobalOrder(); "));
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
            return bResult;
        }

        private bool SaveOrder(ref SqlTransaction tr)
        {
            bool bResult = true;
            try
            {
                if (!this.SetOrderDetails(ref tr))
                {
                    throw new Exception("Order Details couldn't save");
                }

                if (!this.SetOrderLocations(ref tr))
                {
                    return false;
                }

                if (!this.SetOrderCustomers(ref tr))
                {
                    return false;
                }

                if (!this.SetOrderCost(ref tr, ref normalOrder.orderCost))
                {
                    return false;
                }

                if (this.bHasReturnService)
                {
                    if (!this.SetOrderCost(ref tr, ref normalOrder.returnorderCost))
                    {
                        return false;
                    }
                }

                if (!this.SetOrderReturnService(ref tr))
                {
                    throw new Exception("Order Details couldn't save");
                }
            }
            catch (Exception ex1)
            {
                return false;
                throw ex1;

            }
            finally
            {

            }

            return true;

        }

        private bool SetOrderReturnService(ref SqlTransaction tr)
        {
            if (this.OrderStatuscode == statusCode.New && !this.bHasReturnService) return true;
            if (this.normalOrder.returnordDetails.ReturnServiceID.Equals(Guid.Empty)) return true;
            bool bResult = true;
            SqlParameter parm;
            SqlCommand Cmd = new SqlCommand();
            SqlDataAdapter dataAdp = new SqlDataAdapter();

            try
            {

                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerReferenceNum";
                parm.Value = normalOrder.ordDetails.CustomerReferenceNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@QTY";
                parm.Value = normalOrder.returnordDetails.QTY;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UNIT";
                parm.Value = normalOrder.returnordDetails.UNIT;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@SKID";
                parm.Value = normalOrder.returnordDetails.SKID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ActualWeight";
                parm.Value = normalOrder.returnordDetails.ActualWeight;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderedBy";
                parm.Value = normalOrder.ordDetails.OrderedBy;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnServiceID";
                parm.Value = normalOrder.returnordDetails.ReturnServiceID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ServiceType";
                parm.Value = normalOrder.returnordDetails.ServiceType;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@VehicleMode";
                parm.Value = normalOrder.returnordDetails.VehicleMode;
                Cmd.Parameters.Add(parm);


                if (this.normalOrder.ReturnServiceLoc.status == statusCode.New)
                {
                    Cmd.CommandText = "InsertGlobalReturnService";
                }
                else
                {
                    Cmd.CommandText = "UpdateGlobalReturnService";
                    parm = new SqlParameter();
                    parm.ParameterName = "@Active";
                    parm.Value = 1;
                    if (!this.bHasReturnService)
                        parm.Value = 0;
                    Cmd.Parameters.Add(parm);
                }

                Cmd.ExecuteNonQuery();

            }

            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :SetOrderDetails(); "));
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
                Cmd = null;
            }
            return bResult;
        }

        private bool SetOrderDetails(ref SqlTransaction tr)
        {
            bool bResult = true;
            SqlParameter parm;
            SqlCommand Cmd = new SqlCommand();
            SqlDataAdapter dataAdp = new SqlDataAdapter();

            try
            {
                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;

                //Save Order Details and get New Order Number
                if (OrderStatuscode == statusCode.New)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@NextOrderNum";
                    parm.DbType = DbType.String;
                    parm.Direction = ParameterDirection.Output;
                    parm.Size = 50;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.Value = UserID;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.Value = DateTime.Now;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@WebIndicator";
                    parm.Value = normalOrder.ordDetails.IsWebOrder;
                    Cmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@OrderNumber";
                    parm.Value = normalOrder.ordDetails.OrderNumber;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.Value = UserID;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.Value = DateTime.Now;
                    Cmd.Parameters.Add(parm);
                }

                parm = new SqlParameter();
                parm.ParameterName = "@OrderSurchargeRate";
                parm.Value = normalOrder.ordDetails.OrderSurchargeRate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderPercentageDiscount";
                parm.Value = normalOrder.ordDetails.OrderPercentageDiscount;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Memo";
                parm.Value = normalOrder.ordDetails.Memo;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PrintMemoOnInvoice";
                parm.Value = normalOrder.ordDetails.PrintMemoOnInvoice;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@WayBillNumber";
                parm.Value = normalOrder.ordDetails.WayBillNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderStatus";
                parm.Value = normalOrder.ordDetails.OrderStatus.ToString();
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerReferenceNum";
                parm.Value = normalOrder.ordDetails.CustomerReferenceNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Department";
                parm.Value = normalOrder.ordDetails.Department;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@QTY";
                parm.Value = normalOrder.ordDetails.QTY;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UNIT";
                parm.Value = normalOrder.ordDetails.UNIT;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@SKID";
                parm.Value = normalOrder.ordDetails.SKID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ActualWeight";
                parm.Value = normalOrder.ordDetails.ActualWeight;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderedBy";
                parm.Value = normalOrder.ordDetails.OrderedBy;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ConsigneeLocationID";
                parm.Value = normalOrder.ConsigneeLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ShipperLocationID";
                parm.Value = normalOrder.ShipperLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@BillingLocationID";
                parm.Value = normalOrder.BillerLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnSrvLocationID";
                parm.Value = normalOrder.ReturnServiceLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ServiceType";
                parm.Value = normalOrder.ordDetails.ServiceType;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PaymentMethod";
                parm.Value = normalOrder.ordDetails.PaymentMethod;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@InvoiceDueDays";
                parm.Value = normalOrder.ordDetails.InvoiceDueDays;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@EmployeeCommission";
                parm.Value = normalOrder.ordDetails.EmployeeCommission;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@EmployeeNum";
                parm.Value = normalOrder.ordDetails.EmployeeNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@VehicleMode";
                parm.Value = normalOrder.ordDetails.VehicleMode;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnServiceID";
                parm.Value = normalOrder.ordDetails.ReturnServiceID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderGST";
                parm.Value = normalOrder.ordDetails.GSTValue;
                Cmd.Parameters.Add(parm);


                if (this.OrderStatuscode == statusCode.New)
                {
                    Cmd.CommandText = "InsertGlobalDeliveryOrder";
                }
                else
                {
                    Cmd.CommandText = "UpdateGlobalDeliveryOrder";
                }

                Cmd.ExecuteNonQuery();
                if (this.OrderStatuscode == statusCode.New)
                {
                    normalOrder.ordDetails.OrderNumber = Cmd.Parameters["@NextOrderNum"].Value.ToString();
                }
            }

            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :SetOrderDetails(); "));
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
                Cmd = null;
            }
            return bResult;
        }

        #region "Save Location"
        private bool SetOrderLocations(ref SqlTransaction tr)
        {
            bool bResult = true;

            try
            {
                //set shipper
                bResult = this.setOrderLocation(ref tr, ref normalOrder.ShipperLoc);

                if (bResult)
                {
                    //set consignee
                    bResult = this.setOrderLocation(ref tr, ref normalOrder.ConsigneeLoc);
                }

                if (bResult)
                {
                    if (this.bHasReturnService)
                    {
                        bResult = this.setOrderLocation(ref tr, ref normalOrder.ReturnServiceLoc);
                    }
                    else
                    {
                        if (!normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                        {
                            bResult = this.setOrderLocation(ref tr, ref normalOrder.ReturnServiceLoc);
                        }
                    }
                }

                if (bResult && !string.IsNullOrEmpty(normalOrder.biller.status.ToString()))
                {
                    bResult = this.setOrderLocation(ref tr, ref normalOrder.BillerLoc);
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            return bResult;
        }

        private bool setOrderLocation(ref SqlTransaction tr, ref stLocation myLocation)
        {
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();

            try
            {
                this.setOrderLocationsCommand(ref Cmd, ref myLocation);

                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;
                Cmd.ExecuteNonQuery();


            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            finally
            {
                //con.Close();
                Cmd = null;
            }
            return bResult;

        }

        private bool setOrderLocationsCommand(ref SqlCommand sqlCmd, ref stLocation myLocation)
        {
            SqlParameter parm;
            bool bResult = true;
            try
            {

                if (myLocation.status == statusCode.New)
                {
                    sqlCmd.CommandText = "InsertGlobalOrderLocation";
                }
                else
                {
                    sqlCmd.CommandText = "UpdateGlobalOrderLocation";
                }

                #region "Params for Insert & Update"

                parm = new SqlParameter();
                parm.ParameterName = "@StreetLocation";
                parm.Value = myLocation.StreetLocation;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Suite";
                parm.Value = myLocation.Suite;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@City";
                parm.Value = myLocation.City;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Province";
                parm.Value = myLocation.Province;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Country";
                parm.Value = myLocation.Country;
                sqlCmd.Parameters.Add(parm);


                parm = new SqlParameter();
                parm.ParameterName = "@PickUpDeliveryDate";
                parm.Value = myLocation.DeliveryDate.Month.ToString().PadLeft(2, '0') + "/" +
                             myLocation.DeliveryDate.Day.ToString().PadLeft(2, '0') + "/" +
                             myLocation.DeliveryDate.Year.ToString().PadLeft(4, '0');
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PickUpDeliveryTime";
                parm.Value = myLocation.DeliveryTime.ToShortTimeString();
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PostalCode";
                parm.Value = myLocation.PostalCode;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@LocationID";
                parm.Value = myLocation.LocationID;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = this.normalOrder.ordDetails.OrderNumber;
                sqlCmd.Parameters.Add(parm);


                parm = new SqlParameter();
                parm.ParameterName = "@CityRegion";
                if (myLocation.cityRegion == null || myLocation.cityRegion.Equals(string.Empty))
                {
                    parm.Value = DBNull.Value;
                }
                else
                {
                    parm.Value = myLocation.cityRegion;
                }
                sqlCmd.Parameters.Add(parm);


                #endregion
                if (myLocation.status == statusCode.New)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }



                parm = new SqlParameter();
                parm.ParameterName = "@Active";
                if (myLocation.status == statusCode.Delete)
                {
                    parm.Value = 0;
                }
                else
                {
                    parm.Value = 1;
                }
                sqlCmd.Parameters.Add(parm);
            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;

            }
            return bResult;

        }
        #endregion

        #region "Save Customers"

        private bool SetOrderCost(ref SqlTransaction tr, ref GlobalOrderCost pOrdCost)
        {
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();
            SqlParameter parm;
            try
            {
                Cmd.CommandText = "SetOrderCost";
                Cmd.CommandType = CommandType.StoredProcedure;

                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;

                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = normalOrder.ordDetails.OrderNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdValue";
                parm.Value = pOrdCost.OrderCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdBasicValue";
                parm.Value = pOrdCost.OrderBasicCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSaving";
                parm.Value = pOrdCost.OrderSaving;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdAddChrgs";
                parm.Value = DBNull.Value;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdAddGSTChrgs";
                parm.Value = DBNull.Value;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdNetValue";
                parm.Value = pOrdCost.OrderFinalCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdInvAmt";
                parm.Value = DBNull.Value;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdInvGST";
                parm.Value = DBNull.Value;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSysValue";
                parm.Value = pOrdCost.OrderCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSysFuel";
                parm.Value = pOrdCost.SurchargeAmtFromOrder;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSysGST";
                parm.Value = pOrdCost.OrderGST;
                Cmd.Parameters.Add(parm);


                if (pOrdCost.OverrideOrderCost > 0)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideValue";
                    parm.Value = pOrdCost.OverrideOrderCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalBasicValue";
                    parm.Value = pOrdCost.OverrideOrderCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideFuel";
                    parm.Value = pOrdCost.OverRideSurchargeAmtFromOrder;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideGST";
                    parm.Value = pOrdCost.OverRideOrderGSTcharge;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFuel";
                    parm.Value = pOrdCost.OverRideSurchargeAmtFromOrder;
                    Cmd.Parameters.Add(parm);


                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdGST";
                    parm.Value = pOrdCost.OverRideOrderGSTcharge;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalNetValue";
                    parm.Value = pOrdCost.OverRideOrderFinalCost;
                    Cmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalBasicValue";
                    parm.Value = pOrdCost.OrderBasicCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideValue";
                    parm.Value = DBNull.Value;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideFuel";
                    parm.Value = DBNull.Value;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideGST";
                    parm.Value = DBNull.Value;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFuel";
                    parm.Value = pOrdCost.SurchargeAmtFromOrder;
                    Cmd.Parameters.Add(parm);


                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdGST";
                    parm.Value = pOrdCost.OrderGST;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalNetValue";
                    parm.Value = pOrdCost.OrderFinalCost;
                    Cmd.Parameters.Add(parm);

                }
                parm = new SqlParameter();
                parm.ParameterName = "@CreatedBy";
                parm.Value = UserID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@VanCharge";
                parm.Value = pOrdCost.vanCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CarCharges";
                parm.Value = pOrdCost.carCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@WaitCharges";
                parm.Value = pOrdCost.waitCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@WeightCharges";
                parm.Value = pOrdCost.weightCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@LoadUnloadCharge";
                parm.Value = pOrdCost.loadUnloadCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnServiceID";
                parm.Value = pOrdCost.ReturnServiceID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdChargeID";
                parm.Value = pOrdCost.OrdChargeID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Active";
                parm.Value = 1;

                if (pOrdCost.ordCostStatus == statusCode.Delete) parm.Value = 0;

                Cmd.Parameters.Add(parm);
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }

            return bResult;
        }

        private bool SetOrderCustomers(ref SqlTransaction tr)
        {
            bool bResult = true;
            try
            {
                //set shipper
                bResult = this.SetOrderCustomer(ref tr, ref normalOrder.shipper);

                //set consignee
                if (bResult)
                {
                    bResult = this.SetOrderCustomer(ref tr, ref normalOrder.consignee);
                }

                if (bResult && !string.IsNullOrEmpty(normalOrder.biller.status.ToString()))
                {
                    bResult = this.SetOrderCustomer(ref tr, ref normalOrder.biller);
                }

                if (this.bHasReturnService)
                {
                    bResult = this.SetOrderCustomer(ref tr, ref normalOrder.ReturnServiceConsignee);
                }
                else
                {
                    if (!normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                    {
                        bResult = this.SetOrderCustomer(ref tr, ref normalOrder.ReturnServiceConsignee);
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;

            }

            return bResult;
        }

        private bool SetOrderCustomer(ref SqlTransaction tr, ref stCustomer myCustomer)
        {
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();

            try
            {

                bResult = this.setOrderCustomerCommand(ref Cmd, ref myCustomer);

                if (bResult)
                {
                    Cmd.Connection = tr.Connection;
                    Cmd.CommandType = CommandType.StoredProcedure;
                    Cmd.Transaction = tr;
                    Cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            finally
            {
                //con.Close();
                Cmd = null;
            }
            return bResult;
        }

        private bool setOrderCustomerCommand(ref SqlCommand sqlCmd, ref stCustomer myCustomer)
        {
            bool bResult = true;
            SqlParameter parm;
            try
            {
                if (myCustomer.status == statusCode.New)
                {
                    sqlCmd.CommandText = "InsertGlobalOrderCustomer";
                }
                else
                {
                    sqlCmd.CommandText = "UpdateGlobalOrderCustomer";
                }

                #region "Params for Insert & Update"

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerName";
                parm.Value = myCustomer.Name;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@GSTApplicable";
                parm.Value = myCustomer.GSTApplicable;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ContactName";
                parm.Value = myCustomer.ContactName;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ContactPhone";
                parm.Value = myCustomer.ContactPhone;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@SpecialDiscount";
                parm.Value = myCustomer.SpecialDiscount;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PaymentBase";
                parm.Value = DBNull.Value;
                if (myCustomer.paymentBase != null)
                {
                    parm.Value = myCustomer.paymentBase;
                }
                sqlCmd.Parameters.Add(parm);



                #endregion
                if (myCustomer.status == statusCode.New)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerNum";
                parm.Value = myCustomer.CustomerNum;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = normalOrder.ordDetails.OrderNumber;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerIndictor";
                parm.Value = myCustomer.customerType;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdCustID";
                parm.Value = myCustomer.orderCustID;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Active";
                parm.Value = 1;
                if (myCustomer.status == statusCode.Delete) parm.Value = 0;
                sqlCmd.Parameters.Add(parm);

            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            return bResult;
        }

        #endregion
        #endregion

        #region "Calculate Web Order Cost"

        public void RecalculateOrderCost(bool bForReturnService)
        {
            if (!bForReturnService)
            {
                this.normalOrder.orderCost.wait = this.normalOrder.ordDetails.waitTime;
                this.normalOrder.orderCost.agreedSurcharge = this.normalOrder.ordDetails.OrderSurchargeRate;
                this.normalOrder.orderCost.weight = this.normalOrder.ordDetails.ActualWeight;
                this.normalOrder.orderCost.GSTValue = decimal.Parse(Global.GST());
                this.normalOrder.orderCost.customerPaymentBase = this.normalOrder.biller.paymentBase;
                this.normalOrder.orderCost.TotalSkids = this.normalOrder.ordDetails.QTY;
                this.normalOrder.orderCost.ServiceType = this.normalOrder.ordDetails.ServiceType;
                this.normalOrder.orderCost.consigneePostalCode = this.normalOrder.ConsigneeLoc.PostalCode;
                this.normalOrder.orderCost.ConsigneeCity = this.normalOrder.ConsigneeLoc.City;
                this.normalOrder.orderCost.ShipperCity = this.normalOrder.ShipperLoc.City;
                this.normalOrder.orderCost.shipperPostalCode = this.normalOrder.ShipperLoc.PostalCode;
                this.normalOrder.orderCost.consigneeCityRegion = normalOrder.ConsigneeLoc.cityRegion;
                this.normalOrder.orderCost.shipperCityRegion = normalOrder.ShipperLoc.cityRegion;
                this.normalOrder.orderCost.ApplyGST = normalOrder.biller.GSTApplicable;

                if (this.normalOrder.ordDetails.VehicleMode.Equals("T"))
                {
                    this.normalOrder.orderCost.CalculateCost("C");
                }
                else
                {
                    this.normalOrder.orderCost.CalculateCost("Z");
                }
            }
            else
            {
                this.normalOrder.orderCost.agreedSurcharge = this.normalOrder.ordDetails.OrderSurchargeRate;
                this.normalOrder.returnorderCost.wait = this.normalOrder.returnordDetails.waitTime;
                this.normalOrder.returnorderCost.weight = this.normalOrder.returnordDetails.ActualWeight;
                this.normalOrder.returnorderCost.GSTValue = decimal.Parse(Global.GST());
                this.normalOrder.returnorderCost.customerPaymentBase = this.normalOrder.biller.paymentBase;
                this.normalOrder.returnorderCost.TotalSkids = this.normalOrder.returnordDetails.QTY;
                this.normalOrder.returnorderCost.ServiceType = this.normalOrder.returnordDetails.ServiceType;
                this.normalOrder.returnorderCost.consigneePostalCode = this.normalOrder.ReturnServiceLoc.PostalCode;
                this.normalOrder.returnorderCost.ConsigneeCity = this.normalOrder.ReturnServiceLoc.City;
                this.normalOrder.returnorderCost.ShipperCity = this.normalOrder.ConsigneeLoc.City;
                this.normalOrder.returnorderCost.shipperPostalCode = this.normalOrder.ConsigneeLoc.PostalCode;
                this.normalOrder.returnorderCost.consigneeCityRegion = normalOrder.ReturnServiceLoc.cityRegion;
                this.normalOrder.returnorderCost.shipperCityRegion = normalOrder.ConsigneeLoc.cityRegion;
                this.normalOrder.returnorderCost.ApplyGST = normalOrder.biller.GSTApplicable;
                if (this.normalOrder.returnordDetails.VehicleMode.Equals("T"))
                {
                    this.normalOrder.returnorderCost.CalculateCost("C");
                }
                else
                {
                    this.normalOrder.returnorderCost.CalculateCost("Z");
                }
            }
            this.SaveGlobalOrder();
        }

        public void calculateWebOrderCost()
        {
            this.normalOrder.orderCost.myCustomer = CustomersIndicator.Global;
            this.normalOrder.orderCost.weight = this.normalOrder.ordDetails.ActualWeight;
            this.normalOrder.orderCost.GSTValue = decimal.Parse(Global.GST());
            this.normalOrder.orderCost.agreedSurcharge = this.normalOrder.biller.SurchargeRate;
            this.normalOrder.orderCost.customerPaymentBase = this.normalOrder.biller.paymentBase;
            this.normalOrder.orderCost.TotalSkids = this.normalOrder.ordDetails.QTY;
            this.normalOrder.orderCost.ServiceType = this.normalOrder.ordDetails.ServiceType;
            this.normalOrder.orderCost.consigneePostalCode = this.normalOrder.ConsigneeLoc.PostalCode;
            this.normalOrder.orderCost.ConsigneeCity = this.normalOrder.ConsigneeLoc.City;
            this.normalOrder.orderCost.ShipperCity = this.normalOrder.ShipperLoc.City;
            this.normalOrder.orderCost.shipperPostalCode = this.normalOrder.ShipperLoc.PostalCode;
            this.normalOrder.orderCost.consigneeCityRegion = normalOrder.ConsigneeLoc.cityRegion;
            this.normalOrder.orderCost.shipperCityRegion = normalOrder.ShipperLoc.cityRegion;
            this.normalOrder.orderCost.ApplyGST = normalOrder.biller.GSTApplicable;

            switch (this.normalOrder.ordDetails.UNIT.ToUpper())
            {
                case "ENV":
                case "PKG":
                    this.normalOrder.orderCost.CalculateCost("Z");
                    break;
                default:
                    this.normalOrder.orderCost.CalculateCost("C");
                    break;
            }
        }
        #endregion

       
    }
    public class MFIOrder
    {
        private StringBuilder mErrorInfo;
        public statusCode OrderStatuscode;
        public Order normalOrder;
        public Guid UserID;
        public bool bHasReturnService = false;
        string _RetDelDriverNum = string.Empty;
        string _DelDriverNum = string.Empty;
        string _RetPUDriverNum = string.Empty;
        string _PUDriverNum = string.Empty;
        string _RetPassOffDriverNum = string.Empty;
        string _PassOffDriverNum = string.Empty;

        string _RetDeliveryDate = string.Empty;
        string _RetDeliveryTime = string.Empty;
        string _RetDeliverySignature = string.Empty;

        string _DeliveryDate = string.Empty;
        string _DeliveryTime = string.Empty;
        string _DeliverySignature = string.Empty;

        public string RetDelDriverNum { get { return _RetDelDriverNum; } }
        public string DelDriverNum { get { return _DelDriverNum; } }
        public string RetPUDriverNum { get { return _RetPUDriverNum; } }
        public string PUDriverNum { get { return _PUDriverNum; } }

        public string RetPassOffDriverNum { get { return _RetPassOffDriverNum; } }
        public string PassOffDriverNum { get { return _PassOffDriverNum; } }

        public string RetDeliveryDate { get { return _RetDeliveryDate; } }
        public string RetDeliveryTime { get { return _RetDeliveryTime; } }
        public string RetDeliverySignature { get { return _RetDeliverySignature; } }


        public string DeliveryDate { get { return _DeliveryDate; } }
        public string DeliveryTime { get { return _DeliveryTime; } }
        public string DeliverySignature { get { return _DeliverySignature; } }

        private void RunOrderCost(ref GlobalOrderCost ordCost, ref stOrderDetails ordDetails, bool bReturnService)
        {
            try
            {
                ordCost.myCustomer = CustomersIndicator.MFI;
                ordCost.weight = ordDetails.ActualWeight;
                ordCost.wait = ordDetails.waitTime;
                ordCost.GSTValue = ordDetails.GSTValue;
                ordCost.agreedSurcharge = ordDetails.OrderSurchargeRate; // decimal.Parse(paybaleDataRw["SurchargeRate"].ToString());
                ordCost.SpecialDiscount = 0;
                ordCost.TotalSkids = 1;
                ordCost.ServiceType = ordDetails.ServiceType; // cmbService.SelectedValue.ToString().ToUpper();
                ordCost.ApplyGST = this.normalOrder.biller.GSTApplicable;
                ordCost.customerPaymentBase = this.normalOrder.biller.paymentBase; // paybaleDataRw["PaymentBase"].ToString();

                if (ordCost.OverrideOrderCost <= 0)
                {
                    ordCost.OverrideOrderCost = Global.blankOrderNum;
                }

                if (bReturnService)
                {
                    ordCost.shipperCityRegion = this.normalOrder.ConsigneeLoc.cityRegion; // cmbSCityRegion.SelectedValue.ToString();
                    ordCost.consigneeCityRegion = this.normalOrder.ReturnServiceLoc.cityRegion;
                    if (Global.IsNumeric(ordDetails.QTY) && ordDetails.VehicleMode.Equals("T"))
                    {
                        ordCost.TotalSkids = ordDetails.QTY;
                    }

                    ordCost.ConsigneeCity = this.normalOrder.ShipperLoc.City;  //txtCCity.Text;
                    ordCost.ShipperCity = this.normalOrder.ReturnServiceLoc.City; //txtSCity.Text;
                    ordCost.consigneePostalCode = this.normalOrder.ReturnServiceLoc.PostalCode;  //txtCPostalCode.Text;
                    ordCost.shipperPostalCode = this.normalOrder.ConsigneeLoc.PostalCode; //txtSPostalCode.Text;

                }
                else
                {
                    ordCost.shipperCityRegion = this.normalOrder.ShipperLoc.cityRegion;
                    ordCost.consigneeCityRegion = this.normalOrder.ConsigneeLoc.cityRegion; // cmbCCityRegion.SelectedValue.ToString();
                    if (Global.IsNumeric(ordDetails.QTY) && ordDetails.VehicleMode.Equals("T"))
                    {
                        ordCost.TotalSkids = ordDetails.QTY;
                    }

                    ordCost.ConsigneeCity = this.normalOrder.ConsigneeLoc.City;  //txtCCity.Text;
                    ordCost.ShipperCity = this.normalOrder.ShipperLoc.City; //txtSCity.Text;
                    ordCost.consigneePostalCode = this.normalOrder.ConsigneeLoc.PostalCode;  //txtCPostalCode.Text;
                    ordCost.shipperPostalCode = this.normalOrder.ShipperLoc.PostalCode; //txtSPostalCode.Text;

                }
                if (ordDetails.VehicleMode.Equals("V"))
                {
                    ordCost.vanChargesFlag = true;
                }
                else if (ordDetails.VehicleMode.Equals("C"))
                {
                    ordCost.carChargesFlag = true;
                }

                if (ordDetails.VehicleMode.Equals("T"))
                {
                    ordCost.CalculateCost("C");
                }
                else
                {
                    ordCost.CalculateCost("Z");
                }


            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

        }


        public DataTable OrderPOD()
        {
            DataTable myPOD = new DataTable();
            SqlCommand Cmd;
            SqlConnection con;
            con = new SqlConnection();
            con.ConnectionString = Global.GlobalConnectionstring;
            SqlDataAdapter dataAdp;

            try
            {
                con.Open();
                Cmd = new SqlCommand();
                Cmd.CommandText = "GetGlobalPOD";
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Connection = con;
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = this.normalOrder.ordDetails.OrderNumber;
                Cmd.Parameters.Add(parm);

                dataAdp = new SqlDataAdapter();
                dataAdp.SelectCommand = Cmd;
                dataAdp.Fill(myPOD);
            }
            catch (Exception ex1)
            {

                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append("GloberOrder:OrderPOD(); "));
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
            return myPOD;

        }

        public void setWebOrder()
        {
            //normalOrder.ordDetails.PaymentMethod = 
        }

        public stDeliveryDetails GetDriversDeliveryInfo(ref string driverNum)
        {
            stDeliveryDetails myDetails = new stDeliveryDetails();
            myDetails.driverNum = string.Empty;
            myDetails.driverName = string.Empty;
            myDetails.deliveryType = string.Empty;
            myDetails.driverPhone = string.Empty;
            myDetails.passedoffAt = string.Empty;
            myDetails.pickedUpAt = string.Empty;
            myDetails.pod1At = string.Empty;
            myDetails.pod2At = string.Empty;
            myDetails.rpassedoffAt = string.Empty;
            myDetails.rpickedUpAt = string.Empty;
            myDetails.rpod1At = string.Empty;
            myDetails.rpod2At = string.Empty;
            try
            {
                DataTable myInfo = Global.GetGlobalDriverDeliveryInfo(this.normalOrder.ordDetails.OrderNumber, driverNum);
                if (myInfo != null && myInfo.Rows.Count > 0)
                {
                    DataRow myRw = myInfo.Rows[0];
                    if (myRw["DriverName"] != null)
                    {
                        myDetails.driverName = myRw["DriverName"].ToString();
                    }
                    if (myRw["PrimaryPhone"] != null)
                    {
                        myDetails.driverPhone = myRw["PrimaryPhone"].ToString();
                    }
                    if (myRw["ReturnServiceFlag"] != null)
                    {
                        myDetails.ReturnFlag = bool.Parse(myRw["ReturnServiceFlag"].ToString());
                    }
                    if (myRw["DeliveryType"] != null)
                    {
                        myDetails.deliveryType = myRw["DeliveryType"].ToString();
                    }
                    if (myRw["PodDate"] != null)
                    {
                        myDetails.pod1At = myRw["PodDate"].ToString();
                    }

                    if (myDetails.ReturnFlag)
                    {
                        if (myDetails.deliveryType.ToUpper().Equals("O"))
                        {

                            if (myRw["RActivityDateTime"] != null)
                            {
                                myDetails.rpickedUpAt = myRw["RActivityDateTime"].ToString();
                            }
                        }
                        else
                        {
                            if (myRw["RActivityDateTime"] != null)
                            {
                                myDetails.rpassedoffAt = myRw["RActivityDateTime"].ToString();
                            }

                        }

                        if (myRw["RPodDate"] != null)
                        {
                            myDetails.rpod1At = myRw["RPodDate"].ToString();
                        }
                    }
                    else
                    {
                        if (myDetails.deliveryType.ToUpper().Equals("O"))
                        {

                            if (myRw["ActivityDateTime"] != null)
                            {
                                myDetails.pickedUpAt = myRw["ActivityDateTime"].ToString();
                            }
                        }
                        else
                        {
                            if (myRw["ActivityDateTime"] != null)
                            {
                                myDetails.passedoffAt = myRw["ActivityDateTime"].ToString();
                            }

                        }
                    }
                }
            }
            catch (Exception ex1)
            {

                throw ex1;
            }
            return myDetails;
        }

        public MFIOrder(string mOrderNum)
        {
            normalOrder = new Order();
            if (mOrderNum.Equals(Global.blankOrderNum.ToString()))
            {
                normalOrder.ordDetails.OrderStatus = OrderStatusType.WDP;
                GetAddServices(Global.blankOrderNum.ToString(), false);
            }
            else
            {
                HydrateOrder(mOrderNum);
            }
        }

        public void CopyFrom(ref GlobalOrder pGlobalOrder)
        {
            //Copy normal order's Customers
            this.normalOrder.biller = pGlobalOrder.normalOrder.biller;
            this.normalOrder.consignee = pGlobalOrder.normalOrder.consignee;
            this.normalOrder.shipper = pGlobalOrder.normalOrder.shipper;

            //Copy normal order's Locations
            this.normalOrder.BillerLoc = pGlobalOrder.normalOrder.BillerLoc;
            this.normalOrder.ConsigneeLoc = pGlobalOrder.normalOrder.ConsigneeLoc;
            this.normalOrder.ShipperLoc = pGlobalOrder.normalOrder.ShipperLoc;
            this.normalOrder.ordDetails = pGlobalOrder.normalOrder.ordDetails;
            this.normalOrder.returnordDetails = pGlobalOrder.normalOrder.returnordDetails;
            this.normalOrder.ReturnServiceLoc = pGlobalOrder.normalOrder.ReturnServiceLoc;
            this.bHasReturnService = pGlobalOrder.bHasReturnService;
        }

        public void CopyFrom(ref GlobalOrder pGlobalOrder, bool MarkedNew)
        {
            CopyFrom(ref pGlobalOrder);

            this.normalOrder.biller.status = statusCode.New;
            this.normalOrder.consignee.status = statusCode.New;
            this.normalOrder.shipper.status = statusCode.New;

            this.normalOrder.BillerLoc.LocationID = Guid.Empty;
            this.normalOrder.BillerLoc.status = statusCode.New;
            this.normalOrder.ConsigneeLoc.LocationID = Guid.Empty;
            this.normalOrder.ConsigneeLoc.status = statusCode.New;
            this.normalOrder.ShipperLoc.LocationID = Guid.Empty;
            this.normalOrder.ShipperLoc.status = statusCode.New;

            this.normalOrder.ordDetails.status = statusCode.New;
            this.normalOrder.returnordDetails.status = statusCode.New;
            this.normalOrder.ReturnServiceLoc.status = statusCode.New;
            this.bHasReturnService = pGlobalOrder.bHasReturnService;

        }
        private void FillOrderDetails(ref DataRow dtMain, ref stOrderDetails pordDetails)
        {
            pordDetails.ActualWeight = 0;
            if (dtMain["ActualWeight"] != null && Global.IsNumeric(dtMain["ActualWeight"].ToString()))
            {
                pordDetails.ActualWeight = decimal.Parse(dtMain["ActualWeight"].ToString());
            }
            pordDetails.ServiceType = dtMain["ServiceType"].ToString();
            pordDetails.VehicleMode = dtMain["VehicleMode"].ToString();
            if (dtMain["QTY"] != null && Global.IsNumeric(dtMain["QTY"].ToString()))
            {
                pordDetails.QTY = int.Parse(dtMain["QTY"].ToString());
            }

            if (dtMain["waitTime"] != null && Global.IsNumeric(dtMain["waitTime"].ToString()))
            {
                pordDetails.waitTime = int.Parse(dtMain["waitTime"].ToString());
            }

            if (dtMain.Table.Columns.Contains("OrderGST") && dtMain["OrderGST"] != null && Global.IsNumeric(dtMain["OrderGST"].ToString()))
            {
                pordDetails.GSTValue = decimal.Parse(dtMain["OrderGST"].ToString());
            }

            pordDetails.UNIT = dtMain["UNIT"].ToString();
            pordDetails.SKID = 0;
            if (dtMain["SKID"] != null && Global.IsNumeric(dtMain["SKID"].ToString()))
            {
                pordDetails.SKID = int.Parse(dtMain["SKID"].ToString());
            }

            pordDetails.status = statusCode.Update;
            pordDetails.ReturnServiceID = new Guid(dtMain["ReturnServiceID"].ToString());

        }

        private void GetAddServices(string ordNum, bool ForReturnService)
        {
            GlobalAdditionalServices srvs = new GlobalAdditionalServices(ordNum);
            srvs.PopulateServices(ForReturnService);
            if (!ForReturnService)
            {
                this.normalOrder.ordDetails.AddServices = srvs.GlobalAddServices;
            }
            else
            {
                this.normalOrder.returnordDetails.AddServices = srvs.GlobalAddServices;
            }
        }

        private void HydrateOrder(string ordNum)
        {
            this.OrderStatuscode = statusCode.Update;

            DataSet globalOrderSet = Global.GetGlobalDelivery(ordNum);

            //Fill Order Details
            DataRow dtMain = globalOrderSet.Tables[0].Rows[0];

            if (!dtMain["InvoiceID"].Equals(DBNull.Value))
            {
                normalOrder.ordDetails.InvoiceNumber = dtMain["InvoiceID"].ToString();
            }

            if (!dtMain["InvoiceDate"].Equals(DBNull.Value) && !string.IsNullOrEmpty(dtMain["InvoiceDate"].ToString()))
            {
                normalOrder.ordDetails.InvoiceDate = Global.GetASCGDATE(dtMain["InvoiceDate"].ToString()).ToString("MM/dd/yyyy");
            }

            normalOrder.ordDetails.WayBillNumber = dtMain["WayBillNumber"].ToString();
            normalOrder.ordDetails.CustomerReferenceNum = dtMain["CustomerReferenceNum"].ToString();
            normalOrder.ordDetails.Department = dtMain["Department"].ToString();
            normalOrder.ordDetails.OrderedBy = dtMain["OrderedBy"].ToString();
            normalOrder.ordDetails.AppointeeName = (dtMain["AppointeeName"] == null) ? string.Empty : dtMain["AppointeeName"].ToString();
            normalOrder.ordDetails.ContactPhone1 = (dtMain["ContactPhone1"] == null) ? string.Empty : dtMain["ContactPhone1"].ToString();
            normalOrder.ordDetails.Memo = dtMain["Memo"].ToString();
            normalOrder.ordDetails.OrderNumber = dtMain["OrderNumber"].ToString();
            normalOrder.ordDetails.PaymentMethod = dtMain["PaymentMethod"].ToString();
            normalOrder.ordDetails.PrintMemoOnInvoice = false;

            if (!string.IsNullOrEmpty(dtMain["PrintMemoOnInvoice"].ToString()))
            {
                normalOrder.ordDetails.PrintMemoOnInvoice = bool.Parse(dtMain["PrintMemoOnInvoice"].ToString());
            }
            normalOrder.ordDetails.ReturnServiceID = new Guid(dtMain["ReturnServiceID"].ToString());
            normalOrder.ordDetails.dispatchedAt = dtMain["DispatchedDateTime"].ToString();
            normalOrder.ordDetails.RdispatchedAt = dtMain["RDispatchedDateTime"].ToString();
            normalOrder.ordDetails.createdDateTime = dtMain["CreatedDateTime"].ToString();
            normalOrder.ordDetails.AWBNum = (dtMain["AWBNum"] == null) ? string.Empty : dtMain["AWBNum"].ToString();
            normalOrder.ordDetails.CaroContainerNum = (dtMain["CargoControlNum"] == null) ? string.Empty : dtMain["CargoControlNum"].ToString();
            switch (dtMain["OrderStatus"].ToString().ToUpper())
            {
                case "IPR":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.IPR;
                    break;
                case "RDD":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.RDD;
                    break;
                case "RDP":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.RDP;
                    break;
                case "RPK":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.RPK;
                    break;
                case "WDA":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WDA;
                    break;
                case "WDP":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WDP;
                    break;
                case "WFB":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WFB;
                    break;
                case "WFD":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WFD;
                    break;
                case "WFP":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WFP;
                    break;
                case "WPK":
                    normalOrder.ordDetails.OrderStatus = OrderStatusType.WPK;
                    break;
            }



            if (dtMain["WebIndicator"] == null)
            {
                normalOrder.ordDetails.IsWebOrder = false;
            }
            else
            {
                normalOrder.ordDetails.IsWebOrder = bool.Parse(dtMain["WebIndicator"].ToString());
            }

            if (dtMain["OrderPercentageDiscount"] != null && Global.IsNumeric(dtMain["OrderPercentageDiscount"].ToString()))
            {
                normalOrder.ordDetails.OrderPercentageDiscount = decimal.Parse(dtMain["OrderPercentageDiscount"].ToString());
            }
            if (dtMain["OrderSurchargeRate"] != null && Global.IsNumeric(dtMain["OrderSurchargeRate"].ToString()))
            {
                normalOrder.ordDetails.OrderSurchargeRate = decimal.Parse(dtMain["OrderSurchargeRate"].ToString());
            }

            FillOrderDetails(ref dtMain, ref normalOrder.ordDetails);

            //Fill Locations
            DataRow[] rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["ConsigneeLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.ConsigneeLoc);
            }

            rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["ShipperLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.ShipperLoc);
            }

            rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["BillingLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.BillerLoc);
            }

            rw = globalOrderSet.Tables[1].Select("LocationID = '" + dtMain["ReturnSrvLocationID"].ToString() + "'");
            if (rw.Length > 0)
            {
                FillLocation(ref rw[0], ref normalOrder.ReturnServiceLoc);
            }

            //Fill Customers
            foreach (DataRow dtrw in globalOrderSet.Tables[2].Rows)
            {
                switch (dtrw["CustomerIndictor"].ToString())
                {
                    case "C":
                        FillCustomer(dtrw, ref normalOrder.consignee);
                        break;
                    case "S":
                        FillCustomer(dtrw, ref normalOrder.shipper);
                        break;
                    case "R":
                        FillCustomer(dtrw, ref normalOrder.ReturnServiceConsignee);
                        break;
                    case "B":
                        FillCustomer(dtrw, ref normalOrder.biller);
                        break;
                }
            }

            //Fill Return Service
            this.bHasReturnService = false;
            if (globalOrderSet.Tables[3].Rows.Count > 0)
            {
                DataRow rwReturn = globalOrderSet.Tables[3].Rows[0];
                FillOrderDetails(ref rwReturn, ref normalOrder.returnordDetails);
                normalOrder.returnordDetails.GSTValue = normalOrder.ordDetails.GSTValue;
                normalOrder.returnordDetails.OrderSurchargeRate = normalOrder.ordDetails.OrderSurchargeRate;
                normalOrder.returnordDetails.OrderNumber = normalOrder.ordDetails.OrderNumber;
                this.bHasReturnService = bool.Parse(globalOrderSet.Tables[3].Rows[0]["Active"].ToString());
                normalOrder.returnordDetails.AWBNum = (rwReturn["AWBNum"] == null) ? string.Empty : rwReturn["AWBNum"].ToString();
                normalOrder.returnordDetails.CaroContainerNum = (rwReturn["CargoControlNum"] == null) ? string.Empty : rwReturn["CargoControlNum"].ToString();


            }

            GetAddServices(ordNum, false);
            //Fill Cost
            if (this.bHasReturnService)
            {
                rw = globalOrderSet.Tables[4].Select("ReturnServiceID = '" + this.normalOrder.returnordDetails.ReturnServiceID.ToString() + "'");
                if (rw.Length > 0)
                {
                    FillCost(rw[0], ref normalOrder.returnorderCost);
                }

                rw = globalOrderSet.Tables[4].Select("ReturnServiceID <> '" + this.normalOrder.returnordDetails.ReturnServiceID.ToString() + "'");
                if (rw.Length > 0)
                {
                    FillCost(rw[0], ref normalOrder.orderCost);
                }
                normalOrder.returnordDetails.status = statusCode.Update;
                GetAddServices(ordNum, true);
            }
            else
            {
                foreach (DataRow ordcost in globalOrderSet.Tables[4].Rows)
                {
                    if (string.IsNullOrEmpty(ordcost["ReturnServiceID"].ToString()) || ordcost["ReturnServiceID"].Equals(Guid.Empty))
                    {
                        FillCost(ordcost, ref normalOrder.orderCost);
                    }
                }

            }




        }

        private void FillCost(DataRow rw, ref GlobalOrderCost ordCost)
        {
            ordCost.agreedSurcharge = this.normalOrder.biller.SurchargeRate;
            ordCost.ApplyGST = normalOrder.biller.GSTApplicable;
            try
            {
                ordCost.carCharges = decimal.Parse(rw["CarCharges"].ToString());
            }
            catch { }

            try
            {
                ordCost.OrderSysBasicCost = decimal.Parse(rw["OrdSysBasicCost"].ToString());
            }
            catch { }

            try
            {
                ordCost.OrderOverRideBasicCost = decimal.Parse(rw["OrdOverRideBasicCost"].ToString());
            }
            catch { }

            try
            {
                ordCost.loadUnloadCharges = decimal.Parse(rw["LoadUnloadCharge"].ToString());
            }
            catch { }

            try
            {
                ordCost.WaitCarChargeUnit = decimal.Parse(rw["WaitingCarUnitCharge"].ToString());
            }
            catch { }

            ordCost.OrdChargeID = new Guid(rw["OrdChargeID"].ToString());
            ordCost.ordCostStatus = statusCode.Update;

            ordCost.OrderBasicCost = decimal.Parse(rw["OrdBasicValue"].ToString());
            if (Global.IsNumeric(rw["OrdValue"].ToString()))
            {
                ordCost.OrderCost = decimal.Parse(rw["OrdValue"].ToString());
            }

            ordCost.OrderFinalCost = decimal.Parse(rw["OrdFinalNetValue"].ToString());
            ordCost.OrderGST = decimal.Parse(rw["OrdGST"].ToString());

            try
            {
                ordCost.WaitVanChargeUnit = decimal.Parse(rw["WaitingVanUnitCharge"].ToString());
            }
            catch { }

            try
            {
                ordCost.WeightChargeUnit = decimal.Parse(rw["WeigthUnitCharge"].ToString());
            }
            catch { }

            try
            {
                ordCost.WaitCarChargeFreeLimit = decimal.Parse(rw["WaitingVANFreeLimit"].ToString());
            }
            catch { }

            try
            {
                ordCost.WaitVanChargeFreeLimit = decimal.Parse(rw["WaitingCarFreeLimit"].ToString());
            }
            catch { }

            if (Global.IsNumeric(rw["OrdSysFuel"].ToString()))
            {
                ordCost.SurchargeAmtFromOrder = decimal.Parse(rw["OrdSysFuel"].ToString());
            }
            if (Global.IsNumeric(rw["OrdSysGst"].ToString()))
            {
                ordCost.OrderGST = decimal.Parse(rw["OrdSysGst"].ToString());
            }

            //ordCost.orderPaymentBase = normalOrder.biller
            if (Global.IsNumeric(rw["OrdSaving"].ToString()))
            {
                ordCost.OrderSaving = decimal.Parse(rw["OrdSaving"].ToString());
            }
            if (rw["OrdOverRideValue"] != DBNull.Value)
            {
                ordCost.OverrideOrderCost = decimal.Parse(rw["OrdOverRideValue"].ToString());
            }
            if (ordCost.OverrideOrderCost > 0)
            {
                ordCost.OverRideOrderFinalCost = decimal.Parse(rw["OrdFinalNetValue"].ToString());
            }
            if (rw["OrdOverRideGST"] != DBNull.Value)
            {
                ordCost.OverRideOrderGSTcharge = decimal.Parse(rw["OrdOverRideGST"].ToString());
            }
            //ordCost.OverRideOrderSaving = 0;
            if (rw["OrdOverRideFuel"] != DBNull.Value)
            {
                ordCost.OverRideSurchargeAmtFromOrder = decimal.Parse(rw["OrdOverRideFuel"].ToString());
            }
            if (!string.IsNullOrEmpty(rw["ReturnServiceID"].ToString()))
            {
                ordCost.ReturnServiceID = new Guid(rw["ReturnServiceID"].ToString());
            }
            else
            {
                ordCost.ReturnServiceID = Guid.Empty;
            }
            //ordCost.ServiceType = n;
            // ordCost.SpecialDiscount = 0;
            //ordCost.SurchargeAmtFromOrder = decimal.Parse(rw["OrdFuel"].ToString()); 
            //ordCost.TotalMileage = 0;
            try
            {
                ordCost.loadUnloadCharges = decimal.Parse(rw["LoadUnloadCharge"].ToString());
            }
            catch { }

            try
            {
                ordCost.AddServChrgs = decimal.Parse(rw["OrdAddChrgs"].ToString());
            }
            catch { }

            try
            {
                ordCost.AddServGST = decimal.Parse(rw["OrdAddGSTChrgs"].ToString());
            }
            catch { }

            try
            {
                ordCost.vanCharges = decimal.Parse(rw["VanCharge"].ToString());
            }
            catch { }

            try
            {
                ordCost.waitCharges = decimal.Parse(rw["WaitCharges"].ToString());
            }
            catch { }

            try
            {
                ordCost.weightCharges = decimal.Parse(rw["WeightCharges"].ToString());
            }
            catch { }


            ordCost.status = statusCode.Update;
        }

        private void FillCustomer(DataRow rw, ref stCustomer cust)
        {
            cust.CustomerNum = short.Parse(rw["CustomerNum"].ToString().ToString());
            cust.customerType = rw["CustomerIndictor"].ToString();
            cust.GSTApplicable = false;
            if (!string.IsNullOrEmpty(rw["GSTApplicable"].ToString()))
            {
                cust.GSTApplicable = bool.Parse(rw["GSTApplicable"].ToString());
            }
            if (Global.IsNumeric(rw["SpecialDiscount"].ToString()))
            {
                cust.SpecialDiscount = decimal.Parse(rw["SpecialDiscount"].ToString());
            }
            cust.Name = rw["CustomerName"].ToString();
            cust.ContactName = rw["ContactName1"].ToString();
            cust.ContactPhone = rw["Contact1Number"].ToString();
            cust.orderCustID = new Guid(rw["OrdCustID"].ToString());
            cust.paymentBase = rw["PaymentBase"].ToString();
            cust.status = statusCode.Update;
        }

        private void FillLocation(ref DataRow rw, ref stLocation loc)
        {
            loc.City = rw["City"].ToString();
            loc.Country = rw["Country"].ToString();
            if (!string.IsNullOrEmpty(rw["PickUpDeliveryDate"].ToString()))
            {
                string[] datePart = rw["PickUpDeliveryDate"].ToString().Split('/');
                loc.DeliveryDate = new DateTime(int.Parse(datePart[2]), int.Parse(datePart[0]), int.Parse(datePart[1]));

                //datePart = rw["PickUpDeliveryTime"].ToString().Split('/');
                loc.DeliveryTime = DateTime.Parse(rw["PickUpDeliveryTime"].ToString());
            }
            loc.LocationID = new Guid(rw["LocationID"].ToString());
            loc.PostalCode = rw["PostalCode"].ToString();
            loc.Province = rw["Province"].ToString();
            loc.status = statusCode.Update;
            loc.StreetLocation = rw["StreetLocation"].ToString();
            loc.Suite = rw["Suite"].ToString();
            loc.cityRegion = rw["CityRegion"].ToString();
            loc.status = statusCode.Update;

        }

        public void setDeliveryAndDrivers()
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection();
            DataTable tbl;
            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();

            try
            {
                con.ConnectionString = Global.GlobalConnectionstring;
                cmd.CommandText = "GetGlobalDeliveryDrivers";
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = this.normalOrder.ordDetails.OrderNumber;
                cmd.Parameters.Add(parm);
                con.Open();
                cmd.Connection = con;
                adp.SelectCommand = cmd;
                adp.Fill(ds);
                tbl = ds.Tables[0];
                bool bForReturn = false;

                foreach (DataRow podRw in ds.Tables[1].Rows)
                {
                    if (bool.Parse(podRw["ReturnServiceFlag"].ToString()))
                    {
                        _RetDelDriverNum = podRw["PodDriver"].ToString();
                        _RetDeliveryDate = podRw["PodDate"].ToString();
                        _RetDeliveryTime = podRw["PodTime"].ToString();
                        _RetDeliverySignature = podRw["ApprovedBy"].ToString();
                    }
                    else
                    {
                        _DelDriverNum = podRw["PodDriver"].ToString();
                        _DeliveryDate = podRw["PodDate"].ToString();
                        _DeliveryTime = podRw["PodTime"].ToString();
                        _DeliverySignature = podRw["ApprovedBy"].ToString();
                        //if this is return delivery then return delivery is being picked up by 
                        //same driver
                        if (!this.normalOrder.ordDetails.ReturnServiceID.Equals(Guid.Empty))
                        {
                            _RetPUDriverNum = _DelDriverNum;
                        }
                    }
                }

                //Calculate 
                foreach (DataRow rw in tbl.Rows)
                {
                    bForReturn = (bool)rw["ReturnServiceFlag"];

                    //PickUp deriver
                    if (rw["DeliveryType"].ToString().ToUpper().Equals("O"))
                    {
                        _PUDriverNum = rw["DriverNum"].ToString();
                    }
                    else if (rw["DeliveryType"].ToString().ToUpper().Equals("P"))
                    {
                        if (!rw["DriverNum"].ToString().Equals(_RetPUDriverNum))
                        {
                            if (bForReturn)
                            {
                                _RetPassOffDriverNum = rw["DriverNum"].ToString();
                            }
                            else
                            {
                                _PassOffDriverNum = rw["DriverNum"].ToString();
                            }
                        }
                    }

                }


            }

            catch
            {

            }
            finally
            {
                if (con != null) con.Close();
            }


        }



        #region "Save Order"

        public bool SaveMFIOrder()
        {
            bool bResult = true;
            SqlParameter parm = new SqlParameter();
            SqlCommand Cmd = new SqlCommand();
            Cmd.CommandType = CommandType.StoredProcedure;
            SqlTransaction tr = null;
            SqlConnection con = new SqlConnection(Global.GlobalConnectionstring);
            try
            {
                con.Open();
                Cmd.Connection = con;
                tr = con.BeginTransaction();
                if (this.OrderStatuscode == statusCode.New)
                {
                    normalOrder.ordDetails.status = statusCode.New;

                    normalOrder.biller.orderCustID = Guid.NewGuid();
                    normalOrder.consignee.orderCustID = Guid.NewGuid();
                    normalOrder.shipper.orderCustID = Guid.NewGuid();

                    normalOrder.ConsigneeLoc.LocationID = Guid.NewGuid();
                    normalOrder.ShipperLoc.LocationID = Guid.NewGuid();
                    normalOrder.BillerLoc.LocationID = Guid.NewGuid();

                    normalOrder.orderCost.OrdChargeID = Guid.NewGuid();

                    if (this.bHasReturnService)
                    {
                        normalOrder.ReturnServiceLoc.LocationID = Guid.NewGuid();
                        normalOrder.ReturnServiceLoc.status = statusCode.New;

                        normalOrder.returnordDetails.ReturnServiceID = Guid.NewGuid();
                        normalOrder.returnordDetails.status = statusCode.New;
                        normalOrder.ordDetails.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                        normalOrder.returnorderCost.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                        normalOrder.ReturnServiceConsignee.orderCustID = Guid.NewGuid();
                        normalOrder.ReturnServiceConsignee.status = statusCode.New;

                        normalOrder.returnorderCost.OrdChargeID = Guid.NewGuid();
                        normalOrder.returnorderCost.OrdChargeID = normalOrder.returnordDetails.ReturnServiceID;

                    }
                }
                else
                {
                    if (this.bHasReturnService)
                    {
                        if (normalOrder.ReturnServiceLoc.LocationID == null ||
                            normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                        {
                            normalOrder.ReturnServiceLoc.LocationID = Guid.NewGuid();
                            normalOrder.ReturnServiceLoc.status = statusCode.New;

                            normalOrder.returnordDetails.ReturnServiceID = Guid.NewGuid();
                            normalOrder.returnordDetails.status = statusCode.New;
                            normalOrder.ordDetails.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                            normalOrder.returnorderCost.ReturnServiceID = normalOrder.returnordDetails.ReturnServiceID;
                            normalOrder.ReturnServiceConsignee.orderCustID = Guid.NewGuid();
                            normalOrder.ReturnServiceConsignee.status = statusCode.New;

                            normalOrder.returnorderCost.OrdChargeID = Guid.NewGuid();

                        }
                        else
                        {
                            normalOrder.ReturnServiceLoc.status = statusCode.Update;
                            normalOrder.returnordDetails.status = statusCode.Update;
                            normalOrder.ReturnServiceConsignee.status = statusCode.Update;
                            normalOrder.orderCost.status = statusCode.Update;
                        }

                    }
                    else
                    {
                        if (!normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                        {
                            normalOrder.ReturnServiceLoc.status = statusCode.Delete;
                            normalOrder.returnordDetails.status = statusCode.Delete;
                            normalOrder.ReturnServiceConsignee.status = statusCode.Delete;
                            normalOrder.orderCost.status = statusCode.Delete;
                        }
                    }
                }

                bResult = SaveOrder(ref tr);

                if (bResult)
                {
                    tr.Commit();
                }
                else
                {
                    tr.Rollback();
                    throw new Exception("The order couldn't save");
                }

                try
                {
                    Global.setOrdChargesCorrectly(this.normalOrder.ordDetails.OrderNumber);
                }
                catch
                {

                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                if (tr.Connection != null) tr.Rollback();
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :SaveGlobalOrder(); "));
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
            return bResult;
        }

        private bool SaveOrder(ref SqlTransaction tr)
        {
            bool bResult = true;
            try
            {
                if (!this.SetOrderDetails(ref tr))
                {
                    throw new Exception("Order Details couldn't save");
                }

                if (!this.SetOrderLocations(ref tr))
                {
                    return false;
                }

                if (!this.SetOrderCustomers(ref tr))
                {
                    return false;
                }

                if (!this.SetOrderCost(ref tr, ref normalOrder.orderCost))
                {
                    return false;
                }

                if (this.bHasReturnService)
                {
                    if (!this.SetOrderCost(ref tr, ref normalOrder.returnorderCost))
                    {
                        return false;
                    }
                }

                if (!this.SetOrderReturnService(ref tr))
                {
                    throw new Exception("Order Details couldn't save");
                }

                GlobalAdditionalServices srvs = new GlobalAdditionalServices(this.normalOrder.ordDetails.OrderNumber);
                if (!srvs.SaveOrderServices(ref tr, this.normalOrder.ordDetails.AddServices, this.normalOrder.ordDetails.OrderNumber, this.UserID.ToString(), false))
                {
                    throw new Exception("Order Services couldn't be saved");
                }
                if (this.bHasReturnService)
                {
                    if (!srvs.SaveOrderServices(ref tr, this.normalOrder.returnordDetails.AddServices, this.normalOrder.ordDetails.OrderNumber, this.UserID.ToString(), true))
                    {
                        throw new Exception("Order Services couldn't be saved");
                    }
                }
            }
            catch (Exception ex1)
            {
                return false;
                throw ex1;

            }
            finally
            {

            }

            return true;

        }

        private bool SetOrderReturnService(ref SqlTransaction tr)
        {
            if (this.OrderStatuscode == statusCode.New && !this.bHasReturnService) return true;
            if (this.normalOrder.returnordDetails.ReturnServiceID.Equals(Guid.Empty)) return true;
            bool bResult = true;
            SqlParameter parm;
            SqlCommand Cmd = new SqlCommand();
            SqlDataAdapter dataAdp = new SqlDataAdapter();

            try
            {

                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerReferenceNum";
                parm.Value = normalOrder.ordDetails.CustomerReferenceNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@QTY";
                parm.Value = normalOrder.returnordDetails.QTY;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UNIT";
                parm.Value = normalOrder.returnordDetails.UNIT;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@SKID";
                parm.Value = normalOrder.returnordDetails.SKID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ActualWeight";
                parm.Value = normalOrder.returnordDetails.ActualWeight;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderedBy";
                parm.Value = normalOrder.ordDetails.OrderedBy;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnServiceID";
                parm.Value = normalOrder.returnordDetails.ReturnServiceID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ServiceType";
                parm.Value = normalOrder.returnordDetails.ServiceType;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@VehicleMode";
                parm.Value = normalOrder.returnordDetails.VehicleMode;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CargoControlNum";
                parm.Value = normalOrder.returnordDetails.CaroContainerNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@AWBNum";
                parm.Value = normalOrder.returnordDetails.AWBNum;
                Cmd.Parameters.Add(parm);

                if (this.normalOrder.ReturnServiceLoc.status == statusCode.New)
                {
                    Cmd.CommandText = "InsertGlobalReturnService";
                }
                else
                {
                    Cmd.CommandText = "UpdateGlobalReturnService";
                    parm = new SqlParameter();
                    parm.ParameterName = "@Active";
                    parm.Value = 1;
                    if (!this.bHasReturnService)
                        parm.Value = 0;
                    Cmd.Parameters.Add(parm);
                }

                Cmd.ExecuteNonQuery();

            }

            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :SetOrderDetails(); "));
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
                Cmd = null;
            }
            return bResult;
        }

        private bool SetOrderDetails(ref SqlTransaction tr)
        {
            bool bResult = true;
            SqlParameter parm;
            SqlCommand Cmd = new SqlCommand();
            SqlDataAdapter dataAdp = new SqlDataAdapter();

            try
            {
                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;

                //Save Order Details and get New Order Number
                if (OrderStatuscode == statusCode.New)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@NextOrderNum";
                    parm.DbType = DbType.String;
                    parm.Direction = ParameterDirection.Output;
                    parm.Size = 50;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.Value = UserID;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.Value = DateTime.Now;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@WebIndicator";
                    parm.Value = normalOrder.ordDetails.IsWebOrder;
                    Cmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@OrderNumber";
                    parm.Value = normalOrder.ordDetails.OrderNumber;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.Value = UserID;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.Value = DateTime.Now;
                    Cmd.Parameters.Add(parm);
                }

                parm = new SqlParameter();
                parm.ParameterName = "@OrderSurchargeRate";
                parm.Value = normalOrder.ordDetails.OrderSurchargeRate;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderPercentageDiscount";
                parm.Value = normalOrder.ordDetails.OrderPercentageDiscount;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Memo";
                parm.Value = normalOrder.ordDetails.Memo;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PrintMemoOnInvoice";
                parm.Value = normalOrder.ordDetails.PrintMemoOnInvoice;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@WayBillNumber";
                parm.Value = normalOrder.ordDetails.WayBillNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderStatus";
                parm.Value = normalOrder.ordDetails.OrderStatus.ToString();
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerReferenceNum";
                parm.Value = normalOrder.ordDetails.CustomerReferenceNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Department";
                parm.Value = normalOrder.ordDetails.Department;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@QTY";
                parm.Value = normalOrder.ordDetails.QTY;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@UNIT";
                parm.Value = normalOrder.ordDetails.UNIT;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@SKID";
                parm.Value = normalOrder.ordDetails.SKID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ActualWeight";
                parm.Value = normalOrder.ordDetails.ActualWeight;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderedBy";
                parm.Value = normalOrder.ordDetails.OrderedBy;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ConsigneeLocationID";
                parm.Value = normalOrder.ConsigneeLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ShipperLocationID";
                parm.Value = normalOrder.ShipperLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@BillingLocationID";
                parm.Value = normalOrder.BillerLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnSrvLocationID";
                parm.Value = normalOrder.ReturnServiceLoc.LocationID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ServiceType";
                parm.Value = normalOrder.ordDetails.ServiceType;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PaymentMethod";
                parm.Value = normalOrder.ordDetails.PaymentMethod;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@InvoiceDueDays";
                parm.Value = normalOrder.ordDetails.InvoiceDueDays;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@EmployeeCommission";
                parm.Value = normalOrder.ordDetails.EmployeeCommission;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@EmployeeNum";
                parm.Value = normalOrder.ordDetails.EmployeeNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@VehicleMode";
                parm.Value = normalOrder.ordDetails.VehicleMode;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnServiceID";
                parm.Value = normalOrder.ordDetails.ReturnServiceID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderGST";
                parm.Value = normalOrder.ordDetails.GSTValue;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CargoControlNum";
                parm.Value = normalOrder.ordDetails.CaroContainerNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@AWBNum";
                parm.Value = normalOrder.ordDetails.AWBNum;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@AppointeeName";
                parm.Value = normalOrder.ordDetails.AppointeeName;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ContactPhone1";
                parm.Value = normalOrder.ordDetails.ContactPhone1;
                Cmd.Parameters.Add(parm);



                if (this.OrderStatuscode == statusCode.New)
                {
                    Cmd.CommandText = "InsertGlobalDeliveryOrder";
                }
                else
                {
                    Cmd.CommandText = "UpdateGlobalDeliveryOrder";
                }

                Cmd.ExecuteNonQuery();
                if (this.OrderStatuscode == statusCode.New)
                {
                    normalOrder.ordDetails.OrderNumber = Cmd.Parameters["@NextOrderNum"].Value.ToString();
                }
            }

            catch (Exception ex1)
            {
                bResult = false;
                mErrorInfo = new StringBuilder("Error Information");
                mErrorInfo.Append(Environment.NewLine);
                mErrorInfo.Append("Function Name: ");
                mErrorInfo.Append(mErrorInfo.Append(this.GetType().Name + " :SetOrderDetails(); "));
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
                Cmd = null;
            }
            return bResult;
        }

        #region "Save Location"
        private bool SetOrderLocations(ref SqlTransaction tr)
        {
            bool bResult = true;

            try
            {
                //set shipper
                bResult = this.setOrderLocation(ref tr, ref normalOrder.ShipperLoc);

                if (bResult)
                {
                    //set consignee
                    bResult = this.setOrderLocation(ref tr, ref normalOrder.ConsigneeLoc);
                }

                if (bResult)
                {
                    if (this.bHasReturnService)
                    {
                        bResult = this.setOrderLocation(ref tr, ref normalOrder.ReturnServiceLoc);
                    }
                    else
                    {
                        if (!normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                        {
                            bResult = this.setOrderLocation(ref tr, ref normalOrder.ReturnServiceLoc);
                        }
                    }
                }

                if (bResult && !string.IsNullOrEmpty(normalOrder.biller.status.ToString()))
                {
                    bResult = this.setOrderLocation(ref tr, ref normalOrder.BillerLoc);
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            return bResult;
        }

        private bool setOrderLocation(ref SqlTransaction tr, ref stLocation myLocation)
        {
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();

            try
            {
                this.setOrderLocationsCommand(ref Cmd, ref myLocation);

                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;
                Cmd.ExecuteNonQuery();


            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            finally
            {
                //con.Close();
                Cmd = null;
            }
            return bResult;

        }

        private bool setOrderLocationsCommand(ref SqlCommand sqlCmd, ref stLocation myLocation)
        {
            SqlParameter parm;
            bool bResult = true;
            try
            {

                if (myLocation.status == statusCode.New)
                {
                    sqlCmd.CommandText = "InsertGlobalOrderLocation";
                }
                else
                {
                    sqlCmd.CommandText = "UpdateGlobalOrderLocation";
                }

                #region "Params for Insert & Update"

                parm = new SqlParameter();
                parm.ParameterName = "@StreetLocation";
                parm.Value = myLocation.StreetLocation;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Suite";
                parm.Value = myLocation.Suite;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@City";
                parm.Value = myLocation.City;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Province";
                parm.Value = myLocation.Province;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Country";
                parm.Value = myLocation.Country;
                sqlCmd.Parameters.Add(parm);


                parm = new SqlParameter();
                parm.ParameterName = "@PickUpDeliveryDate";
                parm.Value = myLocation.DeliveryDate.Month.ToString().PadLeft(2, '0') + "/" +
                             myLocation.DeliveryDate.Day.ToString().PadLeft(2, '0') + "/" +
                             myLocation.DeliveryDate.Year.ToString().PadLeft(4, '0');
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PickUpDeliveryTime";
                parm.Value = myLocation.DeliveryTime.ToShortTimeString();
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PostalCode";
                parm.Value = myLocation.PostalCode;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@LocationID";
                parm.Value = myLocation.LocationID;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = this.normalOrder.ordDetails.OrderNumber;
                sqlCmd.Parameters.Add(parm);


                parm = new SqlParameter();
                parm.ParameterName = "@CityRegion";
                if (myLocation.cityRegion == null || myLocation.cityRegion.Equals(string.Empty))
                {
                    parm.Value = DBNull.Value;
                }
                else
                {
                    parm.Value = myLocation.cityRegion;
                }
                sqlCmd.Parameters.Add(parm);


                #endregion
                if (myLocation.status == statusCode.New)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }



                parm = new SqlParameter();
                parm.ParameterName = "@Active";
                if (myLocation.status == statusCode.Delete)
                {
                    parm.Value = 0;
                }
                else
                {
                    parm.Value = 1;
                }
                sqlCmd.Parameters.Add(parm);
            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;

            }
            return bResult;

        }
        #endregion

        #region "Save Customers"

        private bool SetOrderCost(ref SqlTransaction tr, ref GlobalOrderCost pOrdCost)
        {
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();
            SqlParameter parm;
            try
            {
                Cmd.CommandText = "SetOrderCost";
                Cmd.CommandType = CommandType.StoredProcedure;

                Cmd.Connection = tr.Connection;
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.Transaction = tr;

                parm = new SqlParameter();
                parm.ParameterName = "@OrdNum";
                parm.Value = normalOrder.ordDetails.OrderNumber;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdValue";
                parm.Value = pOrdCost.OrderCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdBasicValue";
                parm.Value = pOrdCost.OrderBasicCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSaving";
                parm.Value = pOrdCost.OrderSaving;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdAddChrgs";
                parm.Value = pOrdCost.AddServChrgs;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdAddGSTChrgs";
                parm.Value = pOrdCost.AddServGST;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdNetValue";
                parm.Value = pOrdCost.OrderFinalCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdInvAmt";
                parm.Value = DBNull.Value;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdInvGST";
                parm.Value = DBNull.Value;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSysValue";
                parm.Value = pOrdCost.OrderCost;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSysFuel";
                parm.Value = pOrdCost.SurchargeAmtFromOrder;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdSysGST";
                parm.Value = pOrdCost.OrderGST;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderSysBasicCost";
                parm.Value = pOrdCost.OrderSysBasicCost;
                Cmd.Parameters.Add(parm);


                if (pOrdCost.OverrideOrderCost > 0)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideValue";
                    parm.Value = pOrdCost.OverrideOrderCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrderOverRideBasicCost";
                    parm.Value = pOrdCost.OrderOverRideBasicCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalBasicValue";
                    parm.Value = pOrdCost.OverrideOrderCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideFuel";
                    parm.Value = pOrdCost.OverRideSurchargeAmtFromOrder;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideGST";
                    parm.Value = pOrdCost.OverRideOrderGSTcharge;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFuel";
                    parm.Value = pOrdCost.OverRideSurchargeAmtFromOrder;
                    Cmd.Parameters.Add(parm);


                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdGST";
                    parm.Value = pOrdCost.OverRideOrderGSTcharge;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalNetValue";
                    parm.Value = pOrdCost.OverRideOrderFinalCost;
                    Cmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalBasicValue";
                    parm.Value = pOrdCost.OrderBasicCost;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideValue";
                    parm.Value = DBNull.Value;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideFuel";
                    parm.Value = DBNull.Value;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdOverRideGST";
                    parm.Value = DBNull.Value;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFuel";
                    parm.Value = pOrdCost.SurchargeAmtFromOrder;
                    Cmd.Parameters.Add(parm);


                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdGST";
                    parm.Value = pOrdCost.OrderGST;
                    Cmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@OrdFinalNetValue";
                    parm.Value = pOrdCost.OrderFinalCost;
                    Cmd.Parameters.Add(parm);

                }
                parm = new SqlParameter();
                parm.ParameterName = "@CreatedBy";
                parm.Value = UserID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@VanCharge";
                parm.Value = pOrdCost.vanCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CarCharges";
                parm.Value = pOrdCost.carCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@WaitCharges";
                parm.Value = pOrdCost.waitCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@WeightCharges";
                parm.Value = pOrdCost.weightCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@LoadUnloadCharge";
                parm.Value = pOrdCost.loadUnloadCharges;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ReturnServiceID";
                parm.Value = pOrdCost.ReturnServiceID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdChargeID";
                parm.Value = pOrdCost.OrdChargeID;
                Cmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Active";
                parm.Value = 1;

                if (pOrdCost.ordCostStatus == statusCode.Delete) parm.Value = 0;

                Cmd.Parameters.Add(parm);
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }

            return bResult;
        }

        private bool SetOrderCustomers(ref SqlTransaction tr)
        {
            bool bResult = true;
            try
            {
                //set shipper
                bResult = this.SetOrderCustomer(ref tr, ref normalOrder.shipper);

                //set consignee
                if (bResult)
                {
                    bResult = this.SetOrderCustomer(ref tr, ref normalOrder.consignee);
                }

                if (bResult && !string.IsNullOrEmpty(normalOrder.biller.status.ToString()))
                {
                    bResult = this.SetOrderCustomer(ref tr, ref normalOrder.biller);
                }

                if (this.bHasReturnService)
                {
                    bResult = this.SetOrderCustomer(ref tr, ref normalOrder.ReturnServiceConsignee);
                }
                else
                {
                    if (!normalOrder.ReturnServiceLoc.LocationID.Equals(Guid.Empty))
                    {
                        bResult = this.SetOrderCustomer(ref tr, ref normalOrder.ReturnServiceConsignee);
                    }
                }
            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;

            }

            return bResult;
        }

        private bool SetOrderCustomer(ref SqlTransaction tr, ref stCustomer myCustomer)
        {
            bool bResult = true;
            SqlCommand Cmd = new SqlCommand();

            try
            {

                bResult = this.setOrderCustomerCommand(ref Cmd, ref myCustomer);

                if (bResult)
                {
                    Cmd.Connection = tr.Connection;
                    Cmd.CommandType = CommandType.StoredProcedure;
                    Cmd.Transaction = tr;
                    Cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            finally
            {
                //con.Close();
                Cmd = null;
            }
            return bResult;
        }

        private bool setOrderCustomerCommand(ref SqlCommand sqlCmd, ref stCustomer myCustomer)
        {
            bool bResult = true;
            SqlParameter parm;
            try
            {
                if (myCustomer.status == statusCode.New)
                {
                    sqlCmd.CommandText = "InsertGlobalOrderCustomer";
                }
                else
                {
                    sqlCmd.CommandText = "UpdateGlobalOrderCustomer";
                }

                #region "Params for Insert & Update"

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerName";
                parm.Value = myCustomer.Name;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@GSTApplicable";
                parm.Value = myCustomer.GSTApplicable;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ContactName";
                parm.Value = myCustomer.ContactName;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@ContactPhone";
                parm.Value = myCustomer.ContactPhone;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@SpecialDiscount";
                parm.Value = myCustomer.SpecialDiscount;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@PaymentBase";
                parm.Value = DBNull.Value;
                if (myCustomer.paymentBase != null)
                {
                    parm.Value = myCustomer.paymentBase;
                }
                sqlCmd.Parameters.Add(parm);



                #endregion
                if (myCustomer.status == statusCode.New)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@CreatedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }
                else
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedBy";
                    parm.Value = UserID;
                    sqlCmd.Parameters.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "@LastModifiedDateTime";
                    parm.Value = DateTime.Now;
                    sqlCmd.Parameters.Add(parm);
                }

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerNum";
                parm.Value = myCustomer.CustomerNum;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrderNumber";
                parm.Value = normalOrder.ordDetails.OrderNumber;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@CustomerIndictor";
                parm.Value = myCustomer.customerType;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@OrdCustID";
                parm.Value = myCustomer.orderCustID;
                sqlCmd.Parameters.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "@Active";
                parm.Value = 1;
                if (myCustomer.status == statusCode.Delete) parm.Value = 0;
                sqlCmd.Parameters.Add(parm);

            }
            catch (Exception ex1)
            {
                bResult = false;
                throw ex1;
            }
            return bResult;
        }

        #endregion
        #endregion


    }

}
