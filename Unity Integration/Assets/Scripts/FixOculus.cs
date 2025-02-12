using UnityEngine;
using UnityEngine.XR.Management;
 
public class FixOculus : MonoBehaviour
{
    public XRLoader loader;
 
#if UNITY_EDITOR && UNITY_ANDROID
    private void Awake()
    {
        loader.Initialize();
        loader.Start();
        DontDestroyOnLoad(this.gameObject);
    }
 
    private void OnApplicationQuit()
    {
       loader.Stop();
       loader.Deinitialize();
    }
#endif
}