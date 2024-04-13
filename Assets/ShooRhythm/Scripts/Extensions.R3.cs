using System;
using HK;
using R3;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static Observable<Unit> AsObservable(this Action self)
        {
            return Observable.FromEvent(
                x => self += x,
                x => self -= x
            );
        }

        public static Observable<T> AsObservable<T>(this Action<T> self)
        {
            return Observable.FromEvent<T>(
                x => self += x,
                x => self -= x
            );
        }
    }
}
