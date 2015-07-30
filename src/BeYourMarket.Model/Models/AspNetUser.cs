using System;
using System.Collections.Generic;

namespace BeYourMarket.Model.Models
{
    public partial class AspNetUser : Repository.Pattern.Ef6.Entity
    {
        public AspNetUser()
        {
            this.AspNetUserClaims = new List<AspNetUserClaim>();
            this.AspNetUserLogins = new List<AspNetUserLogin>();
            this.Orders = new List<Order>();
            this.Orders1 = new List<Order>();
            this.Items = new List<Item>();
            this.AspNetRoles = new List<AspNetRole>();
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public System.DateTime RegisterDate { get; set; }
        public string RegisterIP { get; set; }
        public System.DateTime LastAccessDate { get; set; }
        public string LastAccessIP { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public bool AcceptEmail { get; set; }
        public string Gender { get; set; }
        public int LeadSourceID { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        public bool Disabled { get; set; }
        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Order> Orders1 { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<AspNetRole> AspNetRoles { get; set; }
    }
}
