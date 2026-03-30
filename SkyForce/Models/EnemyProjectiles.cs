using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;

namespace SkyForce.Models;

public class EnemyProjectile
{
    public Image Visual { get; set; }
    private double _vx, _vy;
    private List<Bitmap> _frames = new List<Bitmap>();
    private int _animCounter = 0;

    public EnemyProjectile(double x, double y, double angleDegrees, EnemyType shooterType)
    {
        Visual = new Image { Width = 60, Height = 60 };
        
        double radians = angleDegrees * (Math.PI / 180);
        double speed = (shooterType == EnemyType.Boss) ? 4.2 : 5.3;
        
        _vx = Math.Cos(radians) * speed;
        _vy = Math.Sin(radians) * speed;

        try {
            if (shooterType == EnemyType.Tracker) {
                _frames.Add(new Bitmap(AssetLoader.Open(new Uri("avares://SkyForce/Assets/Sprites/Projectiles/enemy1bullet1.png"))));
            } 
            else if (shooterType == EnemyType.Boss) {
                _frames.Add(new Bitmap(AssetLoader.Open(new Uri("avares://SkyForce/Assets/Sprites/Projectiles/enemy2Bullet1.png"))));
                _frames.Add(new Bitmap(AssetLoader.Open(new Uri("avares://SkyForce/Assets/Sprites/Projectiles/enemy2Bullet2.png"))));
            }
            else {
                _frames.Add(new Bitmap(AssetLoader.Open(new Uri("avares://SkyForce/Assets/Sprites/Projectiles/enemy2Bullet1.png"))));
                _frames.Add(new Bitmap(AssetLoader.Open(new Uri("avares://SkyForce/Assets/Sprites/Projectiles/enemy2Bullet2.png"))));
            }
            Visual.Source = _frames[0];
        } catch { }

        Canvas.SetLeft(Visual, x);
        Canvas.SetTop(Visual, y);
    }

    public void Update()
    {
        Canvas.SetLeft(Visual, Canvas.GetLeft(Visual) + _vx);
        Canvas.SetTop(Visual, Canvas.GetTop(Visual) + _vy);
        
        if (_frames.Count > 1) {
            _animCounter++;
            if (_animCounter % 10 == 0) 
                Visual.Source = (_animCounter % 20 == 0) ? _frames[0] : _frames[1];
        }
    }
}