﻿using System.Collections.Generic;

namespace ShipIt.Validators
{
    public abstract class BaseValidator<T>
    {
        private readonly List<string> errors;

        protected BaseValidator()
        {
            errors = new List<string>();
        }

        public void Validate(T target) => DoValidation(target);

        protected abstract void DoValidation(T target);

        private void addError(string error) => errors.Add(error);

        private void addErrors(List<string> errors) => this.errors.AddRange(errors);

        /**
         * Object validators
         */

        private void assertNotNull(string fieldName, object value)
        {
            if (value == null)
            {
                addError(string.Format("Field {0} cannot be null", fieldName));
            }
        }

        /**
         * String validators
         */

        protected void assertNotBlank(string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                addError(string.Format("Field {0} cannot be blank", fieldName));
            }
        }

        protected void AssertNumeric(string fieldName, string value)
        {
            if (!double.TryParse(value, out var d))
            {
                addError(string.Format("Field {0} must be numeric", fieldName));
            }
        }

        protected void AssertMaxLength(string fieldName, string value, int maxLength)
        {
            if (value.Length > maxLength)
            {
                addError(string.Format("Field {0} must be shorter than {1} characters", fieldName, maxLength));
            }
        }

        protected void AssertExactLength(string fieldName, string value, int exactLength)
        {
            if (value.Length != exactLength)
            {
                addError(string.Format("Field {0} must be exactly {1} characters", fieldName, exactLength));
            }
        }

        /**
         * Numeric validators
         */

        protected void AssertNonNegative(string fieldName, int value)
        {
            if (value < 0)
            {
                addError(string.Format("Field {0} must be non-negative", fieldName));
            }
        }

        protected void AssertNonNegative(string fieldName, float value)
        {
            if (value < 0)
            {
                addError(string.Format("Field {0} must be non-negative", fieldName));
            }
        }

        /**
         * Specific validators
         */

        protected void ValidateGtin(string value)
        {
            assertNotBlank("gtin", value);
            AssertNumeric("gtin", value);
            AssertMaxLength("gtin", value, 13);
        }

        protected void ValidateGcp(string value)
        {
            assertNotBlank("gcp", value);
            AssertNumeric("gcp", value);
            AssertMaxLength("gcp", value, 13);
        }

        protected void validateWarehouseId(int warehouseId) => AssertNonNegative("warehouseId", warehouseId);
        /*
    protected void validateOrderLines(List<OrderLine> orderLines)
    {
        Set<String> gtins = new HashSet<String>(orderLines.size());
        for (OrderLine orderLine : orderLines)
        {
            OrderLineValidator orderLineValidator = new OrderLineValidator();
            orderLineValidator.doValidation(orderLine);
            addErrors(orderLineValidator.errors);

            if (gtins.contains(orderLine.getGtin()))
            {
                addError(String.format("Order contains duplicate GTINs: {0}", orderLine.getGtin()));
            }
            else
            {
                gtins.add(orderLine.getGtin());
            }
        }
    }*/
    }
}