/// <summary>
/// Tris:
/// 
/// metodi:
/// Tris  -> crea e popola il campo, segna che non é pieno e il vincitore (nessuno)
/// Mossa -> input        (che player vuole fare la mossa, posizione x, posizione y) 
///           -> processo (controlla che la mossa alla posizione data sia valida, se lo é stampa la mossa sul campo e controla se qualcuno ha vinto)
///           -> output   (true se a mossa é andata a buon fine - false se la mossa non é valida o il tris é pieno/vinto)
/// CheckVittoria -> controlla se dopo una mossa uno dei 2 player ha vinto
///     
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
            {
                for (int col = 0; col < 3; col++)
                {
                    field[row, col] = '-';
                }
            }
        }

        // -------------------------------- HELPERS -------------------------------- //

        public char GetVincitore() => winner;
        // public bool GetPieno() => full;
        public char GetCella(int x, int y) => field[x, y];
        public string GetCampo()
        {
            string campo = "";

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    campo += field[row, col];
                }
            }

            return campo;
        }

        public bool GetPieno()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (field[row, col] == '-')
                        return false;  // Trovata una cella vuota
                }
            }
            return true;  // Tutte le celle sono occupate
        }

        // -------------------------------- FINE HELPERS -------------------------------- //

        public bool MakeMove(char player, int row, int col)
        {
            if (field[row, col] == '-' && winner == '-')
            {
                field[row, col] = player;
                CheckWin();
                return true;
            }

            return false;
        }

        public char CheckWin()
        {
            // Controllo righe
            for (int row = 0; row < 3; row++)
            {
                if (winner != '-') break;

                char first = field[row, 0];
                if (first != '-' &&
                    first == field[row, 1] &&
                    first == field[row, 2])
                {
                    winner = first;
                }
            }

            // Controllo colonne
            for (int col = 0; col < 3; col++)
            {
                if (winner != '-') break;

                char first = field[0, col];
                if (first != '-' &&
                    first == field[1, col] &&
                    first == field[2, col])
                {
                    winner = first;
                }
            }

            // Diagonale (\)
            if (winner == '-')
            {
                char first = field[0, 0];
                if (first != '-' &&
                    first == field[1, 1] &&
                    first == field[2, 2])
                {
                    winner = first;
                }
            }

            // Diagonale (/)
            if (winner == '-')
            {
                char first = field[0, 2];
                if (first != '-' &&
                    first == field[1, 1] &&
                    first == field[2, 0])
                {
                    winner = first;
                }
            }

            return winner;
        }
    }
}