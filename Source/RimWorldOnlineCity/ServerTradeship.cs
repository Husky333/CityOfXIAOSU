using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimWorldOnlineCity
{
	// Token: 0x02000E5A RID: 3674
	public class ServerTradeship : PassingShip, ITrader, IThingHolder
	{
		// Token: 0x17000FF4 RID: 4084
		// (get) Token: 0x060059EA RID: 23018 RVA: 0x001DE878 File Offset: 0x001DCA78
		public override string FullTitle
		{
			get
			{
				return this.name + " (" + this.def.label + ")";
			}
		}

		// Token: 0x17000FF5 RID: 4085
		// (get) Token: 0x060059EB RID: 23019 RVA: 0x001DE89A File Offset: 0x001DCA9A
		public int Silver
		{
			get
			{
				return this.CountHeldOf(ThingDefOf.Silver, null);
			}
		}

		// Token: 0x17000FF6 RID: 4086
		// (get) Token: 0x060059EC RID: 23020 RVA: 0x001DE8A8 File Offset: 0x001DCAA8
		public TradeCurrency TradeCurrency
		{
			get
			{
				return this.def.tradeCurrency;
			}
		}

		// Token: 0x17000FF7 RID: 4087
		// (get) Token: 0x060059ED RID: 23021 RVA: 0x001DE8B5 File Offset: 0x001DCAB5
		public IThingHolder ParentHolder
		{
			get
			{
				return base.Map;
			}
		}

		// Token: 0x17000FF8 RID: 4088
		// (get) Token: 0x060059EE RID: 23022 RVA: 0x001DE8BD File Offset: 0x001DCABD
		public TraderKindDef TraderKind
		{
			get
			{
				return this.def;
			}
		}

		// Token: 0x17000FF9 RID: 4089
		// (get) Token: 0x060059EF RID: 23023 RVA: 0x001DE8C5 File Offset: 0x001DCAC5
		public int RandomPriceFactorSeed
		{
			get
			{
				return this.randomPriceFactorSeed;
			}
		}

		// Token: 0x17000FFA RID: 4090
		// (get) Token: 0x060059F0 RID: 23024 RVA: 0x001DC0D7 File Offset: 0x001DA2D7
		public string TraderName
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000FFB RID: 4091
		// (get) Token: 0x060059F1 RID: 23025 RVA: 0x001DE8CD File Offset: 0x001DCACD
		public bool CanTradeNow
		{
			get
			{
				return !base.Departed;
			}
		}

		// Token: 0x17000FFC RID: 4092
		// (get) Token: 0x060059F2 RID: 23026 RVA: 0x0005BEF9 File Offset: 0x0005A0F9
		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000FFD RID: 4093
		// (get) Token: 0x060059F3 RID: 23027 RVA: 0x001DE8D8 File Offset: 0x001DCAD8
		public IEnumerable<Thing> Goods
		{
			get
			{
				int num;
				for (int i = 0; i < this.things.Count; i = num + 1)
				{
					Pawn pawn = this.things[i] as Pawn;
					if (pawn == null || !this.soldPrisoners.Contains(pawn))
					{
						yield return this.things[i];
					}
					num = i;
				}
				yield break;
			}
		}

		// Token: 0x060059F4 RID: 23028 RVA: 0x001DE8E8 File Offset: 0x001DCAE8
		public ServerTradeship()
		{
		}

		// Token: 0x060059F5 RID: 23029 RVA: 0x001DE904 File Offset: 0x001DCB04
		public ServerTradeship(TraderKindDef def, Faction faction = null) : base(faction)
		{
			this.def = def;
			this.things = new ThingOwner<Thing>(this);
			ServerTradeship.tmpExtantNames.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				ServerTradeship.tmpExtantNames.AddRange(from x in maps[i].passingShipManager.passingShips
												  select x.name);
			}
			this.name = OCUnion.MainHelper.ServerTradeshipName;
			if (faction != null)
			{
				this.name = string.Format("{0} {1} {2}", this.name, "OfLower".Translate(), faction.Name);
			}
			this.randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
			this.loadID = Find.UniqueIDsManager.GetNextPassingShipID();
		}

		// Token: 0x060059F6 RID: 23030 RVA: 0x001DEA03 File Offset: 0x001DCC03
		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			foreach (Thing thing in TradeUtility.AllLaunchableThingsForTrade(base.Map, this))
			{
				yield return thing;
			}
			IEnumerator<Thing> enumerator = null;
			foreach (Pawn pawn in TradeUtility.AllSellableColonyPawns(base.Map))
			{
				yield return pawn;
			}
			IEnumerator<Pawn> enumerator2 = null;
			yield break;
			yield break;
		}

		// Token: 0x060059F7 RID: 23031 RVA: 0x001DEA14 File Offset: 0x001DCC14
		public void GenerateThings()
		{
			things.ClearAndDestroyContents();
			goods_dict.Clear();
			goods_dict = SessionClient.Get.GetShipGoods();
			List<Thing> ts = new List<Thing>();
			foreach (KeyValuePair<string, Model.ThingEntry> entry in goods_dict)
            {
				if (entry.Key == "Silver")
					continue;
				Thing t = entry.Value.CreateThing();
				t.HitPoints = t.MaxHitPoints;
				ts.Add(t);
			}
			ThingDef silver_def = (ThingDef)GenDefDatabase.GetDef(typeof(ThingDef), "Silver");
			Thing silver = StockGeneratorUtility.TryMakeForStockSingle(silver_def, 10000);
			ts.Add(silver);

			this.things.TryAddRangeOrTransfer(ts, true, false);
			/*
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = this.def;
			parms.tile = new int?(base.Map.Tile);
			this.things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms), true, false);
			*/
		}

		// Token: 0x060059F8 RID: 23032 RVA: 0x001DEA6C File Offset: 0x001DCC6C
		public override void PassingShipTick()
		{
			// Never departs
			/*
			base.PassingShipTick();
			for (int i = this.things.Count - 1; i >= 0; i--)
			{
				Pawn pawn = this.things[i] as Pawn;
				if (pawn != null)
				{
					pawn.Tick();
					if (pawn.Dead)
					{
						this.things.Remove(pawn);
					}
				}
			}
			*/
		}

		// Token: 0x060059F9 RID: 23033 RVA: 0x001DEAC8 File Offset: 0x001DCCC8
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<TraderKindDef>(ref this.def, "def");
			Scribe_Deep.Look<ThingOwner>(ref this.things, "things", new object[]
			{
				this
			});
			Scribe_Collections.Look<Pawn>(ref this.soldPrisoners, "soldPrisoners", LookMode.Reference, Array.Empty<object>());
			Scribe_Values.Look<int>(ref this.randomPriceFactorSeed, "randomPriceFactorSeed", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.soldPrisoners.RemoveAll((Pawn x) => x == null);
			}
		}

		// Token: 0x060059FA RID: 23034 RVA: 0x001DEB60 File Offset: 0x001DCD60
		public override void TryOpenComms(Pawn negotiator)
		{
			if (!this.CanTradeNow)
			{
				return;
			}
			lock (UpdatingShip)
			{
				GenerateThings();
				update_on_dialog_close = true;
				SessionClientController.tradeship = this;
				Dialog_Trade form = new Dialog_Trade(negotiator, this, false);
				Find.WindowStack.Add(form);
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(this.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, false, true);
				TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon, Array.Empty<string>());
			}
		}

		public  void CheckUpdateServer()
        {
			lock (UpdatingShip)
			{
				if (!update_on_dialog_close) return;
				update_on_dialog_close = false;
				List<Model.ThingEntry> deltas = new List<Model.ThingEntry>();
				HashSet<string> checked_names = new HashSet<string>();
				goods_dict.Remove("Silver");
				/*
				foreach (KeyValuePair<string, Model.ThingEntry> entry in goods_dict)
				{
					OCUnion.Loger.Log("original: " + entry.Key + ", " + entry.Value.Count);
				}
				*/
				// Aggregate by thing.def.defName
				Dictionary<string, Thing> current_things = new Dictionary<string, Thing>();
				foreach (Thing t in things)
				{
					t.HitPoints = t.MaxHitPoints;
					if(current_things.ContainsKey(t.def.defName))
                    {
						current_things[t.def.defName].stackCount += t.stackCount;
					}
					else
                    {
						current_things.Add(t.def.defName, t);
					}
				}

				
				foreach (KeyValuePair<string, Thing> entry in current_things)
				{
					// for each current thing in ship
					Thing t = entry.Value;
					OCUnion.Loger.Log("current: " + t.def.defName + ", " + t.stackCount);
					checked_names.Add(t.def.defName);
					if (t.def.defName == "Silver")
						continue;
					if (goods_dict.ContainsKey(t.def.defName))
					{
						// is pre-existing thing in ship, update amount diff
						goods_dict[t.def.defName].Count = t.stackCount - goods_dict[t.def.defName].Count;
						if (goods_dict[t.def.defName].Count == 0)
						{
							// no amount change
							goods_dict.Remove(t.def.defName);
						}
					}
					else
					{
						// is new thing from player
						goods_dict.Add(t.def.defName, Model.ThingEntry.CreateEntry(t, t.stackCount));
					}
				}

				foreach (KeyValuePair<string, Model.ThingEntry> entry in goods_dict)
				{
					if (!checked_names.Contains(entry.Key))
					{
						// not current in ship but was there before, i.e., is sold out to player
						entry.Value.Count = -entry.Value.Count;
					}

					deltas.Add(entry.Value);
					OCUnion.Loger.Log("delta: " + entry.Key + ", " + entry.Value.Count);
				}
				SessionClient.Get.TradeWithShip(deltas);
				things.ClearAndDestroyContents();
				goods_dict.Clear();
			}
		}

		// Token: 0x060059FB RID: 23035 RVA: 0x001DEBDC File Offset: 0x001DCDDC
		public override void Depart()
		{
			this.passingShipManager.RemoveShip(this);
			this.things.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
			this.soldPrisoners.Clear();
		}

		// Token: 0x060059FC RID: 23036 RVA: 0x001DE878 File Offset: 0x001DCA78
		public override string GetCallLabel()
		{
			return this.name + " (" + this.def.label + ")";
		}

		// Token: 0x060059FD RID: 23037 RVA: 0x001DEBFC File Offset: 0x001DCDFC
		protected override AcceptanceReport CanCommunicateWith_NewTemp(Pawn negotiator)
		{
			AcceptanceReport result = base.CanCommunicateWith_NewTemp(negotiator);
			if (!result.Accepted)
			{
				return result;
			}
			return negotiator.CanTradeWith_NewTemp(base.Faction, this.TraderKind);
		}

		// Token: 0x060059FE RID: 23038 RVA: 0x001DEC2E File Offset: 0x001DCE2E
		protected override bool CanCommunicateWith(Pawn negotiator)
		{
			return base.CanCommunicateWith(negotiator) && negotiator.CanTradeWith(base.Faction, this.TraderKind);
		}

		// Token: 0x060059FF RID: 23039 RVA: 0x001DEC50 File Offset: 0x001DCE50
		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing != null)
			{
				return thing.stackCount;
			}
			return 0;
		}

		// Token: 0x06005A00 RID: 23040 RVA: 0x001DEC74 File Offset: 0x001DCE74
		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
			Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
			if (thing2 != null)
			{
				if (!thing2.TryAbsorbStack(thing, false))
				{
					thing.Destroy(DestroyMode.Vanish);
					return;
				}
			}
			else
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null && pawn.RaceProps.Humanlike)
				{
					this.soldPrisoners.Add(pawn);
				}
				this.things.TryAdd(thing, false);
			}
		}

		// Token: 0x06005A01 RID: 23041 RVA: 0x001DECE0 File Offset: 0x001DCEE0
		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				this.soldPrisoners.Remove(pawn);
			}
			TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(base.Map), base.Map, thing);
		}

		// Token: 0x06005A02 RID: 23042 RVA: 0x001DED2C File Offset: 0x001DCF2C
		private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
			for (int i = 0; i < this.things.Count; i++)
			{
				if (this.things[i].def == thingDef && this.things[i].Stuff == stuffDef)
				{
					return this.things[i];
				}
			}
			return null;
		}

		// Token: 0x06005A03 RID: 23043 RVA: 0x001DED85 File Offset: 0x001DCF85
		public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			Thing thing = this.HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef, false);
			}
			thing.stackCount += count;
		}

		// Token: 0x06005A04 RID: 23044 RVA: 0x001DC0DF File Offset: 0x001DA2DF
		public override string ToString()
		{
			return this.FullTitle;
		}

		// Token: 0x06005A05 RID: 23045 RVA: 0x001DEDB0 File Offset: 0x001DCFB0
		public ThingOwner GetDirectlyHeldThings()
		{
			return this.things;
		}

		// Token: 0x06005A06 RID: 23046 RVA: 0x001DEDB8 File Offset: 0x001DCFB8
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		// Token: 0x04003175 RID: 12661
		public TraderKindDef def;

		// Token: 0x04003176 RID: 12662
		private ThingOwner things;

		// Token: 0x04003177 RID: 12663
		private List<Pawn> soldPrisoners = new List<Pawn>();

		// Token: 0x04003178 RID: 12664
		private int randomPriceFactorSeed = -1;

		// Token: 0x04003179 RID: 12665
		private static List<string> tmpExtantNames = new List<string>();

		private Dictionary<string, Model.ThingEntry> goods_dict = new Dictionary<string, Model.ThingEntry>();

		private bool update_on_dialog_close = false;

		private static object UpdatingShip = new object();
	}
}
