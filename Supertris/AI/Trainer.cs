using Supertris.AI;
using Supertris.Classes;

namespace Supertris.AI
{
    /// <summary>
    /// Sistema di allenamento per far giocare due bot l'uno contro l'altro.
    /// 
    /// FUNZIONAMENTO:
    /// - Fa giocare botAllenato vs botAvversario per N partite
    /// - Alterna chi inizia (X/O) per bilanciare
    /// - Simula partite complete gestendo turni e vittorie
    /// - Notifica risultati ai bot dopo ogni partita
    /// - Raccoglie statistiche: vittorie, sconfitte, pareggi
    /// - Callback ogni 10 partite per aggiornare UI
    /// 
    /// USO TIPICO: AlberoPesato (impara) vs MinimaxBot (forte)
    /// </summary>
   
    internal class Trainer
    {
        private AlberoPesato botAllenato;
        private IBot botAvversario;

        // Statistiche
        private int vittorie;
        private int sconfitte;
        private int pareggi;
        private int partiteGiocate;

        public Trainer(AlberoPesato botDaAllenare, IBot avversario)
        {
            botAllenato = botDaAllenare;
            botAvversario = avversario;
            ResetStatistiche();
        }

        /// <summary>
        /// Allena il bot facendogli giocare un numero specificato di partite
        /// </summary>
        /// <param name="numeroPartite">Quante partite giocare</param>
        /// <param name="callback">Callback per aggiornare la UI (opzionale)</param>
        public void Allena(int numeroPartite, Action<int, int, int, int>? callback = null)
        {
            for (int i = 0; i < numeroPartite; i++)
            {
                // Alterna chi inizia (X o O)
                bool botAllenatoComincia = (i % 2 == 0);

                char risultato = GiocaPartita(botAllenatoComincia);

                // Aggiorna statistiche
                if (risultato == 'X' && botAllenatoComincia || risultato == 'O' && !botAllenatoComincia)
                {
                    vittorie++;
                }
                else if (risultato == 'X' && !botAllenatoComincia || risultato == 'O' && botAllenatoComincia)
                {
                    sconfitte++;
                }
                else if (risultato == '=')
                {
                    pareggi++;
                }

                partiteGiocate++;

                // Callback per aggiornare UI ogni 10 partite
                if (callback != null && (i + 1) % 10 == 0)
                {
                    callback(partiteGiocate, vittorie, sconfitte, pareggi);
                }
            }
        }

        /// <summary>
        /// Gioca una singola partita tra i due bot
        /// </summary>
        /// <param name="botAllenatoComincia">Se true, il bot allenato gioca come X</param>
        /// <returns>Risultato: 'X', 'O' per vittoria, '=' per pareggio</returns>
        private char GiocaPartita(bool botAllenatoComincia)
        {
            var board = new Classes.Supertris();
            botAllenato.ResetPartita();
            botAvversario.ResetPartita();

            int trisObbligatoria = -1;
            char turnoCorrente = 'X';
            int mosseGiocate = 0;
            const int MAX_MOSSE = 81; // Limite per evitare loop infiniti

            while (mosseGiocate < MAX_MOSSE)
            {
                // Determina quale bot deve giocare
                bool turnoDelBotAllenato = (turnoCorrente == 'X' && botAllenatoComincia) ||
                                           (turnoCorrente == 'O' && !botAllenatoComincia);

                IBot botCorrente = turnoDelBotAllenato ? botAllenato : botAvversario;

                // Ottieni stato della board
                string boardState = OttieniBoardState(board);

                // Bot calcola la mossa
                var mossa = botCorrente.CalcolaMossa(boardState, trisObbligatoria, turnoCorrente);

                if (mossa == null)
                {
                    // Nessuna mossa disponibile - pareggio
                    botAllenato.NotificaRisultatoPartita(null);
                    botAvversario.NotificaRisultatoPartita(null);
                    return '=';
                }

                int numTris = mossa.Value.numTris;
                int row = mossa.Value.row;
                int col = mossa.Value.col;

                // Esegui la mossa
                int trisRow = numTris / 3;
                int trisCol = numTris % 3;

                if (!board.MakeMove(turnoCorrente, trisRow, trisCol, row, col))
                {
                    // Mossa invalida - il bot ha sbagliato
                    // Consideriamo questo come sconfitta del bot corrente
                    if (turnoDelBotAllenato)
                    {
                        botAllenato.NotificaRisultatoPartita(false);
                        botAvversario.NotificaRisultatoPartita(true);
                        return turnoCorrente == 'X' ? 'O' : 'X';
                    }
                    else
                    {
                        botAllenato.NotificaRisultatoPartita(true);
                        botAvversario.NotificaRisultatoPartita(false);
                        return turnoCorrente == 'X' ? 'O' : 'X';
                    }
                }

                // Calcola prossimo tris obbligatorio
                int prossimoNumTris = row * 3 + col;
                int prossimoTrisRow = prossimoNumTris / 3;
                int prossimoTrisCol = prossimoNumTris % 3;

                if (board.IsTrisCompletato(prossimoTrisRow, prossimoTrisCol))
                {
                    trisObbligatoria = -1;
                }
                else
                {
                    trisObbligatoria = prossimoNumTris;
                }

                // Controlla vittoria
                char vincitore = board.CheckWin();
                if (vincitore != '-')
                {
                    // Qualcuno ha vinto
                    bool botAllenatoHaVinto = (vincitore == 'X' && botAllenatoComincia) ||
                                              (vincitore == 'O' && !botAllenatoComincia);

                    botAllenato.NotificaRisultatoPartita(botAllenatoHaVinto);
                    botAvversario.NotificaRisultatoPartita(!botAllenatoHaVinto);

                    return vincitore;
                }

                // Cambia turno
                turnoCorrente = turnoCorrente == 'X' ? 'O' : 'X';
                mosseGiocate++;
            }

            // Limite mosse raggiunto - pareggio
            botAllenato.NotificaRisultatoPartita(null);
            botAvversario.NotificaRisultatoPartita(null);
            return '=';
        }

        /// <summary>
        /// Converte lo stato della Supertris in una stringa
        /// </summary>
        private string OttieniBoardState(Classes.Supertris board)
        {
            return board.GetCampo();
        }

        public void ResetStatistiche()
        {
            vittorie = 0;
            sconfitte = 0;
            pareggi = 0;
            partiteGiocate = 0;
        }

        public (int vittorie, int sconfitte, int pareggi, int totale) OttieniStatistiche()
        {
            return (vittorie, sconfitte, pareggi, partiteGiocate);
        }

        public double CalcolaPercentualeVittorie()
        {
            if (partiteGiocate == 0) return 0;
            return (double)vittorie / partiteGiocate * 100;
        }
    }
}