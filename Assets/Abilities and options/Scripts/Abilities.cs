using MainScene;
using System.Collections;
using UnityEngine;

public class AbilityEffect : MonoBehaviour
{
    public enum AbilityType
    {
        SpeedBoosterWall,
        InverserWall,
        StickyWall,
        SoftGround,
        BounceGround,
        MagneticPoint
    }

    [Header("General")]
    [SerializeField] private AbilityType abilityType = AbilityType.SpeedBoosterWall;
    [SerializeField] private PuckScript puck; // reference to the puck (set in inspector)
    [SerializeField] private bool activateOnStart = true; // if true, Activate() will be called in Start()

    [Header("Sticky / Soft / Bounce settings")]
    [SerializeField] private float stickTime = 1f;
    [SerializeField] private float softZoneTime = 1f;
    [SerializeField] private float bounceZoneTime = 1f;

    [Header("Speed booster")]
    [SerializeField] private float speedMultiplier = 2f;
    // note: if you want temporary booster, consider using a duration and restoring original maxSpeed.

    [Header("Magnet settings")]
    [SerializeField] private Vector2 selectedPoint = Vector2.zero;
    [SerializeField] private float magnetRadius = 2f;
    [SerializeField] private float magneticForce = 10f;
    [SerializeField] private float magnetDuration = 2f;

    // internal
    private Rigidbody2D _rb;
    private bool _isActivated;
    private float _magnetStartTime;
    private Vector2 _storedVelocity; // for restoring after sticky/soft/bounce
    private float _originalMaxSpeed;

    private void Start()
    {
        if (puck == null)
        {
            Debug.LogError($"{nameof(AbilityEffect)} on '{gameObject.name}': puck reference is null.");
            Destroy(gameObject);
            return;
        }

        _rb = puck.GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            Debug.LogError($"{nameof(AbilityEffect)}: puck has no Rigidbody2D.");
            Destroy(gameObject);
            return;
        }

        // store original max speed if we need to modify it
        _originalMaxSpeed = puck.maxSpeed;

        if (activateOnStart)
            Activate();
    }

    /// <summary>
    /// Call this to trigger the ability effect (if not activated already).
    /// </summary>
    public void Activate()
    {
        if (_isActivated) return;
        _isActivated = true;

        switch (abilityType)
        {
            case AbilityType.SpeedBoosterWall:
                ApplySpeedBooster();
                break;
            case AbilityType.InverserWall:
                ApplyInverser();
                break;
            case AbilityType.StickyWall:
                StartCoroutine(StickyWallRoutine());
                break;
            case AbilityType.SoftGround:
                StartCoroutine(SoftGroundRoutine());
                break;
            case AbilityType.BounceGround:
                StartCoroutine(BounceGroundRoutine());
                break;
            case AbilityType.MagneticPoint:
                StartMagnetic();
                break;
            default:
                Debug.LogWarning("Unknown ability type.");
                Destroy(gameObject);
                break;
        }
    }

    #region Ability Implementations

    private void ApplySpeedBooster()
    {
        // This permanently modifies puck.maxSpeed. If you want a temporary boost,
        // use a coroutine and restore _originalMaxSpeed later.
        puck.maxSpeed = puck.maxSpeed * speedMultiplier;
        Destroy(gameObject);
    }

    private void ApplyInverser()
    {
        // Reverse current velocity
        _rb.velocity = -_rb.velocity;
        Destroy(gameObject);
    }

    private IEnumerator StickyWallRoutine()
    {
        // Only apply if puck is moving
        _storedVelocity = _rb.velocity;

        if (_storedVelocity.sqrMagnitude > 0.0001f)
        {
            _rb.velocity = Vector2.zero;
        }
        // Wait for stick time (use unscaled or scaled depending on your design)
        yield return new WaitForSeconds(stickTime);

        // restore previous velocity (if any)
        _rb.velocity = _storedVelocity;
        Destroy(gameObject);
    }

    private IEnumerator SoftGroundRoutine()
    {
        // scale down velocity, but store original to restore reliably
        _storedVelocity = _rb.velocity;
        _rb.velocity = _rb.velocity * 0.5f;

        yield return new WaitForSeconds(softZoneTime);

        // Restore stored velocity (avoid multiplying back)
        _rb.velocity = _storedVelocity;
        Destroy(gameObject);
    }

    private IEnumerator BounceGroundRoutine()
    {
        _storedVelocity = _rb.velocity;
        _rb.velocity = _rb.velocity * 2f;

        yield return new WaitForSeconds(bounceZoneTime);

        _rb.velocity = _storedVelocity;
        Destroy(gameObject);
    }

    private void StartMagnetic()
    {
        // start magnetic effect: we will apply force in FixedUpdate while within duration
        _magnetStartTime = Time.time;
        // ensure selectedPoint is set externally or via inspector
        // mark object to be destroyed by FixedUpdate after duration ends
    }

    #endregion

    private void FixedUpdate()
    {
        // Magnetic effect uses physics, so handle in FixedUpdate
        if (!_isActivated || abilityType != AbilityType.MagneticPoint) return;

        // if duration expired -> stop
        if (Time.time > _magnetStartTime + magnetDuration)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 puckPos = _rb.position;
        Vector2 toTarget = selectedPoint - puckPos;
        float distance = toTarget.magnitude;

        // only affect when inside magnetRadius
        if (distance <= magnetRadius && distance > 0.001f)
        {
            // direction toward selected point
            Vector2 dir = toTarget.normalized;

            // force strength can be scaled by distance if desired (e.g., stronger when closer)
            // here we use linear scaling: (1 - distance / radius) so it's stronger when nearer.
            float distanceFactor = 1f - Mathf.Clamp01(distance / magnetRadius);
            Vector2 force = dir * magneticForce * distanceFactor * Time.fixedDeltaTime;

            // AddForce in FixedUpdate; use ForceMode2D.Force for continuous small pushes
            _rb.AddForce(force, ForceMode2D.Force);
        }
    }

    private void OnDestroy()
    {
        // Optional: restore any modified permanent values if needed
        // e.g., if you changed puck.maxSpeed temporarily, restore it here.
        // For now we leave speed booster permanent (as original code did).
    }

    #region Editor Helpers (optional)
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep sensible defaults visible in inspector and avoid negative values
        stickTime = Mathf.Max(0f, stickTime);
        softZoneTime = Mathf.Max(0f, softZoneTime);
        bounceZoneTime = Mathf.Max(0f, bounceZoneTime);
        magnetRadius = Mathf.Max(0.01f, magnetRadius);
        magneticForce = Mathf.Max(0f, magneticForce);
        magnetDuration = Mathf.Max(0f, magnetDuration);
        speedMultiplier = Mathf.Max(0.01f, speedMultiplier);
    }
#endif
    #endregion
}
