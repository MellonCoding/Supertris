using Supertris.Helpers;
using Supertris.AI;

namespace Supertris.Forms
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
        private Button[,] buttons;
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
        private bool sonoGiocatore1;  // true se sono il giocatore che fa la prima mossa
        private bool aspettoMossaAvversario;

        public GameForm(int mod, Form SelectionForm, int modBot, bool player1 = true)
        {
            InitializeComponent();

            BackColor = colorManager.coloreSfondo;
            Size = new Size(550, 620);
            StartPosition = FormStartPosition.CenterScreen;

            buttons = new Button[9, 9];
            panels = new Panel[9];
            
            InitializeUI();

            gm = new GameManager();
            modalitaGioco = mod;
            tipoBot = modBot;
            botInPensiero = false;
            sonoGiocatore1 = player1;
            aspettoMossaAvversario = false;

            // imposta il file mosse.txt
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
                case 1: // PVE - Player vs Bot
                    if (tipoBot == 1)
                    {
                        botAllenato = new AlberoPesato(true);
                    }
                    else
                    {
                        botAlgoritmico = new MinimaxBot();
                    }
                    break;

                case 2: // EVE - Bot vs Bot (networked)
                    // Inizializza il bot locale
                    if (tipoBot == 1)
                    {
                        botAllenato = new AlberoPesato(true);
                    }
                    else
                    {
                        botAlgoritmico = new MinimaxBot();
                    }

                    fileWatcher = new FileWatcher(percorsoFile, OnMossaAvversarioRicevuta);
                    fileWatcher.Avvia();

                    // Se sono il giocatore 1, faccio la prima mossa
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

            if (modalitaGioco != 2) // Non in modalità EvE
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
                    if (prossimoTris == -1)
                    {
                        panels[i].BackColor = colorManager.coloreTrisNormale;
                    }
                    else if (i == prossimoTris)
                    {
                        panels[i].BackColor = colorManager.coloreTrisAttivo;
                    }
                    else
                    {
                        panels[i].BackColor = colorManager.coloreTrisCompletato;
                    }
                }
            }
        }

        private void Mossa(object? sender, EventArgs e)
        {
            // Blocca i click durante il turno del bot con feedback visivo
            if (botInPensiero)
            {
                if (modalitaGioco == 1) // Solo in PVE mostra feedback
                {
                    lblInfo.Text = "⏳ Aspetta che il bot finisca di pensare!";
                    lblInfo.ForeColor = Color.FromArgb(255, 100, 100);

                    // Animazione rapida di "shake"
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        this.Invoke(() =>
                        {
                            if (botInPensiero) // Se ancora in pensiero
                            {
                                lblInfo.Text = "🤖 Il bot sta pensando...";
                                lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
                            }
                        });
                    });
                }
                return;
            }

            if (modalitaGioco == 2) return; // In EvE i click sono disabilitati

            if (sender is not Button btn)
            {
                MessageBox.Show("Errore interno del gioco. Riavvia l'applicazione.");
                return;
            }

            // 🔥 FIX PRINCIPALE: Ignora click su celle già giocate
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
                {
                    numeriTag.Add(int.Parse(c.ToString()));
                }
            }

            if (numeriTag.Count != 3) return;

            int numTris = numeriTag[0];
            int row = numeriTag[1];
            int col = numeriTag[2];

            if (gm.MakeMove(numTris, row, col))
            {
                char turnoAttuale = gm.GetTurno();
                
                btn.Text = turnoAttuale.ToString();
                btn.BackColor = Color.FromArgb(40, 40, 43);
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
                {
                    EseguiTurnoBot();
                }
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
                        btn.Invoke(() => btn.BackColor = Color.FromArgb(150, 50, 50));
                        await Task.Delay(50);
                        btn.Invoke(() => btn.BackColor = Color.FromArgb(55, 55, 58));
                    }
                });
            }
        }

        private bool DeveGiocareilBot()
        {
            if (modalitaGioco == 1 && gm.GetTurno() == 'O')
                return true;

            return false;
        }

        private async void EseguiTurnoBot()
        {
            botInPensiero = true;

            // Mostra feedback visivo che il bot sta pensando
            if (modalitaGioco == 1) // Solo in PVE
            {
                lblInfo.Text = "🤖 Il bot sta pensando...";
                lblInfo.ForeColor = Color.FromArgb(255, 200, 100);

                // Cambia il cursore per tutti i bottoni
                CambiaCursoreBottoni(Cursors.No);

                // Oscura leggermente i panel per indicare che sono bloccati
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
                        Button btn = buttons[numTris, buttonIndex];

                        // 🔥 SOLUZIONE: Stesso ordine
                        btn.Text = turnoBot.ToString();
                        btn.BackColor = Color.FromArgb(40, 40, 43);
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

            // Ripristina il cursore normale e i colori
            if (modalitaGioco == 1)
            {
                CambiaCursoreBottoni(Cursors.Hand);
                DimmaPanels(false);
            }
        }

        /// <summary>
        /// Cambia il cursore di tutti i bottoni attivi
        /// </summary>
        private void CambiaCursoreBottoni(Cursor cursore)
        {
            for (int i = 0; i < 9; i++)
            {
                if (panels[i] != null)
                {
                    foreach (Control ctrl in panels[i].Controls)
                    {
                        if (ctrl is Button btn && btn.Text == "") // Solo bottoni vuoti
                        {
                            btn.Cursor = cursore;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Oscura o ripristina i panel per feedback visivo
        /// </summary>
        private void DimmaPanels(bool dimma)
        {
            int prossimoTris = gm.GetProssimaTrisObbligatoria();

            for (int i = 0; i < 9; i++)
            {
                if (panels[i] != null)
                {
                    if (dimma)
                    {
                        // Oscura leggermente tutti i panel
                        panels[i].BackColor = Color.FromArgb(35, 35, 38);
                    }
                    else
                    {
                        // Ripristina i colori normali in base allo stato
                        if (prossimoTris == -1)
                        {
                            panels[i].BackColor = colorManager.coloreTrisNormale;
                        }
                        else if (i == prossimoTris)
                        {
                            panels[i].BackColor = colorManager.coloreTrisAttivo;
                        }
                        else
                        {
                            panels[i].BackColor = colorManager.coloreTrisCompletato;
                        }
                    }
                }
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
                    // Aggiorna UI
                    int buttonIndex = row * 3 + col;
                    if (buttons[numTris, buttonIndex] != null)
                    {
                        Button btn = buttons[numTris, buttonIndex];
                        
                        // 🔥 SOLUZIONE: Stesso ordine
                        btn.Text = turno.ToString();
                        btn.BackColor = Color.FromArgb(40, 40, 43);
                        btn.ForeColor = turno == 'X' ? colorManager.coloreX : colorManager.coloreO;
                    }

                    // Scrivi la mossa su file
                    string mossaStr = $"{turno} {numTris} {(row * 3) + col}";
                    fileManager.Write(mossaStr);
                    fileWatcher.AggiornaUltimaRiga(mossaStr);

                    // Controlla vittoria
                    char vincitore = gm.CheckWin();
                    if (vincitore != '-')
                    {
                        GestioneVittoria(vincitore);
                        botInPensiero = false;
                        return;
                    }

                    gm.CambiaTurno();
                    AggiornaVisualizzazione();

                    // Aspetta la mossa dell'avversario
                    lblInfo.Text = "⏳ Aspetto mossa avversario...";
                    aspettoMossaAvversario = true;
                }
            }
            else
            {
                // Il bot non ha trovato una mossa valida - ERROR
                MessageBox.Show(
                    $"⚠️ ERRORE: Il bot non ha trovato una mossa valida!\n\n" +
                    $"Turno: {turno}\n" +
                    $"Tris obbligatoria: {trisObb}\n" +
                    $"BoardState length: {boardState.Length}",
                    "Errore Bot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            botInPensiero = false;
        }

        private void OnMossaAvversarioRicevuta(string rigaMossa)
        {
            if (!aspettoMossaAvversario) return;

            this.Invoke(() =>
            {
                aspettoMossaAvversario = false;

                // Parsing della mossa: "X 4 5" -> turno='X', numTris=4, posizione=5
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

                // Esegui la mossa
                if (gm.MakeMove(numTris, row, col))
                {
                    // Aggiorna UI
                    int buttonIndex = row * 3 + col;
                    if (buttons[numTris, buttonIndex] != null)
                    {
                        Button btn = buttons[numTris, buttonIndex];
                        
                        // 🔥 SOLUZIONE: Stesso ordine
                        btn.Text = turnoAvversario.ToString();
                        btn.BackColor = Color.FromArgb(40, 40, 43);
                        btn.ForeColor = turnoAvversario == 'X' ? colorManager.coloreX : colorManager.coloreO;
                    }

                    // Controlla vittoria
                    char vincitore = gm.CheckWin();
                    if (vincitore != '-')
                    {
                        GestioneVittoria(vincitore);
                        return;
                    }

                    gm.CambiaTurno();
                    AggiornaVisualizzazione();

                    // Fai la mia mossa
                    Task.Delay(500).ContinueWith(_ => this.Invoke(() => EseguiMossaBot()));
                }
                else
                {
                    MessageBox.Show(
                        $"❌ MOSSA INVALIDA DELL'AVVERSARIO!\n\nTris: {numTris}\nRiga: {row}\nColonna: {col}",
                        "Errore Avversario",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            });
        }

        private void GestioneVittoria(char vincitore)
        {
            string nomeVincitore = vincitore.ToString();
            Color coloreVincitore = vincitore == 'X' ? colorManager.coloreX : colorManager.coloreO;

            lblInfo.Text = $"🎉 {nomeVincitore} ha vinto! 🎉";
            lblInfo.ForeColor = coloreVincitore;
            lblInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            MessageBox.Show(
                $"Complimenti! {nomeVincitore} ha vinto la partita!",
                "Fine Partita",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 🔥 FIX: Cambia solo il cursore, non disabilitare
            for (int i = 0; i < 9; i++)
            {
                if (panels[i] != null)
                {
                    foreach (Control btnCtrl in panels[i].Controls)
                    {
                        if (btnCtrl is Button b)
                            b.Cursor = Cursors.No;
                    }
                }
            }

            // Ferma il file watcher se in modalità EvE
            if (modalitaGioco == 2)
            {
                fileWatcher?.Ferma();
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ferma il file watcher
            if (fileWatcher != null)
            {
                fileWatcher.Ferma();
            }

            FormIniziale.Show();
        }

        internal void InitializeUI()
        {
            const int BUTTON_SIZE = 50;
            const int BUTTON_MARGIN = 2;
            const int TRIS_SPACING = 10;
            const int START_X = 30;
            const int START_Y = 80;

            // Label per info mossa
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

            // Label per il turno
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

            // Creazione griglia di bottoni
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
                        Button btn = new Button
                        {
                            Text = "",
                            Location = new Point(
                                col * (BUTTON_SIZE + BUTTON_MARGIN),
                                row * (BUTTON_SIZE + BUTTON_MARGIN)
                            ),
                            Size = new Size(BUTTON_SIZE, BUTTON_SIZE),
                            Tag = $"Tris{numTris}Row{row}Col{col}",
                            FlatStyle = FlatStyle.Flat,
                            BackColor = Color.FromArgb(55, 55, 58),
                            ForeColor = colorManager.coloreTesto,
                            Font = new Font("Segoe UI", 16, FontStyle.Bold),
                            Cursor = Cursors.Hand
                        };

                        btn.FlatAppearance.BorderSize = 1;
                        btn.FlatAppearance.BorderColor = colorManager.coloreBordo;

                        btn.MouseEnter += (s, e) =>
                        {
                            if (btn.Text == "")
                                btn.BackColor = colorManager.coloreHover;
                        };

                        btn.MouseLeave += (s, e) =>
                        {
                            if (btn.Text == "")
                                btn.BackColor = Color.FromArgb(55, 55, 58);
                            else
                                btn.BackColor = Color.FromArgb(40, 40, 43);
                        };

                        btn.Click += Mossa;
                        trisPanel.Controls.Add(btn);

                        int buttonIndex = row * 3 + col;
                        buttons[numTris, buttonIndex] = btn;
                    }
                }
            }
        }
    }
}
