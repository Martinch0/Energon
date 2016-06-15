using UnityEngine;

/*
A bonus that gives one coin to the player. The coin change is saved immediately.
*/
public class CoinBonus : Bonus {

    [SerializeField] public float rotationSpeed = 600.0f;
    [SerializeField] private int coinsPerCollect = 1;
	
	// Update is called once per frame
	void Update () {
        // Rotate the coin based on elapsed time.
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
	}

    // Collect the bonus. Add coins to the player.
    public override void CollectBonus()
    {
        PlayerSettings.AddCoins(coinsPerCollect);
    }
}
