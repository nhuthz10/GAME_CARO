using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro
{
    public partial class frmPvsC : Form
    {

        public delegate void delPassData1(TextBox text);
        public frmPlayGame PlayGame;

        public frmPvsC()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
                if (txbPlayerName.Text == "")
                {
                    errNull.SetError(txbPlayerName, "You have not entered a player name yet!!");
                }
                else
                {
                    delPassData1 player1 = new delPassData1(PlayGame.funData1);
                    player1(this.txbPlayerName);

                    this.Close();
                    PlayGame.Show();
                    txbPlayerName.Text = "";
                } 
        }

        private void frmPvsC_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            PlayGame.Show();
        }

        private void txbPlayerName_TextChanged(object sender, EventArgs e)
        {
            errNull.SetError(txbPlayerName, "");
        }

    }
}
