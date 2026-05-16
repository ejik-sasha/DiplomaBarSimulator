using UnityEngine;

public class PourableBottle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HoldableObject holdableObject;
    [SerializeField] private Transform pourPoint;
    [SerializeField] private ParticleSystem pourParticles;

    [Header("Pour Data")]
    [SerializeField] private string ingredientId = "rum_white";
    [SerializeField] private float pourSpeedMlPerSecond = 25f;
    [SerializeField] private float pourTickInterval = 0.05f;

    [Header("Pour Detection")]
    [SerializeField] private float pourDistance = 1.7f;
    [SerializeField] private float pourRadius = 0.06f;
    [SerializeField] private LayerMask glassLayer;
    [SerializeField] private LayerMask spillLayer;

    [Header("Bottle Rotation")]
    [SerializeField] private Vector3 pourLocalEulerAngles = new Vector3(110f, 0f, 0f);
    [SerializeField] private float rotateSpeed = 8f;

    [Header("Debug")]
    [SerializeField] private bool drawPourDebug = true;

    [Header("Liquid Visual")]
    [SerializeField] private Color liquidColor = Color.white;

    public Color LiquidColor => liquidColor;

    private MaterialPropertyBlock _particlePropertyBlock;

    private Quaternion _defaultLocalRotation;
    private float _tickTimer;
    private float _sessionPouredAmount;
    private bool _isPouring;

    private void Awake()
    {
        if (holdableObject == null)
            holdableObject = GetComponent<HoldableObject>();

        ApplyParticleColor();

        if (pourParticles != null)
            pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Start()
    {
        _defaultLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (holdableObject == null)
            return;

        if (!holdableObject.IsHeld)
        {
            StopPouring();

            return;
        }

        if (Input.GetMouseButton(1))
        {
            StartPouring();
        }
        else
        {
            StopPouring();
            ReturnBottle();
        }
    }

    private void StartPouring()
    {
        if (!_isPouring)
        {
            _isPouring = true;
            _sessionPouredAmount = 0f;

            PlayParticles();
        }
        AudioManager.Instance?.PlayPour();

        TiltBottle();
        ProcessPour();
    }

    private void StopPouring()
    {
        if (!_isPouring)
            return;

        _isPouring = false;
        _tickTimer = 0f;

        StopParticles();

        Debug.Log($"Poured session: {_sessionPouredAmount:F1} ml of {ingredientId}");

        _sessionPouredAmount = 0f;
    }

    private void TiltBottle()
    {
        Quaternion targetRotation = Quaternion.Euler(pourLocalEulerAngles);

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    private void ReturnBottle()
    {
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            _defaultLocalRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    private void ProcessPour()
    {
        if (pourPoint == null)
            return;

        _tickTimer += Time.deltaTime;

        if (_tickTimer < pourTickInterval)
            return;

        _tickTimer = 0f;

        float amount = pourSpeedMlPerSecond * pourTickInterval;

        Vector3 origin = pourPoint.position;
        Vector3 direction = Vector3.down;

        bool hitGlass = Physics.Raycast(
            origin,
            direction,
            out RaycastHit glassHit,
            pourDistance,
            glassLayer,
            QueryTriggerInteraction.Collide
        );

        if (hitGlass)
        {
            GlassPourReceiver glassReceiver =
                glassHit.collider.GetComponentInParent<GlassPourReceiver>();

            if (glassReceiver != null)
            {
                glassReceiver.ReceivePour(ingredientId, amount, liquidColor);
                _sessionPouredAmount += amount;

                if (drawPourDebug)
                    Debug.DrawRay(origin, direction * pourDistance, Color.green, 0.1f);

                return;
            }
        }

        bool hitSpill = Physics.Raycast(
            origin,
            direction,
            out RaycastHit spillHit,
            pourDistance,
            spillLayer,
            QueryTriggerInteraction.Collide
        );

        if (hitSpill)
        {
            SpillSurface spillSurface =
                spillHit.collider.GetComponentInParent<SpillSurface>();

            if (spillSurface != null)
            {
                spillSurface.CreateSpill(spillHit.point, spillHit.normal, liquidColor);
                _sessionPouredAmount += amount;

                if (drawPourDebug)
                    Debug.DrawRay(origin, direction * pourDistance, Color.yellow, 0.1f);

                return;
            }
        }

        if (drawPourDebug)
            Debug.DrawRay(origin, direction * pourDistance, Color.red, 0.1f);
    }

    private void PlayParticles()
    {
        if (pourParticles == null)
            return;

        if (!pourParticles.isPlaying)
            pourParticles.Play();
    }

    private void StopParticles()
    {
        if (pourParticles == null)
            return;

        if (pourParticles.isPlaying)
            pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void ApplyParticleColor()
    {
        if (pourParticles == null)
            return;

        ParticleSystem.MainModule main = pourParticles.main;
        main.startColor = liquidColor;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
            pourParticles.colorOverLifetime;

        if (colorOverLifetime.enabled)
        {
            Gradient gradient = new Gradient();

            Color startColor = liquidColor;
            startColor.a = 0.85f;

            Color endColor = liquidColor;
            endColor.a = 0.25f;

            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(endColor, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(startColor.a, 0f),
                    new GradientAlphaKey(endColor.a, 1f)
                }
            );

            colorOverLifetime.color = gradient;
        }

        ParticleSystemRenderer particleRenderer =
            pourParticles.GetComponent<ParticleSystemRenderer>();

        if (particleRenderer == null)
            return;

        if (_particlePropertyBlock == null)
            _particlePropertyBlock = new MaterialPropertyBlock();

        particleRenderer.GetPropertyBlock(_particlePropertyBlock);

        Color visibleColor = liquidColor;
        visibleColor.a = 0.75f;

        _particlePropertyBlock.SetColor("_BaseColor", visibleColor);
        _particlePropertyBlock.SetColor("_Color", visibleColor);

        particleRenderer.SetPropertyBlock(_particlePropertyBlock);
    }

    private void OnValidate()
    {
        ApplyParticleColor();
    }
}