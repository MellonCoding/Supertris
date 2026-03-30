namespace Supertris.Classes
{
    /// <summary>
    /// Gestisce un singolo mini-tris 3x3.
    /// 
    /// FUNZIONAMENTO:
    /// - Mantiene una griglia 3x3 di celle (char: '-' vuoto, 'X', 'O')
    /// - Valida e esegue le mosse verificando che la cella sia vuota
    /// - Controlla vittorie dopo ogni mossa (righe, colonne, diagonali)
    /// - Una volta vinto, non accetta più mosse
    /// - Può verificare se è pieno (tutte celle occupate)
    /// 
    /// STATO: immutabile dopo vittoria
    /// </summary>
    
    internal class Tris
    {
        private char[,] miniBoard;
        private char winner;

        public Tris()
        {
            miniBoard = new char[3, 3];
            winner = '-';

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    miniBoard[row, col] = '-';
                }
            }
        }

        // -------------------------------- HELPERS -------------------------------- //

        public char wonBy() => winner;

        /// <summary>
        /// Ottiene il contenuto di una cella
        /// </summary>
        public char GetCell(int row, int col)
        {
            return miniBoard[row, col];
        }

        /// <summary>
        /// Controlla se il tris è pieno (tutte le celle occupate)
        /// </summary>
        public bool IsFull()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (miniBoard[row, col] == '-')
                        return false;  // Trovata una cella vuota
                }
            }
            return true;  // Tutte le celle sono occupate
        }

        // -------------------------------- END HELPERS ---------------------------- // 

        /// <summary>
        /// Esegue una mossa sul mini-tris
        /// </summary>
        public bool MakeMove(char player, int row, int col)
        {
            // La cella deve essere vuota e non deve esserci già un vincitore
            if (miniBoard[row, col] == '-' && winner == '-')
            {
                miniBoard[row, col] = player;
                CheckWin();  // Aggiorna lo stato del vincitore dopo ogni mossa
                return true;
            }

            return false;
        }

        /// <summary>
        /// Controlla se c'è un vincitore nel mini-tris
        /// </summary>
        public char CheckWin()
        {
            // Controllo righe
            for (int row = 0; row < 3; row++)
            {
                if (winner != '-') break;

                char first = miniBoard[row, 0];
                if (first != '-' &&
                    first == miniBoard[row, 1] &&
                    first == miniBoard[row, 2])
                {
                    winner = first;
                }
            }

            // Controllo colonne
            for (int col = 0; col < 3; col++)
            {
                if (winner != '-') break;

                char first = miniBoard[0, col];
                if (first != '-' &&
                    first == miniBoard[1, col] &&
                    first == miniBoard[2, col])
                {
                    winner = first;
                }
            }

            // Diagonale principale (\)
            if (winner == '-')
            {
                char first = miniBoard[0, 0];
                if (first != '-' &&
                    first == miniBoard[1, 1] &&
                    first == miniBoard[2, 2])
                {
                    winner = first;
                }
            }

            // Diagonale secondaria (/)
            if (winner == '-')
            {
                char first = miniBoard[0, 2];
                if (first != '-' &&
                    first == miniBoard[1, 1] &&
                    first == miniBoard[2, 0])
                {
                    winner = first;
                }
            }

            return winner;
        }
    }
}