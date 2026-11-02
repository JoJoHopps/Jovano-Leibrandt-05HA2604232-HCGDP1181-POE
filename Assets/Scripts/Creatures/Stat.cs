using System;
using UnityEngine;

public abstract class Stat : MonoBehaviour
{
    [SerializeField] protected Trait trait;


    public Stat(Trait trait)
    {
        this.trait = trait;
    }

    protected Stat()
    {
        // Required so Unity's AddComponent can create this type at runtime
        // (used when a baby creature inherits a stat its base prefab didn't have).
    }

    public virtual Trait GetTrait()
    {
        return trait;
    }

    public virtual void CopyFrom(Stat source)
    {
        Trait sourceTrait = source.GetTrait();

        // Create a brand new Trait, not a shared reference, so editing the
        // baby's stat later can never accidentally affect the parent's stat.
        trait = new Trait(sourceTrait.Name, sourceTrait.Value, sourceTrait.Description);
        trait.MaxValue = sourceTrait.MaxValue;
    }

    public virtual string ToString()
    {
        string formattedString = "";
        formattedString = trait.Name + ": " + Mathf.Round(trait.Value) + "/" + trait.MaxValue;
        return formattedString;
    }

}