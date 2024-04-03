using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AudioController : MonoBehaviour, IBootable
    {
        [SerializeField]
        private AudioSource seSource;

        [SerializeField]
        private AudioSource bgmSource;

        public UniTask BootAsync()
        {
            var instance = Instantiate(this);
            DontDestroyOnLoad(instance.gameObject);
            TinyServiceLocator.RegisterAsync(instance, destroyCancellationToken).Forget();
            return UniTask.CompletedTask;
        }

        public void PlaySE(AudioClip clip)
        {
            seSource.PlayOneShot(clip);
        }

        public void PlayBGM(AudioClip clip)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }
}
