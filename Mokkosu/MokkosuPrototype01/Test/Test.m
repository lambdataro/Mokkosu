type Expr<T> =
  Value(T) | Add(Expr<T>, Expr<T>) | Mul(Expr<T>, Expr<T>) | L(List<T>)

and List<T> = Nil | Cons(T, List<T>);

do Cons(3, Nil);
do Nil;
do Cons(Add(Value(3), Mul(Value(4), Value(5))), Nil);