using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddressItem : MonoBehaviour
{
    public delegate void AddressItemClickedDelegate(AddressItem item);

    public string Text;
    public TextMeshProUGUI TMProUI;
    public Button Button;

    public int Index;
    // Start is called before the first frame update
    public event AddressItemClickedDelegate OnClicked;
    public void OnClick()
    {
        if(OnClicked!= null)
        {
            OnClicked(this);
        }
    }
}
