using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PotController : MonoBehaviour
{
    public Transform potTransform;
    public Transform keyTransform;
    public Transform keyTarget;
    public Vector3 endRotation;
    public float animationTime;
    public AnimationCurve animationCurve;
    public Vector3 startRotation;
    private Sequence potSequence;
    private void Start()
    {
        startRotation = transform.eulerAngles;
    }
    [Button("Hit Pot")]
    public void HitPot()
    {
        if (potSequence != null)
        {
            Debug.Log("not null");
            return;
        }
        potSequence = DOTween.Sequence();
        potSequence.Append(potTransform.DORotate(endRotation, animationTime).SetEase(animationCurve));
        potSequence.Join(keyTransform.DOJump(keyTarget.position,1,0, animationTime).SetEase(animationCurve));
        potSequence.Append(potTransform.DORotate(startRotation, animationTime).SetEase(animationCurve)).AppendCallback(() => potSequence = null); ;
    }

    private void OnTriggerEnter(Collider other)
    {
        HitPot();


    }
}
