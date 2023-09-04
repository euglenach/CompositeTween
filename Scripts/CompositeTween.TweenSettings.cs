using DG.Tweening;

namespace DotweenExtensions
{
    public partial class CompositeTween
    {
        public CompositeTween SetAutoKill(bool autoKillOnCompletion = true)
        {
            if(disposed) return this;
            foreach(var tween in tweens)
            {
                tween?.SetAutoKill(autoKillOnCompletion);
            }

            return this;
        }
    }
}
