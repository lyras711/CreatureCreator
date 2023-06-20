using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBone : MonoBehaviour
{
    private Renderer editRef;
    public Material selectedMat;
    public Material defaultMat;

    private void Start()
    {
        editRef = transform.GetChild(0).GetComponent<Renderer>();
    }

    public void TriggerBoneSelection(bool active)
    {
        if (active)
        {
            editRef.material = selectedMat;
        }
        else
            editRef.material = defaultMat;
    }
}
