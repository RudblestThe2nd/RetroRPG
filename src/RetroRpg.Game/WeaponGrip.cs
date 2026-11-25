using RetroRpg.Core.Common;

namespace RetroRpg.Game;

// Sprite koordinatları 256 × 256. Her poz için kapalı avucun merkezini kullanır.
internal readonly record struct GripPose(int Frame, int X, int Y, Direction Direction = Direction.East);
internal readonly record struct GripPoses(GripPose Ready, GripPose Raised, GripPose Strike);
internal readonly record struct WeaponGrip(int X, int Y, int TurnToRight, int Size)
{
    // Sapın kaynak PNG'deki merkezi ve namluyu / ucu sağa getiren açı.
    public static WeaponGrip For(string id) => id switch
    {
        "dagger" => new(128, 62, -90, 72),
        "rapier" => new(133, 49, -90, 124),
        "warhammer" => new(137, 186, 90, 122),
        "mace" => new(146, 183, 90, 108),
        "greatsword" => new(53, 128, 0, 132),
        "light-crossbow" => new(83, 139, 0, 88),
        _ => default
    };

    public static GripPoses ForBody(string race, Gender gender) => (race, gender) switch
    {
        ("dragonborn", Gender.Female) => new(new(0, 182, 75), new(2, 197, 61), new(1, 183, 81)),
        ("dragonborn", Gender.Male) => new(new(0, 181, 76), new(2, 194, 42), new(1, 186, 86)),
        ("drow", Gender.Female) => new(new(0, 131, 127), new(2, 176, 84), new(1, 163, 98)),
        ("drow", Gender.Male) => new(new(0, 128, 127), new(2, 179, 77), new(1, 162, 92)),
        ("dwarf", Gender.Female) => new(new(0, 119, 136), new(2, 177, 87), new(3, 176, 103)),
        ("dwarf", Gender.Male) => new(new(0, 113, 134), new(2, 180, 69), new(3, 175, 101)),
        ("elf", Gender.Female) => new(new(0, 147, 100), new(2, 172, 77), new(1, 163, 95)),
        ("elf", Gender.Male) => new(new(0, 154, 98), new(2, 174, 70), new(1, 164, 90)),
        ("githyanki", Gender.Female) => new(new(0, 153, 116), new(2, 185, 76), new(3, 163, 91)),
        ("githyanki", Gender.Male) => new(new(0, 152, 114), new(2, 183, 71), new(3, 161, 88)),
        ("half-elf", Gender.Female) => new(new(3, 152, 83), new(2, 178, 69), new(1, 173, 93)),
        ("half-elf", Gender.Male) => new(new(3, 146, 84), new(2, 177, 72), new(1, 175, 91)),
        ("half-orc", Gender.Female) => new(new(0, 132, 140), new(2, 177, 72), new(1, 184, 95)),
        ("half-orc", Gender.Male) => new(new(0, 171, 68), new(2, 176, 68), new(1, 177, 86)),
        ("human", Gender.Female) => new(new(3, 147, 80), new(2, 180, 65), new(1, 175, 83)),
        ("human", Gender.Male) => new(new(3, 146, 81), new(2, 177, 67), new(1, 176, 87)),
        ("tiefling", Gender.Female) => new(new(0, 147, 93), new(2, 188, 83), new(1, 181, 90)),
        ("tiefling", Gender.Male) => new(new(0, 152, 81), new(2, 186, 84), new(1, 190, 100)),
        ("gnome", Gender.Female) => new(new(0,169,118,Direction.North), new(3,162,115,Direction.South), new(4,170,120,Direction.South)),
        ("gnome", Gender.Male) => new(new(2,161,133), new(2,168,110,Direction.North), new(4,166,123)),
        ("halfling", Gender.Female) => new(new(2,160,129), new(2,168,112,Direction.North), new(4,167,120)),
        ("halfling", Gender.Male) => new(new(2,160,129), new(2,167,110,Direction.North), new(4,167,121)),
        _ => new(new(0,156,104),new(2,170,84),new(1,169,102))
    };
}
