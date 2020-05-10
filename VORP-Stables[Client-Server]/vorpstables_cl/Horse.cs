using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpstables_cl
{
    public class Horse
    {
        private string Name;
        private uint HashModel;

        private double XP;
        private int Health;
        private int Stamina;

        private uint saddle;
        private uint blanket;
        private uint mane;
        private uint tail;
        private uint bag;
        private uint bedroll;
        private uint stirups;

        public Horse(string name, uint hashModel, double xP, int health, int stamina, uint saddle, uint blanket, uint mane, uint tail, uint bag, uint bedroll, uint stirups)
        {
            Name = name;
            HashModel = hashModel;
            XP = xP;
            Health = health;
            Stamina = stamina;
            this.saddle = saddle;
            this.blanket = blanket;
            this.mane = mane;
            this.tail = tail;
            this.bag = bag;
            this.bedroll = bedroll;
            this.stirups = stirups;
        }

        public Horse() { }
    }
}
