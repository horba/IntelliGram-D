namespace InstaBotPrototype.Models
{
    public class ConfigurationModel
    {
        public int? Id { get; set; }
        public string InstaUsername { get; set; }
        public string InstaPassword { get; set; }
        public string TelegramUsername { get; set; }
        public string Tags { get; set; }
        public string Topics { get; set; }
    }
}