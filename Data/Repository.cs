using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShopbridgeEventProcessor.Models;

namespace ShopbridgeEventProcessor.Data.Repository
{
    public class Repository
    {
        private readonly ShopbridgeContext dbcontext;

        public Repository(ShopbridgeContext _dbcontext)
        {
            this.dbcontext = _dbcontext;
        }

        public void AddorUpdateProduct(Product product)
        {
            var products = dbcontext.Product.Where(x => x.ProductUpc == product.ProductUpc);
            if (products.Any())
            {
                var productToUpdate = products.ToList()[0];
                productToUpdate.Stock = product.Stock;
                productToUpdate.Price = product.Price;
                dbcontext.Update(productToUpdate);
            }
            else
            {
                dbcontext.Add(product);
            }
            dbcontext.SaveChanges();
        }
    }
}
