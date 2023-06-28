using UnityEngine;

public class UINav : MonoBehaviour
{
    public GameObject meshTab;
    public GameObject meshInfo;
    public GameObject sizesTab;
    public GameObject coloursTab;
    public GameObject selectedColourTab;

    public TemplateController templateController;

    private void DisableAllTabs()
    {
        meshTab.SetActive(false);
        meshInfo.SetActive(false);

        templateController.TriggerNewBonesVisible(false);
    }

    public void CompleteBuild()
    {
        meshTab.SetActive(false);
        gameObject.SetActive(false);
    }

    public void OpenPaintTab()
    {
        DisableAllTabs();
        meshTab.SetActive(true);
        UIManager.instance.TriggerBlobColours(true);
        templateController.TriggerColouring(true);
    }

    public void OpenMeshTab()
    {
        coloursTab.SetActive(false);
        selectedColourTab.SetActive(false);
        sizesTab.SetActive(true);
        DisableAllTabs();
        meshTab.SetActive(true);
        meshInfo.SetActive(true);
        templateController.TriggerNewBonesVisible(true);
        templateController.TriggerColouring(false);
    }

    public void SetMainBodyColour(Color color)
    {
        templateController.SetBodyColour(color);
    }
}
