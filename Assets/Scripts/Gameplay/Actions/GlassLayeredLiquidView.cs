using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassLayeredLiquidView : MonoBehaviour
{
    [System.Serializable]
    private class LiquidLayer
    {
        public string IngredientId;
        public float VolumeMl;
        public Color Color;
        public Transform Visual;
        public MaterialPropertyBlock PropertyBlock;
    }

    [Header("References")]
    [SerializeField] private Transform layersRoot;
    [SerializeField] private GameObject layerPrefab;

    [Header("Glass Visual Settings")]
    [SerializeField] private float maxVolumeMl = 250f;
    [SerializeField] private float maxLiquidHeight = 0.7f;
    [SerializeField] private float cylinderRadius = 0.65f;
    [SerializeField] private float minLayerHeight = 0.01f;

    [Header("Mixing")]
    [SerializeField] private float mixDuration = 1.2f;

    private readonly List<LiquidLayer> _layers = new();
    private Coroutine _mixCoroutine;

    public void AddLayer(string ingredientId, float amountMl, Color color)
    {
        if (string.IsNullOrWhiteSpace(ingredientId) || amountMl <= 0f)
            return;

        LiquidLayer lastLayer = GetLastLayer();

        if (lastLayer != null && lastLayer.IngredientId == ingredientId)
        {
            lastLayer.VolumeMl += amountMl;
            lastLayer.Color = Color.Lerp(lastLayer.Color, color, 0.15f);

            ApplyColor(lastLayer);
            RebuildLayerLayout();

            return;
        }

        LiquidLayer newLayer = new LiquidLayer
        {
            IngredientId = ingredientId,
            VolumeMl = amountMl,
            Color = color,
            Visual = CreateLayerVisual(),
            PropertyBlock = new MaterialPropertyBlock()
        };

        _layers.Add(newLayer);

        ApplyColor(newLayer);
        RebuildLayerLayout();
    }

    public void Clear()
    {
        if (_mixCoroutine != null)
        {
            StopCoroutine(_mixCoroutine);
            _mixCoroutine = null;
        }

        foreach (LiquidLayer layer in _layers)
        {
            if (layer.Visual != null)
                Destroy(layer.Visual.gameObject);
        }

        _layers.Clear();
    }

    public void MixAllLayers()
    {
        if (_layers.Count <= 1)
            return;

        if (_mixCoroutine != null)
            StopCoroutine(_mixCoroutine);

        _mixCoroutine = StartCoroutine(MixRoutine());
    }

    private IEnumerator MixRoutine()
    {
        float totalVolume = GetTotalVolume();
        Color mixedColor = GetWeightedMixedColor();

        List<Color> startColors = new();
        foreach (LiquidLayer layer in _layers)
            startColors.Add(layer.Color);

        float timer = 0f;

        while (timer < mixDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / mixDuration);

            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i].Color = Color.Lerp(startColors[i], mixedColor, t);
                ApplyColor(_layers[i]);
            }

            yield return null;
        }

        Clear();

        LiquidLayer mixedLayer = new LiquidLayer
        {
            IngredientId = "mixed",
            VolumeMl = totalVolume,
            Color = mixedColor,
            Visual = CreateLayerVisual(),
            PropertyBlock = new MaterialPropertyBlock()
        };

        _layers.Add(mixedLayer);

        ApplyColor(mixedLayer);
        RebuildLayerLayout();

        _mixCoroutine = null;
    }

    private Transform CreateLayerVisual()
    {
        if (layerPrefab == null)
        {
            Debug.LogError("GlassLayeredLiquidView: Layer prefab is not assigned.");
            return null;
        }

        GameObject layerObject = Instantiate(layerPrefab, layersRoot);
        layerObject.SetActive(true);

        return layerObject.transform;
    }

    private void RebuildLayerLayout()
    {
        float totalVolume = GetTotalVolume();

        if (totalVolume <= 0f)
            return;

        float currentY = 0f;

        foreach (LiquidLayer layer in _layers)
        {
            if (layer.Visual == null)
                continue;

            float normalizedLayerVolume = layer.VolumeMl / maxVolumeMl;
            float layerHeight = Mathf.Max(
                minLayerHeight,
                normalizedLayerVolume * maxLiquidHeight
            );

            layer.Visual.localPosition = new Vector3(
                0f,
                currentY + layerHeight * 0.5f,
                0f
            );

            layer.Visual.localScale = new Vector3(
                cylinderRadius,
                layerHeight,
                cylinderRadius
            );

            currentY += layerHeight;
        }
    }

    private void ApplyColor(LiquidLayer layer)
    {
        if (layer.Visual == null)
            return;

        Renderer renderer = layer.Visual.GetComponent<Renderer>();

        if (renderer == null)
            return;

        if (layer.PropertyBlock == null)
            layer.PropertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(layer.PropertyBlock);

        Color visibleColor = layer.Color;
        visibleColor.a = 0.65f;

        layer.PropertyBlock.SetColor("_BaseColor", visibleColor);
        layer.PropertyBlock.SetColor("_Color", visibleColor);

        renderer.SetPropertyBlock(layer.PropertyBlock);
    }

    private float GetTotalVolume()
    {
        float total = 0f;

        foreach (LiquidLayer layer in _layers)
            total += layer.VolumeMl;

        return total;
    }

    private Color GetWeightedMixedColor()
    {
        float totalVolume = GetTotalVolume();

        if (totalVolume <= 0f)
            return Color.clear;

        float r = 0f;
        float g = 0f;
        float b = 0f;
        float a = 0f;

        foreach (LiquidLayer layer in _layers)
        {
            float weight = layer.VolumeMl / totalVolume;

            r += layer.Color.r * weight;
            g += layer.Color.g * weight;
            b += layer.Color.b * weight;
            a += layer.Color.a * weight;
        }

        return new Color(r, g, b, a);
    }
    private LiquidLayer GetLastLayer()
    {
        if (_layers == null || _layers.Count == 0)
            return null;

        return _layers[_layers.Count - 1];
    }
    public void ShakeMixAllLayers()
    {
        if (_layers.Count == 0)
            return;

        if (_mixCoroutine != null)
            StopCoroutine(_mixCoroutine);

        float totalVolume = GetTotalVolume();
        Color mixedColor = GetWeightedMixedColor();

        mixedColor = MakeMuddyColor(mixedColor);

        Clear();

        LiquidLayer mixedLayer = new LiquidLayer
        {
            IngredientId = "shaken_mixed",
            VolumeMl = totalVolume,
            Color = mixedColor,
            Visual = CreateLayerVisual(),
            PropertyBlock = new MaterialPropertyBlock()
        };

        _layers.Add(mixedLayer);

        ApplyColor(mixedLayer);
        RebuildLayerLayout();
    }

    private Color MakeMuddyColor(Color color)
    {
        float gray = color.grayscale;

        Color muddy = Color.Lerp(
            color,
            new Color(gray, gray, gray, color.a),
            0.35f
        );

        muddy.r = Mathf.Clamp01(muddy.r + 0.08f);
        muddy.g = Mathf.Clamp01(muddy.g + 0.08f);
        muddy.b = Mathf.Clamp01(muddy.b + 0.08f);
        muddy.a = 0.8f;

        return muddy;
    }
}