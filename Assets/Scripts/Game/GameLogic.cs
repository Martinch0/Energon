using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

/*
The class containing all the game logic.
*/
public class GameLogic : MonoBehaviour
{
    [Serializable]
    public struct BonusEntry
    {
        public Bonus type;
        [Range(1,100)]

        // How often does the bonus appear, compared to the other bonuses.
        [SerializeField] public int weight;
    }

	[SerializeField] private Camera GameplayCamera;
	[SerializeField] private float PlayerCollectDistance = 5.7f;
	[SerializeField] private int MaxMissedEnemies = 1;

    // The colors of the enemies.
    [SerializeField] public Color[] colors;

    // The frequency of spawning bonuses during a game.
    [Range(1, 100)]
    [SerializeField] public int BonusFrequency = 1;
    [SerializeField] public BonusEntry[] BonusTypes;

    // Scores UI Text elements
    [SerializeField] private Text Score;
    [SerializeField] private Text Coins;
    [SerializeField] private Text Shields;
    [SerializeField] private Text Restarts;

    // UI Panels
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private LeaderboardPanel Leaderboard;
    [SerializeField] private NewHighScorePanel NewHighScore;
    [SerializeField] private GameObject ReviveMenu;
    [SerializeField] private GameObject ShopMenu;
    [SerializeField] private GameObject Tutorial;
    [SerializeField] private GameObject PausedPanel;
    [SerializeField] private GameObject PurchaseError;
    [SerializeField] private GameObject PurchaseExisting;
    [SerializeField] private GameObject PurchaseSuccessful;

    public enum State { TapToStart, Game, Pause, Revive, GameOver };

    // Keep references of all active objects in the scene
    private List<GameObject> mActiveObjects;

    // Queue the Target colors as they are being generated.
    private Queue<int> mTargetsList;

	private DifficultyCurve mCurrentDifficulty;
	private PlayerCharacter mPlayerCharacter;

    private int mScore = 0;
	private int mMissedEnemies;
	private State mGameStatus;
    private int mBonusWeightsSum = 0;

    private static GameLogic mInstance = null;

	public static float GameDeltaTime { get; private set; }
	public static float GameSpeed { get { return DifficultyCurve.GameSpeed; } }
	public static float PlayerSpeed { get { return DifficultyCurve.PlayerSpeed; } }
	public static float ScreenBounds { get; private set; }
	public static float ScreenHeight { get; private set; }
	public static bool Paused { get; private set; }

    public static GameLogic GetGameLogic()
    {
        return mInstance;
    }

    void Start()
    {
        if (mInstance == null)
            mInstance = this;
        Reset();
    }

	void Awake()
	{
        // Setup the Main Camera
        Application.targetFrameRate = 150;
		float distance = transform.position.z - GameplayCamera.transform.position.z;
		ScreenHeight = CameraUtils.FrustumHeightAtDistance( distance, GameplayCamera.fieldOfView );
		ScreenBounds = ScreenHeight * GameplayCamera.aspect * 0.5f;

        // Setup input callbacks
		GameInput.OnTap += HandleOnTap;
        GameInput.OnHold += HandleOnHold;

        // Initialize the game
		mActiveObjects = new List<GameObject>();
        mTargetsList = new Queue<int>();
        mCurrentDifficulty = GetComponentInChildren<DifficultyCurve>();
		mPlayerCharacter = GetComponentInChildren<PlayerCharacter>();
		mGameStatus = State.TapToStart;
		mMissedEnemies = 0;
		Paused = false;

        // Precalculate Bonus weight sum to avoid calculations each frame.
        for(int i=0; i<BonusTypes.Length; i++)
        {
            mBonusWeightsSum += BonusTypes[i].weight;
        }
	}

	void Update()
	{
        // Save delta time. Do not update if not in game or Paused.
		GameDeltaTime = Paused || !(mGameStatus == State.Game) ? 0.0f : Time.deltaTime;

        UpdateScoreUI();

        if ( mGameStatus == State.Game )
		{

            SpawnNewEnemy();

			// Update the position of each active enemy, keep a track of enemies which have gone off screen or have been collected
			List<GameObject> oldEnemys = new List<GameObject>(); 
			for( int count = 0; count < mActiveObjects.Count; count++ )
			{
                // Update the position
                Vector3 position = GetUpdatedPosition(mActiveObjects[count]);

                // If off screen
				if( position.y < ScreenHeight * -0.5f )
				{
					oldEnemys.Add( mActiveObjects[count] ); 

                    if(mActiveObjects[count].GetComponent<Bonus>() != null)
                    {
                        // If it's a bonus, ignore it.
                    }
                    else if(mActiveObjects[count].GetComponent<Enemy>().GetColorIndex() == mPlayerCharacter.GetFirstTarget())
                    {
                        // If the same as the Main Target, mark as a miss.
                        mMissedEnemies++;

                        if (mMissedEnemies >= MaxMissedEnemies)
                        {
                            ShouldDie();
                            return;
                        }
                    }
                    else
                    {
                        // Else increase score.
                        mScore++;
                    }
				}
				else
				{
                    // Check for collision
					Vector3 diff = mPlayerCharacter.transform.position - position;
					if( diff.sqrMagnitude < PlayerCollectDistance )
					{
                        if (mActiveObjects[count].GetComponent<Bonus>() != null)
                        {
                            // If it's a bonus, collect it and mark it for removal.
                            mActiveObjects[count].GetComponent<Bonus>().CollectBonus();
                            mActiveObjects[count].GetComponent<Bonus>().OnCollect();
                            oldEnemys.Add(mActiveObjects[count]);
                        }
                        else if (mActiveObjects[count].GetComponent<Enemy>().GetColorIndex() == mPlayerCharacter.GetFirstTarget())
                        {
                            // If it's an enemy and it's color is the same as the main target, collect it, increase score, mark it for removal and choose a new secondary target.
                            mActiveObjects[count].GetComponent<Enemy>().OnCollect();
                            oldEnemys.Add(mActiveObjects[count]);
                            mScore++;
                            mPlayerCharacter.AddNewSecondTarget(GetRandomColor());
                        }
                        else
                        {
                            // Else a wrong enemy has been collected. Check if shield is active and ignore collision, otherwise die.
                            if (mPlayerCharacter.IsShieldActive())
                            {
                                oldEnemys.Add(mActiveObjects[count]);
                                mPlayerCharacter.DeactivateShield();
                            }
                            else
                            {
                                ShouldDie();
                                return;
                            }
                        }
					}
				}
            }

            // Recycle marked objects.
            for (int count = 0; count < oldEnemys.Count; count++)
            {
                mActiveObjects.Remove(oldEnemys[count]);
                EnemyFactory.Return(oldEnemys[count]);
            }
        }
    }

    // Updates the position of the GameObject and returns its new position.
    private Vector3 GetUpdatedPosition(GameObject enemy)
    {
        Vector3 currentPos = enemy.transform.position;

        // Update vertical position based on time elapsed and speed.
        currentPos.y -= GameDeltaTime * GameSpeed;

        // Update the horizontal position relative to the screen size.
        currentPos.x = -EnemyFactory.ColumnWidth + (EnemyFactory.ColumnWidth * enemy.GetComponent<Enemy>().column);

        enemy.transform.position = currentPos;
        return currentPos;
    }

    // Refresh the UI with the current stats.
    private void UpdateScoreUI()
    {
        Score.text = mScore + "";
        Coins.text = PlayerSettings.GetCoins() + "";
        Shields.text = PlayerSettings.GetShields() + "";
        Restarts.text = PlayerSettings.GetRestarts() + "";
    }

    // If the player has revives available, show the revive screen, otherwise the game is over.
    private void ShouldDie()
    {
        mPlayerCharacter.OnDie();
        if (PlayerSettings.GetRestarts() > 0)
        {
            ShowRevive();
        }
        else
        {
            GameOver();
        }
    }

    // Clear all active enemies and continue the game.
    public void Revive()
    {
        if(PlayerSettings.UseRestart())
        { 
            ClearEnemies();
            mGameStatus = State.Game;
            ReviveMenu.SetActive(false);
        }
        else
        {
            GameOver();
        }
    }

    // End the game.
    public void GameOver()
    {
        ShowNewHighScore();
        mCurrentDifficulty.Stop();
        mGameStatus = State.GameOver;
    }

    // Spawns a new enemy/bonus if needed.
    private void SpawnNewEnemy()
    {
        // Get the number of enemies to spawn.
        int enemies = mCurrentDifficulty.SpawnNumber();

        if (enemies > 0)
        {
            // Get the bonus type.
            int bonusType = GetSpawnBonusType();
            if(bonusType >= 0)
            {
                // If should spawn a bonus. Spawn one of the given type at a random position. 
                mActiveObjects.Add(EnemyFactory.DispatchBonus(bonusType, (EnemyFactory.Column)UnityEngine.Random.Range(0, 3)));
            }
            else
            {
                // If should spawn an enemy. Spawn one at a random position.
                mActiveObjects.Add(EnemyFactory.Dispatch((EnemyFactory.Column)UnityEngine.Random.Range(0, 3)));
            }
        }
    }

    // Returns the type of the bonus if a bonus should be spawned, -1 if a bonus should not be spawned.
    public int GetSpawnBonusType()
    {
        // Generate a random number and check if it matches the bonus spawn frequency.
        if(UnityEngine.Random.Range(1,100) <= BonusFrequency)
        {
            // Generate a random weigth amount and calculate which bonus corresponds to it.
            int type = UnityEngine.Random.Range(0, mBonusWeightsSum);
            for(int i=0; i<BonusTypes.Length; i++)
            {
                type -= BonusTypes[i].weight;
                if (type < 0) return i;
            }
        }
        return -1;
    }

    internal bool IsTargetsListEmpty()
    {
        return mTargetsList.Count == 0;
    }

    // Recycles all active elements.
    private void ClearEnemies()
    {
        EnemyFactory.Reset();
        mActiveObjects.Clear();
    }

    private void Reset()
    {
        // Reset the difficulty.
        mCurrentDifficulty.Reset();

        // Recycle all active objects.
        ClearEnemies();

        // Reset stats.
        mMissedEnemies = 0;
        mScore = 0;

        // Reset Player and set random colors.
        mPlayerCharacter.Reset();
        mPlayerCharacter.SetFirstTarget(GetRandomColor());
        mPlayerCharacter.SetSecondTarget(GetRandomColor());
    }

    private int GetRandomColor()
    {
        int color = UnityEngine.Random.Range(0, colors.Length);
        mTargetsList.Enqueue(color);
        return color;
    }

    public int PopFromTargetsList()
    {
        return mTargetsList.Dequeue();
    }

	private void HandleOnTap( Vector3 position )
    {
        // If the player is in game, hasn't activated his shield yet and has available shields.
        if (mGameStatus == State.Game && !mPlayerCharacter.IsShieldActive() && PlayerSettings.UseShield())
        {
            mPlayerCharacter.ActivateShield();
        }
	}

    private void HandleOnHold(float x)
    {
        if(mGameStatus == State.Game)
        {
            mPlayerCharacter.MoveToPosition(x);
        }
    }

    public void StartNewGame()
    {
        HideMainMenu();
        if(PlayerSettings.HasSeenTutorial())
        { 
            Reset();
            mCurrentDifficulty.InitilizeGameDifficulty();
            Paused = false;
            mGameStatus = State.Game;
        }
        else
        {
            ShowTutorial();
        }
    }

    public void Pause()
    {
        Paused = true;
        mGameStatus = State.Pause;
    }

    public void Resume()
    {
        Paused = false;
        mGameStatus = State.Game;
    }

    public void MarkTutorialOpened()
    {
        PlayerSettings.MarkSeenTutorial();
    }

    // Getters

    public int GetScore()
    {
        return mScore;
    }

    public State GetGameStatus()
    {
        return mGameStatus;
    }

    // UI elements toggle functions
    
    public void ShowMainMenu()
    {
        MainMenu.SetActive(true);
    }

    public void HideMainMenu()
    {
        MainMenu.SetActive(false);
    }

    public void ShowPaused()
    {
        PausedPanel.SetActive(true);
    }

    public void HidePaused()
    {
        PausedPanel.SetActive(false);
    }

    private void ShowRevive()
    {
        mGameStatus = State.Revive;
        ReviveMenu.SetActive(true);
    }

    public void HideRevive()
    {
        ReviveMenu.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        Leaderboard.SetActive(true);
    }

    public void HideLeaderboard()
    {
        Leaderboard.SetActive(false);
    }

    public void ShowNewHighScore()
    {
        NewHighScore.SetActive(true);
    }

    public void HideNewHighScore()
    {
        NewHighScore.SetActive(false);
    }

    public void ShowShopMenu()
    {
        ShopMenu.SetActive(true);
    }

    public void HideShopMenu()
    {
        ShopMenu.SetActive(false);
    }

    public void ShowPurchaseError()
    {
        PurchaseError.SetActive(true);
    }

    public void HidePurchaseError()
    {
        PurchaseError.SetActive(false);
    }

    public void ShowTutorial()
    {
        Reset();
        mPlayerCharacter.SetFirstTarget(1);
        mPlayerCharacter.SetSecondTarget(0);
        Tutorial.SetActive(true);
    }

    public void HideTutorial()
    {
        Tutorial.SetActive(false);
        if(!PlayerSettings.HasSeenTutorial())
        {
            PlayerSettings.MarkSeenTutorial();
            StartNewGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    public void ShowPurchaseExisting()
    {
        PurchaseExisting.SetActive(true);
    }

    public void HidePurchaseExisting()
    {
        PurchaseExisting.SetActive(false);
    }

    public void ShowPurchaseSuccessful()
    {
        PurchaseSuccessful.SetActive(true);
    }

    public void HidePurchaseSuccessful()
    {
        PurchaseSuccessful.SetActive(false);
    }
}
