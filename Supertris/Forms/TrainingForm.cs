using Supertris.AI;
using System.Diagnostics;

namespace Supertris.Forms
{
    /// <summary>
    /// Interfaccia per allenare l'AlberoPesato.
    /// 
    /// FUNZIONAMENTO:
    /// - NumericUpDown per scegliere numero partite
    /// - Avvia allenamento in background (async)
    /// - Progress bar aggiornata ogni 10 partite
    /// - Mostra statistiche in tempo reale
    /// - Pulsanti salva/carica pesi
    /// - Pulsante reset (ricrea bot da zero)
    /// - Log con timestamp per monitorare progresso
    /// 
    /// CALLBACK: Trainer notifica ogni 10 partite -> aggiorna UI
    /// </summary>
    
    public partial class TrainingForm : Form
    {
        private AlberoPesato bot;
        private Trainer trainer;
        private bool allenamentoInCorso;

        // UI Controls
        private Label lblTitolo;
        private Label lblPartiteGiocate;
        private Label lblVittorie;
        private Label lblSconfitte;
        private Label lblPareggi;
        private Label lblPercentuale;
        private Label lblStatiAppresi;
        private ProgressBar progressBar;
        private Button btnAvviaAllenamento;
        private Button btnSalvaPesi;
        private Button btnCaricaPesi;
        private Button btnReset;
        private NumericUpDown numPartite;
        private TextBox txtLog;

        // Colori
        private readonly Color coloreSfondo = Color.FromArgb(30, 30, 30);
        private readonly Color colorePanel = Color.FromArgb(45, 45, 48);
        private readonly Color coloreTesto = Color.White;
        private readonly Color coloreVerde = Color.FromArgb(100, 200, 100);
        private readonly Color coloreRosso = Color.FromArgb(220, 80, 80);
        private readonly Color coloreBlu = Color.FromArgb(80, 150, 220);

        public TrainingForm()
        {
            InitializeComponent();
            InitializeUI();

            bot = new AlberoPesato(true);
            trainer = new Trainer(bot, new MinimaxBot());
            allenamentoInCorso = false;

            AggiornaStatistiche(0, 0, 0, 0);
        }

        private void InitializeUI()
        {
            this.Text = "Allenamento AlberoPesato";
            this.Size = new Size(700, 650);
            this.BackColor = coloreSfondo;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;

            // Titolo
            lblTitolo = new Label
            {
                Text = "🤖 Allenamento Bot Super Tris",
                Location = new Point(20, y),
                Size = new Size(660, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = coloreTesto,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitolo);
            y += 50;

            // Panel statistiche
            Panel panelStats = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(660, 180),
                BackColor = colorePanel
            };
            Controls.Add(panelStats);

            // Statistiche
            int statY = 10;
            lblPartiteGiocate = CreaLabelStat(panelStats, "Partite giocate:", "0", 10, statY);
            statY += 30;
            lblVittorie = CreaLabelStat(panelStats, "Vittorie:", "0", 10, statY, coloreVerde);
            statY += 30;
            lblSconfitte = CreaLabelStat(panelStats, "Sconfitte:", "0", 10, statY, coloreRosso);
            statY += 30;
            lblPareggi = CreaLabelStat(panelStats, "Pareggi:", "0", 10, statY, coloreBlu);
            statY += 30;
            lblPercentuale = CreaLabelStat(panelStats, "% Vittorie:", "0.0%", 10, statY, coloreVerde);
            statY += 30;
            lblStatiAppresi = CreaLabelStat(panelStats, "Stati appresi:", "0", 10, statY);

            y += 190;

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, y),
                Size = new Size(660, 25),
                Style = ProgressBarStyle.Continuous,
                ForeColor = coloreVerde
            };
            Controls.Add(progressBar);
            y += 35;

            // Controlli allenamento
            Panel panelControlli = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(660, 70),
                BackColor = colorePanel
            };
            Controls.Add(panelControlli);

            Label lblNumPartite = new Label
            {
                Text = "Numero partite:",
                Location = new Point(10, 15),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = coloreTesto,
                BackColor = Color.Transparent
            };
            panelControlli.Controls.Add(lblNumPartite);

            numPartite = new NumericUpDown
            {
                Location = new Point(130, 12),
                Size = new Size(100, 25),
                Minimum = 10,
                Maximum = 100000,
                Value = 1000,
                Increment = 100,
                Font = new Font("Segoe UI", 10)
            };
            panelControlli.Controls.Add(numPartite);

            btnAvviaAllenamento = new Button
            {
                Text = "▶ Avvia Allenamento",
                Location = new Point(250, 10),
                Size = new Size(180, 30),
                BackColor = coloreVerde,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAvviaAllenamento.FlatAppearance.BorderSize = 0;
            btnAvviaAllenamento.Click += BtnAvviaAllenamento_Click;
            panelControlli.Controls.Add(btnAvviaAllenamento);

            btnReset = new Button
            {
                Text = "🔄 Reset",
                Location = new Point(450, 10),
                Size = new Size(90, 30),
                BackColor = coloreRosso,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;
            panelControlli.Controls.Add(btnReset);

            Label lblControlli = new Label
            {
                Text = "💡 Il bot impara giocando contro un bot che utilizza l'algoritmo Minimaxx",
                Location = new Point(10, 45),
                Size = new Size(640, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent
            };
            panelControlli.Controls.Add(lblControlli);

            y += 80;

            // Pulsanti salva/carica
            Panel panelFile = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(660, 45),
                BackColor = colorePanel
            };
            Controls.Add(panelFile);

            btnSalvaPesi = new Button
            {
                Text = "💾 Salva Pesi",
                Location = new Point(10, 8),
                Size = new Size(150, 30),
                BackColor = coloreBlu,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSalvaPesi.FlatAppearance.BorderSize = 0;
            btnSalvaPesi.Click += BtnSalvaPesi_Click;
            panelFile.Controls.Add(btnSalvaPesi);

            btnCaricaPesi = new Button
            {
                Text = "📂 Carica Pesi",
                Location = new Point(170, 8),
                Size = new Size(150, 30),
                BackColor = coloreBlu,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCaricaPesi.FlatAppearance.BorderSize = 0;
            btnCaricaPesi.Click += BtnCaricaPesi_Click;
            panelFile.Controls.Add(btnCaricaPesi);

            y += 55;

            // Log
            Label lblLog = new Label
            {
                Text = "📋 Log:",
                Location = new Point(20, y),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = coloreTesto,
                BackColor = Color.Transparent
            };
            Controls.Add(lblLog);
            y += 25;

            txtLog = new TextBox
            {
                Location = new Point(20, y),
                Size = new Size(660, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Consolas", 9)
            };
            Controls.Add(txtLog);
        }

        private Label CreaLabelStat(Panel parent, string testo, string valore, int x, int y, Color? coloreValore = null)
        {
            Label lblTesto = new Label
            {
                Text = testo,
                Location = new Point(x, y),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = coloreTesto,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblTesto);

            Label lblValore = new Label
            {
                Text = valore,
                Location = new Point(x + 210, y),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = coloreValore ?? coloreTesto,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lblValore);

            return lblValore;
        }

        private async void BtnAvviaAllenamento_Click(object? sender, EventArgs e)
        {
            if (allenamentoInCorso)
            {
                AggiungiLog("⚠️ Allenamento già in corso!");
                return;
            }

            allenamentoInCorso = true;
            btnAvviaAllenamento.Enabled = false;
            btnAvviaAllenamento.Text = "⏸ Allenamento in corso...";

            int numeroPartite = (int)numPartite.Value;
            progressBar.Maximum = numeroPartite;
            progressBar.Value = 0;

            AggiungiLog($"🚀 Avvio allenamento per {numeroPartite} partite...");

            Stopwatch sw = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                trainer.Allena(numeroPartite, (partite, vitt, sconf, par) =>
                {
                    // Aggiorna UI sul thread principale
                    this.Invoke(() =>
                    {
                        AggiornaStatistiche(partite, vitt, sconf, par);
                        progressBar.Value = partite;
                    });
                });
            });

            sw.Stop();

            allenamentoInCorso = false;
            btnAvviaAllenamento.Enabled = true;
            btnAvviaAllenamento.Text = "▶ Avvia Allenamento";

            var stats = bot.OttieniStatistiche();
            AggiungiLog($"✅ Allenamento completato in {sw.Elapsed.TotalSeconds:F2}s");
            AggiungiLog($"📊 Stati appresi: {stats.statiAppresi}, Mosse totali: {stats.mosseApprese}");
        }

        private void AggiornaStatistiche(int partite, int vitt, int sconf, int par)
        {
            lblPartiteGiocate.Text = partite.ToString();
            lblVittorie.Text = vitt.ToString();
            lblSconfitte.Text = sconf.ToString();
            lblPareggi.Text = par.ToString();

            double percentuale = partite > 0 ? (double)vitt / partite * 100 : 0;
            lblPercentuale.Text = $"{percentuale:F1}%";

            var stats = bot.OttieniStatistiche();
            lblStatiAppresi.Text = $"{stats.statiAppresi} ({stats.mosseApprese} mosse)";
        }

        private void BtnSalvaPesi_Click(object? sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "File Pesi (*.weights)|*.weights|Tutti i file (*.*)|*.*",
                DefaultExt = "weights",
                FileName = "supertris_bot.weights"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                bot.SalvaPesi(saveDialog.FileName);
                AggiungiLog($"💾 Pesi salvati in: {Path.GetFileName(saveDialog.FileName)}");
            }
        }

        private void BtnCaricaPesi_Click(object? sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "File Pesi (*.weights)|*.weights|Tutti i file (*.*)|*.*",
                DefaultExt = "weights"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                bot.CaricaPesi(openDialog.FileName);
                var stats = bot.OttieniStatistiche();
                AggiungiLog($"📂 Pesi caricati da: {Path.GetFileName(openDialog.FileName)}");
                AggiungiLog($"📊 Stati caricati: {stats.statiAppresi}, Mosse: {stats.mosseApprese}");

                lblStatiAppresi.Text = $"{stats.statiAppresi} ({stats.mosseApprese} mosse)";
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Sei sicuro di voler resettare tutte le statistiche e l'apprendimento del bot?",
                "Conferma Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                bot = new AlberoPesato(false);
                trainer = new Trainer(bot, new AlberoPesato(true));
                AggiornaStatistiche(0, 0, 0, 0);
                progressBar.Value = 0;
                AggiungiLog("🔄 Bot resettato completamente");
            }
        }

        private void AggiungiLog(string messaggio)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {messaggio}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}