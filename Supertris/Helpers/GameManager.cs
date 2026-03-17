namespace Supertris.Helpers
{
    /// <summary>
    /// Coordina il flusso del gioco e gestisce le regole del Super Tris.
    ///
    /// FUNZIONAMENTO:
    /// - Mantiene lo stato del gioco (board, turno, tris obbligatorio)
    /// - Valida le mosse: controlla se rispettano il tris obbligatorio
    /// - Calcola il prossimo tris obbligatorio dopo ogni mossa
    /// - Gestisce due modalità per tris pieno:
    ///     Nullo -> tris pieno = non giocabile, mossa libera
    ///     Flush -> tris pieno = si resetta, torna giocabile
    /// - Alterna i turni tra X e O
    /// - Verifica vittoria globale
    /// </summary>

    /// <summary>
    /// Comportamento quando un mini-tris è pieno senza vincitore.
    /// Nullo -> il tris rimane pieno e non giocabile (mossa libera)
    /// Flush -> il tris viene resettato e torna giocabile
    /// </summary>
    public enum ModalitaTrisPieno
    {
        Nullo,
        Flush
    }

    internal class GameManager
    {
        private Classes.Supertris board;
        private int ProssimoTrisObbligatorio;
        private char turnoCorrente;

        public ModalitaTrisPieno ModalitaTrisPieno { get; set; } = ModalitaTrisPieno.Nullo;

        public GameManager()
        {
            board = new Classes.Supertris();
            turnoCorrente = 'X';
            ProssimoTrisObbligatorio = -1;
        }

        // ── Helpers ───────────────────────────────────────────────

        public char GetTurno() => turnoCorrente;
        public void CambiaTurno() => turnoCorrente = turnoCorrente == 'X' ? 'O' : 'X';
        public int GetProssimaTrisObbligatoria() => ProssimoTrisObbligatorio;
        public string GetBoardState() => board.GetCampo();
        public char CheckWin() => board.CheckWin();

        // ── Mossa ─────────────────────────────────────────────────

        public bool MakeMove(int numTris, int row, int col)
        {
            int trisRow = numTris / 3;
            int trisCol = numTris % 3;

            // Valida tris obbligatoria
            if (ProssimoTrisObbligatorio != -1 && ProssimoTrisObbligatorio != numTris)
                return false;

            bool esito = board.MakeMove(turnoCorrente, trisRow, trisCol, row, col);
            if (!esito) return false;

            // Calcola il prossimo tris obbligatorio
            int prossimoNumTris = row * 3 + col;
            int prossimoTrisRow = prossimoNumTris / 3;
            int prossimoTrisCol = prossimoNumTris % 3;

            if (board.IsTrisCompletato(prossimoTrisRow, prossimoTrisCol))
            {
                switch (ModalitaTrisPieno)
                {
                    case ModalitaTrisPieno.Flush:
                        // Resetta il tris e mandaci il giocatore
                        board.FlushTris(prossimoTrisRow, prossimoTrisCol);
                        ProssimoTrisObbligatorio = prossimoNumTris;
                        break;

                    case ModalitaTrisPieno.Nullo:
                    default:
                        // Tris completato non giocabile → mossa libera
                        ProssimoTrisObbligatorio = -1;
                        break;
                }
            }
            else
            {
                ProssimoTrisObbligatorio = prossimoNumTris;
            }

            return true;
        }

        // ── Flush helper per UI ───────────────────────────────────

        /// <summary>
        /// Restituisce true se l'ultimo tris obbligatorio è stato
        /// appena flushato — usato da GameForm per aggiornare la griglia.
        /// </summary>
        public bool UltimoTrisFlushato(int prossimoNumTris)
        {
            if (ModalitaTrisPieno != ModalitaTrisPieno.Flush) return false;
            int r = prossimoNumTris / 3;
            int c = prossimoNumTris % 3;
            return !board.IsTrisCompletato(r, c);
        }
    }
}