namespace Supertris.Helpers
{
    /// <summary>
    /// Wrapper retrocompatibile su ThemeManager.
    /// Tutti i riferimenti a ColorManager continuano a funzionare
    /// senza modifiche — leggono dal tema attivo via ThemeManager.
    /// 
    /// MIGRAZIONE: sostituisci gradualmente con ThemeManager.Instance
    /// e rimuovi questo file quando non ci sono più riferimenti.
    /// </summary>
    internal class ColorManager
    {
        private static ThemeManager TM => ThemeManager.Instance;

        public Color coloreSfondo          => TM.ColoreSfondo;
        public Color coloreTrisNormale     => TM.ColoreTrisNormale;
        public Color coloreTrisAttivo      => TM.ColoreTrisAttivo;
        public Color coloreTrisCompletato  => TM.ColoreTrisCompletato;
        public Color coloreHover           => TM.ColoreHover;
        public Color coloreX               => TM.ColoreX;
        public Color coloreO               => TM.ColoreO;
        public Color coloreTesto           => TM.ColoreTesto;
        public Color coloreTestoSpento     => TM.ColoreTestoSpento;
        public Color coloreBordo           => TM.ColoreBordo;
        public Color coloreBordoAttivo     => TM.ColoreBordoAttivo;
        public Color coloreBordoCompletato => TM.ColoreBordoCompletato;
    }
}
