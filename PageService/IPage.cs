using System.Threading.Tasks;

namespace BicUtil.PageService
{
    public interface IPage
    {
        int InitializeOrder{get;}
        PageController PageController{get;set;}
        
        void InitializePage();
        void DeinitializePage();

        Task OnOpenedPage(IPage _fromPage, object _param = null);
        Task OnClosedPage(IPage _fromUI, object _param = null);
    }
}
