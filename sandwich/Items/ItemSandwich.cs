using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static Vintagestory.GameContent.BlockLiquidContainerBase;
using System.Threading.Tasks;
using ACulinaryArtillery;
using HarmonyLib;

namespace sandwich;

public class ItemSandwich : ItemExpandedFood
{
    public override bool Satisfies(ItemStack thisStack, ItemStack otherStack)
    {
        if (thisStack.Class == otherStack.Class && thisStack.Id == otherStack.Id)
        {
            if (!otherStack.Attributes.HasAttribute("madeWith") && thisStack.Attributes.HasAttribute("madeWith"))
            {
                return true;
            }
        }
        return base.Satisfies(thisStack, otherStack);
    }

    public void OnCreatedBySlicing(ItemSlot inputSlot, ItemStack outputSlot, ItemStack byRecipe, int slices)
    {
        //base.OnCreatedBySlicing(allInputslots, outputSlot, byRecipe);
        ItemStack output = outputSlot;

        List<string> ingredients = new List<string>();
        float[] sat = new float[6];
        ItemSlot slot = inputSlot;


        if (slot.Itemstack.Collectible is ItemExpandedFood)
        {
            string[] addIngs = (slot.Itemstack.Attributes["madeWith"] as StringArrayAttribute)?.value;
            float[] addSat = (slot.Itemstack.Attributes["expandedSats"] as FloatArrayAttribute)?.value;

            if (addSat != null && addSat.Length == 6)
                sat = sat.Zip(addSat, (x, y) => x + (y / slices)).ToArray();

            if (addIngs != null && addIngs.Length > 0)
            {
                foreach (string aL in addIngs)
                {
                    if (ingredients.Contains(aL))
                        continue;

                    ingredients.Add(aL);
                }
            }
        }
        else
        {
            GetNutrientsFromIngredient(ref sat, slot.Itemstack.Collectible, 1);
            string aL = slot.Itemstack.Collectible.Code.Domain + ":" + slot.Itemstack.Collectible.Code.Path;

            ingredients.Add(aL);

        }

        ingredients.Sort();

        output.Attributes["madeWith"] = new StringArrayAttribute(ingredients.ToArray());
        output.Attributes["expandedSats"] = new FloatArrayAttribute(sat.ToArray());
    }
}