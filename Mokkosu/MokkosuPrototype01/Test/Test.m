import "System.Windows.Forms.dll";
import "System.Drawing.dll";
import "MokkosuSupport.dll";

using System;
using System.Windows.Forms;
using System.Drawing;
using MokkosuSupport;

let x = ref 0;
let y = ref 0;
let vx = ref 5;
let vy = ref 5;

let form = new DoubleBufferedForm();

let tick ((obj : {System.Object}), (e : {System.EventArgs})) =
  do x := !x + !vx in
  do y := !y + !vy in
  do if !x > 800 - 20 -> vx := ~- !vx else () in
  do if !x < 0 -> vx := ~- !vx else () in
  do if !y > 600 - 20 -> vy := ~- !vy else () in
  do if !y < 0 -> vy := ~- !vy else () in
  form.Invalidate();

let timer = new Timer();

do timer.add_Tick(delegate EventHandler tick);
do timer.set_Interval(33);

let debug x = let x = x in x;

let paint ((obj : {System.Object}), (e : {PaintEventArgs})) =
  let g = e.get_Graphics() in
  let v = !x in
  let brush = call Brushes::get_Blue() in
  g.FillRectangle(brush, !x, !y, 20, 20);
  


do form.add_Paint(delegate PaintEventHandler paint);

let hide = sget SizeGripStyle::Hide;
do form.set_ClientSize(new Size(800, 600));
do form.set_SizeGripStyle(hide);

do timer.Start();

do form.ShowDialog();
