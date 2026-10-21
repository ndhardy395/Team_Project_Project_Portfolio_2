using System.Collections;
using UnityEngine;

public class EnemyPortalControll : MonoBehaviour
{
    public PortalAnimation portalAnimation;

    private int activeSpawner = 0;

    private Coroutine closeRoutine;


    public void BeginSpawning()
    {
        activeSpawner++;

        // cancel closing if another spawner starts

        if(closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        gameObject.SetActive(true);

        if(activeSpawner == 1 && portalAnimation != null)
        {
            portalAnimation.OpenPortal();
        }
    }

    public void FinishSpawning()
    {
        activeSpawner = Mathf.Max(0, activeSpawner - 1);

        if(activeSpawner == 0)
        {
            closeRoutine = StartCoroutine(ClosePortalRoutine());
        }
    }
    public void OpenPortal()
    {
        gameObject.SetActive(true);

        if(portalAnimation != null)
        {
            portalAnimation.OpenPortal();
        }
    }

    public void ClosePortal()
    {
        if(portalAnimation != null)
        {
            portalAnimation.ClosePortal();
        }
    }

    public IEnumerator ClosePortalRoutine()
    {

        if(portalAnimation != null)
        {
            portalAnimation.ClosePortal();
            yield return new WaitUntil(() => portalAnimation.IsClosed);

        }

        if (activeSpawner == 0)
        {
            gameObject.SetActive(false);
        }
        closeRoutine = null;
    }

   
}
