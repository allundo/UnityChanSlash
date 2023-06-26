public class PlayerInfoTest : PlayerInfo
{
    public void SetMapUtil(IPlayerMapUtil mapUtil) => this.mapUtil = mapUtil;
    public void SetInput(IPlayerInput input) => this.input = input;
}
