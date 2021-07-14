﻿using System.Text;

namespace ShipIt.Models.ApiModels
{
    public class OrderLine
    {
        public string gtin { get; set; }
        public int quantity { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendFormat("gtin: {0}, ", gtin)
                .AppendFormat("quantity: {0}", quantity)
                .ToString();
        }
    }
}