/**
 * $File: JCS_2DAnimator.cs $
 * $Date: 2017-03-13 $
 * $Revision: $
 * $Creator: Jen-Chieh Shen $
 * $Notice: See LICENSE.txt for modification and distribution information 
 *	                 Copyright (c) 2017 by Shen, Jen-Chieh $
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JCSUnity
{

    /// <summary>
    /// Hold sequence of animation and play it by condition.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class JCS_2DAnimator 
        : JCS_UnityObject
    {

        //----------------------
        // Public Variables

        //----------------------
        // Private Variables


#if (UNITY_EDITOR)
        [Header("** Helper Variables Variables (JCS_I2DAnimator) **")]

        [SerializeField]
        private bool mTestWithKey = true;
#endif


        [Header("** Check Variables Variables (JCS_I2DAnimator) **")]

        [Tooltip("Current animation id.")]
        [SerializeField]
        private int mCurrentAnimId = 0;

        [Tooltip("Current playing animation.")]
        [SerializeField]
        private JCS_2DAnimation mCurrentAnimation = null;

        [Tooltip("Maxinum frame in the animation.")]
        [SerializeField]
        private int mMaxAnimCount = 0;

        // Current animation stack.
        private JCS_2DAnimation mStackAnim = null;
        private int mStackAnimId = -1;

        private JCS_2DAnimation mOneShotAnim = null;


        [Header("** Runtime Variables Variables (JCS_I2DAnimator) **")]

        [Tooltip(@"This will times the animation time by this, 
default is 1.")]
        [SerializeField]
        [Range(0, 5)]
        private float mAnimationTimeProduction = 1;

        [Tooltip("All the animation possible to this object.")]
        [SerializeField]
        private JCS_2DAnimation[] mAnimations = null;

        //----------------------
        // Protected Variables

        //========================================
        //      setter / getter
        //------------------------------
        public float AnimationTimeProduction { get { return this.mAnimationTimeProduction; } }
        public int CurrentAnimId { get { return this.mCurrentAnimId; } }
        public JCS_2DAnimation CurrentAnimation { get { return this.mCurrentAnimation; } }

        //========================================
        //      Unity's function
        //------------------------------
        private void Awake()
        {
            UpdateUnityData();

            // update the max animation count.
            UpdateMaxAnimCount();

            // active this animation.
            ActiveOneAnimation(0);
        }

        private void Start()
        {
            // override the component in the animation array.
            foreach (JCS_2DAnimation anim in mAnimations)
            {
                // let the animation know there are animator controlling 
                // the animation.
                anim.SetJCS2DAnimator(this);

                // let all animation know there are sprite renderer 
                // take over what they had previous 'SpriteRenderer' 
                // component.
                if (anim.GetObjectType() == this.GetObjectType())
                    anim.LocalType = this.LocalType;
                else
                {
                    JCS_Debug.JcsErrors(
                        this, 
                        "Animator and Animation have different component type is not allowed...");
                }

                anim.PlayOnAwake = false;
            }
        }

        private void Update()
        {
#if (UNITY_EDITOR)
            Test();
#endif
            DoPlayOneShot();
        }

#if (UNITY_EDITOR)
        private void Test()
        {
            if (!mTestWithKey)
                return;

            if (Input.GetKey(KeyCode.A))
                DoAnimation(0, true);

            if (Input.GetKey(KeyCode.S))
                DoAnimation(1, false);

            if (Input.GetKey(KeyCode.D))
                DoAnimation(2, true);

            if (Input.GetKey(KeyCode.G))
                PlayOneShot(0);
        }
#endif

        //========================================
        //      Self-Define
        //------------------------------
        //----------------------
        // Public Functions

        /// <summary>
        /// Play the animation base on the animation ID.
        /// </summary>
        /// <param name="id"> animation index in array. </param>
        /// <param name="over"> override the animation? </param>
        /// <param name="oneShot"> Is this animation playing one shot? </param>
        public void DoAnimation(
            int id, 
            bool over = false, 
            bool oneShot = false)
        {
            if (!over)
            {
                // if same id, return.
                if (mCurrentAnimId == id)
                    return;
            }

            mCurrentAnimId = id;

            PutAnimaIdInSequence();

            // get the current playing animation.
            mCurrentAnimation = mAnimations[mCurrentAnimId];

            if (mCurrentAnimation == null)
            {
                JCS_Debug.JcsErrors(
                    this, "Swtich animation failed cuz of null reference animation assigned...");
                return;
            }

            if (oneShot)
            {
                // restart the target animation.
                mCurrentAnimation.Play(0, true);
            }
            else
            {
                // play target animation.
                mCurrentAnimation.Play(-1, true);
            }

            // active this animation.
            ActiveOneAnimation(id);
        }

        /// <summary>
        /// Play one animation once and go back to 
        /// original animation.
        /// </summary>
        /// <param name="id"> animation index in array. </param>
        /// <param name="over"> override the play one shot action? </param>
        public void PlayOneShot(int id, bool over = false)
        {
            // 如果要蓋過, 就不檢查了.
            if (!over)
            {
                // check if play one shot still do the stuff.
                if (mStackAnim != null)
                    return;
            }

            // push the animation to stack.
            mStackAnim = mCurrentAnimation;
            mStackAnimId = mCurrentAnimId;

            /**
             * Record down the animation, in order to check if there is 
             * an animation changes during the one shot function.
             */
            mOneShotAnim = mAnimations[id];

            // play the newer animation.
            DoAnimation(id, over, true);
        }

        /// <summary>
        /// Check if the animation in the same id.
        /// </summary>
        /// <param name="inAnimId"> id to check </param>
        /// <returns> 
        /// true : the same animation.
        /// false : vice versa.
        /// </returns>
        public bool IsInState(int inAnimId)
        {
            return (inAnimId == mCurrentAnimId);
        }

        /// <summary>
        /// play the animation in current frame.
        /// </summary>
        public void PlayAnimationInFrame()
        {
            if (mCurrentAnimation == null)
                return;

            mCurrentAnimation.Play();
        }

        /// <summary>
        /// Stop animation in current frame.
        /// </summary>
        public void StopAnimationInFrame()
        {
            if (mCurrentAnimation == null)
                return;

            mCurrentAnimation.Pause();
        }

        //----------------------
        // Protected Functions

        //----------------------
        // Private Functions

        /// <summary>
        /// Update the maxinum frame count from the 
        /// anim frame sequence.
        /// </summary>
        public void UpdateMaxAnimCount()
        {
            this.mMaxAnimCount = this.mAnimations.Length;
        }

        /// <summary>
        /// Make sure the animation we are playing 
        /// are not null reference.
        /// </summary>
        private void PutAnimaIdInSequence()
        {
            if (mCurrentAnimId < 0)
                this.mCurrentAnimId = 0;
            else if (mCurrentAnimId >= mMaxAnimCount)
                this.mCurrentAnimId = mMaxAnimCount;
        }

        /// <summary>
        /// Active on animation by ID.
        /// </summary>
        /// <param name="id"> animation index in array. </param>
        private void ActiveOneAnimation(int id)
        {
            foreach (JCS_2DAnimation anim in mAnimations)
            {
                // disable all
                anim.Active = false;
            }

            // only active playing target animation.
            mAnimations[id].Active = true;
        }

        /// <summary>
        /// Check if play one shot valid.
        /// If valid, do the play one shot algorithm.
        /// </summary>
        private void DoPlayOneShot()
        {
            /**
             * Check stack empty, if not empty meaning
             * there are PlayOneShot() animation going on.
             * 
             * ATTENTION(jenchieh): make sure to clean up the 
             * stack after the animatin is played.
             */
            if (mStackAnim == null)
                return;


            // clean up the stack after the animation is 
            // done playing.

            /* During the one shot animation, if there are animation change  */
            if (mOneShotAnim != mCurrentAnimation || 
                /* Of the animation done playing. */
                mCurrentAnimation.DonePlaying)
            {
                // switch the animation back to original 
                // animation by stack id.
                DoAnimation(mStackAnimId);

                // clean up the stack pointer.
                mStackAnim = null;
                mOneShotAnim = null;

                // clean up stack animation id.
                mStackAnimId = -1;
            }
        }
    }
}
