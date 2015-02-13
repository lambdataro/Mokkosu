type Expr = Num(Int) | Add (Expr, Expr);

#[
fun eval e =
  e |> {
    ~Num(n) -> n;
    ~Add(e1, e2) -> eval e1 + eval e2;
  };
#]

let test = Add (Num 3, Num 4);

println <| eval test;