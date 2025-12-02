using UnityEngine;

public class CustomScreen : MonoBehaviour
{
    public bool hide { get; private set; }

    virtual public void Show()
    {
        gameObject.SetActive(true);
        hide = false;
    }

    virtual public void Hide()
    {
        gameObject.SetActive(false);
        hide = true;
    }
}
