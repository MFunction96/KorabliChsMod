# 考拉比汉社厂 KorabliChsMod

[![Build Status](https://dev.azure.com/XanaCN/Lyoko/_apis/build/status/KorabliChsMod/Build?branchName=main)](https://dev.azure.com/XanaCN/Lyoko/_build/latest?definitionId=20&branchName=main) [![Release Status](https://vsrm.dev.azure.com/XanaCN/_apis/public/Release/badge/f06af8ee-5084-455c-ac24-8fc4f735382c/5/7)](https://dev.azure.com/XanaCN/Lyoko/_release?view=all&path=%5CKorabliChsMod&_a=releases) [![Code Coverage](https://img.shields.io/azure-devops/coverage/XanaCN/Lyoko/20/main)]()

[![dotnet](https://img.shields.io/badge/.NET-%3E%3D8.0.4-blue.svg?style=flat-square&logo=.NET)](https://dotnet.microsoft.com/)
[![GitHub License](https://img.shields.io/github/license/MFunction96/KorabliChsMod)](https://github.com/MFunction96/KorabliChsMod/blob/main/LICENSE)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/MFunction96/KorabliChsMod/total)


傻瓜式就连小刻都会用的莱服窝窝屎汉化更新程序。

**P.S. 程序还在早期开发过程中，可能会随时表演胡德绝活，敬请见谅！**

---

## 安装使用

考拉比汉社厂由.NET 8编写，因此需要.NET 8运行环境。

### 安装

无论Github还是Gitee，均转去最新发布下载即可，或访问以下链接以下载安装包含.NET 8运行环境的安装包：

Github: [KorabliChsModInstallerWithRuntime.exe](https://github.com/MFunction96/KorabliChsMod/releases/latest/download/KorabliChsModInstallerWithRuntime.exe)

Gitee: *待完善*
> 如果确定本地已安装.NET 8运行环境，可选择以下安装包：
>
> Github: [KorabliChsModInstaller.exe](https://github.com/MFunction96/KorabliChsMod/releases/latest/download/KorabliChsModInstaller.exe)
>
> Gitee: *待完善*

按照安装说明及提示完成安装即可，默认安装位置为：`%AppData%\KorabliChsMod`，即`C:\Users\<用户名>\AppData\Roaming\KorabliChsMod`，安装过程中会自动在桌面创建快捷方式。

> **<font color='red'>请勿安装在包含空格的路径下，当前版本暂不支持此类路径静默升级。</font>** ~~*后续可能懒得支持了*~~
> 
> 推荐路径：`C:\Program`
>
> 不推荐路径：`C:\Program Files` （包含空格）

### 使用

打开桌面快捷方式，进入程序。

1. 选择正确的游戏客户端安装的位置。
2. 点击`安装`，等待片刻即可完成。**无论检测到汉化安装与否，程序均会安装/覆盖安装最新版本的汉化补丁至指定客户端**。

![主程序](https://dev.azure.com/XanaCN/f06af8ee-5084-455c-ac24-8fc4f735382c/_apis/git/repositories/d36405a6-bc74-45e3-b720-3a2c79f5c30e/items?path=/doc/README/MainWindow.png)
![主程序](https://dev.azure.com/XanaCN/f06af8ee-5084-455c-ac24-8fc4f735382c/_apis/git/repositories/d36405a6-bc74-45e3-b720-3a2c79f5c30e/items?path=/doc/README/MainWindowDetail.png)

> 程序会自动检测游戏客户端安装位置，如果检测不到，请手动选择游戏客户端安装位置。

### 更新

方法一：在程序界面标签-关于-更新，稍等片刻即可完成自更新。

![升级](https://dev.azure.com/XanaCN/f06af8ee-5084-455c-ac24-8fc4f735382c/_apis/git/repositories/d36405a6-bc74-45e3-b720-3a2c79f5c30e/items?path=/doc/README/Update.png)

方法二：按照安装方法重新下载安装即可。

## 计划

**一切都是拍脑袋决定的，多拍一拍没准就会多一条。~~不许用脚后跟拍脑袋！！！~~**

### 阶段一

- [x] 建立Azure DevOps全自动工作流
- [x] Github, Gitee镜像仓库同步
- [x] Github镜像仓库自动发布
- [x] 主汉化程序自更新功能
- [x] 主汉化更新程序核心功能，仅支持Github源的正式服汉化，包括汉化下载与更新、版本识别等核心功能
- [x] 网络代理选项

### 阶段二

- [ ] 界面美化
- [x] 支持自动定位莱服客户端安装位置
- [x] 支持测试服汉化
- [ ] Gitee镜像仓库自动发布
- [ ] 支持多镜像选择
- [x] 傻瓜安装包
- [ ] 降低殉爆频度

### 阶段三

- [x] 汉化包数字签名与加密
- [x] 主汉化程序数字签名
- [ ] 主播语音包更新支持
- [ ] 其他合规Mod更新支持
- [ ] **¿** *WG服支持*

### To be continue

---

## 链接

### 汉化包源

- https://github.com/DDFantasyV/Korabli_localization_chs

### 主仓库

- https://dev.azure.com/XanaCN/Lyoko/_git/KorabliChsMod

### 镜像仓库

- 国内：https://gitee.com/MFunction96/KorabliChsMod
- 全球：https://github.com/MFunction96/KorabliChsMod

## 发布

- 国内：https://gitee.com/MFunction96/KorabliChsMod/releases
- 全球：https://github.com/MFunction96/KorabliChsMod/releases

## 感谢

来自[@年糕特工队](https://space.bilibili.com/103312972)、[@DDF_FantasyV](https://space.bilibili.com/475887963)、[@walksQAQ](https://space.bilibili.com/87278382)共同制作的汉化。

## 莱服邀请码（字母顺序）

- https://flot.ru/DDF_FantasyV
- https://flot.ru/M_Function
- https://flot.ru/nian__gao233
- https://flot.ru/walksQAQ

---

Copyright &copy; 2018-2024 MFunction.
All rights reserved.