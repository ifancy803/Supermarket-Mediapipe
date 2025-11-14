using UnityEngine;

[CreateAssetMenu(fileName = "StuffData", menuName = "Stuff")]
public class StuffData : ScriptableObject
{
    public StuffType stuffType;
    public GameObject stuffModel;
    public object stuffSpecies;
}