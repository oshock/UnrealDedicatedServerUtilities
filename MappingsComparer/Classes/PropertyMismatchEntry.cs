using UsmapDotNet;

namespace MappingsComparer.Classes;

class PropertyMismatchEntry
{
    public EUsmapPropertyType PropertyType;
    public string Name;

    // If true: Only Fortnite has this property
    // If false: The default engine includes this property, however Fortnite does not
    public bool IsCustom;

    public string? PreviousProperty;
    public UsmapPropertyData Data;
    public UsmapSchema Schema;
}