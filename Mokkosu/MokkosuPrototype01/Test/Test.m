import "System.Windows.Forms.dll";

let form = new System.Windows.Forms.Form();

do form.set_Text("test");

let f (x, y) =  println("aaa");

form.add_Click(delegate System.EventHandler f);

do form.ShowDialog();
