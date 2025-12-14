using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorManager : Singleton<SelectorManager>
{
    public List<GameObject> selectors = new();

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
        foreach (var selector in selectors.Where(selector => selector.name == "Shelf_[0,0]"))
        {
            if(selector != null)
                selector.GetComponent<ItemSelector>().SetSelectorVisible(true);
            Debug.Log(111);
        }
    }
}
