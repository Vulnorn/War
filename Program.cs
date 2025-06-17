using System;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using War;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace War
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Battel battel = new Battel();
            battel.War();
        }
    }

    class Battel
    {
        private Platoon _platoonBlue = new Platoon();
        private Platoon _platoonRed = new Platoon();
        private int _numderColorBlue = 9;
        private int _numderColorRed = 12;

        public void War()
        {
            while (_platoonRed.GetUnitList().Count > 0 && _platoonBlue.GetUnitList().Count > 0)
            {
                ShowPlatoon(_numderColorBlue, _platoonBlue);
                ShowPlatoon(_numderColorRed, _platoonRed);

                Console.WriteLine();
                Console.ForegroundColor = (ConsoleColor)_numderColorBlue;
                _platoonBlue.Attack(_platoonRed.GetUnitList());
                Console.WriteLine($"\n{new string('_', 25)}\n");

                Console.ForegroundColor = (ConsoleColor)_numderColorRed;
                _platoonRed.Attack(_platoonBlue.GetUnitList());
                Console.WriteLine($"\n{new string('_', 25)}\n");

                _platoonBlue.RemoveDead();
                _platoonRed.RemoveDead();

                Console.ReadKey();
            }

            ShowResult();
        }

        private void ShowResult()
        {
            if (_platoonRed.GetUnitList().Count == 0 && _platoonBlue.GetUnitList().Count == 0)
            {
                Console.WriteLine($"Ничья");
                return;
            }

            if (_platoonRed.GetUnitList().Count == 0)
            {
                Console.WriteLine($"Победила синяя команда");
                return;
            }

            if ( _platoonBlue.GetUnitList().Count == 0)
            {
                Console.WriteLine($"Победила красная команда");
                return;
            }
        }

        private void ShowPlatoon(int numberColor, Platoon platoon)
        {
            Console.ForegroundColor = (ConsoleColor)numberColor;
            Console.WriteLine($"{new string('_', 25)}");
            platoon.ShowInfo();
            Console.WriteLine($"{new string('_', 25)}");
        }
    }

    class Platoon
    {
        private List<Unit> _units;
        private PlatoonFactory _platoonFactory = new PlatoonFactory();

        public Platoon()
        {
            _units = _platoonFactory.Create();
        }

        public int CountUnits { get; private set; }

        public void Attack(List<Unit> targets)
        {
            if (targets.Count == 0)
            {
                Console.WriteLine("Нет доступных целей для атаки.");
            }
            else
            {
                foreach (var unit in _units)
                {
                    unit.Attack(targets);
                }
            }
        }

        public void RemoveDead()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i].Heals <= 0)
                    _units.RemoveAt(i);
            }
        }

        public void ShowInfo()
        {
            Console.WriteLine("Состав взвода:");

            foreach (var unit in _units)
            {
                unit.ShoyInfo();
            }
        }

        public List<Unit> GetUnitList()
        {
            return new List<Unit>(_units);
        }
    }

    class PlatoonFactory
    {
        private List<Unit> _barracks = new List<Unit>();

        public PlatoonFactory()
        {
            CreateUnits();
        }

        private void CreateUnits()
        {
            _barracks.Add(new Unit("Пехотинец", 1000, 15, 5));
            _barracks.Add(new Sniper("Снайпер", 1000, 15, 5));
            _barracks.Add(new Gunner("Пулеметчик", 1000, 15, 5));
            _barracks.Add(new Mortar("Минаметчик", 1000, 15, 5));
        }

        public List<Unit> Create()
        {
            List<Unit> units = new List<Unit>();

            for (int i = 0; i < _barracks.Count; i++)
            {
                units.Add(_barracks[i].Clone());
            }

            return units;
        }
    }

    class Unit
    {
        public string Name { get; protected set; }
        public int Heals { get; protected set; }
        public int Damage { get; protected set; }
        public int Defens { get; protected set; }

        public Unit(string name, int heals, int damege, int defens)
        {
            Name = name;
            Heals = heals;
            Damage = damege;
            Defens = defens;
        }

        public virtual void ShoyInfo()
        {
            Console.WriteLine($"{Name} - Здоровье {Heals}; Урон {Damage}; Защита {Defens}");
        }

        public virtual void TakeDamage(int damage)
        {
            int effectiveDamage = damage - Defens;

            if (effectiveDamage < 0)
                effectiveDamage = 0;

            Heals -= effectiveDamage;

            if (Heals <= 0)
            {
                Console.WriteLine($"{Name} погиб.");
            }
        }

        public virtual void Attack(List<Unit> targets)
        {
            int index = Utilite.GenerateRandomNumber(0, targets.Count);
            Unit target = targets[index];

            Console.WriteLine($"{Name} атакует: {target.Name}.");
            target.TakeDamage(Damage);
        }

        public virtual Unit Clone()
        {
            return new Unit(Name, Heals, Damage, Defens);
        }
    }

    class Sniper : Unit
    {
        private int _multiplier = 5;

        public Sniper(string name, int heals, int damage, int defens) : base(name, heals, damage, defens) { }

        public override void Attack(List<Unit> targets)
        {
            int index = Utilite.GenerateRandomNumber(0, targets.Count);
            Unit target = targets[index];
            int totalDamage = Damage * _multiplier;

            Console.WriteLine($"{Name} атакует: {target.Name}.");
            target.TakeDamage(totalDamage);
        }

        public override Unit Clone()
        {
            return new Sniper(Name, Heals, Damage, Defens);
        }
    }

    class Gunner : Unit
    {
        private int _targetCount = 3;

        public Gunner(string name, int heals, int damage, int defens) : base(name, heals, damage, defens) { }

        public override void Attack(List<Unit> targets)
        {
            List<Unit> targets2 = new List<Unit>();

            for (int i = 0; i < _targetCount;i++)
            {
                int index = Utilite.GenerateRandomNumber(0, targets.Count);

                targets2.Add(targets[index]);
            }

            Console.Write($"{Name} атакует: ");

            foreach (var target in targets2)
            {
                if (target != null)
                {
                    Console.Write($"{target.Name}, ");
                    target.TakeDamage(Damage);
                }
            }

            Console.WriteLine();
        }

        public override Unit Clone()
        {
            return new Gunner(Name, Heals, Damage, Defens);
        }
    }

    class Mortar : Unit
    {
        private int _targetCount = 5;

        public Mortar(string name, int heals, int damage, int defens) : base(name, heals, damage, defens) { }

        public override void Attack(List<Unit> targets)
        {
            Console.Write($"{Name} атакует: ");

            for (int i = 0; i < _targetCount; i++)
            {
                int index = Utilite.GenerateRandomNumber(0, targets.Count);

                Console.Write($"{targets[index].Name}, ");
                targets[index].TakeDamage(Damage);
            }
        }

        public override Unit Clone()
        {
            return new Mortar(Name, Heals, Damage, Defens);
        }
    }

    class Utilite
    {
        private static Random s_random = new Random();

        public static int GenerateRandomNumber(int lowerLimitRangeRandom, int upperLimitRangeRandom)
        {
            int numberRandom = s_random.Next(lowerLimitRangeRandom, upperLimitRangeRandom);
            return numberRandom;
        }

        public static int GetNumberInRange(int lowerLimitRangeNumbers = Int32.MinValue, int upperLimitRangeNumbers = Int32.MaxValue)
        {
            bool isEnterNumber = true;
            int enterNumber = 0;
            string userInput;

            while (isEnterNumber)
            {
                Console.WriteLine($"Введите число.");

                userInput = Console.ReadLine();

                if (int.TryParse(userInput, out enterNumber) == false)
                    Console.WriteLine("Не корректный ввод.");
                else if (VerifyForAcceptableNumber(enterNumber, lowerLimitRangeNumbers, upperLimitRangeNumbers))
                    isEnterNumber = false;
            }

            return enterNumber;
        }

        private static bool VerifyForAcceptableNumber(int number, int lowerLimitRangeNumbers, int upperLimitRangeNumbers)
        {
            if (number < lowerLimitRangeNumbers)
            {
                Console.WriteLine($"Число вышло за нижний предел допустимого значения.");
                return false;
            }
            else if (number > upperLimitRangeNumbers)
            {
                Console.WriteLine($"Число вышло за верхний предел допустимого значения.");
                return false;
            }

            return true;
        }
    }
}