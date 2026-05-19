using UnityEngine;

public class SpillDecal : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 4f;

    [Header("Size")]
    [SerializeField] private float startWidth = 0.04f;
    [SerializeField] private float startLength = 0.04f;
    [SerializeField] private float maxWidth = 0.22f;
    [SerializeField] private float maxLength = 0.18f;
    [SerializeField] private float growSpeed = 2f;

    [Header("Thickness")]
    [SerializeField] private float thickness = 0.01f;

    [Header("Color")]
    [SerializeField] private float alpha = 0.45f;

    private float _timer;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();

        ApplyScale(0f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        float t = Mathf.Clamp01(_timer * growSpeed);
        ApplyScale(t);

        if (_timer >= lifeTime)
            Destroy(gameObject);
    }

    public void SetColor(Color color)
    {
        if (_renderer == null)
            return;

        color.a = alpha;

        _renderer.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetColor("_BaseColor", color);
        _propertyBlock.SetColor("_Color", color);

        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void ApplyScale(float t)
    {
        float width = Mathf.Lerp(startWidth, maxWidth, t);
        float length = Mathf.Lerp(startLength, maxLength, t);

        transform.localScale = new Vector3(width, thickness, length);
    }
}