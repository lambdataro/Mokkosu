#============================================================
# Mokkosu 標準ライブラリ
#============================================================

let println x = __prim "println" (x);
let print x = __prim "print" (x);
let tostring x = __prim "tostring" (x);
let error x = __prim "error" (x);

let __operator_ltbar x f = f x;
let __operator_bargt f x = f x;

let __operator_eqeq x y = __prim "eq" (x, y);
let __operator_ltgt x y = __prim "ne" (x, y);
let __operator_lt x y = __prim "lt" (x, y);
let __operator_gt x y = __prim "gt" (x, y);
let __operator_le x y = __prim "le" (x, y);
let __operator_ge x y = __prim "ge" (x, y);

let __operator_pls x y = __prim "add" (x, y);
let __operator_mns x y = __prim "sub" (x, y);
let __operator_ast x y = __prim "mul" (x, y);
let __operator_sls x y = __prim "div" (x, y);

