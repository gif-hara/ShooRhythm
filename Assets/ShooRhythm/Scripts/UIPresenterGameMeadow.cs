using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameMeadow
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);

            SetupAnimalButton("Root.Cow", 1);
            SetupAnimalButton("Root.Pig", 2);
            SetupAnimalButton("Root.Chicken", 3);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void SetupAnimalButton(string rootName, int meadowSpecId)
            {
                var rootElement = document.Q<HKUIDocument>(rootName);
                var masterDataMeadowSpec = TinyServiceLocator.Resolve<MasterData>().MeadowSpecs.Get(meadowSpecId);
                var gameController = TinyServiceLocator.Resolve<GameController>();
                var gameData = TinyServiceLocator.Resolve<GameData>();
                var button = rootElement.Q<ObservablePointerClickTrigger>("Button");
                button.OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        var availableCoolTimeIndex = gameData.CurrentUserData.GetAvailableCoolTimeIndex();
                        if (availableCoolTimeIndex == -1)
                        {
                            GameUtility.ShowRequireCoolDownNotification();
                            return;
                        }
                        if (masterDataMeadowSpec.HasItems())
                        {
                            await gameController.ProcessMeadowAsync(meadowSpecId);
                            GameUtility.PlayAcquireItemEffectAsync(document, (RectTransform)button.transform, null, cancellationToken).Forget();
                            gameData.CurrentUserData.SetCoolTime(availableCoolTimeIndex, masterDataMeadowSpec.CoolTimeSeconds);
                        }
                        else
                        {
                            masterDataMeadowSpec.ShowRequireItemNotification();
                        }
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
