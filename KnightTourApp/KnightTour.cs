namespace KnightTourApp;

// Closed knight's tour = Hamiltonian cycle in the knight graph.
// The knight starts on (StartX, StartY), visits all 64 squares exactly once
// and the last square (step 64) is one knight move away from the start,
// so the knight can jump back to where it started.
//
// Method: backtracking + Warnsdorff's rule
//  - Warnsdorff: try first the square with the FEWEST free onward moves
//  - backtracking: when stuck, undo the last move and try another one
//  - pruning: the start square must keep at least one free neighbour,
//    otherwise there is no way back to it
public class KnightTour
{
    public const int N = 8;

    static readonly int[] Dx = { 2, 1, -1, -2, -2, -1, 1, 2 };
    static readonly int[] Dy = { 1, 2, 2, 1, -1, -2, -2, -1 };

    // Step[x, y] = number of the move (1..64) in which the knight visits (x, y)
    public int[,] Step = new int[N, N];
    // Order[s] = square of step s (index 0 is not used)
    public (int x, int y)[] Order = new (int x, int y)[N * N + 1];

    public int StartX, StartY;
    public long Attempts;

    public KnightTour(int startX, int startY)
    {
        StartX = startX;
        StartY = startY;
    }

    public bool Solve()
    {
        Array.Clear(Step);
        Attempts = 0;
        Step[StartX, StartY] = 1;
        if (!Go(StartX, StartY, 1)) return false;

        for (int x = 0; x < N; x++)
            for (int y = 0; y < N; y++)
                Order[Step[x, y]] = (x, y);
        return true;
    }

    static bool Inside(int x, int y) => x >= 0 && x < N && y >= 0 && y < N;

    static bool IsKnightMove(int x1, int y1, int x2, int y2)
    {
        int ax = Math.Abs(x1 - x2), ay = Math.Abs(y1 - y2);
        return (ax == 1 && ay == 2) || (ax == 2 && ay == 1);
    }

    int FreeDegree(int x, int y)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            int nx = x + Dx[i], ny = y + Dy[i];
            if (Inside(nx, ny) && Step[nx, ny] == 0) count++;
        }
        return count;
    }

    bool Go(int x, int y, int move)
    {
        if (move == N * N)
            return IsKnightMove(x, y, StartX, StartY); // can we go home?

        if (FreeDegree(StartX, StartY) == 0) return false; // way home is closed

        var next = new List<(int deg, int x, int y)>();
        for (int i = 0; i < 8; i++)
        {
            int nx = x + Dx[i], ny = y + Dy[i];
            if (Inside(nx, ny) && Step[nx, ny] == 0)
                next.Add((FreeDegree(nx, ny), nx, ny));
        }
        next.Sort((a, b) => a.deg.CompareTo(b.deg)); // Warnsdorff

        foreach (var c in next)
        {
            Attempts++;
            Step[c.x, c.y] = move + 1;
            if (Go(c.x, c.y, move + 1)) return true;
            Step[c.x, c.y] = 0; // backtrack
        }
        return false;
    }

    // every square is used once, every step is a knight move, and the move 64 -> 1 is a knight move too
    public bool Check()
    {
        var seen = new bool[N * N + 1];
        foreach (int v in Step)
        {
            if (v < 1 || v > N * N || seen[v]) return false;
            seen[v] = true;
        }
        for (int s = 1; s <= N * N; s++)
        {
            var a = Order[s];
            var b = Order[s == N * N ? 1 : s + 1];
            if (!IsKnightMove(a.x, a.y, b.x, b.y)) return false;
        }
        return true;
    }

    // chess name: x = column (A..H), y = row from the bottom (1..8)
    public static string Name(int x, int y) => ((char)('A' + x)).ToString() + (y + 1);
}
