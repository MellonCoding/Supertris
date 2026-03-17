using System.Drawing.Drawing2D;
using Supertris.Helpers;

namespace Supertris
{
    /// <summary>
    /// Bottone custom con stile Hellsing.
    /// Colori e font vengono letti da ThemeManager.Instance
    /// così si aggiornano automaticamente al cambio tema.
    /// </summary>
    public class CustomButton : Button
    {
        private static ThemeManager TM => ThemeManager.Instance;

        private bool _isHovered = false;
        private bool _isPressed = false;

        public CustomButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = TM.FontBottone;
            Cursor = Cursors.Hand;
            Size = new Size(150, 40);
            UseVisualStyleBackColor = false;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { _isPressed = true; Invalidate(); };
            MouseUp += (s, e) => { _isPressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            Rectangle inner = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = 6;

            // Dipingi gli angoli col colore del parent così spariscono
            Color parentBg = Parent?.BackColor ?? TM.ColoreSfondo;
            using (SolidBrush cornerBrush = new SolidBrush(parentBg))
                g.FillRectangle(cornerBrush, rect);

            Color bg = _isPressed ? TM.ColoreX
                         : _isHovered ? TM.ColoreHover
                         : TM.ColoreX;
            Color border = _isHovered ? TM.ColoreBordoAttivo : TM.ColoreBordo;

            using (GraphicsPath path = RoundedRect(inner, radius))
            {
                using (SolidBrush brush = new SolidBrush(bg))
                    g.FillPath(brush, path);

                using (Pen pen = new Pen(border, 1f))
                    g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, inner, TM.ColoreTesto,
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
