using System;
using System.Collections.Generic;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public class StockService : IStockService
    {
        private readonly IInventoryTransactionRepository _transactionRepository;
        private readonly IItemRepository _itemRepository;

        public StockService(
            IInventoryTransactionRepository transactionRepository,
            IItemRepository itemRepository)
        {
            if (transactionRepository == null)
            {
                throw new ArgumentNullException("transactionRepository");
            }
            if (itemRepository == null)
            {
                throw new ArgumentNullException("itemRepository");
            }

            _transactionRepository = transactionRepository;
            _itemRepository = itemRepository;
        }

        public ValidationResult ValidateOperation(StockOperationRequest request)
        {
            var result = new ValidationResult();

            if (request == null)
            {
                result.AddError("Request cannot be null.");
                return result;
            }

            if (request.ItemID <= 0)
            {
                result.AddError("A valid product must be selected.");
                return result;
            }

            var item = _itemRepository.GetById(request.ItemID);
            if (item == null)
            {
                result.AddError("The selected product does not exist.");
                return result;
            }

            if (!item.IsActive)
            {
                result.AddError("Cannot perform stock operations on an inactive product.");
            }

            if (string.IsNullOrWhiteSpace(request.OperationType))
            {
                result.AddError("Operation type is required.");
                return result;
            }

            string opType = request.OperationType.Trim().ToUpperInvariant();
            if (opType != "IN" && opType != "OUT" && opType != "ADJUSTMENT")
            {
                result.AddError("Invalid operation type. Allowed types: IN, OUT, ADJUSTMENT.");
                return result;
            }

            // Operation-specific validations
            if (opType == "IN")
            {
                if (request.Quantity <= 0)
                {
                    result.AddError("Stock IN quantity must be greater than zero.");
                }
            }
            else if (opType == "OUT")
            {
                if (request.Quantity <= 0)
                {
                    result.AddError("Stock OUT quantity must be greater than zero.");
                }
                else if (request.Quantity > item.CurrentStock)
                {
                    result.AddError(string.Format(
                        "Insufficient stock. Current stock is {0}, but requested {1}.",
                        item.CurrentStock, request.Quantity));
                }
            }
            else if (opType == "ADJUSTMENT")
            {
                if (request.Quantity == 0)
                {
                    result.AddError("Stock adjustment quantity cannot be zero.");
                }
                else if (item.CurrentStock + request.Quantity < 0)
                {
                    result.AddError(string.Format(
                        "Stock adjustment would cause negative stock. Current stock is {0}, adjustment is {1}.",
                        item.CurrentStock, request.Quantity));
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    result.AddError("A reason is required for stock adjustments.");
                }
                else if (request.Reason.Trim().Length > 250)
                {
                    result.AddError("Reason must not exceed 250 characters.");
                }
            }

            // Price validation
            if (request.UnitPrice < 0)
            {
                result.AddError("Unit price cannot be negative.");
            }

            // String length limits
            if (!string.IsNullOrWhiteSpace(request.ReferenceNumber) && request.ReferenceNumber.Trim().Length > 100)
            {
                result.AddError("Reference number must not exceed 100 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.SupplierOrCustomer) && request.SupplierOrCustomer.Trim().Length > 150)
            {
                result.AddError("Supplier / Customer must not exceed 150 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Trim().Length > 500)
            {
                result.AddError("Notes must not exceed 500 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.CreatedBy) && request.CreatedBy.Trim().Length > 100)
            {
                result.AddError("Created by must not exceed 100 characters.");
            }

            return result;
        }

        public long StockIn(StockOperationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            request.OperationType = "IN";
            return ExecuteOperation(request);
        }

        public long StockOut(StockOperationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            request.OperationType = "OUT";
            return ExecuteOperation(request);
        }

        public long StockAdjustment(StockOperationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            request.OperationType = "ADJUSTMENT";
            return ExecuteOperation(request);
        }

        public long ExecuteOperation(StockOperationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var validation = ValidateOperation(request);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            string opType = request.OperationType.Trim().ToUpperInvariant();
            int stockDelta;

            if (opType == "IN")
            {
                stockDelta = request.Quantity;
            }
            else if (opType == "OUT")
            {
                stockDelta = -request.Quantity;
            }
            else // ADJUSTMENT
            {
                stockDelta = request.Quantity;
            }

            var transaction = new InventoryTransaction
            {
                ItemID = request.ItemID,
                TransactionType = opType,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                ReferenceNumber = !string.IsNullOrWhiteSpace(request.ReferenceNumber) ? request.ReferenceNumber.Trim() : null,
                SupplierOrCustomer = !string.IsNullOrWhiteSpace(request.SupplierOrCustomer) ? request.SupplierOrCustomer.Trim() : null,
                Reason = !string.IsNullOrWhiteSpace(request.Reason) ? request.Reason.Trim() : null,
                Notes = !string.IsNullOrWhiteSpace(request.Notes) ? request.Notes.Trim() : null,
                TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
                CreatedBy = !string.IsNullOrWhiteSpace(request.CreatedBy) ? request.CreatedBy.Trim() : null
            };

            return _transactionRepository.ExecuteStockOperation(transaction, stockDelta);
        }

        public IEnumerable<TransactionListItemDto> SearchTransactions(TransactionSearchCriteria criteria)
        {
            return _transactionRepository.SearchTransactions(criteria);
        }

        public InventoryTransaction GetTransactionById(long transactionId)
        {
            return _transactionRepository.GetById(transactionId);
        }
    }
}
