﻿using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Parsers
{
    public static class ProductParser
    {
        public static Product Parse(this ProductRequestModel requestModel)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(requestModel.Discontinued))
            {
                errors.Add("Discontinued must be set");
            }
            if (string.IsNullOrEmpty(requestModel.LowerThreshold))
            {
                errors.Add("LowerThreshold must be set");
            }
            if (string.IsNullOrEmpty(requestModel.MinimumOrderQuantity))
            {
                errors.Add("MinimumOrderQuantity must be set");
            }
            if (string.IsNullOrEmpty(requestModel.Weight))
            {
                errors.Add("Weight must be set");
            }

            if (errors.Any())
            {
                throw new MalformedRequestException(string.Join("\n", errors));
            }


            if (!bool.TryParse(requestModel.Discontinued, out var discontinued))
            {
                errors.Add("Discontinued must be set to true or false");
            }
            if (!int.TryParse(requestModel.LowerThreshold, out var lowerThreshold))
            {
                errors.Add("LowerThreshold must be set to an integer");
            }
            if (!int.TryParse(requestModel.MinimumOrderQuantity, out var minimumOrderQuantity))
            {
                errors.Add("MinimumOrderQuantity must be set to an integer");
            }
            if (!int.TryParse(requestModel.Weight, out var weight))
            {
                errors.Add("Weight must be set to an integer");
            }

            if (errors.Any())
            {
                throw new MalformedRequestException(string.Join("\n", errors));
            }

            return new Product()
            {
                Discontinued = discontinued,
                Gcp = requestModel.Gcp,
                Gtin = requestModel.Gtin,
                LowerThreshold = lowerThreshold,
                MinimumOrderQuantity = minimumOrderQuantity,
                Name = requestModel.Name,
                Weight = weight
            };
        }
    }
}