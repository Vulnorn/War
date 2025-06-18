using System;
using System.Collections.Generic;

namespace War
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Battle battle = new Battle();
            battle.War();
        }
    }

    public interface IDamageable
    {
        void TakeDamage(int damege);
    }

    class Battle
    {
        private Platoon _platoonBlue = new Platoon();
        private Platoon _platoonRed = new Platoon();
        private ConsoleColor _colorBlue = ConsoleColor.Blue;
        private ConsoleColor _colorRed = ConsoleColor.Red;

        public void War()
        {
            while (_platoonRed.UnitsCount > 0 && _platoonBlue.UnitsCount > 0)
            {
                ShowPlatoon(_colorBlue, _platoonBlue);
                ShowPlatoon(_colorRed, _platoonRed);

                Console.WriteLine();
                Console.ForegroundColor = _colorBlue;
                _platoonBlue.Attack(_platoonRed.GetUnitList());
                Console.WriteLine($"\n{new string('_', 25)}\n");

                Console.ForegroundColor = _colorRed;
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
            if (_platoonRed.UnitsCount == 0 && _platoonBlue.UnitsCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Ничья");
                return;
            }

            if (_platoonRed.UnitsCount == 0)
            {
                Console.ForegroundColor = _colorBlue;
                Console.WriteLine($"Победила синяя команда");
                return;
            }

            if (_platoonBlue.UnitsCount == 0)
            {
                Console.ForegroundColor = _colorRed;
                Console.WriteLine($"Победила красная команда");
                return;
            }
        }

        private void ShowPlatoon(ConsoleColor _color, Platoon platoon)
        {
            Console.ForegroundColor = _color;
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
            _units = _platoonFactory.GetList();
        }

        public int UnitsCount => _units.Count;

        public void Attack(List<IDamageable> targets)
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
            for (int i = _units.Count-1; i > -1; i--)
            {
                if (_units[i].Health <= 0)
                    _units.RemoveAt(i);
            }
        }

        public void ShowInfo()
        {
            Console.WriteLine("Состав взвода:");

            foreach (var unit in _units)
            {
                unit.ShowInfo();
            }
        }

        public List<IDamageable> GetUnitList()
        {
            return new List<IDamageable>(_units);
        }
    }

    class PlatoonFactory
    {
        private UnitListFactory _unitFactory = new UnitListFactory();
        private List<Unit> _platoonFactory = new List<Unit>();

        public PlatoonFactory()
        {
            Create();
        }

        private void Create()
        {
            for (int i = 0; i < _unitFactory.GetCount(); i++)
                _platoonFactory.Add(_unitFactory.GetList()[i].Clone());
        }

        public List<Unit> GetList()
        {
            return new List<Unit>(_platoonFactory);
        }
    }

    class UnitListFactory
    {
        private List<Unit> _barracks = new List<Unit>();

        public UnitListFactory()
        {
            Create();
        }

        private void Create()
        {
            _barracks.Add(new Unit("Пехотинец", 1000, 15, 5));
            _barracks.Add(new Sniper("Снайпер", 1000, 15, 5));
            _barracks.Add(new Gunner("Пулеметчик", 1000, 15, 5));
            _barracks.Add(new Mortar("Минаметчик", 1000, 15, 5));
        }

        public List<Unit> GetList()
        {
            return new List<Unit>(_barracks);
        }

        public int GetCount()
        {
            return _barracks.Count;
        }
    }


    class Unit : IDamageable
    {
        public Unit(string name, int heals, int damege, int defens)
        {
            Name = name;
            Health = heals;
            Damage = damege;
            Defens = defens;
        }

        public string Name { get; protected set; }
        public int Health { get; protected set; }
        public int Damage { get; protected set; }
        public int Defens { get; protected set; }

        public virtual void ShowInfo()
        {
            Console.WriteLine($"{Name} - Здоровье {Health}; Урон {Damage}; Защита {Defens}");
        }

        public virtual void TakeDamage(int damage)
        {
            damage = Math.Max(damage - Defens, 0);

            Health -= damage;

            Console.WriteLine($"{Name} - {damage} ХП");
        }

        public virtual void Attack(List<IDamageable> targets)
        {
            int index = Utilite.GenerateRandomNumber(0, targets.Count);

            IDamageable target = targets[index];

            Console.WriteLine($"{Name} атакует:");
            target.TakeDamage(Damage);
        }

        public virtual Unit Clone()
        {
            return new Unit(Name, Health, Damage, Defens);
        }
    }

    class Sniper : Unit
    {
        private int _multiplier = 5;

        public Sniper(string name, int heals, int damage, int defens) : base(name, heals, damage, defens) { }

        public override void Attack(List<IDamageable> targets)
        {
            int index = Utilite.GenerateRandomNumber(0, targets.Count);

            int totalDamage = Damage * _multiplier;

            Console.WriteLine($"{Name} атакует:");
            targets[index].TakeDamage(totalDamage);
        }

        public override Unit Clone()
        {
            return new Sniper(Name, Health, Damage, Defens);
        }
    }

    class Gunner : Unit
    {
        private int _targetCount = 3;

        public Gunner(string name, int heals, int damage, int defens) : base(name, heals, damage, defens) { }

        public override void Attack(List<IDamageable> targets)
        {
            List<IDamageable> targets2 = new List<IDamageable>();

            for (int i = 0; i < _targetCount; i++)
            {
                int index = Utilite.GenerateRandomNumber(0, targets.Count);

                targets2.Add(targets[index]);
            }

            Console.Write($"{Name} атакует: ");

            for (int i = 0; i < targets2.Count; i++)
                targets2[i].TakeDamage(Damage);


            Console.WriteLine();
        }

        public override Unit Clone()
        {
            return new Gunner(Name, Health, Damage, Defens);
        }
    }

    class Mortar : Unit
    {
        private int _targetCount = 5;

        public Mortar(string name, int heals, int damage, int defens) : base(name, heals, damage, defens) { }

        public override void Attack(List<IDamageable> targets)
        {
            Console.Write($"{Name} атакует: ");

            for (int i = 0; i < _targetCount; i++)
            {
                int index = Utilite.GenerateRandomNumber(0, targets.Count);

                targets[index].TakeDamage(Damage);
            }
        }

        public override Unit Clone()
        {
            return new Mortar(Name, Health, Damage, Defens);
        }
    }

    class Utilite
    {
        private static Random s_random = new Random();

        public static int GenerateRandomNumber(int lowerLimitRangeRandom, int upperLimitRangeRandom)
        {
            return s_random.Next(lowerLimitRangeRandom, upperLimitRangeRandom);
        }
    }
}