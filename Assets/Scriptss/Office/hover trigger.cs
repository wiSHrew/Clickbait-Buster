using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Slot { LookDown, LookUp }

    [Tooltip("Which slot this button controls on the camera script.")]
    [SerializeField] private Slot slot = Slot.LookDown;

    [Tooltip("The camera script that owns these buttons. If left empty, found automatically.")]
    [SerializeField] private camera cameraScript;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();

        if (cameraScript == null)
            cameraScript = FindAnyObjectByType<camera>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cameraScript == null) return;
        if (!button.interactable) return;

        if (slot == Slot.LookDown) cameraScript.SetHoverDown(true);
        else                       cameraScript.SetHoverUp(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cameraScript == null) return;

        if (slot == Slot.LookDown) cameraScript.SetHoverDown(false);
        else                       cameraScript.SetHoverUp(false);
    }
}