﻿/**
 * $File: JCS_SoundManager.cs $
 * $Date: $
 * $Revision: $
 * $Creator: Jen-Chieh Shen $
 * $Notice: $
 */
using UnityEngine;
using System.Collections;


namespace JCSUnity
{

    /// <summary>
    /// Manage of all the music, sound and sfx in the game.
    /// </summary>
    [RequireComponent(typeof(JCS_SoundPlayer))]
    public class JCS_SoundManager
        : MonoBehaviour
    {

        //----------------------
        // Public Variables
        public static JCS_SoundManager instance = null;

        //----------------------
        // Private Variables
        private AudioListener mAudioListener = null;

        // environment, ui, etc.
        private JCS_Vector<AudioSource> mSFXSounds = null;

        // personal stuff, personal skill, personal jump walk, etc.
        private JCS_Vector<AudioSource> mSkillsSounds = null;

        private JCS_SoundPlayer mGlobalSoundPlayer = null;

        private JCS_FadeSound mJCSFadeSound = null;

        // this only hold one audio clip on stack. I do not want
        // to get to messy about this.
        private AudioClip mOnStackAudioClip = null;

        private float mOnStackFadeInTime = 0;
        private float mOnStackFadeOutTime = 0;


        [Header("** Check Variables (JCS_SoundManager) **")]

        [Tooltip("")]
        [SerializeField]
        private AudioSource mBGM = null;

        [Tooltip("Current background music is playing.")]
        [SerializeField]
        private AudioClip mCurrentBGM = null;

        // boolean check if the background music switching.
        [SerializeField]
        private bool mSwitchingBGM = false;

        [Header("** General Music Settings (JCS_SoundManager) **")]

        [Tooltip("Do this scene using the specific setting?")]
        [SerializeField]
        private bool mOverrideSetting = false;

        [Tooltip("")]
        [SerializeField]
        private float mSoundFadeInTime = 1.5f;

        [Tooltip("")]
        [SerializeField]
        private float mSoundFadeOutTime = 1.5f;



        // real time that the bgm fade out.
        private float mRealSoundFadeOutTime = 0;

        private bool mDoneFadingOut = false;


        //----------------------
        // Protected Variables

        //========================================
        //      setter / getter
        //------------------------------
        public void SetAudioListener(AudioListener al) { this.mAudioListener = al; }
        public AudioListener GetAudioListener() { return this.mAudioListener; }
        public AudioSource GetBGMAudioSource() { return this.mBGM; }
        public void SetBackgroundMusic(AudioSource music)
        {
            this.mBGM = music;

            this.mBGM.volume = JCS_GameSettings.GetBGM_Volume();
            this.mBGM.mute = JCS_GameSettings.BGM_MUTE;
        }
        public JCS_Vector<AudioSource> GetEffectSounds() { return this.mSFXSounds; }
        public JCS_SoundPlayer GetGlobalSoundPlayer() { return this.mGlobalSoundPlayer; }

        public bool OverrideSetting { get { return this.mOverrideSetting; } }
        public float SoundFadeInTime { get { return this.mSoundFadeInTime; } set { this.mSoundFadeInTime = value; } }
        public float SoundFadeOutTime { get { return this.mSoundFadeOutTime; } set { this.mSoundFadeOutTime = value; } }

        //========================================
        //      Unity's function
        //------------------------------
        private void OnApplicationFocus(bool focusStatus)
        {
            // turn off all the sound effect
            if (!focusStatus)
            {
                // mute all the sound
                AudioListener.volume = 0;
            }
            else
            {
                // enable all the sound
                AudioListener.volume = 1;
            }
        }

        private void Awake()
        {
            instance = this;

            // try to get component, this is not guarantee.
            this.mJCSFadeSound = this.GetComponent<JCS_FadeSound>();

            mSFXSounds = new JCS_Vector<AudioSource>();
            mSkillsSounds = new JCS_Vector<AudioSource>();

            mGlobalSoundPlayer = this.GetComponent<JCS_SoundPlayer>();
        }

        private void Start()
        {
            if (JCS_Camera.main == null)
            {
                JCS_Debug.JcsErrors(
                    "JCS_SoundManager",

                    "There is no \"JCS_Camera\" assign!");

                return;
            }

            // Reset the sound every scene
            SetSFXSoundVolume(JCS_GameSettings.GetSFXSound_Volume());
            SetSkillsSoundVolume(JCS_GameSettings.GetSkillsSound_Volume());
            SetSFXSoundMute(JCS_GameSettings.EFFECT_MUTE);
            SetSkillsSoundMute(JCS_GameSettings.PERFONAL_EFFECT_MUTE);
        }

        private void Update()
        {
            DoSwitchBGM();

            DoStackBGM();
        }

        //========================================
        //      Self-Define
        //------------------------------
        //----------------------
        // Public Functions

        /// <summary>
        /// Switch the background music, fading in and out.
        /// </summary>
        /// <param name="soundClip"> clip to play. </param>
        /// <param name="fadeTime"> time to fade in and out. </param>
        /// <param name="loop"> loop music? </param>
        public void SwitchBackgroundMusic(
            AudioClip soundClip, 
            bool loop = true)
        {
            SwitchBackgroundMusic(
                soundClip,
                JCS_SoundSettings.instance.GetSoundFadeOutTimeBaseOnSetting(),
                JCS_SoundSettings.instance.GetSoundFadeInTimeBaseOnSetting());
        }

        /// /// <summary>
        /// Switch the background music, fading in and out.
        /// </summary>
        /// <param name="soundClip"> clip to play. </param>
        /// <param name="fadeInTime"> time to fade in. </param>
        /// <param name="fadeOutTime"> time to fade out. </param>
        /// <param name="loop"> loop music? </param>
        public void SwitchBackgroundMusic(
            AudioClip soundClip,
            float fadeInTime,
            float fadeOutTime,
            bool loop = true)
        {
            if (mSwitchingBGM)
                return;

            /* Check if the audio clip good to play. */
            if (soundClip == null)
                return;

            /**
             *  Try to get the fade sound component.
             *  
             *  Since we have multiple component have 
             */
            if (mJCSFadeSound == null)
            {
                mJCSFadeSound = this.GetComponent<JCS_FadeSound>();

                if (mJCSFadeSound == null)
                    mJCSFadeSound = this.gameObject.AddComponent<JCS_FadeSound>();
            }


            // get the background music audio source.
            AudioSource bgmAudioSource = GetBGMAudioSource();

            // check if loop
            bgmAudioSource.loop = loop;

            // set the audio source.
            mJCSFadeSound.SetAudioSource(bgmAudioSource);

            // active the fade sound in effect.
            mJCSFadeSound.FadeOut(
                0,
                /* Fade in the sound base on the setting. */
                fadeInTime);

            this.mRealSoundFadeOutTime = fadeOutTime;

            this.mSwitchingBGM = true;
            this.mDoneFadingOut = false;

            this.mCurrentBGM = soundClip;
        }

        /// <summary>
        /// Play one shot background music, after playing it.
        /// Play the on stack sound.
        /// </summary>
        /// <param name="oneShotClip"> One shot BGM </param>
        /// <param name="onStackClip"> Audio clip on stack </param>
        public bool PlayOneShotBackgroundMusic(
            AudioClip oneShotClip,
            AudioClip onStackClip)
        {
            return PlayOneShotBackgroundMusic(
                oneShotClip, 
                onStackClip,
                JCS_SoundSettings.instance.GetSoundFadeOutTimeBaseOnSetting(),
                JCS_SoundSettings.instance.GetSoundFadeInTimeBaseOnSetting());
        }

        /// <summary>
        /// Play one shot background music, after playing it.
        /// Play the on stack sound.
        /// </summary>
        /// <param name="oneShotClip"> One shot BGM </param>
        /// <param name="onStackClip"> Audio clip on stack </param>
        /// <param name="fadeInTime"> time to fade in the bgm </param>
        /// <param name="fadeOutTime"> time to fade out the bgm </param>
        public bool PlayOneShotBackgroundMusic(
            AudioClip oneShotClip, 
            AudioClip onStackClip, 
            float fadeInTime, 
            float fadeOutTime)
        {
            // stack must be empty before we play it.
            if (mOnStackAudioClip != null)
                return false;

            // store audio clip on stack.
            this.mOnStackAudioClip = onStackClip;

            this.mOnStackFadeInTime = fadeInTime;
            this.mOnStackFadeOutTime = fadeOutTime;

            // Play one shot the BGM
            SwitchBackgroundMusic(
                oneShotClip,
                fadeInTime,
                fadeOutTime,
                /* Dont loop. */
                false);

            return true;
        }

        /// <summary>
        /// Push to the sound effect into array ready for use!
        /// </summary>
        /// <param name="sound"></param>
        public void PlayOneShotSFXSound(int index)
        {
            AudioSource aud = mSFXSounds.at(index);
            if (aud.clip != null)
                aud.PlayOneShot(aud.clip, JCS_GameSettings.GetSFXSound_Volume());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        public void AssignSoundSource(JCS_SoundSettingType type, AudioSource source)
        {
            switch (type)
            {
                case JCS_SoundSettingType.NONE:
                    return;
                case JCS_SoundSettingType.BGM_SOUND:
                    SetBackgroundMusic(source);
                    break;
                case JCS_SoundSettingType.SFX_SOUND:
                    AssignSFX_Sound(source);
                    break;
                case JCS_SoundSettingType.SKILLS_SOUND:
                    AssignSkillsSound(source);
                    break;
            }
        }

        /// <summary>
        /// Set the sound volume base on type.
        /// </summary>
        /// <param name="type"> type of the sound you want to set. </param>
        /// <param name="volume"> volume of the sound. </param>
        public void SetSoundVolume(JCS_SoundSettingType type, float volume)
        {
            switch (type)
            {
                case JCS_SoundSettingType.NONE:
                    return;
                case JCS_SoundSettingType.BGM_SOUND:
                    GetBGMAudioSource().volume = volume;
                    break;
                case JCS_SoundSettingType.SFX_SOUND:
                    SetSFXSoundVolume(volume);
                    break;
                case JCS_SoundSettingType.SKILLS_SOUND:
                    SetSkillsSoundVolume(volume);
                    break;
            }
        }

        /// <summary>
        /// Set weather the sound are mute or not by sound type.
        /// </summary>
        /// <param name="type"> type of the sound. </param>
        /// <param name="act"> action of the mute </param>
        public void SetSoundMute(JCS_SoundSettingType type, bool act)
        {
            switch (type)
            {
                case JCS_SoundSettingType.NONE:
                    return;
                case JCS_SoundSettingType.BGM_SOUND:
                    GetBGMAudioSource().mute = act;
                    break;
                case JCS_SoundSettingType.SFX_SOUND:
                    SetSFXSoundMute(act);
                    break;
                case JCS_SoundSettingType.SKILLS_SOUND:
                    SetSkillsSoundMute(act);
                    break;
            }
        }

        //----------------------
        // Protected Functions

        //----------------------
        // Private Functions

        /// <summary>
        /// Set the sound volume in the list.
        /// </summary>
        /// <param name="list"> list of the audio source </param>
        /// <param name="vol"> target volume. </param>
        private void SetSoundVolume(JCS_Vector<AudioSource> list, float vol)
        {
            for (int index = 0;
                index < list.length;
                ++index)
            {
                list.at(index).volume = vol;
            }
        }

        /// <summary>
        /// Set the sound mute or not with list needed.
        /// </summary>
        /// <param name="list"> list of the audio source. </param>
        /// <param name="act"> target mute action. </param>
        private void SetSoundtMute(JCS_Vector<AudioSource> list, bool act)
        {
            for (int index = 0;
                index < list.length;
                ++index)
            {
                list.at(index).mute = act;
            }
        }

        /// <summary>
        /// Add a SFX in to the list in order to get manage.
        /// </summary>
        /// <param name="sound"> Unity's audio source class. </param>
        private void AssignSFX_Sound(AudioSource sound)
        {
            AssignSoundToList(mSFXSounds, sound);
        }

        /// <summary>
        /// Add a skill sound in to the list in order to get manage.
        /// </summary>
        /// <param name="sound"> Unity's audio source class. </param>
        private void AssignSkillsSound(AudioSource sound)
        {
            AssignSoundToList(mSkillsSounds, sound);
        }

        /// <summary>
        /// Assgin the audio source to audio source list.
        /// </summary>
        /// <param name="list"> List of audio source. </param>
        /// <param name="sound"> audio source to add into list. </param>
        private void AssignSoundToList(JCS_Vector<AudioSource> list, AudioSource sound)
        {
            if (sound == null)
            {
                JCS_Debug.JcsErrors("JCS_SoundManager", "Assigning Source that is null...");
                return;
            }

            list.push(sound);
            sound.volume = JCS_GameSettings.GetSFXSound_Volume();
        }

        /// <summary>
        /// Set the SFX volume.
        /// </summary>
        /// <param name="vol"> volume to set. </param>
        private void SetSFXSoundVolume(float vol)
        {
            SetSoundVolume(mSFXSounds, vol);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vol"></param>
        private void SetSkillsSoundVolume(float vol)
        {
            SetSoundVolume(mSkillsSounds, vol);
        }

        /// <summary>
        /// Set the SFX mute or not mute.
        /// </summary>
        /// <param name="act"> target mute action. </param>
        private void SetSFXSoundMute(bool act)
        {
            SetSoundtMute(mSFXSounds, act);
        }

        /// <summary>
        /// Set the skill sound mute or not mute.
        /// </summary>
        /// <param name="act"> target mute action. </param>
        private void SetSkillsSoundMute(bool act)
        {
            SetSoundtMute(mSkillsSounds, act);
        }

        /// <summary>
        /// Do the switching bgm algorithm.
        /// </summary>
        private void DoSwitchBGM()
        {
            if (!mSwitchingBGM)
                return;

            /* Once if fade out we load next sound buffer. */

            if (!mDoneFadingOut)
            {
                // check if the sound is fade out.
                if (!mJCSFadeSound.IsReachTargetVolue())
                    return;

                // get the background music audio source.
                AudioSource bgmAudioSource = GetBGMAudioSource();

                // set the audio source.
                mJCSFadeSound.SetAudioSource(bgmAudioSource);

                // set the bgm and play it
                bgmAudioSource.clip = this.mCurrentBGM;
                bgmAudioSource.Play();

                // active the fade sound in effect.
                mJCSFadeSound.FadeIn(
                    JCS_GameSettings.GetBGM_Volume(),
                    this.mRealSoundFadeOutTime);

                mDoneFadingOut = true;
            }
            else
            {
                // check if the sound is fade in.
                if (!mJCSFadeSound.IsReachTargetVolue())
                    return;

                // done switching the bgm.
                mSwitchingBGM = false;
            }
        }

        /// <summary>
        /// Check if there are audio clip on stack.
        /// If do, wait for the next BGM done playing, and 
        /// play the on stack bgm. Eventually clear the 
        /// stack. 
        /// 
        /// Attension: This only work for not looping BGM.
        /// </summary>
        private void DoStackBGM()
        {
            // nothing to do, the stacks is clean!.
            if (mOnStackAudioClip == null)
                return;

            // do not check if switching bgm.
            if (mSwitchingBGM)
                return;

            // do nothing if still playing
            if (GetBGMAudioSource().isPlaying)
                return;

            // switch bgm
            SwitchBackgroundMusic(
                this.mOnStackAudioClip, 
                this.mOnStackFadeInTime,
                this.mOnStackFadeOutTime);

            // clean stack
            mOnStackAudioClip = null;
        }

    }
}
