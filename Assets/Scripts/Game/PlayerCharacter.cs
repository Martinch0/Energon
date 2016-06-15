using UnityEngine;

/*
A class that represents the player and handles its states.
*/
public class PlayerCharacter : MonoBehaviour 
{
    // The player ship meshes
    [SerializeField] private GameObject FirstTargetHull;
    [SerializeField] private GameObject SecondTargetHull;
    [SerializeField] private GameObject MainHull;

    [SerializeField] private Camera GameplayCamera;

    // Audio that should play on game over.
    [SerializeField] private AudioClip dieAudio;

    // Audio that should play on activation and deactivation of the shields.
    [SerializeField] private AudioClip shieldActivated;
    [SerializeField] private AudioClip shieldDeactivated;

    [SerializeField] private Color shieldColor;

    private static PlayerCharacter mPlayerCharacter;

    // References to the renderers of the ship's meshes.
    private MeshRenderer mFirstTargetHullRenderer;
    private MeshRenderer mSecondTargetHullRenderer;
    private MeshRenderer mMainHullRenderer;

    // Start vertical position
	private float mStartY;
    
    // Target position of the player.
    private float mTargetPosition;

    // Colors of the targets
    private int mFirstTarget;
    private int mSecondTarget;

    // Shield state
    private bool mShieldActive = false;

    void Awake()
    {
        if(mPlayerCharacter == null)
        {
            mPlayerCharacter = this;
        }
    }

	void Start() 
	{
        // Initialize the players initial position and save it.
		Vector3 position = transform.position;
		position.y = GameLogic.ScreenHeight * -0.3f;
		mStartY = position.y; 
		transform.position = position;

        // Load the renderers
        mFirstTargetHullRenderer = FirstTargetHull.GetComponent<MeshRenderer>();
        mSecondTargetHullRenderer = SecondTargetHull.GetComponent<MeshRenderer>();
        mMainHullRenderer = MainHull.GetComponent<MeshRenderer>();
    }

	void Update()
	{
        // Move the player to its target position based on the delta time and the player speed.
		Vector3 position = transform.position;
		if( mTargetPosition != position.x )
		{
			position.x = Mathf.SmoothStep( position.x, mTargetPosition, GameLogic.GameDeltaTime * GameLogic.PlayerSpeed );
			transform.position = position;
        }

        // Update the colors of the ship based on its target colors.
        UpdateColors();
    }

    // Reset the position and state of the player.
	public void Reset()
	{
		Vector3 position = new Vector3( 0.0f, mStartY, 0.0f );
		transform.position = position;
		mTargetPosition = 0.0f;

        SetShieldActive(false);
    }

    // Sets the Target position of the player to the specified one.
    public void MoveToPosition(float x)
    {
        mTargetPosition = x;
    }

    // Update the colors if the ship based on the target colors.
    public void UpdateColors()
    {
        GameLogic gameLogic = GameLogic.GetGameLogic();

        if (mFirstTargetHullRenderer != null)
            mFirstTargetHullRenderer.material.color = gameLogic.colors[mFirstTarget];

        if (mSecondTargetHullRenderer != null)
            mSecondTargetHullRenderer.material.color = gameLogic.colors[mSecondTarget];

        // Change the color of the wings based on the shield color.
        if (mShieldActive)
        {
            mMainHullRenderer.material.color = shieldColor;
        }
        else
        {
            mMainHullRenderer.material.color = gameLogic.colors[mFirstTarget];
        }
    }

    // Makes the current secondary target a main target and adds a new secondary target.
    public void AddNewSecondTarget(int colorIndex)
    {
        mFirstTarget = mSecondTarget;
        mSecondTarget = colorIndex;
    }

    // Activates the shield.
    public void ActivateShield()
    {
        AudioSource.PlayClipAtPoint(shieldActivated, Camera.main.transform.position, 0.7f);
        SetShieldActive(true);
    }

    // Deactivates the shield.
    public void DeactivateShield()
    {
        AudioSource.PlayClipAtPoint(shieldDeactivated, Camera.main.transform.position, 0.7f);
        SetShieldActive(false);
    }

    // Callback on die.
    public void OnDie()
    {
        AudioSource.PlayClipAtPoint(dieAudio, Camera.main.transform.position, 0.7f);
    }

    // Getters and Setters
    
    public static PlayerCharacter GetPlayerCharacter()
    {
        return mPlayerCharacter;
    }

    public void SetFirstTarget(int colorIndex)
    {
        mFirstTarget = colorIndex;
    }

    public void SetSecondTarget(int colorIndex)
    {
        mSecondTarget = colorIndex;
    }

    public int GetFirstTarget()
    {
        return mFirstTarget;
    }

    public int GetSecondTarget()
    {
        return mSecondTarget;
    }

    public bool IsShieldActive()
    {
        return mShieldActive;
    }

    public void SetShieldActive(bool active)
    {
        mShieldActive = active;
    }
}
