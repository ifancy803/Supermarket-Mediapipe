using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorManager : Singleton<SelectorManager>
{
    public List<GameObject> selectors = new();
    public GameObject currentShelf = null;
    
    public void UpdateSelectors()
    {
        selectors.Clear();
        foreach (var selector in GameObject.FindGameObjectsWithTag("Stuff"))
        {
            if(selector!=null)
                selectors.Add(selector.gameObject);
        }
    }
    public void OnNewGameState()
    {
        GameObject.Find("Shelf_[0,0]").GetComponent<ItemSelector>().SetSelectorVisible(true);
    }
}
