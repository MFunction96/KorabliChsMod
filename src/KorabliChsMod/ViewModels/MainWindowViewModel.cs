using Microsoft.Extensions.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.DI;

namespace Xanadu.KorabliChsMod.ViewModels
{
    public class MainWindowViewModel(Lazy<ILogger<MainWindowViewModel>> logger, Lazy<ILgcIntegrator> lgcIntegrator)
        : BindableBase
    {
        private readonly ILogger<MainWindowViewModel> _logger = logger.Value;

        private readonly ILgcIntegrator _lgcIntegrator = lgcIntegrator.Value;

        private string _title = "考拉比汉社厂";

        private bool _loaded;

        private string _selectedGameFolder = string.Empty;
        private string _selectedUpdateMirror = string.Empty;

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
                    _lgcIntegrator.Load();
                    this._loaded = true;
                }

                var list = _lgcIntegrator.GameFolders.ToList() ?? [];
                list.Add("手动选择客户端路径");
                return list;
            }
        }

        public IEnumerable<string> UpdateMirrors => Enum.GetNames(typeof(MirrorList));

        public string SelectedGameFolder
        {
            get { return string.IsNullOrEmpty(this._selectedGameFolder) ? this.GameFolders.First() : this._selectedGameFolder; }
            set { SetProperty(ref _selectedGameFolder, value); }
        }

        public string SelectedUpdateMirror
        {
            get { return string.IsNullOrEmpty(this._selectedUpdateMirror) ? this.UpdateMirrors.First() : this._selectedUpdateMirror; }
            set { SetProperty(ref _selectedUpdateMirror, value); }
        }
    }
}
