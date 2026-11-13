namespace RetroRpg.Core.Characters;

public class WizardClass : CharacterClass
{
    public WizardClass() : this("", "", "") { }

    protected WizardClass(string subclassId, string subclassName, string subclassFeature)
        : base("wizard", "Sihirbaz", 8, "Intelligence", subclassId, subclassName, subclassFeature) { }

    public override string SignatureAction => "Büyü Kitabı";
}

public sealed class EvocationWizardClass : WizardClass
{
    public EvocationWizardClass() : base("evocation", "Çağırma Okulu", "Alan büyüsü") { }
}

public sealed class AbjurationWizardClass : WizardClass
{
    public AbjurationWizardClass() : base("abjuration", "Koruma Okulu", "Büyü kalkanı") { }
}
