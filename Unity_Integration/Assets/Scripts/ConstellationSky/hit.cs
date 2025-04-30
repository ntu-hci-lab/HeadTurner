using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class hit : MonoBehaviour
{
    bool hitten = false;
    bool emgEnded = false;
    bool emgStarted = false;
    private string animationName, posture = "default";
    PauseAnimation pause;
    [Header("Data Pipeline Settings")]
    static EMGLogger_O emgLogger;
    bool usingEMG = false;
    string ResultFolder = @"Result S";

    void Start()
    {
        pause = GameObject.Find("ControlAnimation").GetComponent<PauseAnimation>();
        animationName = this.gameObject.name;
        if (usingEMG)
        {
            // get the EMGLogger_O component from the scene
            if (emgLogger == null)
            {
                string emg_folder = Path.Combine(ResultFolder, "emg_data");
                emgLogger = new EMGLogger_O(dirname: emg_folder);
                Debug.Log("EMGLogger_O created");
            }
        }
    }
    public void HitByRay()
    {
        if (hitten == false)
        {
            Time.timeScale = 1;

            StartCoroutine(ChangeTouch());
        }
        if (usingEMG)
        {
            if (!emgEnded)
            {
                if (animationName != "LittleBear_ui")
                {
                    Debug.Log(animationName + " end logging");
                    emgLogger.end_logging();
                }
                emgEnded = true;
            }
            if (!emgStarted)
            {
                if (animationName != "Hourse_ui" && !emgLogger.IsEndLogging)
                {
                    emgLogger.start_logging(animationName, posture);
                    Debug.Log(animationName + " start logging");
                    emgStarted = true;
                }
            }
        }
    }
    private IEnumerator ChangeTouch()
    {
        yield return new WaitForSeconds(4);
        hitten = true;
        pause.touch = false;
    }
    private void OnDestroy()
    {
        if (usingEMG)
        {
            emgLogger.close();
        }
    }
}
