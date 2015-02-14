
import "System.Windows.Forms.dll";
import "System.Drawing.dll";

let form = new System.Windows.Forms.Form();

let hide = sget System.Windows.Forms.SizeGripStyle::Hide;

do form.set_ClientSize(new System.Drawing.Size(800, 600));
do form.set_SizeGripStyle(hide);

do form.ShowDialog();
