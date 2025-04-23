using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Inv_M_Sys.Models
{
    public enum UserRole
    {
        [Description("Owner")]
        Owner,

        [Description("Admin")]
        Admin,

        [Description("Selling Staff")]
        SellingStaff,

        [Description("Stock Staff")]
        StockStaff
    }
}
