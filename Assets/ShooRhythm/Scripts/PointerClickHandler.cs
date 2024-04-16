using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private readonly Subject<PointerEventData> onHandled = new();
        
        public Observable<PointerEventData> OnHandledAsObservable() => onHandled;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onHandled.OnNext(eventData);
        }
    }
}
