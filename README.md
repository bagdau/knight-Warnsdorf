# knight-Warnsdorf

Two small Windows Forms programs (C#, .NET) that solve classic **Hamiltonian cycle** problems from graph theory and draw the result:

| Program | Problem |
|---|---|
| [`KnightTourApp`](KnightTourApp) | **Closed knight's tour.** The knight starts on *any* square of a chessboard you click, visits all 64 squares exactly once and returns to the square it started from. |
| [`RoundTableApp`](RoundTableApp) | **Round table.** Some of the guests are friends. Can everybody sit around a round table so that both neighbours of every person are his/her friends? |

Both problems are the same question: *does the graph have a Hamiltonian cycle?* (chessboard squares / people are vertices, knight moves / friendships are edges).

## Requirements

* Windows
* [.NET 10 SDK](https://dotnet.microsoft.com/download) (the projects target `net10.0-windows`)

## Run

```bash
dotnet run --project KnightTourApp
dotnet run --project RoundTableApp
```

## KnightTourApp

Click any square (or press **Random start**). The knight starts there, goes through all 64 squares and the last square is one knight move away from the start, so the knight can jump back home.

* green square = start (1), blue square = move 64, dashed green arrow = the way back home
* animation (with speed control), optional path lines
* the result panel prints the start, the end, the whole route (`1 E4, 2 G5, ...`) and a check; the route can be copied or saved to a `.txt` file

**Algorithm** (`KnightTourApp/KnightTour.cs`) - backtracking search ordered by **Warnsdorff's rule**:

1. Try first the next square that has the *fewest* free onward moves (hard-to-reach squares are visited early).
2. If the knight gets stuck, undo the last move and try another one (backtracking).
3. Prune a branch as soon as the start square has no free neighbour left (there would be no way back).
4. After square 64, accept the tour only if it is one knight move away from the start. `Check()` verifies the whole result.

Why *any* start works: a closed tour is a cycle - it has no beginning, so it can be walked from every square. The program searches again for every chosen start and verifies the result.

During development the solver found a verified closed tour from **all 64** start squares (worst case 105,724 tried moves, about 0.1 s for all 64 starts together).

## RoundTableApp

Choose the number of people (3-12) and click two people to add or remove a friendship. The program tells at once whether everybody can be seated, draws the table with the seating order and, if it is impossible, explains why.

**Theory** (`RoundTableApp/Seating.cs`):

* Necessary: everybody needs at least 2 friends, and the group must stay connected when any single person is removed.
* **Dirac's theorem** (sufficient): `n >= 3` and every person has at least `n/2` friends.
* **Ore's theorem** (sufficient): `n >= 3` and for every two people who are *not* friends the sum of their numbers of friends is at least `n`.
* The sufficient conditions are not necessary: the program finds a seating by an exact subset dynamic programming search (Held-Karp), so it finds one whenever one exists.

The search was compared with brute force on 20,000 random friendship graphs (no mismatches), and every graph satisfying Dirac's or Ore's condition was seated. These checks were run as throw-away scripts during development and are not part of the repository.

## Кіріспе (Қазақша)

Екі бағдарлама да графтағы **Гамильтон циклін** іздейді.

* **KnightTourApp** - тақтадағы кез келген клетканы басасыз, ат сол клеткадан бастап 64 клетканы бір реттен аралап, **бастапқы клеткаға қайтады**. Алгоритм: қайтып оралу (backtracking) + Варнсдорф ережесі.
* **RoundTableApp** - дөңгелек үстелде әр адамның екі жағында да оның досы отыра ала ма? Бағдарлама жауабын, отыру ретін және (болмаса) себебін көрсетеді. Дирак және Оре теоремаларының шарттары тексеріледі.

## License

[MIT](LICENSE) - Copyright (c) 2026 bagdau.

## Contributors

* [bagdau](https://github.com/bagdau) - author
* [muhammedzhann](https://github.com/muhammedzhann) - contributor
