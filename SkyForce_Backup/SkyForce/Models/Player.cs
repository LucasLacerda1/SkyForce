using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.Generic;
using SkyForce.Services; // ADICIONADO: Para acessar o AudioManager

namespace SkyForce.Models;

public class Player : Entity
{
    public new Image Visual { get; set; }
    public new double X { get; set; }
    public new double Y { get; set; }
    public new double Speed { get; set; }
    
    public int Score { get; set; } = 0;
    public double HP { get; set; } = 100.0;
    public bool IsDead { get; private set; } = false;

    public Player(Image visual)
    {
        Visual = visual;
        X = 268.0;   
        Y = 750.0;   
        Speed = 8.5; 
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    public void ResetScore()
    {
        Score = 0;
    }
    
    public void TakeDamage(double damage)
    {
        if (IsDead) return;

        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
            IsDead = true;
            
            AudioManager.Play("deathExplosion.wav");
            
            Visual.IsVisible = false;
        }
    }

    public override void Update() 
    { 
        if (IsDead) return;
        
        Canvas.SetLeft(Visual, X);
        Canvas.SetTop(Visual, Y);
    }

    public void Move(HashSet<Key> pressedKeys)
    {
        if (IsDead) return; // Impede movimento se estiver morto

        double currentSpeed = pressedKeys.Contains(Key.K) ? 3.5 : Speed;

        if (pressedKeys.Contains(Key.W) && Y > 0) Y -= currentSpeed;
        if (pressedKeys.Contains(Key.S) && Y < 836) Y += currentSpeed;
        if (pressedKeys.Contains(Key.A) && X > 0) X -= currentSpeed;
        if (pressedKeys.Contains(Key.D) && X < 536) X += currentSpeed;

        Update();
    }
}