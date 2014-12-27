using System;

namespace Alarmy.Infrastructure
{
    internal class SoundPlayer : IDisposable
    {
        private readonly System.Media.SoundPlayer soundPlayer;

        public bool IsPlaying { get; private set; }
        public string Path { get; private set; }

        public SoundPlayer(string path)
        {
            this.Path = path;
            this.soundPlayer = new System.Media.SoundPlayer(path);
        }

        public void Play()
        {
            if (this.IsPlaying)
                return;
            this.IsPlaying = true;

            if (!this.soundPlayer.IsLoadCompleted)
                this.soundPlayer.Load();

            this.soundPlayer.PlayLooping();
        }

        public void Stop()
        {
            this.IsPlaying = false;
            this.soundPlayer.Stop();
        }

        public void Dispose()
        {
            if (this.soundPlayer != null)
            {
                this.soundPlayer.Dispose();
            }
        }
    }
}
