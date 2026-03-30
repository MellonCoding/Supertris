namespace Supertris.Helpers
{
    /// <summary>
    /// Centralizza i colori del tema scuro dell'interfaccia.
    /// 
    /// COLORI:
    /// - Sfondo: grigio molto scuro (#1E1E1E)
    /// - Tris normale/attivo/completato: variazioni di grigio
    /// - X: rosso (#DC5050)
    /// - O: blu (#5096DC)
    /// - Hover: grigio chiaro
    /// - Testo: bianco
    /// 
    /// SCOPO: consistenza visiva in tutte le form
    /// </summary>
    
    internal class ColorManager
    {
        // Colori del tema
        public readonly Color coloreSfondo = Color.FromArgb(30, 30, 30);
        public readonly Color coloreTrisNormale = Color.FromArgb(45, 45, 48);
        public readonly Color coloreTrisAttivo = Color.FromArgb(60, 100, 60);
        public readonly Color coloreTrisCompletato = Color.FromArgb(50, 50, 50);
        public readonly Color coloreHover = Color.FromArgb(70, 70, 73);
        public readonly Color coloreX = Color.FromArgb(220, 80, 80);      // Rosso
        public readonly Color coloreO = Color.FromArgb(80, 150, 220);     // Blu
        public readonly Color coloreTesto = Color.White;
        public readonly Color coloreBordo = Color.FromArgb(80, 80, 80);
    }
}
