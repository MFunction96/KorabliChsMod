using Microsoft.Extensions.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Xanadu.KorabliChsMod.DI;

namespace Xanadu.KorabliChsMod.ViewModels
{
    public class MainWindowViewModel(Lazy<ILogger<MainWindowViewModel>> logger, Lazy<ILgcIntegrator> lgcIntegrator) : BindableBase
    {
        private string _title = "考拉比汉社厂";

        private bool _loaded = false;
        
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public IEnumerable<string> GameFolders
        {
            get
            {
                if (!this._loaded)
                {
                    lgcIntegrator.Value.Load();
                    this._loaded = true;
                }

                var list = lgcIntegrator.Value.GameFolders.ToList() ?? [];
                list.Add("手动选择客户端路径");
                return list;
            }
        }

    }
}
