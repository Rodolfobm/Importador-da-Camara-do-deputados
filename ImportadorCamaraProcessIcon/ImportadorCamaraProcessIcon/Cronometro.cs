using System;
using System.Timers;

namespace ImportadorCamaraProcessIcon
{
    class Cronometro : Timer
    {
        public Cronometro()
        {
            this.Elapsed += new ElapsedEventHandler(CountZero);
            //A day in milliseconds
            this.Interval = 60;                               //86400000;
            this.Enabled = true;
        }
        private void CountZero(object sender, ElapsedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("CountZero at: " + e.SignalTime);
        }
    }
}