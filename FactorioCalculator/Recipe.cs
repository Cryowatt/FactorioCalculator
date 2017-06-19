using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FactorioCalculator
{
    public class Recipe
    {
        public Recipe(double time, Producer producer, Dictionary<string, double> input, Dictionary<string, double> yield)
        {
            this.Time = time;
            this.Producer = producer;
            this.Input = input;
            this.Yield = yield;
        }

        public double Time { get; set; }
        public Producer Producer { get; set; }
        public Dictionary<string, double> Input { get; set; }
        public Dictionary<string, double> Yield { get; set; }

        public override string ToString()
        {
            string inputs = string.Join(" + ", this.Input.Select(o => $"{o.Key}({o.Value})"));
            string outputs = string.Join(" + ", this.Yield.Select(o => $"{o.Key}({o.Value})"));
            return $"[{Producer}:{Time}] {inputs} => {outputs}";
        }
    }
}
