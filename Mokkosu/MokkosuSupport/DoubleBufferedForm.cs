using System.Windows.Forms;

namespace MokkosuSupport
{
    public class DoubleBufferedForm : Form
    {
        public DoubleBufferedForm()
        {
            this.DoubleBuffered = true;
        }
    }
}
