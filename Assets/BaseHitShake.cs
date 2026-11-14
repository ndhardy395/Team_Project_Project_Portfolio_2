using UnityEngine;
using System.Collections;

public class BaseHitShake : MonoBehaviour
{

    [SerializeField, Min(0.01f)] private float duration = 0.2f;
    [SerializeField, Min(0.01f)] private float strength = 0.15f;

    private Coroutine shakeRoutine;
    private Vector3 resetingPosition;
    private bool isShaking;


    public void Shake()
    {
        if(!isActiveAndEnabled)
        {
            return;
        }

        // Restore before restarting so repeated hits don't move the camera away.
        StopShake();

        resetingPosition = transform.localPosition;
        isShaking = true;
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float fade = 1f - elapsed / duration;
            Vector2 offset = Random.insideUnitCircle * strength * fade;

            // Shake along the camera's horizontal and vertical axis.

            Vector3 localOffset = transform.localRotation * new Vector3(offset.x, offset.y, 0f);

            transform.localPosition = resetingPosition + localOffset; ;

            // Still finishs if game Over pause the game

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = resetingPosition;
        isShaking = false;
        shakeRoutine = null;
    }


    private void StopShake()
    {
        if(shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        if(isShaking)
        {
            transform.localPosition = resetingPosition;
            isShaking= false;
        }
    }

    private void OnDisable()
    {
        StopShake();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
