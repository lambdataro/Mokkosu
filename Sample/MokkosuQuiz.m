#============================================================
#! @file	MokkosuQuiz.m
#! @brief	Mokkosu Quiz
#!
#!   このクイズは、香川考司先生（香川大学）の講義資料の問題を
#!   Mokkosuで書き直したものです。
#!
#!   香川考司. 関数型プログラミング言語 haskell の理論と応用（2007 年度・集中）
#!   http://guppy.eng.kagawa-u.ac.jp/2007/HaskellKyoto/
#!
#! @author	kielnow
#============================================================

#------------------------------------------------------------
# Q1. fib 30を計算せよ。
#------------------------------------------------------------
let fib n =
  fun loop n (a,b) =
    match n {
      0 -> a;
      1 -> b;
      n -> loop (n-1) (b,a+b);
    }
  in loop n (0,1);

do println <| fib 30;

#------------------------------------------------------------
# Q2. リスト中の数の和を求める関数mySumを定義せよ。
#------------------------------------------------------------
let mySum lis =
  fun loop acc = {
    [] -> acc;
    x::xs -> loop (acc+x) xs;
  }
  in loop 0 lis;

do println <| mySum (from_to 1 10);

#------------------------------------------------------------
# Q3. 真偽値のリストを2進数とみなして、対応する整数を計算する関数
#     fromBin : [Bool] -> Int
#     を定義せよ。
#     例: fromBin [true,true] => 3
#         fromBin [true,false,true,false] => 10
#------------------------------------------------------------
let fromBin = foldl (\acc x -> 2*acc+(if x -> 1 else 0)) 0;

do println <| fromBin [true,true];
do println <| fromBin [true,false,true,false];

#------------------------------------------------------------
# Q4. 実数のリストxsと実数から実数への関数fを受け取り、
#     リストの各要素にfを適用した結果の和を計算する関数
#     sumf : [Double] -> (Double -> Double) -> Double
#     を定義せよ。
#------------------------------------------------------------
let sumf xs f = foldr (\x y -> x +. y) 0.0 <| map f (xs : [Double]);

do println <| sumf [1.0, 2.0, 3.0] sqrt;

#------------------------------------------------------------
# Q5. リストを昇べきの順に表わされた多項式とみなし、
#     多項式の値を計算する関数
#     evalPoly : [Double] -> Double -> Double
#     を定義せよ。
#------------------------------------------------------------
let evalPoly cs x = foldr (\c acc -> c +. acc *. x) 0.0 cs;

# 1.0 + 2.0 * x + 3.0 * x^2
do print_list <| map (evalPoly [1.0,2.0,3.0]) [0.0,1.0,2.0,3.0,4.0,5.0];

#------------------------------------------------------------
# Q6. リストを集合だとみなして、そのべき集合（部分集合の集合）を
#     返す関数powersetを定義せよ。
#     例: powerset [1,2,3]
#         => [[],[1],[2],[3],[1,2],[2,3],[3,1],[1,2,3]]
#     （ただし、順番はこの通りでなくてもよい。）
#------------------------------------------------------------
let concat lis = foldr (++) [] lis;
fun powerset = {
  [] -> [];
  [x] -> [[],[x]];
  x::xs -> let ys = powerset xs in ys ++ map {y->x::y} ys;
};

let print_list_with p lis =
  fun loop = {
      [] -> ();
      [x] -> p x;
      x::xs -> do p x; print "," in loop xs; 
    }
  in do print "["; loop lis; println "]" in ();

let print_list_wo_ln lis =
  fun loop = {
      [] -> ();
      [x] -> print x;
      x::xs -> do print x; print "," in loop xs; 
    }
  in do print "["; loop lis; print "]" in ();

do print_list_with print_list_wo_ln <| powerset [1,2,3];

#------------------------------------------------------------
# Q7. 以下の代数的データ型は、二分木を表すデータ型を定義している。
#     type Tree<T> = Branch(Tree<T>,T,Tree<T>) | Empty;
#     このTree型に対して、次のような関数を定義せよ。
#     size      : Tree<α> -> Int     # 要素数
#     depth     : Tree<α> -> Int     # 深さ
#     preorder  : Tree<α> -> [α]     # 前順走査
#     postorder : Tree<α> -> [α]     # 後順走査
#     reflect   : Tree<α> -> Tree<α> # 鏡像
#------------------------------------------------------------
type Tree<T> = Branch(Tree<T>,T,Tree<T>) | Empty;

fun size = {
  ~Empty -> 0;
  ~Branch(b1,_,b2) -> size b1 + 1 + size b2;
};
fun depth = {
  ~Empty -> 0;
  ~Branch(b1,_,b2) -> 1 + max (depth b1) (depth b2);
};
fun preorder = {
  ~Empty -> [];
  ~Branch(b1,e,b2) -> preorder b1 ++ [e] ++ preorder b2;
};
fun postorder = {
  ~Empty -> [];
  ~Branch(b1,e,b2) -> preorder b2 ++ [e] ++ preorder b1;
};
fun reflect = {
  ~Empty -> Empty;
  ~Branch(b1,e,b2) -> Branch(reflect b2, e, reflect b1);
};
let print_tree = {
  ~Empty -> print "Empty";
  ~Branch(b1,e,b2) ->
    do print "Branch(";
       print_tree b1;
       print ",";
       print e;
       print ",";
       print_tree b2;
       print ")"
    in ();
};

let tree1 =
  let b1 = Branch(Empty,2,Empty) in
  let b2 = Branch(Branch(Empty,4,Empty),3,Empty)
  in Branch(b1, 1, b2);

do println <| size tree1;
do println <| depth tree1;
do print_list <| preorder tree1;
do print_list <| postorder tree1;
do print_tree <| tree1;
do print "\n";
do print_tree <| reflect tree1;
do print "\n";

#------------------------------------------------------------
# Q8. geom2を初項1, 項比1の等比数列のリスト[1,2,4,8,16,...]
#     として定義せよ。
#------------------------------------------------------------
let geom2 n = map {x->2**x} <| from_to 0 (n-1);

do print_list <| geom2 10;

#------------------------------------------------------------
# Q9. takeと反対に、リストの最初のn個の要素を取り除く関数
#     myDropを定義せよ。
#------------------------------------------------------------
fun myDrop n ((_::xs) as lis) = if n <= 0 -> lis else myDrop (n-1) xs;

do print_list <| myDrop 4 (from_to 1 10);

#------------------------------------------------------------
# Q10. fibsをフィボナッチ数列のリストとして定義せよ。
#------------------------------------------------------------
let fibs n =
  fun loop n (a,b) =
    match n {
      0 -> [a];
      1 -> [b];
      n -> a::loop (n-1) (b,a+b);
    }
  in loop n (0,1);

do print_list <| fibs 20;

#------------------------------------------------------------
# Q11. 2つの要素に重複のない昇順に並んだリストをマージして、
#      やはり重複のない昇順に並んだリストを生成する関数mergeを
#      定義せよ。
#------------------------------------------------------------
fun dropWhile p = {
  [] -> [];
  x::xs -> if p x -> dropWhile p xs else x::xs
};
let merge xs ys =
  fun loop = {
    ([],[]) -> [];
	(xs,[]) -> xs;
	([],ys) -> ys;
	(x::xs,y::ys) -> 
	    if x == y -> x::loop (xs,ys)
	    else if x < y -> x::loop (xs,y::ys)
	    else y::loop (x::xs,ys);
  }
  in loop (xs,ys);

do print_list <| merge [1,2,4,5,7,9] [2,3,4,6,8,10];

#------------------------------------------------------------
# Q12. 2^i * 3^j * 5^k (i,j,kは0以上の整数)の形で表せる
#      整数のみを重複なしに昇順に並べたリストhammingを定義せよ。
#------------------------------------------------------------
let minimum = {
  [] -> error "minimum";
  (x::xs) -> foldr (\x m -> if x < m -> x else m) x xs;
};
fun index lis n =
  match lis {
    [] -> error "index";
    x::xs -> if n == 0 -> x else index xs (n-1);
  };
fun while p m = if p () -> do m () in while p m else ();
let hamming n =
  let ham = ref [] in
  let (j2,j3,j5) = (ref 0,ref 0,ref 0) in
  let (x2,x3,x5) = (ref 1,ref 1,ref 1) in
  fun loop = {
    0 -> !ham;
    n -> let h = minimum [!x2,!x3,!x5] in
         do ham := !ham ++ [h];
            while {() -> !x2 <= h} {() -> do x2 := 2 * index !ham !j2; j2 := !j2+1 in ()};
            while {() -> !x3 <= h} {() -> do x3 := 3 * index !ham !j3; j3 := !j3+1 in ()};
            while {() -> !x5 <= h} {() -> do x5 := 5 * index !ham !j5; j5 := !j5+1 in ()};
         in loop (n-1);
  }
  in loop n;

do print_list <| hamming 20;

#------------------------------------------------------------
# Q13. 非負の整数nを受け取り、0<=x<=y<nとなるすべてのx,yの組を
#      生成する関数fooを定義せよ。
#------------------------------------------------------------
let foo n =
  for x <- 0 .. (n-1);
      y <- 0 .. (n-1);
      if x <= y;
  in (x,y);

let print_pair (x,y) =
  do print "("; print x; print ","; print y; print ")" in ();

do print_list_with print_pair <| foo 4;

#------------------------------------------------------------
# Q14. 非負の整数nを受け取り、0<x<y<z<=nの範囲でx^2+y^2=z^2
#      となるすべてのx,y,zの組を生成する関数chokkakuを定義せよ。
#------------------------------------------------------------
let chokkaku n =
  for x <- 1 .. n;
      y <- (x+1) .. n;
      z <- (y+1) .. n;
      if x**2 + y**2 == z**2;
  in (x,y,z);

let print_tuple3 (x,y,z) =
  do print "("; print x; print ","; print y; print ","; print z; print ")" in ();

do print_list_with print_tuple3 <| chokkaku 20;

#------------------------------------------------------------
# Q15. primesを素数列[2,3,5,7,11,...]のリストとして定義せよ。
#------------------------------------------------------------
fun take n lst =
  match (n,lst) {
    (n,_) ? n <= 0 -> [];
    (_,[]) -> [];
    (n,x::xs) -> x::take (n-1) xs;
  };
fun all p = {
  [] -> true;
  x::xs -> if p x -> all p xs else false;
};
fun any p = {
  [] -> false;
  x::xs -> if p x -> true else any p xs;
};
let primes n =
  let p = ref (take n [2,3,5,7]) in
  let sieve m = any {i -> m % i == 0} !p in
  fun loop n m =
    if n <= 0 -> !p
    else if sieve m -> loop n (m+2)
    else do p := !p ++ [m] in loop (n-1) (m+2)
  in loop (n-4) 11;

do print_list <| primes 20;

#------------------------------------------------------------
# Q16. 8-Queens問題を解く関数queensを定義せよ。
#      クイーンの配置はここではリストで表す。
#      例: [3,5,0,4,1,7,2,6]は次の配置を表す。
#
#           . . Q . . . . .
#           . . . . Q . . .
#           . . . . . . Q .
#           Q . . . . . . .
#           . . . Q . . . .
#           . Q . . . . . .
#           . . . . . . . Q
#           . . . . . Q . .
#
#------------------------------------------------------------
let zip xs ys =
  fun loop = {
    ([],_) -> [];
    (_,[]) -> [];
    (x::xs,y::ys) -> (x,y)::loop (xs,ys);
  }
  in loop (xs,ys);
let mapi f lst = map f <| zip (0 .. length lst) lst;
let ignore _ = ();
let queens n =
  let ans = ref [] in
  let check q qs = foldr (&&) true <| mapi {(i,x) -> x<>q && x-(i+1)<>q && x+(i+1)<>q} qs in
  fun loop qs =
    if (length qs) == n -> ans := qs::!ans
    else ignore (for q <- 0 .. (n-1); if check q qs; in loop (q::qs))
  in do loop [] in !ans;

do map print_list << take 10 <| queens 8;
