using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameBattle
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, Define.DungeonType dungeonType, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            await gameController.ProcessBattleGetEnemyInstanceDataAsync(dungeonType);
            var enemyInstanceData = gameData.DungeonEnemyInstanceDatas[dungeonType];
            // 敵を倒した場合を考慮してないかもしれない
            var enemySpec = masterData.EnemySpecs.Get(dungeonType)
                .FirstOrDefault(x => x.Id == enemyInstanceData.EnemyId);
            Assert.IsNotNull(enemySpec, $"EnemySpec is null. dungeonType: {dungeonType}, enemyId: {enemyInstanceData.EnemyId}");

            document.Q<TMP_Text>("Text.Enemy").text = enemySpec.Name;

            document.Q<ObservablePointerClickTrigger>("Button.Attack").OnPointerClickAsObservable()
                .SubscribeAwait(async (_, ct) =>
                {
                    var availableCoolTimeIndex = gameData.CurrentUserData.GetAvailableCoolTimeIndex();
                    if (availableCoolTimeIndex == -1)
                    {
                        GameUtility.ShowRequireCoolDownNotification();
                        return;
                    }
                    var equipmentItemId = gameData.CurrentUserData.equipmentItemId.Value;
                    var coolTimeSeconds = enemySpec.CoolTimeSeconds;
                    var damage = equipmentItemId == 0
                        ? TinyServiceLocator.Resolve<GameDesignData>().DefaultDamage
                        : masterData.WeaponSpecs.Get(equipmentItemId).Strength;
                    var attackResultType = await gameController.ProcessBattleAttackEnemyAsync(dungeonType, damage);
                    if (attackResultType == Define.AttackResultType.Defeat)
                    {
                        GameUtility.PlayAcquireItemEffectAsync(document, document.Q<RectTransform>("AcquireItemEffectParent"), null, cancellationToken).Forget();
                    }
                    gameData.CurrentUserData.SetCoolTime(availableCoolTimeIndex, coolTimeSeconds);
                })
                .AddTo(document);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
