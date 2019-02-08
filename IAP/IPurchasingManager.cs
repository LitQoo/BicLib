#if BICUTIL_IAP
using UnityEngine.Purchasing;
using BicDB.Container;
using BicDB.Variable;
using System;

namespace BicUtil.Purchasing{
    public enum PurchasingResult{
        Complete,
        Refunded,
        NotInitialized,
        NotAvailable,
        Failed,
        Unknown
    }

    public interface IPurchasingManager<PRODUCTTYPE> where PRODUCTTYPE : struct
    {
        TableContainer<ProductModel<PRODUCTTYPE>> ProductTable{get;}
        
        ProductModel<PRODUCTTYPE> GetProduct(PRODUCTTYPE _idType);

        void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _value);
        void Initialize();
        void BuyProduct(PRODUCTTYPE _idType, Action<PurchasingResult> _callback);
        void RestorePurchases(Action<bool> _callback);
    }
}
#endif