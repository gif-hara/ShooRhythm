using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PointerUpHandler : MonoBehaviour, IPointerDownHandler
    {
        private readonly Subject<PointerEventData> onHandled = new();
        
        public Observable<PointerEventData> OnHandledAsObservable() => onHandled;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            onHandled.OnNext(eventData);
        }
    }
}
