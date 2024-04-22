using UnityEngine;

public class KeyboardInputManager : MonoBehaviour
{
    public DemoScript noteSpawner;

    void Update()
    {
        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
            {
                Debug.Log(kcode);
                string key = kcode.ToString();
                if (noteSpawner.activeNotes[0].GetComponent<NoteButtonController>().noteKey == key || noteSpawner.activeNotes[1].GetComponent<NoteButtonController>().noteKey == key)
                {
                    if (noteSpawner.activeNotes[0].GetComponent<NoteButtonController>().noteKey == key)
                    {
                        noteSpawner.activeNotes[0].GetComponent<NoteButtonController>().HitNote();

                        if (noteSpawner.activeNotes[1].GetComponent<NoteButtonController>().noteKey == key && noteSpawner.activeNotes[1].transform.position.y == noteSpawner.activeNotes[0].transform.position.y)
                        {
                            noteSpawner.activeNotes[1].GetComponent<NoteButtonController>().HitNote();
                            break;
                        }
                    }

                   
                  
                }
                else
                {
                    ComboController.Instance.ResetCombo();
                }
            }

        }
    }
}
