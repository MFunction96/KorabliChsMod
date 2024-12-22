using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Config;
using Xanadu.KorabliChsMod.DI;
using Xanadu.Skidbladnir.IO.File;

namespace Xanadu.KorabliChsMod.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        private const string ManualSelectionHint = "手动选择客户端位置";

        /// <summary>
        /// 
        /// </summary>
        private const string SelectedGameFolderHint = "请选择游戏位置";

        #region Services

        /// <summary>
        /// 
        /// </summary>
        private readonly ILogger<MainWindowViewModel> _logger;

        /// <summary>
        /// 
        /// </summary>
        private readonly ILgcIntegrator _lgcIntegrator;

        /// <summary>
        /// 
        /// </summary>
        private readonly IGameDetector _gameDetector;

        /// <summary>
        /// 
        /// </summary>
        private readonly INetworkEngine _networkEngine;

        /// <summary>
        /// 
        /// </summary>
        private readonly IKorabliFileHub _korabliFileHub;

        /// <summary>
        /// 
        /// </summary>
        private readonly IUpdateHelper _updateHelper;

        #endregion

        #region local

        /// <summary>
        /// 
        /// </summary>
        private string _title = "考拉比汉社厂";

        /// <summary>
        /// 
        /// </summary>
        private string _selectedGameFolder = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        private string _selectedUpdateMirror;

        /// <summary>
        /// 
        /// </summary>
        private string _message = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        private bool _autoUpdate;

        #endregion

        #region Binding

        /// <summary>
        /// 
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set { SetProperty(ref this._title, value); }
        }

        #region Core

        /// <summary>
        /// 
        /// </summary>
        public ISet<string> GameFolders
        {
            get
            {
                var list = this._lgcIntegrator.GameFolders.ToHashSet();
                list.Add(MainWindowViewModel.ManualSelectionHint);
                return list;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SelectedGameFolder
        {
            get { return this._selectedGameFolder; }
            set { SetProperty(ref this._selectedGameFolder, value); }
        }

        public string GameServer => string.IsNullOrEmpty(this._gameDetector.Server) ? MainWindowViewModel.SelectedGameFolderHint : this._gameDetector.Server;

        public string GameVersion
        {
            get
            {
                if (string.IsNullOrEmpty(this._gameDetector.ClientVersion))
                {
                    return MainWindowViewModel.SelectedGameFolderHint;
                }

                return this._gameDetector.PreInstalled && !string.IsNullOrEmpty(this._gameDetector.ServerVersion) ? this._gameDetector.ServerVersion : this._gameDetector.ClientVersion;
            }
        }

        public string GameTest
        {
            get
            {
                if (string.IsNullOrEmpty(this._gameDetector.Server))
                {
                    return MainWindowViewModel.SelectedGameFolderHint;
                }

                return this._gameDetector.IsTest ? "测试服" : "正式服";
            }
        }

        public string ChsModInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(this._gameDetector.Server))
                {
                    return MainWindowViewModel.SelectedGameFolderHint;
                }

                return this._gameDetector.ChsMod ? "已安装" : "未安装";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Message
        {
            get { return this._message; }
            set { SetProperty(ref this._message, value); }
        }

        #endregion

        #region Settings

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> UpdateMirrors => Enum.GetNames(typeof(MirrorList));

        public bool AutoUpdate
        {
            get { return this._autoUpdate; }
            set
            {
                this._korabliFileHub.AutoUpdate = value;
                SetProperty(ref this._autoUpdate, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SelectedUpdateMirror
        {
            get { return this._selectedUpdateMirror; }
            set
            {
                this._korabliFileHub.Mirror = Enum.Parse<MirrorList>(value);
                SetProperty(ref this._selectedUpdateMirror, value);
            }
        }

        #endregion

        #region About

        /// <summary>
        /// 
        /// </summary>
        public Version AppVersion { get; }

        #endregion

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="lgcIntegrator"></param>
        /// <param name="gameDetector"></param>
        /// <param name="networkEngine"></param>
        /// <param name="korabliFileHub"></param>
        /// <param name="updateHelper"></param>
        public MainWindowViewModel(
            Lazy<ILogger<MainWindowViewModel>> logger,
            Lazy<ILgcIntegrator> lgcIntegrator,
            Lazy<IGameDetector> gameDetector,
            Lazy<INetworkEngine> networkEngine,
            Lazy<IKorabliFileHub> korabliFileHub,
            Lazy<IUpdateHelper> updateHelper)
        {
            this._logger = logger.Value;
            this._lgcIntegrator = lgcIntegrator.Value;
            this._gameDetector = gameDetector.Value;
            this._networkEngine = networkEngine.Value;
            this._korabliFileHub = korabliFileHub.Value;
            this._updateHelper = updateHelper.Value;

            this._lgcIntegrator.ServiceEvent += this.SyncServiceMessage;
            this._networkEngine.ServiceEvent += this.SyncServiceMessage;
            this._korabliFileHub.ServiceEvent += this.SyncServiceMessage;
            this._updateHelper.ServiceEvent += this.SyncServiceMessage;
            this._gameDetector.ServiceEvent += this.SyncServiceMessage;

            this._lgcIntegrator.Load();
            this._networkEngine.Init();
            this._korabliFileHub.Load();

            this._autoUpdate = this._korabliFileHub.AutoUpdate;
            this._selectedUpdateMirror = this._korabliFileHub.Mirror.ToString();

            var fullVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion!;
            _ = Version.TryParse(fullVersion.Split('+')[0], out var version);
            if (!Directory.Exists(IOExtension.AppDataFolder))
            {
                Directory.CreateDirectory(IOExtension.AppDataFolder);
            }

            this.AppVersion = version ?? Version.Parse("0.0.0");
            this.Message += $"考拉比汉社厂 v{this.AppVersion}\r\n";

            var thread = new Thread(async void () =>
            {
                try
                {
                    var available = await this._updateHelper.UpdateAvailable(this.AppVersion);
                    if (available && this._korabliFileHub.AutoUpdate)
                    {
                        await this._updateHelper.Update();
                    }
                }
                catch (Exception exception)
                {
                    this._logger.LogError(exception, string.Empty);
                }

            });

            thread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncServiceMessage(object? sender, ServiceEventArg e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                this.Message += e.Message + "\r\n";
            }

            if (e.Exception is not null)
            {
                this.Message += $"异常详情：{e.Exception.Message}\r\n";
                this._logger.LogError(e.Exception, this.Message);
            }
            else
            {
                this._logger.LogInformation(e.Message);
            }

        }

        #region Command

        private void RefreshViews()
        {
            RaisePropertyChanged(nameof(this.SelectedGameFolder));
            RaisePropertyChanged(nameof(this.GameServer));
            RaisePropertyChanged(nameof(this.GameVersion));
            RaisePropertyChanged(nameof(this.GameTest));
            RaisePropertyChanged(nameof(this.ChsModInstalled));
        }

        public DelegateCommand WindowLoadCommand => new(async void () =>
        {
            try
            {
                this._selectedGameFolder = this._lgcIntegrator.GameFolders.Contains(this._korabliFileHub.GameFolder) ? this._korabliFileHub.GameFolder : string.Empty;
                if (!string.IsNullOrEmpty(this._selectedGameFolder))
                {
                    var result = await this._gameDetector.Load(this._selectedGameFolder);
                    if (!result)
                    {
                        this._selectedGameFolder = string.Empty;
                        this._korabliFileHub.GameFolder = string.Empty;
                        await this._korabliFileHub.SaveAsync();
                    }
                }

                this.RefreshViews();
            }
            catch (Exception e)
            {
                this.Message += $"读取游戏信息失败！ 详细信息：{e.Message}\r\n";
                this._logger.LogError(e.Message, string.Empty);
            }
        });

        public DelegateCommand RefreshViewsCommand => new(RefreshViews);

        public DelegateCommand ReloadGameFolderCommand => new(() =>
        {
            if (string.IsNullOrEmpty(this._selectedGameFolder))
            {
                return;
            }

            if (string.Compare(this._selectedGameFolder, MainWindowViewModel.ManualSelectionHint,
                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                return;
            }
            if (Directory.Exists(this.SelectedGameFolder))
            {
                _ = Process.Start(this.SelectedGameFolder);
            }
        });

        public DelegateCommand GameFolderSelectionChangedCommand => new(async void () =>
        {
            try
            {
                if (string.IsNullOrEmpty(this._selectedGameFolder))
                {
                    return;
                }

                if (string.Compare(this.SelectedGameFolder, MainWindowViewModel.ManualSelectionHint,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var dialog = new OpenFolderDialog
                    {
                        Multiselect = false,
                        Title = "选择窝窝屎本体位置"
                    };

                    var result = dialog.ShowDialog();
                    if (!(result ?? false))
                    {
                        this._selectedGameFolder = string.Empty;
                        RaisePropertyChanged(nameof(this.SelectedGameFolder));
                        return;
                    }

                    this._selectedGameFolder = dialog.FolderName;
                    this._lgcIntegrator.GameFolders.Add(this._selectedGameFolder);
                }

                var loadResult = await this._gameDetector.Load(this._selectedGameFolder);
                if (!loadResult)
                {
                    this._selectedGameFolder = string.Empty;
                    this._korabliFileHub.GameFolder = string.Empty;
                }
                else
                {
                    this._korabliFileHub.GameFolder = this._selectedGameFolder;
                }

                await this._korabliFileHub.SaveAsync();
                this.RefreshViews();
            }
            catch (Exception e)
            {
                this.Message += $"读取游戏信息失败！ 详细信息：{e.Message}\r\n";
                this._logger.LogError(e.Message, string.Empty);
            }

        });

        #endregion
    }
}
