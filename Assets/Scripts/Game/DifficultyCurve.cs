using UnityEngine;

/*
A class that describes and handles the difficulty curve of the game.
*/
public class DifficultyCurve : MonoBehaviour 
{
    public static DifficultyCurve mInstance;

    // Game Start and Max speed
	[SerializeField] private float GameStartSpeed = 30.0f;
    [SerializeField] private float GameMaxSpeed = 50.0f;

    // The time that is required since the start of the game until max speed is reached.
    [SerializeField] private float TimeToMaxSpeed = 60.0f;

    // The slight increase of max speed during a game.
	[SerializeField] private float GameSpeedRamp = 0.05f;

    // Horizontal movement speed of the player.
	[SerializeField] private float PlayerStartSpeed = 20.0f;

    // Time between object spawns.
	[SerializeField] private float TimeBetweenSpawns = 0.7f;

	private float mTimeToNextSpawn;
    private float mTimeBetweenSpawns;
    private float mGameMaxSpeed;
    private float mSlowDown = 0.0f;
    private float mElapsedTime = 0.0f;
    private float mSlowDownStep = 0.0f;

	public static float GameSpeed { get; private set; }
	public static float PlayerSpeed { get; private set; }

	void Awake()
	{
		Reset();
	}

	void Start()
	{
		GameSpeed = GameStartSpeed;
		PlayerSpeed = PlayerStartSpeed;
        mTimeBetweenSpawns = TimeBetweenSpawns;
        mGameMaxSpeed = GameMaxSpeed;
    }

    void Update()
    {
        // Update elapsed time since start of the level.
        mElapsedTime += GameLogic.GameDeltaTime;
    }

    // Returns the number of enemies to be spawned if enough time has elapsed since last spawn. Updates the difficulty step on spawn.
	public int SpawnNumber()
	{
		mTimeToNextSpawn -= GameLogic.GameDeltaTime;
		if( mTimeToNextSpawn <= 0.0f )
		{
            // If should spawn, update the difficulty curve.
            mTimeToNextSpawn = mTimeBetweenSpawns;
            mSlowDown = Mathf.Max(0, mSlowDown - mSlowDownStep);
            mGameMaxSpeed += GameSpeedRamp;
            UpdateSpeed();

            // Return the number of enemies to spawn.
            return 1;
		}

        return 0;
	}

    // Stops the game.
	public void Stop()
	{
		GameSpeed = 0.0f;
		PlayerSpeed = 0.0f;
	}

    // Resets the difficulty curve.
	public void Reset()
	{
		GameSpeed = GameStartSpeed;
		mTimeToNextSpawn = TimeBetweenSpawns;
        mGameMaxSpeed = GameMaxSpeed;
        PlayerSpeed = PlayerStartSpeed;
        mSlowDown = 0.0f;
        mElapsedTime = 0.0f;
    }

    // Initializes the mInstance of the DifficultyCurve.
    public void InitilizeGameDifficulty()
    {
        if(mInstance == null)
        {
            mInstance = this;
        }
    }

    // Update the GameSpeed based on the elapsed time and slow down bonuses.
    private void UpdateSpeed()
    {
        GameSpeed = Mathf.Lerp(GameStartSpeed, mGameMaxSpeed, mElapsedTime / TimeToMaxSpeed) - mSlowDown;
    }

    // Used by the slow down bonus to reduce the speed. Reduces by decrease, the speed recovers with step.
    public void ReduceSpeed(float decrease, float step)
    {
        mSlowDown = decrease;
        mSlowDownStep = step;
        UpdateSpeed();
    }
}
