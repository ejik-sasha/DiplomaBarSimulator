using UnityEngine;

public class Garnish : MonoBehaviour
{
    [SerializeField] private string garnishId = "lime";

    public string GarnishId => garnishId;
}