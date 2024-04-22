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
                var contentsRecord = masterDataMeadowSpec.ToContentsRecord();
                var gameController = TinyServiceLocator.Resolve<GameController>();
                var gameData = TinyServiceLocator.Resolve<GameData>();
                rootElement.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        if (!contentsRecord.IsCompleted(gameData.Stats))
                        {
                            GameUtility.ShowContentsConditionsNotification(contentsRecord);
                        }
                        else
                        {
                            await gameController.ApplyRewardAsync(contentsRecord);
                        }
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
