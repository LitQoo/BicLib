# AdMob lifecycle regression checks

Run `bash Tests/AdmobLifecycle/run.sh` from the BicLib root. The test harness is
outside Unity compilation (`!UNITY_5_3_OR_NEWER`) and compiles the actual manager.

2026-10-30: SpaceQuiz's development APK reported `get_isPlaying can only be called
from the main thread` at `InsightsEmitter → MobileAdsClient → MobileAds.Initialize`.
UMP completion callbacks were forwarded without dispatching to Unity's thread.

The regression doubles complete consent-information and consent-form callbacks on
real worker threads and reject form/SDK initialization on those threads. The new
tests fail on the previous manager and pass after dispatching both callbacks via
Unity's captured SynchronizationContext. A scene-owned TweenPool can disappear
during scene changes; resolving it on an SDK worker can create a Unity object on
the wrong thread. Context tests clear the old tween queue and verify initialization
and loaded-ad completion still arrive. Preserve this dispatch when changing
consent or initialization code. Consent decisions and request eligibility are
unchanged; no consent is bypassed.

41 checks also cover shared ad units/placements, 10,000 readiness polls, bounded
backoff and cooldown, expiry, diagnostics, duplicate close/fail callbacks, reward
completion, and cleanup. This harness verifies callback contracts, not actual ad
impressions or availability; run device tests for those.
