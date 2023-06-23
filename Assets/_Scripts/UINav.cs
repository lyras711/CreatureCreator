using UnityEngine;

public class UINav : MonoBehaviour
{
    public GameObject meshTab;
    public GameObject meshInfo;


    private void DisableAllTabs()
    {
        meshTab.SetActive(false);
        meshInfo.SetActive(false);

        TemplateController.instance.TriggerNewBonesVisible(false);
    }

    public void OpenMeshTab()
    {
        DisableAllTabs();
        meshTab.SetActive(true);
        meshInfo.SetActive(true);
        TemplateController.instance.TriggerNewBonesVisible(true);
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
