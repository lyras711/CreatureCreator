using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOnActive : MonoBehaviour
{
    private bool isScaling;

    public float scaleSpeed = 7f;

    private void OnEnable()
    {
        isScaling = true;
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (isScaling)
        {
            if (transform.localScale == Vector3.one)
            {
                //Do actions when scaled
                isScaling = false;
            }
            else
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime * scaleSpeed);
            }
        }
    }
}
