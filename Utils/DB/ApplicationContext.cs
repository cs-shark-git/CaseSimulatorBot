using CaseSimulatorBot;
using CaseSimulatorBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Utils
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public DbSet<Item> Items => Set<Item>();

        public DbSet<Fruit> Fruits => Set<Fruit>();

        public DbSet<Case> Cases => Set<Case>();

        public ApplicationContext()
        {
            //Database.EnsureCreated(); (настроена миграция)
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=application.db");
        }
    }
}
