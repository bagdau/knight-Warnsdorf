using RoundTableApp;
using Xunit;

namespace Tests;

public class SeatingTests
{
    // helper: friendship matrix from pairs
    static bool[,] Graph(int n, params (int, int)[] pairs)
    {
        var a = new bool[n, n];
        foreach (var (x, y) in pairs) a[x, y] = a[y, x] = true;
        return a;
    }

    static bool IsGoodSeating(bool[,] adj, int n, int[] order)
    {
        if (order == null || order.Length != n || order.Distinct().Count() != n) return false;
        for (int k = 0; k < n; k++)
            if (!adj[order[k], order[(k + 1) % n]]) return false;
        return true;
    }

    [Fact]
    public void Ring_of_friends_can_be_seated()
    {
        var adj = Graph(5, (0, 1), (1, 2), (2, 3), (3, 4), (4, 0));
        Assert.True(IsGoodSeating(adj, 5, Seating.FindCycle(adj, 5)));
    }

    [Fact]
    public void Chain_of_friends_cannot_be_seated()
    {
        // the first and the last person have only one friend
        var adj = Graph(5, (0, 1), (1, 2), (2, 3), (3, 4));
        Assert.Null(Seating.FindCycle(adj, 5));
    }

    [Fact]
    public void Two_separate_groups_cannot_be_seated()
    {
        var adj = Graph(6, (0, 1), (1, 2), (2, 0), (3, 4), (4, 5), (5, 3));
        Assert.Null(Seating.FindCycle(adj, 6));
    }

    [Fact]
    public void Less_than_three_people_cannot_be_seated()
    {
        var adj = Graph(2, (0, 1));
        Assert.Null(Seating.FindCycle(adj, 2));
    }

    [Fact]
    public void Everybody_friends_with_everybody_can_be_seated()
    {
        int n = 8;
        var adj = new bool[n, n];
        for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) adj[i, j] = i != j;
        Assert.True(IsGoodSeating(adj, n, Seating.FindCycle(adj, n)));
        Assert.True(Seating.Dirac(adj, n));
        Assert.True(Seating.Ore(adj, n));
    }

    [Fact]
    public void Dirac_and_Ore_always_give_a_seating_on_random_graphs()
    {
        var rnd = new Random(12345);
        for (int iter = 0; iter < 2000; iter++)
        {
            int n = rnd.Next(3, 9);
            double p = rnd.NextDouble();
            var adj = new bool[n, n];
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (rnd.NextDouble() < p) adj[i, j] = adj[j, i] = true;

            var order = Seating.FindCycle(adj, n);
            if (order != null) Assert.True(IsGoodSeating(adj, n, order));
            if (Seating.Dirac(adj, n) || Seating.Ore(adj, n)) Assert.NotNull(order);
        }
    }

    [Fact]
    public void Ring_does_not_satisfy_Dirac_but_still_can_be_seated()
    {
        // 8 people, everybody has only 2 friends (less than n/2 = 4)
        var adj = Graph(8, (0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (7, 0));
        Assert.False(Seating.Dirac(adj, 8));
        Assert.NotNull(Seating.FindCycle(adj, 8));
    }
}
