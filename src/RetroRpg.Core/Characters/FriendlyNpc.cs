namespace RetroRpg.Core.Characters;

// Konuşulabilen dost veya tarafsız NPC. Diyalog metni sonradan DialogueKey ile bağlanır.
public class FriendlyNpc : Npc
{
    public FriendlyNpc(string name, string visualAssetId, string dialogueKey) : base(name, visualAssetId)
    {
        DialogueKey = dialogueKey;
    }

    public string DialogueKey { get; }

    public override string Interact() => $"{Name} konuşmak istiyor.";
}
