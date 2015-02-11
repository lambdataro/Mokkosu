#============================================================
# Mokkosu 標準ライブラリ
#============================================================

let println x = __prim "println" (x);

let __operator_pls x y = __prim "add" (x, y);
let __operator_mns x y = __prim "sub" (x, y);
let __operator_ast x y = __prim "mul" (x, y);
let __operator_sls x y = __prim "div" (x, y);
