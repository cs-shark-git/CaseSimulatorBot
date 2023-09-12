using CaseSimulatorBot.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Models
{
    internal class User: IUserPulls
    {
        public ulong Id { get; set; }

        public int Money { get; set; }

        public int MessageAmount { get; set; }

        public int Pulls { get; set; }

        public List<Item>? Items { get; set; } = new List<Item>();

    }
}
