# HybridCLRFramework
## 已有功能
* ~~1.网络和proto~~
* ~~2.表格模块~~
* ~~3.HybridCLR~~
* ~~4.AB模块~~
* ~~5.事件模块~~
* ~~6.对象池~~
* ~~7.json模块~~
* ~~8.DOTween~~
* ~~9.视频模块(支持各种直播)~~
* ~~10.链式定时器模块~~

## TODO功能
* UI模块
* 战斗系统
* 性能分析模块
* 声音模块
* 特效模块
* 加密模块
* 动作、技能编辑器
* 自动打包
* UGUI扩展组件
* UI动画组件
* Log模块

## 待优化模块
* 网络协议

## 操作提示
* 首先先打一下包
* 非Bundle模式(1.需要生成dll ABTool/Gen/Dll)
* Bundle模式(1.生成dll 2.生成Bundle 3.导出包)

## 提示
* 如果在Loading界面报错，MessageBox找不到，那是StreamingAsset目录下缺少非热更的资源包导致的
* develop模式必须要确保工程本地有dll，如果没有话ABTool/Generate/Dll生成，打包的时候确保dll删除
