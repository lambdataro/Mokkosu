fun map = \f -> \lis ->
  pat [] = lis -> []
  else
  let pat x :: xs = lis -> f x :: map f xs
