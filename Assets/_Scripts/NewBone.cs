using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudBun;

public class NewBone : MonoBehaviour
{
    public MudMaterial mudMat;
    public Renderer editRef;
    public Material selectedMat;
    public Material defaultMat;

    public int size = 1;
    public bool canPlace = false;

    public GameObject mirrorBone;

    public void SetMirrorBone(GameObject mirrorBone)
    {
        this.mirrorBone = mirrorBone;
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

    public void SetBoneColour(Color color)
    {
        mudMat.Color = color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Bone>() || other.GetComponent<NewBone>())
        {
            canPlace = true;
        }
    }
}
