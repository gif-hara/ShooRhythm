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
            uiPresenterGameSelectItem.OnSelectedItemAsObservable()
                .SubscribeAwait(async (x, ct) =>
                {
                    await TinyServiceLocator.Resolve<GameController>().SetUserEquipmentItemIdAsync(gameData.UserId, x);
                    UpdateEquipmentViews();
                })
                .RegisterTo(cancellationToken);
            UpdateEquipmentViews();
           
            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null && document.gameObject != null)
            {
                Object.Destroy(document.gameObject);
            }

            void UpdateEquipmentViews()
            {
                var masterData = TinyServiceLocator.Resolve<MasterData>();
                var equipmentItemId = gameData.UserEquipmentItemId;
                var masterDataItem = equipmentItemId != 0 ? masterData.Items.Get(equipmentItemId) : null;
                masterData.WeaponSpecs.TryGetValue(equipmentItemId, out var weaponSpec);
                document.Q<TMP_Text>("Text.EquipmentName").text = masterDataItem != null ? masterDataItem.Name : "None";
                document.Q<TMP_Text>("Text.Strength").text = weaponSpec != null ? weaponSpec.Strength.ToString() : "0";
            }
        }
    }
}
