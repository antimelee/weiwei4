using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTransitioner2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Color32 m_NormalColor = Color.white;
    public Color32 m_HoverColor = Color.grey;
    public Color32 m_DownColor = Color.red;

    private Image m_Image = null;
    private void Awake()
    {
        m_Image = GetComponent<Image>();

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //print("Enter");
        m_Image.color = m_HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //print("Exit");
        m_Image.color = m_NormalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //print("Down");
        m_Image.color = m_DownColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //print("up");
        m_Image.color = Color.green;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //print("Click");
        GameObject.Find("Controller (right)").GetComponent<Hand>().isteleporting = true;
        m_Image.color = Color.yellow;
    }
}
