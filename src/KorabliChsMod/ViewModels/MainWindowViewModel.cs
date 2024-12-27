using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

        /// <summary>
        /// 
        /// </summary>
        private readonly IModInstaller _modInstaller;

        #endregion

        #region local

        /// <summary>
        /// 
        /// </summary>
        private string _title = "考拉比汉社厂";

        /// <summary>
        /// 
        /// </summary>
        private readonly HashSet<string> _gameFolders;

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
        private bool _coreEnabled;

        /// <summary>
        /// 
        /// </summary>
        private string _message = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        private bool _autoUpdate;

        /// <summary>
        /// 
        /// </summary>
        private bool _proxyEnabled;

        /// <summary>
        /// 
        /// </summary>
        private string _proxyAddress;

        /// <summary>
        /// 
        /// </summary>
        private string _proxyUsername;

        /// <summary>
        /// 
        /// </summary>
        private string _proxyPassword;

        /// <summary>
        /// 
        /// </summary>
        private bool _updateEnabled;

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
        public ObservableCollection<string> GameFolders => new(this._gameFolders);

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
        public bool CoreEnabled
        {
            get { return this._coreEnabled && !string.IsNullOrEmpty(this._selectedGameFolder); }
            set { SetProperty(ref this._coreEnabled, value); }
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

        /// <summary>
        /// 
        /// </summary>
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
        public bool ProxyEnabled
        {
            get { return this._proxyEnabled; }
            set
            {
                this._korabliFileHub.Proxy.Enabled = value;
                SetProperty(ref this._proxyEnabled, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ProxyAddress
        {
            get { return this._proxyAddress; }
            set
            {
                this._korabliFileHub.Proxy.Address = value;
                SetProperty(ref this._proxyAddress, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ProxyUsername
        {
            get { return this._proxyUsername; }
            set
            {
                this._korabliFileHub.Proxy.Username = value;
                SetProperty(ref this._proxyUsername, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ProxyPassword
        {
            get { return this._proxyPassword; }
            set
            {
                this._korabliFileHub.Proxy.Password = value;
                SetProperty(ref this._proxyPassword, value);
            }
        }

        #endregion

        #region About

        /// <summary>
        /// 
        /// </summary>
        public Version AppVersion { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool UpdateEnabled
        {
            get { return this._updateEnabled; }
            set { SetProperty(ref this._updateEnabled, value); }
        }

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
        /// <param name="modInstaller"></param>
        public MainWindowViewModel(
            Lazy<ILogger<MainWindowViewModel>> logger,
            Lazy<ILgcIntegrator> lgcIntegrator,
            Lazy<IGameDetector> gameDetector,
            Lazy<INetworkEngine> networkEngine,
            Lazy<IKorabliFileHub> korabliFileHub,
            Lazy<IUpdateHelper> updateHelper,
            Lazy<IModInstaller> modInstaller)
        {
            this._logger = logger.Value;
            this._lgcIntegrator = lgcIntegrator.Value;
            this._gameDetector = gameDetector.Value;
            this._networkEngine = networkEngine.Value;
            this._korabliFileHub = korabliFileHub.Value;
            this._updateHelper = updateHelper.Value;
            this._modInstaller = modInstaller.Value;

            var fullVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion!;
            _ = Version.TryParse(fullVersion.Split('+')[0], out var version);
            if (!Directory.Exists(IOExtension.AppDataFolder))
            {
                Directory.CreateDirectory(IOExtension.AppDataFolder);
            }

            this.AppVersion = version ?? Version.Parse("0.0.0");
            this.Message += $"考拉比汉社厂 v{this.AppVersion}\r\n";

            this._lgcIntegrator.ServiceEvent += this.SyncServiceMessage;
            this._networkEngine.ServiceEvent += this.SyncServiceMessage;
            this._korabliFileHub.ServiceEvent += this.SyncServiceMessage;
            this._updateHelper.ServiceEvent += this.SyncServiceMessage;
            this._gameDetector.ServiceEvent += this.SyncServiceMessage;
            this._modInstaller.ServiceEvent += this.SyncServiceMessage;

            this._lgcIntegrator.Load();
            this._networkEngine.Init();
            this._korabliFileHub.Load();

            this._gameFolders = [];
            this._selectedUpdateMirror = this._korabliFileHub.Mirror.ToString();
            this._autoUpdate = this._korabliFileHub.AutoUpdate;
            this._proxyEnabled = this._korabliFileHub.Proxy.Enabled;
            this._proxyAddress = this._korabliFileHub.Proxy.Address;
            this._proxyUsername = this._korabliFileHub.Proxy.Username;
            this._proxyPassword = this._korabliFileHub.Proxy.Password;

            var thread = new Thread(async void () =>
            {
                try
                {
                    this.UpdateEnabled = await this._updateHelper.UpdateAvailable(this.AppVersion);
                    if (!this._updateEnabled || !this._korabliFileHub.AutoUpdate)
                    {
                        return;
                    }

                    var result = await this._updateHelper.Update();
                    if (!result)
                    {
                        this.SyncServiceMessage(this, new ServiceEventArg
                        {
                            Message = "更新失败！"
                        });
                    }

                }
                catch (Exception exception)
                {
                    this.SyncServiceMessage(this, new ServiceEventArg
                    {
                        Exception = exception
                    });

                }

            });

            thread.Start();

            this._coreEnabled = true;
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

        #region Action

        public async void Reload()
        {
            try
            {
                this._gameFolders.Clear();
                this._gameDetector.Clear();
                foreach (var gameFolder in this._lgcIntegrator.GameFolders)
                {
                    var result = this._gameDetector.Load(gameFolder);
                    if (result && this._gameDetector.IsWarship)
                    {
                        this._gameFolders.Add(gameFolder);
                    }

                    this._gameDetector.Clear();
                }
                this._gameFolders.Add(MainWindowViewModel.ManualSelectionHint);

                this._selectedGameFolder = this._gameFolders.Contains(this._korabliFileHub.GameFolder) ? this._korabliFileHub.GameFolder : string.Empty;
                if (!string.IsNullOrEmpty(this._selectedGameFolder))
                {
                    var result = this._gameDetector.Load(this._selectedGameFolder);
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
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "读取游戏信息失败！",
                    Exception = e,
                    AppendException = false
                });

            }
        }

        #endregion

        #region Command

        #region Core

        /// <summary>
        /// 
        /// </summary>
        private void RefreshViews()
        {
            RaisePropertyChanged(nameof(this.GameFolders));
            RaisePropertyChanged(nameof(this.SelectedGameFolder));
            RaisePropertyChanged(nameof(this.GameServer));
            RaisePropertyChanged(nameof(this.GameVersion));
            RaisePropertyChanged(nameof(this.GameTest));
            RaisePropertyChanged(nameof(this.ChsModInstalled));
            RaisePropertyChanged(nameof(this.CoreEnabled));
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand WindowLoadCommand => new(Reload);

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand RefreshViewsCommand => new(RefreshViews);

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

                var loadResult = this._gameDetector.Load(this._selectedGameFolder);
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
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "读取游戏信息失败！",
                    Exception = e
                });
            }

        });

        public DelegateCommand InstallChsModCommand => new(async void () =>
        {
            try
            {
                this.CoreEnabled = false;
                var result = await this._modInstaller.Install();
                if (!result)
                {
                    this.SyncServiceMessage(this, new ServiceEventArg
                    {
                        Message = "安装汉化失败！"
                    });
                    return;
                }
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "安装汉化成功！"
                });

                this.Reload();
                this.CoreEnabled = true;
                this.RefreshViews();
            }
            catch (Exception e)
            {
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "安装汉化失败！",
                    Exception = e
                });
            }
        });

        public DelegateCommand UninstallChsModCommand => new(async void () =>
        {
            try
            {
                this.CoreEnabled = false;
                await IOExtension.DeleteDirectory(this._gameDetector.ModFolder);
                this.Reload();
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "移除Mod完成！"
                });

                this.CoreEnabled = true;
                this.RefreshViews();
            }
            catch (Exception e)
            {
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "卸载汉化失败！",
                    Exception = e
                });
            }
        });

        #endregion

        #region Settings

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand SaveSettingsCommand => new(async void () =>
        {
            try
            {
                if (this._proxyEnabled)
                {
                    var proxy = this._korabliFileHub.UpdateEngineProxy(true);
                    if (!proxy)
                    {
                        this.SyncServiceMessage(this, new ServiceEventArg
                        {
                            Message = "代理设置失败！"
                        });

                        return;
                    }

                }

                var result = await this._korabliFileHub.SaveAsync();
                if (!result)
                {
                    this.SyncServiceMessage(this, new ServiceEventArg
                    {
                        Message = "保存设置失败！"
                    });

                    return;
                }

                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "保存设置成功！"
                });

            }
            catch (Exception e)
            {
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "保存设置失败！",
                    Exception = e
                });
            }
        });

        #endregion

        #region About

        public DelegateCommand UpdateCommand => new(async void () =>
        {
            try
            {
                this._updateEnabled = false;
                var result = await this._updateHelper.Update();
                if (!result)
                {
                    this.SyncServiceMessage(this, new ServiceEventArg
                    {
                        Message = "更新失败！"
                    });
                    return;
                }

                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "更新完成！"
                });

                this.UpdateEnabled = true;
            }
            catch (Exception e)
            {
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "更新失败！",
                    Exception = e
                });
            }
        });

        #endregion

        #endregion
    }
}
