using Supertris.Helpers;

namespace Supertris.Forms
{
    /// <summary>
    /// Menu principale per scegliere modalità di gioco.
    ///
    /// FUNZIONAMENTO:
    /// - 4 pulsanti: PVP, PVE, EvE, Training
    /// - Toggle tipo bot (Albero / Algoritmico)
    /// - Toggle modalità tris pieno (Nullo / Flush)
    /// - Crea e apre GameForm con parametri corretti
    /// - Per EvE: apre EvESetupDialog prima di GameForm
    /// </summary>

    public partial class SelectionForm : Form
    {
        private static int PLAYERvsPLAYERmod = 0;
        private static int PLAYERvsBOTmod = 1;
        private static int BOTvsBOTmod = 2;
        private static int BOTmod = 1;  // 1=AlberoPesato, 2=Minimax

        private static ModalitaTrisPieno modalitaTris = ModalitaTrisPieno.Nullo;

        public SelectionForm()
        {
            InitializeComponent();
        }

        private void btnPlayerPlayer_Click(object sender, EventArgs e)
        {
            var gf = new GameForm(PLAYERvsPLAYERmod, this, 0, modalitaTris);
            gf.Show();
            this.Hide();
        }

        private void btnPlayerBot_Click(object sender, EventArgs e)
        {
            var gf = new GameForm(PLAYERvsBOTmod, this, BOTmod, modalitaTris);
            gf.Show();
            this.Hide();
        }

        private void btnBotBot_Click(object sender, EventArgs e)
        {
            var dialog = new EvESetupDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var gf = new GameForm(BOTvsBOTmod, this, BOTmod, modalitaTris, dialog.SonoGiocatore1);
                gf.Show();
                this.Hide();
            }
        }

        private void BtnApriTraining_Click(object sender, EventArgs e)
        {
            var trainingForm = new TrainingForm();
            trainingForm.ShowDialog();
        }

        private void btnTipoBot_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            if (BOTmod == 1)
            {
                BOTmod = 2;
                btn.Text = "Bot: Algoritmico";
            }
            else
            {
                BOTmod = 1;
                btn.Text = "Bot: Albero pesato";
            }
        }

        private void btnModalitaTris_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            if (modalitaTris == ModalitaTrisPieno.Nullo)
            {
                modalitaTris = ModalitaTrisPieno.Flush;
                btn.Text = "Tris pieno: Flush";
            }
            else
            {
                modalitaTris = ModalitaTrisPieno.Nullo;
                btn.Text = "Tris pieno: Nullo";
            }
        }
    }
}