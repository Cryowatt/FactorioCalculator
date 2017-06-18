using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioCalculator
{
    public class Recipe
    {
        public Resource Resource { get; set; }
        public double Time { get; set; }
        public Producer Producer { get; set; }
        public Dictionary<Resource, int> Input { get; set; }
        public Dictionary<Resource, int> Yield { get; set; }

    }
}
