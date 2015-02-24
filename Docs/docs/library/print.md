# 表示関数ライブラリ

このライブラリを使うにはソースコードに
```
include "Print.mok";
```
と追加します。

## 補助関数
### bracket関数
```
bracket : (() -> ()) -> ()
```
関数の実行の前後に角括弧を出力する

### paren関数
```
paren : (() -> ()) -> ()
```
関数の実行の前後に括弧を出力する

### endl関数
```
endl : (() -> ()) -> ()
```
関数を実行して改行文字を出力する。

### sep関数
```
sep : α -> (β -> ()) -> [β] -> ()
```
`sep x f lis`で`lis`の各要素に関数`f`を適用しつつ、
その間で`x`を出力する。

## リストの表示

### print_list_with
```
print_list_with : (α -> ()) -> [α] -> ()
```
指定した関数を使ってリストの要素を表示する。

### println_list_with
```
println_list_with : (α -> ()) -> [α] -> ()
```
リストの各要素を表示して改行する。

### print_list
```
print_list : [α] -> ()
```
リストを表示する。

### println_list
```
println_list : [α] -> ()
```
リストを表示して改行する。

## 2要素タプルの表示
### print_tuple2_with
```
print_tuple2_with : (α -> (), β -> ()) -> (α, β) -> ()
print_pair_with : (α -> (), β -> ()) -> (α, β) -> ()
```
指定した関数を使ってタプルを表示する。

### println_tuple2_with
```
println_tuple2_with : (α -> (), β -> ()) -> (α, β) -> ()
println_pair_with : (α -> (), β -> ()) -> (α, β) -> ()
```
指定した関数を使ってタプルを表示して改行する。

### print_tuple2
```
print_tuple2 : (α, β) -> ()
print_pair : (α, β) -> ()
```
タプルを表示する。

### println_tuple2
```
println_tuple2 : (α, β) -> ()
println_pair : (α, β) -> ()
```
タプルを表示して改行する。

## 3要素タプルの表示
### print_tuple3_with
```
print_tuple3_with : (α -> (), β -> (), γ -> ()) -> (α, β, γ) -> ()
```
指定した関数を使ってタプルを表示する。

### println_tuple3_with
```
println_tuple3_with : (α -> (), β -> (), γ -> ()) -> (α, β, γ) -> ()
```
指定した関数を使ってタプルを表示して改行する。

### print_tuple3
```
print_tuple3 : (α, β, γ) -> ()
```
タプルを表示する。

### println_tuple3
```
println_tuple3 : (α, β, γ) -> ()
```
タプルを表示して改行する。

## 4要素タプルの表示
### print_tuple4_with
```
print_tuple4_with : (α -> (), β -> (), γ -> (), δ -> ()) -> (α, β, γ, δ) -> ()
```
指定した関数を使ってタプルを表示する。

### println_tuple4_with
```
println_tuple4_with : (α -> (), β -> (), γ -> (), δ -> ()) -> (α, β, γ, δ) -> ()
```
指定した関数を使ってタプルを表示して改行する。

### print_tuple4
```
print_tuple4 : (α, β, γ, δ) -> ()
```
タプルを表示する。

### println_tuple4
```
println_tuple4 : (α, β, γ, δ) -> ()
```
タプルを表示して改行する。