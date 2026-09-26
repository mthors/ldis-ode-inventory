using System.Collections.Generic;
using LDIS.Core.Models;

namespace LDIS.Core.Data.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category GetById(long categoryId);
        Category GetByName(string categoryName);
        long Create(Category category);
        void Update(Category category);
        void Delete(long categoryId);
        bool IsCategoryInUse(long categoryId);
    }
}
