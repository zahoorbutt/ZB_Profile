using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalWebAppBAL
{
    public class WebCustomerDetails
    {

        public Nullable<System.Guid> MailingLocID { get; set; }
        public Nullable<System.Guid> BillingLocID { get; set; }
        public long CustomerNum { get; set; }
        public string Name { get; set; }
        public string ContactName1 { get; set; }
        public Nullable<long> AccNum { get; set; }
        public string MailingStreetLocation { get; set; }
        public string MailingSuite { get; set; }
        public string MailingCity { get; set; }
        public string MailingProvince { get; set; }
        public string MailingCountry { get; set; }
        public string MailingPhone1 { get; set; }
        public string MailingPhone2 { get; set; }
        public string MailingPostalCode { get; set; }
        public string BillingStreetLocation { get; set; }
        public string BillingSuite { get; set; }
        public string BillingCity { get; set; }
        public string BillingProvince { get; set; }
        public string BillingCountry { get; set; }
        public string BillingPostalCode { get; set; }
        public string CityRegion { get; set; }
        public Nullable<bool> GSTApplicable { get; set; }
        public string SurchargeRate { get; set; }
        public string PaymentBase { get; set; }
        public string SpecialDiscount { get; set; }
    }
}
