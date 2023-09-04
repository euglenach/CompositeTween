using DG.Tweening;

namespace DotweenExtensions
{
    public partial class CompositeTween
    {
        public CompositeTween Play()
        {
            if(disposed) return this;
            foreach(var tween in tweens)
            {
                tween?.Play();
            }

            return this;
        }
        
        public void PlayForward()
        {
            if(disposed) return;
            foreach(var tween in tweens)
            {
                tween?.PlayForward();
            }
        }
        
        public void PlayBackwards()
        {
            if(disposed) return;
            foreach(var tween in tweens)
            {
                tween?.PlayBackwards();
            }
        }
        
        public CompositeTween Pause()
        {
            if(disposed) return this;
            foreach(var tween in tweens)
            {
                tween?.Pause();
            }

            return this;
        }
    }
}
