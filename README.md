# RenderTextureDebuggerについて 
## このツールについて

このツールは、UnityEditor上で実行中の際にRenderTextureを確認するツールです。

## 利用方法について

「Tools/UTJ/RenderTextureDebugger」で下記ウィンドウを呼び出します。<br />

![Alt text](/Documentation~/img/RenderTextureDebugger.png)
<br />

ここでRenderTextureの状況を確認することが可能になっています。<br />
1:表示するサイズを変更します <br />
2:表示するRenderTextureを名前でフィルタリングします<br />

## その他
Unity 2020.2以降ならば HDR画像の保存・及びDepthをHDR画像として保存することに対応しました。
(DepthOnlyならば、普通に保存できている様子ですが… Color付きだとなんかカラーが少し乗っかってしまっているDepth画像になってしまっております)
