using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace GlobalWebAppBAL
{
    public class GlobalOrders
    {
        private DateTime _FromDate;
        private DateTime _ToDate;
        private RecordFetchFlags _FecthFlag;
        private string _CutomerNum;
        public List<GlobalOrder> _AllOrders = new List<GlobalOrder>();
        public GlobalOrders(string CutomerNum, DateTime FromDate, DateTime ToDate, RecordFetchFlags FecthFlag)
        {
            _FromDate = FromDate;
            _ToDate = ToDate;
            _FecthFlag = FecthFlag;
            _CutomerNum = CutomerNum;
        }



        public fetchResults GetOrders()
        {
            fetchResults result;
            try
            {
                DataTable myReader = Global.GetAllOrders(_CutomerNum, _FromDate, _ToDate, _FecthFlag);
                if (myReader.Rows.Count > 0)
                {

                    int counter = 0;
                    foreach (DataRow rw in myReader.Rows)
                    {
                        GlobalOrder ord = new GlobalOrder(rw["OrderNumber"].ToString());
                        _AllOrders.Add(ord);
                        counter = counter + 1;
                    }
                    result.RecCount = counter;
                    result.Result = true;
                    result.ErrMsg = string.Empty;
                }
                else
                {
                    result.RecCount = 0;
                    result.Result = false;
                    result.ErrMsg = "no record found.";

                }

            }
            catch (Exception ex)
            {
                result.RecCount = 0;
                result.Result = false;
                result.ErrMsg = "Operation could not be completed.";
            }

            return result;

        }


        public List<GlobalOrder> OrderList { get { return _AllOrders; } }
    }
}
