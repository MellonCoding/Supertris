namespace Supertris.AI
{
    /// <summary>
    /// Interfaccia comune per tutti i tipi di bot
    /// </summary>
    internal interface IBot
    {
        /// <summary>
        /// Calcola la prossima mossa del bot
        /// </summary>
        /// <param name="boardState">Stato corrente della board (81 caratteri: 9 tris x 9 celle)</param>
        /// <param name="trisObbligatoria">Tris dove si deve giocare (-1 se mossa libera)</param>
        /// <param name="turno">Simbolo del bot ('X' o 'O')</param>
        /// <returns>Tupla (numTris, row, col) della mossa scelta</returns>
        (int numTris, int row, int col)? CalcolaMossa(string boardState, int trisObbligatoria, char turno);

        /// <summary>
        /// Notifica il bot del risultato di una partita
        /// </summary>
        /// <param name="haVinto">True se il bot ha vinto, False se ha perso, null se pareggio</param>
        void NotificaRisultatoPartita(bool? haVinto);

        /// <summary>
        /// Resetta lo stato interno del bot per una nuova partita
        /// </summary>
        void ResetPartita();
    }
}