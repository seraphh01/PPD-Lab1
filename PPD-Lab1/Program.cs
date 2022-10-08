using System;
using System.Collections.Generic;
using System.Linq;
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

        public void AddProdocut(Product product, int quantity)
        {
            if (items.ContainsKey(product))
            {
                items[product] += quantity;
            }
            else
            {
                items.Add(product, quantity);
            }

            totalPrice += product.Price * quantity;
        }
    }

    class Program
    {
        private const int MaxProducts = 100;
        private static List<Bill> _sales = new();
        private static Product[] _products;
        private static int[] _initialInventory;
        private static int[] _inventory;
        private static int _totalMoney = 0;
        private static Mutex[] _mutexes;
        private static Mutex _saleMutex = new Mutex();
        private const int numIterations = 3;
        private const int numThreads = 10;

        static void Initialize()
        {
            _products = new Product[MaxProducts];
            _initialInventory = new int[MaxProducts];
            _inventory = new int[MaxProducts];
            _mutexes = new Mutex[MaxProducts];

            for (int i = 0; i < MaxProducts; i++)
            {
                var p = new Product($"product{i}", 10);
                _products[i] = p;
                _initialInventory[i] = MaxProducts;
                _inventory[i] = MaxProducts;
                _mutexes[i] = new Mutex();
            }
        }

        Product TakeRandomProduct()
        {
            var random = new Random(0);
            return _products[random.Next(MaxProducts)];
        }

        void MakeSale(Bill bill)
        {
            _sales.Add(bill);
            _totalMoney += bill.totalPrice;
        }


        static void Main()
        {
            Initialize();
            StartThreads();
        }

        private static void StartThreads()
        {
            for (int i = 0; i < numThreads; i++)
            {
                Thread newThread = new Thread(ThreadProc)
                {
                    Name = $"Thread{i}"
                };
                newThread.Start();
            }

            Thread inventoryThread = new Thread(InventoryCheck);
            inventoryThread.Start();
        }

        private static void ThreadProc()
        {
            for (int i = 0; i < numIterations; i++)
            {
                MakeSale();
                Thread.Sleep(500);
            }
        }

        private static void MakeSale()
        {
            var rand = new Random(0);
            var maxProducts = rand.Next(8) + 3;
            var bill = new Bill {items = new Dictionary<Product, int>(), totalPrice = 0};

            
            for (int i = 0; i < maxProducts; i++)
            {
                var productIndex = rand.Next(MaxProducts);
                var quantity = rand.Next(10) + 1;
                
                var mutex = _mutexes[productIndex];
                mutex.WaitOne();
                
                var availableQuantity = Math.Min(_inventory[productIndex], quantity);
                _inventory[productIndex] -= availableQuantity;

                mutex.ReleaseMutex();
                if (availableQuantity == 0)
                {
                    continue;
                }

                var product = _products[productIndex];
                bill.AddProdocut(product, quantity);

            }

            _saleMutex.WaitOne();
            _sales.Add(bill);
            _totalMoney += bill.totalPrice;

            _saleMutex.ReleaseMutex();
        }

        private static void InventoryCheck()
        {
            while (true)
            {
                Thread.Sleep(300);
                Console.WriteLine("Checking Inventory...");
                for (int i = 0; i < MaxProducts; i++)
                {
                    _mutexes[i].WaitOne();
                }
                _saleMutex.WaitOne();
                
                var missingProducts = new int[MaxProducts];
                for (int i = 0; i < MaxProducts; i++)
                {
                    missingProducts[i] = _initialInventory[i] - _inventory[i];
                }

                var fromInventory = missingProducts.Select((m, i) => _products[i].Price * m).Sum();
                
                var sales = _sales.Select(b => b.totalPrice).Sum();
                Console.WriteLine($"House :{_totalMoney}, bills {sales}, inventory {fromInventory}");
                
                for (int i = 0; i < MaxProducts; i++)
                {
                    _mutexes[i].ReleaseMutex();
                }
                _saleMutex.ReleaseMutex();
                
                
                Console.WriteLine("Inventory checked!");
            }
        }
    }
}