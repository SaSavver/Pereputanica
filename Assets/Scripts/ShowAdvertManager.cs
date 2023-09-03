using GoogleMobileAds.Api;
using UnityEngine;

public class ShowAdvertManager : MonoBehaviour
{
    [SerializeField] private bool _testMode = true;
    [SerializeField] private string _interstitialId;
    [SerializeField] private string _interstitialIdIOS;
    [SerializeField] private string _interstitialTest;

    public string InterstitialId => _testMode ? _interstitialTest : (Application.platform == RuntimePlatform.Android ? _interstitialId : _interstitialIdIOS);

    private InterstitialAd interstitial;
    

    private void LoadAd()
    {
        InterstitialAd.Load(InterstitialId, new AdRequest(),
            (InterstitialAd ad, LoadAdError  loadAdError) =>
            {
                if (loadAdError != null)
                {
                    Debug.Log("Interstitial ad failed to load with error: " +
                              loadAdError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    Debug.Log("Interstitial ad failed to load.");
                    return;
                }

                Debug.Log("Interstitial ad loaded.");
                interstitial = ad;
            });
    }

    private void Start()
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize((_) => { LoadAd(); });
    }

    public void ShowIntersitial()
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            interstitial.Show();
        }
        else
        {
            Debug.Log("Interstitial ad cannot be shown.");
        }
    }
}