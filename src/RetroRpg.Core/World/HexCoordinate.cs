namespace RetroRpg.Core.World;

public readonly record struct HexCoordinate(int Q, int R)
{
    private static readonly HexCoordinate[] Directions =
    [
        new(1, 0), new(1, -1), new(0, -1), new(-1, 0), new(-1, 1), new(0, 1)
    ];

    public IEnumerable<HexCoordinate> Neighbors()
    {
        int q = Q;
        int r = R;
        return Directions.Select(x => new HexCoordinate(q + x.Q, r + x.R));
    }

    public int DistanceTo(HexCoordinate other)
    {
        int s = -Q - R;
        int otherS = -other.Q - other.R;
        return (Math.Abs(Q - other.Q) + Math.Abs(R - other.R) + Math.Abs(s - otherS)) / 2;
    }
}
