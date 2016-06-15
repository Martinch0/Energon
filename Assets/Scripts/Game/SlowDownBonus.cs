using UnityEngine;

/*
A bonus that slows down the game speed for a certain amount of time.
*/
public class SlowDownBonus : Bonus {

    [SerializeField] public float SlowDownPower = 15.0f;
    [SerializeField] public float SlowDownDecreaseStep = 0.75f;

    public override void CollectBonus()
    {
        DifficultyCurve.mInstance.ReduceSpeed(SlowDownPower, SlowDownDecreaseStep);
    }
}
