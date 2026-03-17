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
    /// - Gestisce fine partita con opzioni reset/menu
    ///
    /// MODALITÀ: 0=PVP, 1=PVE, 2=EvE
    /// </summary>

    public partial class GameForm : Form
    {
        // ── Stato gioco ───────────────────────────────────────────
        private GameManager      gm;
        private Form             FormIniziale;
        private int              modalitaGioco;   // 0=PVP, 1=PVE, 2=EvE
        private int              tipoBot;
        private bool             botInPensiero;
        private ModalitaTrisPieno modalitaTrisPieno;

        // ── Bot ───────────────────────────────────────────────────
        private AlberoPesato botAllenato;
        private MinimaxBot   botAlgoritmico;

        // ── UI ────────────────────────────────────────────────────
        private CustomButton[,] buttons;
        private Panel[]         panels;
        private Label           lblTurno;
        private Label           lblInfo;

        // ── File (EvE) ────────────────────────────────────────────
        // TODO: rimuovere fileManager e fileWatcher quando il debug non serve più
        private string      percorsoFile;
        private FileManager fileManager;
        private FileWatcher fileWatcher;
        private bool        sonoGiocatore1;
        private bool        aspettoMossaAvversario;

        // ── Flag interni ─────────────────────────────────────────
        // true quando si sta aprendo una nuova partita — impedisce
        // a FormClosing di mostrare il SelectionForm
        private bool _nuovaPartita = false;

        // ── Theme ─────────────────────────────────────────────────
        private static ThemeManager TM => ThemeManager.Instance;

        // ═════════════════════════════════════════════════════════
        // COSTRUTTORE
        // ═════════════════════════════════════════════════════════

        public GameForm(int mod, Form selectionForm, int modBot,
                        ModalitaTrisPieno modalitaTris = ModalitaTrisPieno.Nullo,
                        bool player1 = true)
        {
            InitializeComponent();

            BackColor     = TM.ColoreSfondo;
            Size          = new Size(550, 620);
            StartPosition = FormStartPosition.CenterScreen;

            buttons = new CustomButton[9, 9];
            panels  = new Panel[9];

            InitializeUI();

            modalitaGioco      = mod;
            tipoBot            = modBot;
            botInPensiero      = false;
            sonoGiocatore1     = player1;
            aspettoMossaAvversario = false;
            modalitaTrisPieno  = modalitaTris;
            FormIniziale       = selectionForm;

            gm = new GameManager
            {
                ModalitaTrisPieno = modalitaTris
            };

            // ── File mosse (solo EvE, tenuto per debug) ───────────
            // TODO: spostare o rimuovere quando il debug non serve più
            if (mod == 2)
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter     = "File mosse (*.txt)|*.txt|Tutti i file (*.*)|*.*",
                    DefaultExt = "mosse"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    percorsoFile = openDialog.FileName;
                    fileManager  = new FileManager(percorsoFile);
                    fileManager.Start();
                }
            }

            // ── Inizializzazione bot ──────────────────────────────
            switch (mod)
            {
                case 1: // PVE
                    if (tipoBot == 1) botAllenato    = new AlberoPesato(true);
                    else              botAlgoritmico = new MinimaxBot();
                    break;

                case 2: // EvE
                    if (tipoBot == 1) botAllenato    = new AlberoPesato(true);
                    else              botAlgoritmico = new MinimaxBot();

                    if (percorsoFile != null)
                    {
                        fileWatcher = new FileWatcher(percorsoFile, OnMossaAvversarioRicevuta);
                        fileWatcher.Avvia();
                    }

                    if (sonoGiocatore1)
                    {
                        lblInfo.Text = "🤖 Sei Giocatore 1 (X) - Fai la prima mossa!";
                        Task.Delay(1000).ContinueWith(_ => this.Invoke(() => EseguiMossaBot()));
                    }
                    else
                    {
                        lblInfo.Text          = "🤖 Sei Giocatore 2 (O) - Aspetto mossa avversario...";
                        aspettoMossaAvversario = true;
                    }
                    break;
            }

            AggiornaVisualizzazione();
        }

        // ═════════════════════════════════════════════════════════
        // VISUALIZZAZIONE
        // ═════════════════════════════════════════════════════════

        private void AggiornaVisualizzazione()
        {
            lblTurno.Text      = $"Turno: {gm.GetTurno()}";
            lblTurno.ForeColor = gm.GetTurno() == 'X' ? TM.ColoreX : TM.ColoreO;

            int prossimoTris = gm.GetProssimaTrisObbligatoria();

            if (modalitaGioco != 2)
            {
                if (prossimoTris == -1)
                {
                    lblInfo.Text      = "Mossa libera!";
                    lblInfo.ForeColor = Color.FromArgb(100, 200, 100);
                }
                else
                {
                    lblInfo.Text      = $"Devi giocare nel tris #{prossimoTris + 1}";
                    lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;

                panels[i].BackColor = prossimoTris == -1 ? TM.ColoreTrisNormale
                                    : i == prossimoTris  ? TM.ColoreTrisAttivo
                                                         : TM.ColoreTrisCompletato;
            }
        }

        /// <summary>
        /// Aggiorna visivamente una cella dopo un flush:
        /// azzera testo e colore di tutti i bottoni del tris specificato.
        /// </summary>
        private void AggiornaUIFlush(int numTris)
        {
            for (int i = 0; i < 9; i++)
            {
                var btn = buttons[numTris, i];
                if (btn == null) continue;
                btn.Text      = "";
                btn.ForeColor = TM.ColoreTesto;
            }
        }

        // ═════════════════════════════════════════════════════════
        // GESTIONE MOSSA GIOCATORE
        // ═════════════════════════════════════════════════════════

        private void Mossa(object? sender, EventArgs e)
        {
            if (botInPensiero)
            {
                if (modalitaGioco == 1)
                {
                    lblInfo.Text      = "⏳ Aspetta che il bot finisca di pensare!";
                    lblInfo.ForeColor = TM.ColoreX;

                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        this.Invoke(() =>
                        {
                            if (botInPensiero)
                            {
                                lblInfo.Text      = "🤖 Il bot sta pensando...";
                                lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
                            }
                        });
                    });
                }
                return;
            }

            if (modalitaGioco == 2) return;

            if (sender is not CustomButton btn2)
            {
                MessageBox.Show("Errore interno del gioco. Riavvia l'applicazione.");
                return;
            }

            if (btn2.Text != "")
            {
                lblInfo.Text      = "❌ Cella già occupata!";
                lblInfo.ForeColor = TM.ColoreX;
                return;
            }

            var (numTris, row, col) = ParseTag(btn2.Tag?.ToString() ?? "");
            if (numTris < 0) return;

            EseguiMossa(numTris, row, col, btn2);
        }

        private void EseguiMossa(int numTris, int row, int col, CustomButton? btn)
        {
            int prossimoNumTris = row * 3 + col;

            if (gm.MakeMove(numTris, row, col))
            {
                char turnoAttuale = gm.GetTurno();

                if (btn != null)
                {
                    btn.Text      = turnoAttuale.ToString();
                    btn.ForeColor = turnoAttuale == 'X' ? TM.ColoreX : TM.ColoreO;
                }

                // TODO: rimuovere quando il debug non serve più
                fileManager?.Write($"{turnoAttuale} {numTris} {(row * 3) + col}");

                // Se modalità Flush e il prossimo tris è stato resettato, aggiorna UI
                if (modalitaTrisPieno == ModalitaTrisPieno.Flush
                    && gm.GetProssimaTrisObbligatoria() == prossimoNumTris)
                {
                    AggiornaUIFlush(prossimoNumTris);
                }

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
                lblInfo.Text      = "❌ Mossa non valida! Controlla dove puoi giocare.";
                lblInfo.ForeColor = TM.ColoreX;

                if (btn != null)
                {
                    Task.Run(async () =>
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            await Task.Delay(50);
                            btn.Invoke(() => btn.BackColor = TM.ColoreTrisAttivo);
                            await Task.Delay(50);
                            btn.Invoke(() => btn.BackColor = TM.ColoreTrisNormale);
                        }
                    });
                }
            }
        }

        // ═════════════════════════════════════════════════════════
        // TURNO BOT (PVE)
        // ═════════════════════════════════════════════════════════

        private bool DeveGiocareilBot()
            => modalitaGioco == 1 && gm.GetTurno() == 'O';

        private async void EseguiTurnoBot()
        {
            botInPensiero = true;

            lblInfo.Text      = "🤖 Il bot sta pensando...";
            lblInfo.ForeColor = Color.FromArgb(255, 200, 100);
            CambiaCursoreBottoni(Cursors.No);
            DimmaPanels(true);

            await Task.Delay(300);

            var mossa = CalcolaMossaBot(gm.GetBoardState(), gm.GetProssimaTrisObbligatoria(), gm.GetTurno());

            if (mossa.HasValue)
            {
                int numTris = mossa.Value.numTris;
                int row     = mossa.Value.row;
                int col     = mossa.Value.col;
                var btn     = buttons[numTris, row * 3 + col];

                EseguiMossa(numTris, row, col, btn);
            }

            botInPensiero = false;
            CambiaCursoreBottoni(Cursors.Hand);
            DimmaPanels(false);
        }

        // ═════════════════════════════════════════════════════════
        // MODALITÀ EVE
        // ═════════════════════════════════════════════════════════

        private void EseguiMossaBot()
        {
            if (aspettoMossaAvversario) return;

            botInPensiero     = true;
            lblInfo.Text      = "🤖 Il mio bot sta pensando...";

            char turno  = gm.GetTurno();
            var  mossa  = CalcolaMossaBot(gm.GetBoardState(), gm.GetProssimaTrisObbligatoria(), turno);

            if (mossa.HasValue)
            {
                int numTris = mossa.Value.numTris;
                int row     = mossa.Value.row;
                int col     = mossa.Value.col;
                var btn     = buttons[numTris, row * 3 + col];

                EseguiMossa(numTris, row, col, btn);

                if (fileManager != null)
                {
                    string mossaStr = $"{turno} {numTris} {(row * 3) + col}";
                    fileWatcher?.AggiornaUltimaRiga(mossaStr);
                }

                lblInfo.Text           = "⏳ Aspetto mossa avversario...";
                aspettoMossaAvversario = true;
            }
            else
            {
                MessageBox.Show(
                    $"⚠️ ERRORE: Il bot non ha trovato una mossa valida!\n\nTurno: {turno}",
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
                int  numTris         = int.Parse(parti[1]);
                int  posizione       = int.Parse(parti[2]);
                int  row             = posizione / 3;
                int  col             = posizione % 3;

                lblInfo.Text = $"📥 Ricevuta mossa avversario: Tris {numTris}, Pos {posizione}";

                var btn = buttons[numTris, row * 3 + col];
                EseguiMossa(numTris, row, col, btn);

                Task.Delay(500).ContinueWith(_ => this.Invoke(() => EseguiMossaBot()));
            });
        }

        // ═════════════════════════════════════════════════════════
        // FINE PARTITA
        // ═════════════════════════════════════════════════════════

        private void GestioneVittoria(char vincitore)
        {
            Color coloreVincitore = vincitore == 'X' ? TM.ColoreX : TM.ColoreO;

            lblInfo.Text      = $"🎉 {vincitore} ha vinto! 🎉";
            lblInfo.ForeColor = coloreVincitore;
            lblInfo.Font      = TM.FontTitolo;

            // Blocca tutti i bottoni
            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;
                foreach (Control c in panels[i].Controls)
                    if (c is CustomButton b) b.Cursor = Cursors.No;
            }

            if (modalitaGioco == 2) fileWatcher?.Ferma();

            // Dialog fine partita con scelta
            var risultato = MessageBox.Show(
                $"🎉 {vincitore} ha vinto!\n\nVuoi giocare ancora?",
                "Fine Partita",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (risultato == DialogResult.Yes)
                NuovaPartita();
            else
                TornaAlMenu();
        }

        private void NuovaPartita()
        {
            _nuovaPartita = true; // blocca FormClosing dal mostrare il SelectionForm
            var nuovoForm = new GameForm(modalitaGioco, FormIniziale, tipoBot, modalitaTrisPieno, sonoGiocatore1);
            nuovoForm.Show();
            this.Close();
        }

        private void TornaAlMenu()
        {
            fileWatcher?.Ferma();
            FormIniziale.Show();
            this.Close();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileWatcher?.Ferma();
            // Non mostrare il menu se si sta aprendo una nuova partita
            if (!_nuovaPartita)
                FormIniziale.Show();
        }

        // ═════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════

        private (int numTris, int row, int col) ParseTag(string tag)
        {
            // Formato atteso: "Tris{n}Row{r}Col{c}" con n,r,c cifra singola
            var digits = tag.Where(char.IsDigit).Select(c => c - '0').ToList();
            if (digits.Count != 3) return (-1, -1, -1);
            return (digits[0], digits[1], digits[2]);
        }

        private (int numTris, int row, int col)? CalcolaMossaBot(string boardState, int trisObb, char turno)
        {
            return tipoBot == 1
                ? botAllenato?.CalcolaMossa(boardState, trisObb, turno)
                : botAlgoritmico?.CalcolaMossa(boardState, trisObb, turno);
        }

        private void CambiaCursoreBottoni(Cursor cursore)
        {
            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;
                foreach (Control ctrl in panels[i].Controls)
                    if (ctrl is CustomButton btn && btn.Text == "")
                        btn.Cursor = cursore;
            }
        }

        private void DimmaPanels(bool dimma)
        {
            int prossimoTris = gm.GetProssimaTrisObbligatoria();
            for (int i = 0; i < 9; i++)
            {
                if (panels[i] == null) continue;
                panels[i].BackColor = dimma
                    ? TM.ColoreTrisCompletato
                    : prossimoTris == -1 ? TM.ColoreTrisNormale
                    : i == prossimoTris  ? TM.ColoreTrisAttivo
                                         : TM.ColoreTrisCompletato;
            }
        }

        // ═════════════════════════════════════════════════════════
        // INIT UI
        // ═════════════════════════════════════════════════════════

        internal void InitializeUI()
        {
            const int BUTTON_SIZE   = 50;
            const int BUTTON_MARGIN = 2;
            const int TRIS_SPACING  = 10;
            const int START_X       = 30;
            const int START_Y       = 80;

            lblInfo = new Label
            {
                Location  = new Point(START_X + 100, 25),
                Size      = new Size(400, 20),
                Font      = TM.FontUI,
                ForeColor = TM.ColoreTestoSpento,
                BackColor = Color.Transparent,
                Text      = "Mossa libera - Gioca dove vuoi!"
            };
            Controls.Add(lblInfo);

            lblTurno = new Label
            {
                Location  = new Point(START_X, 20),
                Size      = new Size(300, 30),
                Font      = TM.FontTitolo,
                ForeColor = TM.ColoreTesto,
                BackColor = Color.Transparent,
                Text      = "Turno: X"
            };
            Controls.Add(lblTurno);

            for (int numTris = 0; numTris < 9; numTris++)
            {
                int trisRow = numTris / 3;
                int trisCol = numTris % 3;

                Panel trisPanel = new Panel
                {
                    Location  = new Point(
                        START_X + trisCol * (BUTTON_SIZE * 3 + BUTTON_MARGIN * 2 + TRIS_SPACING),
                        START_Y + trisRow * (BUTTON_SIZE * 3 + BUTTON_MARGIN * 2 + TRIS_SPACING)
                    ),
                    Size      = new Size(BUTTON_SIZE * 3 + BUTTON_MARGIN * 2, BUTTON_SIZE * 3 + BUTTON_MARGIN * 2),
                    BackColor = TM.ColoreTrisNormale,
                    Tag       = $"Panel{numTris}"
                };
                Controls.Add(trisPanel);
                panels[numTris] = trisPanel;

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        var btn = new CustomButton
                        {
                            Text     = "",
                            Location = new Point(col * (BUTTON_SIZE + BUTTON_MARGIN), row * (BUTTON_SIZE + BUTTON_MARGIN)),
                            Size     = new Size(BUTTON_SIZE, BUTTON_SIZE),
                            Tag      = $"Tris{numTris}Row{row}Col{col}",
                            Font     = new Font(TM.FontTitolo.FontFamily, 16, FontStyle.Bold),
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