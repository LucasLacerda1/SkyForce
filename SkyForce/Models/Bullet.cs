using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Collections.Generic;

namespace SkyForce.Models;

public class Bullet
{
    public Image Visual { get; set; }
    public double Speed { get; set; } = 16.0;

    private List<Bitmap> _animationFrames;
    private int _currentFrameIndex = 0;
    private int _animationDelayCounter = 0;
    private int _animationSpeedDelay;
    
    public List<Entity> HitEnemies { get; set; } = new List<Entity>();

    public Bullet(double x, double y, List<Bitmap> frames, int animDelay, double width = 36, double height = 84)
    {
        _animationFrames = frames;
        _animationSpeedDelay = animDelay;

        Visual = new Image
        {
            Width = width,
            Height = height,
            Stretch = Avalonia.Media.Stretch.Fill
        };

        if (_animationFrames != null && _animationFrames.Count > 0)
        {
            Visual.Source = _animationFrames[0];
        }

        Canvas.SetLeft(Visual, x - (Visual.Width / 2));
        Canvas.SetTop(Visual, y - Visual.Height);
    }

    public void Update()
    {
        double currentTop = Canvas.GetTop(Visual);
        Canvas.SetTop(Visual, currentTop - Speed);
        
        if (_animationFrames != null && _animationFrames.Count > 1)
        {
            _animationDelayCounter++;
            if (_animationDelayCounter >= _animationSpeedDelay)
            {
                _animationDelayCounter = 0;
                
                if (_animationFrames.Count == 4)
                {
                    if (_currentFrameIndex < _animationFrames.Count - 1)
                    {
                        _currentFrameIndex++;
                        Visual.Source = _animationFrames[_currentFrameIndex];
                    }
                }
                else
                {
                    _currentFrameIndex = (_currentFrameIndex + 1) % _animationFrames.Count;
                    Visual.Source = _animationFrames[_currentFrameIndex];
                }
            }
        }
    }
}