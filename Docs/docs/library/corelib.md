# コアライブラリ
## システム関数

### println関数
```
println : α -> ()
```
整数、浮動小数点数、文字列、文字の値を表示して改行します。

### print関数
```
print : α -> ()
```
整数、浮動小数点数、文字列、文字の値を表示します。

### error関数
```
error : String -> α
```
実行時エラーを発生させます。
引数にはエラー内容を記述します。

### undefined関数
```
undefined : α -> β
```
変数や関数が未定義であることを示します。

### ignore関数
```
ignore : α -> ()
```
引数を無視して`()`を返します。

## 比較演算
### (==)演算子
```
__operator_eqeq : α -> α -> Bool
```
整数、浮動小数点数、文字列、文字、真偽値、ユニットの値を比較し、
等しければ`true`をそうでなければ`false`を返します。

### (<>)演算子
```
__operator_ltgt : α -> α -> Bool
```
整数、浮動小数点数、文字列、文字、真偽値、ユニットの値を比較し、
等しければ`false`をそうでなければ`true`を返します。

### (<)演算子
```
__operator_lt : α -> α -> Bool
```
整数、浮動小数点数、文字列、文字、真偽値、ユニットの値を比較し、
左辺の方が右辺の値より小さい場合に`true`を、
そうでない場合に`false`を返します。

### (>)演算子
```
__operator_gt : α -> α -> Bool
```
整数、浮動小数点数、文字列、文字、真偽値、ユニットの値を比較し、
左辺の方が右辺の値より大きい場合に`true`を、
そうでない場合に`false`を返します。

### (<=)演算子
```
__operator_le : α -> α -> Bool
```
整数、浮動小数点数、文字列、文字、真偽値、ユニットの値を比較し、
左辺の方が右辺の値以下の場合に`true`を、
そうでない場合に`false`を返します。

### (>=)演算子
```
__operator_le : α -> α -> Bool
```
整数、浮動小数点数、文字列、文字、真偽値、ユニットの値を比較し、
左辺の方が右辺の値以上の場合に`true`を、
そうでない場合に`false`を返します。

### intequal関数
```
intequal : Int -> Int -> Bool
```
2つの値を整数として比較して、等しい場合に`true`を、
そうでない場合に`false`を返します。
列挙体の値を等しさを判定する場合に使います。

### max関数
```
max : α -> α -> α
```
2つの引数のうち大きい方の値を返します。
値は、整数、浮動小数点数、文字列、文字、真偽値、ユニットの値の
いずれかである必要があります。

### min関数
```
min : α -> α -> α
```
2つの引数のうち小さい方の値を返します。
値は、整数、浮動小数点数、文字列、文字、真偽値、ユニットの値の
いずれかである必要があります。

## 整数演算
### (+)演算子
```
__operator_pls : Int -> Int -> Int
```
整数同士の加算です。

### (-)演算子
```
__operator_mns : Int -> Int -> Int
```
整数同士の減算です。

### (*)演算子
```
__operator_ast : Int -> Int -> Int
```
整数同士の乗算です。

### (/)演算子
```
__operator_sls : Int -> Int -> Int
```
整数同士の除算です。

### (%)演算子
```
__operator_per : Int -> Int -> Int
```
整数同士の剰余です。

### (~-)演算子
```
__operator_neg : Int -> Int
```
整数の符号を反転します。

### succ関数
```
succ : Int -> Int
```
引数の値に1足した値を返します。

### pred関数
```
pred : Int -> Int
```
引数の値から1引いた値を返します。

### abs関数
```
abs : Int -> Int
```
引数の整数の絶対値を求めます。

## ビット演算
### band関数
```
band : Int -> Int -> Int
```
2つの整数のビットごとの論理積を求めます。

### bor関数
```
bor : Int -> Int -> Int
```
2つの整数のビットごとの論理和を求めます。

### bxor関数
```
bxor : Int -> Int -> Int
```
2つの整数のビットごとの排他的論理和を求めます。

### bnot関数
```
bnot : Int -> Int
```
整数のすべてのビットを反転した値を返します。

### bshr関数
```
bshr : Int -> Int -> Int
```
整数を指定した値分だけ右に算術シフトします。

### bshl関数
```
bshl : Int -> Int -> Int
```
整数を指定した値分だけ左にシフトします。

### bshrun関数
```
bshrun : Int -> Int -> Int
```
整数を指定した値分だけ右に論理シフトします。

## 浮動小数点数演算
### (+.)演算子
```
__operator_plsdot : Double -> Double -> Double
```
浮動小数点数同士の和を求めます。

### (-.)演算子
```
__operator_mnsdot : Double -> Double -> Double
```
浮動小数点数同士の差を求めます。

### (*.)演算子
```
__operator_astdot : Double -> Double -> Double
```
浮動小数点数同士の積を求めます。

### (/.)演算子
```
__operator_astdot : Double -> Double -> Double
```
浮動小数点数の左辺の値を右辺の値で割った値を求めます。

### (~-.)演算子
```
__operator_negdot : Double -> Double
```
浮動小数点数の符号を反転します。

### fabs関数
```
fabs : Double -> Double
```

### pi定数
```
pi : Double
```
円周率です。

### pi2定数
```
pi2 : Double
```
円周率の2倍です。

### e定数
```
e : Double
```
自然対数の底です。

### sqrt関数
```
sqrt : Double -> Double
```
平方根を求めます。

### exp関数
```
exp : Double -> Double
```
自然対数の底の累乗を求めます。

### log関数
```
log : Double -> Double
```
自然対数です。

### log10関数
```
log10 : Double -> Double
```
常用対数です。

### logn関数
```
logn : Double -> Double -> Double
```
`logn x base`で対数の底`base`を指定して`x`の対数を求めます。

### pow関数
```
pow : Double -> Double -> Double
```
`pow x y`で`x`の`y`乗を求めます。

### sin関数, cos関数, tan関数
```
sin : Double -> Double
cos : Double -> Double
tan : Double -> Double
```
三角関数です。単位はラジアンです。

### asin関数, acos関数, atan関数
```
asin : Double -> Double
acos : Double -> Double
atan : Double -> Double
```
逆三角関数です。

### atan2関数
```
atan2 : Double -> Double -> Double
```
`atan2 x y`で`x/y`の逆正接を求めます。

### sinh関数、cosh関数、tanh関数
```
双曲線関数です。
```

### ceiling関数
```
ceiling : Double -> Double
```
指定した数以上の数のうち最小の整数値を返します。

### truncate関数
```
truncate : Double -> Double
```
指定した数の整数部を返します。

### round関数
```
round : Double -> Double
```
指定した数を最も近い整数に丸めます。

### floor関数
```
floor : Double -> Double
```
指定した数以下の数のうち最大の整数値を返します。

## 文字列演算
### (^)演算子
```
__operator_hat : String -> String -> String
```
2つの文字列を連結します。

### to_string関数
```
to_string : α -> String
```
引数の値を文字列化します。
値は、整数、浮動小数点数、文字列、文字のいずれかである必要があります。

### strlen関数
```
strlen : String -> Int
```
文字列の長さを求めます。

### strnth関数
```
strnth : String -> Int -> Char
```
文字列のn番目の文字を返します。

## 文字演算
```
is_control : Char -> Bool
```
文字が制御文字であれば真を返します。

```
is_digit : Char -> Bool
```
文字が十進数字であれば真を返します。

```
is_letter : Char -> Bool
```
文字がUnicode文字であれば真を返します。

```
is_letter_or_digit : Char -> Bool
```
文字がUnicode文字か十進数字であれば真を返します。

```
is_lower : Char -> Bool
```
文字が小文字であれば真を返します。

```
is_symbol : Char -> Bool
```
文字が記号であれば真を返します。

```
is_upper : Char -> Bool
```
文字が大文字であれば真を返します。

```
is_whitespace : Char -> Bool
```
文字が空白文字であれば真を返します。

```
to_lower : Char -> Char
```
文字を小文字に変換します。

```
to_upper : Char -> Char
```
文字を大文字に変換します。

## 論理演算

### not関数
```
not : Bool -> Bool
```
真偽値の反転です。

```
xor : Bool -> Bool -> Bool
```
真偽値の排他的論理和です。

## 変換
### parse_int関数
```
parse_int : String -> Int
```
整数を表す文字列を整数値に変換します。

### parse_double関数
```
parse_double : String -> Double
```
浮動小数点数を表す文字列を浮動小数点数値に変換します。

### int_to_double関数
```
int_to_double : Int -> Double
```
整数を浮動小数点数値に変換します。

### double_to_int関数
```
double_to_int : Double -> Int
```
浮動小数点数値を整数に変換します。

### int_to_char関数
```
int_to_char : Int -> Char
```
文字コードを文字に変換します。

### char_to_int関数
```
char_to_int : Char -> Int
```
文字の文字コードを返します。

### int_to_single関数
```
int_to_single : Int -> {System.Single}
```
整数を単精度浮動小数点数に変換します。

### single_to_int関数
```
single_to_int : {System.Single} -> Int
```
単精度浮動小数点数を整数に変換します。

### double_to_single関数
```
double_to_single : Double -> {System.Single}
```
倍精度浮動小数点数を単精度浮動小数点数に変換します。

### single_to_double関数
```
single_to_double : {System.Single} -> Double
```
単精度浮動小数点数を倍精度浮動小数点数に変換します。

### box関数
```
box : α -> {System.Object}
```
値をObject型にキャストします。

## 関数

### (<|)演算子
```
__operator_ltbar : (α -> β) -> α -> β
```
関数適用演算子。

### (|>)演算子
```
__operator_bargt : α -> (α -> β) -> β
```
逆方向関数適用演算子

### (<<)演算子
```
__operator_ltlt : (α -> β) -> (γ -> α) -> γ -> β
```
関数合成。

### (>>)演算子
```
__operator_gtgt : (α -> β) -> (β -> γ) -> α -> γ
```
逆方向関数合成。

### match関数
```
match : α -> (α -> β) -> β
```
第1引数の値を第2引数の関数に適用する。

### id関数
```
id : α -> α
```
恒等関数。

### const関数
```
const : α -> β -> α
```
2引数受け取って、1引数目を返す。

### flip関数
```
flip : (α -> β -> γ) -> β -> α -> γ
```
関数の引数順を入れ替える。

## タプル
### fst関数
```
fst : (α, β) -> α
```
2要素タプルの最初の要素を返す。

### snd関数
```
snd : (α, β) -> β
```
2要素タプルの2番目の要素を返す。

### curry関数
```
curry : ((α, β) -> γ) -> α -> β -> γ
```
関数をカリー化する。

### uncurry関数
```
uncurry : (α -> β -> γ) -> (α, β) -> γ
```
関数のカリー化を解除する。

## リファレンスセル
### ref関数
```
ref : α -> Ref<α>
```
リファレンスセルを作る。

### (!)演算子
```
__operator_bang : Ref<α> -> α
```
参照を剥がす。

### (:=)演算子
```
__operator_coleq : Ref<α> -> α -> ()
```
参照に代入する。

### incr関数
```
incr : Ref<Int> -> ()
```
参照の値を1増やす。

### decr関数
```
decr : Ref<Int> -> ()
```
参照の値を1減らす

## リスト
### (..)演算子
```
__operator_dotdot : Int -> Int -> [Int]
```
指定した範囲の整数を含むリストを生成する。

### (++)演算子
```
__operator_plspls : [α] -> [α] -> [α]
```
2つのリストを連結する。

### length関数
```
length : [α] -> Int
```
リストの長さを返す。

### reverse関数
```
reverse : [α] -> [α]
```
反転したリストを返す。

### map関数
```
map : (α -> β) -> [α] -> [β]
```
リストの各要素に関数を適用したリストを返す。

### is_nil関数
```
is_nil : [α] -> Bool
```
リストが空リストであれば真を返します。

### head関数
```
head : [α] -> α
```
リストの先頭要素を返す。

### tail関数
```
tail : [α] -> [α]
```
リストの先頭を取り除いたリストを返す。

### nth関数
```
nth : [α] -> Int -> α
```
リストのn番目の要素を返す

### foldl関数
```
foldl : (α -> β -> α) -> α -> [β] -> α
```
リストを左から右に向かって畳み込みする。

### foldr関数
```
foldr : (α -> β -> β) -> β -> [α] -> β
```
リストを右から左に向かって畳み込みする。

### lookup関数
```
lookup : α -> [(α, β)] -> Maybe<β>
```
連想リストから対応する値を探索する。
見つからなければ`Nothing`を見つかった場合は`Just`でくるんだ値を返す。

### concat関数
```
concat : [[α]] -> [α]
```
リストのリストをリストに変換する。

### concat_map関数
```
concat_map : (α -> [β]) -> [α] -> [β]
```
`map`してその結果を`concat`する。
 
## Maybe
```
type Maybe<T> = Nothing | Just(T);
```
### maybe関数
```
maybe : α -> (β -> α) -> Maybe<β> -> α
```
`Maybe`値に関数を適用する。`Nothing`のときはデフォルト値を返す。

### map_maybe関数
```
map_maybe : (α -> β) -> Maybe<α> -> Maybe<β>
```
`Maybe`値に関数を適用する。`Nothing`のときは`Nothing`を返す。

### is_just関数
```
is_just : Maybe<α> -> Bool
```
`Just(_)`であれば`true`を返し、そうでなければ`false`を返す。

### is_nothing関数
```
is_nothing : Maybe<α> -> Bool
```
`Nothing`であれば`true`を返し、そうでなければ`false`を返す。

### from_just関数
```
from_just : Maybe<α> -> α
```
`Maybe`値から値を取り出す。`Nothing`の場合はエラー

## 文字のリスト
### string_to_list
```
string_to_list : String -> [Char]
```
文字列を文字のリストに変換します。

```
list_to_string : [Char] -> String
```
文字のリストを文字列に変換します。

## Either
```
type Either<T, U> = Left(T) | Right(U);
```

### either関数
```
either : (α -> β) -> (γ -> β) -> Either<α, γ> -> β
```
`Either`値に関数を適用する。

## 配列
### list_to_array関数
```
list_to_array : [{System.Object}] -> {System.Object[]}
```
`Object`のリストを`Object[]`に変換する。

### format関数
```
format : String -> [{System.Object}] -> String
```
フォーマット文字列に従って、値を文字列に変換する。

## リフレクション
### get_type関数
```
get_type : String -> {System.Type}
```
指定した名前の`Type`オブジェクトを返す。

### null定数
```
null : {System.Object}
```
null値。

## メッセージ
### msgbox関数
```
msgbox : α -> ()
```
メッセージボックスに値を表示する。
値は整数、浮動小数点数、文字列、文字のいずれか。
