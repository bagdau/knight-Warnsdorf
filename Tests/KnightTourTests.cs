using KnightTourApp;
using Xunit;

namespace Tests;

public class KnightTourTests
{
    [Theory]
    [InlineData(0, 0)] // A1, a corner
    [InlineData(4, 3)] // E4, the center
    [InlineData(7, 7)] // H8, the other corner
    [InlineData(1, 0)] // B1, where the knight stands in a real game
    public void Tour_is_closed_for_chosen_start(int x, int y)
    {
        var tour = new KnightTour(x, y);
        Assert.True(tour.Solve());
        Assert.True(tour.Check());
        Assert.Equal((x, y), tour.Order[1]);
    }

    [Fact]
    public void Tour_is_found_from_all_64_squares()
    {
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                var tour = new KnightTour(x, y);
                Assert.True(tour.Solve(), "no tour from " + KnightTour.Name(x, y));
                Assert.True(tour.Check(), "bad tour from " + KnightTour.Name(x, y));
            }
    }

    [Fact]
    public void Last_square_is_one_knight_move_from_the_start()
    {
        var tour = new KnightTour(4, 3);
        Assert.True(tour.Solve());
        var first = tour.Order[1];
        var last = tour.Order[64];
        int dx = Math.Abs(first.x - last.x), dy = Math.Abs(first.y - last.y);
        Assert.True((dx == 1 && dy == 2) || (dx == 2 && dy == 1));
    }

    [Fact]
    public void Every_square_gets_a_different_number_from_1_to_64()
    {
        var tour = new KnightTour(2, 5);
        Assert.True(tour.Solve());
        var numbers = new List<int>();
        foreach (int v in tour.Step) numbers.Add(v);
        Assert.Equal(Enumerable.Range(1, 64), numbers.OrderBy(v => v));
    }

    [Fact]
    public void Check_fails_if_a_number_is_repeated()
    {
        var tour = new KnightTour(0, 0);
        Assert.True(tour.Solve());
        tour.Step[0, 1] = tour.Step[3, 3]; // break the board on purpose
        Assert.False(tour.Check());
    }

    [Fact]
    public void Squares_have_chess_names()
    {
        Assert.Equal("A1", KnightTour.Name(0, 0));
        Assert.Equal("E4", KnightTour.Name(4, 3));
        Assert.Equal("H8", KnightTour.Name(7, 7));
    }
}
