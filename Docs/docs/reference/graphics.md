# グラフィックスライブラリ

## MouseButton
```
type MouseButton =
    LeftButton
  | RightButton
  | MiddleButton
  | UnknownButton;
```

## Event
```
type Event =
    Draw({Graphics})
  | Tick
  | MouseDown(MouseButton, Int, Int)
  | MouseMove(Int, Int)
  | MouseUp(MouseButton, Int, Int);
```

## シーン
### scene関数
```
scene : String -> (Event -> ()) -> ()
```
新しいシーンを定義する。

### switch関数
```
switch : String -> ()
```
シーンを切り替える。

## タイマー
### set_speed関数
```
set_speed : Int -> ()
```
`Tick`イベントを発行する感覚をミリ秒で指定する。

## ウインドウ
### show_window関数
```
show_window : String -> ()
```
ウインドウを表示する。引数は初期シーン名。

### set_title関数
```
set_title : String -> ()
```
ウインドウのタイトルを設定する。

### set_size関数
```
set_size : Int -> Int -> ()
```
ウインドウの幅と高さを指定する。

### redraw関数
```
redraw : () -> ()
```
ウインドウの内容を再描画する。

## ペン
### new_pen関数
```
new_pen : Int -> Int -> Int -> Int -> {System.Drawing.Pen}
```
`newpen r g b w`で色`(r,g,b)`と線幅`w`のペンを作る。

### 既定のペン
```
black_pen : {System.Drawing.Pen}
white_pen : {System.Drawing.Pen}
red_pen : {System.Drawing.Pen}
green_pen : {System.Drawing.Pen}
blue_pen : {System.Drawing.Pen}
yellow_pen : {System.Drawing.Pen}
cyan_pen : {System.Drawing.Pen}
magenta_pen : {System.Drawing.Pen
```

## ブラシ
### new_solid_brush
```
new_solid_brush : Int -> Int -> Int -> {System.Drawing.Brush}
```
`new_solid_brush r g b`で色`(r,g,b)`のブラシを作る。

### 既定のブラシ
```
black_brush : {System.Drawing.Brush}
white_brush : {System.Drawing.Brush}
red_brush : {System.Drawing.Brush}
green_brush : {System.Drawing.Brush}
blue_brush : {System.Drawing.Brush}
yellow_brush : {System.Drawing.Brush}
cyan_brush : {System.Drawing.Brush}
magenta_brush : {System.Drawing.Brush}
```

## 描画関数
### draw_line関数
```
draw_line : {System.Drawing.Graphics} -> {System.Drawing.Pen} ->
    Int -> Int -> Int -> Int -> ()
```
`draw_line graphics pen x1 y1 x2 y2`で指定した座標間に線を引く。

### draw_rectangle関数
```
draw_rectangle : {System.Drawing.Graphics} -> {System.Drawing.Pen} ->
    Int -> Int -> Int -> Int -> ()
```
`draw_rectangle graphics pen x y w h`で指定した大きさの四角形を、
指定した座標に描く。

### draw_ellipse関数
```
draw_ellipse : {System.Drawing.Graphics} -> {System.Drawing.Pen} ->
    Int -> Int -> Int -> Int -> ()
```
`draw_ellipse graphics pen x y w h`で指定した大きさの四角形に
内接する楕円を描く。

### draw_string関数
```
draw_string : {System.Drawing.Graphics} -> {System.Drawing.Brush} ->
    α -> Int -> Int -> Int -> ()
```
`draw_string graphics brush string size x y `で文字列を描く。

### fill_rectangle関数
```
fill_rectangle : {System.Drawing.Graphics} -> {System.Drawing.Brush} ->
    Int -> Int -> Int -> Int -> ()
```
`fill_rectangle graphics brush x y w h`で塗りつぶした四角形を描く。

### fill_ellipse関数
```
fill_ellipse : {System.Drawing.Graphics} -> {System.Drawing.Brush} ->
    Int -> Int -> Int -> Int -> ()
```
`fill_ellipse graphics brush x y w h`で塗りつぶした楕円を描く。

### draw_pixel関数
```
draw_pixel : {System.Drawing.Graphics} ->
   Int -> Int -> Int -> Int -> Int -> ()
```
`draw_pixel graphics r h b x y`で指定した座標に指定した色のピクセルを打つ。

## 色の変換
### hsv_to_rgb関数
```
hsv_to_rgb : Double -> Double -> Double -> (Int, Int, Int)
```
色相(0.0-360.0)、彩度(0.0-1.0)、明度(0.0-1.0)をRGB値に変換する。
