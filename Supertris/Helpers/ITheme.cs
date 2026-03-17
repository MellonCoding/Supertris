namespace Supertris.Helpers
{
    /// <summary>
    /// Contratto per tutti i temi visivi dell'applicazione.
    /// Ogni tema implementa questa interfaccia e definisce
    /// colori, font e dimensioni bordi.
    /// </summary>
    internal interface ITheme
    {
        // ── Sfondo ────────────────────────────────────────────────
        Color ColoreSfondo          { get; }

        // ── Tris / celle ──────────────────────────────────────────
        Color ColoreTrisNormale     { get; }
        Color ColoreTrisAttivo      { get; }
        Color ColoreTrisCompletato  { get; }

        // ── Hover ─────────────────────────────────────────────────
        Color ColoreHover           { get; }

        // ── X / O ─────────────────────────────────────────────────
        Color ColoreX               { get; }
        Color ColoreO               { get; }

        // ── Testo ─────────────────────────────────────────────────
        Color ColoreTesto           { get; }
        Color ColoreTestoSpento     { get; }

        // ── Bordi ─────────────────────────────────────────────────
        Color ColoreBordo           { get; }
        Color ColoreBordoAttivo     { get; }
        Color ColoreBordoCompletato { get; }

        // ── Font ──────────────────────────────────────────────────
        Font  FontBottone           { get; }
        Font  FontUI                { get; }
        Font  FontTitolo            { get; }
    }
}
