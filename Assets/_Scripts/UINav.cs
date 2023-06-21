using UnityEngine;

public class UINav : MonoBehaviour
{
    public GameObject baseTab;
    public GameObject meshTab;
    public GameObject paintTab;
    public GameObject addTab;
    public GameObject meshInfo;


    private void DisableAllTabs()
    {
        baseTab.SetActive(false);
        meshTab.SetActive(false);
        paintTab.SetActive(false);
        addTab.SetActive(false);
        meshInfo.SetActive(false);

        TemplateController.instance.TriggerNewBonesVisible(false);
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
        meshInfo.SetActive(true);
        TemplateController.instance.TriggerNewBonesVisible(true);
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

    public void SetBoneColour(Color color)
    {
        TemplateController.instance.SetBoneColour(color);
    }
}
