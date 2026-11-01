using TMPro;
using UnityEngine;

public class HoverPanelManager : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject smallPanelPrefabTrans;

    // Set in code
    private Transform smallPanel;
    private Vector2 vector;     // Used for mouse position

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //vector = Vector2.zero;
        //smallPanel = Instantiate(smallPanelPrefabTrans, canvas.transform).transform;
        //smallPanel.GetChild(0).GetComponent<TMP_Text>().text = "Info Panel Text";
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 mousePos = MousePositionInCanvas();
        //smallPanel.position = -mousePos;
        //Debug.Log(smallPanel.position);
    }

    Vector2 MousePositionInCanvas()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out vector);
        return vector;
    }
}
