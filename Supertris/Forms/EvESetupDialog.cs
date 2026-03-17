namespace Supertris.Forms
{
    /// <summary>
    /// Dialog per scegliere ruolo in modalità EvE.
    /// 
    /// FUNZIONAMENTO:
    /// - 2 pulsanti: Giocatore 1 (X) o Giocatore 2 (O)
    /// - Giocatore 1: fa prima mossa
    /// - Giocatore 2: aspetta mossa avversario
    /// - Imposta SonoGiocatore1 (bool)
    /// - Ritorna DialogResult.OK se scelto
    /// 
    /// IMPORTANTE: determina chi inizia la partita
    /// </summary>
    
    public partial class EvESetupDialog : Form
    {
        public bool SonoGiocatore1 { get; private set; }

        // Colori del tema
        public readonly Color coloreSfondo = Color.FromArgb(30, 30, 30);
        public readonly Color coloreTrisNormale = Color.FromArgb(45, 45, 48);
        public readonly Color coloreTrisAttivo = Color.FromArgb(60, 100, 60);
        public readonly Color coloreTrisCompletato = Color.FromArgb(50, 50, 50);
        public readonly Color coloreHover = Color.FromArgb(70, 70, 73);
        public readonly Color coloreX = Color.FromArgb(220, 80, 80);      // Rosso
        public readonly Color coloreO = Color.FromArgb(80, 150, 220);     // Blu
        public readonly Color coloreTesto = Color.White;
        public readonly Color coloreBordo = Color.FromArgb(80, 80, 80);

        public EvESetupDialog()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Setup Modalità Bot vs Bot";
            this.Size = new Size(500, 300);
            this.BackColor = coloreSfondo;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Titolo
            Label lblTitolo = new Label
            {
                Text = "🤖 Modalità Bot vs Bot (EvE)",
                Location = new Point(20, 20),
                Size = new Size(460, 40),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = coloreTesto,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTitolo);

            // Descrizione
            Label lblDescrizione = new Label
            {
                Text = "Scegli quale bot sei in questa partita.\n" +
                       "Il Giocatore 1 (X) fa la prima mossa.\n" +
                       "Il Giocatore 2 (O) aspetta la mossa dell'avversario.\n\n" +
                       "⚠️ Assicurati che l'altra istanza del gioco sia già aperta!",
                Location = new Point(20, 70),
                Size = new Size(460, 100),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopCenter
            };
            Controls.Add(lblDescrizione);

            // Pulsante Giocatore 1
            Button btnPlayer1 = new Button
            {
                Text = "🎯 Giocatore 1 (X)\nFaccio la prima mossa",
                Location = new Point(50, 180),
                Size = new Size(180, 60),
                BackColor = coloreX,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlayer1.FlatAppearance.BorderSize = 0;
            btnPlayer1.Click += (s, e) =>
            {
                SonoGiocatore1 = true;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnPlayer1);

            // Pulsante Giocatore 2
            Button btnPlayer2 = new Button
            {
                Text = "⏳ Giocatore 2 (O)\nAspetto la prima mossa",
                Location = new Point(270, 180),
                Size = new Size(180, 60),
                BackColor = coloreO,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlayer2.FlatAppearance.BorderSize = 0;
            btnPlayer2.Click += (s, e) =>
            {
                SonoGiocatore1 = false;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnPlayer2);
        }
    }
}