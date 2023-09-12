using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Models
{
    internal class Fruit
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Rarity { get; set; }

        public int DropChance { get; set; }

        public int Price { get; set; }

        public List<Item> Items { get; set; } = new List<Item>();

        public List<Case> Cases { get; set; } = new List<Case>();

    }
}
