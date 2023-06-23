using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MudBun;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Transform camPivot;

    private Bone bone;
    private MudSphere deformSphere;

    [Header("Editing")]
    public GameObject blobDestroyButton;
    public GameObject blobConfirmButton;
    public Image toggleBG;
    public Text blobsRemainingText;
    public Color deformColor;
    public Color destroyColor;

    [Header("Colour References")]
    public Color[] colorsToUse;
    public GameObject blobColourTemplate;
    public Transform blobColourParent;
    public GameObject blobColoursPanel;
    public GameObject blobSizesPanel;

    bool deform = true;
    int blobSize;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        for (int i = 0; i < colorsToUse.Length; i++)
        {
            GameObject colour = Instantiate(blobColourTemplate, blobColourParent);
            colour.transform.GetChild(0).GetComponent<Image>().color = colorsToUse[i];
            colour.SetActive(true);
        }
    }

    public void ToggleDeformDestroy()
    {
        deform = !deform;

        if(deform)
        {
            blobDestroyButton.SetActive(false);
            blobConfirmButton.SetActive(true);
            toggleBG.color = deformColor;
        }
        else
        {
            blobDestroyButton.SetActive(true);
            blobConfirmButton.SetActive(false);
            toggleBG.color = destroyColor;
        }
    }

    public void TriggerBlobColours(bool active)
    {
        blobSizesPanel.SetActive(!active);
        blobColoursPanel.SetActive(active);
    }

    public void SelectBlobSize(int size)
    {
        blobSize = size;

        foreach (Transform item in blobColourParent)
        {
            item.GetChild(0).localScale = Vector3.one * (blobSize * 0.3f);
        }
        TriggerBlobColours(true);
    }

    public void SelectColour(Image image)
    {
        TemplateController.instance.CreateDeformSphere(blobSize, image.color);
        TriggerBlobColours(false);
    }

    public bool InAddMode()
    {
        return deform;
    }

    public void SetSelectedBone(Bone bone) 
    {
        this.bone = bone;
    }

    public void SetBlend(float value)
    {
        bone.SetBlend(value, true);
    }

    public void SetDeformSphere(MudSphere sphere)
    {
        deformSphere = sphere;
    }

    public void SetRadius(float value)
    {
        bone.SetRadius(value, true);
    }

    public void SetDeformRadius(float value)
    {
        deformSphere.Radius = value;
    }

    public void SetDeformBlend(float value)
    {
        deformSphere.Blend = value;
    }

    public void SetCamPivot(float value)
    {
        var rot = camPivot.rotation;
        Vector3 euler = rot.eulerAngles;
        euler.y = value;
        rot.eulerAngles = euler;
        camPivot.rotation = rot;
    }

    public void SetBlobsRemaining(int blobs)
    {
        blobsRemainingText.text = "Blobs Remaining: " + blobs;
    }
}
