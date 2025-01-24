#if UNITY_EDITOR
using DlibFaceLandmarkDetector.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace DlibFaceLandmarkDetector
{

    public class ResetDebugMode : MonoBehaviour
    {

        [InitializeOnEnterPlayMode]
        static void InitializeOnEnterPlayMode()
        {

            Debug.Log("InitializeOnEnterPlayMode()");

            Utils.setDebugMode(false);

        }
    }
}
#endif