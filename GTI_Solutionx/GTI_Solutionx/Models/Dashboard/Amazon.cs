﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GTI_Solutionx.Models.Dashboard
{
    public class Amazon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        public string Asin { get; set; }

        public string sku { get; set; }

        public double price { get; set; }

        public string wholesaler { get; set; }

        public bool blackList { get; set; }

        [Remote(action: "BlackList", controller: "Amazon")]
        public string marketPlace { get; set; }
        
    }
}