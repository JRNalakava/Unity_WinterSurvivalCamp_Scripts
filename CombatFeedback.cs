using System.Collections;
using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private ParticleSystem hitParticlePrefab;
    [SerializeField] private Transform visualModel; // Assign in inspector, or defaults to transform

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Wobble Settings")]
    [SerializeField] private Vector3 punchScaleAmount = new Vector3(1.2f, 0.8f, 1.2f);
    [SerializeField] private float punchDuration = 0.2f;

    private Coroutine _flashCoroutine;
    private Coroutine _wobbleCoroutine;
    private Vector3 _originalScale;
    
    // Optimization: Recycle the PropertyBlock
    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        if (visualModel == null) visualModel = transform;
        _originalScale = visualModel.localScale;
        
        _propBlock = new MaterialPropertyBlock();
    }

    public void OnHit()
    {
        // 1. Flash
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());

        // 2. Wobble
        if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
        _wobbleCoroutine = StartCoroutine(WobbleRoutine());

        // 3. Particles
        if (hitParticlePrefab != null)
        {
            ParticleSystem ps = Instantiate(hitParticlePrefab, transform.position + Vector3.up, Quaternion.identity); // Offset slightly up
            Destroy(ps.gameObject, 2f);
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (renderers == null) yield break;

        // 1. Apply Flash
        _propBlock.Clear(); // Clean start
        _propBlock.SetColor("_Color", flashColor);     // Standard
        _propBlock.SetColor("_BaseColor", flashColor); // URP

        foreach (var r in renderers)
        {
            if (r != null)
            {
                r.SetPropertyBlock(_propBlock);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // 2. Revert (Clear Property Block)
        foreach (var r in renderers)
        {
            if (r != null)
            {
                r.SetPropertyBlock(null);
            }
        }
    }

    private IEnumerator WobbleRoutine()
    {
        float timer = 0f;
        
        while (timer < punchDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / punchDuration;
            
            // Simple ping-pong: 0 -> 1 -> 0
            float curve = Mathf.Sin(progress * Mathf.PI); // 0 at start, 1 at mid, 0 at end

            visualModel.localScale = Vector3.Lerp(_originalScale, Vector3.Scale(_originalScale, punchScaleAmount), curve);

            yield return null;
        }

        visualModel.localScale = _originalScale;
    }
}
