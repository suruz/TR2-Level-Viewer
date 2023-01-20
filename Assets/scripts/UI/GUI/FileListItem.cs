using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FileListItem : MonoBehaviour
{
    public enum ItemType
    {
        File = 0,
        Folder
    }
    
    
    public delegate void ItemClickDelegate(FileListItem item);

    public RawImage Icon;
    public TMPro.TextMeshProUGUI Tmpro;
    public string Text;
    public int Index;
    public event ItemClickDelegate OnItemClick;
    public ItemType Type;
    // Start is called before the first frame update 
    public void SetIcon(Texture2D icon)
    {
        Icon.texture = icon;
    }

    public void ItemClicked()
    {
        if(OnItemClick != null)
        {
            OnItemClick(this);
        }
    }
}
