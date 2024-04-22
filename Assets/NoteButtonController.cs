using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class NoteButtonController : MonoBehaviour
{
    public float speed = 5.0f; // Speed of the movement
    public float lifetime = 2f;
    public DemoScript noteSpawner;
    public string noteKey;
    private Vector3 startScale;
    private bool wasCorrect;
    private void Start()
    {
        Invoke(nameof(DestroyNote), lifetime); // Automatically destroys this GameObject after 'lifetime' seconds
        startScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(startScale, 0.25f);
    }

    void Update()
    {
        // Move the transform upwards at 'speed' units per second in local space
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }
    private Sequence destroySequence;
    private void DestroyNote()
    {
        if (wasCorrect) return;
        ComboController.Instance.ResetCombo();
        destroySequence = DOTween.Sequence();
        destroySequence.Append(transform.GetChild(0).gameObject.GetComponent<TMP_Text>().DOColor(new Vector4(1, 0, 0, 1), 0.2f)); // Destroy the note
        destroySequence.Join(gameObject.GetComponent<Image>().DOColor(new Vector4(0, 0, 0, 0), 0.2f)); // Destroy the note
        destroySequence.Append(transform.GetChild(0).gameObject.GetComponent<TMP_Text>().DOColor(new Vector4(1, 0, 0, 0), 0.2f)); // Destroy the note
        noteSpawner.RemoveNote(gameObject);

    }

    public void HitNote()
    {
        wasCorrect = true;
        destroySequence = DOTween.Sequence();
        destroySequence.Append(transform.GetChild(0).gameObject.GetComponent<TMP_Text>().DOColor(new Vector4(0, 1, 0, 1), 0.2f)); // Destroy the note
        destroySequence.Join(gameObject.GetComponent<Image>().DOColor(new Vector4(0, 0, 0, 0), 0.2f));
        destroySequence.Append(transform.GetChild(0).gameObject.GetComponent<TMP_Text>().DOColor(new Vector4(0, 0, 0, 0), 0.2f)); // Destroy the note
        ComboController.Instance.AddCombo();
        noteSpawner.RemoveNote(gameObject);
    }

}