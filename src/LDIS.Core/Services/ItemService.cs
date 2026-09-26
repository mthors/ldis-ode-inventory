using System;
using System.Collections.Generic;
using LDIS.Core.Data.Repositories;
using LDIS.Core.DTOs;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ItemService(IItemRepository itemRepository, ICategoryRepository categoryRepository)
        {
            if (itemRepository == null)
            {
                throw new ArgumentNullException("itemRepository");
            }
            if (categoryRepository == null)
            {
                throw new ArgumentNullException("categoryRepository");
            }

            _itemRepository = itemRepository;
            _categoryRepository = categoryRepository;
        }

        public Item GetItem(long itemId)
        {
            return _itemRepository.GetById(itemId);
        }

        public Item GetItemBySku(string sku)
        {
            return _itemRepository.GetBySku(sku);
        }

        public ValidationResult ValidateItem(Item item, bool isNew)
        {
            var result = new ValidationResult();

            if (item == null)
            {
                result.AddError("Item cannot be null.");
                return result;
            }

            // SKU validation
            if (string.IsNullOrWhiteSpace(item.SKU))
            {
                result.AddError("SKU is required.");
            }
            else
            {
                string sku = item.SKU.Trim();
                if (sku.Length > 50)
                {
                    result.AddError("SKU must not exceed 50 characters.");
                }

                long? excludeId = isNew ? (long?)null : item.ItemID;
                if (_itemRepository.ExistsSku(sku, excludeId))
                {
                    result.AddError(string.Format("SKU '{0}' is already in use by another product.", sku));
                }
            }

            // Name validation
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                result.AddError("Product name is required.");
            }
            else if (item.Name.Trim().Length > 150)
            {
                result.AddError("Product name must not exceed 150 characters.");
            }

            // Category validation (optional, but if provided must exist)
            if (item.CategoryID.HasValue && item.CategoryID.Value > 0)
            {
                var cat = _categoryRepository.GetById(item.CategoryID.Value);
                if (cat == null)
                {
                    result.AddError("The selected category does not exist.");
                }
            }

            // Field length checks
            if (!string.IsNullOrWhiteSpace(item.Brand) && item.Brand.Trim().Length > 50)
            {
                result.AddError("Brand must not exceed 50 characters.");
            }
            if (!string.IsNullOrWhiteSpace(item.Color) && item.Color.Trim().Length > 50)
            {
                result.AddError("Color must not exceed 50 characters.");
            }
            if (!string.IsNullOrWhiteSpace(item.Size) && item.Size.Trim().Length > 50)
            {
                result.AddError("Size must not exceed 50 characters.");
            }

            // Controlled Gender validation
            if (!string.IsNullOrWhiteSpace(item.Gender) && !GenderOptions.IsValid(item.Gender))
            {
                result.AddError(string.Format("Invalid gender '{0}'. Allowed values: Unisex, Men, Women, Kids, None / Unspecified.", item.Gender));
            }

            // Price validation
            if (item.PurchasePrice < 0)
            {
                result.AddError("Purchase price cannot be negative.");
            }
            if (item.SellingPrice < 0)
            {
                result.AddError("Selling price cannot be negative.");
            }

            // Stock limits
            if (item.MinStockLevel < 0)
            {
                result.AddError("Minimum stock level cannot be negative.");
            }

            return result;
        }

        public long SaveItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            bool isNew = (item.ItemID <= 0);

            var validation = ValidateItem(item, isNew);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            // Normalize fields
            item.SKU = item.SKU.Trim();
            item.Name = item.Name.Trim();
            item.Brand = !string.IsNullOrWhiteSpace(item.Brand) ? item.Brand.Trim() : null;
            item.Color = !string.IsNullOrWhiteSpace(item.Color) ? item.Color.Trim() : null;
            item.Size = !string.IsNullOrWhiteSpace(item.Size) ? item.Size.Trim() : null;
            item.Gender = GenderOptions.Normalize(item.Gender);

            if (item.CategoryID.HasValue && item.CategoryID.Value <= 0)
            {
                item.CategoryID = null;
            }

            if (isNew)
            {
                item.CurrentStock = 0;
                return _itemRepository.Create(item);
            }
            else
            {
                _itemRepository.Update(item);
                return item.ItemID;
            }
        }

        public void DeactivateItem(long itemId)
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            _itemRepository.SetActiveStatus(itemId, false);
        }

        public void ActivateItem(long itemId)
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            _itemRepository.SetActiveStatus(itemId, true);
        }

        public IEnumerable<ItemListItemDto> SearchItems(ItemSearchCriteria criteria)
        {
            return _itemRepository.Search(criteria);
        }

        public ProductDistinctAttributesDto GetDistinctAttributes()
        {
            return _itemRepository.GetDistinctAttributes();
        }
    }
}
