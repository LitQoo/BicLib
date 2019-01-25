using System;
using System.Linq;
using BicDB.Container;
using UnityEngine.Purchasing;

namespace BicUtil.Purchasing{
    public class DummyManager<PRODUCTTYPE> : IPurchasingManager<PRODUCTTYPE> where PRODUCTTYPE : struct
    {
        static private IPurchasingManager<PRODUCTTYPE> instance = null;
        static public IPurchasingManager<PRODUCTTYPE> Instance{
            get{
                if(instance == null){
                    instance = new DummyManager<PRODUCTTYPE>();
                }

                return instance;
            }
        } 

        public TableContainer<ProductModel<PRODUCTTYPE>> productTable = new TableContainer<ProductModel<PRODUCTTYPE>>("Puma");
        public TableContainer<ProductModel<PRODUCTTYPE>> ProductTable{get{return productTable;}}

        public void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _value)
        {
            var _product = new ProductModel<PRODUCTTYPE>(_idType, _id, _productType, _value);
            ProductTable.Add(_product);
        }

        public void BuyProduct(PRODUCTTYPE _idType, Action<PurchasingResult> _callback)
        {
            _callback(PurchasingResult.Failed);
        }

        public ProductModel<PRODUCTTYPE> GetProduct(PRODUCTTYPE _idType)
        {
            return ProductTable.FirstOrDefault<ProductModel<PRODUCTTYPE>>(_row=>Enum.Equals(_row.IdType.AsEnum, _idType));
        }

        public void Initialize()
        {
            
        }

        public void RestorePurchases(Action<bool> _callback)
        {
            _callback(false);
        }
    }
}