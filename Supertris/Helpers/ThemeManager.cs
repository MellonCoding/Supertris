namespace Supertris.Helpers
{
    /// <summary>
    /// Punto di accesso globale al tema visivo dell'applicazione.
    /// 
    /// UTILIZZO:
    ///   ThemeManager.Instance.ColoreSfondo
    ///   ThemeManager.Instance.FontBottone
    /// 
    /// CAMBIO TEMA A RUNTIME:
    ///   ThemeManager.Instance.SetThema(new LightTheme());
    ///   // tutti i form che ridisegnano useranno il nuovo tema
    /// </summary>
    internal sealed class ThemeManager
    {
        // ── Singleton ─────────────────────────────────────────────
        private static readonly ThemeManager _instance = new ThemeManager();
        public  static ThemeManager Instance => _instance;

        // Costruttore privato — impedisce new ThemeManager() dall'esterno
        private ThemeManager()
        {
            _tema = new HellsingTheme();
        }

        // ── Tema attivo ───────────────────────────────────────────
        private ITheme _tema;

        public ITheme Tema => _tema;

        /// <summary>
        /// Cambia il tema a runtime.
        /// Dopo la chiamata tutti i controlli che ridisegnano
        /// useranno automaticamente il nuovo tema.
        /// </summary>
        public void SetThema(ITheme nuovoTema)
        {
            _tema = nuovoTema;
        }

        // ── Shortcut colori (evita .Tema. in mezzo) ───────────────
        public Color ColoreSfondo          => _tema.ColoreSfondo;
        public Color ColoreTrisNormale     => _tema.ColoreTrisNormale;
        public Color ColoreTrisAttivo      => _tema.ColoreTrisAttivo;
        public Color ColoreTrisCompletato  => _tema.ColoreTrisCompletato;
        public Color ColoreHover           => _tema.ColoreHover;
        public Color ColoreX               => _tema.ColoreX;
        public Color ColoreO               => _tema.ColoreO;
        public Color ColoreTesto           => _tema.ColoreTesto;
        public Color ColoreTestoSpento     => _tema.ColoreTestoSpento;
        public Color ColoreBordo           => _tema.ColoreBordo;
        public Color ColoreBordoAttivo     => _tema.ColoreBordoAttivo;
        public Color ColoreBordoCompletato => _tema.ColoreBordoCompletato;
        public Font  FontBottone           => _tema.FontBottone;
        public Font  FontUI                => _tema.FontUI;
        public Font  FontTitolo            => _tema.FontTitolo;

        // ── Helper font (usato dai temi per caricare i font) ──────
        /// <summary>
        /// Carica il font richiesto con fallback automatico.
        /// Se il font non è installato, ricade su Consolas poi Segoe UI.
        /// </summary>
        public static Font LoadFont(string name, float size, FontStyle style = FontStyle.Regular)
        {
            foreach (string candidate in new[] { name, "Consolas", "Segoe UI" })
            {
                try
                {
                    var f = new Font(candidate, size, style, GraphicsUnit.Point);
                    if (f.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                        return f;
                    f.Dispose();
                }
                catch { }
            }
            return SystemFonts.DefaultFont;
        }
    }
}
