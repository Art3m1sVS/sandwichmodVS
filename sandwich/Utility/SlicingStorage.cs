using System.Collections.Generic;
using Vintagestory.API.Common;

namespace sandwich
{
    public static class SlicingStorage
    {
        public static Dictionary<string, SlicingData> SlicingItems = new Dictionary<string, SlicingData>
        {
            // Sammiches
            { "bread-rye-perfect", new SlicingData("sandwich:slicedbread-rye-perfect", 4) },
            { "bread-spelt-perfect", new SlicingData("sandwich:slicedbread-spelt-perfect", 4) },
            { "bread-rice-perfect", new SlicingData("sandwich:slicedbread-rice-perfect", 4) },
            { "bread-sunflower-perfect", new SlicingData("sandwich:slicedbread-sunflower-perfect", 4) },
            { "bread-flax-perfect", new SlicingData("sandwich:slicedbread-flax-perfect", 4) },
            { "bread-amaranth-perfect", new SlicingData("sandwich:slicedbread-amaranth-perfect", 4) },
            { "bread-cassava-perfect", new SlicingData("sandwich:slicedbread-cassava-perfect", 4) },

            { "bushmeat-cooked", new SlicingData("sandwich:slicedmeat-generic", 2) },
            { "bushmeat-cured", new SlicingData("sandwich:slicedmeat-generic", 2) },
            { "bushmeat-charred", new SlicingData("sandwich:slicedmeat-charred", 2) },
            { "redmeat-cooked", new SlicingData("sandwich:slicedmeat-generic", 2) },
            { "redmeat-cured", new SlicingData("sandwich:slicedmeat-generic", 2) },
            { "redmeat-charred", new SlicingData("sandwich:slicedmeat-charred", 2) },
            { "cheese-blue-4slice", new SlicingData("game:cheese-blue-1slice", 4) },
            { "cheese-cheddar-4slice", new SlicingData("game:cheese-cheddar-1slice", 4) },
            { "cheese-cheddar-1slice", new SlicingData("sandwich:slicedcheese", 4) },
            { "pumpkin-fruit-4", new SlicingData("game:vegetable-pumpkin", 4) },
            { "vegetable-cabbage", new SlicingData("sandwich:slicedcabbage", 4) },

            // Sausagedog
            { "sausage-bushmeat-cooked", new SlicingData("sandwich:sausagedog-bushmeat-cooked", 4) },
            { "sausage-bushmeatcheese-cooked", new SlicingData("sandwich:sausagedog-bushmeatcheese-cooked", 4) },
            { "sausage-poultry-cooked", new SlicingData("sandwich:sausagedog-poultry-cooked", 4) },
            { "sausage-poultrycheese-cooked", new SlicingData("sandwich:sausagedog-poultrycheese-cooked", 4) },
            { "sausage-redmeat-cooked", new SlicingData("sandwich:sausagedog-redmeat-cooked", 4) },
            { "sausage-redmeatcheese-cooked", new SlicingData("sandwich:sausagedog-redmeatcheese-cooked", 4) },
            { "sausage-bushmeat-dried", new SlicingData("sandwich:sausagedog-bushmeat-dried", 4) },
            { "sausage-bushmeatcheese-dried", new SlicingData("sandwich:sausagedog-bushmeatcheese-dried", 4) },
            { "sausage-poultry-dried", new SlicingData("sandwich:sausagedog-poultry-dried", 4) },
            { "sausage-poultrycheese-dried", new SlicingData("sandwich:sausagedog-poultrycheese-dried", 4) },
            { "sausage-redmeat-dried", new SlicingData("sandwich:sausagedog-redmeat-dried", 4) },
            { "sausage-redmeatcheese-dried", new SlicingData("sandwich:sausagedog-redmeatcheese-dried", 4) },
            { "sausage-bushmeat-cured", new SlicingData("sandwich:sausagedog-bushmeat-cured", 4) },
            { "sausage-bushmeatcheese-cured", new SlicingData("sandwich:sausagedog-bushmeatcheese-cured", 4) },
            { "sausage-poultry-cured", new SlicingData("sandwich:sausagedog-poultry-cured", 4) },
            { "sausage-poultrycheese-cured", new SlicingData("sandwich:sausagedog-poultrycheese-cured", 4) },
            { "sausage-redmeat-cured", new SlicingData("sandwich:sausagedog-redmeat-cured", 4) },
            { "sausage-redmeatcheese-cured", new SlicingData("sandwich:sausagedog-redmeatcheese-cured", 4) },
            { "sausage-bushmeat-driedcooked", new SlicingData("sandwich:sausagedog-bushmeat-driedcooked", 4) },
            { "sausage-bushmeatcheese-driedcooked", new SlicingData("sandwich:sausagedog-bushmeatcheese-driedcooked", 4) },
            { "sausage-poultry-driedcooked", new SlicingData("sandwich:sausagedog-poultry-driedcooked", 4) },
            { "sausage-poultrycheese-driedcooked", new SlicingData("sandwich:sausagedog-poultrycheese-driedcooked", 4) },
            { "sausage-redmeat-driedcooked", new SlicingData("sandwich:sausagedog-redmeat-driedcooked", 4) },
            { "sausage-redmeatcheese-driedcooked", new SlicingData("sandwich:sausagedog-redmeatcheese-driedcooked", 4) },

            // Salami
            { "sausagedog-bushmeat-cooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeatcheese-cooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultry-cooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultrycheese-cooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeat-cooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeatcheese-cooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeat-dried", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeatcheese-dried", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultry-dried", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultrycheese-dried", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeat-dried", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeatcheese-dried", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeat-cured", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeatcheese-cured", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultry-cured", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultrycheese-cured", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeat-cured", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeatcheese-cured", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeat-driedcooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-bushmeatcheese-driedcooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultry-driedcooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-poultrycheese-driedcooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeat-driedcooked", new SlicingData("sandwich:slicedsausage", 8) },
            { "sausagedog-redmeatcheese-driedcooked", new SlicingData("sandwich:slicedsausage", 8) },

            //Ancient Tools
            { "bread-birch-perfect", new SlicingData("sandwich:slicedbread-birch-perfect", 4) },
            { "bread-pine-perfect", new SlicingData("sandwich:slicedbread-pine-perfect", 4) },
            { "bread-maple-perfect", new SlicingData("sandwich:slicedbread-maple-perfect", 4) },

            // Expanded Foods
            { "slicedcabbage", new SlicingData("expandedfoods:choppedvegetable-cabbage", 1) },
            { "vegetable-carrot", new SlicingData("expandedfoods:choppedvegetable-carrot", 1) },
            { "vegetable-turnip", new SlicingData("expandedfoods:choppedvegetable-turnip", 1) },
            { "vegetable-parsnip", new SlicingData("expandedfoods:choppedvegetable-parsnip", 1) },
            { "vegetable-onion", new SlicingData("expandedfoods:choppedvegetable-onion", 2) },
            { "vegetable-pumpkin", new SlicingData("expandedfoods:choppedvegetable-pumpkin", 2) },

            { "pickledvegetable-cabbage", new SlicingData("expandedfoods:choppedvegetable-pickledcabbage", 1) },
            { "pickledvegetable-carrot", new SlicingData("expandedfoods:choppedvegetable-pickledcarrot", 1) },
            { "pickledvegetable-turnip", new SlicingData("expandedfoods:choppedvegetable-pickledturnip", 1) },
            { "pickledvegetable-parsnip", new SlicingData("expandedfoods:choppedvegetable-pickledparsnip", 1) },
            { "pickledvegetable-onion", new SlicingData("expandedfoods:choppedvegetable-pickledonion", 2) },
            { "pickledvegetable-pumpkin", new SlicingData("expandedfoods:choppedvegetable-pickledpumpkin", 2) },
        };
    }

    public static class BakingStorage
    {
        // List of bakeable items, using JSON wildcards for flexibility
        public static List<string> BakeableItems = new List<string>
        {
            "slicedbread-*",
            "slicedfruitbread-*",
            "burgerbun-*",
            "burgerbuntop-*",
            "burgerbunbottom-*"
        };
        public static bool IsBakeable(string item)
        {
            foreach (var bakeable in BakeableItems)
            {
                if (DoesItemMatch(item, bakeable))
                {
                    return true;
                }
            }
            return false;
        }

        // Helper method to check if the item matches the wildcard pattern
        private static bool DoesItemMatch(string item, string wildcard)
        {
            // Simple wildcard matching logic
            // Supports '*' as a wildcard character
            if (wildcard.Contains('*'))
            {
                var parts = wildcard.Split('*');
                if (parts.Length == 0) return false;

                bool startsWith = item.StartsWith(parts[0]);
                bool endsWith = parts.Length == 1 || item.EndsWith(parts[1]);

                return startsWith && endsWith;
            }
            return item == wildcard; // Direct match if no wildcard
        }
    }
    public class SlicingData
    {
        public AssetLocation OutputAsset { get; set; }
        public int OutputQuantity { get; set; }

        public SlicingData(string assetLocation, int quantity)
        {
            OutputAsset = new AssetLocation(assetLocation);
            OutputQuantity = quantity;
        }
    }
}
