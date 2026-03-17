using System.Drawing.Drawing2D;

namespace Supertris
{
    public class CustomButton : Button
    {
        private static readonly ColorManager _cm = new ColorManager();

        private bool _isHovered = false;
        private bool _isPressed = false;

        public CustomButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = LoadFont("ShureTechMono Nerd Font", 9.5f);
            Cursor = Cursors.Hand;
            Size = new Size(150, 40);
            UseVisualStyleBackColor = false;

            // Necessario per trasparenza angoli
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { _isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { _isPressed = false; Invalidate(); };
        }



        // Carica il font richiesto; se non installato cade su Consolas poi Segoe UI
        private static Font LoadFont(string name, float size)
        {
            foreach (string candidate in new[] { name, "Consolas", "Segoe UI" })
            {
                try
                {
                    var f = new Font(candidate, size, FontStyle.Regular, GraphicsUnit.Point);
                    if (f.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                        return f;
                    f.Dispose();
                }
                catch { }
            }
            return SystemFonts.DefaultFont;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            Rectangle inner = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = 6;

            // 1. Dipingi gli angoli col colore del parent così spariscono
            Color parentBg = Parent?.BackColor ?? _cm.coloreSfondo;
            using (SolidBrush cornerBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(cornerBrush, rect);
            }

            Color bg = _isPressed ? _cm.coloreX
                         : _isHovered ? _cm.coloreHover
                         : _cm.coloreX;
            Color border = _isHovered ? _cm.coloreBordoAttivo : _cm.coloreBordo;

            // 2. Disegna il bottone arrotondato sopra
            using (GraphicsPath path = RoundedRect(inner, radius))
            {
                using (SolidBrush brush = new SolidBrush(bg))
                    g.FillPath(brush, path);

                using (Pen pen = new Pen(border, 1f))
                    g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, inner, _cm.coloreTesto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private GraphicsPath RoundedRect(Rectangle b, int r)
        {
            int d = r * 2;
            var path = new GraphicsPath();
            path.AddArc(b.X, b.Y, d, d, 180, 90);
            path.AddArc(b.Right - d, b.Y, d, d, 270, 90);
            path.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            path.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}