import "System.Windows.Forms.dll";

let form = new System.Windows.Forms.Form();

form.add_Click(delegate System.EventHandler (\_ -> println "aaa"));

form.ShowDialog();