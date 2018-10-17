using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalDAL;
using System.Data;

namespace GlobalWebAppBAL
{
    public class CustomerManager:WebCustomerDetails
    {
        private static object CustomerRepository;

        public string name { get; set; }
        public string streetAddress { get; set; }
        public string suite { get; set; }
        public string city { get; set; }
        public  string province { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public string contactName { get; set; }

        public static DataTable GetShipmentTypes()
        {
            
            DataTable customerTable = new DataTable();
            customerTable.Columns.Add("ShippingType");
            customerTable.Columns.Add("ShippingValue");
            DataRow rw = customerTable.NewRow();
            rw["ShippingType"] = "Envelopes";
            rw["ShippingValue"] = "ENV";
            customerTable.Rows.Add(rw);

            rw = customerTable.NewRow();
            rw["ShippingType"] = "Boxes";
            rw["ShippingValue"] = "BOX";
            customerTable.Rows.Add(rw);

            rw = customerTable.NewRow();
            rw["ShippingType"] = "Packages";
            rw["ShippingValue"] = "PKG";
            customerTable.Rows.Add(rw);


            rw = customerTable.NewRow();
            rw["ShippingType"] = "Skids";
            rw["ShippingValue"] = "SKD";
            customerTable.Rows.Add(rw);

            rw = customerTable.NewRow();
            rw["ShippingType"] = "Load";
            rw["ShippingValue"] = "LDS";
            customerTable.Rows.Add(rw);

            rw = customerTable.NewRow();
            rw["ShippingType"] = "Machinery";
            rw["ShippingValue"] = "MAC";
            customerTable.Rows.Add(rw);

            rw = customerTable.NewRow();
            rw["ShippingType"] = "Equipment";
            rw["ShippingValue"] = "EQP";
            customerTable.Rows.Add(rw);

            try
            {
                //customerAdapter.Fill(customerTable);

                return customerTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Int64 GetCustomerLogin(string customerName, string customerPassword)
        {
            return GlobalDAL.CustomerRepository.getCustomerLogin(customerName, customerPassword);

        }
        public static Int64 GetNewWayBill()
        {
            return GlobalDAL.CustomerRepository.getNewWayBill();

        }
        public static List<GetReceiverAddressesByCustomerID_Result> GetReceiverAddressesByCustomerID(Int64 customerID, Int64 consigneeID)
        {
                return GlobalDAL.CustomerRepository.getReceiverAddressesByCustomerID(customerID,consigneeID);
        }
        public static List<GetShipperAddressesByCustomerID_Result> GetShipperAddressesByCustomerID(Int64 customerID, Int64 shipperID)
        {
            return GlobalDAL.CustomerRepository.getShipperAddressesByCustomerID(customerID, shipperID);
        }
        public static List<GetReceiversByCustomerID_Result> getReceiversByCustomerID(Int64 customerID)
        {            
            return GlobalDAL.CustomerRepository.getReceiversByCustomerID(customerID).ToList();
        }
        public static List<GetShippersByCustomerID_Result> getShippersByCustomerID(Int64 customerID)
        {
            return GlobalDAL.CustomerRepository.getShippersByCustomerID(customerID).ToList();
        }
        public static List<GetShipperDepartments_Result> GetShipperDepartment(Int64 customerID)
        {
            return GlobalDAL.CustomerRepository.getShipperDepartmentByID(customerID).ToList();
        }
        public static  List<GetWebCustomerDetails_Result> GetWebCustomerDetails(Int64 customerID)
        {
             return GlobalDAL.CustomerRepository.getWebCustomerDetails(customerID);
        }
        public static List<Province> GetProvinces()
        {
            return GlobalDAL.CustomerRepository.getProvinces();
        }
        public static List<GetClientInformation_Result> GetClientInfo()
        {
            return GlobalDAL.CustomerRepository.getClientInfo();
        }
        public static List<GetGlobalWebDeliveryWayBill_Result> GetWayBillByOrderID(string orderID)
        {
            return GlobalDAL.CustomerRepository.getWayBillByOrderID(orderID);
        }
        public static List<GetAllDeliveryType_Result> GetDeliveryServiceTypes()
        {
            try
            {                
                return GlobalDAL.CustomerRepository.GetAllDeliveryTypes();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int GetBlankCustomerOder()
        {
            return Global.blankCuromerNum;
        }
    }

}
