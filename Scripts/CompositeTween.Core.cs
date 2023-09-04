using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace DOTweenExtensions
{
    public partial class CompositeTween : IDisposable, ICollection<Tween>
    {
        private List<Tween> tweens = new();
        private readonly object gate = new();
        private bool disposed;
        private int count;
        private const int SHRINK_THRESHOLD = 64;
        public bool IsDisposed => disposed;
        private CancelBehaviour cancelBehaviour = CancelBehaviour.Kill;

        public CompositeTween(CancelBehaviour cancelBehaviour = CancelBehaviour.Kill)
        {
            this.cancelBehaviour = cancelBehaviour;
        }

        public CompositeTween(int capacity, CancelBehaviour cancelBehaviour = CancelBehaviour.Kill)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            tweens = new List<Tween>(capacity);
            this.cancelBehaviour = cancelBehaviour;
        }

        public CompositeTween(CancelBehaviour cancelBehaviour = CancelBehaviour.Kill, params Tween[] tweens)
        {
            if (tweens == null)
                throw new ArgumentNullException(nameof(tweens));
            
            this.tweens = new List<Tween>(tweens);
            this.cancelBehaviour = cancelBehaviour;
            count = this.tweens.Count;
        }

        public CompositeTween(IEnumerable<Tween> tweens, CancelBehaviour cancelBehaviour = CancelBehaviour.Kill)
        {
            if (tweens == null)
                throw new ArgumentNullException(nameof(tweens));
            
            this.tweens = new List<Tween>(tweens);
            this.cancelBehaviour = cancelBehaviour;
            count = this.tweens.Count;
        }
        
        public int Count => count;
        
        public void Add(Tween item)
        {
            if(item == null) return;

            var shouldKill = false;
            lock (gate)
            {
                shouldKill = disposed;
                if (!disposed)
                {
                    tweens.Add(item);
                    count++;
                }
            }
            if (shouldKill)
                KillAction(item);
        }
        
        public bool Remove(Tween item)
        {
            if(item == null) return false;
            
            var shouldKill = false;

            lock (gate)
            {
                if(!disposed)
                {
                    var i = tweens.IndexOf(item);
                    if (i >= 0)
                    {
                        shouldKill = true;
                        tweens[i] = null;
                        count--;

                        if (tweens.Capacity > SHRINK_THRESHOLD && count < tweens.Capacity / 2)
                        {
                            var old = tweens;
                            tweens = new List<Tween>(tweens.Capacity / 2);

                            foreach (var d in old)
                                if (d != null)
                                    tweens.Add(d);
                        }
                    }
                }
            }

            if (shouldKill)
                KillAction(item);

            return shouldKill;
        }
        
        public void Dispose()
        {
            var currentTweens = default(Tween[]);
            lock (gate)
            {
                if (!disposed)
                {
                    disposed = true;
                    currentTweens = tweens.ToArray();
                    tweens.Clear();
                    count = 0;
                }
            }

            if(currentTweens == null) return;
            foreach (var d in currentTweens)
            {
                d?.Kill();
            }
        }

        public void Clear()
        {
            var currentTweens = default(Tween[]);
            lock (gate)
            {
                currentTweens = tweens.ToArray();
                tweens.Clear();
                count = 0;
            }

            foreach (var t in currentTweens)
                if (t != null)
                    KillAction(t);
        }

        public bool Contains(Tween item)
        {
            if(item == null) return false;

            lock (gate)
            {
                return tweens.Contains(item);
            }
        }

        public void CopyTo(Tween[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            lock (gate)
            {
                var disArray = new List<Tween>();
                foreach (var item in tweens)
                {
                    if (item != null) disArray.Add(item);
                }

                Array.Copy(disArray.ToArray(), 0, array, arrayIndex, array.Length - arrayIndex);
            }
        }

        public bool IsReadOnly => false;

        void KillAction(Tween tween)
        {
            switch(cancelBehaviour)
            {
                case CancelBehaviour.Kill:
                    tween.Kill();
                    break;
                case CancelBehaviour.KillWithCompleteCallback:
                    tween.Kill(true);
                    break;
                case CancelBehaviour.Complete:
                    tween.Complete();
                    break;
                case CancelBehaviour.CompleteWithSequenceCallback:
                    tween.Complete(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerator<Tween> GetEnumerator()
        {
            var res = new List<Tween>();

            lock (gate)
            {
                foreach (var t in tweens)
                {
                    if (t != null) res.Add(t);
                }
            }

            return res.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class CompositeTweenExtensions
    {
        public static T SetLink<T>(this T tween, CompositeTween compositeTween) where T: Tween
        {
            compositeTween.Add(tween);
            return tween;
        }
    }

    public enum CancelBehaviour
    {
        Kill,
        KillWithCompleteCallback,
        Complete,
        CompleteWithSequenceCallback,
    }
}
