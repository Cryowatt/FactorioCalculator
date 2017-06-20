using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FactorioCalculator
{
    public class Recipe
    {
        public Recipe(string name, double time, Producer producer, Dictionary<string, double> input, Dictionary<string, double> yield)
        {
            this.Name = name;
            this.Time = time;
            this.Producer = producer;
            this.Input = input;
            this.Yield = yield;
        }

        public string Name { get; set; }
        public double Time { get; set; }
        public Producer Producer { get; set; }
        public Dictionary<string, double> Input { get; set; }
        public Dictionary<string, double> Yield { get; set; }

        public override string ToString()
        {
            string inputs = string.Join(" + ", this.Input.Select(o => $"{o.Key}({o.Value})"));
            string outputs = string.Join(" + ", this.Yield.Select(o => $"{o.Key}({o.Value})"));
            return $"{Name}: {inputs} =[{Producer}:{Time}]> {outputs}";
        }
    }
}
