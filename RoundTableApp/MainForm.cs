using System.Drawing.Drawing2D;
using System.Text;

namespace RoundTableApp;

// Left: the picture (people on a circle, friendships as lines, the table).
// Right: controls and the text result.
public class MainForm : Form
{
    static readonly string[] AllNames =
    {
        "Айдар", "Бота", "Ержан", "Дана", "Асел", "Ермек",
        "Мадина", "Нұрлан", "Салтанат", "Тимур", "Зарина", "Арман"
    };

    const int MaxN = 12;

    int n = 8;
    bool[,] adj = new bool[MaxN, MaxN];
    int[] seat;                 // seating order (null = nobody can be seated)
    int[] slot = new int[MaxN]; // slot[p] = place of person p around the circle
    int selected = -1;          // person selected by the first click
    readonly Random rnd = new Random();

    readonly Panel canvas = new DoubleBufferedPanel();
    readonly NumericUpDown numPeople = new NumericUpDown();
    readonly TextBox txtInfo = new TextBox();
    readonly Button btnSave = new Button();

    class DoubleBufferedPanel : Panel { public DoubleBufferedPanel() { DoubleBuffered = true; } }

    public MainForm()
    {
        Text = "Дөңгелек үстел: екі жағында да дос отыруы үшін";
        BackColor = Color.FromArgb(15, 15, 15);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(960, 650);

        canvas.SetBounds(10, 10, 610, 630);
        canvas.BackColor = Color.FromArgb(15, 15, 15);
        canvas.Paint += (s, e) => DrawCanvas(e.Graphics);
        canvas.MouseClick += Canvas_MouseClick;
        canvas.Cursor = Cursors.Hand;

        int x = 635, w = 315;

        var lbl = new Label { Text = "Адам саны:", ForeColor = Color.White, AutoSize = true, Left = x, Top = 16 };
        numPeople.SetBounds(x + 90, 12, 60, 26);
        numPeople.Minimum = 3;
        numPeople.Maximum = MaxN;
        numPeople.Value = n;
        numPeople.ValueChanged += (s, e) => { n = (int)numPeople.Value; selected = -1; Recompute(); };

        var btnRing = MakeButton("Мысал: үстелді айнала (отыруға болады)", x, 48, w, Color.FromArgb(22, 163, 74));
        btnRing.Click += (s, e) => { Clear(); for (int i = 0; i < n; i++) Link(i, (i + 1) % n); Recompute(); };

        var btnPath = MakeButton("Мысал: тізбек (отыруға болмайды)", x, 84, w, Color.FromArgb(180, 83, 9));
        btnPath.Click += (s, e) => { Clear(); for (int i = 0; i + 1 < n; i++) Link(i, i + 1); Recompute(); };

        var btnRandom = MakeButton("Кездейсоқ достық", x, 120, w, Color.FromArgb(37, 99, 235));
        btnRandom.Click += (s, e) =>
        {
            Clear();
            for (int a = 0; a < n; a++) for (int b = a + 1; b < n; b++) if (rnd.Next(2) == 0) Link(a, b);
            Recompute();
        };

        var btnAll = MakeButton("Бәрі дос", x, 156, w, Color.FromArgb(60, 60, 60));
        btnAll.Click += (s, e) => { Clear(); for (int a = 0; a < n; a++) for (int b = a + 1; b < n; b++) Link(a, b); Recompute(); };

        var btnClear = MakeButton("Достықты тазалау", x, 192, w, Color.FromArgb(60, 60, 60));
        btnClear.Click += (s, e) => { Clear(); Recompute(); };

        var hint = new Label
        {
            Text = "Достық қосу/өшіру: екі адамды ретімен басыңыз.",
            ForeColor = Color.Silver, Left = x, Top = 232, Width = w, Height = 34
        };

        txtInfo.Multiline = true;
        txtInfo.ReadOnly = true;
        txtInfo.ScrollBars = ScrollBars.Vertical;
        txtInfo.BackColor = Color.FromArgb(30, 30, 30);
        txtInfo.ForeColor = Color.White;
        txtInfo.BorderStyle = BorderStyle.FixedSingle;
        txtInfo.Font = new Font("Segoe UI", 9.5f);
        txtInfo.SetBounds(x, 270, w, 335); // a bit shorter, the save button is below

        btnSave = MakeButton("Нәтижені файлға сақтау", x, 610, w, Color.FromArgb(60, 60, 60));
        btnSave.Click += (s, e) => SaveReport();

        Controls.AddRange(new Control[] { canvas, lbl, numPeople, btnRing, btnPath, btnRandom, btnAll, btnClear, hint, txtInfo, btnSave });

        for (int i = 0; i < n; i++) Link(i, (i + 1) % n); // start with the ring: everything works
        Link(0, 3); Link(2, 6);
        Recompute();
    }

    void SaveReport()
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "Text file (*.txt)|*.txt",
            FileName = "round_table_result.txt"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            File.WriteAllText(dlg.FileName, txtInfo.Text, Encoding.UTF8);
    }

    static Button MakeButton(string text, int x, int y, int w, Color back)
    {
        var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = Color.White };
        b.SetBounds(x, y, w, 30);
        return b;
    }

    void Link(int a, int b) { if (a != b) adj[a, b] = adj[b, a] = true; }
    void Clear() { Array.Clear(adj); }

    // ------------------------------------------------------------ logic
    void Recompute()
    {
        seat = Seating.FindCycle(adj, n);
        for (int p = 0; p < MaxN; p++) slot[p] = p;
        if (seat != null)
            for (int k = 0; k < n; k++) slot[seat[k]] = k;

        txtInfo.Text = BuildReport();
        canvas.Invalidate();
    }

    string BuildReport()
    {
        string[] names = AllNames;
        var sb = new StringBuilder();

        // the result goes first: it is the most important part
        sb.AppendLine("НӘТИЖЕ:");
        if (seat != null)
        {
            sb.AppendLine("✔ Отырғызуға БОЛАДЫ. Үстел басынан бастап:");
            var line = new List<string>();
            foreach (int p in seat) line.Add(names[p]);
            sb.AppendLine(string.Join(" – ", line) + " – (қайтадан " + names[seat[0]] + ")");
            sb.AppendLine("Әр адамның екі жағында да оның досы отыр.");
        }
        else
        {
            sb.AppendLine("✘ Отырғызуға БОЛМАЙДЫ.");
            sb.AppendLine("Себебі: " + Seating.WhyNot(adj, n, names));
        }

        int min = int.MaxValue;
        var list = new StringBuilder();
        for (int v = 0; v < n; v++)
        {
            var friends = new List<string>();
            for (int u = 0; u < n; u++) if (adj[v, u]) friends.Add(names[u]);
            min = Math.Min(min, friends.Count);
            list.AppendLine(" " + names[v] + " (" + friends.Count + "): " + (friends.Count == 0 ? "-" : string.Join(", ", friends)));
        }

        sb.AppendLine();
        sb.AppendLine("Жеткілікті шарттар:");
        sb.AppendLine(" Дирак: әр адамның кемінде n/2 = " + (n / 2.0).ToString("0.#") + " досы бар."
                      + " Ең аз дос саны: " + min + " → " + (Seating.Dirac(adj, n) ? "ОРЫНДАЛДЫ" : "орындалмады"));
        sb.AppendLine(" Оре: дос емес әр жұптың достар қосындысы ≥ n = " + n + " → "
                      + (Seating.Ore(adj, n) ? "ОРЫНДАЛДЫ" : "орындалмады"));
        sb.AppendLine(" (Шарттар жеткілікті, бірақ қажетті емес: орындалмаса да отыруға болуы мүмкін.)");

        sb.AppendLine();
        sb.AppendLine("Достары (адам: дос саны):");
        sb.Append(list);
        return sb.ToString();
    }

    // ------------------------------------------------------------ drawing
    const float Cx = 305, Cy = 315, R = 215, NodeR = 26;

    PointF Pos(int person)
    {
        double a = -Math.PI / 2 + 2 * Math.PI * slot[person] / n;
        return new PointF(Cx + (float)(R * Math.Cos(a)), Cy + (float)(R * Math.Sin(a)));
    }

    bool NextToEachOther(int a, int b)
    {
        if (seat == null) return false;
        int d = Math.Abs(slot[a] - slot[b]);
        return d == 1 || d == n - 1;
    }

    void DrawCanvas(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        // table in the middle
        if (seat != null)
        {
            using var tb = new SolidBrush(Color.FromArgb(181, 136, 99));
            g.FillEllipse(tb, Cx - 85, Cy - 85, 170, 170);
            using var f = new Font("Segoe UI", 14, FontStyle.Bold);
            g.DrawString("Стол", f, Brushes.White, new RectangleF(Cx - 85, Cy - 85, 170, 170), center);
        }

        // friendships that are not neighbours at the table: thin grey lines
        using (var thin = new Pen(Color.FromArgb(110, 110, 110), 2))
            for (int a = 0; a < n; a++)
                for (int b = a + 1; b < n; b++)
                    if (adj[a, b] && !NextToEachOther(a, b)) g.DrawLine(thin, Pos(a), Pos(b));

        // neighbours at the table: thick green lines
        if (seat != null)
        {
            using var thick = new Pen(Color.FromArgb(34, 197, 94), 6);
            for (int k = 0; k < n; k++) g.DrawLine(thick, Pos(seat[k]), Pos(seat[(k + 1) % n]));
        }

        // people
        using var nodeFont = new Font("Segoe UI", 13, FontStyle.Bold);
        using var nameFont = new Font("Segoe UI", 10, FontStyle.Bold);
        for (int p = 0; p < n; p++)
        {
            var c = Pos(p);
            var fill = seat != null ? Color.FromArgb(240, 218, 181) : Color.FromArgb(190, 190, 190);
            using var b = new SolidBrush(fill);
            g.FillEllipse(b, c.X - NodeR, c.Y - NodeR, NodeR * 2, NodeR * 2);
            using var pen = new Pen(p == selected ? Color.Gold : Color.Black, p == selected ? 4 : 2);
            g.DrawEllipse(pen, c.X - NodeR, c.Y - NodeR, NodeR * 2, NodeR * 2);

            string inside = seat != null ? (slot[p] + 1).ToString() : AllNames[p].Substring(0, 1);
            g.DrawString(inside, nodeFont, Brushes.Black, new RectangleF(c.X - NodeR, c.Y - NodeR, NodeR * 2, NodeR * 2), center);

            // name outside the circle
            float dx = c.X - Cx, dy = c.Y - Cy;
            if (Math.Abs(dx) < 40)      // top / bottom: label above or below the circle
            {
                float ly = dy < 0 ? c.Y - NodeR - 14 : c.Y + NodeR + 14;
                g.DrawString(AllNames[p], nameFont, Brushes.White, new RectangleF(c.X - 60, ly - 12, 120, 24), center);
            }
            else if (dx > 0)            // right side: label to the right
            {
                var left = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                g.DrawString(AllNames[p], nameFont, Brushes.White, new RectangleF(c.X + NodeR + 6, c.Y - 12, 100, 24), left);
            }
            else                        // left side: label to the left
            {
                var right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                g.DrawString(AllNames[p], nameFont, Brushes.White, new RectangleF(c.X - NodeR - 106, c.Y - 12, 100, 24), right);
            }
        }

        if (seat == null)
        {
            using var f = new Font("Segoe UI", 12, FontStyle.Bold);
            g.DrawString("Отыруға болмайды", f, Brushes.OrangeRed, new RectangleF(Cx - 110, Cy - 15, 220, 30), center);
        }
    }

    void Canvas_MouseClick(object sender, MouseEventArgs e)
    {
        int hit = -1;
        for (int p = 0; p < n; p++)
        {
            var c = Pos(p);
            float dx = e.X - c.X, dy = e.Y - c.Y;
            if (dx * dx + dy * dy <= (NodeR + 4) * (NodeR + 4)) { hit = p; break; }
        }
        if (hit < 0) { selected = -1; canvas.Invalidate(); return; }

        if (selected < 0) selected = hit;
        else if (selected == hit) selected = -1;
        else
        {
            adj[selected, hit] = adj[hit, selected] = !adj[selected, hit];
            selected = -1;
            Recompute();
            return;
        }
        canvas.Invalidate();
    }
}
