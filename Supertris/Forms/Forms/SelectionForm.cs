using Supertris.Forms;

namespace Supertris
{
    /// <summary>
    /// Menu principale per scegliere modalità di gioco.
    /// 
    /// FUNZIONAMENTO:
    /// - 4 pulsanti: PVP, PVE, EvE, Training
    /// - Pulsante toggle per tipo bot (Albero/Algoritmico)
    /// - Crea e apre GameForm con parametri corretti
    /// - Per EvE: apre EvESetupDialog prima di GameForm
    /// - Si nasconde quando si apre GameForm
    /// - Viene mostrato quando GameForm si chiude
    /// 
    /// VARIABILI: modalità (0/1/2), tipo bot (1/2)
    /// </summary>

    public partial class SelectionForm : Form
    {
        private static int PLAYERvsPLAYERmod = 0;
        private static int PLAYERvsBOTmod = 1;
        private static int BOTvsBOTmod = 2;
        private static int BOTmod = 1;  // se é 1 il bot utilizzato sará albero se 2 sará algoritmico

        public SelectionForm()
        {
            InitializeComponent();
        }

        private void btnPlayerPlayer_Click(object sender, EventArgs e)
        {
            GameForm gf = new GameForm(PLAYERvsPLAYERmod, this, 0);
            gf.Show();
            this.Hide();
        }

        private void btnPlayerBot_Click(object sender, EventArgs e)
        {
            GameForm gf = new GameForm(PLAYERvsBOTmod, this, BOTmod);
            gf.Show();
            this.Hide();
        }

        private void btnBotBot_Click(object sender, EventArgs e)
        {
            // Apri dialog per scegliere se essere Player 1 o Player 2
            EvESetupDialog dialog = new EvESetupDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                bool sonoPlayer1 = dialog.SonoGiocatore1;
                GameForm gf = new GameForm(BOTvsBOTmod, this, BOTmod, sonoPlayer1);
                gf.Show();
                this.Hide();
            }
        }

        private void BtnApriTraining_Click(object sender, EventArgs e)
        {
            TrainingForm trainingForm = new TrainingForm();
            trainingForm.ShowDialog();
        }

        private void btnTipoBot_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Text == "Bot: Albero pesato")
            {
                (sender as Button).Text = "Bot: algoritmico";
                BOTmod = 2;
            }
            else if ((sender as Button).Text == "Bot: algoritmico")
            {
                (sender as Button).Text = "Bot: Albero pesato";
                BOTmod = 1;
            }
        }
    }
}