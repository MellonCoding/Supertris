namespace Supertris.AI
{
    internal interface IBot
    {
        (int numTris, int row, int col)? CalcolaMossa(string campo, int trisObbligatoria, char turno);

        /// <summary>
        /// Notifica il bot del risultato di una partita
        /// </summary>
        /// <param name="vittoria">True se il bot ha vinto, False se ha perso, null se pareggio</param>
        void NotificaRisultatoPartita(bool? vittoria);

        /// <summary>
        /// Resetta lo stato interno del bot per una nuova partita
        /// </summary>
        void ResetPartita();
    }
}
