﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class Payment
    {
        [Key]
        [Required]
        public int ReceiptNo { get; set; }
        [Required]
        public DateTime TimeOfPayment { get; set; }
        [Required]
        public decimal Total { get; set; }



    }
}