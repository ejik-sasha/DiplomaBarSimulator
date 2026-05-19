using UnityEngine;

public class IceReceiver : MonoBehaviour
{
    [SerializeField] private BartendingManager bartendingManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IceCube>() != null)
        {
            bartendingManager.ApplyAction(
                new CocktailAction(
                    CocktailActionType.AddIce,
                    "ice",
                    1f
                )
            );

            Destroy(other.gameObject);
        }
    }
}