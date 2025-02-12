using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hit : MonoBehaviour
{
    bool hitten=false;
    PauseAnimation pause;
    void Start()
    {
        pause = GameObject.Find("ControlAnimation").GetComponent<PauseAnimation>();
    }
    public void HitByRay()
    {
        if (hitten==false)
        {
            Time.timeScale = 1;
            hitten = true;
            StartCoroutine(ChangeTouch());
            //pause.touch =false;
        }
         
    }
    private IEnumerator ChangeTouch()
    {       
        yield return new WaitForSeconds(2);
        pause.touch = false;
    }

}
