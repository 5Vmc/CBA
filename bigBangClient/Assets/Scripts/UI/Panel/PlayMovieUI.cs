using UnityEngine;
using deVoid.UIFramework;
using System;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;
using YooAsset;
using BigBang.Animation;
using Utils;

namespace BigBang.UI
{

    public class PlayMovieUIProperties : WindowProperties
    {
        public Action playEndCallBack = null;
        public string movieName = "";
        public PlayMovieUIProperties(string movieName, Action playEndCallBack)
        {
            this.movieName = movieName;
            this.playEndCallBack = playEndCallBack;
        }
    }

    public class PlayMovieUI : AWindowController<PlayMovieUIProperties>
    {
        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private PlayMovieUIAnim anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            videoPlayer.loopPointReached += OnLoopPointReached;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            videoPlayer.loopPointReached -= OnLoopPointReached;
        }

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            anim.Init();
            EnableRender();
            anim.PlayBeforePlayMovie();
            await PlayMovie();
            anim.PlayAfterPlayMovie();
        }

        private void OnLoopPointReached(VideoPlayer source)
        {
            OnClose(closeBtn);
        }

        private void OnClose(BabuButton sender)
        {
            Properties.playEndCallBack?.Invoke();
            DisableRender();
            UIController.Instance.CloseWindow<PlayMovieUI>();
        }

        public async Task PlayMovie()
        {
            // VideoClip videoClip = await LoadMovie(Properties.movieName);
            string url = getStreamingAssetsVideoFilePath(Properties.movieName + ".mp4");
            videoPlayer.url = url;
            videoPlayer.Play();
        }

        private string getStreamingAssetsVideoFilePath(string fileName)
        {
            string path =
#if UNITY_ANDROID && !UNITY_EDITOR
        Application.streamingAssetsPath + "/Videos/" + fileName;
#elif UNITY_IPHONE && !UNITY_EDITOR
        "file://" + Application.streamingAssetsPath + "/Videos/" + fileName;
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
        "file://" + Application.streamingAssetsPath + "/Videos/" + fileName;
#else
        Application.streamingAssetsPath + "/Videos/" + fileName;
#endif
            return path;
        }

        // 视频如果被再次LZ4压缩会报错，YooAsset目前不大好调整，mp4文件直接放到streamingAssets目录下，不会被压缩
        // public async Task<VideoClip> LoadMovie(string videoName)
        // {
        //     if (string.IsNullOrEmpty(videoName))
        //     {
        //         Debug.LogWarning("PlayMovieUI , LoadMovie , videoName is null or empty");
        //         return null;
        //     }
        //     AssetOperationHandle assetOperationHandle = YooAssets.LoadAssetAsync<VideoClip>(ResourcePath.VideoPath + videoName);
        //     await assetOperationHandle.Task;
        //     VideoClip videoClip = assetOperationHandle.AssetObject as VideoClip;
        //     if (videoClip == null)
        //     {
        //         Debug.LogWarningFormat("PlayMovieUI , LoadMovie , videoClip is null , videoName = {0}", videoName);
        //         return null;
        //     }
        //     return videoClip;
        // }

        RenderTexture renderTextureTemp = null;
        // 启用渲染
        private void EnableRender()
        {
            if (renderTextureTemp != null)
            {
                DisableRender();
            }
            renderTextureTemp = RenderTexture.GetTemporary(720, 1680, 24);
            renderTextureTemp.antiAliasing = 8;
            renderTextureTemp.autoGenerateMips = false;
            renderTextureTemp.useMipMap = false;
            rawImage.texture = renderTextureTemp;
            videoPlayer.targetTexture = renderTextureTemp;
        }

        // 禁用渲染
        private void DisableRender()
        {
            if (renderTextureTemp == null)
            {
                return;
            }
            RenderTexture.ReleaseTemporary(renderTextureTemp);
            videoPlayer.targetTexture = null;
            rawImage.texture = null;
        }


    }
}