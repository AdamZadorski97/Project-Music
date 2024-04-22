using F1yingBanana.SfizzUnity;
using System.IO;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Don't forget to link the reference to the SfizzPlayer in the scene!
    public SfizzPlayer player;
    public string sfzPath;
   
    private void Start()
    {
        string path = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, sfzPath));
     
        player.Sfizz.LoadFile(path);
    }

    private void Update()
    {
        player.Sfizz.SendNoteOn(/* delay= */ 0, /* noteNumber= */ 60, /* velocity= */ 64);
    }
}