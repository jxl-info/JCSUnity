﻿/**
 * $File: JCS_ItemPickable.cs $
 * $Date: $
 * $Revision: $
 * $Creator: Jen-Chieh Shen $
 * $Notice: See LICENSE.txt for modification and distribution information 
 *                   Copyright (c) 2016 by Shen, Jen-Chieh $
 */
using UnityEngine;
using System.Collections;

namespace JCSUnity
{
    public delegate void PickCallback(Collider other);

    /// <summary>
    /// Base class for all the item subclasses.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class JCS_Item
        : JCS_UnityObject
    {
        /* Variables */

        protected bool mCanPick = true;
        protected BoxCollider mBoxCollider = null;

        [Header("** Check Variables (JCS_Item) **")]

        [Tooltip(@"Collider detect can be picked.
You can set this directly in order to get the pick effect too. Once you set this, 
the object will do tween effect to this transform.")]
        [SerializeField]
        protected Collider mPickCollider = null;

        [Header("** Player Specific Settings (JCS_Item) **")]

        [Tooltip("Is the auto pick collider must be player?")]
        [SerializeField]
        protected bool mMustBeActivePlayer = true;

        [Tooltip("Key to active pick event.")]
        [SerializeField]
        protected KeyCode mPickKey = KeyCode.Z;

        [Header("** System Settings (JCS_Item) **")]

        [Tooltip("Pick item by click/mouse?")]
        [SerializeField]
        protected bool mPickByMouseDown = false;

        [Tooltip("When player hit this object pick it up automatically.")]
        [SerializeField]
        protected bool mAutoPickColliderTouched = false;

        [Tooltip(@"When the item are on the ground, pick it up when there is 
object that we target.")]
        [SerializeField]
        protected bool mAutoPickWhileCan = false;

        [Header("** Sound Settings (JCS_Item) **")]

        [Tooltip(@"Play one shot while not playing any other sound. (Pick Sound)")]
        [SerializeField]
        protected bool mPlayOneShotWhileNotPlayingForPickSound = true;

        [Tooltip("Sound played when you pick up this item.")]
        [SerializeField]
        protected AudioClip mPickSound = null;

        [Tooltip(@"Play one shot while not playing any other sound.")]
        [SerializeField]
        protected bool mPlayOneShotWhileNotPlayingForEffectSound = false;

        [Tooltip("Sound played when you pick up this item.")]
        [SerializeField]
        protected AudioClip mEffectSound = null;

        protected PickCallback mPickCallback = DefaultPickCallback;

        [Header("** Optional Variables (JCS_UnityObject) **")]

        [Tooltip("Make item tween to the destination.")]
        [SerializeField]
        private JCS_TransformTweener mTweener = null;

        [Tooltip("Destroy when reach the destination.")]
        [SerializeField]
        private JCS_DestinationDestroy mDestinationDestroy = null;

        /* Setter & Getter */

        public bool AutoPickColliderTouched { get { return this.mAutoPickColliderTouched; } set { this.mAutoPickColliderTouched = value; } }
        public bool PickByMouseDown { get { return this.mPickByMouseDown; } set { this.mPickByMouseDown = value; } }
        public bool AutoPickWhileCan { get { return this.mAutoPickWhileCan; } set { this.mAutoPickWhileCan = value; } }
        public bool CanPick { get { return this.mCanPick; } set { this.mCanPick = value; } }
        public BoxCollider GetBoxCollider() { return this.mBoxCollider; }
        public Collider PickCollider { get { return this.mPickCollider; } set { this.mPickCollider = value; } }

        public void SetPickCallback(PickCallback func) { this.mPickCallback = func; }
        public PickCallback GetPickCallback() { return this.mPickCallback; }

        /* Functions */

        protected override void Awake()
        {
            // Update the data once depends on what game object is.
            base.Awake();

            mBoxCollider = this.GetComponent<BoxCollider>();
        }

        protected virtual void Start()
        {
            if (mTweener == null)
                mTweener = this.GetComponent<JCS_TransformTweener>();
            if (mDestinationDestroy == null)
                mDestinationDestroy = this.GetComponent<JCS_DestinationDestroy>();
        }

        protected virtual void Update()
        {
            DoAutoPickWhileCan();
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (mAutoPickColliderTouched)
            {
                // picked
                Pick(other);
                return;
            }

            if (JCS_Input.GetKey(mPickKey))
            {
                Pick(other);
            }
        }

        protected virtual void OnMouseDown()
        {
            if (!mPickByMouseDown)
                return;

            if (mPickCollider == null)
            {
                JCS_Debug.LogError("Cannot pick the item cuz there is no collider set");
                return;
            }

            Pick(mPickCollider);
        }

        /// <summary>
        /// Pick the item up.
        /// </summary>
        /// <param name="other"></param>
        public void Pick(Collider other)
        {
            if (!mCanPick)
                return;

            JCS_OneJump joj = this.GetComponent<JCS_OneJump>();
            if (joj != null)
            {
                // Only when item is on the ground!
                if (joj.GetVelocity().y != 0)
                    return;
            }

            JCS_Player p = other.GetComponent<JCS_Player>();

            if (mAutoPickColliderTouched && p != null)
            {
                DoPick(other);
                return;
            }

            if (mMustBeActivePlayer)
            {
                // Check the colliding object are is active player.
                if (JCS_PlayerManager.instance.IsActivePlayerTransform(other.transform))
                {
                    DoPick(other);
                }
            }
            else
            {
                DoPick(other);
            }
        }

        /// <summary>
        /// Default pick up callback.
        /// </summary>
        /// <param name="other"></param>
        public static void DefaultPickCallback(Collider other)
        {
            // do anything after the character pick this item up.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void DoPick(Collider other)
        {
            DropEffect(other);

            JCS_SoundPlayer sp = JCS_SoundManager.instance.GetGlobalSoundPlayer();

            /* Play Pick Sound */
            if (mPlayOneShotWhileNotPlayingForPickSound)
                sp.PlayOneShotWhileNotPlaying(mPickSound);
            else
                sp.PlayOneShot(mPickSound);

            // call item effect.
            mPickCallback.Invoke(other);

            /* Play Effect Sound */
            if (mPlayOneShotWhileNotPlayingForEffectSound)
                sp.PlayOneShotWhileNotPlaying(mEffectSound);
            else
                sp.PlayOneShot(mEffectSound);

            mCanPick = false;
        }

        /// <summary>
        /// Do the drop effect.
        /// </summary>
        /// <param name="other"></param>
        private void DropEffect(Collider other)
        {
            // Throw Action Effect...
            {
                //JCS_ThrowAction ta = this.gameObject.AddComponent<JCS_ThrowAction>();
                //ta.SetTargetTransform(other.transform);
                //ta.ActiveEffect();
            }

            // Tweener Effect...
            {
                if (mTweener == null)
                {
                    // default settings
                    mTweener = this.gameObject.AddComponent<JCS_TransformTweener>();

                    mTweener.EasingY = JCS_TweenType.EASE_OUT_BACK;
                    mTweener.DurationX = 2.0f;
                    mTweener.DurationY = 5.0f;
                    mTweener.DurationZ = 0;
                    mTweener.StopTweenDistance = 0.2f;
                }
                mTweener.DoTweenContinue(other.transform);
            }


            if (mDestinationDestroy == null)
            {
                // default settings
                mDestinationDestroy = this.gameObject.AddComponent<JCS_DestinationDestroy>();
                mDestinationDestroy.DestroyDistance = 0.5f;
            }
            mDestinationDestroy.SetTargetTransform(other.transform);
        }

        /// <summary>
        /// If the Pick Collider is not null, 
        /// then pick it up immediatly while we can pick.
        /// </summary>
        private void DoAutoPickWhileCan()
        {
            if (!mAutoPickWhileCan)
                return;

            if (mPickCollider == null)
                return;

            Pick(mPickCollider);
        }
    }
}
