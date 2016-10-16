using System.Timers;


namespace ImportadorCamaraProcessIcon
{
    class Cronometro : Timer
    {
        Importador importador;
        System.Windows.Forms.NotifyIcon ni;
        public Cronometro(System.Windows.Forms.NotifyIcon ni)
        {
            importador = new Importador();
            this.Elapsed += new ElapsedEventHandler(CountZero);
            this.Interval = 60;                               
            this.Enabled = true;
            this.ni = ni;
        }
        private void CountZero(object sender, ElapsedEventArgs e)
        {
            this.Stop();
            if (Interval == 60)
            {
                //Um dia em milisegundos
                this.Interval = 86400000;
            }
            ni.BalloonTipText = "Importando sessões e presenças";
            ni.Text = "Importando sessões e presenças";

            importador.controlaMetodo();
            
            ni.BalloonTipText = "";
            ni.Text = "Importador da câmara dos deputados";
            this.Start();
        }
    }
}