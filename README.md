<p align="center">
<img width="200" height="200" src="https://raw.githubusercontent.com/lambdataro/Mokkosu/master/Logo/mokkosu.png"/>
</p>

Mokkosuはインタラクティブなコンテンツを手軽に作成可能な関数型プログラミング言語です。

[公式サイトはこちら](http://lambdataro.sakura.ne.jp/mokkosu/)

[ダウンロードはこちら](https://github.com/lambdataro/Mokkosu/releases)

# 主な機能
## 関数型プログラミング言語
ラムダ式や一級関数、自動カリー化、代数的データ型、パターンマッチ、リスト内包表記、末尾再帰最適化といった
関数型言語でよく見かける機能を搭載しています。

## .NET Frameworkの中間言語へのコンパイラ
Mokkosuのコンパイラは.NET Frameworkへの中間言語を吐き出します。また、プログラムから、.NET Framework
のクラスライブラリに用意されている豊富な機能を利用することができます。

## 統合開発環境(IDE)
Mokkosuはプログラム言語のコンパイラに加えて、開発環境の整備にも力を入れています。
Mokkosu専用の簡易IDEであるMokkosuPadは、シンタックスハイライトや推論された型の表示、
ワンボタンでのプログラムの実行などをサポートしています。

## 2D描画用フレームワーク
Mokkosuは2Dの描画を行うインタラクティブなプログラムを手軽に実装できる環境の構築を目指して開発しています。
Mokkosuの標準ライブラリには関数型スタイルで2Dの描画やマウスやキーボードからの入力の受け取りを記述できる
2D描画用フレームワークが用意されています。2D描画用フレームワークはMokkosuのプログラムで記述されています。

## 充実したドキュメント
言語マニュアルとライブラリマニュアルに加えて、関数型言語初心者をターゲットとしたチュートリアルを
用意しており、だれでも手軽に始められる環境を整備しています。

# プログラム例
## Hello, World.
```
msgbox "Hello, World.";
```

## 階乗計算
```
fun fact = {
  0 -> 1;
  n -> n * fact (n - 1);
};
```

## 代数的データ型とパターンマッチ
```
type Expr = Num(Int) | Add(Expr, Expr) | Mul(Expr, Expr);

fun eval = {
  ~Num(n) -> n;
  ~Add(e1, e2) -> eval e1 + eval e2;
  ~Mul(e1, e2) -> eval e1 * eval e2;
};
```

## リスト内包表記
```
ignore (
  for x <- 1 .. 10;
      y <- 1 .. 10;
      z <- 1 .. 10;
      if x * x + y * y == z * z;
  in  println(format "{0}, {1}, {2}" (map box [x, y, z])));
```
