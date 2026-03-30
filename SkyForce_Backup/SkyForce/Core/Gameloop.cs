using Avalonia.Threading;
using System;

namespace SkyForce.Core;

public class Gameloop
{
    private DispatcherTimer _timer;
    private Action _updateAction;
    

    public Gameloop(Action updateAction)
    {
        _updateAction = updateAction;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (s, e) => _updateAction.Invoke();
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
}