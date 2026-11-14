using System.Text;

namespace KnightTourApp;

public class MainForm : Form
{
    const int SideWidth = 280; // result panel on the right

    readonly BoardPanel board = new BoardPanel();
    readonly Button btnRandom = new Button();
    readonly CheckBox chkAnim = new CheckBox();
    readonly CheckBox chkPath = new CheckBox();
    readonly TrackBar trackSpeed = new TrackBar();
    readonly Label lblStatus = new Label();
    readonly TextBox txtResult = new TextBox();
    readonly Button btnCopy = new Button();
    readonly Button btnSave = new Button();
    readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
    readonly Random rnd = new Random();

    public MainForm()
    {
        Text = "Атпен жүру: кез келген клеткадан бастап, сол клеткаға қайту";
        BackColor = Color.FromArgb(15, 15, 15);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        int totalWidth = board.Width + SideWidth;

        // top bar
        var top = new Panel { Left = 0, Top = 0, Width = totalWidth, Height = 46, BackColor = Color.FromArgb(30, 30, 30) };

        StyleButton(btnRandom, "Кездейсоқ бастау", Color.FromArgb(37, 99, 235));
        btnRandom.SetBounds(10, 8, 150, 30);
        btnRandom.Click += (s, e) => StartTour(rnd.Next(8), rnd.Next(8));

        chkAnim.Text = "Анимация";
        chkAnim.Checked = true;
        chkAnim.SetBounds(175, 12, 95, 24);
        chkAnim.ForeColor = Color.White;

        chkPath.Text = "Жол сызығы";
        chkPath.Checked = true;
        chkPath.SetBounds(275, 12, 105, 24);
        chkPath.ForeColor = Color.White;
        chkPath.CheckedChanged += (s, e) => { board.ShowPath = chkPath.Checked; board.Invalidate(); };

        var lblSpeed = new Label { Text = "Жылдамдық", ForeColor = Color.White, AutoSize = true, Left = 395, Top = 14 };
        trackSpeed.SetBounds(470, 8, 100, 30);
        trackSpeed.Minimum = 1;
        trackSpeed.Maximum = 10;
        trackSpeed.Value = 5;
        trackSpeed.TickStyle = TickStyle.None;
        trackSpeed.ValueChanged += (s, e) => timer.Interval = SpeedToInterval();

        top.Controls.AddRange(new Control[] { btnRandom, chkAnim, chkPath, lblSpeed, trackSpeed });

        // board
        board.Location = new Point(0, top.Height);
        board.CellClicked += StartTour;

        // result panel (right side)
        int sx = board.Width + 8;
        int sw = SideWidth - 16;
        var lblResult = new Label
        {
            Text = "Нәтиже",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Left = sx,
            Top = top.Height + 10
        };

        txtResult.Multiline = true;
        txtResult.ReadOnly = true;
        txtResult.ScrollBars = ScrollBars.Vertical;
        txtResult.BackColor = Color.FromArgb(30, 30, 30);
        txtResult.ForeColor = Color.White;
        txtResult.BorderStyle = BorderStyle.FixedSingle;
        txtResult.Font = new Font("Consolas", 10);
        txtResult.SetBounds(sx, top.Height + 40, sw, board.Height - 40 - 44);
        txtResult.Text = "Тақтадағы кез келген клетканы басыңыз.\r\nНәтиже осы жерде шығады.";

        StyleButton(btnCopy, "Көшіру", Color.FromArgb(60, 60, 60));
        btnCopy.SetBounds(sx, top.Height + board.Height - 36, sw / 2 - 4, 30);
        btnCopy.Click += (s, e) => { if (board.Tour != null && board.Shown >= 65) Clipboard.SetText(txtResult.Text); };

        StyleButton(btnSave, "Файлға сақтау", Color.FromArgb(60, 60, 60));
        btnSave.SetBounds(sx + sw / 2 + 4, top.Height + board.Height - 36, sw / 2 - 4, 30);
        btnSave.Click += (s, e) => SaveResult();

        // status line
        lblStatus.SetBounds(10, top.Height + board.Height, totalWidth - 20, 44);
        lblStatus.ForeColor = Color.White;
        lblStatus.Font = new Font("Segoe UI", 10);
        lblStatus.Text = "Тақтадағы кез келген клетканы басыңыз.";

        ClientSize = new Size(totalWidth, top.Height + board.Height + 48);
        Controls.AddRange(new Control[] { top, board, lblResult, txtResult, btnCopy, btnSave, lblStatus });

        timer.Interval = SpeedToInterval();
        timer.Tick += (s, e) => Tick();
    }

    static void StyleButton(Button b, string text, Color back)
    {
        b.Text = text;
        b.FlatStyle = FlatStyle.Flat;
        b.BackColor = back;
        b.ForeColor = Color.White;
    }

    int SpeedToInterval() => 600 / trackSpeed.Value; // 600 ms ... 60 ms per move

    void StartTour(int x, int y)
    {
        timer.Stop();

        var tour = new KnightTour(x, y);
        if (!tour.Solve())
        {
            lblStatus.Text = "Бұл клеткадан тұйық жол табылмады.";
            return;
        }

        board.Tour = tour;
        if (chkAnim.Checked)
        {
            board.Shown = 1;
            txtResult.Text = "Ат жүріп жатыр...";
            lblStatus.Text = "Бастау: " + KnightTour.Name(x, y) + " ...";
            timer.Start();
        }
        else
        {
            board.Shown = 65;
            ShowResult();
        }
        board.Invalidate();
    }

    void Tick()
    {
        board.Shown++;
        board.Invalidate();
        if (board.Shown >= 65)
        {
            timer.Stop();
            ShowResult();
        }
    }

    // the program prints the result: start, end, whole route and the check
    void ShowResult()
    {
        var t = board.Tour;
        var st = t.Order[1];
        var en = t.Order[64];
        string start = KnightTour.Name(st.x, st.y);
        string end = KnightTour.Name(en.x, en.y);
        bool ok = t.Check();

        var sb = new StringBuilder();
        sb.AppendLine("Бастау (1):  " + start);
        sb.AppendLine("Соңы (64):   " + end);
        sb.AppendLine("Қайту (65):  " + end + " -> " + start + " (ат жүрісі)");
        sb.AppendLine();
        sb.AppendLine("Клетка саны: 64, әрқайсысы 1 рет");
        sb.AppendLine("Тексеру:     " + (ok ? "OK" : "ҚАТЕ"));
        sb.AppendLine("Сынаған жүріс: " + t.Attempts);
        sb.AppendLine();
        sb.AppendLine("Жол:");
        for (int s = 1; s <= 64; s++)
        {
            var q = t.Order[s];
            sb.Append(s.ToString().PadLeft(2)).Append(' ').Append(KnightTour.Name(q.x, q.y));
            sb.Append(s % 4 == 0 ? "\r\n" : "   ");
        }
        sb.AppendLine("65 " + start + "  <- қайттық!");
        txtResult.Text = sb.ToString();

        lblStatus.Text =
            "Бастау (1): " + start + "   Соңы (64): " + end +
            "   →   64-ші қадамнан " + start + " клеткасына ат жүрісімен қайтады.\n" +
            "64 клетка, әрқайсысы 1 рет. Тексеру: " + (ok ? "OK" : "ҚАТЕ");
    }

    void SaveResult()
    {
        if (board.Tour == null || board.Shown < 65) return;
        var st = board.Tour.Order[1];
        using var dlg = new SaveFileDialog
        {
            Filter = "Text file (*.txt)|*.txt",
            FileName = "knight_tour_" + KnightTour.Name(st.x, st.y) + ".txt"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            File.WriteAllText(dlg.FileName, txtResult.Text, Encoding.UTF8);
    }
}
