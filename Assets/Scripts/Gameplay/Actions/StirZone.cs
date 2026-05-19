using UnityEngine;

public class StirZone : MonoBehaviour
{
    [SerializeField] private BartendingManager bartendingManager;
    [SerializeField] private float stirSensitivity = 0.05f;

    private Vector3 _lastSpoonPosition;
    private bool _spoonInside;
    private float _stirAmount;

    private void OnTriggerEnter(Collider other)
    {
        SpoonTool spoon = other.GetComponentInParent<SpoonTool>();

        if (spoon != null)
        {
            _spoonInside = true;
            _lastSpoonPosition = spoon.transform.position;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        SpoonTool spoon = other.GetComponentInParent<SpoonTool>();

        if (!_spoonInside || spoon == null)
            return;

        float movement = Vector3.Distance(
            spoon.transform.position,
            _lastSpoonPosition
        );

        if (movement > stirSensitivity)
            _stirAmount += movement;

        _lastSpoonPosition = spoon.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        SpoonTool spoon = other.GetComponentInParent<SpoonTool>();

        if (spoon == null)
            return;

        _spoonInside = false;

        if (_stirAmount > 0)
        {
            bartendingManager.ApplyAction(
                new CocktailAction(
                    CocktailActionType.Stir,
                    "stir",
                    _stirAmount
                )
            );
        }

        _stirAmount = 0f;
    }
}