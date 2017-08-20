﻿/**
 * $File: JCS_2DLiveObjectManager.cs $
 * $Date: $
 * $Revision: $
 * $Creator: Jen-Chieh Shen $
 * $Notice: See LICENSE.txt for modification and distribution information 
 *	                    Copyright (c) 2016 by Shen, Jen-Chieh $
 */
using UnityEngine;
using System.Collections;


namespace JCSUnity
{

    /// <summary>
    /// Handle all the 2d live object in the scene.
    /// </summary>
    public class JCS_2DLiveObjectManager
        : MonoBehaviour
    {

        //----------------------
        // Public Variables
        public static JCS_2DLiveObjectManager instance = null;


        [Header("** Runtime Variables (JCS_2DLiveObjectManager) **")]

        [Tooltip("All the live object in the scene.")]
        public JCS_2DLiveObject[] LIVE_OBJECT_LIST = null;

        [Tooltip("Time to find all the live object in the scene periodically.")]
        public float TIME_TO_FIND_ALL_LIVE_OBJECT_IN_SCENE = 3;

        //----------------------
        // Private Variables

        //----------------------
        // Protected Variables

        //========================================
        //      setter / getter
        //------------------------------

        //========================================
        //      Unity's function
        //------------------------------
        private void Awake()
        {
            instance = this;

            StartCoroutine(FindAllLiveObjectInScene(TIME_TO_FIND_ALL_LIVE_OBJECT_IN_SCENE));
        }

        //========================================
        //      Self-Define
        //------------------------------
        //----------------------
        // Public Functions

        /// <summary>
        /// Destroy all the live object in the scene.
        /// 全圖殺怪!
        /// </summary>
        public void DestroyAllLiveObject()
        {
            // Destroy all the live object in the scene.
            JCS_2DLiveObject[] jcsLiveObjects = Resources.FindObjectsOfTypeAll<JCS_2DLiveObject>();

            foreach (JCS_2DLiveObject lo in jcsLiveObjects)
            {
                // NOTE(JenChieh): kill the object that are clone!
                // or else it will effect the prefab object...
                if (lo.gameObject.name.Contains("(Clone)"))
                    lo.Die();
            }
        }

        //----------------------
        // Protected Functions

        //----------------------
        // Private Functions

        /// <summary>
        /// Time to find all live object in scene.
        /// </summary>
        /// <param name="time"> time to find. </param>
        /// <returns> wait. </returns>
        private IEnumerator FindAllLiveObjectInScene(float time)
        {
            while (true)
            {
                LIVE_OBJECT_LIST = Object.FindObjectsOfType<JCS_2DLiveObject>();
                yield return new WaitForSeconds(time);
            }
        }
    }
}
