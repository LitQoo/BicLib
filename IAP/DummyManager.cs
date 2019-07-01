#if BICUTIL_IAP
using System;
using System.Linq;
using BicDB;
using BicDB.Container;
using BicDB.Storage;
using BicDB.Variable;
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
        public bool isLoadedProductTable = false;
        public void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _amount, string _defaultCurrentCode, string _defaultPriceString, float _defaultPrice, string _title, Action<IVariable> _valueChangedCallback)
        {
            if(isLoadedProductTable == false){
				productTable.SetStorage(FileStorage.GetInstance());
				productTable.Load(null, new FileStorageParameter("purchase"));
				isLoadedProductTable = true; 
			}

            var _product = GetProduct(_idType);

            if(_product == null){
                _product = new ProductModel<PRODUCTTYPE>(_idType, _id, _productType, _amount);
                productTable.Add(_product);
            }

            _product.Price.AsFloat = _defaultPrice;
            _product.PriceString.AsString = _defaultPriceString;
            _product.CurrencyCode.AsString = _defaultCurrentCode;
            _product.Amount.AsInt = _amount;
            _product.Title.AsString = _title;
            _product.ProductType.AsEnum = _productType;

            if(_valueChangedCallback != null){
                _product.PurchaseCount.OnChangedValueActions += _valueChangedCallback;
                _product.PurchaseCount.NotifyChanged();
            }
        }

        public void BuyProduct(PRODUCTTYPE _idType, Action<PurchasingResult> _callback)
        {
            _callback(PurchasingResult.Failed);
        }

        public ProductModel<PRODUCTTYPE> GetProduct(PRODUCTTYPE _idType)
        {
            return productTable.FirstOrDefault<ProductModel<PRODUCTTYPE>>(_row=>Enum.Equals(_row.IdType.AsEnum, _idType));
        }

        public void Initialize()
        {
           
        }

        public void RestorePurchases(Action<bool> _callback)
        {
            _callback(false);
        }

        public void Save(Action<BicDB.Result> _callback = null, object _parameter = null)
        {
            productTable.Save(_callback, _parameter);
        }
    }
}
#endif