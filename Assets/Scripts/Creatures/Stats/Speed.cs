using UnityEngine;

public class Speed : Stat
{
    public Speed(Trait trait) : base(trait)
    {

    }

    public Speed()
    {
        // Required for Unity's AddComponent to work at runtime.
    }

    public override string ToString()
    {
        string formattedString = "";
        formattedString = trait.Name + ": " + Mathf.Round(trait.Value);
        return formattedString;
    }
}