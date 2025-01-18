using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType
{
    Happy = 1,
    Angry = 2,
    Sad = 3,
}
    
public class ComicItem : MonoBehaviour
{
    private Dictionary<int, ComicItem> _nextComicItems = new Dictionary<int, ComicItem>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
