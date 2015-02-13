type List<T> = Nil | Cons(T, List<T>);

let test1 = Cons(2, Cons(3, Nil));
let test2 = Cons("a", Cons("b", Nil));

##[
fun map f lis =
  lis |> {
    ~Nil -> Nil;
	~Cons(x, xs) -> Cons(f x, map f xs);
  };
#]

fun print_list lis =
  lis |> {
    ~Nil -> ();
	~Cons(x, xs) -> do println x in
	                print_list xs;
  };

print_list (map {x -> x * 2} test1);
print_list (map {x -> x ^ "bbb"} test2);

println "abcdefg".Substring(1,4);


let f x y z = x + y + z;

println ((3 `f` 4) 5);

let not x = { true -> false; false -> true } x;

println (not true);