using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using SkyForce.Services; // Para o AudioManager

namespace SkyForce.Models;

public class Boss : Entity
{
    public double HP { get; set; } = 800; 
    public bool IsDead { get; private set; } = false;
    
    private DateTime _lastShotTime = DateTime.MinValue;
    private bool _isShooting = false;
    private int _sequenceStep = 15; 
    private double _sequenceOffset = 0;

    private double _angleLeft = 0;   
    private double _angleRight = 180;
    private double _angleSweepLeft = 0;  
    private double _angleSweepRight = 0; 

    private int _attackPattern = 0; 

    public Boss(double x)
    {
        X = x; 
        Y = -300; 
        Visual = new Image { Width = 300, Height = 300 };
        UpdateAppearance();
        
        Canvas.SetLeft(Visual, X);
        Canvas.SetTop(Visual, Y);
    }
    
    public void TakeDamage(double damage)
    {
        if (IsDead) return;

        HP -= damage;
        UpdateAppearance();

        if (HP <= 0)
        {
            HP = 0;
            IsDead = true;
            Visual.IsVisible = false;
        }
    }

    public override void Update()
    {
        if (IsDead) return;

        if (Y < 50) Y += 1.5;
        Canvas.SetLeft(Visual, X);
        Canvas.SetTop(Visual, Y);
    }

    private void UpdateAppearance()
    {
        int stage = 8 - (int)Math.Floor((HP - 1) / 200.0);
        stage = Math.Clamp(stage, 1, 8);
        
        try {
            string uri = $"avares://SkyForce/Assets/Sprites/Enemies/bossVida{stage}.png";
            Visual.Source = new Bitmap(AssetLoader.Open(new Uri(uri)));
        } catch { }
    }

    public List<EnemyProjectile> TryShoot(Point playerPos)
    {
        List<EnemyProjectile> projectiles = new List<EnemyProjectile>();
        if (IsDead) return projectiles; // Boss morto não atira

        DateTime now = DateTime.Now;

        if (!_isShooting && (now - _lastShotTime).TotalSeconds > 0.8)
        {
            _isShooting = true;
            if (_attackPattern == 0) {
                _angleLeft = 0 + _sequenceOffset;
                _angleRight = 180 + _sequenceOffset;
            } else {
                _angleSweepLeft = 160;
                _angleSweepRight = 20;
            }
        }

        if (_isShooting)
        {
            if ((now - _lastShotTime).TotalMilliseconds > 50) 
            {
                if (_attackPattern == 0) 
                {
                    projectiles.Add(new EnemyProjectile(X + 150, Y + 200, _angleLeft, EnemyType.Boss));
                    projectiles.Add(new EnemyProjectile(X + 150, Y + 200, _angleRight, EnemyType.Boss));
                    _angleLeft += _sequenceStep;
                    _angleRight -= _sequenceStep;
                    if (_angleLeft > 180 + _sequenceOffset) TerminarAtaque();
                } 
                else 
                {
                    projectiles.Add(new EnemyProjectile(X, Y + 200, _angleSweepLeft, EnemyType.Boss));
                    projectiles.Add(new EnemyProjectile(X + 300, Y + 200, _angleSweepRight, EnemyType.Boss));
                    _angleSweepLeft -= 10;
                    _angleSweepRight += 10;
                    if (_angleSweepLeft < 20 || _angleSweepRight > 160) TerminarAtaque();
                }
                _lastShotTime = now;
            }
        }
        return projectiles;
    }

    private void TerminarAtaque()
    {
        _isShooting = false;
        _attackPattern = (_attackPattern == 0) ? 1 : 0;
        _sequenceOffset = (_sequenceOffset == 15) ? 0 : 15;
    }
}