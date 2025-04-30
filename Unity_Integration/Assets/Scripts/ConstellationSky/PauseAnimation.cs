using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAnimation : MonoBehaviour
{
    //public Animator stars;
    public bool pause = false, touch = false,play=false;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(pause);
        if (pause && touch == false)
        {
            Time.timeScale = 0;
            touch = true;
            pause = false;

        }
        if(play){
            Time.timeScale = 1;
            play=false;

        }
    }
}
