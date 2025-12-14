using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorManager : Singleton<SelectorManager>
{
    public List<GameObject> selectors = new List<GameObject>();

    public void UpdateSelectors()
    {
        selectors?.Clear();
        foreach (var selector in GameObject.FindGameObjectsWithTag("Stuff"))
        {
            selectors.Add(selector.gameObject);
        }
    }

    public void OnNewGameState()
    {
        foreach (var selector in selectors.Where(selector => selector.name == "0,0"))
        {
            selector.GetComponent<ItemSelector>().SetSelectorVisible(true);
        }
    }
}
