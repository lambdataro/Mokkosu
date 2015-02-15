fun even n =
  if n == 0 -> true
  else odd (n - 1)
and odd n =
  if n == 0 -> false
  else even (n - 1);

println (even 10);


fun fact n =
  if n == 0 -> 1
  else n * fact (n - 1);