using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Models
{
    internal class Item
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }   
        public int FruitId { get; set; }
        public string? Date { get; set; }

        public User? User { get; set; }
        public Fruit? Fruit { get; set; }

    }
}
