/// <summary>
///     Supertris:
/// 
///     metodi:
///     Tris  -> crea e popola il campo, segna che non é pieno e il vincitore (nessuno)
///     Mossa -> input    (che player vuole fare la mossa, posizione x, posizione y) 
///           -> processo (controlla che la mossa alla posizione data sia valida, se lo é stampa la mossa sul campo e controla se qualcuno ha vinto)
///           -> output   (true se a mossa é andata a buon fine - false se la mossa non é valida o il tris é pieno/vinto)
///     CheckVittoria -> controlla se dopo una
///     
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
            {
                for (int col = 0; col < 3; col++)
                {
                    bigField[row, col] = new Tris();
                }
            }
        }

        // -------------------------------- HELPERS -------------------------------- //

        public string GetCampo()
        {
            string array = "";

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    array += bigField[row, col].GetCampo();
                }
            }

            return array;
        }

        public bool IsTrisCompletato(int riga, int colonna)
        {
            return (bigField[riga, colonna].GetVincitore() != '-') ? true : false;
        }

        // -------------------------------- END HELPERS ---------------------------- // 

        public bool MakeMove(char giocaore, int rigaBigField, int colonnaBigField, int riga, int colonna)
        {
            // partita finita = false
            if (winner != '-') { return false; }

            // Prova a fare la mossa nel mini-tris specificato
            return bigField[rigaBigField, colonnaBigField].MakeMove(giocaore, riga, colonna);
        }

        public char CheckWin()
        {
            // Controllo vittoria righe
            for (int row = 0; row < 3; row++)
            {
                if (winner != '-') break;

                char first = bigField[row, 0].GetVincitore();
                if (first != '-' &&
                    first == bigField[row, 1].GetVincitore() &&
                    first == bigField[row, 2].GetVincitore())
                {
                    winner = first;
                }
            }

            // Controllo vittoria colonne
            for (int col = 0; col < 3; col++)
            {
                if (winner != '-') break;

                char first = bigField[0, col].GetVincitore();
                if (first != '-' &&
                    first == bigField[1, col].GetVincitore() &&
                    first == bigField[2, col].GetVincitore())
                {
                    winner = first;
                }
            }

            // Controllo diagonale principale (\)
            if (winner == '-')
            {
                char first = bigField[0, 0].GetVincitore();
                if (first != '-' &&
                    first == bigField[1, 1].GetVincitore() &&
                    first == bigField[2, 2].GetVincitore())
                {
                    winner = first;
                }
            }

            // Controllo diagonale secondaria (/)
            if (winner == '-')
            {
                char first = bigField[0, 2].GetVincitore();
                if (first != '-' &&
                    first == bigField[1, 1].GetVincitore() &&
                    first == bigField[2, 0].GetVincitore())
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
            return bigField[row, col].CheckWin() != '-';
        }
    }
}
