using System;
using System.Windows.Forms;

namespace ImportadorCamaraProcessIcon
{
    class ContextMenus
    {
        public ContextMenuStrip Create()
            {
            // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;

            // Exit.
            item = new ToolStripMenuItem();
            item.Text = "Sair";
            item.Click += new System.EventHandler(Exit_Click);
            menu.Items.Add(item);

            return menu;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
    }
}