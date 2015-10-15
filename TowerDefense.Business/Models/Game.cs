﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using TestTower;
using TowerDefense.Interfaces;
using Size = TowerDefense.Interfaces.Size;

namespace TowerDefense.Business.Models
{
    public class Game
    {
        private Thread _thread;
        private GameThread _gameThread;
        public int FoeCount { get; set; }

        public int Killed {get; set; }
        public GameState GameState { get; private set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }
        public Size DefaultSize = new Size(800, 800);
        public Size Size { get; set; }
        public IGameBroadcaster GameBroadcaster { get; set; }

        public Game()
        {
            Players = new List<Player>();
            Size = DefaultSize;
            Tank.SetLocationProvider(new LocationProvider());
        }

        private void Setup()
        {
            if (Players.Count == 0)
            {
                Players.Add(new Player
                {
                    Name = "demo",
                    Tanks = new List<Tank> { new TestTank() }
                });
            }

            GameState = GenerateGameState(DefaultSize.Height, DefaultSize.Width, this);

            Monster.MonsterMaxHealth = 10;
        }

        public void StartNewGame(IGameBroadcaster gameBroadcaster)
        {
            Setup();

            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort(gameBroadcaster);
            }

            GameBroadcaster = gameBroadcaster;

            _gameThread = new GameThread(this);
            FoeCount = 1;
            _thread = new Thread(_gameThread.Run);
            _thread.Start(this);
        }

        public List<Monster> GetFoesInRange(int x, int y, int radius)
	    {
			List<Monster> foesInRange = new List<Monster>();

		    int foeWidth = 16;
		    int foeHeight = 16;

		    Rectangle rect = new Rectangle(x - radius / 2, y - radius / 2, radius, radius);

		    foreach (var foe in GameState.Foes)
		    {
			    int foeCenterX = (int)foe.Location.X + (foeWidth/2);
			    int foeCenterY = (int)foe.Location.Y + (foeHeight/2);

			    if (rect.Contains(foeCenterX, foeCenterY))
			    {
					foesInRange.Add((Monster) foe);
			    }
		    }

			return foesInRange;
	    }

        public bool IsTankInBounds(ITank tank, double newX, double newY, IGameState gameState)
        {
            return newX + tank.Size.Width < gameState.Size.Width && newX > 0 &&
                   newY + tank.Size.Height < gameState.Size.Height && newY > 0;
        }

        public bool CanReach(IEntity shooter, Bullet bullet, IEntity target)
        {
            var xDistance = (shooter.X + shooter.Size.Width) - (target.X + target.Size.Width);
            var yDistance = (shooter.Y + shooter.Size.Height) - (target.Y + target.Size.Height);
            var distance = bullet.Range - Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
            var sizeOfThings = (shooter.Size.Height + shooter.Size.Width + target.Size.Height + target.Size.Width) / 2;
            return target != null && (distance > -sizeOfThings);
        }

        private static GameState GenerateGameState(double height, double width, Game game)
        {
            return new GameState
            {
                Size = new Size { Height = height, Width = width },
                Foes = new List<IFoe>(),
                Goals = new List<IGoal>
                {
                    new Goal {Location = new Location(0,0)},
                    new Goal {Location = new Location(width - Goal.Width, 0)},
                    new Goal {Location = new Location(0, height - Goal.Height)},
                    new Goal {Location = new Location(width - Goal.Width, height - Goal.Height)}
                },
                GameTanks = game.Players.SelectMany(player => player.Tanks.Select(tank => (IGameTank)new GameTank(tank, player.Name))).ToList()
            };
        }

        public IGoal IsMonsterAtGoal(IFoe monster, List<IGoal> goals)
        {
            foreach (var goal in goals)
            {
                if (((monster.X - monster.Size.Width / 2) > goal.X) && (monster.X + monster.Size.Width / 2 < (goal.X + goal.Size.Width)) &&
                    ((monster.Y - monster.Size.Height / 2) > goal.Y) && (monster.Y + monster.Size.Height / 2 < (goal.Y + goal.Size.Height)))
                {
                    return goal;
                }
            }

            return null;
        }

        public void ClearGameOut(IGameBroadcaster gameBroadcaster)
        {
            Players.Clear();
            _thread.Abort(gameBroadcaster);
        }

        public void AttackFoe()
        {
            
        }
    }
}