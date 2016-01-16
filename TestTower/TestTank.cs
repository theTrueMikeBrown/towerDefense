﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Business.Models;
using TowerDefense.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Business.Models;
using TowerDefense.Interfaces;

namespace TestTower
{
    public class TestTank : Tank
    {
        private Random _rng = new Random();
        public Bullet Bullet { get; set; }
        public override string Name { get { return "Mr. Shooty"; } }
        private double _xTarget;
        private double _yTarget;

        public TestTank()
            : base(400, 400)
        {
            _xTarget = 400;
            _yTarget = 400;
        }
        public override TankUpdate Update(IGameState gameState)
        {
            TankUpdate tankUpdate = new TankUpdate();
            tankUpdate.TankColor = ConvertColorToHexString(Color.Red);

            if (gameState.Foes.Any() && gameState.Goals.Any())
            {
                var target = gameState.Foes
                    .Where(foe => 1000 / GetDistanceFromTank(foe) >= 1)
                    .OrderBy(GetDistanceFromTank)
                    .FirstOrDefault();

                if (target != null)
                {
                    tankUpdate.ShotTarget = target.Center;
                    ChangeBulletPower(target);
                    tankUpdate.Bullet = Bullet;
                }

                UpdateMovementTarget(tankUpdate, gameState);
            }

            return tankUpdate;
        }

        private void UpdateMovementTarget(TankUpdate tankUpdate, IGameState gameState)
        {
            int maxFoes = GetFoesInRange(_xTarget, _yTarget, gameState);

            for (int i = 0; i < 20; i++)
            {
                var y = _rng.NextDouble() * gameState.Size.Height;
                var x = _rng.NextDouble() * gameState.Size.Width;
                var f = GetFoesInRange(x, y, gameState);
                if (f > maxFoes)
                {
                    maxFoes = f;
                    _xTarget = x;
                    _yTarget = y;
                }
            }

            tankUpdate.MovementTarget = LocationProvider.GetLocation(_xTarget, _yTarget);
        }

        private int GetFoesInRange(double xTarget, double yTarget, IGameState gameState)
        {
            var foes = 0;
            var range = 50;
            foreach (var foe in gameState.Foes)
            {
                if (Math.Abs(foe.X - xTarget) < range && Math.Abs(foe.Y - yTarget) < range)
                {
                    foes++;
                }
            }
            return foes;
        }

        private void ChangeBulletPower(IFoe foe)
        {
            var range = GetDistanceFromTank(foe) + 1;
            var damage = (int)(1000 / range);
            Bullet = new Bullet { Damage = damage, Range = range, Freeze = 0, SplashRange = 0 };
        }
    }
}
