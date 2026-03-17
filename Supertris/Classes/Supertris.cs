/// <summary>
/// Supertris — griglia 3x3 di mini-tris.
///
/// metodi:
/// MakeMove         -> esegue una mossa nel mini-tris specificato
/// CheckWin         -> controlla vittoria globale sulla meta-griglia
/// IsTrisCompletato -> true se il mini-tris è vinto OPPURE pieno (pareggio)
/// FlushTris        -> resetta un mini-tris specifico (modalità Flush)
/// GetCampo         -> restituisce lo stato completo come stringa 81 char
/// </summary>

namespace Supertris.Classes
{
    internal class Supertris
    {
        private Tris[,] bigField;
        private char winner;

        public Supertris()
        {
            bigField = new Tris[3, 3];
            winner = '-';

            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    bigField[row, col] = new Tris();
        }

        // ── Helpers ───────────────────────────────────────────────

        public string GetCampo()
        {
            string array = "";
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    array += bigField[row, col].GetCampo();
            return array;
        }

        /// <summary>
        /// Un tris è "completato" (non giocabile) se è stato vinto
        /// oppure se è pieno senza un vincitore (pareggio nel mini-tris).
        /// </summary>
        public bool IsTrisCompletato(int riga, int colonna)
        {
            var tris = bigField[riga, colonna];
            return tris.GetVincitore() != '-' || tris.GetPieno();
        }

        // ── Mossa ─────────────────────────────────────────────────

        public bool MakeMove(char giocatore, int rigaBigField, int colonnaBigField, int riga, int colonna)
        {
            if (winner != '-') return false;
            return bigField[rigaBigField, colonnaBigField].MakeMove(giocatore, riga, colonna);
        }

        // ── Flush ─────────────────────────────────────────────────

        /// <summary>
        /// Resetta il mini-tris specificato (modalità Flush).
        /// </summary>
        public void FlushTris(int riga, int colonna)
        {
            bigField[riga, colonna].Flush();
        }

        // ── Vittoria ──────────────────────────────────────────────

        public char CheckWin()
        {
            if (winner != '-') return winner;

            // Righe
            for (int row = 0; row < 3; row++)
            {
                char first = bigField[row, 0].GetVincitore();
                if (first != '-'
                    && first == bigField[row, 1].GetVincitore()
                    && first == bigField[row, 2].GetVincitore())
                {
                    winner = first;
                    return winner;
                }
            }

            // Colonne
            for (int col = 0; col < 3; col++)
            {
                char first = bigField[0, col].GetVincitore();
                if (first != '-'
                    && first == bigField[1, col].GetVincitore()
                    && first == bigField[2, col].GetVincitore())
                {
                    winner = first;
                    return winner;
                }
            }

            // Diagonale (\)
            {
                char first = bigField[0, 0].GetVincitore();
                if (first != '-'
                    && first == bigField[1, 1].GetVincitore()
                    && first == bigField[2, 2].GetVincitore())
                {
                    winner = first;
                    return winner;
                }
            }

            // Diagonale (/)
            {
                char first = bigField[0, 2].GetVincitore();
                if (first != '-'
                    && first == bigField[1, 1].GetVincitore()
                    && first == bigField[2, 0].GetVincitore())
                {
                    winner = first;
                    return winner;
                }
            }

            return winner;
        }
    }
}