using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace FactorioCalculator
{
    class Program
    {
        public static object Ingredients { get; private set; }

        static void Main(string[] args)
        {
            string factorioDataPath = args[0];
            string recipePath = Path.Combine(factorioDataPath, @"base\prototypes\recipe");

            var script = @"data = {}
function data:extend(data)
  return data
end


return ";
            var allRecipes = from file in Directory.EnumerateFiles(recipePath, "*.lua")
                             let data = Script.RunString(script + File.ReadAllText(file))
                             from entry in data.Table.Values
                             let recipe = entry.Table
                             select CreateRecipe(recipe);

            foreach (var recipe in allRecipes)
            {
                Console.WriteLine(recipe);
            }
        }

        private static Recipe CreateRecipe(Table recipe)
        {
            var results = recipe.RawGet("results")?.Table;
            Dictionary<string, double> yield;

            if (results != null)
            {
                yield = ConvertIngredients(results);
            }
            else
            {
                yield = new Dictionary<string, double>
                {
                    { (string)recipe["result"],  (double)(recipe["result_count"] ?? 1.0) }
                };
            }

            return new Recipe(
                (double)(recipe["energy_required"] ?? 0.5),
                ConvertCategory(recipe["category"]),
                ConvertIngredients(recipe.RawGet("ingredients").Table),
                yield);
        }

        private static Dictionary<string, double> ConvertIngredients(Table ingedientList)
        {
            return ingedientList.Values.Select(o => o.Table).ToDictionary(o => (string)(o["name"] ?? o[1]), o => (double)(o["amount"] ?? o[2]));
        }

        private static Producer ConvertCategory(object category)
        {
            if (category == null)
            {
                return Producer.Assembler;
            }
            else
            {
                switch ((string)category)
                {
                    case "smelting":
                        return Producer.Furnace;
                    case "oil-processing":
                        return Producer.OilRefinery;
                    case "chemistry":
                        return Producer.ChemicalPlant;
                    case "advanced-crafting":
                    case "crafting":
                    case "crafting-with-fluid":
                        return Producer.Assembler;
                    case "rocket-building":
                        return Producer.Rocket;
                    default:
                        throw new InvalidDataException($"Don't know what a {category} is.");
                }
            }
        }
    }
}