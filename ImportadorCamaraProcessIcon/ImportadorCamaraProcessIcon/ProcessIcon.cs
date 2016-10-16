using ImportadorCamaraProcessIcon.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImportadorCamaraProcessIcon
{
    class ProcessIcon : IDisposable
    {
        Cronometro contador;
        NotifyIcon ni;

        public ProcessIcon()
        {
            ni = new NotifyIcon();
        }

        public void Display()
        {
            ni.Icon = Resources.simbolo_do_congresso_nacional;
            ni.Text = "Importador da câmara dos deputados";
            ni.Visible = true;
            ni.ContextMenuStrip = new ContextMenus().Create();
            contador = new Cronometro(ni);
        }
        public void Dispose()
        {
            ni.Dispose();
        }
    }
}
