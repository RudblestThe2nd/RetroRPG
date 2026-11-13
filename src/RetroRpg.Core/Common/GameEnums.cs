namespace RetroRpg.Core.Common;

public enum Direction { South, East, North, West }
public enum Gender { Female, Male }
public enum Biome { Forest, Swamp, Desert, Snow, Mountain, Tropical, Volcanic, Corrupted, Underground, Plains }
public enum DiscoveryState { Undiscovered, Fogged, Discovered }
public enum CharacterAnimation { Idle, Walk, Attack, Cast, Defend, Hit, Death, Talk, Sit, UseItem }
// StoryLayer sona eklendi; kayıtlarda sayı olarak tutulan eski yuvaların değeri değişmez.
public enum EquipmentSlot { Head, Torso, Feet, Cloak, Accessory, MainHand, OffHand, Back, StoryLayer }
public enum QuestStatus { NotStarted, Active, Completed, Failed }
public enum SceneKind { MainMenu, CharacterCreation, Map, Travel, Combat, Camp, Inventory }
