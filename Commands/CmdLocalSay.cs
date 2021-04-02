using System;
using MCGalaxy;

namespace Cmds {
    public sealed class CmdLocalSay : Command2 {        
        public override string name { get { return "LocalSay"; } }
        public override string shortcut { get { return "Local"; } }
        public override string type { get { return CommandTypes.Chat; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message, CommandData data) {
            if (message.Length == 0) { Help(p); return; }

            message = Colors.Escape(message);
            Chat.Message(ChatScope.Level, message, p.level, null, true);
        }
        
        public override void Help(Player p) {
            p.Message("&T/LocalSay [message]");
            p.Message("&HBroadcasts a message to everyone in the map you're on.");
        }
    }

}
