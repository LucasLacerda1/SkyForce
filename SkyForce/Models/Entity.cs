using Avalonia.Controls;

namespace SkyForce.Models;

public abstract class Entity
{
    public Image Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }

    public abstract void Update();
    
    
}