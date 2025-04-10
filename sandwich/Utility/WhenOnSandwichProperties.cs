using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using ACulinaryArtillery;
using EFRecipes;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace sandwich;

public class WhenOnSandwichProperties
{
    public CompositeShape Shape { get; set; }

    public CompositeShape ShapeLast { get; set; }


    public Dictionary<string, CompositeTexture> Textures { get; set; } = new();

    public float Size { get; set; } = 0.0325f;

    public bool Rotate { get; set; }

    public bool CopyLastRotation { get; set; }

    public NatFloat Rotation { get; set; } = NatFloat.One;

    public float LitresPerLayer { get; set; } = 0.1f;

    public static WhenOnSandwichProperties GetProps(CollectibleObject obj)
    {
        return HasAtribute(obj) ? obj.Attributes[attributeWhenOnSandwich].AsObject<WhenOnSandwichProperties>() : null;
    }

    public static WhenOnSandwichProperties GetPropsMadeWith(CollectibleObject obj)
    {
        return HasAtributeMadeWith(obj) ? obj.Attributes["madeWith"].AsObject<WhenOnSandwichProperties>() : null;
    }

    public static WhenOnSandwichProperties GetPropsES(CollectibleObject obj)
    {
        return HasAtributeES(obj) ? obj.Attributes["expandedSats"].AsObject<WhenOnSandwichProperties>() : null;
    }

    public static bool HasAtribute(CollectibleObject obj)
    {
        return obj != null && obj.Attributes != null && obj.Attributes.KeyExists(attributeWhenOnSandwich);
    }

    public static bool HasAtributeMadeWith(CollectibleObject obj)
    {
        return obj != null && obj.Attributes != null && obj.Attributes.KeyExists("madeWith");
    }

    public static bool HasAtributeES(CollectibleObject obj)
    {
        return obj != null && obj.Attributes != null && obj.Attributes.KeyExists("expandedSats");
    }

    public static void SetAtribute(CollectibleObject obj, WhenOnSandwichProperties props)
    {
        obj.Attributes.Token[attributeWhenOnSandwich] = JToken.FromObject(props);
    }

    public static void SetAtributeMadeWith(CollectibleObject obj, WhenOnSandwichProperties props)
    {
        obj.Attributes.Token["madeWith"] = JToken.FromObject(props);
    }

    public static void SetAtributeES(CollectibleObject obj, WhenOnSandwichProperties props)
    {
        obj.Attributes.Token["expandedSats"] = JToken.FromObject(props);
    }

    public WhenOnSandwichProperties Clone()
    {
        return new()
        {
            Shape = Shape.Clone(),
            ShapeLast = ShapeLast.Clone(),
            Textures = Textures.ToDictionary(x => x.Key, y => y.Value.Clone()),
            Size = Size,
            Rotate = Rotate,
            CopyLastRotation = CopyLastRotation,
            Rotation = Rotation.Clone(),
            LitresPerLayer = LitresPerLayer
        };
    }
}
