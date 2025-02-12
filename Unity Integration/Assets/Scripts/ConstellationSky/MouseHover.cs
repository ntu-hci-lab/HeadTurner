using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHover : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnMouseOver()
    {
        Debug.Log("The number is :" + this.name);
    }
}
