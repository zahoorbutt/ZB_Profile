//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZB_ProfileDataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class AdditionalService
    {
        public System.Guid ServiceID { get; set; }
        public string ServiceDescription { get; set; }
        public Nullable<bool> GSTApplicable { get; set; }
        public Nullable<System.Guid> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDateTime { get; set; }
        public Nullable<System.Guid> LastModifiedBY { get; set; }
        public Nullable<System.DateTime> LastModifiedDateTime { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<bool> PayToDriver { get; set; }
        public string Code { get; set; }
        public Nullable<bool> ReturnService { get; set; }
    }
}
