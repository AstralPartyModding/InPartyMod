namespace AstralPartyMod.Core.Events
{
    /// <summary>
    /// 游戏启动事件
    /// </summary>
    public class GameStartEvent
    {
        public static void Publish()
        {
            EventBus.Publish(new GameStartEvent());
        }
    }

    /// <summary>
    /// 游戏退出事件
    /// </summary>
    public class GameExitEvent
    {
        public static void Publish()
        {
            EventBus.Publish(new GameExitEvent());
        }
    }

    /// <summary>
    /// 回合开始事件
    /// </summary>
    public class RoundStartEvent
    {
        public int RoundNumber { get; }

        public RoundStartEvent(int roundNumber)
        {
            RoundNumber = roundNumber;
        }

        public static void Publish(int roundNumber)
        {
            EventBus.Publish(new RoundStartEvent(roundNumber));
        }
    }

    /// <summary>
    /// 回合结束事件
    /// </summary>
    public class RoundEndEvent
    {
        public int RoundNumber { get; }

        public RoundEndEvent(int roundNumber)
        {
            RoundNumber = roundNumber;
        }

        public static void Publish(int roundNumber)
        {
            EventBus.Publish(new RoundEndEvent(roundNumber));
        }
    }

    /// <summary>
    /// 卡牌被使用事件
    /// </summary>
    public class CardUsedEvent
    {
        public string CardId { get; }
        public int PlayerIndex { get; }

        public CardUsedEvent(string cardId, int playerIndex)
        {
            CardId = cardId;
            PlayerIndex = playerIndex;
        }

        public static void Publish(string cardId, int playerIndex)
        {
            EventBus.Publish(new CardUsedEvent(cardId, playerIndex));
        }
    }

    /// <summary>
    /// 场景加载完成事件
    /// </summary>
    public class SceneLoadedEvent
    {
        public string SceneName { get; }

        public SceneLoadedEvent(string sceneName)
        {
            SceneName = sceneName;
        }

        public static void Publish(string sceneName)
        {
            EventBus.Publish(new SceneLoadedEvent(sceneName));
        }
    }
}
