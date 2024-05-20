using System;
using System.IO;
using System.Collections.Generic;

using MCGalaxy;
using MCGalaxy.DB;
using MCGalaxy.Eco;
using MCGalaxy.Events;
using MCGalaxy.Commands;
using MCGalaxy.Commands.Chatting;
using MCGalaxy.Commands.Moderation;
using MCGalaxy.Events.PlayerEvents;

namespace Core {

	public class FancyWhois : Plugin {
		public override string creator { get { return "Alex_"; } }
		public override string MCGalaxy_Version { get { return "1.9.4.9"; } }
		public override string name { get { return "FancyWhois"; } }

		public override void Load(bool startup) {
			OfflineStat.Stats = NewOfflineStat.NewStats;
			OnlineStat.Stats = NewOnlineStat.NewStats;
		}

		public override void Unload(bool shutdown) {
		}

		public delegate void NewOfflineStatPrinter(Player p, Player who);

		public static class NewOfflineStat {

				public static List<OfflineStatPrinter> NewStats = new List<OfflineStatPrinter>() {
						CoreLine,
						(p, who) => NewOnlineStat.MiscLine(p, who.Name, who.Deaths, who.Money),
						TimeSpentLine,
						LoginLine,
						(p, who) => NewOnlineStat.LoginsLine(p, who.Logins),
						(p, who) => NewOnlineStat.BanLine(p, who.Name),
						(p, who) => NewOnlineStat.SpecialGroupLine(p, who.Name),
						(p, who) => NewOnlineStat.IPLine(p, who.Name, who.IP),
				};

				public static void CoreLine(Player p, PlayerData data) {
						Group group = Group.GroupIn(data.Name);
						string color = data.Color.Length == 0 ? group.Color : data.Color;
						string prefix = data.Title.Length == 0 ? "" : color + "[" + data.TitleColor + data.Title + color + "] ";
						string fullName = prefix + color + data.Name.RemoveLastPlus();
						NewOnlineStat.CommonCoreLine(p, fullName, data.Name, group, data.Messages);
				}


				public static void TimeSpentLine(Player p, PlayerData who) {
						p.Message("· Spent &a{0} &Son the server", who.TotalTime.Shorten());
				}

				public static void LoginLine(Player p, PlayerData who) {
						p.Message("· First login: &a{0}",
								who.FirstLogin.ToString("dd MMM yyyy"));
			p.Message("· Last login: &a{0}",
								who.LastLogin.ToString("dd MMM yyyy"));
				}
		}

		public delegate void NewOnlineStatPrinter(Player p, Player who);

		public static class NewOnlineStat {

				public static List<OnlineStatPrinter> NewStats = new List<OnlineStatPrinter>() {
						CoreLine,
						(p, who) => MiscLine(p, who.name, who.TimesDied, who.money),
						TimeSpentLine,
						LoginLine,
						(p, who) => LoginsLine(p, who.TimesVisited),
						(p, who) => BanLine(p, who.name),
						(p, who) => SpecialGroupLine(p, who.name),
						(p, who) => IPLine(p, who.name, who.ip),
						IdleLine,
						EntityLine,
				};

				public static void CoreLine(Player p, Player who) {
						string prefix	= who.title.Length == 0 ? "" : who.color + "[" + who.titlecolor + who.title + who.color + "] ";
						string fullName = prefix + who.ColoredName;
						CommonCoreLine(p, fullName, who.name, who.group, who.TotalMessagesSent);
				}

				internal static void CommonCoreLine(Player p, string fullName, string name, Group grp, int messages) {
						List<Pronouns> pros = Pronouns.GetFor(name);
						string prns = "";
						if (pros[0] != Pronouns.Default) {
							prns = pros.Join((pro) => pro.Name, "&S, &a");
							prns = " &S(&a" + prns + "&S)";
						}
						/*Pronouns pro = Pronouns.GetFor(name);
						prns = "";
						if (pro != Pronouns.Default) prns = " (&a" + pro.Name	+ "&S)";*/
						p.Message("{0} &S({1}){2}:", fullName, name, prns);
						p.Message("· Rank of {0}&S, wrote &a{1} &Smessages", grp.ColoredName, messages);
				}

				public static void MiscLine(Player p, string name, int deaths, int money) {
						if (Economy.Enabled) {
								p.Message("· &a{0} &cdeaths&S, &a{1} &S{2}",
															 deaths, money, Server.Config.Currency);
						} else {
								p.Message("· &a{0} &cdeaths&S",
															 deaths);
						}
				}

				public static void TimeSpentLine(Player p, Player who) {
						TimeSpan timeOnline = DateTime.UtcNow - who.SessionStartTime;
						p.Message("· Spent &a{0} &Son the server, &a{1} &Sthis session",
													 who.TotalTime.Shorten(), timeOnline.Shorten());
				}

				public static void LoginLine(Player p, Player who) {
						p.Message("· First login: &a{0}",
													 who.FirstLogin.ToString("dd MMM yyyy"));
						string client = " ";
						if (who.Session.ClientName().CaselessContains("web")) {
							client = " &Son &fweb";
						} else if (who.Session.ClientName().CaselessContains("android") || who.Session.ClientName().CaselessContains("mobile")) {
							client = " &Son &fmobile";
						} else {
							client = " &Son &fdesktop";
						}
			p.Message("· Currently &aonline" + client);
				}

				public static void LoginsLine(Player p, int logins) {
						p.Message("· Logged in &a{0} &Stimes", logins);
				}

				public static void BanLine(Player p, string name) {
						if (!Group.BannedRank.Players.Contains(name)) return;
						string banner, reason, prevRank;
						DateTime time;
						Ban.GetBanData(name, out banner, out reason, out time, out prevRank);

						if (banner != null) {
								p.Message("· Banned for {0} by {1}", reason, p.FormatNick(banner));
						} else {
								p.Message("· Is banned");
						}
				}

				public static void SpecialGroupLine(Player p, string name) {
						if (Server.Config.OwnerName.CaselessEq(name))
								p.Message("· Player is the &cserver owner");
				}

				public static void IPLine(Player p, string name, string ip) {
						ItemPerms seeIpPerms = CommandExtraPerms.Find("WhoIs", 1);
						if (!seeIpPerms.UsableBy(p.Rank)) return;

						string ipMsg = ip;
						if (Server.bannedIP.Contains(ip)) ipMsg = "&8" + ip + ", which is banned";

						p.Message("· IP: " + ipMsg);
						if (Server.Config.WhitelistedOnly && Server.whiteList.Contains(name))
								p.Message("· Player is &fWhitelisted");
				}

				public static void IdleLine(Player p, Player who) {
						TimeSpan idleTime = DateTime.UtcNow - who.LastAction;
						if (who.afkMessage != null) {
								p.Message("· Idle for {0} (AFK {1}&S)", idleTime.Shorten(), who.afkMessage);
						} else if (idleTime.TotalMinutes >= 1) {
								p.Message("· Idle for {0}", idleTime.Shorten());
						}
				}

				public static void EntityLine(Player p, Player who) {
						bool hasSkin = !who.SkinName.CaselessEq(who.truename);
						// TODO remove hardcoding
						bool hasModel = !(who.Model.CaselessEq("humanoid") || who.Model.CaselessEq("human"));

						if (hasSkin && hasModel) {
								p.Message("· Skin: &f{0} &Smodel: &f{1}", who.SkinName, who.Model);
						} else if (hasSkin) {
								p.Message("· Skin: &f{0}", who.SkinName);
						} else if (hasModel) {
								p.Message("· Model: &f{0}", who.Model);
						}
				}

		}

		}
}
