using UnityEngine;

namespace Babu.SDK
{
    class ReviewServiceManager : BabuSingleton<ReviewServiceManager>
    {
        protected AndroidReviewService _androidReviewService = new AndroidReviewServiceDefault();

        public void SetAndroidReviewServiceHandler(AndroidReviewService androidReviewService)
        {
            _androidReviewService = androidReviewService;
        }

        public void Review()
        {
            Debug.Log("Start Review");
#if UNITY_IOS
            UnityEngine.iOS.Device.RequestStoreReview();
#else
            _androidReviewService?.Review();
#endif
        }
    }
}