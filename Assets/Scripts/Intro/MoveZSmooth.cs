using UnityEngine;
using System.Collections;

public class MoveZSmooth : MonoBehaviour
{
    private Coroutine moveCoroutine;

    public void MoveToZ(float targetZ)
    {
        float speed = 3f; // default speed (editable if you want)
        StartMove(targetZ, speed);
    }

    public void MoveToZ(float targetZ, float speed)
    {
        StartMove(targetZ, speed);
    }

    private void StartMove(float targetZ, float speed)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveZCoroutine(targetZ, speed));
    }

    private IEnumerator MoveZCoroutine(float targetZ, float speed)
    {
        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y,
            targetZ
        );

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                speed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPos;
    }
}
