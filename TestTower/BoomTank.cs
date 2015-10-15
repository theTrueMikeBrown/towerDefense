﻿using System;
using System.Drawing;
using System.Linq;
using TowerDefense.Business.Models;
using TowerDefense.Interfaces;

namespace TestTower
{
    public class BoomTank : Tank
    {
        public Bullet Bullet { get; set; }
        public override string Name { get { return "Mr. Boomy"; } }

        public BoomTank()
            : base(500, 500)
        {
            this.Speed = 1;
        }
        public override TankUpdate Update(IGameState gameState)
        {
            TankUpdate tankUpdate = new TankUpdate();

            if (gameState.Foes.Any() && gameState.Goals.Any())
            {
                tankUpdate.ShotTarget = gameState.Foes.OrderBy(foe => GetDistance(foe)).First().Location;
                ChangeBulletPower(tankUpdate.ShotTarget);

                var x = (gameState.Foes.Average(foe => foe.X) + 99 * gameState.Goals.Average(goal => goal.X)) / 100;
                var y = (gameState.Foes.Average(foe => foe.Y) + 99 * gameState.Goals.Average(goal => goal.Y)) / 100;
            }

            return tankUpdate;
        }

        private double GetDistance(IFoe foe)
        {
            var xDistance = (this.X + this.Size.Width) - (foe.X + foe.Size.Width);
            var yDistance = (this.Y + this.Size.Height) - (foe.Y + foe.Size.Height);
            return Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
        }
        private void ChangeBulletPower(ILocation target)
        {
            var range = GetDistanceFromTank(target) + 1;
            var damage = (int)(1000 / range);
            var splash = new SplashBullet
            {
                Range = 100
            };
            Bullet = new Bullet { Damage = damage, Range = range, Freeze = 0, Splash = splash };
        }

        public override IBullet GetBullet()
        {
            return Bullet; //new Bullet { Damage = 1000 / 400, Range = 400 };
        }
    }
}