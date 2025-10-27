using System.Data;
using System.Reflection.Metadata;
using MappingsComparer.Classes;
using OodleDotNet;
using UsmapDotNet;

var options = new UsmapOptions
{ Oodle = new Oodle("oo2core_9_win64.dll") };

var defaultMappingsPath = args[0].Replace("\"", "").Trim();
var defaultMappings = Usmap.Parse(defaultMappingsPath, options);

if (defaultMappings == null)
    throw new NoNullAllowedException($"Could not load '{nameof(defaultMappings)}'");

var customMappingsPath = args[1].Replace("\"", "").Trim();;
var customMappings = Usmap.Parse(customMappingsPath, options);

if (customMappings == null)
    throw new NoNullAllowedException($"Could not load '{nameof(customMappings)}'");

var sharedTypes = new List<SharedSchema>();

foreach (var schema in defaultMappings.Schemas)
{
    var customSchema = customMappings.Schemas.FirstOrDefault(x => x.Name == schema.Name);
    if (customSchema == null)
        continue;

    sharedTypes.Add(SharedSchema.Create(schema, customSchema));
}

var mismatchedSchemas = new List<SharedSchema>();

foreach (var type in sharedTypes)
{
    var defaultProps = type.Default.Properties;
    var customProps = type.Custom.Properties;

    for (int i = 0; i < defaultProps.Count; i++)
    {
        if (i >= defaultProps.Count || i >= customProps.Count)
            break;

        var prop = defaultProps[i];
        var customProp = customProps[i];

        if (prop.Name == customProp.Name)
            continue;

        mismatchedSchemas.Add(type);
        break;
    }
}

var properties = new List<PropertyMismatchEntry>();

foreach (var type in mismatchedSchemas)
{
    var defaultProps = type.Default.Properties;
    var customProps = type.Custom.Properties;

    for (int i = 0; i < customProps.Count; i++)
    {
        var customProp = customProps[i];
        if (defaultProps.Any(x => x.Name == customProp.Name))
            continue;

        var previousProp = i > 0 ? customProps[i - 1].Name : null;

        properties.Add(new PropertyMismatchEntry
        {
            PropertyType = customProp.Data.Type,
            Name = customProp.Name,
            IsCustom = true, // TODO
            PreviousProperty = previousProp,
            Data = customProp.Data,
            Schema = type.Custom
        });
    }
}

var fileName = $"{Path.GetFileNameWithoutExtension(customMappingsPath)} OUTPUT.txt";
if (File.Exists(fileName))
    File.Delete(fileName);

var writer = new StreamWriter(fileName);

void Write(string text, ConsoleColor color = ConsoleColor.White, bool newLine = true)
{
    Console.ForegroundColor = color;
    if (newLine)
    {
        Console.WriteLine(text);
        writer.WriteLine(text);
    }
    else
    {
        Console.Write(text);
        writer.Write(text);
    }
}

foreach (var property in properties)
{
    Write($"================================================================\n'{property.Name}' ({property.PropertyType}) does not exist in default engine.", ConsoleColor.Red);
    Write("Calculated Property: \n");

    var type = property.PropertyType switch
    {
        EUsmapPropertyType.StructProperty => $"F{property.Data.StructType}",
        _ => RecursivelyFindType(new List<string>(), property.Data)
    };
    
    Write($"UPROPERTY()\n{type} {property.Name};", ConsoleColor.Blue);

    Write("\nResides in UClass or UStruct: ", ConsoleColor.Cyan, false);
    Write($"'{property.Schema.Name}'");
    Write((property.PreviousProperty != null ? $"Previous property: '{property.PreviousProperty}'" : $"This property is the first property.") + "\n", ConsoleColor.Magenta);
}

writer.Close();

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"\n\n{properties.Count} total properties not found in the default engine.");
Console.WriteLine($"Outputted to '{fileName}'");
Console.ReadKey();

string RecursivelyFindType(List<string> types, UsmapPropertyData? data)
{
    if (data != null)
    {
        if (data.Type == EUsmapPropertyType.ArrayProperty)
        {
            types.Add("TArray");
            return RecursivelyFindType(types, data.InnerType);
        }
        else if (data.Type == EUsmapPropertyType.MapProperty)
        {
            var keyType = RecursivelyFindType(types, data.InnerType);
            var valueType = RecursivelyFindType(types, data.ValueType);
            types.Add($"TMap<{keyType}, {valueType}>");
        }
        else if (data.Type == EUsmapPropertyType.StructProperty)
        {
            types.Add(data.StructType != null ? $"F{data.StructType}" : "NULL STRUCT TYPE");
        }
        else
        {
            types.Add(data.Type switch
            {
                EUsmapPropertyType.ByteProperty => "uint8",
                EUsmapPropertyType.BoolProperty => "uint8",
                EUsmapPropertyType.IntProperty => "int32",
                EUsmapPropertyType.FloatProperty => "float",
                EUsmapPropertyType.ObjectProperty => "(FIND OBJECT TYPE IN SDK)",
                EUsmapPropertyType.NameProperty => "FName",
                EUsmapPropertyType.DelegateProperty => data.Type.ToString(),
                EUsmapPropertyType.DoubleProperty => "double",
                EUsmapPropertyType.StrProperty => "FString",
                EUsmapPropertyType.TextProperty => "FText",
                EUsmapPropertyType.InterfaceProperty => data.Type.ToString(),
                EUsmapPropertyType.MulticastDelegateProperty => data.Type.ToString(),
                EUsmapPropertyType.WeakObjectProperty => "TWeakObjectPtr",
                EUsmapPropertyType.LazyObjectProperty => "FLazyObjectPtr",
                EUsmapPropertyType.AssetObjectProperty => "TAssetPtr",
                EUsmapPropertyType.SoftObjectProperty => "TSoftObjectPtr",
                EUsmapPropertyType.UInt64Property => "uint64",
                EUsmapPropertyType.UInt32Property => "uint32",
                EUsmapPropertyType.UInt16Property => "uint16",
                EUsmapPropertyType.Int64Property => "int64",
                EUsmapPropertyType.Int16Property => "int16",
                EUsmapPropertyType.Int8Property => "int8",
                EUsmapPropertyType.SetProperty => data.Type.ToString(),
                EUsmapPropertyType.EnumProperty => data.Type.ToString(),
                EUsmapPropertyType.FieldPathProperty => data.Type.ToString(),
                EUsmapPropertyType.OptionalProperty => data.Type.ToString(),
                EUsmapPropertyType.Utf8StrProperty => data.Type.ToString(),
                EUsmapPropertyType.AnsiStrProperty => data.Type.ToString(),
                EUsmapPropertyType.Unknown => data.Type.ToString(),
                _ => throw new ArgumentOutOfRangeException()
            });
        }
    }

    return ListToRecursiveType(types);
}

string ListToRecursiveType(List<string> types)
{
    types.Reverse();
    return types.Aggregate("", (current, type) => current == "" ? $"{type}" : $"{type}<{current}>");
}
