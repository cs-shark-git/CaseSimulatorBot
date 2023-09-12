using CaseSimulatorBot.Models;
using CaseSimulatorBot.Utils;
using CaseSimulatorBot.Utils.CollectionsExtensions;

namespace CaseSimulatorBot.Cases
{
    internal class LootCaseService
    {
        private User _user;
        public int DefaultCasePrice{ get; }

        private Dictionary<string, LootCase> _cases;
        private Random _random;
        private Case _defaultCaseInfo;
        public LootCaseService(User user)
        {
            _user = user;
            _defaultCaseInfo = DBService.GetCases().First();
            _cases = InitCases();
            _random = new Random();
            DefaultCasePrice = _defaultCaseInfo.Price;
        }
        public Item OpenDefaultCase()
        {
            Item item = _cases.First().Value.OpenCase();
            DBService.AddItemToUserInventory(_user.Id, item);
            DBService.SetUserPulls(_user.Id, _user.Pulls + 1);
            return item;
        }
        public IEnumerable<Item> OpenDefaultCaseRange(int range)
        {
            List<Item> items = new List<Item>();

            for (int i = 0; i < range; i++) 
            {
                _user.Pulls += 1;
                Item item = _cases.First().Value.OpenCase();
                DBService.AddItemToUserInventory(_user.Id, item);
                DBService.SetUserPulls(_user.Id, _user.Pulls);
                items.Add(item);
            }            
            items.Shuffle();
            return items;
        }

        private Dictionary<string, LootCase> InitCases()
        {
            _cases = new Dictionary<string, LootCase>
            {
                {
                    "Default", new LootCase(_defaultCaseInfo, _user)
                },             
            };
            return _cases;
        }
    }
}
