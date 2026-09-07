using UnityEngine;

public class cursorclicker : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Clicked: " + hit.collider.gameObject.name);

                objectclickable clickable = hit.collider.GetComponent<objectclickable>();
                if (clickable != null)
                {
                    clickable.Click();
                }
            }
        }
    }
}
