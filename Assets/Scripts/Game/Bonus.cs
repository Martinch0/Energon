using System;

/*
An abstract class that is used to represent Bonus items in the game. Every bonus should inherit from this class and must override the CollectBonus method.
*/
public abstract class Bonus : Enemy {

    [NonSerialized] public int poolIndex;

    // Should be called when the player collects the bonus.
    public abstract void CollectBonus();
}
