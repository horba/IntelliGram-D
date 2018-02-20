using System.Collections.Generic;

namespace Worker
{
    class BotCommand
    {
        public string Command { get; set; }
        public AccessModifier Access { get; set; }
    }

    class BotCommandEqualityComparer : IEqualityComparer<BotCommand>
    {
        public bool Equals(BotCommand x, BotCommand y) => x.Command == y.Command && x.Access == y.Access;
        public int GetHashCode(BotCommand obj) => obj.Command.GetHashCode();
    }
}