type Expr<T> =
  Value(T) | Add(Expr<T>, Expr<T>) | Mul(Expr<T>, Expr<T>) | L(List<T>)

and List<T> = Nil | Cons(T, List<T>);

do pat x = 12 -> x * x else 0;