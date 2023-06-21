using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneSetter : MonoBehaviour
{
    public Transform parentToSet;

    public Transform mirrorParent;

    public void SetParent()
    {
        transform.SetParent(parentToSet);
    }
}
