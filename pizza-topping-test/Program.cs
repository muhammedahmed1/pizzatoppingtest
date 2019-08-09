using System;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace pizza_topping_test
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            List<Order> pizzas = GetPizzas();
            if (pizzas == null) return;

            IEnumerable<ToppingCombination> allToppingCombinations = GetToppingCombinations(pizzas);

            int top = 20;
            IEnumerable<ToppingCombination> mostPopularToppings = allToppingCombinations.OrderByDescending(ag => ag.count).Take(top);

            int i = 1;
            foreach (ToppingCombination combo in mostPopularToppings)
            {
                Console.WriteLine(String.Format("Rank: {0, 4} \t Order Count: {1, 5} \t Combinations: {2} ", i, combo.count, combo.toppings));
                i++;
            }

            Console.ReadLine();
        }

        public static List<Order> GetPizzas()
        {
            string url = ConfigurationManager.AppSettings["BaseURL"].ToString();
            HttpWebRequest WebRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            List<Order> order = new List<Order>();
            using (HttpWebResponse response = WebRequest.GetResponse() as HttpWebResponse)
            {
                Stream stream = response.GetResponseStream();
                string json = new StreamReader(stream).ReadToEnd();
                stream.Close();

                order = JsonConvert.DeserializeObject<List<Order>>(json);
            }
            return order;
        }

        public static IEnumerable<ToppingCombination> GetToppingCombinations(List<Order> order)
        {            
            var orderedPizzaToppings = order.Select(p => p.toppings.OrderBy(t => t));            
            IEnumerable<string> combination = orderedPizzaToppings.Select((toppings => toppings.Aggregate((x, y) => x + "," + y)));
            IEnumerable<ToppingCombination> groupedToppings = combination
                .GroupBy(toppingsGroup => toppingsGroup)
                .Select(toppingsGroup => new ToppingCombination()
                {
                    toppings = toppingsGroup.Key,
                    count = toppingsGroup.Count()
                });

            return groupedToppings;
        }

        public class Order
        {
            public List<string> toppings { get; set; }
        }

        public class ToppingCombination
        {
            public string toppings;
            public int count;
        }
    }    
}
