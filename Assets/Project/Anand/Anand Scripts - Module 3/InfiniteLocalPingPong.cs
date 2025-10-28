using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class InfiniteLocalPingPong : MonoBehaviour
{
    [Tooltip("If null, will use this GameObject's transform.")]
    public Transform target;

    [Tooltip("If UseOffset is true, this is an offset from the start localPosition. Otherwise it's the absolute localPosition to move to.")]
    public Vector3 endLocalPosition = new Vector3(0, 1, 0);

    [Tooltip("When true, endLocalPosition is treated as an offset from the starting localPosition.")]
    public bool useOffset = true;

    [Tooltip("Time to move from start -> end (seconds).")]
    public float duration = 1f;

    [Tooltip("Animation curve for easing (replaces DOTween ease).")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Whether the tween should autoplay on Start.")]
    public bool playOnStart = true;

    // internal
    private Coroutine _pingPongCoroutine;
    private Vector3 _startLocalPos;

    void Awake()
    {
        if (target == null) target = transform;
    }

    void Start()
    {
        // Capture starting position here to ensure it's the correct initial position
        _startLocalPos = target.localPosition;
        
        if (playOnStart)
            StartPingPong();
    }

    /// <summary>
    /// Begins the infinite ping-pong tween. Safe to call multiple times.
    /// </summary>
    public void StartPingPong()
    {
        if (target == null) return;

        // Stop existing coroutine
        if (_pingPongCoroutine != null)
        {
            StopCoroutine(_pingPongCoroutine);
        }

        // Update start position in case object was moved
        _startLocalPos = target.localPosition;

        _pingPongCoroutine = StartCoroutine(PingPongCoroutine());
    }

    /// <summary>
    /// Stops the movement and optionally snap back to start.
    /// </summary>
    public void StopPingPong(bool snapToStart = false)
    {
        if (_pingPongCoroutine != null)
        {
            StopCoroutine(_pingPongCoroutine);
            _pingPongCoroutine = null;
        }

        if (snapToStart && target != null)
            target.localPosition = _startLocalPos;
    }

    private IEnumerator PingPongCoroutine()
    {
        Vector3 absoluteEnd = useOffset ? _startLocalPos + endLocalPosition : endLocalPosition;
        
        while (true)
        {
            // Move to end position
            yield return StartCoroutine(MoveToPosition(_startLocalPos, absoluteEnd, duration));
            
            // Move back to start position
            yield return StartCoroutine(MoveToPosition(absoluteEnd, _startLocalPos, duration));
        }
    }

    private IEnumerator MoveToPosition(Vector3 startPos, Vector3 endPos, float moveDuration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDuration)
        {
            if (target == null) yield break;
            
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);
            
            // Apply easing using animation curve
            float easedT = easeCurve.Evaluate(t);
            
            target.localPosition = Vector3.Lerp(startPos, endPos, easedT);
            
            yield return null;
        }
        
        // Ensure we end up exactly at the target position
        if (target != null)
            target.localPosition = endPos;
    }

    void OnDisable()
    {
        if (_pingPongCoroutine != null)
        {
            StopCoroutine(_pingPongCoroutine);
            _pingPongCoroutine = null;
        }
    }

    void OnValidate()
    {
        // keep sane values in editor
        if (duration < 0.01f) duration = 0.01f;
    }
}