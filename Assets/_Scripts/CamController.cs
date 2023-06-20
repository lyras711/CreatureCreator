using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CamController : MonoBehaviour
{
    private Vector3 defPos = new Vector3();
    private Quaternion defRot = new Quaternion();

    public GameObject deformLayer1;
    public GameObject deformLayer2;

    bool deformlayer1 = true;

    private void Start()
    {
        defPos = transform.position;
        defRot = transform.rotation;
    }

    public void GoToBone(Transform boneTransform)
    {
        var dist = transform.position - boneTransform.position;
        Vector3 normalized = dist.normalized * 4;
 
        transform.DOMove(boneTransform.position + normalized, 1f);
        transform.parent.DORotate(Vector3.up * 180, 1f);
        transform.DOLookAt(boneTransform.position, 1f);
    }

    public void SetToEditMode()
    {
        Vector3 newRot = new Vector3(0, 180, 0);
        transform.parent.DORotate(newRot, 1f);
    }

    public void GoBackToTransform()
    {
        transform.DOMove(defPos, 1f);
        transform.DORotateQuaternion(defRot, 1f);
    }

    public void RotateCam()
    {
        float curRot = transform.parent.rotation.eulerAngles.y;
        Vector3 newRot = new Vector3(0, curRot + 90, 0);
        transform.parent.DORotate(newRot, 1f);

        deformlayer1 = !deformlayer1;

        if (deformlayer1)
        {
            deformLayer1.SetActive(true);
            deformLayer2.SetActive(false);
        }
        else
        {
            deformLayer1.SetActive(false);
            deformLayer2.SetActive(true);
        }
    }
}
