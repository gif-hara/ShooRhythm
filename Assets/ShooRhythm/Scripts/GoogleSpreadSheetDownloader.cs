using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GoogleSpreadSheetDownloader
    {
        const string url = "https://script.google.com/macros/s/AKfycby4nP6l_mD-MLY87B8namuqlee0vs_exsfi5OGUmduFvfJWVCDrw8SXPJpcRGlYf_76/exec";

        public static async UniTask<string> DownloadAsync(string sheetName)
        {
            var request = UnityWebRequest.Get(url + "?sheetName=" + sheetName);
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                // エラー処理
                UnityEngine.Debug.LogError(request.error);
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }
}
