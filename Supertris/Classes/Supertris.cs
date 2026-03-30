namespace Supertris.Classes
{
    /// <summary>
    /// Gestisce la griglia completa di 9 mini-tris disposti 3x3.
    /// 
    /// FUNZIONAMENTO:
    /// - Contiene una matrice 3x3 di oggetti Tris
    /// - Coordina le mosse tra i vari mini-tris
    /// - Controlla vittoria globale: 3 mini-tris vinti in fila
    /// - Genera lo stato completo (81 caratteri) per i bot
    /// - Verifica se un tris è completato (vinto o pieno)
    /// 
    /// VITTORIA: come il tris normale ma con mini-tris invece di celle
    /// </summary>
    internal class SupertrisClass
    {
        private Tris[,] arrayOfMiniBoard;
        private char winner;

        public SupertrisClass()
        {
            arrayOfMiniBoard = new Tris[3, 3];
            winner = '-';

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    arrayOfMiniBoard[row, col] = new Tris();
                }
            }
        }

        // -------------------------------- HELPERS -------------------------------- //

        /// <summary>
        /// Ottiene lo stato completo della board come stringa
        /// Formato: 81 caratteri (9 tris x 9 celle), '-' per vuoto, 'X' o 'O' per occupato
        /// </summary>
        public string GetBoardState()
        {
            char[] state = new char[81];
            int index = 0;

            for (int trisRow = 0; trisRow < 3; trisRow++)
            {
                for (int trisCol = 0; trisCol < 3; trisCol++)
                {
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            state[index++] = arrayOfMiniBoard[trisRow, trisCol].GetCell(row, col);
                        }
                    }
                }
            }

            return new string(state);
        }

        /// <summary>
        /// Controlla se un mini-tris è completato (vinto da qualcuno o pieno)
        /// </summary>
        public bool IsTrisCompleted(int trisRow, int trisCol)
        {
            // Se qualcuno ha vinto questo tris, è completato
            if (arrayOfMiniBoard[trisRow, trisCol].wonBy() != '-')
                return true;

            // Se il tris è pieno (tutte le celle occupate), è completato
            return arrayOfMiniBoard[trisRow, trisCol].IsFull();
        }

        // -------------------------------- END HELPERS ---------------------------- // 

        /// <summary>
        /// Esegue una mossa su un mini-tris specifico
        /// </summary>
        public bool MakeMove(char player, int trisRow, int trisCol, int row, int col)
        {
            if (winner != '-')
            {
                // Se c'è già un vincitore globale, non si possono fare altre mosse
                return false;
            }

            // Prova a fare la mossa nel mini-tris specificato
            return arrayOfMiniBoard[trisRow, trisCol].MakeMove(player, row, col);
        }

        /// <summary>
        /// Controlla se c'è un vincitore globale del Super Tris
        /// </summary>
        public char CheckWin()
        {
            // Controllo vittoria righe
            for (int row = 0; row < 3; row++)
            {
                if (winner != '-') break;

                char first = arrayOfMiniBoard[row, 0].wonBy();
                if (first != '-' &&
                    first == arrayOfMiniBoard[row, 1].wonBy() &&
                    first == arrayOfMiniBoard[row, 2].wonBy())
                {
                    winner = first;
                }
            }

            // Controllo vittoria colonne
            for (int col = 0; col < 3; col++)
            {
                if (winner != '-') break;

                char first = arrayOfMiniBoard[0, col].wonBy();
                if (first != '-' &&
                    first == arrayOfMiniBoard[1, col].wonBy() &&
                    first == arrayOfMiniBoard[2, col].wonBy())
                {
                    winner = first;
                }
            }

            // Controllo diagonale principale (\)
            if (winner == '-')
            {
                char first = arrayOfMiniBoard[0, 0].wonBy();
                if (first != '-' &&
                    first == arrayOfMiniBoard[1, 1].wonBy() &&
                    first == arrayOfMiniBoard[2, 2].wonBy())
                {
                    winner = first;
                }
            }

            // Controllo diagonale secondaria (/)
            if (winner == '-')
            {
                char first = arrayOfMiniBoard[0, 2].wonBy();
                if (first != '-' &&
                    first == arrayOfMiniBoard[1, 1].wonBy() &&
                    first == arrayOfMiniBoard[2, 0].wonBy())
                {
                    winner = first;
                }
            }

            return winner;
        }

        /// <summary>
        /// Controlla se un mini-tris specifico è stato vinto
        /// </summary>
        public bool CheckWinMiniBoard(int row, int col)
        {
            return arrayOfMiniBoard[row, col].CheckWin() != '-';
        }
    }
}