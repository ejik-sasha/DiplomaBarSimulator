using UnityEngine;

public class IceCube : MonoBehaviour
{
    public bool IsRegistered { get; private set; }

    public void MarkRegistered()
    {
        IsRegistered = true;
    }
}