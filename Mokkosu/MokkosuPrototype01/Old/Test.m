data Bool = True | False;

let f = \x -> x in
let x = True in
print (f f 10)