using System;
using System.Collections.Generic;
using System.Threading;

namespace PPD_Lab1
{
    public struct Product
    {
        public string Name;
        public int Price;

        public Product(string name, int price)
        {
            Price = price;
            Name = name;
        }
    }

    public struct Bill
    {
        public Dictionary<Product, int> items;
        public int totalPrice;
    }
    
    class Program
    {
        private List<Bill> _sales = new List<Bill>();
        private Dictionary<Product, int> _inventory = new ();
        private int _totalMoney = 0;

        void AddProducts()
        {
            for (int i = 0; i < 100; i++)
            {
                _inventory.Add(new Product($"product{i}", i), 100);
            }
            
        }
        
        static void Main(string[] args)
        {
            
        }
    }
}
