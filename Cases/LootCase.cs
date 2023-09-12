using CaseSimulatorBot.Models;
using CaseSimulatorBot.Models.Interfaces;
using CaseSimulatorBot.Utils;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Cases
{
    internal class LootCase
    {

        public Case CaseInfo { get; }

        private readonly IUserPulls _pulls;
        private readonly Random _random;

        public LootCase(Case caseInfo, IUserPulls pulls)
        {
            CaseInfo = caseInfo;
            _pulls = pulls;
            _random = new Random();
        }

        public Item OpenCase()
        {
            List<Fruit> fruits = CaseInfo.Fruits!;
            int dropSum = 0;


            foreach (var fruit in fruits)
            {
                dropSum += fruit.DropChance;
            }
            int randomNumber = _random.Next(0, dropSum + 1);
            fruits.OrderByDescending(x => x.DropChance);
            int currentSum = 0;
            for (int i = 0; i < fruits.Count; i++)
            {
                currentSum += fruits[i].DropChance;
                if (randomNumber >= currentSum - fruits[i].DropChance && randomNumber <= currentSum)
                {
                    Item item = new Item();
                    item.Fruit = fruits[i];
                    item = RareFruitGuaranteed(item);
                    fruits[i].Items.Add(item);

                    return item;
                }
            }

            throw new Exception("Unsuccessful attempt to open case");
        }


        private Item RareFruitGuaranteed(Item item)
        {
            if (item.Fruit!.Rarity == "Legendary" || item.Fruit.Rarity == "Mythical")
                return item;
            var fruits = CaseInfo.Fruits!;
            int tentDigit = GetDigit(_pulls.Pulls, 2);
            int tenDigitAfter = GetDigit(_pulls.Pulls + 1, 2);

            if (tenDigitAfter > tentDigit || (tentDigit == 9 && tenDigitAfter == 0))
            {
                List<Fruit> rareFruits = fruits.Where(x => x.Rarity == "Rare").ToList();
                int rnd = new Random().Next(0, rareFruits.Count);
                var result = new Item();
                result.Fruit = rareFruits[rnd];
                rareFruits[rnd].Items.Add(item);
                return result;
            }
            return item;
        }

        private int GetDigit(int x, int digitNumber)
        {
            if (digitNumber < 0)
                throw new ArgumentOutOfRangeException("Digit number must be bigger than zero");

            int digitCount = (int)Math.Log10(x) + 1;
            if (digitNumber > digitCount)
                return 0;
            var pow = (int)Math.Pow(10, digitNumber - 1);
            return (x / pow) % 10;
        }
    }
}
