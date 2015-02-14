
import "System.Windows.Forms.dll";

let form = new System.Windows.Forms.Form();

let hide = sget System.Windows.Forms.SizeGripStyle::Hide;

do form.set_SizeGripStyle(hide);

do form.ShowDialog();
