using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Collections.Generic;

namespace SkyForce.Models;

public class Explosion
{
    public Image Visual { get; set; }
    private List<Bitmap> _frames;
    private int _currentFrame = 0;
    private int _counter = 0;
    private const int AnimSpeed = 3;
    public bool IsFinished { get; private set; } = false;

    public Explosion(double x, double y, List<Bitmap> frames)
    {
        _frames = frames;
        Visual = new Image { Width = 100, Height = 100 };
        Visual.Source = _frames[0];
        
        Canvas.SetLeft(Visual, x);
        Canvas.SetTop(Visual, y);
    }

    public void Update()
    {
        _counter++;
        if (_counter >= AnimSpeed)
        {
            _counter = 0;
            _currentFrame++;
            if (_currentFrame < _frames.Count)
                Visual.Source = _frames[_currentFrame];
            else
                IsFinished = true;
        }
    }
}