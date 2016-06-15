using System;
using UnityEngine;

/*
A class that represents the Energon balls in the game, containing different colors and position in the field.
The class is also used as a base class for the Bonus objects.
*/
public class Enemy : MonoBehaviour {

    private int colorIndex;
    [NonSerialized] public int column;

    public AudioClip collectAudio;
    private float audioVolume = 0.7f;

    private SpriteRenderer mSpriteRenderer;

    void Start ()
    {
        mSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update () {
        // Update the color of the Enemy if its not a Bonus
        if (gameObject.GetComponent<Bonus>() == null && mSpriteRenderer != null)
        {
            mSpriteRenderer.material.color = GameLogic.GetGameLogic().colors[colorIndex];
            mSpriteRenderer.material.SetColor("_Color", GameLogic.GetGameLogic().colors[colorIndex]);
        }
    }

    // Callback after collection.
    public void OnCollect()
    {
        // Play a sound at the Main Camera's position.
        if(collectAudio != null)
        {
            AudioSource.PlayClipAtPoint(collectAudio, Camera.main.transform.position, audioVolume);
        }
    }

    // Getters and Setters

    public void SetColorIndex(int index)
    {
        colorIndex = index;
    }

    public int GetColorIndex()
    {
        return colorIndex;
    }
}
