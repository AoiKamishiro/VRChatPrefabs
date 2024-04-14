# VRCSDK3 Udon用 高機能タッチスイッチ AKSwitch

![HeaderImage](./_Resources/Images/Thumb.png)  

version 2.0.1  
[English Readme][00]  

制作：神城アオイ([@aoi3192][01])  
アイコン：-お布団-([@Ohuton0501][02])  
効果音：\[ALO\](U-Stella)([@ALOHAGOTO][03])  

## 概要

SDK3用のスイッチギミックです。  
チェックボックス一つでローカル動作とグローバル動作の切り替えが出来ます。  
VRユーザーは物理接触によるスイッチの操作、デスクトップユーザーは従来のインタラクトによる操作が可能です。
また、操作方法は任意のタイミングで切り替えが可能です。  
[Booth配布ページ][71]

## ダウンロード

[リリース][21]ページより最新版のダウンロードをお願いします。  

## 導入方法

1. 事前に、VRCSDK3 と UdonSharp の最新版のインポートをしてください。
2. Assets\00Kamishiro\AKSwitch\Prefabs 内のお好みのプレハブをシーン内に設置してご利用ください。  

## サンプルシーン

サンプルシーンを利用するには、追加で以下のアセットが必要になります。

* [CC0なVRChat向けマテリアル集](https://coquelicotz.booth.pm/items/2516986)  
* [VRChat向け家具セット(１)](https://coquelicotz.booth.pm/items/1276329)  
* [VRChat向け家具セット(２)](https://coquelicotz.booth.pm/items/1573249)  

### アクセス可能な Udon 変数・関数

AKSwich の Udon へのアクセスは、以下がサポートされます。  

* int State - 現在のスイッチの状態が入っている変数です。1-5の範囲です。  
* void OnInteracted() - スイッチのクリック操作を行います。  
* void _PhysicalMode() - スイッチを物理タッチモードへ設定します。  
* void _RaycastMode() - スイッチをインタラクトモードへ設定します。  

## 利用規約

* Assets\00Kamishiro\AKSwitch\_Resources\Textures\Icons.png は本スイッチで利用する場合を除き、再配布を禁じます。販売ワールドに本スイッチを組み込んで販売するなどは可能です。  
* Assets\00Kamishiro\AKSwitch\_Resources\Audios\Click.ogg は本スイッチで利用する場合を除き、再配布を禁じます。販売ワールドに本スイッチを組み込んで販売するなどは可能です。  
* UnityPackage内のその他のアセットは、[MIT License][61]の下で配布しております。  

### 連絡先

[神城工業 Discrod Server][81]  
[Twitter: @aoi3192][82]  
[VRChat: 神城アオイ][83]  

## 関連サイト

[Booth: 神城工業][91]  
[Vket: 神城工業][92]  
[Github: 神城アオイ][93]  

[00]:AKSwitch-README_EN.md
[01]:https://twitter.com/aoi3192
[02]:https://twitter.com/Ohuton0501
[03]:https://twitter.com/ALOHAGOTO
[21]:https://github.com/AoiKamishiro/VRChatPrefabs/releases
[61]:LICENSE-MIT
[71]:https://kamishirolab.booth.pm/items/3159633
[81]:https://discord.gg/8muNKrzaSK
[82]:https://twitter.com/aoi3192
[83]:https://www.vrchat.com/home/user/usr_19514816-2cf8-43cc-a046-9e2d87d15af7
[91]:https://kamishirolab.booth.pm/
[92]:https://www.v-market.work/ec/shops/1810/detail/
[93]:https://github.com/AoiKamishiro
