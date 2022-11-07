public class CoinInput : BulletInput
{
    protected CoinDrop drop;
    protected override void SetCommands()
    {
        fire = new CoinFire(target, 28f);
        moveForward = new CoinMove(target, 28f);
        die = new BulletDie(target, 28f);
        drop = new CoinDrop(target, 56f);
    }

    public ICommand InputDrop()
    {
        ClearAll();
        Interrupt(drop.SetCoin(SpawnHandler.Instance.PlaceItem(ItemType.Coin, 1, map.onTilePos)));
        DisableInput();
        return drop;
    }
}
