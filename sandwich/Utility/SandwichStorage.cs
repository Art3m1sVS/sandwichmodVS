using System.Collections.Generic;

namespace sandwich
{
    public static class SandwichStorage
    {
        // Dictionary where the key is the sandwich name, and the value contains the required slices and ingredients
        public static Dictionary<string, SandwichData> SandwichTypes = new Dictionary<string, SandwichData>
        {
            { "peanutbuttersandwich", new SandwichData(new List<string> { "honeybread", "peanutbutterbread" }) },
            { "cheesebreadslice", new SandwichData(new List<string> { "breadslice", "cheeseslice" }) },
            { "redmeatbread", new SandwichData(new List<string> { "breadslice", "redmeatslice" }) },
            { "cheesesandwich", new SandwichData(new List<string> { "cheesebreadslice", "breadslice" }) },
            { "redmeatsandwich", new SandwichData(new List<string> { "redmeatbread", "breadslice" }) },
            { "redmeatcheesesandwich", new SandwichData(new List<string> { "redmeatbread", "cheesebreadslice" }) },
        };
    }

    public class SandwichData
    {
        public List<string> RequiredSlices { get; set; }

        public SandwichData(List<string> requiredSlices)
        {
            RequiredSlices = requiredSlices;
        }

        public string GetPriorityVariant()
        {
            // Extract the variant from the first required slice
            string firstSlice = RequiredSlices[0];
            string variant = firstSlice.Contains("-") ? firstSlice.Split('-')[1] : "";
            return variant;
        }
    }
}
