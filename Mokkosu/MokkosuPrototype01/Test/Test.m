# fun fact n =
#   pat 0 = n -> 1
#   else n * fact (n - 1);

# do __prim "println" (fact 5);

# do __prim "println" ((\x y -> __prim "mul" (x, y)) 10 20);


# do println (__prim "concat" ("abc", "def"));


# do println (pat x ? x > 3 = 2 -> 123 else 456);

# fun f x = f x;
# do f 5;

# fun f x = 
# do __prim "println" (x) in
#  f x;

# do __prim "println" (f 5);


# do println (2 == 3);


# let f = ;

# do println ((\x -> x) 1);

# let f = (\x y -> y);

# do f 5 6;

# println ("abc" < "abc");

# println (__prim "lt" (1, 2));

# println (123 <> 123);

##[
do println ({ [] -> 123; x :: xs -> x } [2]);
#]
println "hello";
