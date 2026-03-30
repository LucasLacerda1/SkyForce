using NetCoreAudio;
using System.IO;
using System;
using System.Threading.Tasks;

namespace SkyForce.Services;

public static class AudioManager
{
    private static readonly Player MainPlayer = new Player();
    private static readonly Player FastPlayer = new Player();
    private static readonly Player MusicPlayer = new Player(); 
    private static readonly string SoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds");
    private static string _currentMusic = "";
    private static bool _blockSfx = false;

    public static void Play(string fileName, byte volume = 20)
    {
        if (_blockSfx) return;

        string filePath = Path.Combine(SoundPath, fileName);
        if (!File.Exists(filePath)) return;

        Task.Run(async () => {
            try {
                var player = fileName.Contains("bullet") ? FastPlayer : MainPlayer;
                if (player.Playing) await player.Stop();
                await player.SetVolume(volume);
                await player.Play(filePath);
            } catch { }
        });
    }

    public static void PlayMusic(string fileName, byte volume = 80)
    {
        if (_currentMusic == fileName) return;

        string filePath = Path.Combine(SoundPath, fileName);
        if (!File.Exists(filePath)) return;

        Task.Run(async () => {
            try {
                if (MusicPlayer.Playing) await MusicPlayer.Stop();
                _currentMusic = fileName;
                await MusicPlayer.SetVolume(volume);
                await MusicPlayer.Play(filePath);
            } catch { }
        });
    }

    public static void StopMusic()
    {
        _currentMusic = "";
        Task.Run(async () => {
            try { if (MusicPlayer.Playing) await MusicPlayer.Stop(); } catch { }
        });
    }

    public static void SilenceEffects(int durationMs)
    {
        _blockSfx = true;
        Task.Delay(durationMs).ContinueWith(_ => _blockSfx = false);
    }

    public static void StopAll()
    {
        _currentMusic = "";
        _blockSfx = false;
        Task.Run(async () => {
            try {
                await MusicPlayer.Stop();
                await MainPlayer.Stop();
                await FastPlayer.Stop();
            } catch { }
        });
    }

    public static async Task PreLoad()
    {
        try {
            await MainPlayer.SetVolume(20);
            await FastPlayer.SetVolume(20);
            await MusicPlayer.SetVolume(80);
        } catch { }
    }
}