using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameFishing
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
           
            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null && document.gameObject != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
