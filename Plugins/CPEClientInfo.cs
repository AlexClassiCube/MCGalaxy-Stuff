using System;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;

namespace Core {
	public class CPEClientInfo : Plugin {
		public override string creator { get { return "Alex_"; } }
		public override string MCGalaxy_Version { get { return "1.9.4.3"; } }
		public override string name { get { return "CPEClientInfo"; } }

		public override void Load(bool startup) {
            OnSentMapEvent.Register(ClientInfoMsg, Priority.Low);
		}

		public override void Unload(bool shutdown) {
            OnSentMapEvent.Unregister(ClientInfoMsg);
		}

        void ClientInfoMsg(Player p, Level prevLevel, Level lvl) {
            string app = p.Session.appName;
            try { app = app.Substring(0, app.IndexOf(" +")); } catch { }
            p.SendCpeMessage(CpeMessageType.Status1, "&SYou are playing on:");
            p.SendCpeMessage(CpeMessageType.Status2, "&H" + app);
        }
    }
}