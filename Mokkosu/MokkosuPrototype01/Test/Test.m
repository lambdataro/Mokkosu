

# do __prim "println" ((\x y -> __prim "mul" (x, y)) 10 20);


# fun f x = f x;
# do f 5;

# fun f x = 
# do __prim "println" (x) in
#  f x;

# do __prim "println" (f 5);

fun fact n =
  pat 0 = n -> 1
  else n * fact (n - 1);

println (fact 5);