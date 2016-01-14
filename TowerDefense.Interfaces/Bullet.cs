﻿using System;
using System.Drawing;
using TowerDefense.Interfaces;

namespace TowerDefense.Business.Models
{
    public class Bullet : IBullet
    {
        public double Range { get; set; }
        public int Damage { get; set; }
        public int Freeze { get; set; }
	    public double SplashRange { get; set; }
        public double SplashHeatMultiplier => 2;
        public double FreezeHeatMultiplier => .5;
        public double GravityDuration { get; set; }
        public double GravityStrength { get; set; }
        public double GravityMultiplier => 10000;

        public double ReloadTime
        {
	        get
	        {
	            var splash = (Math.Abs(SplashRange) * SplashHeatMultiplier);
	            var freeze = (Math.Abs(Freeze) * FreezeHeatMultiplier);
	            var gravity = (Math.Abs(GravityDuration) * Math.Abs(GravityStrength) * GravityMultiplier);

	            if (gravity > 0)
	            {
	                return gravity;
	            }
	            else
	            {
                    return Range * ((Math.Abs(Damage) + freeze) + (Math.Abs(Damage) * splash)) / 1000;
                }
            }
        }
    }
}