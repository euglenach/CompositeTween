namespace DOTweenExtensions
{
    public partial class CompositeTween
    {
        public void SetTimeScale(float timeScale)
        {
            if(disposed) return;
            foreach(var tween in tweens)
            {
                if(tween is null) continue;
                tween.timeScale = timeScale;
            }
        }
    }
}
