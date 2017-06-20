using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using System.Xml.Linq;

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

            var parsedRecipes = from file in Directory.EnumerateFiles(recipePath, "*.lua")
                                let data = Script.RunString(script + File.ReadAllText(file))
                                from entry in data.Table.Values
                                let recipe = entry.Table
                                select CreateRecipe(recipe);

            var drillRecipes = new[] {
                new Recipe("raw-wood", 0.071428571, Producer.Hand, new Dictionary<string, double>(), new Dictionary<string, double>{ {"raw-wood", 2 } }),
                new Recipe("coal", 1.904761905, Producer.Drill, new Dictionary<string, double>(), new Dictionary<string, double>{ {"coal", 1 } }),
                new Recipe("iron-ore", 1.904761905, Producer.Drill, new Dictionary<string, double>(), new Dictionary<string, double>{ {"iron-ore", 1 } }),
                new Recipe("copper-ore", 1.904761905, Producer.Drill, new Dictionary<string, double>(), new Dictionary<string, double>{ {"copper-ore", 1 } }),
                new Recipe("stone", 1.538461538, Producer.Drill, new Dictionary<string, double>(), new Dictionary<string, double>{ {"stone", 1 } }),
                new Recipe("uranium-ore", 3.80952381, Producer.Drill, new Dictionary<string, double>(), new Dictionary<string, double>{ {"uranium-ore", 1 } }),
                new Recipe("water", 0.000833333, Producer.Pump, new Dictionary<string, double>(), new Dictionary<string, double>{ {"water", 1 } }),
                new Recipe("crude-oil", 1.0, Producer.Pump, new Dictionary<string, double>(), new Dictionary<string, double>{ {"crude-oil", 1 } }),
            };

            var allRecipes = parsedRecipes.Union(drillRecipes).ToList();
            var resources = (from recipe in allRecipes
                             from ingredient in recipe.Input.Union(recipe.Yield)
                             select new XElement(
                                 "{http://schemas.microsoft.com/vs/2009/dgml}Node",
                                 new XAttribute("Id", $"{ingredient.Key}"))
                            ).Distinct();
            var recipeNodes = allRecipes.Select(o => new XElement("{http://schemas.microsoft.com/vs/2009/dgml}Node", new XAttribute("Id", $"{o.Producer}::{o.Name}")));

            var inputLinks = from recipe in allRecipes
                             from ingredient in recipe.Input
                             select new XElement(
                                 "{http://schemas.microsoft.com/vs/2009/dgml}Link",
                                 new XAttribute("Source", ingredient.Key),
                                 new XAttribute("Target", $"{recipe.Producer}::{recipe.Name}"));
            var outputLinks = from recipe in allRecipes
                              from ingredient in recipe.Yield
                              select new XElement(
                                  "{http://schemas.microsoft.com/vs/2009/dgml}Link",
                                  new XAttribute("Source", $"{recipe.Producer}::{recipe.Name}"),
                                  new XAttribute("Target", ingredient.Key));


            XDocument doc = new XDocument(
                new XElement("{http://schemas.microsoft.com/vs/2009/dgml}DirectedGraph",
                    new XElement("{http://schemas.microsoft.com/vs/2009/dgml}Nodes", resources),
                    new XElement("{http://schemas.microsoft.com/vs/2009/dgml}Links", inputLinks.Union(outputLinks)))
                );

            string target = "science-pack-3";

            var single = from recipe in allRecipes
                         where recipe.Yield.ContainsKey(target)
                         select recipe;

            foreach(var thing in single)
            {
                Console.WriteLine(thing);
            }

            doc.Save("graph.dgml");
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
                (string)recipe["name"],
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