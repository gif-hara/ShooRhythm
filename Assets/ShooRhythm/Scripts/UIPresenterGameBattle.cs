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
            var enemyInstanceData = await gameController.GetEnemyInstanceDataAsync(dungeonType);
            var enemySpec = masterData.EnemySpecs.Get(dungeonType)
                .FirstOrDefault(x => x.Id == enemyInstanceData.EnemyId);
            Assert.IsNotNull(enemySpec, $"EnemySpec is null. dungeonType: {dungeonType}, enemyId: {enemyInstanceData.EnemyId}");

            document.Q<TMP_Text>("Text.Enemy").text = enemySpec.Name;

            document.Q<ObservablePointerClickTrigger>("Button.Attack").OnPointerClickAsObservable()
                .SubscribeAwait(async (_, ct) =>
                {
                    var equipmentItemId = gameData.CurrentUserData.equipmentItemId.Value;
                    var damage = equipmentItemId == 0
                        ? TinyServiceLocator.Resolve<GameDesignData>().DefaultDamage
                        : masterData.WeaponSpecs.Get(equipmentItemId).Strength;
                    var attackResultType = await gameController.AttackEnemyAsync(dungeonType, damage);
                    if (attackResultType == Define.AttackResultType.Defeat)
                    {
                        GameUtility.PlayAcquireItemEffectAsync(document, document.Q<RectTransform>("AcquireItemEffectParent"), null, cancellationToken).Forget();
                    }
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
