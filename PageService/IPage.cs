using System.Threading.Tasks;

namespace BicUtil.PageService
{
    public interface IPage
    {
        Task OnOpenedPage(IPage _fromPage, object _param = null);
        Task OnClosedPage(IPage _fromUI, object _param = null);

        void InitializePage();
        void DeinitializePage();

        int InitializeOrder{get;}
        PageController PageController{get;set;}
    }
}
