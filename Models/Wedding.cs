using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId {get;set;}
        ////////////////////
        public string WedderOne {get;set;}
        public string WedderTwo {get;set;}

        [DataType(DataType.Date), DisplayFormat(DataFormatString="{0:dd/MM/YYY)", ApplyFormatInEditMode = true)]
        public DateTime Date {get;set;} 
        public string Address {get;set;}
        ////////////////////
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
        ////////////////////
        public int CreatorId {get;set;}
        public List<Association> GuestList {get;set;}
    }
}