using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkyForce.Models;
using SkyForce.Core;
using SkyForce.Services; 
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyForce.Views;

public class EnemyWave {
    public double SpawnTime { get; set; } 
    public double X { get; set; }         
    public EnemyType Type { get; set; }   
    public bool Spawned { get; set; } = false;
    public EnemyWave(double t, double x, EnemyType tp) { SpawnTime = t; X = x; Type = tp; }
}

public partial class MainWindow : Window
{
    private bool _inTitleScreen = true, _inCharacterSelect = false, _gameActive = false, _isBossTime = false, _isGameOver = false, _isVictory = false;
    private int _charIndex = 0;
    private Player _player;
    private InputManager _input;
    private Gameloop _loop;
    private int _blinkCounter = 0;

    private List<Bullet> _bullets = new List<Bullet>();
    private List<Entity> _enemies = new List<Entity>(); 
    private List<EnemyProjectile> _enemyBullets = new List<EnemyProjectile>();
    private List<Explosion> _explosions = new List<Explosion>();
    private List<SpecialProjectile> _specialProjectiles = new List<SpecialProjectile>();
    private List<Bitmap> _explosionBitmapCache = new List<Bitmap>();
    
    private List<Bitmap> _p1BulletFrames = new List<Bitmap>();
    private List<Bitmap> _p2BulletFrames = new List<Bitmap>();

    private List<Bitmap> _numberSprites = new List<Bitmap>();
    private double _scoreScale = 1.0; 
    private double _displayedScore = 0; 

    private double _gameTimer = 0.0, _phaseProgress = 0.0, _targetProgress = 1200.0, _scrollSpeed = 0.8, _bgVisualY = -1100.0;
    private DateTime _lastShot = DateTime.MinValue;

    private List<EnemyWave> _waves = new List<EnemyWave> {
        new EnemyWave(1.0, 100, EnemyType.Tracker),
        new EnemyWave(1.5, 400, EnemyType.Tracker),
        new EnemyWave(3.0, 250, EnemyType.ConeShooter),
        new EnemyWave(5.0, 50, EnemyType.Tracker),
        new EnemyWave(5.5, 500, EnemyType.Tracker),
        new EnemyWave(7.0, 200, EnemyType.ConeShooter),
        new EnemyWave(7.2, 350, EnemyType.ConeShooter),
        new EnemyWave(9.0, 150, EnemyType.Tracker),
        new EnemyWave(10.5, 450, EnemyType.Tracker),
        new EnemyWave(12.0, 300, EnemyType.ConeShooter),
        new EnemyWave(14.0, 100, EnemyType.Tracker),
        new EnemyWave(14.5, 500, EnemyType.Tracker),
        new EnemyWave(16.0, 250, EnemyType.ConeShooter),
        new EnemyWave(17.0, 300, EnemyType.Tracker)
    };

    private readonly List<(string Name, string MenuImg)> _pilots = new List<(string, string)> {
        ("RoCoCop", "avares://SkyForce/Assets/Sprites/Player/rococop.png"), 
        ("Sudo&Mudo", "avares://SkyForce/Assets/Sprites/Player/sudomudo.png") 
    };
    
    private bool _canFireSpecial = true;
    private bool _musicStarted = false;

    public MainWindow() {
        InitializeComponent();

        if (Avalonia.Controls.Design.IsDesignMode) return;

        _input = new InputManager();
        _player = new Player(PlayerShip);
        LoadExplosionBitmaps();
        LoadNumberSprites(); 
        LoadBulletFrames(); 
        this.Focusable = true;
        this.Loaded += (s, e) => this.Focus();
        _loop = new Gameloop(Update); 
        _loop.Start();
        
        this.Opened += (s, e) => {
            AudioManager.PlayMusic("gameSong.wav", 60);
        };

        this.Closing += (s, e) => {
            AudioManager.StopAll();
        };

        _ = AudioManager.PreLoad();
    }

    private void LoadBulletFrames() {
        _p1BulletFrames.Clear(); _p2BulletFrames.Clear();
        try {
            for (int i = 1; i <= 4; i++) _p1BulletFrames.Add(new Bitmap(AssetLoader.Open(new Uri($"avares://SkyForce/Assets/Sprites/Projectiles/player1bullet{i}.png"))));
            for (int i = 1; i <= 8; i++) _p2BulletFrames.Add(new Bitmap(AssetLoader.Open(new Uri($"avares://SkyForce/Assets/Sprites/Projectiles/player2bullet{i}.png"))));
        } catch (Exception ex) { Console.WriteLine("Erro ao carregar balas: " + ex.Message); }
    }

    private void LoadNumberSprites() {
        _numberSprites.Clear();
        try {
            for (int i = 0; i <= 9; i++) _numberSprites.Add(new Bitmap(AssetLoader.Open(new Uri($"avares://SkyForce/Assets/Backgrounds/Number{i} 7x10.png"))));
        } catch { }
    }

    private void Update() {
        ScrollBackground();
        if (_inTitleScreen || _isGameOver || _isVictory) HandleBlink();
        if (_gameActive) {
            _gameTimer += 0.016; 
            _player.Move(_input.PressedKeys);
            HandlePlayerShooting();
            HandleSpecialInput();
            SpawnProgrammedEnemies();
            UpdateEntities();
            UpdateExplosions(); 
            CheckCollisions();
            UpdateScoreAnimation(); 
            
            if (_displayedScore < _player.Score) {
                _displayedScore += 5; 
                if (_displayedScore > _player.Score) _displayedScore = _player.Score;
                UpdateScoreUI((int)_displayedScore);
            }
            else if (_displayedScore > _player.Score) {
                _displayedScore = _player.Score;
                UpdateScoreUI((int)_displayedScore);
            }
        }
    }

    private void HandleSpecialInput() {
        if (_charIndex == 0 && _player.Score >= 700 && 
            _input.PressedKeys.Contains(Key.J) && _canFireSpecial) {
            
            _canFireSpecial = false;
            
            AudioManager.Play("special.wav", 40); 
            
            var spec = new SpecialProjectile(_player.X + 32, _player.Y);
            _specialProjectiles.Add(spec);
            ProjectilesLayer.Children.Add(spec.Visual);
            
            _player.Score -= 700;
            
            _displayedScore = _player.Score; 
            
            UpdateScoreUI((int)_player.Score);
        }
        
        if (!_input.PressedKeys.Contains(Key.J)) {
            _canFireSpecial = true;
        }
    }

    private void UpdateScoreUI(int scoreToShow) {
        if (ScoreContainer == null || _numberSprites.Count < 10) return;
        ScoreContainer.Children.Clear();
        string scoreStr = scoreToShow.ToString("D6");
        foreach (char c in scoreStr) {
            int digit = int.Parse(c.ToString());
            ScoreContainer.Children.Add(new Image { Source = _numberSprites[digit], Width = 21, Height = 30, Margin = new Thickness(1, 0) });
        }
        if (scoreToShow < _player.Score) _scoreScale = 1.2;
    }

    private void UpdateScoreAnimation() {
        if (_scoreScale > 1.0) {
            _scoreScale -= 0.05; 
            if (ScoreContainer.RenderTransform is ScaleTransform st) st.ScaleX = st.ScaleY = _scoreScale;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) 
    {
        _input.KeyDown(e.Key);
        
        if (_isGameOver || _isVictory) 
        {
            if (e.Key == Key.Enter) 
            {
                AudioManager.Play("enter.wav", 30);
                RestartFromGameOver();
                return;
            }
            else if (e.Key == Key.Escape) 
            {
                BackToMenu();
                return;
            }
            return;
        }
        
        if (_inTitleScreen) 
        {
            if (e.Key == Key.Enter) 
            {
                AudioManager.Play("enter.wav", 30);
                _inTitleScreen = false; 
                _inCharacterSelect = true;
                TitleScreen.IsVisible = false; 
                CharacterSelectScreen.IsVisible = true;
                UpdateCharacterUI();
                
                AudioManager.PlayMusic("gameSong.wav", 85); 
            
                return;
            }
            return;
        }
        
        if (_inCharacterSelect) 
        {
            if (e.Key == Key.Enter) 
            {
                AudioManager.Play("enter.wav", 30);
                StartGameplay();
                AudioManager.PlayMusic("gameSong.wav", 85);
                return;
            }
            else if (e.Key == Key.Left || e.Key == Key.Right) 
            { 
                AudioManager.Play("select.wav", 15); 
                _charIndex = (_charIndex == 0) ? 1 : 0; 
                UpdateCharacterUI(); 
                return;
            }
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e) => _input.KeyUp(e.Key);

    private void StartGameplay() {
        _inCharacterSelect = false; _isGameOver = false; _isVictory = false; _isBossTime = false; _gameActive = true;
        CharacterSelectScreen.IsVisible = false; GameOverScreen.IsVisible = false; VictoryScreen.IsVisible = false; PlayArea.IsVisible = true;
        ClearEntities();
        _player.ResetScore(); _displayedScore = 0; UpdateScoreUI(0);
        try { PlayerShip.Source = new Bitmap(AssetLoader.Open(new Uri("avares://SkyForce/Assets/Sprites/Player/nave.png"))); } catch { }
        _player.X = 268; _player.Y = 750;
        _player.HP = 100; 
        _phaseProgress = 0; _gameTimer = 0; _bgVisualY = -1100.0;
        PlayerShip.IsVisible = true;
        this.Focus();
    }

    private void CheckCollisions() {
        foreach (var eb in _enemyBullets) if (CheckHitPrecise(eb.Visual, PlayerShip, 0.4, 0.4)) { GameOver(); return; }
        foreach (var target in _enemies) if (CheckHitPrecise(target.Visual, PlayerShip, 0.6, 0.6)) { GameOver(); return; }
        
        for (int i = _bullets.Count - 1; i >= 0; i--) {
            var currentBullet = _bullets[i];
            bool bulletDestroyed = false;
            for (int j = _enemies.Count - 1; j >= 0; j--) {
                var target = _enemies[j];
                if (CheckHitPrecise(currentBullet.Visual, target.Visual, 0.6, 0.8)) {
                    if (currentBullet.HitEnemies.Contains(target)) continue;
                    currentBullet.HitEnemies.Add(target);
                    ApplyDamage(target, (_charIndex == 0) ? 10 : 25, j);
                    if (_charIndex == 0) { 
                        ProjectilesLayer.Children.Remove(currentBullet.Visual); 
                        _bullets.RemoveAt(i); 
                        bulletDestroyed = true; 
                    }
                    if (bulletDestroyed) break;
                }
            }
        }

        for (int i = _specialProjectiles.Count - 1; i >= 0; i--) {
            var spec = _specialProjectiles[i];
            if (spec.IsExploding) continue;
            foreach (var target in _enemies) {
                if (CheckHitPrecise(spec.Visual, target.Visual, 0.7, 0.7)) {
                    TriggerTacticalNuke(spec);
                    break;
                }
            }
        }
    }

    private void ApplyDamage(Entity target, int damage, int index) {
        if (target is Enemy enemyTarget) {
            enemyTarget.TakeDamage(damage);
            if (enemyTarget.IsDead) {
                TriggerEnemyExplosion(enemyTarget.X, enemyTarget.Y);
                _player.AddScore(100);
                PlayArea.Children.Remove(enemyTarget.Visual);
                _enemies.RemoveAt(index);
            }
        }
        else if (target is Boss bossTarget) {
            int finalDamage = damage;
        
            if (_charIndex == 1) {
                finalDamage = damage * 2;
            }

            bossTarget.TakeDamage(finalDamage); 
        
            if (bossTarget.IsDead) {
                TriggerEnemyExplosion(bossTarget.X, bossTarget.Y);
                _player.AddScore(5000);
                Victory();
                PlayArea.Children.Remove(bossTarget.Visual);
                _enemies.RemoveAt(index);
            }
        }
    }

    private void TriggerTacticalNuke(SpecialProjectile spec) {
        spec.Explode(); 
        for (int i = _enemies.Count - 1; i >= 0; i--) {
            ApplyDamage(_enemies[i], 500, i);
        }
    }

    private void UpdateEntities() {
        for (int i = _specialProjectiles.Count - 1; i >= 0; i--) {
            _specialProjectiles[i].Update();
            if (_specialProjectiles[i].IsFinished || (_specialProjectiles[i].Y < -200 && !_specialProjectiles[i].IsExploding)) {
                ProjectilesLayer.Children.Remove(_specialProjectiles[i].Visual);
                _specialProjectiles.RemoveAt(i);
            }
        }

        for (int i = _enemies.Count - 1; i >= 0; i--) {
            var target = _enemies[i]; target.Update();
            if (target.Y > 0) {
                var shots = (target is Enemy e) ? e.TryShoot(new Point(_player.X, _player.Y)) : ((Boss)target).TryShoot(new Point(_player.X, _player.Y));
                foreach (var s in shots) { _enemyBullets.Add(s); ProjectilesLayer.Children.Add(s.Visual); }
            }
        }
        for (int j = _enemyBullets.Count - 1; j >= 0; j--) {
            _enemyBullets[j].Update();
            if (Canvas.GetTop(_enemyBullets[j].Visual) > 950) { ProjectilesLayer.Children.Remove(_enemyBullets[j].Visual); _enemyBullets.RemoveAt(j); }
        }
    }

    private void ScrollBackground() {
        if (!_gameActive || _bgVisualY < 0) _bgVisualY += _scrollSpeed;
        if (!_gameActive && !_isGameOver && !_isVictory && _bgVisualY >= 0) _bgVisualY = -1100.0;
        Canvas.SetTop(MainBackground, _bgVisualY);
        if (_gameActive && _phaseProgress < _targetProgress) {
            _phaseProgress += _scrollSpeed;
            if (_phaseProgress >= _targetProgress && !_isBossTime) SpawnBoss();
        }
    }

    private void SpawnProgrammedEnemies() { 
        foreach (var wave in _waves) if (!_isBossTime && !wave.Spawned && _gameTimer >= wave.SpawnTime) { wave.Spawned = true; Enemy e = new Enemy(wave.Type, wave.X); _enemies.Add(e); PlayArea.Children.Add(e.Visual); } 
    }

    private void HandleBlink() { 
        _blinkCounter++; 
        if (_blinkCounter >= 30) { 
            if (_inTitleScreen) ImgPressEnter.IsVisible = !ImgPressEnter.IsVisible; 
            if (_isGameOver) ImgPressEnterRestart.IsVisible = !ImgPressEnterRestart.IsVisible; 
            if (_isVictory) ImgPressEnterVictory.IsVisible = !ImgPressEnterVictory.IsVisible;
            _blinkCounter = 0; 
        } 
    }

    private void UpdateCharacterUI() { TxtPilotName.Text = _pilots[_charIndex].Name; try { SelectedShipPreview.Source = new Bitmap(AssetLoader.Open(new Uri(_pilots[_charIndex].MenuImg))); } catch { } }

    private void HandlePlayerShooting() {
        int fireRate = (_charIndex == 0) ? 180 : 400;
        
        if (_input.PressedKeys.Contains(Key.L) && (DateTime.Now - _lastShot).TotalMilliseconds > fireRate) {
            List<Bitmap> selectedFrames = (_charIndex == 0) ? _p1BulletFrames : _p2BulletFrames;
            int animDelay = (_charIndex == 0) ? 10 : 2; double bulletSpeed = (_charIndex == 0) ? 16.0 : 8.0;
            double bWidth = (_charIndex == 0) ? 36 : 60, bHeight = (_charIndex == 0) ? 84 : 110;
            Bullet b = new Bullet(_player.X + 32, _player.Y, selectedFrames, animDelay, bWidth, bHeight);
            b.Speed = bulletSpeed; 
        
            _bullets.Add(b); 
            ProjectilesLayer.Children.Add(b.Visual); 
            _lastShot = DateTime.Now;

            AudioManager.Play("bullet1Sound.wav", 25);
        }
    
        for (int i = _bullets.Count - 1; i >= 0; i--) { 
            _bullets[i].Update(); 
            if (Canvas.GetTop(_bullets[i].Visual) < -100) { 
                ProjectilesLayer.Children.Remove(_bullets[i].Visual); 
                _bullets.RemoveAt(i); 
            } 
        }
    }

    private void UpdateExplosions() { for (int i = _explosions.Count - 1; i >= 0; i--) { _explosions[i].Update(); if (_explosions[i].IsFinished) { PlayArea.Children.Remove(_explosions[i].Visual); _explosions.RemoveAt(i); } } }
    private void TriggerEnemyExplosion(double x, double y) { if (_explosionBitmapCache.Count > 0) { Explosion ex = new Explosion(x, y, _explosionBitmapCache); _explosions.Add(ex); PlayArea.Children.Add(ex.Visual); } }
    private void LoadExplosionBitmaps() { try { for (int i = 1; i <= 10; i++) _explosionBitmapCache.Add(new Bitmap(AssetLoader.Open(new Uri($"avares://SkyForce/Assets/Sprites/Effects/explosion{i}.png")))); } catch { } }
    private bool CheckHitPrecise(Control o1, Control o2, double sx, double sy) { Rect r1 = new Rect(Canvas.GetLeft(o1), Canvas.GetTop(o1), o1.Width * sx, o1.Height * sy); Rect r2 = new Rect(Canvas.GetLeft(o2), Canvas.GetTop(o2), o2.Width * 0.7, o2.Height * 0.7); return r1.Intersects(r2); }

    private void Victory() { _gameActive = false; _isVictory = true; VictoryScreen.IsVisible = true; }
    private void SpawnBoss() { _isBossTime = true; Boss b = new Boss(140); _enemies.Add(b); PlayArea.Children.Add(b.Visual); }
    
    private void GameOver() { 
        if (_isGameOver) return;

        _gameActive = false; 
        _isGameOver = true; 
        
        AudioManager.StopMusic();
        
        AudioManager.Play("deathExplosion.wav", 30); 
        
        AudioManager.SilenceEffects(1500);

        GameOverScreen.IsVisible = true; 
        PlayerShip.IsVisible = false; 
    }
    
    private void RestartFromGameOver() { 
        AudioManager.StopAll(); 
        ClearEntities(); 
        StartGameplay(); 
        AudioManager.PlayMusic("gameSong.wav", 60); 
    }

    private void BackToMenu() { 
        _isGameOver = false; 
        _isVictory = false; 
        GameOverScreen.IsVisible = false; 
        VictoryScreen.IsVisible = false; 
        _inTitleScreen = true; 
        TitleScreen.IsVisible = true; 
        PlayArea.IsVisible = false; 
        ClearEntities(); 
        
        AudioManager.PlayMusic("gameSong.wav", 60); 
    }

    private void ClearEntities() {
        _enemies.Clear(); _bullets.Clear(); _enemyBullets.Clear(); _explosions.Clear(); _specialProjectiles.Clear();
        ProjectilesLayer.Children.Clear();
        var toRemove = PlayArea.Children.OfType<Image>().Where(c => c != PlayerShip).ToList();
        foreach (var child in toRemove) PlayArea.Children.Remove(child);
        foreach (var wave in _waves) wave.Spawned = false;
    }
}