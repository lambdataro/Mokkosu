# ラムダ計算インタプリタ


type Expr = Num(Int) | Add(Expr, Expr) | Mul(Expr, Expr);

let test = Add(Num(3), Mul(Num(4), Num(5)));

fun eval expr =
  {
     Num (x) -> x;
	 Add (e1, e2) -> eval e1 + eval e2;
	 Mul (e1, e2) -> eval e1 * eval e2
  } expr;


println (eval test);

#[
type color = red | green | blue;

fun color_num color =
  {
    red -> 123;
	blue -> 234;
	green -> 345
  } color;

println (color_num blue);
#]
