using System.Collections.Generic;
using sandwich;

namespace sandwich
{
    public static class BreadStorage
    {
        // Dictionary to store allowed bread types
        // Key is the bread type, and value is any additional properties or classifications (like if it's "perfect")
        public static Dictionary<string, BreadData> BreadTypes = new Dictionary<string, BreadData>
        {
            { "bread-rye-perfect", new BreadData("perfect") },
            { "bread-spelt-perfect", new BreadData("perfect") },
            { "bread-rice-perfect", new BreadData("perfect") },
            { "bread-cassava-perfect", new BreadData("perfect") },
            { "bread-flax-perfect", new BreadData("perfect") },
            { "bread-amaranth-perfect", new BreadData("perfect") },
            { "bread-sunflower-perfect", new BreadData("perfect") },
        };
    }

    public class BreadData
    {
        public string Classification { get; set; }

        public BreadData(string classification)
        {
            Classification = classification;
        }
    }
}
