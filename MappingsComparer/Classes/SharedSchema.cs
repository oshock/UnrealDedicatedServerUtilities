using UsmapDotNet;

namespace MappingsComparer.Classes;

public class SharedSchema
{
    public UsmapSchema Default;
    public UsmapSchema Custom; // Fortnite

    public static SharedSchema Create(UsmapSchema defaultSchema, UsmapSchema customSchema)
    {
        return new SharedSchema
        {
            Default = defaultSchema,
            Custom = customSchema
        };
    }
}