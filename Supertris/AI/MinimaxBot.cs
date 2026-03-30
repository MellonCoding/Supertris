namespace Supertris.AI
{
    /// <summary>
    /// Bot che usa l'algoritmo Minimax con potatura alpha-beta.
    ///
    /// FUNZIONAMENTO:
    /// - Esplora albero delle mosse possibili fino a profondità 7
    /// - Per ogni mossa simula le risposte dell'avversario
    /// - Assume che l'avversario giochi in modo ottimale
    /// - Valuta posizioni con euristica (centro, angoli, vittorie)
    /// - Potatura alpha-beta per velocizzare (taglia rami inutili)
    /// - Transposition table con Zobrist hashing per evitare ricalcoli
    ///
    /// NON IMPARA: sempre stessa strategia, forte da subito
    /// </summary>
    internal class MinimaxBot : IBot
    {
        // -------- CONFIGURAZIONE --------
        private const int MAX_DEPTH = 9;

        // -------- PESI EURISTICI --------
        private const int PesoTrisVinto = 1000;
        private const int PesoLinea = 5;
        private const int PesoCentro = 5;
        private const int PesoAngolo = 2;
        private const int PesoCentroGlobale = 50;
        private const int PesoAngoloGlobale = 20;

        // -------- STRUTTURE DATI --------
        private struct Mossa
        {
            public int Row;   // coordinata globale Y (0-8)
            public int Col;   // coordinata globale X (0-8)
        }

        // -------- STATO BOARD --------
        // gameState[row, col]: 0=vuoto, 1=bot, -1=player
        // Le coordinate sono GLOBALI (0-8 su entrambi gli assi)
        private readonly int[,] gameState = new int[9, 9];

        // Cache dei vincitori per tris (aggiornata ad ogni ParseBoardState)
        // trisWinner[t]: 0=nessuno, 1=bot, -1=player
        private readonly int[] trisWinner = new int[9];

        // -------- TRANSPOSITION TABLE --------
        private const int TT_EXACT = 0;
        private const int TT_LOWER = 1;
        private const int TT_UPPER = 2;

        private struct TTEntry
        {
            public long Hash;
            public int Depth;
            public int Value;
            public byte Type;
        }

        // 1 << 22 = 4M entry, bilancio memoria/collisioni
        private const int TT_SIZE = 1 << 22;
        private const int TT_MASK = TT_SIZE - 1;
        private readonly TTEntry[] tt = new TTEntry[TT_SIZE];

        // -------- ZOBRIST HASHING --------
        // zobrist[row, col, pezzo]: pezzo=0 → valore fisico +1 (bot), pezzo=1 → valore fisico -1 (player)
        // L'indice NON dipende da chi è il bot corrente, ma dal valore fisico nella cella
        private readonly long[,,] zobrist = new long[9, 9, 2];
        private long currentHash;

        // -------- COSTRUTTORE --------
        public MinimaxBot()
        {
            // Inizializza tabella Zobrist con valori pseudo-casuali deterministici
            // Usiamo un seed fisso per riproducibilità
            var rng = new Random(0x5EED);
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    zobrist[r, c, 0] = NextLong(rng);
                    zobrist[r, c, 1] = NextLong(rng);
                }
        }

        private static long NextLong(Random rng)
        {
            var buf = new byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }

        // ======================================================
        // INTERFACCIA IBot
        // ======================================================

        public (int numTris, int row, int col)? CalcolaMossa(string boardState, int trisObbligatoria, char turno)
        {
            // 1. Aggiorna gameState e currentHash dal boardState
            ParseBoardState(boardState, turno);

            // 2. Resetta TT per questa ricerca
            Array.Clear(tt, 0, tt.Length);

            // 3. Esegui minimax dalla root
            Mossa best = EseguiMinimaxRoot(trisObbligatoria);

            // 4. Converti coordinate globali → (numTris, row, col) locali
            int numTris = (best.Row / 3) * 3 + (best.Col / 3);
            int localRow = best.Row % 3;
            int localCol = best.Col % 3;

            return (numTris, localRow, localCol);
        }

        public void NotificaRisultatoPartita(bool? haVinto) { }

        public void ResetPartita()
        {
            Array.Clear(gameState, 0, gameState.Length);
            Array.Clear(trisWinner, 0, trisWinner.Length);
            currentHash = 0;
        }

        // ======================================================
        // PARSING
        // ======================================================

        private void ParseBoardState(string boardState, char turno)
        {
            // turno è il carattere del bot ('X' o 'O')
            char botChar = turno;
            char playerChar = (turno == 'X') ? 'O' : 'X';

            currentHash = 0;
            int index = 0;

            for (int t = 0; t < 9; t++)
            {
                int tRow = t / 3;
                int tCol = t % 3;
                int offsetR = tRow * 3;
                int offsetC = tCol * 3;

                for (int cell = 0; cell < 9; cell++)
                {
                    int globalR = offsetR + cell / 3;
                    int globalC = offsetC + cell % 3;
                    char ch = boardState[index++];

                    if (ch == botChar)
                    {
                        gameState[globalR, globalC] = 1;
                        currentHash ^= zobrist[globalR, globalC, 0]; // +1 → indice 0
                    }
                    else if (ch == playerChar)
                    {
                        gameState[globalR, globalC] = -1;
                        currentHash ^= zobrist[globalR, globalC, 1]; // -1 → indice 1
                    }
                    else
                    {
                        gameState[globalR, globalC] = 0;
                    }
                }

                // Aggiorna cache vincitore tris
                trisWinner[t] = CalcolaVincitoreTris(t);
            }
        }

        // ======================================================
        // ROOT MINIMAX
        // Gestita separatamente per tracciare la mossa migliore
        // senza passare ref attraverso la TT
        // ======================================================

        private Mossa EseguiMinimaxRoot(int trisObbligatoria)
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            int best = int.MinValue;
            Mossa bestMossa = default;

            // Determina il range di celle valide per la prima mossa
            GetRange(trisObbligatoria, out int rMin, out int rMax, out int cMin, out int cMax);

            for (int r = rMin; r < rMax; r++)
            {
                for (int c = cMin; c < cMax; c++)
                {
                    if (gameState[r, c] != 0) continue;

                    // Salta celle in tris già vinti
                    int t = (r / 3) * 3 + (c / 3);
                    if (trisWinner[t] != 0) continue;

                    // Piazza mossa bot (+1)
                    PlacePiece(r, c, 1);

                    Mossa mossa = new Mossa { Row = r, Col = c };
                    int val = Minimax(MAX_DEPTH - 1, false, mossa, alpha, beta);

                    // Rimuovi mossa
                    RemovePiece(r, c, 1);

                    if (val > best)
                    {
                        best = val;
                        bestMossa = mossa;
                    }

                    alpha = Math.Max(alpha, val);
                    // Nessun taglio alla root: vogliamo la mossa migliore assoluta
                }
            }

            return bestMossa;
        }

        // ======================================================
        // MINIMAX RICORSIVO
        // ======================================================

        private int Minimax(int depth, bool isBot, Mossa ultimaMossa, int alpha, int beta)
        {
            // --- Lookup TT ---
            int ttIdx = (int)(currentHash & TT_MASK);
            ref TTEntry entry = ref tt[ttIdx];

            if (entry.Hash == currentHash && entry.Depth >= depth)
            {
                if (entry.Type == TT_EXACT) return entry.Value;
                if (entry.Type == TT_LOWER) alpha = Math.Max(alpha, entry.Value);
                if (entry.Type == TT_UPPER) beta = Math.Min(beta, entry.Value);
                if (alpha >= beta) return entry.Value;
            }

            // --- Controlla vittoria globale ---
            int vittoria = CalcolaVittoriaGlobale();
            if (vittoria == 1) return int.MaxValue - (MAX_DEPTH - depth);
            if (vittoria == -1) return int.MinValue + (MAX_DEPTH - depth);

            // --- Profondità massima raggiunta ---
            if (depth == 0) return FunzioneValutativa();

            // --- Determina range celle per questa mossa ---
            // La mossa successiva deve andare nel tris corrispondente
            // alla posizione LOCALE dell'ultima mossa giocata
            int nextTris = (ultimaMossa.Row % 3) * 3 + (ultimaMossa.Col % 3);

            int rMin, rMax, cMin, cMax;
            if (trisWinner[nextTris] != 0 || IsTrisCompleto(nextTris))
            {
                // Tris obbligatorio pieno o vinto: mossa libera su tutta la board
                rMin = 0; rMax = 9;
                cMin = 0; cMax = 9;
            }
            else
            {
                // Mossa obbligatoria nel tris nextTris
                int tRow = (nextTris / 3) * 3;
                int tCol = (nextTris % 3) * 3;
                rMin = tRow; rMax = tRow + 3;
                cMin = tCol; cMax = tCol + 3;
            }

            int origAlpha = alpha;
            int origBeta = beta;
            int bestVal = isBot ? int.MinValue : int.MaxValue;
            bool anyMove = false;

            for (int r = rMin; r < rMax; r++)
            {
                for (int c = cMin; c < cMax; c++)
                {
                    if (gameState[r, c] != 0) continue;

                    int t = (r / 3) * 3 + (c / 3);
                    if (trisWinner[t] != 0) continue; // cella in tris già vinto

                    anyMove = true;
                    Mossa mossa = new Mossa { Row = r, Col = c };

                    if (isBot)
                    {
                        PlacePiece(r, c, 1);
                        int val = Minimax(depth - 1, false, mossa, alpha, beta);
                        RemovePiece(r, c, 1);

                        bestVal = Math.Max(bestVal, val);
                        alpha = Math.Max(alpha, val);
                    }
                    else
                    {
                        PlacePiece(r, c, -1);
                        int val = Minimax(depth - 1, true, mossa, alpha, beta);
                        RemovePiece(r, c, -1);

                        bestVal = Math.Min(bestVal, val);
                        beta = Math.Min(beta, val);
                    }

                    if (beta <= alpha) goto CutOff;
                }
            }

        CutOff:

            // Nessuna mossa disponibile → valuta posizione corrente
            if (!anyMove) return FunzioneValutativa();

            // --- Salva in TT ---
            entry.Hash = currentHash;
            entry.Depth = depth;
            entry.Value = bestVal;
            entry.Type = (byte)(bestVal <= origAlpha ? TT_UPPER
                               : bestVal >= origBeta ? TT_LOWER
                                                      : TT_EXACT);

            return bestVal;
        }

        // ======================================================
        // HELPERS MOSSA (aggiornano gameState, trisWinner, hash)
        // ======================================================

        private void PlacePiece(int r, int c, int piece)
        {
            gameState[r, c] = piece;
            // piece==1 → indice zobrist 0, piece==-1 → indice zobrist 1
            currentHash ^= zobrist[r, c, piece == 1 ? 0 : 1];

            // Aggiorna cache vincitore del tris che contiene (r,c)
            int t = (r / 3) * 3 + (c / 3);
            trisWinner[t] = CalcolaVincitoreTris(t);
        }

        private void RemovePiece(int r, int c, int piece)
        {
            // XOR inverso (identico a PlacePiece per Zobrist)
            currentHash ^= zobrist[r, c, piece == 1 ? 0 : 1];
            gameState[r, c] = 0;

            // Aggiorna cache vincitore (ora il tris potrebbe non essere più vinto)
            int t = (r / 3) * 3 + (c / 3);
            trisWinner[t] = CalcolaVincitoreTris(t);
        }

        // ======================================================
        // RANGE CELLE
        // ======================================================

        private void GetRange(int trisObbligatoria, out int rMin, out int rMax, out int cMin, out int cMax)
        {
            if (trisObbligatoria == -1 || trisWinner[trisObbligatoria] != 0 || IsTrisCompleto(trisObbligatoria))
            {
                // Mossa libera
                rMin = 0; rMax = 9;
                cMin = 0; cMax = 9;
            }
            else
            {
                int tRow = (trisObbligatoria / 3) * 3;
                int tCol = (trisObbligatoria % 3) * 3;
                rMin = tRow; rMax = tRow + 3;
                cMin = tCol; cMax = tCol + 3;
            }
        }

        // ======================================================
        // CONTROLLO VITTORIA
        // ======================================================

        private int CalcolaVincitoreTris(int t)
        {
            int tRow = (t / 3) * 3;
            int tCol = (t % 3) * 3;
            int first, center;

            // Righe
            for (int dr = 0; dr < 3; dr++)
            {
                first = gameState[tRow + dr, tCol];
                if (first != 0
                    && first == gameState[tRow + dr, tCol + 1]
                    && first == gameState[tRow + dr, tCol + 2])
                    return first;
            }

            // Colonne
            for (int dc = 0; dc < 3; dc++)
            {
                first = gameState[tRow, tCol + dc];
                if (first != 0
                    && first == gameState[tRow + 1, tCol + dc]
                    && first == gameState[tRow + 2, tCol + dc])
                    return first;
            }

            // Diagonali
            center = gameState[tRow + 1, tCol + 1];
            if (center != 0
                && center == gameState[tRow, tCol]
                && center == gameState[tRow + 2, tCol + 2])
                return center;
            if (center != 0
                && center == gameState[tRow, tCol + 2]
                && center == gameState[tRow + 2, tCol])
                return center;

            return 0;
        }

        private int CalcolaVittoriaGlobale()
        {
            int first, center;

            // Righe meta-board
            for (int row = 0; row < 3; row++)
            {
                first = trisWinner[row * 3];
                if (first != 0
                    && first == trisWinner[row * 3 + 1]
                    && first == trisWinner[row * 3 + 2])
                    return first;
            }

            // Colonne meta-board
            for (int col = 0; col < 3; col++)
            {
                first = trisWinner[col];
                if (first != 0
                    && first == trisWinner[col + 3]
                    && first == trisWinner[col + 6])
                    return first;
            }

            // Diagonali meta-board
            center = trisWinner[4];
            if (center != 0 && center == trisWinner[0] && center == trisWinner[8]) return center;
            if (center != 0 && center == trisWinner[2] && center == trisWinner[6]) return center;

            return 0;
        }

        private bool IsTrisCompleto(int t)
        {
            if (trisWinner[t] != 0) return true;

            int tRow = (t / 3) * 3;
            int tCol = (t % 3) * 3;

            for (int dr = 0; dr < 3; dr++)
                for (int dc = 0; dc < 3; dc++)
                    if (gameState[tRow + dr, tCol + dc] == 0)
                        return false;

            return true;
        }

        // ======================================================
        // FUNZIONE VALUTATIVA
        // ======================================================

        private int FunzioneValutativa()
        {
            int[] valoriTris = new int[9];
            for (int t = 0; t < 9; t++)
                valoriTris[t] = ValutaSingoloTris(t);
            return ValutaTrisGlobale(valoriTris);
        }

        private int ValutaSingoloTris(int t)
        {
            if (trisWinner[t] != 0) return trisWinner[t] * PesoTrisVinto;

            int tRow = (t / 3) * 3;
            int tCol = (t % 3) * 3;
            int score = 0;

            // Righe
            for (int dr = 0; dr < 3; dr++)
                score += ValutaLinea(
                    gameState[tRow + dr, tCol],
                    gameState[tRow + dr, tCol + 1],
                    gameState[tRow + dr, tCol + 2]);

            // Colonne
            for (int dc = 0; dc < 3; dc++)
                score += ValutaLinea(
                    gameState[tRow, tCol + dc],
                    gameState[tRow + 1, tCol + dc],
                    gameState[tRow + 2, tCol + dc]);

            // Diagonali
            score += ValutaLinea(gameState[tRow, tCol], gameState[tRow + 1, tCol + 1], gameState[tRow + 2, tCol + 2]);
            score += ValutaLinea(gameState[tRow, tCol + 2], gameState[tRow + 1, tCol + 1], gameState[tRow + 2, tCol]);

            // Bonus posizionali
            score += gameState[tRow + 1, tCol + 1] * PesoCentro;
            score += gameState[tRow, tCol] * PesoAngolo;
            score += gameState[tRow, tCol + 2] * PesoAngolo;
            score += gameState[tRow + 2, tCol] * PesoAngolo;
            score += gameState[tRow + 2, tCol + 2] * PesoAngolo;

            return score;
        }

        private int ValutaTrisGlobale(int[] v)
        {
            int score = 0;

            // Righe meta-board
            for (int row = 0; row < 3; row++)
                score += ValutaLineaGlobale(v[row * 3], v[row * 3 + 1], v[row * 3 + 2]);

            // Colonne meta-board
            for (int col = 0; col < 3; col++)
                score += ValutaLineaGlobale(v[col], v[col + 3], v[col + 6]);

            // Diagonali meta-board
            score += ValutaLineaGlobale(v[0], v[4], v[8]);
            score += ValutaLineaGlobale(v[2], v[4], v[6]);

            // Bonus centro e angoli meta-board
            if (v[4] == PesoTrisVinto) score += PesoCentroGlobale;
            else if (v[4] == -PesoTrisVinto) score -= PesoCentroGlobale;

            foreach (int idx in new[] { 0, 2, 6, 8 })
            {
                if (v[idx] == PesoTrisVinto) score += PesoAngoloGlobale;
                else if (v[idx] == -PesoTrisVinto) score -= PesoAngoloGlobale;
            }

            return score;
        }

        private int ValutaLineaGlobale(int a, int b, int c)
        {
            int botVinti = 0;
            int playerVinti = 0;

            if (a == PesoTrisVinto) botVinti++;
            else if (a == -PesoTrisVinto) playerVinti++;

            if (b == PesoTrisVinto) botVinti++;
            else if (b == -PesoTrisVinto) playerVinti++;

            if (c == PesoTrisVinto) botVinti++;
            else if (c == -PesoTrisVinto) playerVinti++;

            // Linea bloccata
            if (botVinti > 0 && playerVinti > 0) return 0;

            int score = 0;
            if (botVinti == 2) score += 2000;
            else if (playerVinti == 2) score -= 2000;

            // Contributo locale dei tris non ancora vinti
            score += (a + b + c);

            return score;
        }

        private int ValutaLinea(int a, int b, int c)
        {
            bool haVuoto = (a == 0 || b == 0 || c == 0);
            int somma = a + b + c;
            if (!haVuoto || somma == 0) return 0;
            return somma * PesoLinea;
        }
    }
}