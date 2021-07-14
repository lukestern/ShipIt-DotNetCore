﻿using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Repositories
{
    public interface IStockRepository
    {
        int GetTrackedItemsCount();
        int GetStockHeldSum();
        IEnumerable<StockDataModel> GetStockByWarehouseId(int id);
        Dictionary<int, StockDataModel> GetStockByWarehouseAndProductIds(int warehouseId, List<int> productIds);
        void RemoveStock(int warehouseId, List<StockAlteration> lineItems);
        void AddStock(int warehouseId, List<StockAlteration> lineItems);
    }

    public class StockRepository : RepositoryBase, IStockRepository
    {

        public int GetTrackedItemsCount()
        {
            var sql = "SELECT COUNT(*) FROM stock";
            return (int)QueryForLong(sql);
        }

        public int GetStockHeldSum()
        {
            var sql = "SELECT SUM(hld) FROM stock";
            return (int)QueryForLong(sql);
        }

        public IEnumerable<StockDataModel> GetStockByWarehouseId(int id)
        {
            var sql = "SELECT p_id, hld, w_id FROM stock WHERE w_id = @w_id";
            var parameter = new NpgsqlParameter("@w_id", id);
            var noProductWithIdErrorMessage = string.Format("No stock found with w_id: {0}", id);
            try
            {
                return base.RunGetQuery(sql, reader => new StockDataModel(reader), noProductWithIdErrorMessage, parameter).ToList();
            }
            catch (NoSuchEntityException)
            {
                return new List<StockDataModel>();
            }
        }

        public Dictionary<int, StockDataModel> GetStockByWarehouseAndProductIds(int warehouseId, List<int> productIds)
        {
            var sql = string.Format("SELECT p_id, hld, w_id FROM stock WHERE w_id = @w_id AND p_id IN ({0})",
                string.Join(",", productIds));
            var parameter = new NpgsqlParameter("@w_id", warehouseId);
            var noProductWithIdErrorMessage = string.Format("No stock found with w_id: {0} and p_ids: {1}",
                warehouseId, string.Join(",", productIds));
            var stock = base.RunGetQuery(sql, reader => new StockDataModel(reader), noProductWithIdErrorMessage, parameter);
            return stock.ToDictionary(s => s.ProductId, s => s);
        }

        public void AddStock(int warehouseId, List<StockAlteration> lineItems)
        {
            var parametersList = new List<NpgsqlParameter[]>();
            foreach (var orderLine in lineItems)
            {
                parametersList.Add(
                    new NpgsqlParameter[] {
                        new NpgsqlParameter("@p_id", orderLine.ProductId),
                        new NpgsqlParameter("@w_id", warehouseId),
                        new NpgsqlParameter("@hld", orderLine.Quantity)
                    });
            }

            var sql = "INSERT INTO stock (p_id, w_id, hld) VALUES (@p_id, @w_id, @hld) "
                         + "ON CONFLICT (p_id, w_id) DO UPDATE SET hld = stock.hld + EXCLUDED.hld";

            var recordsAffected = new List<int>();
            foreach (var parameters in parametersList)
            {
                recordsAffected.Add(
                    RunSingleQueryAndReturnRecordsAffected(sql, parameters)
                );
            }

            string errorMessage = null;

            for (var i = 0; i < recordsAffected.Count; i++)
            {
                if (recordsAffected[i] == 0)
                {
                    errorMessage = string.Format("Product {0} in warehouse {1} was unexpectedly not updated (rows updated returned {2})",
                        parametersList[i][0], warehouseId, recordsAffected[i]);
                }
            }

            if (errorMessage != null)
            {
                throw new InvalidStateException(errorMessage);
            }
        }

        public void RemoveStock(int warehouseId, List<StockAlteration> lineItems)
        {
            var sql = string.Format("UPDATE stock SET hld = hld - @hld WHERE w_id = {0} AND p_id = @p_id",
                warehouseId);

            var parametersList = new List<NpgsqlParameter[]>();
            foreach (var lineItem in lineItems)
            {
                parametersList.Add(new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@hld", lineItem.Quantity),
                    new NpgsqlParameter("@p_id", lineItem.ProductId)
                });
            }

            base.RunTransaction(sql, parametersList);
        }
    }
}