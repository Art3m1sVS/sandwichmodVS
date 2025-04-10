using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using ACulinaryArtillery;
using Newtonsoft.Json.Linq;
using sandwich;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace sandwich;

public class SandwichProperties
{
    protected OrderedDictionary<int, ItemStack> Layers { get; set; } = new();

    public bool Any => Layers.Any();

    public IEnumerable<ItemStack> GetOrdered(IWorldAccessor world)
    {
        return Layers.OrderBy(x => x.Key).Select(x => x.Value);
    }

    public bool TryAdd(ItemStack stack, IWorldAccessor world)
    {
        if (CanAdd(stack, world))
        {
            Layers[Layers.Count] = stack;
            return true;
        }
        return false;
    }

    public bool CanAdd(ItemStack stack, IWorldAccessor world)
    {
        return Layers.Count < GetLayersLimit(world) && stack != null && stack.StackSize > 0;
    }

    public static int GetLayersLimit(IWorldAccessor world)
    {
        return world.Config.GetAsInt(worldConfigSandwichLayersLimit);
    }

    public static SandwichProperties FromStack(ItemStack stack, IWorldAccessor world)
    {
        if (stack?.Collectible is not ItemSandwich)
        {
            return null;
        }

        ITreeAttribute treeSandwichLayers = stack.Attributes.GetOrAddTreeAttribute(attributeSandwichLayers);
        SandwichProperties properties = new SandwichProperties();
        foreach ((string key, IAttribute attribute) in treeSandwichLayers)
        {
            int index = key.ToInt();
            if (!properties.Layers.ContainsKey(index))
            {
                ItemStack _stack = treeSandwichLayers.GetItemstack(key);
                if (_stack?.ResolveBlockOrItem(world) == true)
                {
                    properties.Layers[index] = _stack;
                }
            }
        }

        return properties;
    }

    public void ToStack(ItemStack stack)
    {
        if (stack?.Collectible is not ItemSandwich)
        {
            return;
        }

        if (Layers.Any())
        {
            ITreeAttribute treeSandwichLayers = stack.Attributes.GetOrAddTreeAttribute(attributeSandwichLayers);

            foreach ((int key, ItemStack val) in Layers)
            {
                treeSandwichLayers.SetItemstack(key.ToString(), val);
            }
        }
    }

    public StringBuilder GetDescription(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool noLimit = false)
    {
        ItemStack stackWithoutSandwichAttributes = inSlot.Itemstack.Clone();
        stackWithoutSandwichAttributes.Attributes.RemoveAttribute(attributeSandwichLayers);
        IEnumerable<ItemStack> stacks = new List<ItemStack>() { stackWithoutSandwichAttributes }.Concat(GetOrdered(world));

        OrderedDictionary<string, float> stackSummary = new OrderedDictionary<string, float>();

        foreach (ItemStack stack in stacks)
        {
            if (stack == null || stack.StackSize <= 0)
            {
                continue;
            }

            WaterTightContainableProps containableProps = BlockLiquidContainerBase.GetContainableProps(stack);
            if (containableProps != null)
            {
                float litres = stack.StackSize / containableProps.ItemsPerLitre;
                if (litres > 0f)
                {
                    string inContainerName = Lang.Get(stack.Collectible.Code.Domain + ":incontainer-" + stack.Class.ToString().ToLowerInvariant() + "-" + stack.Collectible.Code.Path);

                    if (!stackSummary.ContainsKey(inContainerName))
                    {
                        stackSummary[inContainerName] = 0f;
                    }
                    stackSummary[inContainerName] += litres;

                    continue;
                }
            }

            string stackName = stack.GetName();
            if (stack.Collectible is IContainedCustomName ccn)
            {
                stackName = ccn.GetContainedInfo(new DummySlot(stack));
            }

            if (!stackSummary.TryGetValue(stackName, out float currentStackSize))
            {
                currentStackSize = 0;
            }
            stackSummary[stackName] = currentStackSize + stack.StackSize;
        }

        dsc.AppendLine(Lang.Get(langSandwichContents));
        foreach (KeyValuePair<string, float> entry in noLimit ? stackSummary : stackSummary.TakeLast(6))
        {
            if (entry.Value % 1 == 0)
            {
                dsc.AppendLine($"- {Lang.Get("{0}x {1}", (int)entry.Value, entry.Key)}");
            }
            else
            {
                dsc.AppendLine($"- {Lang.Get("{0} litres of {1}", entry.Value, entry.Key)}");
            }
        }
        return dsc;
    }

    public SandwichNutritionProperties GetNutritionProperties(ItemSlot inSlot, IWorldAccessor world, Entity forEntity)
    {
        ItemStack[] stacks = new List<ItemStack>() { inSlot.Itemstack }.Concat(GetOrdered(world)).ToArray();
        UnspoilContents(world, stacks);
        SandwichNutritionProperties sandwichNutritionProps = new SandwichNutritionProperties();
        FoodNutritionProperties[] nutritionPropsArray = GetContentNutritionProperties(world, inSlot, stacks, forEntity as EntityAgent);
        sandwichNutritionProps.NutritionPropertiesMany.AddRange(nutritionPropsArray);
        return sandwichNutritionProps;
    }

    public static FoodNutritionProperties[] GetContentNutritionProperties(IWorldAccessor world, ItemSlot inSlot, ItemStack[] contentStacks, EntityAgent forEntity, bool mulWithStacksize = false, float nutritionMul = 1f, float healthMul = 1f)
    {
        List<FoodNutritionProperties> list = new List<FoodNutritionProperties>();
        if (contentStacks == null)
        {
            return list.ToArray();
        }

        for (int i = 0; i < contentStacks.Length; i++)
        {
            var contentStack = contentStacks[i];
            if (contentStacks[i] != null)
            {
                CollectibleObject collectible = contentStacks[i].Collectible;

                if (collectible.Attributes?["transitionableProps"].AsObject<TransitionableProperties>() != null)
                {
                    //world.Logger.Event("has thing");
                }
                // world.Logger.Event(collectible.Attributes?["transitionableProps"].AsObject<TransitionableProperties>().ToString());

                FoodNutritionProperties foodNutritionProperties = ((collectible.CombustibleProps == null || collectible.CombustibleProps.SmeltedStack == null) ? collectible.GetNutritionProperties(world, contentStacks[i], forEntity) : collectible.CombustibleProps.SmeltedStack.ResolvedItemstack.Collectible.GetNutritionProperties(world, collectible.CombustibleProps.SmeltedStack.ResolvedItemstack, forEntity));

                WaterTightContainableProps props = (contentStacks[i] == null) ? null : BlockLiquidContainerBase.GetContainableProps(contentStacks[i]);
                if (foodNutritionProperties == null && props?.NutritionPropsPerLitre != null)
                {
                    FoodNutritionProperties nutriProps = props.NutritionPropsPerLitre.Clone();
                    float litre = (float)contentStacks[i].StackSize / props.ItemsPerLitre;
                    nutriProps.Health *= litre;
                    nutriProps.Satiety *= litre;
                    foodNutritionProperties = nutriProps;
                }

                if (foodNutritionProperties == null && collectible.Attributes != null && collectible.Attributes["nutritionPropsWhenInMeal"].Exists)
                {
                    foodNutritionProperties = collectible.Attributes?["nutritionPropsWhenInMeal"].AsObject<FoodNutritionProperties>();
                }




                if (foodNutritionProperties != null)
                {
                    float stacksize = ((!mulWithStacksize) ? 1 : contentStack.StackSize);
                    FoodNutritionProperties foodNutritionProperties2 = foodNutritionProperties.Clone();
                    DummySlot dummySlot = new DummySlot(contentStacks[i], inSlot.Inventory);
                    try
                    {
                        float spoilState = contentStacks[i].Collectible.UpdateAndGetTransitionState(world, dummySlot, EnumTransitionType.Perish)?.TransitionLevel ?? 0f;
                        float satLossMul = GlobalConstants.FoodSpoilageSatLossMul(spoilState, dummySlot.Itemstack, forEntity);
                        float healthLossMul = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, dummySlot.Itemstack, forEntity);
                        foodNutritionProperties2.Satiety *= satLossMul * nutritionMul * stacksize;
                        foodNutritionProperties2.Health *= healthLossMul * healthMul * stacksize;
                    }
                    catch
                    {
                    }
                    list.Add(foodNutritionProperties2);
                }
            }
        }

        return list.ToArray();
    }

    protected void UnspoilContents(IWorldAccessor world, ItemStack[] cstacks)
    {
        // Dont spoil the pie contents, the pie itself has a spoilage timer. Semi hacky fix reset their spoil timers each update

        for (int i = 0; i < cstacks.Length; i++)
        {
            ItemStack cstack = cstacks[i];
            if (cstack == null) continue;

            if (!(cstack.Attributes["transitionstate"] is ITreeAttribute))
            {
                cstack.Attributes["transitionstate"] = new TreeAttribute();
            }
            ITreeAttribute attr = (ITreeAttribute)cstack.Attributes["transitionstate"];

            if (attr.HasAttribute("createdTotalHours"))
            {
                attr.SetDouble("createdTotalHours", world.Calendar.TotalHours);
                attr.SetDouble("lastUpdatedTotalHours", world.Calendar.TotalHours);
                var transitionedHours = (attr["transitionedHours"] as FloatArrayAttribute)?.value;
                for (int j = 0; transitionedHours != null && j < transitionedHours.Length; j++)
                {
                    transitionedHours[j] = 0;
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        if (Layers.Any())
        {
            result.Append('(');
            result.Append(string.Join('|', Layers.Select(x => $"{x.Key}-{StackToString(x.Value)}")));
            result.Append(')');
        }
        return result.ToString();
    }

    private static string StackToString(ItemStack stack)
    {
        return new ItemstackAttribute(stack).ToJsonToken();
    }
}