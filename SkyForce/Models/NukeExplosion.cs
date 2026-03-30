using Avalonia;
using Avalonia.Controls;
using Avalonia.Media; // Necessário para ScaleTransform e RotateTransform
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;

namespace SkyForce.Models;

public class NukeExplosion : Entity
{
    private List<Bitmap> _frames = new();
    private int _currentFrame = 0;
    private int _frameCounter = 0;
    private const int FrameDelay = 3;
    public bool IsFinished { get; private set; } = false;
    
    private RotateTransform _rotateTransform;
    private double _rotationAngle = 0;
    private const double RotationSpeed = 8.0;
    
    private ScaleTransform _scaleTransform;
    private double _currentScale = 0.5; 
    private const double ScaleGrowthRate = 0.05;
    
    private TransformGroup _transformGroup;

    public NukeExplosion(double x, double y)
    {
        X = x;
        Y = y;
        
        Visual = new Image { Width = 300, Height = 300 };
        
        LoadFrames();
        Visual.Source = _frames[0];
        
        Visual.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        
        _scaleTransform = new ScaleTransform(_currentScale, _currentScale);
        _rotateTransform = new RotateTransform(_rotationAngle);
        _transformGroup = new TransformGroup();
        _transformGroup.Children.Add(_scaleTransform);
        _transformGroup.Children.Add(_rotateTransform);
        Visual.RenderTransform = _transformGroup;
        
        Canvas.SetLeft(Visual, X - 150);
        Canvas.SetTop(Visual, Y - 150);
    }

    private void LoadFrames()
    {
        for (int i = 1; i <= 8; i++)
        {
            try
            {
                var uri = new Uri($"avares://SkyForce/Assets/Sprites/Effects/poo{i}.png");
                _frames.Add(new Bitmap(AssetLoader.Open(uri)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar frame poo{i}: {ex.Message}");
            }
        }
    }

    public override void Update()
    {
        if (IsFinished) return;
        
        _rotationAngle += RotationSpeed;
        if (_rotationAngle >= 360) _rotationAngle -= 360;
        _rotateTransform.Angle = _rotationAngle;
        
        _currentScale += ScaleGrowthRate;
        if (_currentScale > 2.5) _currentScale = 2.5; 
        _scaleTransform.ScaleX = _scaleTransform.ScaleY = _currentScale;
        
        _frameCounter++;
        if (_frameCounter >= FrameDelay)
        {
            _frameCounter = 0;
            _currentFrame++;
            if (_currentFrame < _frames.Count)
            {
                Visual.Source = _frames[_currentFrame];
            }
            else
            {
                IsFinished = true;
            }
        }
    }
}