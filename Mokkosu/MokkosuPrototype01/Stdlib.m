#============================================================
# Mokkosu 標準ライブラリ
#============================================================

__define "HIDE_TYPE";

import "mscorlib.dll";
import "System.dll";
import "System.Core.dll";

#------------------------------------------------------------
# システム関数
#------------------------------------------------------------

let println x = __prim "println" (x);
let print x = __prim "print" (x);
let error x = __prim "error" (x);
let undefined _ = error "undefined";

#------------------------------------------------------------
# 比較演算
#------------------------------------------------------------

let __operator_eqeq x y = __prim "eq" (x, y);
let __operator_ltgt x y = __prim "ne" (x, y);
let __operator_lt x y = __prim "lt" (x, y);
let __operator_gt x y = __prim "gt" (x, y);
let __operator_le x y = __prim "le" (x, y);
let __operator_ge x y = __prim "ge" (x, y);

#------------------------------------------------------------
# 整数演算
#------------------------------------------------------------

let __operator_pls x y = __prim "add" (x, y);
let __operator_mns x y = __prim "sub" (x, y);
let __operator_ast x y = __prim "mul" (x, y);
let __operator_sls x y = __prim "div" (x, y);
let __operator_per x y = __prim "mod" (x, y);

let __operator_neg x = 0 - x;

fun __operator_astast x y =
  if y == 0 -> 1
  else x * x ** (y - 1);

let succ x = x + 1;
let pred x = x - 1;

let abs x = if x >= 0 -> x else ~-x;

fun from_to x y =
  if x > y -> []
  else x :: from_to (x + 1) y;

let __operator_dotdot = from_to;

#------------------------------------------------------------
# ビット演算
#------------------------------------------------------------

let band x y = __prim "band" (x, y);
let bor x y = __prim "bor" (x, y);
let bxor x y = __prim "bxor" (x, y);
let bnot x = __prim "bnot" (x);
let bshr x y = __prim "bshr" (x, y);
let bshl x y = __prim "bshl" (x, y);
let bshrun x y = __prim "bshrun" (x, y);

#------------------------------------------------------------
# 浮動小数点数演算
#------------------------------------------------------------

let __operator_plsdot x y = __prim "fadd" (x, y);
let __operator_mnsdot x y = __prim "fsub" (x, y);
let __operator_astdot x y = __prim "fmul" (x, y);
let __operator_slsdot x y = __prim "fdiv" (x, y);

let __operator_negdot x = 0.0 -. x;

let fabs x = if x >= 0.0 -> x else ~-.x;

let pi = sget System.Math::PI;
let e = sget System.Math::E;

let sqrt x = call System.Math::Sqrt((x : Double));
let exp x = call System.Math::Exp((x : Double));
let log x = call System.Math::Log((x : Double));
let log10 x = call System.Math::Log10((x : Double));
let logn x y = call System.Math::Log((x : Double), (y : Double));
let pow x y = call System.Math::Pow((x : Double), (y : Double));

let sin x = call System.Math::Sin((x : Double));
let cos x = call System.Math::Cos((x : Double));
let tan x = call System.Math::Tan((x : Double));

let asin x = call System.Math::Asin((x : Double));
let acos x = call System.Math::Acos((x : Double));
let atan x = call System.Math::Atan((x : Double));
let atan2 x y = call System.Math::Atan2((x : Double), (y : Double));

let sinh x = call System.Math::Sinh((x : Double));
let cosh x = call System.Math::Cosh((x : Double));
let tanh x = call System.Math::Tanh((x : Double));

let ceiling x = call System.Math::Ceiling((x : Double));
let truncate x = call System.Math::Truncate((x : Double));
let round x = call System.Math::Round((x : Double));

#------------------------------------------------------------
# 文字列演算
#------------------------------------------------------------

let __operator_hat x y = __prim "concat" (x, y);
let tostring x = __prim "tostring" (x);

#------------------------------------------------------------
# リファレンスセル
#------------------------------------------------------------

let ref x = __prim "ref" (x);
let __operator_bang x = __prim "deref" (x);
let __operator_coleq x y = __prim "assign" (x, y);

#------------------------------------------------------------
# 関数
#------------------------------------------------------------

let __operator_ltbar f x = f x;
let __operator_bargt x f = f x;
let __operator_ltlt f g x = f (g x);
let __operator_gtgt f g x = g (f x);
let match x f = f x;
let id x = x;
let const x y = x;
let flip f y x = f x y;

#------------------------------------------------------------
# 論理演算
#------------------------------------------------------------

let not = { true -> false; false -> true };

#------------------------------------------------------------
# リスト
#------------------------------------------------------------

let print_list lis =
  fun loop lis =
    match lis {
	  [] -> ();
	  [x] -> print x;
	  x :: xs ->
	   do print x in
	   do print ", " in
	   loop xs; 
	}
  in
  do print "[" in
  do loop lis in
  println "]";

fun __operator_plspls xs ys =
  match xs {
    [] -> ys;
	hd :: tl -> hd :: (tl ++ ys);
  };

fun length lis =
  match lis {
    [] -> 0;
	_ :: xs -> 1 + length xs;
  };

fun reverse lis =
  match lis {
    [] -> [];
	x :: xs -> reverse xs ++ [x];
  };

fun map f = {
  [] -> [];
  x :: xs -> f x :: map f xs;
};

fun filter p = {
  [] -> [];
  x :: xs ? p x -> x :: filter p xs;
  _ :: xs -> filter p xs;
};

let head (x :: _) = x;
let tail (_ :: xs) = xs;

fun last lis =
  match lis {
    [x] -> x;
	_ :: xs -> last xs;
  };

fun init lis =
  match lis {
    [x] -> [];
	x :: xs -> x :: init xs; 
  };

fun nth n lis =
  if n == 0 -> head lis
  else nth (n - 1) (tail lis);

let null = { [] -> true; _ -> false };

fun foldl f seed = {
  [] -> seed;
  x :: xs -> foldl f (f seed x) xs;
};

fun foldr f seed = {
  [] -> seed;
  x :: xs -> f x (foldr f seed xs)
};

fun concat lis =
  match lis {
    [] -> [];
	x :: xs -> x ++ concat xs;
  };

let concatmap f lis = concat (map f lis); 

let __for_bind lis f = concatmap f lis;
let __for_unit x = [x];
let __for_zero = [];


#------------------------------------------------------------
# Maybe<T>
#------------------------------------------------------------

type Maybe<T> = Nothing | Just(T);

let maybe def f = { ~Nothing -> def; ~Just(x) -> f x };

#------------------------------------------------------------
# Either<T>
#------------------------------------------------------------

type Either<T, U> = Left(T) | Right(U);

let either f g = { ~Left(x) -> f x; ~Right(x) -> g x };

#------------------------------------------------------------
# Ordering
#------------------------------------------------------------

type Ordering = LT | EQ | GT;

let compare x y =
  if x == y -> EQ
  else if x < y -> LT
  else GT;

let max x y = if x > y -> x else y;
let min x y = if x < y -> x else y;

#------------------------------------------------------------
# Tuple
#------------------------------------------------------------

let fst (x, _) = x;
let snd (_, x) = x;

let curry f a b = f (a, b);
let uncurry f (a, b) = f a b;

__undefine "HIDE_TYPE";
