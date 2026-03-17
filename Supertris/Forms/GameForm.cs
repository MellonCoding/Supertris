using Supertris.Helpers;
using Supertris.Classes;
using Supertris.AI;

namespace Supertris
{
    /// <summary>
    /// Interfaccia di gioco principale con griglia 9x9.
    /// 
    /// FUNZIONAMENTO:
    /// - Crea 9 panel (mini-tris) con 9 bottoni ciascuno
    /// - Gestisce click sui bottoni e valida mosse
    /// - Coordina turni: giocatore -> bot (in PVE)
    /// - Aggiorna visualizzazione: colori panel, turno, info
    /// - In EvE: bot locale + FileWatcher per mosse remote
    /// - Feedback visivo: cursori, panel oscurati durante bot
    /// - Gestisce fine partita e vittoria
    /// 
    /// MODALITÀ: 0=PVP, 1=PVE, 2=EvE
    /// </summary>

    public partial class GameForm : Form
    {
        private GameManager gm;
        private Form FormIniziale;
        private CustomButton[,] buttons;
        private Panel[] panels;
        private Label lblTurno;
        private Label lblInfo;
        private int modalitaGioco;  // 0 = PVP, 1 = PVE, 2 = EVE
        private int tipoBot;
        private AlberoPesato botAllenato;
        private MinimaxBot botAlgoritmico;
        private bool botInPensiero;
        private ColorManager colorManager = new ColorManager();
        private string percorsoFile;
        private FileManager fileManager;

        // EvE Mode
        private FileWatcher fileWatcher;
        private bool sonoGiocatore1;
        private bool aspettoMossaAvversario;

        public GameForm(int mod, Form SelectionForm, int modBot, bool player1 = true)
        {
            InitializeComponent();

            BackColor = colorManager.coloreSfondo;
            Size = new Size(550, 620);
            StartPosition = FormStartPosition.CenterScreen;

            buttons = new CustomButton[9, 9];
            panels = new Panel[9];

            InitializeUI();

            gm = new GameManager();
            modalitaGioco = mod;
            tipoBot = modBot;
            botInPensiero = false;
            sonoGiocatore1 = player1;
            aspettoMossaAvversario = false;

            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "File mosse (*.txt)|*.txt|Tutti i file (*.*)|*.*",
                DefaultExt = "mosse"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                percorsoFile = openDialog.FileName;
                fileManager = new FileManager(percorsoFile);
                fileManager.Start();
            }

            switch (mod)
            {
                case 1:
                    if (tipoBot == 1)
                        botAllenato = new AlberoPesato(true);
                    else
                        botAlgoritmico = new MinimaxBot();
                    break;

                case 2:
                    if (tipoBot == 1)
                        botAllenato = new AlberoPesato(true);
                    else
                        botAlgoritmico = new MinimaxBot();

                    fileWatcher = new FileWatcher(percorsoFile, OnMossaAvversarioRicevuta);
                    fileWatcher.Avvia();

                    if (sonoGiocatore1)
                    {
                        lblInfo.Text = "🤖 Sei Giocatore 1 (X) - Fai la prima mossa!";
                        Task.Delay(1000).ContinueWith(_ => this.Invoke(() => EseguiMossaBot()));
                    }
                    else
                    {
                        lblInfo.Text = "🤖 Sei Giocatore 2 (O) - Aspetto mossa avversario...";
                        aspettoMossaAvversario = true;
                    }
                    break;
            }

            this.FormIniziale = SelectionForm;
            AggiornaVisualizzazione();
        }

        private void AggiornaVisualizzazione()
        {
            lblTurno.Text = $"Turno: {gm.GetTurno()}";
            lblTurno.ForeColor = gm.GetTurno() == 'X' ? colorManager.coloreX : colorManager.coloreO;

            int prossimoTris = gm.GetProssimaTrisObbligatoria();

            if (modalitaGioco != 2)
            {
                if (prossimoTris == -1)
                {
                    lblInfo.Text = "Mossa libera!";
                    lblInfo.ForeColor = Color.FromArgb(100, 200, 100);
                }
                else
                {
                    lblInfo.Text = $"Devi giocare nel tris #{prossimoTris + 1}";
                    lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (panels[i] != null)
                {
                    panels[i].BackColor = prossimoTris == -1 ? colorManager.coloreTrisNormale
                                        : i == prossimoTris ? colorManager.coloreTrisAttivo
                                                              : colorManager.coloreTrisCompletato;
                }
            }
        }

        private void Mossa(object? sender, EventArgs e)
        {
            if (botInPensiero)
            {
                if (modalitaGioco == 1)
                {
                    lblInfo.Text = "⏳ Aspetta che il bot finisca di pensare!";
                    lblInfo.ForeColor = Color.FromArgb(255, 100, 100);

                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        this.Invoke(() =>
                        {
                            if (botInPensiero)
                            {
                                lblInfo.Text = "🤖 Il bot sta pensando...";
                                lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
                            }
                        });
                    });
                }
                return;
            }

            if (modalitaGioco == 2) return;

            if (sender is not CustomButton btn)
            {
                MessageBox.Show("Errore interno del gioco. Riavvia l'applicazione.");
                return;
            }

            if (btn.Text != "")
            {
                lblInfo.Text = "❌ Cella già occupata!";
                lblInfo.ForeColor = Color.FromArgb(255, 100, 100);
                return;
            }

            string tag = btn.Tag?.ToString() ?? "";
            List<int> numeriTag = new List<int>();

            foreach (char c in tag)
            {
                if (char.IsDigit(c))
                    numeriTag.Add(int.Parse(c.ToString()));
            }

            if (numeriTag.Count != 3) return;

            int numTris = numeriTag[0];
            int row = numeriTag[1];
            int col = numeriTag[2];

            if (gm.MakeMove(numTris, row, col))
            {
                char turnoAttuale = gm.GetTurno();

                btn.Text = turnoAttuale.ToString();
                btn.ForeColor = turnoAttuale == 'X' ? colorManager.coloreX : colorManager.coloreO;

                fileManager.Write($"{turnoAttuale} {numTris} {(row * 3) + col}");

                char vincitore = gm.CheckWin();
                if (vincitore != '-')
                {
                    GestioneVittoria(vincitore);
                    return;
                }

                gm.CambiaTurno();
                AggiornaVisualizzazione();

                if (DeveGiocareilBot())
                    EseguiTurnoBot();
            }
            else
            {
                lblInfo.Text = "❌ Mossa non valida! Controlla dove puoi giocare.";
                lblInfo.ForeColor = Color.FromArgb(255, 100, 100);

                Task.Run(async () =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(50);
                        btn.Invoke(() => btn.BackColor = colorManager.coloreTrisAttivo);
                        await Task.Delay(50);
                        btn.Invoke(() => btn.BackColor = colorManager.coloreTrisNormale);
                    }
                });
            }
        }

        private bool DeveGiocareilBot()
            => modalitaGioco == 1 && gm.GetTurno() == 'O';

        private async void EseguiTurnoBot()
        {
            botInPensiero = true;

            if (modalitaGioco == 1)
            {
                lblInfo.Text = "🤖 Il bot sta pensando...";
                lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
                CambiaCursoreBottoni(Cursors.No);
                DimmaPanels(true);
            }

            await Task.Delay(300);

            string boardState = gm.GetBoardState();
            int trisObb = gm.GetProssimaTrisObbligatoria();
            char turnoBot = gm.GetTurno();

            var mossa = tipoBot == 1
                ? botAllenato?.CalcolaMossa(boardState, trisObb, turnoBot)
                : botAlgoritmico?.CalcolaMossa(boardState, trisObb, turnoBot);

            if (mossa.HasValue)
            {
                int numTris = mossa.Value.numTris;
                int row = mossa.Value.row;
                int col = mossa.Value.col;

                if (gm.MakeMove(numTris, row, col))
                {
                    int buttonIndex = row * 3 + col;
                    if (buttons[numTris, buttonIndex] != null)
                    {
                        var btn = buttons[numTris, buttonIndex];
                        btn.Text = turnoBot.ToString();
                        btn.ForeColor = turnoBot == 'X' ? colorManager.coloreX : colorManager.coloreO;
                    }

                    fileManager.Write($"{turnoBot} {numTris} {(row * 3) + col}");

                    char vincitore = gm.CheckWin();
                    if (vincitore != '-')
                    {
                        GestioneVittoria(vincitore);
                        botInPensiero = false;
                        return;
                    }

                    gm.CambiaTurno();
                    AggiornaVisualizzazione();
                }
            }

            botInPensiero = false;

            if (modalitaGioco == 1)
            {
                CambiaCursoreBottoni(Cursors.Hand);
                DimmaPanels(false);
            }
        }

        private void CambiaCursoreBottoni(Cursor cursore)
        {
            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;
                foreach (Control ctrl in panels[i].Controls)
                {
                    if (ctrl is CustomButton btn && btn.Text == "")
                        btn.Cursor = cursore;
                }
            }
        }

        private void DimmaPanels(bool dimma)
        {
            int prossimoTris = gm.GetProssimaTrisObbligatoria();

            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;

                panels[i].BackColor = dimma
                    ? colorManager.coloreTrisCompletato
                    : prossimoTris == -1 ? colorManager.coloreTrisNormale
                    : i == prossimoTris ? colorManager.coloreTrisAttivo
                                         : colorManager.coloreTrisCompletato;
            }
        }

        // ==================== MODALITÀ EVE ==================== //

        private void EseguiMossaBot()
        {
            if (aspettoMossaAvversario) return;

            botInPensiero = true;
            lblInfo.Text = "🤖 Il mio bot sta pensando...";

            string boardState = gm.GetBoardState();
            int trisObb = gm.GetProssimaTrisObbligatoria();
            char turno = gm.GetTurno();

            var mossa = tipoBot == 1
                ? botAllenato?.CalcolaMossa(boardState, trisObb, turno)
                : botAlgoritmico?.CalcolaMossa(boardState, trisObb, turno);

            if (mossa.HasValue)
            {
                int numTris = mossa.Value.numTris;
                int row = mossa.Value.row;
                int col = mossa.Value.col;

                if (gm.MakeMove(numTris, row, col))
                {
                    int buttonIndex = row * 3 + col;
                    if (buttons[numTris, buttonIndex] != null)
                    {
                        var btn = buttons[numTris, buttonIndex];
                        btn.Text = turno.ToString();
                        btn.ForeColor = turno == 'X' ? colorManager.coloreX : colorManager.coloreO;
                    }

                    string mossaStr = $"{turno} {numTris} {(row * 3) + col}";
                    fileManager.Write(mossaStr);
                    fileWatcher.AggiornaUltimaRiga(mossaStr);

                    char vincitore = gm.CheckWin();
                    if (vincitore != '-')
                    {
                        GestioneVittoria(vincitore);
                        botInPensiero = false;
                        return;
                    }

                    gm.CambiaTurno();
                    AggiornaVisualizzazione();

                    lblInfo.Text = "⏳ Aspetto mossa avversario...";
                    aspettoMossaAvversario = true;
                }
            }
            else
            {
                MessageBox.Show(
                    $"⚠️ ERRORE: Il bot non ha trovato una mossa valida!\n\nTurno: {turno}\nTris obbligatoria: {trisObb}",
                    "Errore Bot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            botInPensiero = false;
        }

        private void OnMossaAvversarioRicevuta(string rigaMossa)
        {
            if (!aspettoMossaAvversario) return;

            this.Invoke(() =>
            {
                aspettoMossaAvversario = false;

                string[] parti = rigaMossa.Split(' ');
                if (parti.Length != 3)
                {
                    MessageBox.Show("❌ Formato mossa avversario non valido!");
                    return;
                }

                char turnoAvversario = parti[0][0];
                int numTris = int.Parse(parti[1]);
                int posizione = int.Parse(parti[2]);
                int row = posizione / 3;
                int col = posizione % 3;

                lblInfo.Text = $"📥 Ricevuta mossa avversario: Tris {numTris}, Pos {posizione}";

                if (gm.MakeMove(numTris, row, col))
                {
                    int buttonIndex = row * 3 + col;
                    if (buttons[numTris, buttonIndex] != null)
                    {
                        var btn = buttons[numTris, buttonIndex];
                        btn.Text = turnoAvversario.ToString();
                        btn.ForeColor = turnoAvversario == 'X' ? colorManager.coloreX : colorManager.coloreO;
                    }

                    char vincitore = gm.CheckWin();
                    if (vincitore != '-')
                    {
                        GestioneVittoria(vincitore);
                        return;
                    }

                    gm.CambiaTurno();
                    AggiornaVisualizzazione();

                    Task.Delay(500).ContinueWith(_ => this.Invoke(() => EseguiMossaBot()));
                }
                else
                {
                    MessageBox.Show(
                        $"❌ MOSSA INVALIDA DELL'AVVERSARIO!\n\nTris: {numTris}\nRiga: {row}\nColonna: {col}",
                        "Errore Avversario", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void GestioneVittoria(char vincitore)
        {
            Color coloreVincitore = vincitore == 'X' ? colorManager.coloreX : colorManager.coloreO;

            lblInfo.Text = $"🎉 {vincitore} ha vinto! 🎉";
            lblInfo.ForeColor = coloreVincitore;
            lblInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            MessageBox.Show(
                $"Complimenti! {vincitore} ha vinto la partita!",
                "Fine Partita", MessageBoxButtons.OK, MessageBoxIcon.Information);

            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;
                foreach (Control btnCtrl in panels[i].Controls)
                {
                    if (btnCtrl is CustomButton b)
                        b.Cursor = Cursors.No;
                }
            }

            if (modalitaGioco == 2)
                fileWatcher?.Ferma();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileWatcher?.Ferma();
            FormIniziale.Show();
        }

        internal void InitializeUI()
        {
            const int BUTTON_SIZE = 50;
            const int BUTTON_MARGIN = 2;
            const int TRIS_SPACING = 10;
            const int START_X = 30;
            const int START_Y = 80;

            lblInfo = new Label
            {
                Location = new Point(START_X + 100, 25),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent,
                Text = "Mossa libera - Gioca dove vuoi!"
            };
            Controls.Add(lblInfo);

            lblTurno = new Label
            {
                Location = new Point(START_X, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = colorManager.coloreTesto,
                BackColor = Color.Transparent,
                Text = "Turno: X"
            };
            Controls.Add(lblTurno);

            for (int numTris = 0; numTris < 9; numTris++)
            {
                int trisRow = numTris / 3;
                int trisCol = numTris % 3;

                Panel trisPanel = new Panel
                {
                    Location = new Point(
                        START_X + trisCol * (BUTTON_SIZE * 3 + BUTTON_MARGIN * 2 + TRIS_SPACING),
                        START_Y + trisRow * (BUTTON_SIZE * 3 + BUTTON_MARGIN * 2 + TRIS_SPACING)
                    ),
                    Size = new Size(BUTTON_SIZE * 3 + BUTTON_MARGIN * 2, BUTTON_SIZE * 3 + BUTTON_MARGIN * 2),
                    BackColor = colorManager.coloreTrisNormale,
                    Tag = $"Panel{numTris}"
                };
                Controls.Add(trisPanel);
                panels[numTris] = trisPanel;

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        var btn = new CustomButton
                        {
                            Text = "",
                            Location = new Point(col * (BUTTON_SIZE + BUTTON_MARGIN), row * (BUTTON_SIZE + BUTTON_MARGIN)),
                            Size = new Size(BUTTON_SIZE, BUTTON_SIZE),
                            Tag = $"Tris{numTris}Row{row}Col{col}",
                            Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        };

                        btn.Click += Mossa;
                        trisPanel.Controls.Add(btn);

                        buttons[numTris, row * 3 + col] = btn;
                    }
                }
            }
        }
    }
}