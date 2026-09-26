using System.Collections.Generic;
using LDIS.Core.Models;

namespace LDIS.Core.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        Category GetCategory(long categoryId);
        ValidationResult ValidateCategory(Category category, bool isNew);
        Category CreateCategory(string categoryName);
        void UpdateCategory(long categoryId, string newCategoryName);
        bool CanDeleteCategory(long categoryId, out string reason);
        void DeleteCategory(long categoryId);
    }
}
