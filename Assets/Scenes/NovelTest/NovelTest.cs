using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NovelTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayTutorial_1();
    }

    private async void PlayTutorial_1()
    {
        // await NovelManager.InitAsync();
        // NovelManager.Instance.PlayTutorial(1);

        var bgm = await SoundManager.Instance.LoadAudioClipByAddressables("BattleBGM");
        SoundManager.Instance.Play(bgm, SystemEnum.Sound.Bgm);

    }
    
    // public void OnButtonClick()
    // {
    //     
    //     NovelManager.Instance.PlayScript("Tutorial_2");
    // }


}
