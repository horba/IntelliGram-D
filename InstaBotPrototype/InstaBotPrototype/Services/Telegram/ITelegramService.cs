namespace InstaBotPrototype.Services.Telegram
{
    public interface ITelegramService
    {
        int Connect(string username);
        void SendMessage(string message, string username);
    }
}