/// <summary>
/// Tris — mini-tris 3x3.
/// 
/// metodi:
/// Tris     -> crea e popola il campo, segna che non é pieno e il vincitore (nessuno)
/// MakeMove -> input        (player, posizione x, posizione y)
///          -> processo     (controlla validità, piazza, controlla vittoria)
///          -> output       (true se mossa ok, false se cella occupata / tris vinto)
/// CheckWin -> controlla se qualcuno ha vinto dopo una mossa
/// Flush    -> azzera il tris (modalità flush: tris pieno = si resetta)
/// </summary>

namespace Supertris.Classes
{
    internal class Tris
    {
        private char[,] field;
        private bool full;
        private char winner;

        public Tris()
        {
            field = new char[3, 3];
            full = false;
            winner = '-';

            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    field[row, col] = '-';
        }

        // ── Helpers ───────────────────────────────────────────────

        public char GetVincitore() => winner;
        public char GetCella(int x, int y) => field[x, y];
        public bool GetPieno() => full;

        public string GetCampo()
        {
            string campo = "";
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    campo += field[row, col];
            return campo;
        }

        // ── Mossa ─────────────────────────────────────────────────

        public bool MakeMove(char player, int row, int col)
        {
            if (field[row, col] != '-' || winner != '-') return false;

            field[row, col] = player;
            CheckWin();

            // Aggiorna full solo se non è già stato settato
            if (!full) full = CalcolaSepieno();

            return true;
        }

        // ── Vittoria ──────────────────────────────────────────────

        public char CheckWin()
        {
            if (winner != '-') return winner;

            // Righe
            for (int row = 0; row < 3; row++)
            {
                char first = field[row, 0];
                if (first != '-' && first == field[row, 1] && first == field[row, 2])
                {
                    winner = first;
                    return winner;
                }
            }

            // Colonne
            for (int col = 0; col < 3; col++)
            {
                char first = field[0, col];
                if (first != '-' && first == field[1, col] && first == field[2, col])
                {
                    winner = first;
                    return winner;
                }
            }

            // Diagonale (\)
            {
                char first = field[0, 0];
                if (first != '-' && first == field[1, 1] && first == field[2, 2])
                {
                    winner = first;
                    return winner;
                }
            }

            // Diagonale (/)
            {
                char first = field[0, 2];
                if (first != '-' && first == field[1, 1] && first == field[2, 0])
                {
                    winner = first;
                    return winner;
                }
            }

            return winner;
        }

        // ── Flush ─────────────────────────────────────────────────

        /// <summary>
        /// Resetta completamente il tris (usato in modalità Flush).
        /// Azzera campo, vincitore e stato pieno.
        /// </summary>
        public void Flush()
        {
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    field[row, col] = '-';

            full = false;
            winner = '-';
        }

        // ── Privati ───────────────────────────────────────────────

        private bool CalcolaSepieno()
        {
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    if (field[row, col] == '-') return false;
            return true;
        }
    }
}