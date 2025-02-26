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
    /// 主窗口视图模型
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// 控制信息常量
        /// </summary>
        private const string ManualSelectionHint = "手动选择客户端位置";

        /// <summary>
        /// 控制信息常量
        /// </summary>
        private const string SelectedGameFolderHint = "请选择游戏位置";

        #region Services

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<MainWindowViewModel> _logger;

        /// <summary>
        /// LGC探查器
        /// </summary>
        private readonly ILgcIntegrator _lgcIntegrator;

        /// <summary>
        /// 游戏探查器
        /// </summary>
        private readonly IGameDetector _gameDetector;

        /// <summary>
        /// 网络引擎
        /// </summary>
        private readonly INetworkEngine _networkEngine;

        /// <summary>
        /// 考拉比配置中心
        /// </summary>
        private readonly IKorabliFileHub _korabliFileHub;

        /// <summary>
        /// 更新助手
        /// </summary>
        private readonly IUpdateHelper _updateHelper;

        /// <summary>
        /// 汉化安装器
        /// </summary>
        private readonly IChsModInstaller _chsModInstaller;

        /// <summary>
        /// 元数据获取器
        /// </summary>
        private readonly IMetadataFetcher _metadataFetcher;

        #endregion

        #region local

        /// <summary>
        /// 标题
        /// </summary>
        private string _title = "考拉比汉社厂";

        /// <summary>
        /// 游戏文件夹
        /// </summary>
        private readonly HashSet<string> _gameFolders;

        /// <summary>
        /// 选中的游戏文件夹
        /// </summary>
        private string _selectedGameFolder = string.Empty;

        /// <summary>
        /// 选中的更新镜像
        /// </summary>
        private string _selectedUpdateMirror;

        /// <summary>
        /// 控件可用状态
        /// </summary>
        private bool _coreEnabled;

        /// <summary>
        /// 消息
        /// </summary>
        private string _message = string.Empty;

        /// <summary>
        /// 自动更新
        /// </summary>
        private bool _autoUpdate;

        /// <summary>
        /// 代理开关
        /// </summary>
        private bool _proxyEnabled;

        /// <summary>
        /// 代理地址
        /// </summary>
        private string _proxyAddress;

        /// <summary>
        /// 代理用户名
        /// </summary>
        private string _proxyUsername;

        /// <summary>
        /// 代理密码
        /// </summary>
        private string _proxyPassword;

        /// <summary>
        /// 更新开关
        /// </summary>
        private bool _updateEnabled;

        #endregion

        #region Binding

        /// <summary>
        /// 绑定窗体标题
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set { SetProperty(ref this._title, value); }
        }

        #region Core

        /// <summary>
        /// 绑定游戏文件夹
        /// </summary>
        public ObservableCollection<string> GameFolders => [.. this._gameFolders];

        /// <summary>
        /// 绑定选中的游戏文件夹
        /// </summary>
        public string SelectedGameFolder
        {
            get { return this._selectedGameFolder; }
            set { SetProperty(ref this._selectedGameFolder, value); }
        }

        /// <summary>
        /// 绑定游戏服务器
        /// </summary>
        public string GameServer => string.IsNullOrEmpty(this._gameDetector.Server) ? MainWindowViewModel.SelectedGameFolderHint : this._gameDetector.Server;

        /// <summary>
        /// 绑定游戏版本
        /// </summary>
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

        /// <summary>
        /// 绑定游戏测试服
        /// </summary>
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

        /// <summary>
        /// 绑定汉化Mod安装状态
        /// </summary>
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
        /// 绑定控件可用状态
        /// </summary>
        public bool CoreEnabled
        {
            get { return this._coreEnabled && !string.IsNullOrEmpty(this._selectedGameFolder); }
            set { SetProperty(ref this._coreEnabled, value); }
        }

        /// <summary>
        /// 绑定消息
        /// </summary>
        public string Message
        {
            get { return this._message; }
            set { SetProperty(ref this._message, value); }
        }

        #endregion

        #region Settings

        /// <summary>
        /// 绑定更新镜像
        /// </summary>
        public static IEnumerable<string> UpdateMirrors => Enum.GetNames(typeof(MirrorList));

        /// <summary>
        /// 绑定选中的更新镜像
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
        /// 绑定自动更新
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
        /// 绑定代理开关
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
        /// 绑定代理地址
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
        /// 绑定代理用户名
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
        /// 绑定代理密码
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
        /// 绑定应用版本
        /// </summary>
        public Version AppVersion { get; }

        /// <summary>
        /// 绑定更新开关
        /// </summary>
        public bool UpdateEnabled
        {
            get { return this._updateEnabled; }
            set { SetProperty(ref this._updateEnabled, value); }
        }

        #endregion

        #endregion


        /// <summary>
        /// 构造视图模型
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="lgcIntegrator">LGC探查器</param>
        /// <param name="gameDetector">游戏探查器</param>
        /// <param name="networkEngine">网络引擎</param>
        /// <param name="korabliFileHub">考拉比配置中心</param>
        /// <param name="updateHelper">更新助手</param>
        /// <param name="modInstaller">Mod安装器</param>
        /// <param name="fetchMetadata">元信息获取器</param>
        public MainWindowViewModel(
            Lazy<ILogger<MainWindowViewModel>> logger,
            Lazy<ILgcIntegrator> lgcIntegrator,
            Lazy<IGameDetector> gameDetector,
            Lazy<INetworkEngine> networkEngine,
            Lazy<IKorabliFileHub> korabliFileHub,
            Lazy<IUpdateHelper> updateHelper,
            Lazy<IChsModInstaller> modInstaller,
            Lazy<IMetadataFetcher> fetchMetadata)
        {
            this._logger = logger.Value;
            this._lgcIntegrator = lgcIntegrator.Value;
            this._gameDetector = gameDetector.Value;
            this._networkEngine = networkEngine.Value;
            this._korabliFileHub = korabliFileHub.Value;
            this._updateHelper = updateHelper.Value;
            this._chsModInstaller = modInstaller.Value;
            this._metadataFetcher = fetchMetadata.Value;

            var fullVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion!;
            _ = Version.TryParse(fullVersion.Split('+')[0], out var version);
            if (!Directory.Exists(IOExtension.AppDataFolder))
            {
                Directory.CreateDirectory(IOExtension.AppDataFolder);
            }

            this.AppVersion = version ?? Version.Parse("0.0.0");
            this.Message += $"考拉比汉社厂 v{this.AppVersion}\r\n";
            this._selectedUpdateMirror = this._korabliFileHub.Mirror.ToString();
            this._lgcIntegrator.ServiceEvent += this.SyncServiceMessage;
            this._networkEngine.ServiceEvent += this.SyncServiceMessage;
            this._korabliFileHub.ServiceEvent += this.SyncServiceMessage;
            this._updateHelper.ServiceEvent += this.SyncServiceMessage;
            this._gameDetector.ServiceEvent += this.SyncServiceMessage;
            this._chsModInstaller.ServiceEvent += this.SyncServiceMessage;
            this._metadataFetcher.ServiceEvent += this.SyncServiceMessage;

            this._lgcIntegrator.Load();
            this._korabliFileHub.Load();
            this._networkEngine.Init();

            this._gameFolders = [];
            this._autoUpdate = this._korabliFileHub.AutoUpdate;
            this._proxyEnabled = this._korabliFileHub.Proxy.Enabled;
            this._proxyAddress = this._korabliFileHub.Proxy.Address;
            this._proxyUsername = this._korabliFileHub.Proxy.Username;
            this._proxyPassword = this._korabliFileHub.Proxy.Password;

            var thread = new Thread(async void () =>
            {
                try
                {
                    this.UpdateEnabled = await this._updateHelper.Check(this._korabliFileHub.Mirror, this.AppVersion);
                    if (!this._updateEnabled || !this._korabliFileHub.AutoUpdate)
                    {
                        return;
                    }

                    var result = await this._updateHelper.Update(this._korabliFileHub.Mirror);
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
        /// 同步信息至消息框
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">服务事件参数</param>
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

        /// <summary>
        /// 重载功能
        /// </summary>
        public async void Reload()
        {
            try
            {
                this._gameFolders.Clear();
                this._gameDetector.Clear();
                this._selectedUpdateMirror = this._korabliFileHub.Mirror.ToString();
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
        /// 刷新视图
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
            RaisePropertyChanged(nameof(this.SelectedUpdateMirror));
        }

        /// <summary>
        /// 绑定窗体加载
        /// </summary>
        public DelegateCommand WindowLoadCommand => new(Reload);

        /// <summary>
        /// 绑定刷新窗体
        /// </summary>
        public DelegateCommand RefreshViewsCommand => new(RefreshViews);

        /// <summary>
        /// 绑定重载游戏文件夹
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
        /// 绑定游戏文件夹选择变更
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

        /// <summary>
        /// 绑定安装汉化Mod
        /// </summary>
        public DelegateCommand InstallChsModCommand => new(async void () =>
        {
            try
            {
                this.CoreEnabled = false;
                var result = await this._chsModInstaller.Install(this._korabliFileHub.Mirror);
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

            }
            catch (Exception e)
            {
                this.SyncServiceMessage(this, new ServiceEventArg
                {
                    Message = "安装汉化失败！",
                    Exception = e
                });
            }
            finally
            {
                this.Reload();
                this.CoreEnabled = true;
                this.RefreshViews();
            }
        });

        /// <summary>
        /// 绑定卸载汉化Mod
        /// </summary>
        public DelegateCommand UninstallChsModCommand => new(void () =>
        {
            try
            {
                this.CoreEnabled = false;
                IOExtension.DeleteDirectory(this._gameDetector.ModFolder);
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
        /// 绑定保存设置
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

        /// <summary>
        /// 绑定更新
        /// </summary>
        public DelegateCommand UpdateCommand => new(async void () =>
        {
            try
            {
                this._updateEnabled = false;
                var result = await this._updateHelper.Update(this._korabliFileHub.Mirror);
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
