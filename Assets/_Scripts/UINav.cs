using MudBun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINav : MonoBehaviour
{
    public GameObject baseTab;
    public GameObject meshTab;
    public GameObject paintTab;
    public GameObject addTab;


    [Header("Colors")]
    public MudMaterial[] details1;
    public MudMaterial[] details2;

    private void DisableAllTabs()
    {
        baseTab.SetActive(false);
        meshTab.SetActive(false);
        paintTab.SetActive(false);
        addTab.SetActive(false);

        TemplateController.instance.TriggerBonesVisible(false);
        TemplateController.instance.TriggerColoursVisible(false);
        UIManager.instance.TriggerBoneColouringUI(true);
    }

    public void OpenBaseTab()
    {
        DisableAllTabs();
        baseTab.SetActive(true);
    }

    public void OpenMeshTab()
    {
        DisableAllTabs();
        meshTab.SetActive(true);
        //TemplateController.instance.TriggerBonesVisible(true);
    }

    public void OpenPaintTab()
    {
        DisableAllTabs();
        paintTab.SetActive(true);
        TemplateController.instance.TriggerColoursVisible(true);
    }

    public void OpenAddTab()
    {
        DisableAllTabs();
        addTab.SetActive(true);
    }

    public void SetMainBodyColour(Color color)
    {
        TemplateController.instance.SetBodyColour(color);
    }

    public void SetDetails1(Color color)
    {
        for (int i = 0; i < details1.Length; i++)
        {
            details1[i].Color = color;
        }
    }

    public void SetBoneColour(Color color)
    {
        TemplateController.instance.SetBoneColour(color);
    }

    public void SetDetails2(Color color)
    {
        for (int i = 0; i < details2.Length; i++)
        {
            details2[i].Color = color;
        }
    }
}
