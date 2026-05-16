using UnityEngine;

public class SpillSurface : MonoBehaviour
{
    [Header("Spill Settings")]
    [SerializeField] private GameObject spillPrefab;
    [SerializeField] private float spawnCooldown = 0.25f;
    [SerializeField] private float spillOffset = 0.01f;

    private float _lastSpawnTime;

    public void CreateSpill(Vector3 position, Vector3 normal, Color liquidColor)
    {
        if (spillPrefab == null)
            return;

        if (Time.time - _lastSpawnTime < spawnCooldown)
            return;

        _lastSpawnTime = Time.time;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        GameObject spill = Instantiate(
            spillPrefab,
            position + normal * spillOffset,
            rotation
        );

        spill.transform.SetParent(transform);

        SpillDecal decal = spill.GetComponent<SpillDecal>();
        if (decal != null)
            decal.SetColor(liquidColor);
    }
}