//  PlaymodeStateObserver.cs
//  http://kan-kikuchi.hatenablog.com/entry/PlaymodeStateObserver
//
//  Created by kan.kikuchi on 2016.05.26.
//
// Modified by AoiKamishiro on 2021.07.26

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UnityEditor;

namespace Kamishiro.VRChatUDON.AKSwitch
{
    /// <summary>
    /// エディタの状態を監視するクラス
    /// </summary>
    [InitializeOnLoad]
    public static class PlaymodeStateObserver
    {
        internal static PlayModeStateChangedType playModeState = PlayModeStateChangedType.Ended;

        //ポーズされていたか、実行されていたか
        private static bool _wasPaused = false, _wasPlaying = false;

        /// <summary>
        /// エディタの状態が切り替わった時の種類
        /// </summary>
        public enum PlayModeStateChangedType
        {
            PressedPlayButton, Began, PressedEndButton, Ended, Paused, Resumed, ExecutedStep, Exception
        }

        //=================================================================================
        //イベント
        //=================================================================================

        /// <summary>
        /// エディタの状態が変わった時のイベント
        /// </summary>
        public static event Action<PlayModeStateChangedType> OnChangedState = delegate { };

        /// <summary>
        /// エディタの再生ボタンを押した時のイベント(まだ再生はされてない)
        /// </summary>
        public static event Action OnPressedPlayButton = delegate { };

        /// <summary>
        /// エディタの再生を開始した時のイベント
        /// </summary>
        public static event Action OnBegan = delegate { };

        /// <summary>
        /// エディタの再生終了ボタンを押した時のイベント(まだ終了されていない)
        /// </summary>
        public static event Action OnPressedEndButton = delegate { };

        /// <summary>
        /// エディタ再生を終了した時のイベント
        /// </summary>
        public static event Action OnEnded = delegate { };

        /// <summary>
        /// エディタ再生を一時停止した時のイベント(エディタが再生してなくても呼ばれる)
        /// </summary>
        public static event Action OnPaused = delegate { };

        /// <summary>
        /// エディタ再生の一時停止を解除した時のイベント(エディタが再生してなくても呼ばれる)
        /// </summary>
        public static event Action OnResumed = delegate { };

        /// <summary>
        /// Stepを実行した時(エディタが再生してない時はよばれない)
        /// </summary>
        public static event Action OnExecutedStep = delegate { };

        //=================================================================================
        //初期化
        //=================================================================================

        /// <summary>
        /// コンストラクタ(InitializeOnLoad属性によりエディター起動時に呼び出される)
        /// </summary>
        static PlaymodeStateObserver()
        {
            //Playmodeの状態が変わった時のイベントを登録
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        private static void OnPauseStateChanged(PauseState state)
        {
            OnStateChanged();
        }
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            OnStateChanged();
        }

        //=================================================================================
        //状態変更
        //=================================================================================

        //Playmodeの状態が変わった
        private static void OnStateChanged()
        {
            //予期せぬ判定が出た時用に初期値にException
            PlayModeStateChangedType playModeStateChangedType = PlayModeStateChangedType.Exception;

            //前回から一時停止or解除の状態が変わった
            if (_wasPaused != EditorApplication.isPaused)
            {
                //一時停止
                if (EditorApplication.isPaused)
                {
                    //ポーズを押した状態で実行した時
                    if (EditorApplication.isPlaying && !_wasPlaying)
                    {
                        OnBegan();
                    }

                    playModeStateChangedType = PlayModeStateChangedType.Paused;
                    OnPaused();
                }
                //再開
                else
                {
                    playModeStateChangedType = PlayModeStateChangedType.Resumed;
                    OnResumed();
                }
            }
            //再生ボタンを押した
            else if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                playModeStateChangedType = PlayModeStateChangedType.PressedPlayButton;
                OnPressedPlayButton();
            }
            //再生開始orステップ実行
            else if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (EditorApplication.isPaused)
                {
                    playModeStateChangedType = PlayModeStateChangedType.ExecutedStep;
                    OnExecutedStep();
                }
                else
                {
                    playModeStateChangedType = PlayModeStateChangedType.Began;
                    OnBegan();
                }
            }
            //再生終了ボタンを押した
            else if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                playModeStateChangedType = PlayModeStateChangedType.PressedEndButton;
                OnPressedEndButton();
            }
            //再生終了
            else if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                playModeStateChangedType = PlayModeStateChangedType.Ended;
                OnEnded();
            }

            OnChangedState(playModeStateChangedType);

            //現在の状態を保存
            _wasPaused = EditorApplication.isPaused;
            _wasPlaying = EditorApplication.isPlaying;
            playModeState = playModeStateChangedType;

            //ログ表示
            //Debug.Log(playModeStateChangedType.ToString());
        }

    }
}
#endif