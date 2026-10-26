using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompanyListItem : MonoBehaviour
{
    [HideInInspector] public int companyId;
    [HideInInspector] public string brandName;
    
    private Monitor2_Manager controller;
    private Button button;

    public void Setup(int id, string brand, Monitor2_Manager mainController)
    {
        companyId = id;
        brandName = brand;
        controller = mainController;

        // Set the text on the button
        TMP_Text btnText = GetComponentInChildren<TMP_Text>();
        if (btnText != null) 
            btnText.text = brand;

        // Wire the click event
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners(); // Prevent duplicate calls
        button.onClick.AddListener(() => controller.ShowCompanyDetails(companyId, brandName));
    }
}