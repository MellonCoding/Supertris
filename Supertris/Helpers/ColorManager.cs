using System.Drawing;

namespace Supertris
{
    internal class ColorManager
    {
        // ── Sfondo ────────────────────────────────────────────────
        public readonly Color coloreSfondo          = Color.FromArgb( 65,  65,  65); // #414141

        // ── Tris / celle ──────────────────────────────────────────
        public readonly Color coloreTrisNormale     = Color.FromArgb( 46,  46,  46); // #2e2e2e
        public readonly Color coloreTrisAttivo      = Color.FromArgb( 74,  15,  24); // #4a0f18
        public readonly Color coloreTrisCompletato  = Color.FromArgb( 30,  30,  30); // #1e1e1e

        // ── Hover ─────────────────────────────────────────────────
        public readonly Color coloreHover           = Color.FromArgb(138,  16,  48); // #8a1030

        // ── X / O ─────────────────────────────────────────────────
        public readonly Color coloreX               = Color.FromArgb(108,   4,  36); // #6c0424 rosso sangue
        public readonly Color coloreO               = Color.FromArgb( 46,  95, 138); // #2e5f8a blu acciaio

        // ── Testo ─────────────────────────────────────────────────
        public readonly Color coloreTesto           = Color.FromArgb(255,  77, 109); // #ff4d6d rosso neon
        public readonly Color coloreTestoSpento     = Color.FromArgb( 85,  85,  85); // #555555 completato

        // ── Bordi ─────────────────────────────────────────────────
        public readonly Color coloreBordo           = Color.FromArgb(108,   4,  36); // #6c0424 accento
        public readonly Color coloreBordoAttivo     = Color.FromArgb(217, 160, 165); // #d9a0a5 rosa antico
        public readonly Color coloreBordoCompletato = Color.FromArgb( 68,  68,  68); // #444444 spento
    }
}
