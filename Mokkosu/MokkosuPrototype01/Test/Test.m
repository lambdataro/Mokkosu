type Expr<T> =
  Value(T) | Add(Expr<T>, Expr<T>) | Mul(Expr<T>, Expr<T>) | L(List<T>)

and List<T> = Nil | Cons(T, List<T>);

do pat x = 12 -> x * x else 0;

let f = \x -> x;

fun fact = \n -> if true -> 1 else n * fact (n - 1);

fun fact = fact2
and fact2 = fact;

