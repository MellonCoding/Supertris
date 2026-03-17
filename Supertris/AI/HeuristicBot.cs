namespace Supertris.AI
{
    /// <summary>
    /// Bot basato su euristica che valuta le mosse usando regole strategiche.
    /// 
    /// FUNZIONAMENTO:
    /// - Valuta ogni mossa con un punteggio basato su posizione, tattica e strategia
    /// - Posizionale: centro vale più di angoli, angoli più di lati
    /// - Tattico: priorità a vittorie immediate e blocchi
    /// - Strategico: valuta dove manda l'avversario (tris vinti/pieni = buono, centro = cattivo)
    /// - Ottimizzato per mosse libere: valuta 9 mosse invece di 81
    /// 
    /// NON IMPARA: usa sempre le stesse regole, non migliora con l'esperienza
    /// </summary>
    internal class HeuristicBot : IBot
    {
        private Random random;

        private const int PESO_VINCITA_GIOCO = 10000;
        private const int PESO_BLOCCO_VITTORIA_GIOCO = 9000;
        private const int PESO_VINCITA_TRIS = 500;
        private const int PESO_BLOCCO_VITTORIA_TRIS = 400;
        private const int PESO_DOPPIA_MINACCIA_TRIS = 300;
        private const int PESO_CENTRO_CENTRALE = 50;
        private const int PESO_CENTRO_TRIS = 25;
        private const int PESO_ANGOLO = 15;
        private const int PESO_LATO = 10;
        private const int PESO_MANDA_TRIS_VINTO = -100;
        private const int PESO_MANDA_TRIS_PIENO = -80;
        private const int PESO_CONTROLLO_CENTRO = 40;
        private const int PESO_MANDA_CENTRO = -60;

        public HeuristicBot()
        {
            random = new Random();
        }

        public (int numTris, int row, int col)? CalcolaMossa(string boardState, int trisObbligatoria, char turno)
        {
            char avversario = turno == 'X' ? 'O' : 'X';

            if (trisObbligatoria == -1)
            {
                (int numTris, int row, int col) mossaMiglioreGlobale = (-1, -1, -1);
                int punteggioMiglioreGlobale = int.MinValue;

                for (int i = 0; i < 9; i++)
                {
                    var mossaMiglioreTris = TrovaMiglioreMossaInTris(boardState, i, turno, avversario);

                    if (mossaMiglioreTris.HasValue)
                    {
                        int punteggio = mossaMiglioreTris.Value.punteggio;

                        if (punteggio > punteggioMiglioreGlobale ||
                            (punteggio == punteggioMiglioreGlobale && random.Next(2) == 0))
                        {
                            punteggioMiglioreGlobale = punteggio;
                            mossaMiglioreGlobale = (i, mossaMiglioreTris.Value.row, mossaMiglioreTris.Value.col);
                        }
                    }
                }

                return mossaMiglioreGlobale.numTris == -1 ? null : mossaMiglioreGlobale;
            }
            else
            {
                var mossaMigliore = TrovaMiglioreMossaInTris(boardState, trisObbligatoria, turno, avversario);

                if (mossaMigliore.HasValue)
                    return (trisObbligatoria, mossaMigliore.Value.row, mossaMigliore.Value.col);

                return null;
            }
        }

        private (int row, int col, int punteggio)? TrovaMiglioreMossaInTris(
            string boardState, int numTris, char turno, char avversario)
        {
            List<(int numTris, int row, int col)> mosse = new List<(int numTris, int row, int col)>();
            AggiungiMosseTris(mosse, boardState, numTris);

            if (mosse.Count == 0)
                return null;

            (int row, int col) mossaMigliore = (mosse[0].row, mosse[0].col);
            int punteggioMigliore = int.MinValue;

            foreach (var mossa in mosse)
            {
                int punteggio = ValutaMossa(boardState, mossa, turno, avversario);

                if (punteggio > punteggioMigliore ||
                    (punteggio == punteggioMigliore && random.Next(2) == 0))
                {
                    punteggioMigliore = punteggio;
                    mossaMigliore = (mossa.row, mossa.col);
                }
            }

            return (mossaMigliore.row, mossaMigliore.col, punteggioMigliore);
        }

        public void NotificaRisultatoPartita(bool? haVinto)
        {
        }

        public void ResetPartita()
        {
        }

        private int ValutaMossa(string boardState, (int numTris, int row, int col) mossa, char turno, char avversario)
        {
            int punteggio = 0;

            punteggio += ValutaPosizione(mossa.numTris, mossa.row, mossa.col);
            punteggio += ValutaTattica(boardState, mossa, turno, avversario);
            punteggio += ValutaStrategia(boardState, mossa, turno);

            return punteggio;
        }

        private int ValutaPosizione(int numTris, int row, int col)
        {
            int punteggio = 0;

            if (numTris == 4 && row == 1 && col == 1)
            {
                punteggio += PESO_CENTRO_CENTRALE;
            }
            else if (row == 1 && col == 1)
            {
                punteggio += PESO_CENTRO_TRIS;
            }
            else if ((row == 0 || row == 2) && (col == 0 || col == 2))
            {
                punteggio += PESO_ANGOLO;
            }
            else
            {
                punteggio += PESO_LATO;
            }

            if (numTris == 4)
            {
                punteggio += PESO_CONTROLLO_CENTRO / 2;
            }
            else if (numTris == 0 || numTris == 2 || numTris == 6 || numTris == 8)
            {
                punteggio += PESO_ANGOLO / 2;
            }

            return punteggio;
        }

        private int ValutaTattica(string boardState, (int numTris, int row, int col) mossa, char turno, char avversario)
        {
            int punteggio = 0;

            string nuovoBoardState = SimulaMossa(boardState, mossa, turno);

            if (ControllaVittoriaGioco(nuovoBoardState, turno))
            {
                return PESO_VINCITA_GIOCO;
            }

            if (MossaBloccoVittoriaGioco(boardState, mossa, avversario))
            {
                punteggio += PESO_BLOCCO_VITTORIA_GIOCO;
            }

            if (ControllaVittoriaTris(nuovoBoardState, mossa.numTris, turno))
            {
                punteggio += PESO_VINCITA_TRIS;
            }

            if (MossaBloccoVittoriaTris(boardState, mossa, avversario))
            {
                punteggio += PESO_BLOCCO_VITTORIA_TRIS;
            }

            if (CreaDoppiaMinaccia(nuovoBoardState, mossa.numTris, turno))
            {
                punteggio += PESO_DOPPIA_MINACCIA_TRIS;
            }

            return punteggio;
        }

        private int ValutaStrategia(string boardState, (int numTris, int row, int col) mossa, char turno)
        {
            int punteggio = 0;

            int prossimoTris = mossa.row * 3 + mossa.col;

            char vincitoreTris = GetVincitoreTris(boardState, prossimoTris);
            if (vincitoreTris != '-')
            {
                punteggio += PESO_MANDA_TRIS_VINTO;
            }
            else if (TrisPieno(boardState, prossimoTris))
            {
                punteggio += PESO_MANDA_TRIS_PIENO;
            }
            else if (prossimoTris == 4)
            {
                punteggio += PESO_MANDA_CENTRO;
            }

            return punteggio;
        }

        private string SimulaMossa(string boardState, (int numTris, int row, int col) mossa, char simbolo)
        {
            char[] stato = boardState.ToCharArray();
            int offset = mossa.numTris * 9;
            int posizione = mossa.row * 3 + mossa.col;
            stato[offset + posizione] = simbolo;
            return new string(stato);
        }

        private bool ControllaVittoriaGioco(string boardState, char turno)
        {
            char[] vincitoriTris = new char[9];
            for (int i = 0; i < 9; i++)
            {
                vincitoriTris[i] = GetVincitoreTris(boardState, i);
            }

            for (int row = 0; row < 3; row++)
            {
                if (vincitoriTris[row * 3] == turno &&
                    vincitoriTris[row * 3 + 1] == turno &&
                    vincitoriTris[row * 3 + 2] == turno)
                    return true;
            }

            for (int col = 0; col < 3; col++)
            {
                if (vincitoriTris[col] == turno &&
                    vincitoriTris[col + 3] == turno &&
                    vincitoriTris[col + 6] == turno)
                    return true;
            }

            if (vincitoriTris[0] == turno && vincitoriTris[4] == turno && vincitoriTris[8] == turno)
                return true;
            if (vincitoriTris[2] == turno && vincitoriTris[4] == turno && vincitoriTris[6] == turno)
                return true;

            return false;
        }

        private bool MossaBloccoVittoriaGioco(string boardState, (int numTris, int row, int col) mossa, char avversario)
        {
            string statoConMossaAvversario = SimulaMossa(boardState, mossa, avversario);
            return ControllaVittoriaGioco(statoConMossaAvversario, avversario);
        }

        private bool ControllaVittoriaTris(string boardState, int numTris, char turno)
        {
            int offset = numTris * 9;
            char[] tris = boardState.Substring(offset, 9).ToCharArray();

            for (int row = 0; row < 3; row++)
            {
                if (tris[row * 3] == turno && tris[row * 3 + 1] == turno && tris[row * 3 + 2] == turno)
                    return true;
            }

            for (int col = 0; col < 3; col++)
            {
                if (tris[col] == turno && tris[col + 3] == turno && tris[col + 6] == turno)
                    return true;
            }

            if (tris[0] == turno && tris[4] == turno && tris[8] == turno)
                return true;
            if (tris[2] == turno && tris[4] == turno && tris[6] == turno)
                return true;

            return false;
        }

        private bool MossaBloccoVittoriaTris(string boardState, (int numTris, int row, int col) mossa, char avversario)
        {
            string statoConMossaAvversario = SimulaMossa(boardState, mossa, avversario);
            return ControllaVittoriaTris(statoConMossaAvversario, mossa.numTris, avversario);
        }

        private bool CreaDoppiaMinaccia(string boardState, int numTris, char turno)
        {
            int offset = numTris * 9;
            char[] tris = boardState.Substring(offset, 9).ToCharArray();
            int minacce = 0;

            for (int row = 0; row < 3; row++)
            {
                int count = 0;
                int vuoti = 0;
                for (int col = 0; col < 3; col++)
                {
                    if (tris[row * 3 + col] == turno) count++;
                    if (tris[row * 3 + col] == '-') vuoti++;
                }
                if (count == 2 && vuoti == 1) minacce++;
            }

            for (int col = 0; col < 3; col++)
            {
                int count = 0;
                int vuoti = 0;
                for (int row = 0; row < 3; row++)
                {
                    if (tris[row * 3 + col] == turno) count++;
                    if (tris[row * 3 + col] == '-') vuoti++;
                }
                if (count == 2 && vuoti == 1) minacce++;
            }

            if ((tris[0] == turno ? 1 : 0) + (tris[4] == turno ? 1 : 0) + (tris[8] == turno ? 1 : 0) == 2 &&
                (tris[0] == '-' ? 1 : 0) + (tris[4] == '-' ? 1 : 0) + (tris[8] == '-' ? 1 : 0) == 1)
                minacce++;

            if ((tris[2] == turno ? 1 : 0) + (tris[4] == turno ? 1 : 0) + (tris[6] == turno ? 1 : 0) == 2 &&
                (tris[2] == '-' ? 1 : 0) + (tris[4] == '-' ? 1 : 0) + (tris[6] == '-' ? 1 : 0) == 1)
                minacce++;

            return minacce >= 2;
        }

        private char GetVincitoreTris(string boardState, int numTris)
        {
            if (ControllaVittoriaTris(boardState, numTris, 'X')) return 'X';
            if (ControllaVittoriaTris(boardState, numTris, 'O')) return 'O';
            return '-';
        }

        private bool TrisPieno(string boardState, int numTris)
        {
            int offset = numTris * 9;
            for (int i = 0; i < 9; i++)
            {
                if (boardState[offset + i] == '-')
                    return false;
            }
            return true;
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