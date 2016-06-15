using UnityEngine;
using System.Collections.Generic;

/*
A class that describes the enemies and bonuses to be spawned and manages their object pools.
*/
public class EnemyFactory : MonoBehaviour
{
    public enum Column { One, Two, Three, NumColumns }

    private static EnemyFactory mInstance;

    [SerializeField] private Camera GameplayCamera;
    [SerializeField] private GameObject EnemyObject;
	[Range(5, 100)]
	[SerializeField] private int EnemyPoolSize = 10;
    [Range(5, 100)]
    [SerializeField] private int BonusPoolSize = 5;

    // Enemies pool
	private GameObject [] mPool;
    private List<GameObject> mActive;
    private List<GameObject> mInactive;

    // Bonus pools
    private GameObject[,] mBonusPool;
    private List<GameObject>[] mBonusActive;
    private List<GameObject>[] mBonusInactive;

    // Width of the spawn columns
    public static float ColumnWidth;
	
    // Make sure there is only one factory in the scene.
	void Awake()
	{
		if( mInstance == null )
        {
            mInstance = this;
        }
        else
		{
			Debug.LogError( "Only one EnemyFactory allowed - destorying duplicate" );
			Destroy( this.gameObject );
		}
	}

    void Start()
    {
        if(mInstance == null)
        {
            mInstance = this;
        }

        // Initialize the pools
        InitializeEnemyPool();
        InitializeBonusPool();
    }

    void Update()
    {
        // Update the width of the columns, possible changes due to resizing.
        ColumnWidth = (GameLogic.ScreenHeight * GameplayCamera.aspect * 0.8f) / (int)Column.NumColumns;
    }

    // Create the enemies, initialise the active and inactive lists, put all enemies in the inactive list
    private void InitializeEnemyPool()
    {
        mActive = new List<GameObject>();
        mInactive = new List<GameObject>();
        mPool = new GameObject[EnemyPoolSize];
        for (int count = 0; count < mPool.Length; count++)
        {
            GameObject enemy = Instantiate(EnemyObject);
            enemy.transform.parent = transform;
            mPool[count] = enemy;
            mInactive.Add(enemy);
            enemy.SetActive(false);
        }
    }

    // Create the bonuses, initialize the active and inactive lists, put all bonuses in their inactive lists
    private void InitializeBonusPool()
    {
        mBonusActive = new List<GameObject>[GameLogic.GetGameLogic().BonusTypes.Length];
        mBonusInactive = new List<GameObject>[GameLogic.GetGameLogic().BonusTypes.Length];
        mBonusPool = new GameObject[GameLogic.GetGameLogic().BonusTypes.Length, BonusPoolSize];
        for (int i = 0; i < GameLogic.GetGameLogic().BonusTypes.Length; i++)
        {
            mBonusActive[i] = new List<GameObject>();
            mBonusInactive[i] = new List<GameObject>();
            for (int j = 0; j < BonusPoolSize; j++)
            {
                GameObject bonus = Instantiate(GameLogic.GetGameLogic().BonusTypes[i].type).gameObject;
                bonus.GetComponent<Bonus>().poolIndex = i;
                bonus.transform.parent = transform;
                mBonusPool[i, j] = bonus;
                mBonusInactive[i].Add(bonus);
                bonus.SetActive(false);
            }
        }
    }

    // Spawn a enemy in the given column.
    public static GameObject Dispatch(Column column)
    {
        if (mInstance != null)
        {
            return mInstance.DoDispatch(column);
        }
        return null;
    }

    // Spawn a bonus in the given column.
    public static GameObject DispatchBonus(int type, Column column)
    {
        if (mInstance != null)
        {
            return mInstance.DoDispatchBonus(type, column);
        }
        return null;
    }

    // Recycle objects.
    public static void Return( GameObject enemy )
	{
		if( mInstance != null )
        {
            // Disable the object
            enemy.SetActive(false);
            if (enemy.GetComponent<Bonus>() != null)
            {
                // If it's a bonus, recycle it to its corresponding pool.
                mInstance.mBonusInactive[enemy.GetComponent<Bonus>().poolIndex].Add(enemy);
            }
			if( mInstance.mActive.Remove( enemy ) )
			{
                // If it's an enemy, recycle it to the inactive pool.
				mInstance.mInactive.Add( enemy ); 
			}
		}
    }

    // Resets the Factory. Disables all objects and puts them in their inactive pools.
    public static void Reset()
	{
        RecycleAllEnemies();
        RecycleAllBonuses();
	}

    // Disable and recycle all enemies.
    private static void RecycleAllEnemies()
    {
        if (mInstance != null && mInstance.mPool != null)
        {
            mInstance.mActive.Clear();
            mInstance.mInactive.Clear();

            for (int count = 0; count < mInstance.mPool.Length; count++)
            {
                mInstance.mPool[count].SetActive(false);
                mInstance.mInactive.Add(mInstance.mPool[count]);
            }
        }
    }

    // Disable and recycle all bonuses.
    private static void RecycleAllBonuses()
    {
        if (mInstance != null && mInstance.mBonusPool != null)
        {
            for (int i = 0; i < GameLogic.GetGameLogic().BonusTypes.Length; i++)
            {
                mInstance.mBonusActive[i].Clear();
                mInstance.mBonusInactive[i].Clear();

                for (int j = 0; j < mInstance.BonusPoolSize; j++)
                {
                    mInstance.mBonusPool[i, j].SetActive(false);
                    mInstance.mBonusInactive[i].Add(mInstance.mBonusPool[i, j]);
                }
            }

        }
    }

    // Returns an initialized Enemy from the Enemy pool. If no Enemy is available, returns null.
	private GameObject DoDispatch( Column column )
	{
		// Look for a free enemy and then dispatch them 
		GameObject result = null;
		if( mInactive.Count > 0 )
        {
            GameObject enemy = mInactive[0];

            // Initialize its color
            int color = GetSpawnColor();
            enemy.GetComponent<Enemy>().SetColorIndex(color);

            // Initialize its position
            enemy.GetComponent<Enemy>().column = (int)column;
            Vector3 position = enemy.transform.position;
            position.x = -ColumnWidth + (ColumnWidth * (float)column);
            position.y = GameLogic.ScreenHeight * 1.0f;
            position.z = 0.0f;
            enemy.transform.position = position;

            // Manage the pool
            mInactive.Remove(enemy);
            mActive.Add(enemy);

            // Activate the enemy
            enemy.SetActive(true);
            result = enemy;
        }

        // Returns the enemy if a free enemy was found and dispatched, otherwise null.
        return result;
    }

    // Returns an initialized Bonus from the Bonus pool. If no Bonus is available, returns null.
    private GameObject DoDispatchBonus(int type, Column column)
    {
        // Look for a free bonus and then dispatch them 
        GameObject result = null;
        if (mBonusInactive[type].Count > 0)
        {
            GameObject bonus = mBonusInactive[type][0];

            // Initialize its position
            bonus.GetComponent<Enemy>().column = (int)column;
            Vector3 position = bonus.transform.position;
            position.x = -ColumnWidth + (ColumnWidth * (float)column);
            position.y = GameLogic.ScreenHeight * 1.0f;
            position.z = 0.0f;
            bonus.transform.position = position;

            // Manage the pool
            mBonusInactive[type].Remove(bonus);
            mBonusActive[type].Add(bonus);

            // Activate the bonus
            bonus.SetActive(true);
            result = bonus;
        }

        // Returns the bonus if a free bonus was found and dispatched, otherwise null.
        return result;
    }

    // Returns either a random color or the next one for collection.
    private static int GetSpawnColor()
    {
        GameLogic mGameLogic = GameLogic.GetGameLogic();
        int color;

        // Two out of three times should spawn the next color for collection.
        if (mGameLogic.IsTargetsListEmpty() || UnityEngine.Random.Range(0, 2) % 3 == 0)
        {
            // Random color
            color = UnityEngine.Random.Range(0, mGameLogic.colors.Length);
        }
        else
        {
            // The next color in the queue
            color = mGameLogic.PopFromTargetsList();
        }
        return color;
    }
}
