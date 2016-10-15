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
            contador = new Cronometro();
        }

        public void Display()
        {
            ni.Icon = Resources.images;
            ni.Text = "Importador da câmara dos deputados";
            ni.Visible = true;

            ni.ContextMenuStrip = new ContextMenus().Create();
        }
        public void Dispose()
        {
            ni.Dispose();
        }
    }
}
