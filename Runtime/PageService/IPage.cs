using Cysharp.Threading.Tasks;

namespace BicUtil.PageService
{
    public interface IPage
    {
        int InitializeOrder{get;}
        PageController PageController{get;set;}
        
        void InitializePage();
        void DeinitializePage();

        UniTask OnOpenedPage(IPage _fromPage, object _param = null);
        UniTask OnClosedPage(IPage _fromUI, object _param = null);

        UniTask OnClickBackButton();
    }
}
