using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using SkyForce.Services;

namespace SkyForce.Models;

public class SpecialProjectile : Entity
{
    public double Speed { get; set; } = 6.0;
    private const double RotationSpeed = 10.0;
    private const double ExplosionRotationSpeed = 12.0;

    private List<Bitmap> _projectileFrames = new();
    private List<Bitmap> _explosionFrames = new();
    
    private int _currentFrame = 0;
    private int _frameCounter = 0;
    
    private const int FlightFrameDelay = 6; 
    private const int ExplosionFrameDelay = 10; 

    private double _angle = 0;
    private double _scale = 1.0;
    
    private const double MaxScale = 5.5; 
    private const double GrowthRate = 0.04; 

    private RotateTransform _rotateTransform = new RotateTransform();
    private ScaleTransform _scaleTransform = new ScaleTransform();
    
    public bool IsExploding { get; private set; } = false;
    public bool IsFinished { get; private set; } = false;
    
    private bool _soundPlayed = false;

    public SpecialProjectile(double x, double y)
    {
        X = x; Y = y;
        Visual = new Image { Width = 160, Height = 160 };

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_rotateTransform);
        transformGroup.Children.Add(_scaleTransform);
        
        Visual.RenderTransform = transformGroup;
        Visual.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);

        LoadResources();
        
        Canvas.SetLeft(Visual, X - 80);
        Canvas.SetTop(Visual, Y - 80);
    }

    private void LoadResources()
    {
        try {
            for (int i = 1; i <= 5; i++)
                _projectileFrames.Add(new Bitmap(AssetLoader.Open(new Uri($"avares://SkyForce/Assets/Sprites/Projectiles/special{i}.png"))));

            for (int i = 1; i <= 8; i++)
                _explosionFrames.Add(new Bitmap(AssetLoader.Open(new Uri($"avares://SkyForce/Assets/Sprites/Effects/poo{i}.png"))));

            if (_projectileFrames.Count > 0) Visual.Source = _projectileFrames[0];
        } catch { }
    }

    public void Explode()
    {
        if (IsExploding) return;
        
        IsExploding = true;
        _currentFrame = 0;
        _frameCounter = 0;
    }

    public override void Update()
    {
        if (IsFinished) return;

        if (!IsExploding)
        {
            Y -= Speed;
            Canvas.SetTop(Visual, Y);
            _angle += RotationSpeed;
            UpdateAnimation(_projectileFrames, FlightFrameDelay);
        }
        else
        {
            if (!_soundPlayed)
            {
                AudioManager.Play("poo.wav", 80);
                _soundPlayed = true;
            }

            _angle += ExplosionRotationSpeed;

            if (_scale < MaxScale)
            {
                _scale += GrowthRate;
                _scaleTransform.ScaleX = _scaleTransform.ScaleY = _scale;
            }

            _frameCounter++;
            if (_frameCounter >= ExplosionFrameDelay) 
            {
                _frameCounter = 0;
                _currentFrame++;
                if (_currentFrame < _explosionFrames.Count)
                {
                    Visual.Source = _explosionFrames[_currentFrame];
                }
                else
                {
                    if (_scale >= MaxScale) IsFinished = true;
                }
            }
        }

        if (_angle >= 360) _angle -= 360;
        _rotateTransform.Angle = _angle;
    }

    private void UpdateAnimation(List<Bitmap> frames, int delay)
    {
        if (frames.Count <= 1) return;
        _frameCounter++;
        if (_frameCounter >= delay)
        {
            _frameCounter = 0;
            _currentFrame = (_currentFrame + 1) % frames.Count;
            Visual.Source = frames[_currentFrame];
        }
    }
}