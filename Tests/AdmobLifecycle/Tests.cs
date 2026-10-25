#if !UNITY_5_3_OR_NEWER
using System;
using BicUtil.Ads;
using BicUtil.Tween;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
class Tests {
    static int checks;
    static void Assert(bool condition,string message) { checks++; if(!condition) throw new Exception(message); }
    static AdmobManager New() {
        BicTween.Reset(); InterstitialAd.Requests=0; RewardedAd.Requests=0;
        InterstitialAd.ThrowLoad=false; ConsentInformation.Allowed=true;
        var manager=new AdmobManager(false);
        manager.SetAdsSettingAndroidOnly("int",new object[]{"clear","restart"});
        manager.SetAdsSettingAndroidOnly("reward",new object[]{"hint","skip","visitor"});
        manager.Initialize(null); BicTween.Pump(); return manager;
    }
    static void Poll(AdmobManager manager) {
        for(int i=0;i<10000;i++) {
            manager.IsReadyRewardBased(i%2==0?"hint":"visitor");
            manager.IsReadyInterstitial(i%2==0?"clear":"restart");
        }
    }
    static void Main() {
        var manager=New(); Poll(manager);
        Assert(RewardedAd.Requests==1 && InterstitialAd.Requests==1,"poll flood must have only one in-flight load per unit");
        var reward=new RewardedAd(); var interstitial=new InterstitialAd();
        RewardedAd.Complete(reward,null); InterstitialAd.Complete(interstitial,null);
        Assert(!manager.IsReadyRewardBased("skip"),"worker callback must wait for main-thread dispatch");
        BicTween.Pump(); Poll(manager);
        Assert(RewardedAd.Requests==1 && InterstitialAd.Requests==1,"placements must share their unit cache");
        Assert(manager.IsReadyRewardBased("skip") && manager.IsReadyInterstitial("restart"),"other placement must see loaded ad");
        int callbacks=0; AdsResult result=AdsResult.Failed;
        manager.ShowRewardBased("hint",r=>{callbacks++;result=r;});
        Poll(manager);
        Assert(RewardedAd.Requests==1 && InterstitialAd.Requests==1,"no load or readiness while full-screen ad is showing");
        reward.Reward(new Reward()); reward.Close(); reward.Fail(); BicTween.Pump();
        Assert(callbacks==1 && result==AdsResult.Finished && reward.DestroyCount==1,"reward/close/fail must complete and destroy once");
        Poll(manager); Assert(RewardedAd.Requests==1,"polling must respect post-show delay");
        BicTween.Advance(0.5f); Assert(RewardedAd.Requests==2,"one reload after close");
        var next=new RewardedAd(); RewardedAd.Complete(next,null); BicTween.Pump();
        manager.ShowRewardBased("skip",r=>result=r); next.Close(); BicTween.Pump();
        Assert(result==AdsResult.Skipped,"reward state must not leak into next ad");
        manager.ShowInterstitial("restart",r=>{callbacks++;result=r;}); interstitial.Fail(); interstitial.Close(); BicTween.Pump();
        Assert(result==AdsResult.Failed && interstitial.DestroyCount==1,"display failure must not be reported as success or destroy twice");

        manager=New(); Poll(manager);
        for(int failures=1;failures<=6;failures++) {
            RewardedAd.Complete(null,new LoadAdError()); InterstitialAd.Complete(null,new LoadAdError()); BicTween.Pump();
            Poll(manager);
            Assert(RewardedAd.Requests==failures && InterstitialAd.Requests==failures,"readiness bypassed failed-load backoff");
            Assert(BicTween.Timers.Count==(failures<=5?2:0),"retry chains must be bounded, one per unit");
            if(failures<=5) BicTween.Advance((float)Math.Pow(2,failures-1));
        }
        BicTween.Advance(299); Poll(manager);
        Assert(RewardedAd.Requests==6 && InterstitialAd.Requests==6,"exhausted retries need cooldown");
        BicTween.Advance(1); Poll(manager);
        Assert(RewardedAd.Requests==7 && InterstitialAd.Requests==7,"later explicit readiness may recover after cooldown");
        RewardedAd.Complete(new RewardedAd(),null); InterstitialAd.Complete(new InterstitialAd(),null); BicTween.Pump();
        Assert(manager.IsReadyRewardBased("visitor"),"successful retry must recover");

        manager=New(); manager.LoadInterstitial("clear");
        var expired=new InterstitialAd { Ready=false }; InterstitialAd.Complete(expired,null); BicTween.Pump();
        manager.IsReadyInterstitial("restart");
        Assert(expired.DestroyCount==1 && InterstitialAd.Requests==2,"expired ad must be destroyed before replacement");
        manager=New(); InterstitialAd.ThrowLoad=true; manager.LoadInterstitial("clear"); Poll(manager);
        Assert(InterstitialAd.Requests==1 && BicTween.Timers.Count==1,"synchronous SDK exception must respect backoff");
        manager=New(); ConsentInformation.Allowed=false; Poll(manager);
        Assert(RewardedAd.Requests==0 && InterstitialAd.Requests==0,"consent must gate every load");
        Console.WriteLine("PASS: "+checks+" lifecycle checks, including 10,000-call polling floods, shared placements, bounded retries, callback ordering and cleanup.");
    }
}
#endif
