using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    // Start is called before the first frame update

    LineRenderer line;

    void Start()
    {
        line = this.gameObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        line.SetPosition(0, ray.origin);
        if (Physics.Raycast(ray, out hit, 1000,1<<6))
        {
            line.SetPosition(1, hit.point);
            hit.transform.gameObject.GetComponent<hit>().HitByRay();
            //Debug.Log(hit.transform.gameObject.name);
        }
        else
        {
            line.SetPosition(1, ray.GetPoint(1000));
        }
    }
}
