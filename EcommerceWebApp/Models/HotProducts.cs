namespace EcommerceWebApp.Models
{ 
    using System.Collections.Generic;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using EcommerceWebApp.Models;
    using System.ComponentModel.DataAnnotations;

    public class HotProduct
    {
        [Key]
        [Required, StringLength(100), Display(Name = "Name")]
        public string item { get; set; }

        public int countevents { get; set; }

        [Display(Name = "Price")]
        public int price { get; set; }

        public override bool Equals(object obj)
        {
            HotProduct hotItem = obj as HotProduct;
            return (this.item == hotItem.item && this.price == hotItem.price);
        }

        public override int GetHashCode()
        {
            return this.item.GetHashCode();
        }
    }
}