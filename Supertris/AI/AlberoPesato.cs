namespace Supertris.AI
{
    /// <summary>
    /// Bot che impara giocando usando Reinforcement Learning.
    /// 
    /// FUNZIONAMENTO:
    /// - Mantiene una tabella: stato_gioco -> pesi_mosse
    /// - Chiave stato: boardState + trisObbligatoria
    /// - Sceglie la mossa con peso maggiore
    /// - Dopo vittoria: aumenta pesi delle mosse fatte (+0.1)
    /// - Dopo sconfitta: diminuisce pesi delle mosse fatte (-0.1)
    /// - Salva/carica pesi su file per mantenere apprendimento
    /// - Ottimizzato: valuta 9 tris invece di 81 celle con mossa libera
    /// 
    /// IMPARA: migliora con l'esperienza, più partite = più forte
    /// </summary>

    internal class AlberoPesato : IBot
    {
        // Tabella che mappa stato -> pesi delle mosse
        // Chiave: stato della board (es. "---X-----O---...") + trisObbligatoria
        // Valore: dizionario di mosse con i loro pesi
        private Dictionary<string, Dictionary<string, int>> tabellaStati;

        // Percorso seguito durante la partita corrente
        private List<(string stato, string mossa)> percorsoPartita;

        // Random per esplorare mosse nuove (epsilon-greedy opzionale)
        private Random random;

        // Path per i pesi
        private static string path = System.AppDomain.CurrentDomain.BaseDirectory + "supertris_bot.weights";

        // Parametri di apprendimento
        private int incrementoVittoria = 2;
        private int decrementoSconfitta = 2;
        private int pesoIniziale = 5; // Peso neutro per mosse mai viste

        public AlberoPesato(bool a_pesi)
        {
            tabellaStati = new Dictionary<string, Dictionary<string, int>>();
            percorsoPartita = new List<(string, string)>();
            random = new Random();

            // se il bot é un bot da allenamento, denominato da a_pesi, allora carico i pesi, 
            if (a_pesi)
            {
                if (File.Exists(path))
                {
                    this.CaricaPesi(path);
                }
            }
        }

        public (int numTris, int row, int col)? CalcolaMossa(string boardState, int trisObbligatoria, char turno)
        {
            if (trisObbligatoria == -1)
            {
                // MOSSA LIBERA - Ottimizzazione:
                // Valuta la migliore mossa di ogni tris e scegli la migliore globale
                (int numTris, int row, int col) mossaMiglioreGlobale = (-1, -1, -1);
                float pesoMiglioreGlobale = float.MinValue;

                for (int numTris = 0; numTris < 9; numTris++)
                {
                    // Crea chiave stato per questo tris
                    string chiaveStato = CreaChiaveStato(boardState, numTris);

                    // Assicurati che lo stato esista
                    if (!tabellaStati.ContainsKey(chiaveStato))
                    {
                        tabellaStati[chiaveStato] = new Dictionary<string, int>();
                    }

                    // Trova la migliore mossa in questo tris
                    var mossaMiglioreTris = TrovaMiglioreMossaInTris(boardState, numTris, chiaveStato);

                    if (mossaMiglioreTris.HasValue)
                    {
                        float peso = mossaMiglioreTris.Value.peso;

                        if (peso > pesoMiglioreGlobale)
                        {
                            pesoMiglioreGlobale = peso;
                            mossaMiglioreGlobale = (numTris, mossaMiglioreTris.Value.row, mossaMiglioreTris.Value.col);
                        }
                    }
                }

                if (mossaMiglioreGlobale.numTris == -1)
                    return null;

                // Salva nel percorso con lo stato corretto
                string chiaveStatoFinale = CreaChiaveStato(boardState, mossaMiglioreGlobale.numTris);
                string chiaveMossaFinale = CreaChiaveMossa(
                    mossaMiglioreGlobale.numTris, 
                    mossaMiglioreGlobale.row, 
                    mossaMiglioreGlobale.col);
                percorsoPartita.Add((chiaveStatoFinale, chiaveMossaFinale));

                return mossaMiglioreGlobale;
            }
            else
            {
                // MOSSA OBBLIGATORIA - Valuta solo il tris specificato
                string chiaveStato = CreaChiaveStato(boardState, trisObbligatoria);

                // Assicurati che lo stato esista
                if (!tabellaStati.ContainsKey(chiaveStato))
                {
                    tabellaStati[chiaveStato] = new Dictionary<string, int>();
                }

                var mossaMigliore = TrovaMiglioreMossaInTris(boardState, trisObbligatoria, chiaveStato);

                if (mossaMigliore.HasValue)
                {
                    string chiaveMossa = CreaChiaveMossa(
                        trisObbligatoria, 
                        mossaMigliore.Value.row, 
                        mossaMigliore.Value.col);
                    
                    // Salva nel percorso
                    percorsoPartita.Add((chiaveStato, chiaveMossa));

                    return (trisObbligatoria, mossaMigliore.Value.row, mossaMigliore.Value.col);
                }

                return null;
            }
        }

        /// <summary>
        /// Trova la migliore mossa (peso più alto) in un singolo tris
        /// </summary>
        private (int row, int col, float peso)? TrovaMiglioreMossaInTris(
            string boardState, int numTris, string chiaveStato)
        {
            List<(int numTris, int row, int col)> mosse = new List<(int numTris, int row, int col)>();
            AggiungiMosseTris(mosse, boardState, numTris);

            if (mosse.Count == 0)
                return null;

            (int row, int col) mossaMigliore = (mosse[0].row, mosse[0].col);
            float pesoMigliore = float.MinValue;

            foreach (var mossa in mosse)
            {
                string chiaveMossa = CreaChiaveMossa(mossa.numTris, mossa.row, mossa.col);

                // Se la mossa non è mai stata vista, inizializza con peso neutro
                if (!tabellaStati[chiaveStato].ContainsKey(chiaveMossa))
                {
                    tabellaStati[chiaveStato][chiaveMossa] = pesoIniziale;
                }

                float peso = tabellaStati[chiaveStato][chiaveMossa];

                if (peso > pesoMigliore)
                {
                    pesoMigliore = peso;
                    mossaMigliore = (mossa.row, mossa.col);
                }
            }

            return (mossaMigliore.row, mossaMigliore.col, pesoMigliore);
        }

        public void NotificaRisultatoPartita(bool? haVinto)
        {
            if (haVinto == null)
            {
                // Pareggio - nessun cambio ai pesi
                percorsoPartita.Clear();
                return;
            }

            int delta = haVinto.Value ? incrementoVittoria : -decrementoSconfitta;

            // Aggiorna i pesi di tutte le mosse nel percorso
            foreach (var (stato, mossa) in percorsoPartita)
            {
                if (tabellaStati.ContainsKey(stato) && tabellaStati[stato].ContainsKey(mossa))
                {
                    tabellaStati[stato][mossa] += delta;

                    // Mantieni i pesi in un range ragionevole (0.0 - 1.0)
                    tabellaStati[stato][mossa] = Math.Max(0, Math.Min(1, tabellaStati[stato][mossa]));
                }
            }

            // Pulisci il percorso per la prossima partita
            percorsoPartita.Clear();
        }

        public void ResetPartita()
        {
            percorsoPartita.Clear();
        }

        /// <summary>
        /// Salva i pesi appresi su file
        /// </summary>
        public void SalvaPesi(string percorsoFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(percorsoFile))
                {
                    foreach (var statoEntry in tabellaStati)
                    {
                        string stato = statoEntry.Key;

                        foreach (var mossaEntry in statoEntry.Value)
                        {
                            string mossa = mossaEntry.Key;
                            float peso = mossaEntry.Value;

                            // Formato: stato|mossa|peso
                            writer.WriteLine($"{stato}|{mossa}|{peso}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvare i pesi: {ex.Message}");
            }
        }

        /// <summary>
        /// Carica i pesi da file
        /// </summary>
        public void CaricaPesi(string percorsoFile)
        {
            try
            {
                if (!File.Exists(percorsoFile))
                    return;

                tabellaStati.Clear();

                using (StreamReader reader = new StreamReader(percorsoFile))
                {
                    string linea;
                    while ((linea = reader.ReadLine()) != null)
                    {
                        string[] parti = linea.Split('|');
                        if (parti.Length != 3)
                            continue;

                        string stato = parti[0];
                        string mossa = parti[1];
                        int peso = int.Parse(parti[2]);

                        if (!tabellaStati.ContainsKey(stato))
                        {
                            tabellaStati[stato] = new Dictionary<string, int>();
                        }

                        tabellaStati[stato][mossa] = peso;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricare i pesi: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottieni statistiche sull'apprendimento
        /// </summary>
        public (int statiAppresi, int mosseApprese) OttieniStatistiche()
        {
            int statiAppresi = tabellaStati.Count;
            int mosseApprese = tabellaStati.Values.Sum(dict => dict.Count);
            return (statiAppresi, mosseApprese);
        }

        // ==================== METODI HELPER ==================== //

        private string CreaChiaveStato(string boardState, int trisObbligatoria)
        {
            return $"{boardState}:{trisObbligatoria}";
        }

        private string CreaChiaveMossa(int numTris, int row, int col)
        {
            return $"T{numTris}R{row}C{col}";
        }

        private (int numTris, int row, int col) DecodificaMossa(string chiaveMossa)
        {
            // Formato: "T4R1C2" -> numTris=4, row=1, col=2
            int numTris = int.Parse(chiaveMossa.Substring(1, 1));
            int row = int.Parse(chiaveMossa.Substring(3, 1));
            int col = int.Parse(chiaveMossa.Substring(5, 1));
            return (numTris, row, col);
        }

        private List<(int numTris, int row, int col)> TrovaMosseValide(string boardState, int trisObbligatoria)
        {
            List<(int numTris, int row, int col)> mosse = new List<(int numTris, int row, int col)>();

            if (trisObbligatoria == -1)
            {
                // Mossa libera - controlla tutti i tris
                for (int numTris = 0; numTris < 9; numTris++)
                {
                    AggiungiMosseTris(mosse, boardState, numTris);
                }
            }
            else
            {
                // Deve giocare in un tris specifico
                AggiungiMosseTris(mosse, boardState, trisObbligatoria);
            }

            return mosse;
        }

        private void AggiungiMosseTris(List<(int numTris, int row, int col)> mosse, string boardState, int numTris)
        {
            int offset = numTris * 9;

            for (int i = 0; i < 9; i++)
            {
                if (boardState[offset + i] == '-')
                {
                    int row = i / 3;
                    int col = i % 3;
                    mosse.Add((numTris, row, col));
                }
            }
        }
    }
}
