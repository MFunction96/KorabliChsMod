using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xanadu.KorabliChsMod.Core;
using Xanadu.KorabliChsMod.Core.Models;
using Xanadu.KorabliChsMod.Core.Services;
using Xanadu.KorabliChsMod.Services;
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
        private readonly LgcIntegratorService _lgcIntegratorService;

        /// <summary>
        /// 考拉比配置中心
        /// </summary>
        private readonly KorabliConfigService _korabliConfigService;

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
        public string GameServer => string.IsNullOrEmpty(this.SelectedGameDetectModel.Server) ? MainWindowViewModel.SelectedGameFolderHint : this.SelectedGameDetectModel.Server;

        /// <summary>
        /// 绑定游戏版本
        /// </summary>
        public string GameVersion
        {
            get
            {
                if (string.IsNullOrEmpty(this.SelectedGameDetectModel.ClientVersion))
                {
                    return MainWindowViewModel.SelectedGameFolderHint;
                }

                return this.SelectedGameDetectModel.PreInstalled && !string.IsNullOrEmpty(this.SelectedGameDetectModel.ServerVersion) ? this.SelectedGameDetectModel.ServerVersion : this.SelectedGameDetectModel.ClientVersion;
            }
        }

        /// <summary>
        /// 绑定游戏测试服
        /// </summary>
        public string GameTest
        {
            get
            {
                if (string.IsNullOrEmpty(this.SelectedGameDetectModel.Server))
                {
                    return MainWindowViewModel.SelectedGameFolderHint;
                }

                return this.SelectedGameDetectModel.IsTest ? "测试服" : "正式服";
            }
        }

        /// <summary>
        /// 绑定汉化Mod安装状态
        /// </summary>
        public string ChsModInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(this.SelectedGameDetectModel.Server))
                {
                    return MainWindowViewModel.SelectedGameFolderHint;
                }

                return this.SelectedGameDetectModel.ChsMod ? "已安装" : "未安装";
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

        public bool RemoveEnabled => !this.SelectedGameDetectModel.IsTest && this._coreEnabled;

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
                this._korabliConfigService.CurrentConfig.Mirror = Enum.Parse<MirrorList>(value);
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
                this._korabliConfigService.CurrentConfig.AutoUpdate = value;
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
                this._korabliConfigService.CurrentConfig.Proxy.Enabled = value;
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
                this._korabliConfigService.CurrentConfig.Proxy.Address = value;
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
                this._korabliConfigService.CurrentConfig.Proxy.Username = value;
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
                this._korabliConfigService.CurrentConfig.Proxy.Password = value;
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

        #region Models

        private GameDetectModel SelectedGameDetectModel { get; set; }

        #endregion

        /// <summary>
        /// 构造视图模型
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="lgcIntegrator">LGC探查器</param>
        /// <param name="korabliConfigService">LGC探查器</param>
        public MainWindowViewModel(
            Lazy<ILogger<MainWindowViewModel>> logger,
            Lazy<LgcIntegratorService> lgcIntegrator,
            Lazy<KorabliConfigService> korabliConfigService)
        {
            this._logger = logger.Value;
            this._lgcIntegratorService = lgcIntegrator.Value;
            this._korabliConfigService = korabliConfigService.Value;

            var fullVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion!;
            _ = Version.TryParse(fullVersion.Split('+')[0], out var version);
            if (!Directory.Exists(IOExtension.AppDataFolder))
            {
                Directory.CreateDirectory(IOExtension.AppDataFolder);
            }

            this.AppVersion = version ?? Version.Parse("0.0.0");
            this.Message += $"考拉比汉社厂 v{this.AppVersion}\r\n";
            this._selectedUpdateMirror = this._korabliConfigService.CurrentConfig.Mirror.ToString();
            this._lgcIntegratorService.ServiceEvent += this.SyncServiceMessage;
            this._korabliConfigService.ServiceEvent += this.SyncServiceMessage;

            this._lgcIntegratorService.Load();
            this._korabliConfigService.Load();

            this._gameFolders = [];
            this._autoUpdate = this._korabliConfigService.CurrentConfig.AutoUpdate;
            this._proxyEnabled = this._korabliConfigService.CurrentConfig.Proxy.Enabled;
            this._proxyAddress = this._korabliConfigService.CurrentConfig.Proxy.Address;
            this._proxyUsername = this._korabliConfigService.CurrentConfig.Proxy.Username;
            this._proxyPassword = this._korabliConfigService.CurrentConfig.Proxy.Password;

            //var thread = new Thread(async void () =>
            //{
            //    try
            //    {
                    
            //        this.UpdateEnabled = await this._updateHelper.Check(this._korabliConfigService.Mirror, this.AppVersion);
            //        if (!this._updateEnabled || !this._korabliConfigService.AutoUpdate)
            //        {
            //            return;
            //        }

            //        var result = await this._updateHelper.Update(this._korabliConfigService.Mirror);
            //        if (!result)
            //        {
            //            this.SyncServiceMessage(this, new ServiceEventArg
            //            {
            //                Message = "更新失败！"
            //            });
            //        }

            //    }
            //    catch (Exception exception)
            //    {
            //        this.SyncServiceMessage(this, new ServiceEventArg
            //        {
            //            Exception = exception
            //        });

            //    }

            //});

            //thread.Start();

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
            //try
            //{
            //    this._gameFolders.Clear();
            //    this._selectedUpdateMirror = this._korabliConfigService.CurrentConfig.Mirror.ToString();
            //    foreach (var gameFolder in this._lgcIntegratorService.GameFolders)
            //    {
            //        var detectModel = this._gameDetectorService.Load(gameFolder);
            //        if (detectModel?.IsWarship ?? false)
            //        {
            //            this._gameFolders.Add(gameFolder);
            //        }

            //    }
            //    this._gameFolders.Add(MainWindowViewModel.ManualSelectionHint);

            //    this._selectedGameFolder = this._gameFolders.Contains(this._korabliConfigService.GameFolder) ? this._korabliConfigService.GameFolder : string.Empty;
            //    if (!string.IsNullOrEmpty(this._selectedGameFolder))
            //    {
            //        var detectModel = this._gameDetectorService.Load(this._selectedGameFolder);
            //        if (detectModel is null)
            //        {
            //            this._selectedGameFolder = string.Empty;
            //            this._korabliConfigService.GameFolder = string.Empty;
            //            await this._korabliConfigService.SaveAsync();
            //        }
            //    }

            //    this.RefreshViews();
            //}
            //catch (Exception e)
            //{
            //    this.SyncServiceMessage(this, new ServiceEventArg
            //    {
            //        Message = "读取游戏信息失败！",
            //        Exception = e,
            //        AppendException = false
            //    });

            //}
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
            RaisePropertyChanged(nameof(this.RemoveEnabled));
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
            //try
            //{
            //    if (string.IsNullOrEmpty(this._selectedGameFolder))
            //    {
            //        return;
            //    }

            //    if (string.Compare(this.SelectedGameFolder, MainWindowViewModel.ManualSelectionHint,
            //            StringComparison.OrdinalIgnoreCase) == 0)
            //    {
            //        var dialog = new OpenFolderDialog
            //        {
            //            Multiselect = false,
            //            Title = "选择窝窝屎本体位置"
            //        };

            //        var result = dialog.ShowDialog();
            //        if (!(result ?? false))
            //        {
            //            this._selectedGameFolder = string.Empty;
            //            RaisePropertyChanged(nameof(this.SelectedGameFolder));
            //            return;
            //        }

            //        this._selectedGameFolder = dialog.FolderName;
            //        this._lgcIntegratorService.GameFolders.Add(this._selectedGameFolder);
            //    }

            //    var detectModel = this._gameDetectorService.Load(this._selectedGameFolder);
            //    if (detectModel is null)
            //    {
            //        this._selectedGameFolder = string.Empty;
            //        this._korabliConfigService.GameFolder = string.Empty;
            //    }
            //    else
            //    {
            //        this._korabliConfigService.GameFolder = this._selectedGameFolder;
            //    }

            //    await this._korabliConfigService.SaveAsync();
            //    this.RefreshViews();
            //}
            //catch (Exception e)
            //{
            //    this.SyncServiceMessage(this, new ServiceEventArg
            //    {
            //        Message = "读取游戏信息失败！",
            //        Exception = e
            //    });
            //}

        });

        /// <summary>
        /// 绑定安装汉化Mod
        /// </summary>
        public DelegateCommand InstallChsModCommand => new(async void () =>
        {
            //try
            //{
            //    this.CoreEnabled = false;
            //    var result = await this._chsModService.Install(this._korabliConfigService.Mirror, new GameDetectModel());
            //    if (!result)
            //    {
            //        this.SyncServiceMessage(this, new ServiceEventArg
            //        {
            //            Message = "安装汉化失败！"
            //        });
            //        return;
            //    }

            //    this.SyncServiceMessage(this, new ServiceEventArg
            //    {
            //        Message = "安装汉化成功！"
            //    });

            //}
            //catch (Exception e)
            //{
            //    this.SyncServiceMessage(this, new ServiceEventArg
            //    {
            //        Message = "安装汉化失败！",
            //        Exception = e
            //    });
            //}
            //finally
            //{
            //    this.Reload();
            //    this.CoreEnabled = true;
            //    this.RefreshViews();
            //}
        });

        /// <summary>
        /// 绑定卸载汉化Mod
        /// </summary>
        public DelegateCommand UninstallChsModCommand => new(void () =>
        {
            try
            {
                this.CoreEnabled = false;
                IOExtension.DeleteDirectory(this.SelectedGameDetectModel.ModFolder);
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
                    var proxy = this._korabliConfigService.UpdateEngineProxy();
                    if (!proxy)
                    {
                        this.SyncServiceMessage(this, new ServiceEventArg
                        {
                            Message = "代理设置失败！"
                        });

                        return;
                    }

                }

                var result = await this._korabliConfigService.SaveAsync();
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
            //try
            //{
            //    this._updateEnabled = false;
            //    var result = await this._updateHelper.Update(this._korabliConfigService.Mirror);
            //    if (!result)
            //    {
            //        this.SyncServiceMessage(this, new ServiceEventArg
            //        {
            //            Message = "更新失败！"
            //        });
            //        return;
            //    }

            //    this.SyncServiceMessage(this, new ServiceEventArg
            //    {
            //        Message = "更新完成！"
            //    });

            //    this.UpdateEnabled = true;
            //}
            //catch (Exception e)
            //{
            //    this.SyncServiceMessage(this, new ServiceEventArg
            //    {
            //        Message = "更新失败！",
            //        Exception = e
            //    });
            //}
        });

        #endregion

        #endregion
    }
}
