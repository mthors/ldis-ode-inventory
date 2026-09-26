using System;
using System.Collections.Generic;
using LDIS.Core.Data.Repositories;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            if (categoryRepository == null)
            {
                throw new ArgumentNullException("categoryRepository");
            }
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _categoryRepository.GetAll();
        }

        public Category GetCategory(long categoryId)
        {
            return _categoryRepository.GetById(categoryId);
        }

        public ValidationResult ValidateCategory(Category category, bool isNew)
        {
            var result = new ValidationResult();

            if (category == null)
            {
                result.AddError("Category cannot be null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                result.AddError("Category name is required.");
            }
            else
            {
                string trimmed = category.CategoryName.Trim();
                if (trimmed.Length > 100)
                {
                    result.AddError("Category name must not exceed 100 characters.");
                }

                var existing = _categoryRepository.GetByName(trimmed);
                if (existing != null && (isNew || existing.CategoryID != category.CategoryID))
                {
                    result.AddError(string.Format("Category '{0}' already exists.", trimmed));
                }
            }

            return result;
        }

        public Category CreateCategory(string categoryName)
        {
            var category = new Category
            {
                CategoryName = categoryName != null ? categoryName.Trim() : string.Empty
            };

            var validation = ValidateCategory(category, true);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            _categoryRepository.Create(category);
            return category;
        }

        public void UpdateCategory(long categoryId, string newCategoryName)
        {
            var category = _categoryRepository.GetById(categoryId);
            if (category == null)
            {
                throw new InvalidOperationException("Category not found.");
            }

            category.CategoryName = newCategoryName != null ? newCategoryName.Trim() : string.Empty;

            var validation = ValidateCategory(category, false);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            _categoryRepository.Update(category);
        }

        public bool CanDeleteCategory(long categoryId, out string reason)
        {
            var category = _categoryRepository.GetById(categoryId);
            if (category == null)
            {
                reason = "Category does not exist.";
                return false;
            }

            if (_categoryRepository.IsCategoryInUse(categoryId))
            {
                reason = string.Format("Category '{0}' cannot be deleted because it is currently assigned to one or more products.", category.CategoryName);
                return false;
            }

            reason = null;
            return true;
        }

        public void DeleteCategory(long categoryId)
        {
            string reason;
            if (!CanDeleteCategory(categoryId, out reason))
            {
                throw new InvalidOperationException(reason);
            }

            _categoryRepository.Delete(categoryId);
        }
    }
}
