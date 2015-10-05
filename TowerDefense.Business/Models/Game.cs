﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TestTower;
using TowerDefense.Interfaces;

namespace TowerDefense.Business.Models
{
    public class Game
    {
        private Thread Thread;

        public Game()
        {
            Players = new List<Player>();
            Size = DefaultSize;
        }

        public static Size DefaultSize = new Size(800, 800);
        private int _foeCount;

        public string Name { get; set; }
        public List<Player> Players { get; set; }
        public Size Size { get; set; }
        public IGameBroadcaster GameBroadcaster { get; set; }

        public void StartNewGame(IGameBroadcaster gameBroadcaster)
        {
            if (Thread != null && Thread.IsAlive)
            {
                Thread.Abort(gameBroadcaster);
            }
            Thread = new Thread(ThreadStart);
            _foeCount = 1;
            GameBroadcaster = gameBroadcaster;
            Thread.Start(this);
        }

        private static void ThreadStart(object gameObj)
        {
            var game = (Game)gameObj;
            //Random r = new Random();

            var gameState = GenerateGameState(DefaultSize.Height, DefaultSize.Width, game);
            ITank tt1 = new TestTank() { X = 33, Y = 33, Id = 0 };
            ITank tt2 = new TestTank() { X = 733, Y = 33, Id = 1 };
            ITank tt3 = new TestTank() { X = 33, Y = 733, Id = 2 };
            ITank tt4 = new TestTank() { X = 733, Y = 733, Id = 3 };
            gameState.GameTanks.Add(new GameTank(tt1));
            gameState.GameTanks.Add(new GameTank(tt2));
            gameState.GameTanks.Add(new GameTank(tt3));
            gameState.GameTanks.Add(new GameTank(tt4));

            var killed = 0;
            while (true)
            {
                game.GameBroadcaster.BroadcastGameState(gameState);

                if (gameState.Foes.Count < game._foeCount)
                {
                    Monster m = new Monster { X = 400 - 8, Y = 400 - 8 };
                    gameState.Foes.Add(m);
                }

                for (int j = 0; j < gameState.Foes.Count; j++)
                {
                    var monster = (IMonster)gameState.Foes[j];
                    monster.Update(gameState);
                    var goal = IsMonsterAtGoal(monster, gameState.Goals);
                    if (goal != null)
                    {
                        gameState.Foes.Remove(monster);
                        ((Goal)goal).Health -= 1;
                        if (goal.Health <= 0)
                        {
                            gameState.Goals.Remove(goal);
                            if (!gameState.Goals.Any())
                            {
                                gameState.Lost = true;
                            }
                        }
                        j--;
                    }
                }

                foreach (var gameTank in gameState.GameTanks)
                {
                    var tank = gameTank.Tank;
                    gameTank.Target = (Monster)tank.Update(gameState);
                    gameTank.Shooting = false;
                    if (gameTank.Heat <= 0)
                    {
                        var bullet = gameTank.Bullet;
                        if (CanReach(tank, bullet, gameTank.Target))
                        {
                            gameTank.Shooting = true;
                            gameTank.Heat += bullet.ReloadTime;
                            gameTank.Target.Health -= bullet.Damage;
                            if (gameTank.Target.Health <= 0)
                            {
                                gameState.Foes.Remove(gameTank.Target);
                                if (!gameState.Lost)
                                {
                                    gameTank.Killed++;
                                    killed++;
                                    if (killed == game._foeCount)
                                    {
                                        killed = 0;
                                        gameState.Wave++;
                                        game._foeCount++;
                                        Monster.MonsterMaxHealth = (int)(Monster.MonsterMaxHealth * 1.1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        gameTank.Heat--;
                    }
                }
                Thread.Sleep(10);
            }
        }

        private static bool CanReach(IEntity shooter, Bullet bullet, IEntity target)
        {
            var xDistance = shooter.X - target.X + (shooter.Size.Width - target.Size.Width);
            var yDistance = shooter.Y - target.Y + (shooter.Size.Height - target.Size.Height);
            var distance = Math.Pow(bullet.Range, 2) - (Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
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
                    new Goal {X = 0, Y = 0},
                    new Goal {X = width - Goal.Width, Y = 0},
                    new Goal {X = 0, Y = height - Goal.Height},
                    new Goal {X = width - Goal.Width, Y = height - Goal.Height}
                },
                GameTanks = game.Players.SelectMany(player => player.Tanks).Select(tank => (IGameTank)new GameTank(tank)).ToList()
            };
        }

        public static IGoal IsMonsterAtGoal(IFoe monster, List<IGoal> goals)
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
    }
}