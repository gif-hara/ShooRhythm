using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameEquipment
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var uiPresenterGameSelectItem = new UIPresenterGameSelectItems();
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            uiPresenterGameSelectItem.OnSelectedItemAsObservable()
                .SubscribeAwait(async (x, ct) =>
                {
                    await TinyServiceLocator.Resolve<GameController>().SetUserEquipmentItemIdAsync(gameData.CurrentUserId, x);
                })
                .RegisterTo(cancellationToken);
            uiPresenterGameSelectItem.BeginAsync(
                document.Q<HKUIDocument>("SelectItems"),
                x =>
                {
                    return x.Where(y => masterData.WeaponSpecs.ContainsKey(y.Key));
                })
                .Forget();
            gameData.CurrentUserData.equipmentItemId
                .Subscribe(x =>
                {
                    var masterDataItem = x != 0 ? masterData.Items.Get(x) : null;
                    masterData.WeaponSpecs.TryGetValue(x, out var weaponSpec);
                    document.Q<TMP_Text>("Text.EquipmentName").text = masterDataItem != null ? masterDataItem.Name : "None";
                    document.Q<TMP_Text>("Text.Strength").text = weaponSpec != null ? weaponSpec.Strength.ToString() : "0";
                })
                .RegisterTo(cancellationToken);
            
            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null && document.gameObject != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
