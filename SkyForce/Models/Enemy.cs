using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using SkyForce.Services;

namespace SkyForce.Models;

public enum EnemyType { Tracker, ConeShooter, Boss }

public class Enemy : Entity
{
    public EnemyType Type { get; set; }
    public double HP { get; set; }
    public bool IsDead { get; private set; } = false;
    
    private double _stopY; 
    private DateTime _lastShot = DateTime.MinValue;
    private double _shootCooldown;

    public Enemy(EnemyType type, double startX)
    {
        Type = type;
        X = startX;
        Y = -200.0; 

        switch (type)
        {
            case EnemyType.Tracker:
                Visual = new Image { Width = 100, Height = 100 };
                Speed = 3.0;
                HP = 40.0;
                _shootCooldown = 0.8; 
                _stopY = new Random().Next(50, 250);
                break;

            case EnemyType.ConeShooter:
                Visual = new Image { Width = 100, Height = 100 };
                Speed = 2.0;
                HP = 60.0;
                _shootCooldown = 1.2;
                _stopY = new Random().Next(50, 350);
                break;
            
            case EnemyType.Boss:
                Visual = new Image { Width = 250, Height = 250 };
                Speed = 1.0;
                HP = 500.0;
                _shootCooldown = 2.0;
                _stopY = 100;
                break;
        }

        LoadSprite();
    }

    private void LoadSprite()
    {
        string fileName = Type == EnemyType.Tracker ? "enemy1.png" : "enemy2.png";
        if (Type == EnemyType.Boss) fileName = "boss.png"; 
        
        try
        {
            Uri uri = new Uri($"avares://SkyForce/Assets/Sprites/Enemies/{fileName}");
            Visual.Source = new Bitmap(AssetLoader.Open(uri));
        }
        catch { }
    }

    public void TakeDamage(double damage)
    {
        if (IsDead) return;

        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
            IsDead = true;

            if (Type != EnemyType.Boss)
            {
                AudioManager.Play("deathExplosion.wav", 30);
            }

            Visual.IsVisible = false;
        }
    }

    public override void Update()
    {
        if (IsDead) return;

        if (Y < _stopY) Y += Speed;
        
        Canvas.SetLeft(Visual, X);
        Canvas.SetTop(Visual, Y);
    }

    public List<EnemyProjectile> TryShoot(Point playerPos)
    {
        List<EnemyProjectile> shots = new List<EnemyProjectile>();
        
        if (IsDead || (DateTime.Now - _lastShot).TotalSeconds < _shootCooldown) 
            return shots;
            
        _lastShot = DateTime.Now;

        double originX = X + (Visual.Width / 2);
        double originY = Y + (Visual.Height * 0.8);

        if (Type == EnemyType.Tracker)
        {
            double dx = playerPos.X - originX;
            double dy = playerPos.Y - originY;
            double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            shots.Add(new EnemyProjectile(originX, originY, angle, EnemyType.Tracker));
        }
        else if (Type == EnemyType.ConeShooter)
        {
            double[] angles = { 70.0, 90.0, 110.0 }; 
            foreach (double a in angles)
            {
                shots.Add(new EnemyProjectile(originX, originY, a, EnemyType.ConeShooter));
            }
        }
        return shots;
    }
}