namespace Supertris.Helpers
{
    /// <summary>
    /// Tema ispirato a Hellsing: rosso sangue su nero gotico.
    /// Implementa ITheme con la palette definita durante il design.
    /// </summary>
    internal class HellsingTheme : ITheme
    {
        // ── Sfondo ────────────────────────────────────────────────
        public Color ColoreSfondo          => Color.FromArgb( 54,  57,  63); // #414141

        // ── Tris / celle ──────────────────────────────────────────
        public Color ColoreTrisNormale     => Color.FromArgb( 46,  46,  46); // #2e2e2e
        public Color ColoreTrisAttivo      => Color.FromArgb( 74,  15,  24); // #4a0f18
        public Color ColoreTrisCompletato  => Color.FromArgb( 30,  30,  30); // #1e1e1e

        // ── Hover ─────────────────────────────────────────────────
        public Color ColoreHover           => Color.FromArgb(138,  16,  48); // #8a1030

        // ── X / O ─────────────────────────────────────────────────
        public Color ColoreX               => Color.FromArgb(108,   4,  36); // #6c0424 rosso sangue
        public Color ColoreO               => Color.FromArgb( 46,  95, 138); // #2e5f8a blu acciaio

        // ── Testo ─────────────────────────────────────────────────
        public Color ColoreTesto           => Color.FromArgb(255,  77, 109); // #ff4d6d rosso neon
        public Color ColoreTestoSpento     => Color.FromArgb( 85,  85,  85); // #555555

        // ── Bordi ─────────────────────────────────────────────────
        public Color ColoreBordo           => Color.FromArgb(108,   4,  36); // #6c0424
        public Color ColoreBordoAttivo     => Color.FromArgb(217, 160, 165); // #d9a0a5 rosa antico
        public Color ColoreBordoCompletato => Color.FromArgb( 68,  68,  68); // #444444

        // ── Font ──────────────────────────────────────────────────
        public Font FontBottone => ThemeManager.LoadFont("ShureTechMono Nerd Font", 9.5f);
        public Font FontUI      => ThemeManager.LoadFont("ShureTechMono Nerd Font", 10f);
        public Font FontTitolo  => ThemeManager.LoadFont("ShureTechMono Nerd Font", 14f, FontStyle.Bold);
    }
}
