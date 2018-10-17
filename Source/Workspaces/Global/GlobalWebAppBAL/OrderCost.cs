using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace GlobalWebAppBAL
{
   public class GlobalOrderCost
    {
     public statusCode ordCostStatus;
    public CustomersIndicator myCustomer = CustomersIndicator.MFI;
    public bool ApplyGST;
    public string ServiceType;
    public string ConsigneeCity;
    public string ShipperCity;
    public int TotalSkids;
    private DataRow[] rws;
    public decimal OrderGST;
    public decimal agreedSurcharge;
    public decimal SpecialDiscount;
    public decimal OrderCost;
    public decimal OrderBasicCost;
    public decimal OrderSysBasicCost;
    public decimal OrderOverRideBasicCost;
    public decimal SurchargeAmtFromOrder;
    public decimal OrderFinalCost;
    public decimal OrderSaving;
    public decimal OverRideOrderSaving;
    public decimal OverRideOrderFinalCost;
    public decimal OverrideOrderCost;
    public decimal OverRideSurchargeAmtFromOrder;
    public decimal OverRideOrderGSTcharge;
    public decimal GSTValue;
    public decimal AddServGST;
    public decimal AddServChrgs;

    public string orderPaymentBase = string.Empty;
    public string customerPaymentBase = string.Empty;
    public string consigneePostalCode = string.Empty;
    public string shipperPostalCode = string.Empty;

    public string consigneeCityRegion = string.Empty;
    public string shipperCityRegion = string.Empty;

    public string FromZone = string.Empty;
    public string ToZone = string.Empty;

    public string ConsigneeZone = string.Empty;
    public string ShipperZone = string.Empty;

    public decimal TotalMileage;
    private decimal dbvanCharges;
    private decimal dbcarCharges;

    public decimal vanCharges;
    public decimal carCharges;

    public bool vanChargesFlag;
    public bool carChargesFlag;

    public decimal WeightChargeFreeLimit;
    public decimal WaitVanChargeFreeLimit;
    public decimal WaitCarChargeFreeLimit;

    public decimal WeightChargeUnit;
    public decimal WaitVanChargeUnit;
    public decimal WaitCarChargeUnit;

    public decimal weight;
    public decimal wait;

    public decimal weightCharges;
    public decimal waitCharges;
    public decimal loadUnloadCharges;

    public Guid ReturnServiceID = Guid.Empty;
    public Guid OrdChargeID = Guid.Empty;
    public statusCode status;

    public void CalculateCost(string pOrderBasedPayment)
    {
        if (shipperPostalCode.Contains(" "))
        {
            shipperPostalCode = shipperPostalCode.Replace(" ", "");
        }
        if (consigneePostalCode.Contains(" "))
        {
            consigneePostalCode = consigneePostalCode.Replace(" ", "");
        }
        if (shipperPostalCode.Contains("-"))
        {
            shipperPostalCode = shipperPostalCode.Replace("-", "");
        }
        if (consigneePostalCode.Contains("-"))
        {
            consigneePostalCode = consigneePostalCode.Replace("-", "");
        }
        if (shipperPostalCode.Contains(" - "))
        {
            shipperPostalCode = shipperPostalCode.Replace(" - ", "");
        }
        if (consigneePostalCode.Contains(" - "))
        {
            consigneePostalCode = consigneePostalCode.Replace(" - ", "");
        }
        if (shipperPostalCode.Contains(" -"))
        {
            shipperPostalCode = shipperPostalCode.Replace(" -", "");
        }
        if (consigneePostalCode.Contains(" -"))
        {
            consigneePostalCode = consigneePostalCode.Replace(" -", "");
        }
        if (shipperPostalCode.Contains("- "))
        {
            shipperPostalCode = shipperPostalCode.Replace("- ", "");
        }
        if (consigneePostalCode.Contains("- "))
        {
            consigneePostalCode = consigneePostalCode.Replace("- ", "");
        }

        consigneePostalCode = consigneePostalCode.ToUpper();
        shipperPostalCode = shipperPostalCode.ToUpper();

        orderPaymentBase = pOrderBasedPayment;
        if (myCustomer == CustomersIndicator.MFI)
        {
            CalculateSKIDBasedCost();
        }
        else if (myCustomer == CustomersIndicator.Global)
        {
            if (ServiceType.ToString().Equals(DeliverySerrviceType.NP.ToString()))
            {
                GetFinalCost(5);

            }
            else
            {
                GetExtraCostVariables();
                if (orderPaymentBase.Equals("Z"))
                {
                    if (customerPaymentBase.ToUpper().Equals("C"))
                    {
                        CalculateCityBasedCourierCost();
                    }
                    else
                    {
                        CalculateZoneBasedCost();
                    }
                }
                else
                {
                    CalculateSKIDBasedCost();
                }
            }
        }
        else
        {
            GetFinalCost(0);
        }
    }

    private void GetExtraCostVariables()
    {
        DataTable envCostVariance = Global.GetExtraCostVariables();
        WeightChargeFreeLimit = decimal.Parse(envCostVariance.Rows[0]["WeightFreeLimit"].ToString());
        WeightChargeUnit = decimal.Parse(envCostVariance.Rows[0]["WeigthUnitCharge"].ToString());

        WaitCarChargeFreeLimit = decimal.Parse(envCostVariance.Rows[0]["WaitingCarFreeLimit"].ToString());
        WaitVanChargeFreeLimit = decimal.Parse(envCostVariance.Rows[0]["WaitingVANFreeLimit"].ToString());

        WaitCarChargeUnit = decimal.Parse(envCostVariance.Rows[0]["WaitingCarUnitCharge"].ToString());
        WaitVanChargeUnit = decimal.Parse(envCostVariance.Rows[0]["WaitingVanUnitCharge"].ToString());


        dbvanCharges = decimal.Parse(envCostVariance.Rows[0]["VanCharges"].ToString());
        dbcarCharges = decimal.Parse(envCostVariance.Rows[0]["CarCharges"].ToString());

    }

    private void CalculateCityBasedCourierCost()
    {
        string FromCityRegion = string.Empty;
        string ToCityRegion = string.Empty;

        if (TotalSkids <= 0)
        {
            GetFinalCost(0);
            return;
        }


        XDocument CityRegions;
        string fName = Guid.NewGuid().ToString() + ".xml";
        string sXML = Global.GetPostalCodesZones();
        if (string.IsNullOrEmpty(sXML))
        {
            CityRegions = XDocument.Load(System.Configuration.ConfigurationManager.AppSettings["cityregionfile"]);
        }
        else
        {
            CityRegions = XDocument.Parse(sXML);
        }

        var myCityRangeCost = from cityRange in CityRegions.Descendants("CityRegion")
                              where ((string)cityRange.Attribute("From")).Equals(shipperCityRegion) &&
                                    ((string)cityRange.Attribute("To")).Equals(consigneeCityRegion)
                              select cityRange;


        if (myCityRangeCost == null || myCityRangeCost.Count() == 0)
        {
            myCityRangeCost = from cityRange in CityRegions.Descendants("CityRegion")
                              where ((string)cityRange.Attribute("From")).Equals(shipperCityRegion) &&
                                    ((string)cityRange.Attribute("To")).Equals(shipperCityRegion)
                              select cityRange;
        }

        if (myCityRangeCost == null || myCityRangeCost.Count() == 0)
        {
            myCityRangeCost = from cityRange in CityRegions.Descendants("CityRegion")
                              where ((string)cityRange.Attribute("From")).Equals(consigneeCityRegion) &&
                                    ((string)cityRange.Attribute("To")).Equals(consigneeCityRegion)
                              select cityRange;
        }

        if (myCityRangeCost == null || myCityRangeCost.Count() == 0)
        {
            OrderCost = 0;
            GetFinalCost(OrderCost);
            return;
        }

        if (ServiceType.Equals(DeliverySerrviceType.SMD.ToString()))
        {
            OrderCost = decimal.Parse(myCityRangeCost.Elements("SMD").First().Value.ToString());

        }
        else if (ServiceType.Equals(DeliverySerrviceType.RSH.ToString()))
        {
            OrderCost = decimal.Parse(myCityRangeCost.Elements("Rush").First().Value.ToString());
        }
        else if (ServiceType.Equals(DeliverySerrviceType.DIR.ToString()))
        {

            OrderCost = decimal.Parse(myCityRangeCost.Elements("DIR").First().Value.ToString());
        }
        else if (ServiceType.Equals(DeliverySerrviceType.ON.ToString()))
        {

            OrderCost = decimal.Parse(myCityRangeCost.Elements("ON").First().Value.ToString());
        }
        else if (ServiceType.Equals(DeliverySerrviceType.ON1.ToString()) ||
                 ServiceType.Equals(DeliverySerrviceType.ON2.ToString()) ||
                 ServiceType.Equals(DeliverySerrviceType.ON3.ToString()) ||
                 ServiceType.Equals(DeliverySerrviceType.AFT.ToString()))
        {
            string depService = string.Empty;
            string depservoperator = string.Empty;
            decimal depsevChrg = 0;

            XDocument specialService = XDocument.Load("Administration\\SpecialServiceCharges.xml");
            var mySPServiceCost = from spservChrg in specialService.Descendants("service")
                                  where ((string)spservChrg.Attribute("type")).Equals(ServiceType.ToUpper())
                                  select spservChrg;
            foreach (var item in mySPServiceCost)
            {
                depService = ((XElement)item).Attribute("dependent").Value.ToString();
                depservoperator = ((XElement)item).Attribute("opration").Value.ToString();
                depsevChrg = decimal.Parse(((XElement)item).Attribute("addCharg").Value.ToString());
            }

            OrderCost = decimal.Parse(myCityRangeCost.Elements(depService).First().Value.ToString());
            if (depservoperator.Equals("+"))
            {
                OrderCost = OrderCost + depsevChrg;
            }
            else if (depservoperator.Equals("-"))
            {
                OrderCost = OrderCost - depsevChrg;
            }
            else if (depservoperator.Equals("*"))
            {
                OrderCost = OrderCost * depsevChrg;
            }
        }

        OrderCost = OrderCost * TotalSkids;

        //carCharges = 0;
        //if (carChargesFlag)
        //{
        //    if (ToZone.Equals("11") || FromZone.Equals("11"))
        //    {
        //        OrderCost = OrderCost + dbcarCharges;
        //        carCharges = dbcarCharges;
        //    }
        //}

        //vanCharges = 0;
        //if (vanChargesFlag)
        //{
        //    OrderCost = OrderCost + dbvanCharges;
        //    vanCharges = dbvanCharges;
        //}

        //if ((wait - WaitChargeFreeLimit) > 0)
        //{
        //    waitCharges = (wait - WaitChargeFreeLimit) * WaitChargeUnit;
        //    OrderCost = waitCharges;
        //}

        //if ((weight - WeightChargeFreeLimit > 0))
        //{
        //    weightCharges = (weight - WeightChargeFreeLimit) * WeightChargeUnit;
        //    OrderCost = OrderCost + weightCharges;
        //}

        //OrderCost = OrderCost + loadUnloadCharges;

        GetFinalCost(OrderCost);

    }

    private void CalculateZoneBasedCost()
    {
        OrderCost = 0;
        ToZone = string.Empty;
        FromZone = string.Empty;
        //if (shipperPostalCode.Length < 3 || consigneePostalCode.Length < 3)
        //{

        //    if (vanChargesFlag)
        //        OrderCost = OrderCost + dbvanCharges;

        //    if ((wait - WaitChargeFreeLimit) > 0)
        //        OrderCost = OrderCost + (wait - WaitChargeFreeLimit) * WaitChargeUnit;

        //    if ((weight - WeightChargeFreeLimit > 0))
        //        OrderCost = OrderCost + (weight - WeightChargeFreeLimit) * WeightChargeUnit;

        //    OrderCost = OrderCost + loadUnloadCharges;

        //    GetFinalCost(0);
        //    return;
        //}
        string sXML = Global.GetPostalCodesZones();
        XDocument postalZones;

        if (string.IsNullOrEmpty(sXML))
        {
            postalZones = XDocument.Load(System.Configuration.ConfigurationManager.AppSettings["postalcodezonefile"]);
        }
        else
        {
            postalZones = XDocument.Parse(sXML);
        }


        var myStartZone = from pcZone in postalZones.Descendants("PostalCodes")
                          where pcZone.Value.ToString().Contains(shipperPostalCode.Substring(0, 3))
                          select pcZone.Parent.Attribute("Number");



        try
        {
            if (myStartZone.Count() != 0)
            {
                FromZone = myStartZone.ElementAt(0).Value.ToString();
                ShipperZone = FromZone;
            }
        }
        catch
        { }


        var myEndZone = from pcZone in postalZones.Descendants("PostalCodes")
                        where pcZone.Value.ToString().Contains(consigneePostalCode.Substring(0, 3))
                        select pcZone.Parent.Attribute("Number");
        try
        {
            if (myEndZone.Count() != 0)
            {
                ToZone = myEndZone.ElementAt(0).Value.ToString();
                ConsigneeZone = ToZone;
            }
        }
        catch
        { }

        if (TotalSkids <= 0)
        {
            GetFinalCost(0);
            return;
        }


        sXML = Global.GetZonesRates();
        if (string.IsNullOrEmpty(sXML))
        {
            postalZones = XDocument.Load(System.Configuration.ConfigurationManager.AppSettings["zonefile"]);
        }
        else
        {
            postalZones = XDocument.Parse(sXML);
        }
        var zoneCost = from pcZone in postalZones.Descendants("PostalCodes")
                       where pcZone.Value.ToString().Contains(shipperPostalCode.Substring(0, 3))
                       select pcZone.Parent.Attribute("Number");



        var myZoneCost = from pcZone in postalZones.Descendants("Zone")
                         where ((string)pcZone.Attribute("From")).Equals(FromZone) &&
                               ((string)pcZone.Attribute("To")).Equals(ToZone)
                         select pcZone;


        if (myZoneCost == null || myZoneCost.Count() == 0)
        {
            myZoneCost = from pcZone in postalZones.Descendants("Zone")
                         where ((string)pcZone.Attribute("To")).Equals(FromZone) &&
                               ((string)pcZone.Attribute("From")).Equals(ToZone)
                         select pcZone;
        }

        if (myZoneCost.Count() == 0)
        {
            OrderCost = 0;
            GetFinalCost(OrderCost);
            return;
        }

        if (ServiceType.Equals(DeliverySerrviceType.SMD.ToString()))
        {
            OrderCost = decimal.Parse(myZoneCost.Elements("SMD").First().Value.ToString());

        }
        else if (ServiceType.Equals(DeliverySerrviceType.RSH.ToString()))
        {
            OrderCost = decimal.Parse(myZoneCost.Elements("Rush").First().Value.ToString());
        }
        else if (ServiceType.Equals(DeliverySerrviceType.DIR.ToString()))
        {

            OrderCost = decimal.Parse(myZoneCost.Elements("DIR").First().Value.ToString());
        }
        else if (ServiceType.Equals(DeliverySerrviceType.ON.ToString()))
        {

            OrderCost = decimal.Parse(myZoneCost.Elements("ON").First().Value.ToString());
        }
        else if (ServiceType.Equals(DeliverySerrviceType.ON1.ToString()) ||
                 ServiceType.Equals(DeliverySerrviceType.ON2.ToString()) ||
                 ServiceType.Equals(DeliverySerrviceType.ON3.ToString()) ||
                 ServiceType.Equals(DeliverySerrviceType.AFT.ToString()))
        {
            string depService = string.Empty;
            string depservoperator = string.Empty;
            decimal depsevChrg = 0;

            XDocument specialService = XDocument.Load(System.Configuration.ConfigurationManager.AppSettings["SSerCharges"]);
            var mySPServiceCost = from spservChrg in specialService.Descendants("service")
                                  where ((string)spservChrg.Attribute("type")).Equals(ServiceType.ToUpper())
                                  select spservChrg;
            foreach (var item in mySPServiceCost)
            {
                depService = ((XElement)item).Attribute("dependent").Value.ToString();
                depservoperator = ((XElement)item).Attribute("opration").Value.ToString();
                depsevChrg = decimal.Parse(((XElement)item).Attribute("addCharg").Value.ToString());
            }

            OrderCost = decimal.Parse(myZoneCost.Elements(depService).First().Value.ToString());
            if (depservoperator.Equals("+"))
            {
                OrderCost = OrderCost + depsevChrg;
            }
            else if (depservoperator.Equals("-"))
            {
                OrderCost = OrderCost - depsevChrg;
            }
            else if (depservoperator.Equals("*"))
            {
                OrderCost = OrderCost * depsevChrg;
            }
        }

        //carCharges = 0;
        //if (carChargesFlag)
        //{
        //    if (ToZone.Equals("11") || FromZone.Equals("11"))
        //    {
        //        OrderCost = OrderCost + dbcarCharges;
        //        carCharges = dbcarCharges;
        //    }
        //}

        //vanCharges = 0;
        //if (vanChargesFlag)
        //{
        //    OrderCost = OrderCost + dbvanCharges;
        //    vanCharges = dbvanCharges;
        //}

        //if ((wait - WaitChargeFreeLimit) > 0)
        //{
        //    waitCharges = (wait - WaitChargeFreeLimit) * WaitChargeUnit;
        //    OrderCost = waitCharges;
        //}

        //if ((weight - WeightChargeFreeLimit > 0))
        //{
        //    weightCharges = (weight - WeightChargeFreeLimit) * WeightChargeUnit;
        //    OrderCost = OrderCost + weightCharges;
        //}

        //OrderCost = OrderCost + loadUnloadCharges;
        OrderCost = OrderCost * TotalSkids;
        GetFinalCost(OrderCost);

    }

    private void CalculateSKDBasedCost()
    {
        OrderCost = 0;
        decimal cost = 0;
        decimal FirstSkidRate = 0;
        decimal AfterFirstSkidRate = 0;


        rws = null;
        rws = Global.AllCitiesRates().Select("CityName = '" +
            ShipperCity + "' And ServiceType = '" + ServiceType + "'");

        if (rws.Length > 0)
        {
            if (rws[0]["FirstSkidRate"] != null && Global.IsNumeric(rws[0]["FirstSkidRate"]))
            {
                FirstSkidRate = (decimal)rws[0]["FirstSkidRate"];
                AfterFirstSkidRate = (decimal)rws[0]["AfterFirstSkidRate"];
            }
        }

        rws = null;
        rws = Global.AllCitiesRates().Select("CityName = '" +
                        ConsigneeCity + "' And ServiceType = '" + ServiceType + "'");

        if (rws.Length > 0)
        {
            if (rws[0]["FirstSkidRate"] != null && Global.IsNumeric(rws[0]["FirstSkidRate"]))
            {
                if (FirstSkidRate < (decimal)rws[0]["FirstSkidRate"])
                {
                    FirstSkidRate = (decimal)rws[0]["FirstSkidRate"];
                    AfterFirstSkidRate = (decimal)rws[0]["AfterFirstSkidRate"];
                }
            }
        }


        //Now calculate Skid Rates

        cost = ((TotalSkids - 1) * AfterFirstSkidRate) + FirstSkidRate;

        GetFinalCost(cost);


    }

    private decimal CalculateSKIDBasedCost()
    {
        if (TotalSkids <= 0)
        {
            GetFinalCost(0);
            return OrderFinalCost;
        }

        if (ServiceType.Equals(DeliverySerrviceType.SMD.ToString()) && myCustomer == CustomersIndicator.MFI)
        {
            CalculateSameDayCost();
        }
        else if (ServiceType.Equals(DeliverySerrviceType.RSH.ToString()) || ServiceType.Equals(DeliverySerrviceType.DIR.ToString()) ||
                            (ServiceType.Equals(DeliverySerrviceType.SMD.ToString()) && myCustomer == CustomersIndicator.Global))
        {

            CalculateSKDBasedCost();
        }
        else
        {
            GetFinalCost(OrderCost);
        }

        return OrderFinalCost;
    }

    private void CalculateSameDayCost()
    {

        decimal CityRate = 0;

        rws = null;

        rws = Global.AllSameDayCityRates().Select("CityName = '" +
                                        ShipperCity + "' And Skids = '" +
            TotalSkids.ToString() + "'");

        if (rws.Length > 0)
        {
            if (rws[0]["Rates"] != null && Global.IsNumeric(rws[0]["Rates"]))
            {
                CityRate = (decimal)rws[0]["Rates"];
            }
        }

        rws = null;
        rws = Global.AllSameDayCityRates().Select("CityName = '" +
                   ConsigneeCity + "' And Skids = '" +
                                                        TotalSkids.ToString() + "'");
        if (rws.Length > 0)
        {
            if (rws[0]["Rates"] != null && Global.IsNumeric(rws[0]["Rates"]))
            {
                if (CityRate < (decimal)rws[0]["Rates"]) CityRate = (decimal)rws[0]["Rates"];
            }
        }

        GetFinalCost(CityRate);

    }

    private void GetFinalCost(decimal skdCost)
    {
        OrderCost = skdCost;
        OrderBasicCost = skdCost;
        OrderSysBasicCost = skdCost;
        calculateGSTNFuelSurchargeNTTLCost(skdCost, false);
        OverRideOrderGSTcharge = 0;
        OverRideSurchargeAmtFromOrder = 0;
        OverRideOrderFinalCost = 0;
        if (OverrideOrderCost != Global.blankOrderNum && OverrideOrderCost != OrderCost)
        {
            calculateGSTNFuelSurchargeNTTLCost(OverrideOrderCost, true);
            OrderOverRideBasicCost = OrderBasicCost;
        }
        else
        {
            OverrideOrderCost = 0;
            OrderOverRideBasicCost = 0;
        }

    }

    private void calculateGSTNFuelSurchargeNTTLCost(decimal basicCost, bool IsItForOvrd)
    {
        decimal lordSaving = 0;
        decimal lordGST = 0;
        decimal lordFuelChrg = 0;
        decimal lordTTLcst = 0;
        decimal lordFuelChrgGst = 0;

        basicCost = decimal.Parse(basicCost.ToString("00.0000"));
        basicCost = decimal.Parse(basicCost.ToString().Substring(0, basicCost.ToString().IndexOf(".") > 0 ? basicCost.ToString().IndexOf(".") + 3 : basicCost.ToString().Length));

        decimal lskdCost = basicCost;

        if (IsItForOvrd && basicCost > 0)
        {
            OrderBasicCost = basicCost;
        }

        if (SpecialDiscount > 0)
        {
            lordSaving = ((lskdCost * SpecialDiscount) / 100);
            lskdCost = lskdCost - lordSaving;
            lskdCost = decimal.Parse(lskdCost.ToString("00.0000"));
            lskdCost = decimal.Parse(lskdCost.ToString().Substring(0, lskdCost.ToString().IndexOf(".") > 0 ? lskdCost.ToString().IndexOf(".") + 3 : lskdCost.ToString().Length));
            if (IsItForOvrd)
            {
                OverrideOrderCost = lskdCost;
                OverRideOrderSaving = lordSaving;

            }
            else
            {
                OrderCost = lskdCost;
                //It is temporary hck
                //OrderBasicCost = lskdCost;
                OrderSaving = lordSaving;
            }

        }


        if (agreedSurcharge > 0 && myCustomer == CustomersIndicator.MFI)
        {
            lordFuelChrg = ((decimal)((lskdCost * agreedSurcharge) / 100));
            lordFuelChrg = decimal.Parse(lordFuelChrg.ToString("00.0000"));
            lordFuelChrg = decimal.Parse(lordFuelChrg.ToString().Substring(0, lordFuelChrg.ToString().IndexOf(".") > 0 ? lordFuelChrg.ToString().IndexOf(".") + 3 : lordFuelChrg.ToString().Length));

        }


        if (myCustomer == CustomersIndicator.Global)
        {
            if (agreedSurcharge == 0) agreedSurcharge = 10;
            carCharges = 0;
            waitCharges = 0;
            weightCharges = 0;
            if ((weight - WeightChargeFreeLimit > 0))
            {
                weightCharges = (weight - WeightChargeFreeLimit) * WeightChargeUnit;
                weightCharges = decimal.Parse(weightCharges.ToString("00.0000"));
                weightCharges = decimal.Parse(weightCharges.ToString().Substring(0, weightCharges.ToString().IndexOf(".") > 0 ? weightCharges.ToString().IndexOf(".") + 3 : weightCharges.ToString().Length));

                lskdCost = lskdCost + weightCharges;
            }

            if (agreedSurcharge > 0)
            {
                lordFuelChrg = ((decimal)((lskdCost * agreedSurcharge) / 100));
                lordFuelChrg = decimal.Parse(lordFuelChrg.ToString("00.0000"));
                lordFuelChrg = decimal.Parse(lordFuelChrg.ToString().Substring(0, lordFuelChrg.ToString().IndexOf(".") > 0 ? lordFuelChrg.ToString().IndexOf(".") + 3 : lordFuelChrg.ToString().Length));
            }

            if (carChargesFlag)
            {
                //As per Illias suuegtion on July25th 2010 we need to add extar Zone 11 fee
                //only if pick-up and delivery inside zone 11
                if (ToZone.Equals("11") && FromZone.Equals("11"))
                {
                    lskdCost = lskdCost + dbcarCharges;
                    carCharges = dbcarCharges;
                }
            }

            vanCharges = 0;
            if (vanChargesFlag)
            {
                lskdCost = lskdCost + dbvanCharges;
                vanCharges = dbvanCharges;
            }

            if (vanChargesFlag)
            {
                if ((wait - WaitVanChargeFreeLimit) > 0)
                {
                    waitCharges = (wait - WaitVanChargeFreeLimit) * WaitVanChargeUnit;
                    waitCharges = decimal.Parse(waitCharges.ToString("00.0000"));
                    waitCharges = decimal.Parse(waitCharges.ToString().Substring(0, waitCharges.ToString().IndexOf(".") > 0 ? waitCharges.ToString().IndexOf(".") + 3 : waitCharges.ToString().Length));
                    lskdCost = lskdCost + waitCharges;
                }
            }
            else
            {
                if ((wait - WaitCarChargeFreeLimit) > 0)
                {
                    waitCharges = (wait - WaitCarChargeFreeLimit) * WaitCarChargeUnit;
                    waitCharges = decimal.Parse(waitCharges.ToString("00.0000"));
                    waitCharges = decimal.Parse(waitCharges.ToString().Substring(0, waitCharges.ToString().IndexOf(".") > 0 ? waitCharges.ToString().IndexOf(".") + 3 : waitCharges.ToString().Length));
                    lskdCost = lskdCost + waitCharges;
                }
            }



            lskdCost = lskdCost + loadUnloadCharges;
            lskdCost = decimal.Parse(lskdCost.ToString("00.0000"));
            lskdCost = decimal.Parse(lskdCost.ToString().Substring(0, lskdCost.ToString().IndexOf(".") > 0 ? lskdCost.ToString().IndexOf(".") + 3 : lskdCost.ToString().Length));



        }

        if (this.myCustomer == CustomersIndicator.Global && ApplyGST && this.GSTValue == 0)
            this.GSTValue = decimal.Parse(Global.GST());

        if (ApplyGST)
        {
            lordGST = ((decimal)((lskdCost * this.GSTValue) / 100));
            lordGST = decimal.Parse(lordGST.ToString("00.0000"));
            lordGST = decimal.Parse(lordGST.ToString().Substring(0, lordGST.ToString().IndexOf(".") > 0 ? lordGST.ToString().IndexOf(".") + 3 : lordGST.ToString().Length));
        }

        if (ApplyGST)
        {
            lordFuelChrgGst = ((decimal)((lordFuelChrg * this.GSTValue) / 100));
            lordFuelChrgGst = decimal.Parse(lordFuelChrgGst.ToString("00.0000"));
            lordFuelChrgGst = decimal.Parse(lordFuelChrgGst.ToString().Substring(0, lordFuelChrgGst.ToString().IndexOf(".") > 0 ? lordFuelChrgGst.ToString().IndexOf(".") + 3 : lordFuelChrgGst.ToString().Length));
        }

        lordTTLcst = lskdCost + lordGST + lordFuelChrg + lordFuelChrgGst;
        lordTTLcst = decimal.Parse(lordTTLcst.ToString().Substring(0, lordTTLcst.ToString().IndexOf(".") > 0 ? lordTTLcst.ToString().IndexOf(".") + 3 : lordTTLcst.ToString().Length));

        if (IsItForOvrd)
        {
            OverRideOrderGSTcharge = lordGST + lordFuelChrgGst;
            OverRideOrderGSTcharge = decimal.Parse(OverRideOrderGSTcharge.ToString("00.0000"));
            OverRideOrderGSTcharge = decimal.Parse(OverRideOrderGSTcharge.ToString().Substring(0, OverRideOrderGSTcharge.ToString().IndexOf(".") > 0 ? OverRideOrderGSTcharge.ToString().IndexOf(".") + 3 : OverRideOrderGSTcharge.ToString().Length));
            OverRideSurchargeAmtFromOrder = lordFuelChrg;
            OverRideOrderFinalCost = lordTTLcst;
        }
        else
        {
            OrderGST = lordGST + lordFuelChrgGst;
            OrderGST = decimal.Parse(OrderGST.ToString("00.0000"));
            OrderGST = decimal.Parse(OrderGST.ToString().Substring(0, OrderGST.ToString().IndexOf(".") > 0 ? OrderGST.ToString().IndexOf(".") + 3 : OrderGST.ToString().Length));
            SurchargeAmtFromOrder = lordFuelChrg;
            OrderFinalCost = lordTTLcst;

        }
        lordSaving = decimal.Parse(lordSaving.ToString("00.0000"));
        lordSaving = decimal.Parse(lordSaving.ToString().Substring(0, lordSaving.ToString().IndexOf(".") > 0 ? lordSaving.ToString().IndexOf(".") + 3 : lordSaving.ToString().Length));
        OrderSaving = lordSaving;


    }

}
    }

