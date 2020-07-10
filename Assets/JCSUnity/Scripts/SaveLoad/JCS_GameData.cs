/**
 * $File: JCS_GameData.cs $
 * $Date: 2020-07-10 12:15:20 $
 * $Revision: $
 * $Creator: Jen-Chieh Shen $
 * $Notice: See LICENSE.txt for modification and distribution information
 *                   Copyright � 2020 by Shen, Jen-Chieh $
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JCSUnity
{
    /// <summary>
    /// Base game data structure.
    /// </summary>
    [System.Serializable]
    public abstract class JCS_GameData
    {
        /// <summary>
        /// Get complete save data path.
        /// </summary>
        public static string SavePath()
        {
            var gs = JCS_GameSettings.instance;
            return Application.dataPath + gs.DATA_PATH;
        }
    }
}
