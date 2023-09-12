using CaseSimulatorBot.Models;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseSimulatorBot.Utils
{
    internal static class DBService
    {

        public static User? CreateNewUser(ulong id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                try
                {
                    User user = new User()
                    {
                        Id = id,                        
                        Money = 0,
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                    return user;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static User? GetUser(ulong id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                    var user = db.Users.Where(x => x.Id == id)
                        .FirstOrDefault();
                    return user;
            }
        }

        public static void SetUserPulls(ulong id, int pulls) 
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = db.Users.Where(x => x.Id == id)
                    .FirstOrDefault();
                user!.Pulls = pulls;
                db.SaveChanges();
            }
        }

        public static User? InitializeUser(DiscordUser user)
        {
            var existingUser = GetUser(user.Id);
            if (existingUser != null) return existingUser;
            return CreateNewUser(user.Id);
        }

        public static void EditUserMoney(ulong id, int money)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                try
                {
                    User user = db.Users.Where(x => x.Id == id)
                        .First();
                    user.Money = money;
                    db.SaveChanges();
                  //  return true;
                }
                catch (InvalidOperationException)
                {
                 //   return false;
                }
            }
        }

        public static void EditUserMessageAmount(ulong id, int amount)
        {
            using (ApplicationContext db = new ApplicationContext()) 
            {
                User user = db.Users.Where(x => x.Id == id)
                    .First();
                user.MessageAmount = amount;
                db.SaveChanges(true);
            }
        }

        public static bool AddItemToUserInventory(ulong userId, Item item)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                try
                {
                    db.Entry(item.Fruit!).State = EntityState.Modified;
                    var user = db.Users.Where(x => x.Id == userId)
                        .First();
                    user.Items?.Add(item);

                    db.SaveChanges();
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        public static List<Item>? GetUserInventory(ulong userId)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var items = db.Items.Include(x => x.Fruit).Where(x => x.UserId == userId).ToList();
                return items;
            }
        }
        public static bool IsItemExistInUserInventoryByFruitName(ulong userId, string? name)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var item = GetUserInventory(userId)!.Where(x => x.Fruit!.Name!.ToLower() == name!.ToLower()).FirstOrDefault();
                return item != null;
            }
        }

        public static List<Item>? RemoveItemsFromUserInventoryByFruitName(ulong userId, string name, int amount = 0)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                List<Item> allItems = db.Items.Include(x => x.Fruit).Where(x => x.UserId == userId).Where(x => x.Fruit!.Name!.ToLower() == name.ToLower()).ToList();
                if (amount == 0)
                {
                    db.RemoveRange(allItems);
                    db.SaveChanges();
                    return allItems.ToList();
                }
                else
                {
                    if(amount > allItems.Count)
                        return null;
                    db.RemoveRange(allItems.Take(amount));
                    db.SaveChanges();
                    return allItems.Take(amount).ToList();
                }               
            }
        }

        public static int GetUserRank(ulong id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {

                return db.Users.OrderByDescending(x => x.Money)
                    .Select(x => x.Id)
                    .ToList()
                    .IndexOf(id) + 1;
            }
        }

        public static User GetItemOwner(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Items.Where(x => x.Id == id).Include(x => x.User)
                    .First().User!;
            }
        }

        public static Item? GetItemById(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var item = db.Items.Where(x => x.Id == id).Include(x => x.Fruit).FirstOrDefault();
                return item;
            }
        }

        public static void RemoveItemById(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var item = GetItemById(id);
                db.Items.Remove(item!);
                db.SaveChanges();
            }
        }

        public static void RemoveItemRange(IEnumerable<Item> items)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Items.RemoveRange(items);
                db.SaveChanges();
            }
        }

        public static List<Item> GetItemsByFruitName(string name)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Items.Include(x => x.Fruit).Where(x => x.Fruit!.Name == name).ToList();
            }
        }

        public static void RemoveItemsByFruitName(string name)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Items.RemoveRange(GetItemsByFruitName(name));
                db.SaveChanges(true);
            }
        }

        public static bool IsFruitExistByName(string name)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var value = db.Fruits.Where(x => x.Name!.ToLower() == name.ToLower()).FirstOrDefault();
                return value != null;
            }
        }

        public static List<Fruit> GetFruits() 
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Fruits.ToList();
            }
        }

        public static void AddCaseToFruit(Case @case, Fruit fruit)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Entry(fruit).State = EntityState.Modified;
                fruit.Cases.Add(@case);
                db.SaveChanges();
            }
        }

        public static void AddCaseToFruitRange(Case @case, List<Fruit> fruits)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (var fruit in fruits) 
                {
                    db.Entry(fruit).State = EntityState.Modified;
                    fruit.Cases.Add(@case);
                }
                db.SaveChanges();                
            }
        }

        public static void AddCase(Case @case) 
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (var fruit in @case.Fruits!) 
                {
                    db.Entry(fruit).State = EntityState.Modified;
                }

                db.Cases.Add(@case);
                db.SaveChanges();
            }
        }

        public static List<Case> GetCases()
        {
            using (ApplicationContext db = new ApplicationContext()) 
            {
                return db.Cases.Include(x => x.Fruits).ToList();
            }
        }
    }
}
