using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Model.Enum
{
    public enum Enum_LeadSource
    {
        None = 0,
        EmailInvitation
    }

    public enum Enum_OrderStatus
    {
        Created = 0,
        Pending,
        Confirmed,
        Cancelled
    }

    public enum Enum_MetaFieldControlType
    {
        None = 0,
        /// <summary>
        /// Dropdown list
        /// </summary>
        DropdownList = 1,
        /// <summary>
        /// Radio list
        /// </summary>
        RadioList = 2,
        /// <summary>
        /// Checkboxes
        /// </summary>        
        Checkboxes = 3,
        /// <summary>
        /// TextBox
        /// </summary>
        TextBox = 4,
        /// <summary>
        /// Multiline textbox
        /// </summary>
        MultilineTextbox = 10,
        /// <summary>
        /// Datepicker
        /// </summary>
        Datepicker = 20,
        ///// <summary>
        ///// File upload control
        ///// </summary>
        //FileUpload = 30,
        ///// <summary>
        ///// Color squares
        ///// </summary>
        //ColorSquares = 40,
        ///// <summary>
        ///// Read-only checkboxes
        ///// </summary>
        //ReadonlyCheckboxes = 50,
    }

    public enum Enum_SortView
    {
        Grid,
        List,
        Map
    }

    public enum Enum_EstateSortPageSize
    {
        [Description("12")]
        Page12 = 12,
        [Description("20")]
        Page20 = 20,
        [Description("40")]
        Page40 = 40,
        [Description("60")]
        Page60 = 60,
        [Description("100")]
        Page100 = 100
    }

    public enum Enum_UserType
    {
        Normal = 0,
        Administrator = 1
    }

    public enum Enum_PluginAction
    {
        Install = 0,
        Uninstall = 1,
        Enabled = 2,
        Disabled = 3
    }

    public enum Enum_ListingOrderType
    {
        None = 0,
        DateRange,
        Quantity
    }

    public enum Enum_MessageFolder
    {
        Inbox = 0,
        Archive = 1
    }

    public enum Enum_MessageAction
    {
        None = 0,
        MarkAsRead = 1
    }
}
