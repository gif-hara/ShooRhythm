using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OnPointerClickTrigger : MonoBehaviour, IPointerClickHandler
    {
        private readonly Subject<PointerEventData> onClick = new();
        
        public Observable<PointerEventData> OnClickAsObservable() => onClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.OnNext(eventData);
        }
    }
}
