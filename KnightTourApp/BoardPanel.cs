using System.Drawing.Drawing2D;

namespace KnightTourApp;

// Draws the chessboard (A-H on top, 8-1 on the left) and the knight's tour.
public class BoardPanel : Panel
{
    public const int Cell = 64; // small enough for a 768-px-high laptop screen
    public const int BoardLeft = 50;
    public const int BoardTop = 40;

    static readonly Color Light = Color.FromArgb(240, 218, 181);
    static readonly Color Dark = Color.FromArgb(181, 136, 99);
    static readonly Color Green = Color.FromArgb(22, 163, 74);
    static readonly Color Blue = Color.FromArgb(37, 99, 235);

    public KnightTour Tour;      // null = no tour yet
    public int Shown;            // how many steps are visible (65 = also the way back home)
    public bool ShowPath = true;

    public event Action<int, int> CellClicked;

    public BoardPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(15, 15, 15);
        Size = new Size(BoardLeft + 8 * Cell + 12, BoardTop + 8 * Cell + 12);
        Cursor = Cursors.Hand;
    }

    static Rectangle CellRect(int x, int y)
        => new Rectangle(BoardLeft + x * Cell, BoardTop + (7 - y) * Cell, Cell, Cell);

    static PointF CellCenter(int x, int y)
    {
        var r = CellRect(x, y);
        return new PointF(r.X + Cell / 2f, r.Y + Cell / 2f);
    }

    // saves the board with the numbers and the path as a PNG picture
    public void SaveImage(string path)
    {
        using var bmp = new Bitmap(Width, Height);
        DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.X < BoardLeft || e.Y < BoardTop) return;
        int x = (e.X - BoardLeft) / Cell;
        int row = (e.Y - BoardTop) / Cell;
        if (x > 7 || row > 7) return;
        CellClicked?.Invoke(x, 7 - row);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var labelFont = new Font("Segoe UI", 11, FontStyle.Bold);
        using var numFont = new Font("Segoe UI", 13, FontStyle.Bold);

        // coordinates
        for (int i = 0; i < 8; i++)
        {
            g.DrawString(((char)('A' + i)).ToString(), labelFont, Brushes.White,
                new RectangleF(BoardLeft + i * Cell, 8, Cell, 28), center);
            g.DrawString((8 - i).ToString(), labelFont, Brushes.White,
                new RectangleF(4, BoardTop + i * Cell, BoardLeft - 8, Cell), center);
        }

        int visible = Tour == null ? 0 : Math.Min(Shown, KnightTour.N * KnightTour.N);

        // squares
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Color c = (x + y) % 2 == 0 ? Dark : Light;
                if (Tour != null)
                {
                    int s = Tour.Step[x, y];
                    if (s == 1 && visible >= 1) c = Green;
                    else if (s == 64 && visible >= 64) c = Blue;
                }
                var r = CellRect(x, y);
                using var b = new SolidBrush(c);
                g.FillRectangle(b, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
            }
        }

        if (Tour == null)
        {
            using var f = new Font("Segoe UI", 15, FontStyle.Bold);
            var rect = new RectangleF(BoardLeft, BoardTop + 3 * Cell, 8 * Cell, 2 * Cell);
            using var back = new SolidBrush(Color.FromArgb(200, 15, 15, 15));
            g.FillRectangle(back, rect);
            g.DrawString("Кез келген клетканы басыңыз!\nАт сол жерден бастап, сол жерге қайтады.",
                f, Brushes.White, rect, center);
            return;
        }

        // path
        if (ShowPath)
        {
            for (int s = 1; s < visible; s++)
            {
                var a = CellCenter(Tour.Order[s].x, Tour.Order[s].y);
                var b = CellCenter(Tour.Order[s + 1].x, Tour.Order[s + 1].y);
                int t = s * 255 / 64;
                using var pen = new Pen(Color.FromArgb(190, t, 60, 255 - t), 3);
                g.DrawLine(pen, a, b);
            }
        }

        // the way back home (move 64 -> 1)
        if (Shown >= 65)
        {
            var a = CellCenter(Tour.Order[64].x, Tour.Order[64].y);
            var b = CellCenter(Tour.Order[1].x, Tour.Order[1].y);
            using var pen = new Pen(Color.LimeGreen, 5) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor };
            g.DrawLine(pen, a, b);
        }

        // numbers
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                int s = Tour.Step[x, y];
                if (s > visible) continue;
                var r = CellRect(x, y);
                if (ShowPath && s != 1 && s != 64)
                {
                    Color c = (x + y) % 2 == 0 ? Dark : Light;
                    using var back = new SolidBrush(c);
                    g.FillEllipse(back, r.X + Cell / 2f - 15, r.Y + Cell / 2f - 15, 30, 30);
                }
                bool white = (s == 1 || (s == 64 && visible >= 64) || (x + y) % 2 == 0);
                g.DrawString(s.ToString(), numFont, white ? Brushes.White : Brushes.Black, r, center);
            }
        }

        // knight: yellow frame + horse on the current square
        if (visible >= 1)
        {
            var cur = Shown >= 65 ? Tour.Order[1] : Tour.Order[visible];
            var r = CellRect(cur.x, cur.y);
            using var pen = new Pen(Color.Gold, 4);
            g.DrawRectangle(pen, r.X + 2, r.Y + 2, r.Width - 4, r.Height - 4);
            using var horse = new Font("Segoe UI Symbol", 20, FontStyle.Bold);
            g.DrawString("♞", horse, Brushes.Black, r.X + 4, r.Y + 1);
        }
    }
}
