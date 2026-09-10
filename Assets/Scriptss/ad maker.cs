using UnityEngine;

public class admaker : MonoBehaviour
{
    public GameObject adPrefab;
    public Transform screen;
    public void makeAd()
    {
        GameObject ad = Instantiate(adPrefab, screen);

        RectTransform adRect = ad.GetComponent<RectTransform>();

        float x = Random.Range(-500f, 500f);
        float y = Random.Range(-250f, 250f);

        adRect.anchoredPosition = new Vector2(x, y);
    }
}
