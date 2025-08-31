using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Color crosshairColor = Color.white;
    public float crosshairSize = 20f;
    public float crosshairThickness = 2f;
    public float crosshairGap = 5f;

    private RectTransform[] crosshairLines;

    private void Awake()
    {
        CreateCrosshair();
        gameObject.SetActive(false);
    }

    private void CreateCrosshair()
    {
        // LMJ: Create 4 lines for crosshair
        crosshairLines = new RectTransform[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject line = new GameObject($"CrosshairLine{i}");
            line.transform.SetParent(transform, false);

            Image img = line.AddComponent<Image>();
            img.color = crosshairColor;

            RectTransform rect = line.GetComponent<RectTransform>();
            crosshairLines[i] = rect;
        }

        UpdateCrosshairLayout();
    }

    private void UpdateCrosshairLayout()
    {
        // LMJ: Top line
        crosshairLines[0].anchoredPosition = new Vector2(0, crosshairGap + crosshairSize / 2);
        crosshairLines[0].sizeDelta = new Vector2(crosshairThickness, crosshairSize);

        // LMJ: Bottom line
        crosshairLines[1].anchoredPosition = new Vector2(0, -(crosshairGap + crosshairSize / 2));
        crosshairLines[1].sizeDelta = new Vector2(crosshairThickness, crosshairSize);

        // LMJ: Left line
        crosshairLines[2].anchoredPosition = new Vector2(-(crosshairGap + crosshairSize / 2), 0);
        crosshairLines[2].sizeDelta = new Vector2(crosshairSize, crosshairThickness);

        // LMJ: Right line
        crosshairLines[3].anchoredPosition = new Vector2(crosshairGap + crosshairSize / 2, 0);
        crosshairLines[3].sizeDelta = new Vector2(crosshairSize, crosshairThickness);
    }
}