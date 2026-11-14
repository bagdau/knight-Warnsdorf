namespace RoundTableApp;

// Round table problem = Hamiltonian cycle in the "friendship graph".
//   person  -> vertex
//   friends -> edge
// Everybody sits at a round table so that BOTH neighbours of every person are
// his/her friends  <=>  the friendship graph has a Hamiltonian cycle.
public static class Seating
{
    public static int Degree(bool[,] adj, int n, int v)
    {
        int d = 0;
        for (int u = 0; u < n; u++) if (adj[v, u]) d++;
        return d;
    }

    // Dirac's theorem (sufficient condition): n >= 3 and every person has at least n/2 friends
    public static bool Dirac(bool[,] adj, int n)
    {
        if (n < 3) return false;
        for (int v = 0; v < n; v++)
            if (2 * Degree(adj, n, v) < n) return false;
        return true;
    }

    // Ore's theorem (sufficient condition): n >= 3 and for every two people who are NOT
    // friends the sum of their numbers of friends is at least n
    public static bool Ore(bool[,] adj, int n)
    {
        if (n < 3) return false;
        for (int a = 0; a < n; a++)
            for (int b = a + 1; b < n; b++)
                if (!adj[a, b] && Degree(adj, n, a) + Degree(adj, n, b) < n) return false;
        return true;
    }

    // Finds a seating order (a Hamiltonian cycle) or returns null.
    // Method: dynamic programming over subsets (Held-Karp), n <= about 16.
    //   parent[mask, v] = the person before v, if there is a chain that starts at person 0,
    //   visits exactly the people of 'mask' and ends at v.
    public static int[] FindCycle(bool[,] adj, int n)
    {
        if (n < 3) return null;
        int size = 1 << n;
        var parent = new int[size * n];
        Array.Fill(parent, -1);
        parent[1 * n + 0] = 0;

        for (int mask = 1; mask < size; mask += 2) // person 0 is always in the mask
        {
            for (int v = 0; v < n; v++)
            {
                if ((mask >> v & 1) == 0 || parent[mask * n + v] == -1) continue;
                for (int u = 1; u < n; u++)
                {
                    if ((mask >> u & 1) != 0 || !adj[v, u]) continue;
                    int idx = (mask | 1 << u) * n + u;
                    if (parent[idx] == -1) parent[idx] = v;
                }
            }
        }

        int full = size - 1;
        for (int last = 1; last < n; last++)
        {
            if (parent[full * n + last] == -1 || !adj[last, 0]) continue; // last must be a friend of person 0

            var order = new List<int>();
            int cur = last, m = full;
            while (cur != 0)
            {
                order.Add(cur);
                int prev = parent[m * n + cur];
                m ^= 1 << cur;
                cur = prev;
            }
            order.Add(0);
            order.Reverse();
            return order.ToArray();
        }
        return null;
    }

    static bool Connected(bool[,] adj, int n, int skip)
    {
        int start = skip == 0 ? 1 : 0;
        var seen = new bool[n];
        var stack = new Stack<int>();
        stack.Push(start);
        seen[start] = true;
        int count = 1;
        while (stack.Count > 0)
        {
            int v = stack.Pop();
            for (int u = 0; u < n; u++)
                if (u != skip && adj[v, u] && !seen[u]) { seen[u] = true; count++; stack.Push(u); }
        }
        return count == n - (skip >= 0 ? 1 : 0);
    }

    // short explanation (written in Kazakh) why the guests cannot be seated
    public static string WhyNot(bool[,] adj, int n, string[] names)
    {
        if (n < 3) return "Дөңгелек үстелге отыру үшін кемінде 3 адам керек.";

        for (int v = 0; v < n; v++)
            if (Degree(adj, n, v) < 2)
                return names[v] + "-тың " + Degree(adj, n, v) + " досы ғана бар. Екі жағында да дос отыруы үшін әр адамның кемінде 2 досы болуы керек.";

        if (!Connected(adj, n, -1))
            return "Компания достық бойынша бөлек топтарға бөлінген: бір топтан екінші топқа дос арқылы өту мүмкін емес.";

        for (int v = 0; v < n; v++)
            if (!Connected(adj, n, v))
                return names[v] + " болмаса, қалғандар екі бөлек топқа бөлінеді. Үстелді айналған сақинада кез келген адамды алып тастаса да, қалғандар бір тізбек болып қалуы керек, ал мұнда олай емес.";

        return "Әр адамның кемінде 2 досы бар және компания біртұтас, бірақ достық байланыстар тұйық сақина құруға жетпейді.";
    }
}
