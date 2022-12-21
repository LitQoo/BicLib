using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicUtil.UI;
using UnityEngine;

namespace BicUtil.PageService.Review{
    public class ReviewPopup : ReviewPopupBase, IPage
    {
        public int InitializeOrder => 99;

        public PageController PageController { get;set; }

        public void DeinitializePage()
        {
            this.OnClose -= backPage;
        }

        public void InitializePage()
        {
            this.Init();
            this.OnClose += backPage;
        }

        public Task OnClosedPage(IPage _fromUI, object _param = null)
        {
            return Task.CompletedTask;
        }

        public Task OnOpenedPage(IPage _fromPage, object _param = null)
        {
            this.Open();
            return Task.CompletedTask;
        }

        private void backPage(){
            if(PageController.CurrentPage == this as IPage){
                PageController.Back(PageTransition.Sequance);
            }
        }
    }
}
