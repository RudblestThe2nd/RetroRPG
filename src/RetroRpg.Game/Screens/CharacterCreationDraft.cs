using RetroRpg.Core.Characters;
using RetroRpg.Core.Common;

namespace RetroRpg.Game.Screens;

// Ekranlar arasında taşınan sade karakter taslağı. Oyuncu nesnesi yalnız önizleme
// veya son onay gerektiğinde üretilir; böylece ırk/geçmiş bonusları iki kez eklenmez.
internal sealed class CharacterCreationDraft(
    string name,
    Gender gender,
    CharacterRace race,
    CharacterClass characterClass,
    IReadOnlyList<BackgroundStageChoice> backgroundChoices,
    int portraitIndex,
    int hairStyle = 1,
    int facialStyle = 0)
{
    private readonly int[] _points = new int[6];

    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? "Gezgin" : name.Trim();
    public Gender Gender { get; } = gender;
    public CharacterRace Race { get; } = race;
    public CharacterClass CharacterClass { get; } = characterClass;
    public IReadOnlyList<BackgroundStageChoice> BackgroundChoices { get; } = backgroundChoices.ToArray();
    public CharacterBackground Background => BackgroundCatalog.Create(BackgroundChoices);
    public int PortraitIndex { get; } = portraitIndex;
    public int HairStyle { get; } = hairStyle;
    public int FacialStyle { get; } = facialStyle;
    public int PointsRemaining => 3 - _points.Sum();
    public IReadOnlyList<int> AssignedPoints => _points;

    public bool Adjust(int index, int amount)
    {
        if (index < 0 || index >= _points.Length || amount is < -1 or > 1) return false;
        int next = _points[index] + amount;
        if (next < 0 || next > 3 || (amount > 0 && PointsRemaining == 0)) return false;
        _points[index] = next;
        return true;
    }

    public CharacterStats CreateBaseStats() => new()
    {
        Strength = 10 + _points[0],
        Dexterity = 10 + _points[1],
        Constitution = 10 + _points[2],
        Intelligence = 10 + _points[3],
        Wisdom = 10 + _points[4],
        Charisma = 10 + _points[5]
    };

    public PlayerCharacter CreatePlayer() =>
        new(Name, Gender, Race, CharacterClass, Background, CreateBaseStats());
}
