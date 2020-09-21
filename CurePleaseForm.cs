namespace CurePlease
{
    using CurePlease.Addon;
    using CurePlease.Properties;
    using EliteMMO.API;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using static EliteMMO.API.EliteAPI;

    public partial class CurePleaseForm : Form
    {

        private OptionsForm Form2 = new OptionsForm();

        public class BuffStorage : List<BuffStorage>
        {
            public string CharacterName { get; set; }

            public string CharacterBuffs { get; set; }
        }

        public class SongData : List<SongData>
        {
            public string song_type { get; set; }

            public int song_position { get; set; }

            public string song_name { get; set; }

            public int buff_id { get; set; }
        }

        public class SpellsData : List<SpellsData>
        {
            public string Spell_Name { get; set; }

            public int spell_position { get; set; }

            public int type { get; set; }

            public int buffID { get; set; }

            public bool aoe_version { get; set; }
        }

        public class GeoData : List<GeoData>
        {
            public int geo_position { get; set; }

            public string indi_spell { get; set; }

            public string geo_spell { get; set; }
        }

        private int currentSCHCharges = 0;

        private string debug_MSG_show = string.Empty;

        private int lastCommand = 0;

        private int lastKnownEstablisherTarget = 0;

        // BARD SONG VARIABLES
        private int song_casting = 0;

        private int PL_BRDCount = 0;
        private bool ForceSongRecast = false;
        private string Last_Song_Cast = string.Empty;

        // GEO ENGAGED CHECK
        public bool targetEngaged = false;

        public bool EclipticStillUp = false;

        public bool CastingBackground_Check = false;
        public bool JobAbilityLock_Check = false;

        public string JobAbilityCMD = String.Empty;

        private DateTime DefaultTime = new DateTime(1970, 1, 1);

        private bool curePlease_autofollow = false;

        private List<string> characterNames_naRemoval = new List<string>();

        public IAddon AddonManager;

        public List<SpellsData> barspells = new List<SpellsData>();

        public List<SpellsData> enspells = new List<SpellsData>();

        public List<SpellsData> stormspells = new List<SpellsData>();


        private int GetInventoryItemCount(EliteAPI api, ushort itemid)
        {
            int count = 0;
            for (int x = 0; x <= 80; x++)
            {
                EliteAPI.InventoryItem item = api.Inventory.GetContainerItem(0, x);
                if (item != null && item.Id == itemid)
                {
                    count += (int)item.Count;
                }
            }

            return count;
        }

        private int GetTempItemCount(EliteAPI api, ushort itemid)
        {
            int count = 0;
            for (int x = 0; x <= 80; x++)
            {
                EliteAPI.InventoryItem item = api.Inventory.GetContainerItem(3, x);
                if (item != null && item.Id == itemid)
                {
                    count += (int)item.Count;
                }
            }

            return count;
        }

        private ushort GetItemId(string name)
        {
            EliteAPI.IItem item = _ELITEAPIPL.Resources.GetItem(name, 0);
            return item != null ? (ushort)item.ItemID : (ushort)0;
        }

        private int GetAbilityRecastBySpellId(int id)
        {
            List<int> abilityIds = _ELITEAPIPL.Recast.GetAbilityIds();
            for (int x = 0; x < abilityIds.Count; x++)
            {
                if (abilityIds[x] == id)
                {
                    return _ELITEAPIPL.Recast.GetAbilityRecast(x);
                }
            }

            return -1;
        }

        public static EliteAPI _ELITEAPIPL;

        public EliteAPI _ELITEAPIMonitored;

        public ListBox processids = new ListBox();

        public ListBox activeprocessids = new ListBox();

        public double last_percent = 1;

        public string castingSpell = string.Empty;

        public int max_count = 10;
        public int spell_delay_count = 0;

        public int geo_step = 0;

        public int followWarning = 0;

        public bool stuckWarning = false;
        public int stuckCount = 0;

        public int protectionCount = 0;

        public int IDFound = 0;

        public float lastZ;
        public float lastX;
        public float lastY;

        // Stores the previously-colored button, if any
        public List<BuffStorage> ActiveBuffs = new List<BuffStorage>();

        public List<SongData> SongInfo = new List<SongData>();

        public List<GeoData> GeomancerInfo = new List<GeoData>();

        public List<int> known_song_buffs = new List<int>();

        public List<string> TemporaryItem_Zones = new List<string> { "Escha Ru'Aun", "Escha Zi'Tah", "Reisenjima", "Abyssea - La Theine", "Abyssea - Konschtat", "Abyssea - Tahrongi",
                                                                        "Abyssea - Attohwa", "Abyssea - Misareaux", "Abyssea - Vunkerl", "Abyssea - Altepa", "Abyssea - Uleguerand", "Abyssea - Grauberg", "Walk of Echoes" };

        public string wakeSleepSpellName = "Cure";

        public string plSilenceitemName = "Echo Drops";

        public string plDoomItemName = "Holy Water";

        private float plX;

        private float plY;

        private float plZ;

        private byte playerOptionsSelected;

        private byte autoOptionsSelected;

        private bool pauseActions;

        private bool islowmp;

        public bool LUA_Plugin_Loaded = false;

        public int firstTime_Pause = 0;


        public int GetAbilityRecast(string checked_abilityName)
        {
            int id = _ELITEAPIPL.Resources.GetAbility(checked_abilityName, 0).TimerID;
            List<int> IDs = _ELITEAPIPL.Recast.GetAbilityIds();
            for (int x = 0; x < IDs.Count; x++)
            {
                if (IDs[x] == id)
                {
                    return _ELITEAPIPL.Recast.GetAbilityRecast(x);
                }
            }
            return 0;
        }

        public int CheckSpellRecast(string spellName)
        {
            var spell = _ELITEAPIPL.Resources.GetSpell(spellName, (int) Language.EN);

            if (spell.Index == (int) Spell.HonorMarch)
            {
                return 0;
            }

            return _ELITEAPIPL.Recast.GetSpellRecast(spell.Index);
        }

        public bool HasAbility(string name)
        {
            if (_ELITEAPIPL.Player.GetPlayerInfo().Buffs.Any(b => b == 261 || b == 16))
            {// Can't use JAs when Inpairment or Amnesia
                return false;
            }

            return _ELITEAPIPL.Player.HasAbility(
                _ELITEAPIPL.Resources.GetAbility(name, 0).ID
            );
        }

        public bool CanCastSpell(ISpell spell)
        {
            if (_ELITEAPIPL.Player.GetPlayerInfo().Buffs.Any(b => b == 6 || b == 29 || b == 262))
            {// Can't cast magic when Silence, Mute or Omerta
                return false;
            }

            if (spell.ID == (int)Spell.HonorMarch)
            {
                return true;
            }

            if (_ELITEAPIPL.Player.MP < spell.MPCost)
            {
                return false;
            }

            return _ELITEAPIPL.Player.HasSpell(spell.Index) && JobChecker(spell);
        }

        public bool CanCastSpell(string name)
        {
            return CanCastSpell(_ELITEAPIPL.Resources.GetSpell(name, (int)Language.EN));
        }

        public static bool JobChecker(ISpell spell)
        {
            if (spell.Index == (int) Spell.HonorMarch)
            {
                return true;
            }

            var mainJob = _ELITEAPIPL.Player.MainJob;
            var mainjobLevelRequired = spell.LevelRequired[mainJob];
            var subjobLevelRequired = spell.LevelRequired[_ELITEAPIPL.Player.SubJob];

            if (mainjobLevelRequired == -1 && subjobLevelRequired == -1)
            {// -1 means the job doesn't have access to the spell
                return false;
            }

            if (mainjobLevelRequired <= _ELITEAPIPL.Player.MainJobLevel && subjobLevelRequired <= _ELITEAPIPL.Player.SubJobLevel)
            {// Main and sub job aren't high enough to cast the spell
                return true;
            }

            if (mainjobLevelRequired <= 99)
            {// Not a job point spell
                return false;
            }

            PlayerJobPoints jobPoints = _ELITEAPIPL.Player.GetJobPoints(mainJob);

            if (spell.Index == (int) Spell.Refresh3 || spell.Index == (int) Spell.Temper2)
            {
                return mainJob == (int)Job.RDM
                    && jobPoints.SpentJobPoints >= 1200;
            }

            if (spell.Index == (int) Spell.Distract3 || spell.Index == (int) Spell.Frazzle3)
            {
                return mainJob == (int)Job.RDM
                    && jobPoints.SpentJobPoints >= 550;
            }

            if (spell.Name[(int) Language.EN].Contains("storm ii"))
            {
                return mainJob == (int)Job.SCH
                    && jobPoints.SpentJobPoints >= 100;
            }

            if (spell.Index == (int)Spell.Reraise4)
            {
                return mainJob == (int)Job.WHM
                    && jobPoints.SpentJobPoints >= 100;
            }

            if (spell.Index == (int)Spell.FullCure)
            {
                return mainJob == (int)Job.WHM
                    && jobPoints.SpentJobPoints >= 1200;
            }

            return false;
        }

        // SPELL CHECKER CODE: (CheckSpellRecast("") == 0) && (HasSpell(""))
        // ABILITY CHECKER CODE: (GetAbilityRecast("") == 0) && (HasAbility(""))
        // PIANISSIMO TIME FORMAT
        // SONGNUMBER_SONGSET (Example: 1_2 = Song #1 in Set #2
        private bool[] autoHasteEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoHaste_IIEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoFlurryEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoFlurry_IIEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoPhalanx_IIEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoRegen_Enabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoShell_Enabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoProtect_Enabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoSandstormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoRainstormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoWindstormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoFirestormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoHailstormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoThunderstormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoVoidstormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoAurorastormEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoRefreshEnabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private bool[] autoAdloquium_Enabled = new bool[]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };

        private DateTime currentTime = DateTime.Now;

        private DateTime[] playerHaste = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerHaste_II = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerStormspell = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerFlurry = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerFlurry_II = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerShell = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerProtect = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerPhalanx_II = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerRegen = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerRefresh = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerAdloquium = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerSong1 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerSong2 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerSong3 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerSong4 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] Last_SongCast_Timer = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerPianissimo1_1 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerPianissimo2_1 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerPianissimo1_2 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private DateTime[] playerPianissimo2_2 = new DateTime[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0),
            new DateTime(1970, 1, 1, 0, 0, 0)
        };

        private TimeSpan[] playerHasteSpan = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerStormspellSpan = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerHaste_IISpan = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerFlurrySpan = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerFlurry_IISpan = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerShell_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerProtect_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerPhalanx_IISpan = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerRegen_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerRefresh_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerAdloquium_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan()
        };

        private TimeSpan[] playerSong1_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private TimeSpan[] playerSong2_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private TimeSpan[] playerSong3_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private TimeSpan[] playerSong4_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private TimeSpan[] Last_SongCast_Timer_Span = new TimeSpan[]
        {
            new TimeSpan()
        };

        private TimeSpan[] pianissimo1_1_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private TimeSpan[] pianissimo2_1_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private TimeSpan[] pianissimo1_2_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private TimeSpan[] pianissimo2_2_Span = new TimeSpan[]
        {
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
            new TimeSpan(),
        };

        private void PaintBorderlessGroupBox(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            DrawGroupBox(box, e.Graphics, Color.Black, Color.Gray);
        }

        private void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor)
        {
            if (box != null)
            {
                Brush textBrush = new SolidBrush(textColor);
                Brush borderBrush = new SolidBrush(borderColor);
                Pen borderPen = new Pen(borderBrush);
                SizeF strSize = g.MeasureString(box.Text, box.Font);
                Rectangle rect = new Rectangle(box.ClientRectangle.X,
                                           box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                           box.ClientRectangle.Width - 1,
                                           box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

                // Clear text and border
                g.Clear(BackColor);

                // Draw text
                g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

                // Drawing Border
                //Left
                g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                //Right
                g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Bottom
                g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Top1
                g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
                //Top2
                g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));
            }
        }

        private void PaintButton(object sender, PaintEventArgs e)
        {
            Button button = sender as Button;

            button.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
        }


        public CurePleaseForm()
        {


            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();




            currentAction.Text = string.Empty;

            if (File.Exists("debug"))
            {
                debug.Visible = true;
            }

            int position = 0;

            // Buff lists
            known_song_buffs.Add(197);
            known_song_buffs.Add(198);
            known_song_buffs.Add(195);
            known_song_buffs.Add(199);
            known_song_buffs.Add(200);
            known_song_buffs.Add(215);
            known_song_buffs.Add(196);
            known_song_buffs.Add(214);
            known_song_buffs.Add(216);
            known_song_buffs.Add(218);
            known_song_buffs.Add(222);

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne II",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne III",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne IV",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minne",
                song_name = "Knight's Minne V",
                song_position = position,
                buff_id = 197
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet II",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet III",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet IV",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Minuet",
                song_name = "Valor Minuet V",
                song_position = position,
                buff_id = 198
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon II",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon III",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon IV",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon V",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Paeon",
                song_name = "Army's Paeon VI",
                song_position = position,
                buff_id = 195
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Madrigal",
                song_name = "Sword Madrigal",
                song_position = position,
                buff_id = 199
            });
            position++;
            SongInfo.Add(new SongData
            {
                song_type = "Madrigal",
                song_name = "Blade Madrigal",
                song_position = position,
                buff_id = 199
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Prelude",
                song_name = "Hunter's Prelude",
                song_position = position,
                buff_id = 200
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Prelude",
                song_name = "Archer's Prelude",
                song_position = position,
                buff_id = 200
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Sinewy Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Dextrous Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Vivacious Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Quick Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Learned Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Spirited Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Enchanting Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Herculean Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Uncanny Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Vital Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Swift Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Sage Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Logical Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Etude",
                song_name = "Bewitching Etude",
                song_position = position,
                buff_id = 215
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Mambo",
                song_name = "Sheepfoe Mambo",
                song_position = position,
                buff_id = 201
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Mambo",
                song_name = "Dragonfoe Mambo",
                song_position = position,
                buff_id = 201
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Ballad",
                song_name = "Mage's Ballad",
                song_position = position,
                buff_id = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Ballad",
                song_name = "Mage's Ballad II",
                song_position = position,
                buff_id = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Ballad",
                song_name = "Mage's Ballad III",
                song_position = position,
                buff_id = 196
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "March",
                song_name = "Advancing March",
                song_position = position,
                buff_id = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "March",
                song_name = "Victory March",
                song_position = position,
                buff_id = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "March",
                song_name = "Honor March",
                song_position = position,
                buff_id = 214
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Fire Carol",
                song_position = position
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Fire Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Ice Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Ice Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = " Wind Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Wind Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Earth Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Earth Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Lightning Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Lightning Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Water Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Water Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Light Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Light Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Dark Carol",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Carol",
                song_name = "Dark Carol II",
                song_position = position,
                buff_id = 216
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Hymnus",
                song_name = "Godess's Hymnus",
                song_position = position,
                buff_id = 218
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Blank",
                song_name = "Blank",
                song_position = position,
                buff_id = 0
            });
            position++;

            SongInfo.Add(new SongData
            {
                song_type = "Scherzo",
                song_name = "Sentinel's Scherzo",
                song_position = position,
                buff_id = 222
            });
            position++;

            int geo_position = 0;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Voidance",
                geo_spell = "Geo-Voidance",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Precision",
                geo_spell = "Geo-Precision",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Regen",
                geo_spell = "Geo-Regen",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Haste",
                geo_spell = "Geo-Haste",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Attunement",
                geo_spell = "Geo-Attunement",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Focus",
                geo_spell = "Geo-Focus",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Barrier",
                geo_spell = "Geo-Barrier",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Refresh",
                geo_spell = "Geo-Refresh",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-CHR",
                geo_spell = "Geo-CHR",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-MND",
                geo_spell = "Geo-MND",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Fury",
                geo_spell = "Geo-Fury",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-INT",
                geo_spell = "Geo-INT",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-AGI",
                geo_spell = "Geo-AGI",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Fend",
                geo_spell = "Geo-Fend",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-VIT",
                geo_spell = "Geo-VIT",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-DEX",
                geo_spell = "Geo-DEX",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Acumen",
                geo_spell = "Geo-Acumen",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-STR",
                geo_spell = "Geo-STR",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Poison",
                geo_spell = "Geo-Poison",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Slow",
                geo_spell = "Geo-Slow",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Torpor",
                geo_spell = "Geo-Torpor",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Slip",
                geo_spell = "Geo-Slip",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Languor",
                geo_spell = "Geo-Languor",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Paralysis",
                geo_spell = "Geo-Paralysis",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Vex",
                geo_spell = "Geo-Vex",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Frailty",
                geo_spell = "Geo-Frailty",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Wilt",
                geo_spell = "Geo-Wilt",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Malaise",
                geo_spell = "Geo-Malaise",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Gravity",
                geo_spell = "Geo-Gravity",
                geo_position = geo_position,
            });
            geo_position++;

            GeomancerInfo.Add(new GeoData
            {
                indi_spell = "Indi-Fade",
                geo_spell = "Geo-Fade",
                geo_position = geo_position,
            });
            geo_position++;

            barspells.Add(new SpellsData
            {
                Spell_Name = "Barfire",
                type = 1,
                spell_position = 0,
                buffID = 100,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barfira",
                type = 1,
                spell_position = 0,
                buffID = 100,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barstone",
                type = 1,
                spell_position = 1,
                buffID = 103,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barstonra",
                type = 1,
                spell_position = 1,
                buffID = 103,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barwater",
                type = 1,
                spell_position = 2,
                buffID = 105,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barwatera",
                type = 1,
                spell_position = 2,
                buffID = 105,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Baraero",
                type = 1,
                spell_position = 3,
                buffID = 102
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Baraera",
                type = 1,
                spell_position = 3,
                buffID = 102,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblizzard",
                type = 1,
                spell_position = 4,
                buffID = 101
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblizzara",
                type = 1,
                spell_position = 4,
                buffID = 101,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barthunder",
                type = 1,
                spell_position = 5,
                buffID = 104
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barthundra",
                type = 1,
                spell_position = 5,
                buffID = 104,
                aoe_version = true,
            });

            barspells.Add(new SpellsData
            {
                Spell_Name = "Baramnesia",
                type = 2,
                spell_position = 0,
                buffID = 286,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Baramnesra",
                type = 2,
                spell_position = 0,
                buffID = 286,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barvirus",
                type = 2,
                spell_position = 1,
                buffID = 112
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barvira",
                type = 2,
                spell_position = 1,
                buffID = 112,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barparalyze",
                type = 2,
                spell_position = 2,
                buffID = 108
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barparalyzra",
                type = 2,
                spell_position = 2,
                buffID = 108,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsilence",
                type = 2,
                spell_position = 3,
                buffID = 110
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsilencera",
                type = 2,
                spell_position = 3,
                buffID = 110,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpetrify",
                type = 2,
                spell_position = 4,
                buffID = 111
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpetra",
                type = 2,
                spell_position = 4,
                buffID = 111,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpoison",
                type = 2,
                spell_position = 5,
                buffID = 107
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barpoisonra",
                type = 2,
                spell_position = 5,
                buffID = 107,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblind",
                type = 2,
                spell_position = 6,
                buffID = 109
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barblindra",
                type = 2,
                spell_position = 6,
                buffID = 109,
                aoe_version = true,
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsleep",
                type = 2,
                spell_position = 7,
                buffID = 106
            });
            barspells.Add(new SpellsData
            {
                Spell_Name = "Barsleepra",
                type = 2,
                spell_position = 7,
                buffID = 106,
                aoe_version = true,
            });

            enspells.Add(new SpellsData
            {
                Spell_Name = "Enfire",
                type = 1,
                spell_position = 0,
                buffID = 94
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enstone",
                type = 1,
                spell_position = 1,
                buffID = 97
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enwater",
                type = 1,
                spell_position = 2,
                buffID = 99
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enaero",
                type = 1,
                spell_position = 3,
                buffID = 96
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enblizzard",
                type = 1,
                spell_position = 4,
                buffID = 95
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enthunder",
                type = 1,
                spell_position = 5,
                buffID = 98
            });

            enspells.Add(new SpellsData
            {
                Spell_Name = "Enfire II",
                type = 1,
                spell_position = 6,
                buffID = 277
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enstone II",
                type = 1,
                spell_position = 7,
                buffID = 280
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enwater II",
                type = 1,
                spell_position = 8,
                buffID = 282
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enaero II",
                type = 1,
                spell_position = 9,
                buffID = 279
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enblizzard II",
                type = 1,
                spell_position = 10,
                buffID = 278
            });
            enspells.Add(new SpellsData
            {
                Spell_Name = "Enthunder II",
                type = 1,
                spell_position = 11,
                buffID = 281
            });

            stormspells.Add(new SpellsData
            {
                Spell_Name = "Firestorm",
                type = 1,
                spell_position = 0,
                buffID = 178
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Sandstorm",
                type = 1,
                spell_position = 1,
                buffID = 181
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Rainstorm",
                type = 1,
                spell_position = 2,
                buffID = 183
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Windstorm",
                type = 1,
                spell_position = 3,
                buffID = 180
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Hailstorm",
                type = 1,
                spell_position = 4,
                buffID = 179
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Thunderstorm",
                type = 1,
                spell_position = 5,
                buffID = 182
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Voidstorm",
                type = 1,
                spell_position = 6,
                buffID = 185
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Aurorastorm",
                type = 1,
                spell_position = 7,
                buffID = 184
            });

            stormspells.Add(new SpellsData
            {
                Spell_Name = "Firestorm II",
                type = 1,
                spell_position = 8,
                buffID = 589
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Sandstorm II",
                type = 1,
                spell_position = 9,
                buffID = 592
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Rainstorm II",
                type = 1,
                spell_position = 10,
                buffID = 594
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Windstorm II",
                type = 1,
                spell_position = 11,
                buffID = 591
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Hailstorm II",
                type = 1,
                spell_position = 12,
                buffID = 590
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Thunderstorm II",
                type = 1,
                spell_position = 13,
                buffID = 593
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Voidstorm II",
                type = 1,
                spell_position = 14,
                buffID = 596
            });
            stormspells.Add(new SpellsData
            {
                Spell_Name = "Aurorastorm II",
                type = 1,
                spell_position = 15,
                buffID = 595
            });

            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (pol.Count() < 1)
            {
                MessageBox.Show("FFXI not found");
            }
            else
            {
                for (int i = 0; i < pol.Count(); i++)
                {
                    POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    processids.Items.Add(pol.ElementAt(i).Id);
                    activeprocessids.Items.Add(pol.ElementAt(i).Id);
                }
                POLID.SelectedIndex = 0;
                POLID2.SelectedIndex = 0;
                processids.SelectedIndex = 0;
                activeprocessids.SelectedIndex = 0;
            }
            // Show the current version number..
            Text = notifyIcon1.Text = "Cure Please v" + Application.ProductVersion;

            notifyIcon1.BalloonTipTitle = "Cure Please v" + Application.ProductVersion;
            notifyIcon1.BalloonTipText = "CurePlease has been minimized.";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
        }

        private void setinstance_Click(object sender, EventArgs e)
        {
            if (!CheckForDLLFiles())
            {
                MessageBox.Show(
                    "Unable to locate EliteAPI.dll or EliteMMO.API.dll\nMake sure both files are in the same directory as the application",
                    "Error");
                return;
            }

            processids.SelectedIndex = POLID.SelectedIndex;
            activeprocessids.SelectedIndex = POLID.SelectedIndex;
            _ELITEAPIPL = new EliteAPI((int)processids.SelectedItem);
            plLabel.Text = "Selected PL: " + _ELITEAPIPL.Player.Name;
            Text = notifyIcon1.Text = _ELITEAPIPL.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;

            plLabel.ForeColor = Color.Green;
            POLID.BackColor = Color.White;
            plPosition.Enabled = true;
            setinstance2.Enabled = true;
            OptionsForm.config.autoFollowName = string.Empty;

            ForceSongRecast = true;

            foreach (Process dats in Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi")).Where(dats => POLID.Text == dats.MainWindowTitle))
            {
                for (int i = 0; i < dats.Modules.Count; i++)
                {
                    if (dats.Modules[i].FileName.Contains("Ashita.dll"))
                    {
                        AddonManager = new Ashita(_ELITEAPIPL);
                    }
                    else if (dats.Modules[i].FileName.Contains("Hook.dll"))
                    {
                        AddonManager = new Windower(_ELITEAPIPL);
                    }
                }
            }

            if (firstTime_Pause == 0)
            {
                Follow_BGW.RunWorkerAsync();
                AddonReader.RunWorkerAsync();
                firstTime_Pause = 1;
            }

            LoadConfig(sender, e);

            // Wait a milisecond and then load and set the config.
            Thread.Sleep(500);

            LoadLuaAddon();
        }

        private void LoadLuaAddon()
        {
            if (LUA_Plugin_Loaded)
            {
                return;
            }

            if (pauseActions)
            {
                return;
            }

            if (!OptionsForm.config.EnableAddOn)
            {
                return;
            }

            if (_ELITEAPIMonitored == null && _ELITEAPIPL == null)
            {
                return;
            }

            if (AddonManager == null)
            {
                return;
            }

            AddonManager.Load();
            if (OptionsForm.config.enableHotKeys)
            {
                AddonManager.BindHotKeys();
            }

            currentAction.Text = "LUA Addon loaded. ( " + OptionsForm.config.ipAddress + " - " + OptionsForm.config.listeningPort + " )";

            LUA_Plugin_Loaded = true;
        }

        private void LoadConfig(object sender, EventArgs e)
        {
            if (_ELITEAPIPL.Player.MainJob == 0 && _ELITEAPIPL.Player.SubJob == 0)
            {
                return;
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
            if (!File.Exists(path + "/loadSettings"))
            {
                return;
            }

            var mainJob = Enum.Parse(typeof(Job), _ELITEAPIPL.Player.MainJob.ToString()).ToString();
            var subJob = Enum.Parse(typeof(Job), _ELITEAPIPL.Player.SubJob.ToString()).ToString();

            var filename = $"{path}\\{_ELITEAPIPL.Player.Name}_{mainJob}_{subJob}.xml";

            if (!File.Exists(filename))
            {
                filename = $"{path}\\{mainJob}_{subJob}.xml";

                if (!File.Exists(filename))
                {
                    return;
                }
            }

            XmlSerializer mySerializer = new XmlSerializer(typeof(OptionsForm.MySettings));

            StreamReader reader = new StreamReader(filename);
            var config = (OptionsForm.MySettings)mySerializer.Deserialize(reader);

            reader.Close();
            reader.Dispose();

            Form2.updateForm(config);
            Form2.button4_Click(sender, e);
        }

        private void setinstance2_Click(object sender, EventArgs e)
        {
            if (!CheckForDLLFiles())
            {
                MessageBox.Show(
                    "Unable to locate EliteAPI.dll or EliteMMO.API.dll\nMake sure both files are in the same directory as the application",
                    "Error");
                return;
            }
            processids.SelectedIndex = POLID2.SelectedIndex;
            _ELITEAPIMonitored = new EliteAPI((int)processids.SelectedItem);
            monitoredLabel.Text = "Monitoring: " + _ELITEAPIMonitored.Player.Name;
            monitoredLabel.ForeColor = Color.Green;
            POLID2.BackColor = Color.White;
            partyMembersUpdate.Enabled = true;
            actionTimer.Enabled = true;
            pauseButton.Enabled = true;
            hpUpdates.Enabled = true;

            if (OptionsForm.config.pauseOnStartBox)
            {
                pauseActions = true;
                pauseButton.Text = "Loaded, Paused!";
                pauseButton.ForeColor = Color.Red;
                actionTimer.Enabled = false;
            }
            else
            {
                if (OptionsForm.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }
            }

            LoadLuaAddon();
        }

        private bool CheckForDLLFiles()
        {
            return File.Exists("eliteapi.dll")
                && File.Exists("elitemmo.api.dll");
        }

        private string CureTiers(string cureSpell, bool HP)
        {
            if (cureSpell.ToLower() == "cure vi")
            {
                if (CanCastSpell("Cure VI") && CheckSpellRecast("Cure VI") == 0)
                {
                    return "Cure VI";
                }
                else if (CanCastSpell("Cure V") && CheckSpellRecast("Cure V") == 0 && OptionsForm.config.Undercure)
                {
                    return "Cure V";
                }
                else if (CanCastSpell("Cure IV") && CheckSpellRecast("Cure IV") == 0 && OptionsForm.config.Undercure)
                {
                    return "Cure IV";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "cure v")
            {
                if (CanCastSpell("Cure V") && CheckSpellRecast("Cure V") == 0)
                {
                    return "Cure V";
                }
                else if (CanCastSpell("Cure IV") && CheckSpellRecast("Cure IV") == 0 && OptionsForm.config.Undercure)
                {
                    return "Cure IV";
                }
                else if (CanCastSpell("Cure VI") && CheckSpellRecast("Cure VI") == 0 && (OptionsForm.config.Overcure && OptionsForm.config.OvercureOnHighPriority != true || OptionsForm.config.OvercureOnHighPriority && HP == true))
                {
                    return "Cure VI";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "cure iv")
            {
                if (CanCastSpell("Cure IV") && CheckSpellRecast("Cure IV") == 0)
                {
                    return "Cure IV";
                }
                else if (CanCastSpell("Cure III") && CheckSpellRecast("Cure III") == 0 && OptionsForm.config.Undercure)
                {
                    return "Cure III";
                }
                else if (CanCastSpell("Cure V") && CheckSpellRecast("Cure V") == 0 && (OptionsForm.config.Overcure && OptionsForm.config.OvercureOnHighPriority != true || OptionsForm.config.OvercureOnHighPriority && HP == true))
                {
                    return "Cure V";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "cure iii")
            {
                if (CanCastSpell("Cure III") && CheckSpellRecast("Cure III") == 0)
                {
                    return "Cure III";
                }
                else if (CanCastSpell("Cure IV") && CheckSpellRecast("Cure IV") == 0 && (OptionsForm.config.Overcure && OptionsForm.config.OvercureOnHighPriority != true || OptionsForm.config.OvercureOnHighPriority && HP == true))
                {
                    return "Cure IV";
                }
                else if (CanCastSpell("Cure II") && CheckSpellRecast("Cure II") == 0 && OptionsForm.config.Undercure)
                {
                    return "Cure II";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "cure ii")
            {
                if (CanCastSpell("Cure II") && CheckSpellRecast("Cure II") == 0)
                {
                    return "Cure II";
                }
                else if (CanCastSpell("Cure") && CheckSpellRecast("Cure") == 0 && OptionsForm.config.Undercure)
                {
                    return "Cure";
                }
                else if (CanCastSpell("Cure III") && CheckSpellRecast("Cure III") == 0 && (OptionsForm.config.Overcure && OptionsForm.config.OvercureOnHighPriority != true || OptionsForm.config.OvercureOnHighPriority && HP == true))
                {
                    return "Cure III";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "cure")
            {
                if (CanCastSpell("Cure") && CheckSpellRecast("Cure") == 0)
                {
                    return "Cure";
                }
                else if (CanCastSpell("Cure II") && CheckSpellRecast("Cure II") == 0 && (OptionsForm.config.Overcure && OptionsForm.config.OvercureOnHighPriority != true || OptionsForm.config.OvercureOnHighPriority && HP == true))
                {
                    return "Cure II";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "curaga v")
            {
                if (CanCastSpell("Curaga V") && CheckSpellRecast("Curaga V") == 0)
                {
                    return "Curaga V";
                }
                else if (CanCastSpell("Curaga IV") && CheckSpellRecast("Curaga IV") == 0 && OptionsForm.config.Undercure)
                {
                    return "Curaga IV";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "curaga iv")
            {
                if (CanCastSpell("Curaga IV") && CheckSpellRecast("Curaga IV") == 0)
                {
                    return "Curaga IV";
                }
                else if (CanCastSpell("Curaga V") && CheckSpellRecast("Curaga V") == 0 && OptionsForm.config.Overcure)
                {
                    return "Curaga V";
                }
                else if (CanCastSpell("Curaga III") && CheckSpellRecast("Curaga III") == 0 && OptionsForm.config.Undercure)
                {
                    return "Curaga III";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "curaga iii")
            {
                if (CanCastSpell("Curaga III") && CheckSpellRecast("Curaga III") == 0)
                {
                    return "Curaga III";
                }
                else if (CanCastSpell("Curaga IV") && CheckSpellRecast("Curaga IV") == 0 && OptionsForm.config.Overcure)
                {
                    return "Curaga IV";
                }
                else if (CanCastSpell("Curaga II") && CheckSpellRecast("Curaga II") == 0 && OptionsForm.config.Undercure)
                {
                    return "Curaga II";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "curaga ii")
            {
                if (CanCastSpell("Curaga II") && CheckSpellRecast("Curaga II") == 0)
                {
                    return "Curaga II";
                }
                else if (CanCastSpell("Curaga") && CheckSpellRecast("Curaga") == 0 && OptionsForm.config.Undercure)
                {
                    return "Curaga";
                }
                else if (CanCastSpell("Curaga III") && CheckSpellRecast("Curaga III") == 0 && OptionsForm.config.Overcure)
                {
                    return "Curaga III";
                }
                else
                {
                    return "false";
                }
            }
            else if (cureSpell.ToLower() == "curaga")
            {
                if (CanCastSpell("Curaga") && CheckSpellRecast("Curaga") == 0)
                {
                    return "Curaga";
                }
                else if (CanCastSpell("Curaga II") && CheckSpellRecast("Curaga II") == 0 && OptionsForm.config.Overcure)
                {
                    return "Curaga II";
                }
                else
                {
                    return "false";
                }
            }
            return "false";
        }

        private T GetField<T>(string name)
        {
            return (T) GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
        }

        private void UpdatePartyMemberDetails(int index, PartyMember partyMember)
        {
            var fieldPrefix = "player" + index;

            var playerName = GetField<Label>(fieldPrefix);
            var playerHealth = GetField<NewProgressBar>($"{fieldPrefix}HP");
            var playerOptions = GetField<Button>($"{fieldPrefix}optionsButton");

            if (partyMember.Active == 1 && _ELITEAPIPL.Player.ZoneId == partyMember.Zone)
            {
                playerName.Text = partyMember.Name;
                playerName.Enabled = true;
                playerOptions.Enabled = true;
                UpdateHPProgressBar(playerHealth, partyMember.CurrentHPP);

                return;
            }

            playerName.Text = Resources.Form1_partyMembersUpdate_Tick_Inactive;
            playerName.Enabled = false;
            playerHealth.Value = 0;
            playerOptions.Enabled = false;
        }

        private void TogglePartyMemberBuffButton(int index, PartyMember partyMember)
        {
            GetField<Button>($"player{index}buffsButton").Enabled
                = partyMember.Active == 1 && _ELITEAPIPL.Player.ZoneId == partyMember.Zone;
        }

        private async void PartyMembersUpdate_TickAsync(object sender, EventArgs e)
        {
            if (_ELITEAPIPL == null || _ELITEAPIMonitored == null)
            {
                return;
            }

            if (_ELITEAPIPL.Player.LoginStatus == (int)LoginStatus.Loading || _ELITEAPIMonitored.Player.LoginStatus == (int)LoginStatus.Loading)
            {
                if (OptionsForm.config.pauseOnZoneBox == true)
                {
                    song_casting = 0;
                    ForceSongRecast = true;
                    if (pauseActions != true)
                    {
                        pauseButton.Text = "Zoned, paused.";
                        pauseButton.ForeColor = Color.Red;
                        pauseActions = true;
                        actionTimer.Enabled = false;
                    }
                }
                else
                {
                    song_casting = 0;
                    ForceSongRecast = true;

                    if (pauseActions != true)
                    {
                        pauseButton.Text = "Zoned, waiting.";
                        pauseButton.ForeColor = Color.Red;
                        await Task.Delay(100);
                        Thread.Sleep(17000);
                        pauseButton.Text = "Pause";
                        pauseButton.ForeColor = Color.Black;
                    }
                }
                ActiveBuffs.Clear();
            }

            if (_ELITEAPIPL.Player.LoginStatus != (int)LoginStatus.LoggedIn || _ELITEAPIMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }

            var partyMembers = _ELITEAPIMonitored.Party.GetPartyMembers();
            for (int i = 0; i < partyMembers.Count; i++)
            {
                UpdatePartyMemberDetails(i, partyMembers[i]);

                if (i < 6)
                {// Exclude Alliance Members
                    TogglePartyMemberBuffButton(i, partyMembers[i]);
                }
            }
        }

        private void UpdateHPProgressBar(ProgressBar healthBar, int healthPercent)
        {
            healthBar.Value = healthPercent;
            if (healthPercent >= 75)
            {
                healthBar.ForeColor = Color.DarkGreen;
            }
            else if (healthPercent > 50)
            {
                healthBar.ForeColor = Color.Yellow;
            }
            else if (healthPercent > 25)
            {
                healthBar.ForeColor = Color.Orange;
            }
            else
            {
                healthBar.ForeColor = Color.Red;
            }
        }

        private void plPosition_Tick(object sender, EventArgs e)
        {
            if (_ELITEAPIPL == null || _ELITEAPIMonitored == null)
            {
                return;
            }

            if (_ELITEAPIPL.Player.LoginStatus != (int)LoginStatus.LoggedIn || _ELITEAPIMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }

            plX = _ELITEAPIPL.Player.X;
            plY = _ELITEAPIPL.Player.Y;
            plZ = _ELITEAPIPL.Player.Z;
        }

        private void removeDebuff(string characterName, int debuffID)
        {
            lock (ActiveBuffs)
            {
                foreach (BuffStorage ailment in ActiveBuffs)
                {
                    if (ailment.CharacterName.ToLower() == characterName.ToLower())
                    {
                        //MessageBox.Show("Found Match: " + ailment.CharacterName.ToLower()+" => "+characterName.ToLower());

                        // Build a new list, find cast debuff and remove it.
                        List<string> named_Debuffs = ailment.CharacterBuffs.Split(',').ToList();
                        named_Debuffs.Remove(debuffID.ToString());

                        // Now rebuild the list and replace previous one
                        string stringList = string.Join(",", named_Debuffs);

                        int i = ActiveBuffs.FindIndex(x => x.CharacterName.ToLower() == characterName.ToLower());
                        ActiveBuffs[i].CharacterBuffs = stringList;
                    }
                }
            }
        }

        private void CureCalculator_PL(bool HP)
        {
            // FIRST GET HOW MUCH HP IS MISSING FROM THE CURRENT PARTY MEMBER
            if (_ELITEAPIPL.Player.HP > 0)
            {
                uint HP_Loss = (_ELITEAPIPL.Player.HP * 100) / (_ELITEAPIPL.Player.HPP) - (_ELITEAPIPL.Player.HP);

                if (OptionsForm.config.cure6enabled && HP_Loss >= OptionsForm.config.cure6amount && CanCastSpell("Cure VI"))
                {
                    string cureSpell = CureTiers("Cure VI", HP);
                    if (cureSpell != "false")
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, cureSpell);
                    }
                }
                else if (OptionsForm.config.cure5enabled && HP_Loss >= OptionsForm.config.cure5amount && CanCastSpell("Cure V"))
                {
                    string cureSpell = CureTiers("Cure V", HP);
                    if (cureSpell != "false")
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, cureSpell);
                    }
                }
                else if (OptionsForm.config.cure4enabled && HP_Loss >= OptionsForm.config.cure4amount && CanCastSpell("Cure IV"))
                {
                    string cureSpell = CureTiers("Cure IV", HP);
                    if (cureSpell != "false")
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, cureSpell);
                    }
                }
                else if (OptionsForm.config.cure3enabled && HP_Loss >= OptionsForm.config.cure3amount && CanCastSpell("Cure III"))
                {
                    if (OptionsForm.config.PrioritiseOverLowerTier == true) { RunDebuffChecker(); }
                    string cureSpell = CureTiers("Cure III", HP);
                    if (cureSpell != "false")
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, cureSpell);
                    }
                }
                else if (OptionsForm.config.cure2enabled && HP_Loss >= OptionsForm.config.cure2amount && CanCastSpell("Cure II"))
                {
                    if (OptionsForm.config.PrioritiseOverLowerTier == true) { RunDebuffChecker(); }
                    string cureSpell = CureTiers("Cure II", HP);
                    if (cureSpell != "false")
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, cureSpell);
                    }
                }
                else if (OptionsForm.config.cure1enabled && HP_Loss >= OptionsForm.config.cure1amount && CanCastSpell("Cure"))
                {
                    if (OptionsForm.config.PrioritiseOverLowerTier == true) { RunDebuffChecker(); }
                    string cureSpell = CureTiers("Cure", HP);
                    if (cureSpell != "false")
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, cureSpell);
                    }
                }
            }
        }

        private void CureCalculator(PartyMember partyMember, bool HP)
        {
            uint lostHealth = (partyMember.CurrentHP * 100) / (partyMember.CurrentHPP) - (partyMember.CurrentHP);

            var cures = new List<(string name, bool eneabled, int amount)>()
            {
                ("Cure VI",  OptionsForm.config.cure6enabled, OptionsForm.config.cure6amount),
                ("Cure V",   OptionsForm.config.cure5enabled, OptionsForm.config.cure5amount),
                ("Cure IV",  OptionsForm.config.cure4enabled, OptionsForm.config.cure4amount),
                ("Cure III", OptionsForm.config.cure3enabled, OptionsForm.config.cure3amount),
                ("Cure II",  OptionsForm.config.cure2enabled, OptionsForm.config.cure2amount),
                ("Cure",     OptionsForm.config.cure1enabled, OptionsForm.config.cure1amount),
            }
                .Where(s => s.eneabled)
                .Where(s => s.amount <= lostHealth);

            if (!cures.Any())
            {
                return;
            }

            var cureSpell = CureTiers(cures.First().name, HP);

            if (cureSpell == "false")
            {
                return;
            }

            CastSpell(partyMember.Name, cureSpell);
        }

        private void RunDebuffChecker()
        {
            // PL and Monitored Player Debuff Removal Starting with PL
            if (_ELITEAPIPL.Player.Status != 33)
            {
                if (OptionsForm.config.plSilenceItem == 0)
                {
                    plSilenceitemName = "Catholicon";
                }
                else if (OptionsForm.config.plSilenceItem == 1)
                {
                    plSilenceitemName = "Echo Drops";
                }
                else if (OptionsForm.config.plSilenceItem == 2)
                {
                    plSilenceitemName = "Remedy";
                }
                else if (OptionsForm.config.plSilenceItem == 3)
                {
                    plSilenceitemName = "Remedy Ointment";
                }
                else if (OptionsForm.config.plSilenceItem == 4)
                {
                    plSilenceitemName = "Vicar's Drink";
                }

                if (OptionsForm.config.plDoomitem == 0)
                {
                    plDoomItemName = "Holy Water";
                }
                else if (OptionsForm.config.plDoomitem == 1)
                {
                    plDoomItemName = "Hallowed Water";
                }

                if (OptionsForm.config.wakeSleepSpell == 0)
                {
                    wakeSleepSpellName = "Cure";
                }
                else if (OptionsForm.config.wakeSleepSpell == 1)
                {
                    wakeSleepSpellName = "Cura";
                }
                else if (OptionsForm.config.wakeSleepSpell == 2)
                {
                    wakeSleepSpellName = "Curaga";
                }

                foreach (StatusEffect plEffect in _ELITEAPIPL.Player.Buffs)
                {
                    if ((plEffect == StatusEffect.Doom) && (OptionsForm.config.plDoom) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Cursna");
                    }
                    else if ((plEffect == StatusEffect.Paralysis) && (OptionsForm.config.plParalysis) && (CheckSpellRecast("Paralyna") == 0) && CanCastSpell("Paralyna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Paralyna");
                    }
                    else if ((plEffect == StatusEffect.Amnesia) && (OptionsForm.config.plAmnesia) && (CheckSpellRecast("Esuna") == 0) && CanCastSpell("Esuna") && BuffChecker(0, 418))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Esuna");
                    }
                    else if ((plEffect == StatusEffect.Poison) && (OptionsForm.config.plPoison) && (CheckSpellRecast("Poisona") == 0) && CanCastSpell("Poisona"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Poisona");
                    }
                    else if ((plEffect == StatusEffect.Attack_Down) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Blindness) && (OptionsForm.config.plBlindness) && (CheckSpellRecast("Blindna") == 0) && CanCastSpell("Blindna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Blindna");
                    }
                    else if ((plEffect == StatusEffect.Bind) && (OptionsForm.config.plBind) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Weight) && (OptionsForm.config.plWeight) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Slow) && (OptionsForm.config.plSlow) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Curse) && (OptionsForm.config.plCurse) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Cursna");
                    }
                    else if ((plEffect == StatusEffect.Curse2) && (OptionsForm.config.plCurse2) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Cursna");
                    }
                    else if ((plEffect == StatusEffect.Addle) && (OptionsForm.config.plAddle) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Bane) && (OptionsForm.config.plBane) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Cursna");
                    }
                    else if ((plEffect == StatusEffect.Plague) && (OptionsForm.config.plPlague) && (CheckSpellRecast("Viruna") == 0) && CanCastSpell("Viruna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Viruna");
                    }
                    else if ((plEffect == StatusEffect.Disease) && (OptionsForm.config.plDisease) && (CheckSpellRecast("Viruna") == 0) && CanCastSpell("Viruna"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Viruna");
                    }
                    else if ((plEffect == StatusEffect.Burn) && (OptionsForm.config.plBurn) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Frost) && (OptionsForm.config.plFrost) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Choke) && (OptionsForm.config.plChoke) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Rasp) && (OptionsForm.config.plRasp) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Shock) && (OptionsForm.config.plShock) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Drown) && (OptionsForm.config.plDrown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Dia) && (OptionsForm.config.plDia) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Bio) && (OptionsForm.config.plBio) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.STR_Down) && (OptionsForm.config.plStrDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.DEX_Down) && (OptionsForm.config.plDexDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.VIT_Down) && (OptionsForm.config.plVitDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.AGI_Down) && (OptionsForm.config.plAgiDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.INT_Down) && (OptionsForm.config.plIntDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.MND_Down) && (OptionsForm.config.plMndDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.CHR_Down) && (OptionsForm.config.plChrDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Max_HP_Down) && (OptionsForm.config.plMaxHpDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Max_MP_Down) && (OptionsForm.config.plMaxMpDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Accuracy_Down) && (OptionsForm.config.plAccuracyDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Evasion_Down) && (OptionsForm.config.plEvasionDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Defense_Down) && (OptionsForm.config.plDefenseDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Flash) && (OptionsForm.config.plFlash) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Magic_Acc_Down) && (OptionsForm.config.plMagicAccDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Magic_Atk_Down) && (OptionsForm.config.plMagicAtkDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Helix) && (OptionsForm.config.plHelix) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Max_TP_Down) && (OptionsForm.config.plMaxTpDown) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Requiem) && (OptionsForm.config.plRequiem) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Elegy) && (OptionsForm.config.plElegy) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                    else if ((plEffect == StatusEffect.Threnody) && (OptionsForm.config.plThrenody) && (OptionsForm.config.plAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                    {
                        CastSpell(_ELITEAPIPL.Player.Name, "Erase");
                    }
                }
            }

            // Next, we check monitored player
            if ((_ELITEAPIPL.Entity.GetEntity((int)_ELITEAPIMonitored.Party.GetPartyMember(0).TargetIndex).Distance < 21) && (_ELITEAPIPL.Entity.GetEntity((int)_ELITEAPIMonitored.Party.GetPartyMember(0).TargetIndex).Distance > 0) && (_ELITEAPIMonitored.Player.HP > 0) && _ELITEAPIPL.Player.Status != 33)
            {
                foreach (StatusEffect monitoredEffect in _ELITEAPIMonitored.Player.Buffs)
                {
                    if ((monitoredEffect == StatusEffect.Doom) && (OptionsForm.config.monitoredDoom) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Cursna");
                    }
                    else if ((monitoredEffect == StatusEffect.Sleep) && (OptionsForm.config.monitoredSleep) && (OptionsForm.config.wakeSleepEnabled))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, wakeSleepSpellName);
                    }
                    else if ((monitoredEffect == StatusEffect.Sleep2) && (OptionsForm.config.monitoredSleep2) && (OptionsForm.config.wakeSleepEnabled))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, wakeSleepSpellName);
                    }
                    else if ((monitoredEffect == StatusEffect.Silence) && (OptionsForm.config.monitoredSilence) && (CheckSpellRecast("Silena") == 0) && CanCastSpell("Silena"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Silena");
                    }
                    else if ((monitoredEffect == StatusEffect.Petrification) && (OptionsForm.config.monitoredPetrification) && (CheckSpellRecast("Stona") == 0) && CanCastSpell("Stona"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Stona");
                    }
                    else if ((monitoredEffect == StatusEffect.Paralysis) && (OptionsForm.config.monitoredParalysis) && (CheckSpellRecast("Paralyna") == 0) && CanCastSpell("Paralyna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Paralyna");
                    }
                    else if ((monitoredEffect == StatusEffect.Amnesia) && (OptionsForm.config.monitoredAmnesia) && (CheckSpellRecast("Esuna") == 0) && CanCastSpell("Esuna") && BuffChecker(0, 418))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Esuna");
                    }
                    else if ((monitoredEffect == StatusEffect.Poison) && (OptionsForm.config.monitoredPoison) && (CheckSpellRecast("Poisona") == 0) && CanCastSpell("Poisona"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Poisona");
                    }
                    else if ((monitoredEffect == StatusEffect.Attack_Down) && (OptionsForm.config.monitoredAttackDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Blindness) && (OptionsForm.config.monitoredBlindness) && (CheckSpellRecast("Blindna") == 0) && CanCastSpell("Blindna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Blindna");
                    }
                    else if ((monitoredEffect == StatusEffect.Bind) && (OptionsForm.config.monitoredBind) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Weight) && (OptionsForm.config.monitoredWeight) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Slow) && (OptionsForm.config.monitoredSlow) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Curse) && (OptionsForm.config.monitoredCurse) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Cursna");
                    }
                    else if ((monitoredEffect == StatusEffect.Curse2) && (OptionsForm.config.monitoredCurse2) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Cursna");
                    }
                    else if ((monitoredEffect == StatusEffect.Addle) && (OptionsForm.config.monitoredAddle) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Bane) && (OptionsForm.config.monitoredBane) && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Cursna");
                    }
                    else if ((monitoredEffect == StatusEffect.Plague) && (OptionsForm.config.monitoredPlague) && (CheckSpellRecast("Viruna") == 0) && CanCastSpell("Viruna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Viruna");
                    }
                    else if ((monitoredEffect == StatusEffect.Disease) && (OptionsForm.config.monitoredDisease) && (CheckSpellRecast("Viruna") == 0) && CanCastSpell("Viruna"))
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Viruna");
                    }
                    else if ((monitoredEffect == StatusEffect.Burn) && (OptionsForm.config.monitoredBurn) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Frost) && (OptionsForm.config.monitoredFrost) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Choke) && (OptionsForm.config.monitoredChoke) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Rasp) && (OptionsForm.config.monitoredRasp) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Shock) && (OptionsForm.config.monitoredShock) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Drown) && (OptionsForm.config.monitoredDrown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Dia) && (OptionsForm.config.monitoredDia) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Bio) && (OptionsForm.config.monitoredBio) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.STR_Down) && (OptionsForm.config.monitoredStrDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.DEX_Down) && (OptionsForm.config.monitoredDexDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.VIT_Down) && (OptionsForm.config.monitoredVitDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.AGI_Down) && (OptionsForm.config.monitoredAgiDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.INT_Down) && (OptionsForm.config.monitoredIntDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.MND_Down) && (OptionsForm.config.monitoredMndDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.CHR_Down) && (OptionsForm.config.monitoredChrDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Max_HP_Down) && (OptionsForm.config.monitoredMaxHpDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Max_MP_Down) && (OptionsForm.config.monitoredMaxMpDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Accuracy_Down) && (OptionsForm.config.monitoredAccuracyDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Evasion_Down) && (OptionsForm.config.monitoredEvasionDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Defense_Down) && (OptionsForm.config.monitoredDefenseDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Flash) && (OptionsForm.config.monitoredFlash) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Magic_Acc_Down) && (OptionsForm.config.monitoredMagicAccDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Magic_Atk_Down) && (OptionsForm.config.monitoredMagicAtkDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Helix) && (OptionsForm.config.monitoredHelix) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Max_TP_Down) && (OptionsForm.config.monitoredMaxTpDown) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Requiem) && (OptionsForm.config.monitoredRequiem) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Elegy) && (OptionsForm.config.monitoredElegy) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                    else if ((monitoredEffect == StatusEffect.Threnody) && (OptionsForm.config.monitoredThrenody) && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase") && plMonitoredSameParty() == true)
                    {
                        CastSpell(_ELITEAPIMonitored.Player.Name, "Erase");
                    }
                }
            }
            // End MONITORED Debuff Removal


            if (OptionsForm.config.EnableAddOn)
            {
                int BreakOut = 0;

                List<EliteAPI.PartyMember> partyMembers = _ELITEAPIPL.Party.GetPartyMembers();

                List<BuffStorage> generated_base_list = ActiveBuffs.ToList();

                lock (generated_base_list)
                {

                    foreach (BuffStorage ailment in generated_base_list)
                    {
                        foreach (EliteAPI.PartyMember ptMember in partyMembers)
                        {
                            if (ailment != null && ptMember != null)
                            {
                                if (ailment.CharacterName != null && ptMember.Name != null && ailment.CharacterName.ToLower() == ptMember.Name.ToLower())
                                {
                                    if (ailment.CharacterBuffs != null)
                                    {
                                        List<string> named_Debuffs = ailment.CharacterBuffs.Split(',').ToList();

                                        if (named_Debuffs != null && named_Debuffs.Count() != 0)
                                        {
                                            named_Debuffs = named_Debuffs.Select(t => t.Trim()).ToList();


                                            // IF SLOW IS NOT ACTIVE, YET NEITHER IS HASTE / FLURRY DESPITE BEING ENABLED
                                            // RESET THE TIMER TO FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "13") && !DebuffContains(named_Debuffs, "33") && !DebuffContains(named_Debuffs, "265") && !DebuffContains(named_Debuffs, "562"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerHaste[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                    playerHaste_II[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                    playerFlurry[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                    playerFlurry_II[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                }
                                            }
                                            // IF SUBLIMATION IS NOT ACTIVE, YET NEITHER IS REFRESH DESPITE BEING
                                            // ENABLED RESET THE TIMER TO FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "187") && !DebuffContains(named_Debuffs, "188") && !DebuffContains(named_Debuffs, "43"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerRefresh[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);  // ERROR
                                                }
                                            }
                                            // IF REGEN IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER TO
                                            // FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "42"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerRegen[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                }
                                            }
                                            // IF PROTECT IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER TO
                                            // FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "40"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerProtect[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                }
                                            }

                                            // IF SHELL IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER TO
                                            // FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "41"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerShell[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                }
                                            }
                                            // IF PHALANX II IS NOT ACTIVE DESPITE BEING ENABLED RESET THE TIMER
                                            // TO FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "116"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerPhalanx_II[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                }

                                            }
                                            // IF NO STORM SPELL IS ACTIVE DESPITE BEING ENABLED RESET THE TIMER
                                            // TO FORCE IT TO BE CAST
                                            if (!DebuffContains(named_Debuffs, "178") && !DebuffContains(named_Debuffs, "179") && !DebuffContains(named_Debuffs, "180") && !DebuffContains(named_Debuffs, "181") &&
                                                !DebuffContains(named_Debuffs, "182") && !DebuffContains(named_Debuffs, "183") && !DebuffContains(named_Debuffs, "184") && !DebuffContains(named_Debuffs, "185") &&
                                                !DebuffContains(named_Debuffs, "589") && !DebuffContains(named_Debuffs, "590") && !DebuffContains(named_Debuffs, "591") && !DebuffContains(named_Debuffs, "592") &&
                                                !DebuffContains(named_Debuffs, "593") && !DebuffContains(named_Debuffs, "594") && !DebuffContains(named_Debuffs, "595") && !DebuffContains(named_Debuffs, "596"))
                                            {
                                                if (ptMember != null)
                                                {
                                                    playerStormspell[ptMember.MemberNumber] = new DateTime(1970, 1, 1, 0, 0, 0);
                                                }
                                            }


                                            // ==============================================================================================================================================================================
                                            // PARTY DEBUFF REMOVAL



                                            string character_name = ailment.CharacterName.ToLower();

                                            if (OptionsForm.config.enablePartyDebuffRemoval && !string.IsNullOrEmpty(character_name) && (characterNames_naRemoval.Contains(character_name) || OptionsForm.config.SpecifiednaSpellsenable == false))
                                            {
                                                //DOOM
                                                if (OptionsForm.config.naCurse && DebuffContains(named_Debuffs, "15") && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                                                {
                                                    CastSpell(ptMember.Name, "Cursna");
                                                    BreakOut = 1;
                                                }
                                                //SLEEP
                                                else if (DebuffContains(named_Debuffs, "2") && (CheckSpellRecast(wakeSleepSpellName) == 0) && (CanCastSpell(wakeSleepSpellName)))
                                                {
                                                    CastSpell(ptMember.Name, wakeSleepSpellName);
                                                    removeDebuff(ptMember.Name, 2);
                                                    BreakOut = 1;
                                                }
                                                //SLEEP 2
                                                else if (DebuffContains(named_Debuffs, "19") && (CheckSpellRecast(wakeSleepSpellName) == 0) && (CanCastSpell(wakeSleepSpellName)))
                                                {
                                                    CastSpell(ptMember.Name, wakeSleepSpellName);
                                                    removeDebuff(ptMember.Name, 19);
                                                    BreakOut = 1;
                                                }
                                                //PETRIFICATION
                                                else if (OptionsForm.config.naPetrification && DebuffContains(named_Debuffs, "7") && (CheckSpellRecast("Stona") == 0) && CanCastSpell("Stona"))
                                                {
                                                    CastSpell(ptMember.Name, "Stona");
                                                    removeDebuff(ptMember.Name, 7);
                                                    BreakOut = 1;
                                                }
                                                //SILENCE
                                                else if (OptionsForm.config.naSilence && DebuffContains(named_Debuffs, "6") && (CheckSpellRecast("Silena") == 0) && CanCastSpell("Silena"))
                                                {
                                                    CastSpell(ptMember.Name, "Silena");
                                                    removeDebuff(ptMember.Name, 6);
                                                    BreakOut = 1;
                                                }
                                                //PARALYSIS
                                                else if (OptionsForm.config.naParalysis && DebuffContains(named_Debuffs, "4") && (CheckSpellRecast("Paralyna") == 0) && CanCastSpell("Paralyna"))
                                                {
                                                    CastSpell(ptMember.Name, "Paralyna");
                                                    removeDebuff(ptMember.Name, 4);
                                                    BreakOut = 1;
                                                }
                                                // PLAGUE
                                                else if (OptionsForm.config.naDisease && DebuffContains(named_Debuffs, "31") && (CheckSpellRecast("Viruna") == 0) && CanCastSpell("Viruna"))
                                                {
                                                    CastSpell(ptMember.Name, "Viruna");
                                                    removeDebuff(ptMember.Name, 31);
                                                    BreakOut = 1;
                                                }
                                                //DISEASE
                                                else if (OptionsForm.config.naDisease && DebuffContains(named_Debuffs, "8") && (CheckSpellRecast("Viruna") == 0) && CanCastSpell("Viruna"))
                                                {
                                                    CastSpell(ptMember.Name, "Viruna");
                                                    removeDebuff(ptMember.Name, 8);
                                                    BreakOut = 1;
                                                }
                                                // AMNESIA
                                                else if (OptionsForm.config.Esuna && DebuffContains(named_Debuffs, "16") && (CheckSpellRecast("Esuna") == 0) && CanCastSpell("Esuna") && BuffChecker(1, 418))
                                                {
                                                    CastSpell(ptMember.Name, "Esuna");
                                                    removeDebuff(ptMember.Name, 16);
                                                    BreakOut = 1;
                                                }
                                                //CURSE
                                                else if (OptionsForm.config.naCurse && DebuffContains(named_Debuffs, "9") && (CheckSpellRecast("Cursna") == 0) && CanCastSpell("Cursna"))
                                                {
                                                    CastSpell(ptMember.Name, "Cursna");
                                                    removeDebuff(ptMember.Name, 9);
                                                    BreakOut = 1;
                                                }
                                                //BLINDNESS
                                                else if (OptionsForm.config.naBlindness && DebuffContains(named_Debuffs, "5") && (CheckSpellRecast("Blindna") == 0) && CanCastSpell("Blindna"))
                                                {
                                                    CastSpell(ptMember.Name, "Blindna");
                                                    removeDebuff(ptMember.Name, 5);
                                                    BreakOut = 1;
                                                }
                                                //POISON
                                                else if (OptionsForm.config.naPoison && DebuffContains(named_Debuffs, "3") && (CheckSpellRecast("Poisona") == 0) && CanCastSpell("Poisona"))
                                                {
                                                    CastSpell(ptMember.Name, "Poisona");
                                                    removeDebuff(ptMember.Name, 3);
                                                    BreakOut = 1;
                                                }
                                                // SLOW
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Slow && DebuffContains(named_Debuffs, "13") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Slow → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 13);
                                                    BreakOut = 1;
                                                }
                                                // BIO
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Bio && DebuffContains(named_Debuffs, "135") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Bio → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 135);
                                                    BreakOut = 1;
                                                }
                                                // BIND
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Bind && DebuffContains(named_Debuffs, "11") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Bind → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 11);
                                                    BreakOut = 1;
                                                }
                                                // GRAVITY
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Weight && DebuffContains(named_Debuffs, "12") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Gravity → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 12);
                                                    BreakOut = 1;
                                                }
                                                // ACCURACY DOWN
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_AccuracyDown && DebuffContains(named_Debuffs, "146") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Acc. Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 146);
                                                    BreakOut = 1;
                                                }
                                                // DEFENSE DOWN
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_DefenseDown && DebuffContains(named_Debuffs, "149") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Def. Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 149);
                                                    BreakOut = 1;
                                                }
                                                // MAGIC DEF DOWN
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MagicDefenseDown && DebuffContains(named_Debuffs, "167") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Mag. Def. Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 167);
                                                    BreakOut = 1;
                                                }
                                                // ATTACK DOWN
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_AttackDown && DebuffContains(named_Debuffs, "147") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Attk. Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 147);
                                                    BreakOut = 1;
                                                }
                                                // HP DOWN
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MaxHpDown && DebuffContains(named_Debuffs, "144") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "HP Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 144);
                                                    BreakOut = 1;
                                                }
                                                // VIT Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_VitDown && DebuffContains(named_Debuffs, "138") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "VIT Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 138);
                                                    BreakOut = 1;
                                                }
                                                // Threnody
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Threnody && DebuffContains(named_Debuffs, "217") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Threnody → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 217);
                                                    BreakOut = 1;
                                                }
                                                // Shock
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Shock && DebuffContains(named_Debuffs, "132") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Shock → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 132);
                                                    BreakOut = 1;
                                                }
                                                // StrDown
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_StrDown && DebuffContains(named_Debuffs, "136") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "STR Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 136);
                                                    BreakOut = 1;
                                                }
                                                // Requiem
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Requiem && DebuffContains(named_Debuffs, "192") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Requiem → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 192);
                                                    BreakOut = 1;
                                                }
                                                // Rasp
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Rasp && DebuffContains(named_Debuffs, "131") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Rasp → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 131);
                                                    BreakOut = 1;
                                                }
                                                // Max TP Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MaxTpDown && DebuffContains(named_Debuffs, "189") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Max TP Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 189);
                                                    BreakOut = 1;
                                                }
                                                // Max MP Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MaxMpDown && DebuffContains(named_Debuffs, "145") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Max MP Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 145);
                                                    BreakOut = 1;
                                                }
                                                // Magic Attack Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MagicAttackDown && DebuffContains(named_Debuffs, "175") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Mag. Atk. Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 175);
                                                    BreakOut = 1;
                                                }
                                                // Magic Acc Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MagicAccDown && DebuffContains(named_Debuffs, "174") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Mag. Acc. Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 174);
                                                    BreakOut = 1;
                                                }
                                                // Mind Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_MndDown && DebuffContains(named_Debuffs, "141") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "MND Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 141);
                                                    BreakOut = 1;
                                                }
                                                // Int Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_IntDown && DebuffContains(named_Debuffs, "140") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "INT Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 140);
                                                    BreakOut = 1;
                                                }
                                                // Helix
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Helix && DebuffContains(named_Debuffs, "186") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Helix → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 186);
                                                    BreakOut = 1;
                                                }
                                                // Frost
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Frost && DebuffContains(named_Debuffs, "129") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Frost → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 129);
                                                    BreakOut = 1;
                                                }
                                                // EvasionDown
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_EvasionDown && DebuffContains(named_Debuffs, "148") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Evasion Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 148);
                                                    BreakOut = 1;
                                                }
                                                // ELEGY
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Elegy && DebuffContains(named_Debuffs, "194") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Elegy → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 194);
                                                    BreakOut = 1;
                                                }
                                                // Drown
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Drown && DebuffContains(named_Debuffs, "133") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Drown → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 133);
                                                    BreakOut = 1;
                                                }
                                                // Dia
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Dia && DebuffContains(named_Debuffs, "134") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Dia → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 134);
                                                    BreakOut = 1;
                                                }
                                                // DexDown
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_DexDown && DebuffContains(named_Debuffs, "137") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "DEX Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 137);
                                                    BreakOut = 1;
                                                }
                                                // Choke
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Choke && DebuffContains(named_Debuffs, "130") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Choke → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 130);
                                                    BreakOut = 1;
                                                }
                                                // ChrDown
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_ChrDown && DebuffContains(named_Debuffs, "142") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "CHR Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 142);
                                                    BreakOut = 1;
                                                }
                                                // Burn
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Burn && DebuffContains(named_Debuffs, "128") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Burn → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 128);
                                                    BreakOut = 1;
                                                }
                                                // Addle
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_Addle && DebuffContains(named_Debuffs, "21") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "Addle → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 21);
                                                    BreakOut = 1;
                                                }
                                                // AGI Down
                                                else if (OptionsForm.config.naErase == true && OptionsForm.config.na_AgiDown && DebuffContains(named_Debuffs, "139") && (CheckSpellRecast("Erase") == 0) && CanCastSpell("Erase"))
                                                {
                                                    CastSpell(ptMember.Name, "Erase", "AGI Down → " + ptMember.Name);
                                                    removeDebuff(ptMember.Name, 139);
                                                    BreakOut = 1;
                                                }
                                            }
                                        }


                                    }


                                }

                                if (BreakOut == 1)
                                {
                                    break;
                                }
                            }
                        }
                    } // Closing FOREACH base_list
                }// Closing LOCK
            }
        }

        private bool DebuffContains(List<string> Debuff_list, string Checked_id)
        {
            if (Debuff_list == null)
            {
                return false;
            }

            return Debuff_list.Any(x => x == Checked_id);
        }

        private void CuragaCalculator(PartyMember partyMember)
        {
            string lowestHP_Name = partyMember.Name;
            var partyMemberMaxHealth = ((partyMember.CurrentHP * 100) / partyMember.CurrentHPP) - partyMember.CurrentHP;

            var canAccession = OptionsForm.config.Accession
                && OptionsForm.config.accessionCure
                && HasAbility("Accession")
                && currentSCHCharges >= 1
                && (plStatusCheck(StatusEffect.Light_Arts) || plStatusCheck(StatusEffect.Addendum_White));

            var caragaSpells = CuragaSpells()
                .Where(s => s.accession && canAccession)
                .Where(s => s.cureAmount <= partyMemberMaxHealth)
                .Where(s => CanCastSpell(s.name));

            if (!caragaSpells.Any())
            {
                return;
            }

            var cureSpell = CureTiers(caragaSpells.First().name, false);

            if (cureSpell == "false")
            {
                return;
            }

            if (OptionsForm.config.curagaTargetType == 0)
            {
                CastSpell(lowestHP_Name, cureSpell);
                return;
            }

            CastSpell(OptionsForm.config.curagaTargetName, cureSpell);
        }

        private IEnumerable<(string name, bool eneabled, bool accession, int cureAmount)> CuragaSpells()
        {
            return new List<(string name, bool eneabled, bool accession, int cureAmount)>()
            {
                ("Curaga V",   OptionsForm.config.curaga5enabled, false, OptionsForm.config.curaga5Amount),
                ("Curaga IV",  OptionsForm.config.curaga4enabled, false, OptionsForm.config.curaga4Amount),
                ("Cure IV",    OptionsForm.config.curaga4enabled, true,  OptionsForm.config.cure4amount),
                ("Curaga III", OptionsForm.config.curaga3enabled, false, OptionsForm.config.curaga3Amount),
                ("Cure III",   OptionsForm.config.curaga3enabled, true,  OptionsForm.config.cure3amount),
                ("Curaga II",  OptionsForm.config.curaga2enabled, false, OptionsForm.config.curaga2Amount),
                ("Cure II",    OptionsForm.config.curaga2enabled, true,  OptionsForm.config.cure2amount),
                ("Curaga",     OptionsForm.config.curagaEnabled,  false, OptionsForm.config.curagaAmount),
                ("Cure",       OptionsForm.config.curagaEnabled,  true,  OptionsForm.config.cure1amount),
            }
                .OrderBy(s => s.cureAmount)
                .Where(s => s.eneabled);
        }

        private bool CanCastOnPartyMember(PartyMember partyMember)
        {
            var entity = _ELITEAPIPL.Entity.GetEntity((int)partyMember.TargetIndex);

            if (partyMember.CurrentHP < 1)
            {// Player is KO
                return false;
            }

            if (_ELITEAPIPL.Player.TargetID == partyMember.ID)
            {// PL healing themself
                return true;
            }

            if (entity.Distance < 0)
            {// Player greater than 50 yalms
                return false;
            }

            // Player in spell casting range
            return entity.Distance < 21;
        }

        private bool CanCastOnPartyMember(int index)
        {
            return CanCastOnPartyMember(_ELITEAPIMonitored.Party.GetPartyMember(index));
        }

        private bool plStatusCheck(StatusEffect requestedStatus)
        {
            return _ELITEAPIPL.Player.Buffs
                .Cast<StatusEffect>()
                .Any(status => requestedStatus == status);
        }

        private bool monitoredStatusCheck(StatusEffect requestedStatus)
        {
            return _ELITEAPIMonitored.Player.Buffs
                .Cast<StatusEffect>()
                .Any(status => requestedStatus == status);
        }

        public bool BuffChecker(int buffID, int checkedPlayer)
        {
            if (checkedPlayer == 1)
            {
                return _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Any(b => b == buffID);
            }

            return _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Any(b => b == buffID);
        }

        private void CastSpell(string partyMemberName, string spellName, [Optional] string OptionalExtras)
        {
            if (CastingBackground_Check)
            {
                return;
            }

            var spell = _ELITEAPIPL.Resources.GetSpell(spellName.Trim(), 0);

            castingSpell = spell.Name[0];

            _ELITEAPIPL.ThirdParty.SendString("/ma \"" + castingSpell + "\" " + partyMemberName);

            currentAction.Text = "Casting: " + castingSpell;

            if (OptionalExtras != null)
            {
                currentAction.Text += " [" + OptionalExtras + "]";
            }

            CastingBackground_Check = true;
            if (!ProtectCasting.IsBusy) 
            { 
                ProtectCasting.RunWorkerAsync(); 
            }

            if (!OptionsForm.config.trackCastingPackets && !OptionsForm.config.EnableAddOn)
            {
                castingLockLabel.Text = "Casting is LOCKED";
            }
        }

        private void hastePlayer(byte partyMemberId)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Haste");
            playerHaste[partyMemberId] = DateTime.Now;
        }

        private void haste_IIPlayer(byte partyMemberId)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Haste II");
            playerHaste_II[partyMemberId] = DateTime.Now;
        }

        private void AdloquiumPlayer(byte partyMemberId)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Adloquium");
            playerAdloquium[partyMemberId] = DateTime.Now;
        }

        private void FlurryPlayer(byte partyMemberId)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Flurry");
            playerFlurry[partyMemberId] = DateTime.Now;
        }

        private void Flurry_IIPlayer(byte partyMemberId)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Flurry II");
            playerFlurry_II[partyMemberId] = DateTime.Now;
        }

        private void Phalanx_IIPlayer(byte partyMemberId)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, "Phalanx II");
            playerPhalanx_II[partyMemberId] = DateTime.Now;
        }

        private void StormSpellPlayer(byte partyMemberId, string Spell)
        {
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, Spell);
            playerStormspell[partyMemberId] = DateTime.Now;
        }

        private void Regen_Player(byte partyMemberId)
        {
            string[] regen_spells = { "Regen", "Regen II", "Regen III", "Regen IV", "Regen V" };
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, regen_spells[OptionsForm.config.autoRegen_Spell]);
            playerRegen[partyMemberId] = DateTime.Now;
        }

        private void Refresh_Player(byte partyMemberId)
        {
            string[] refresh_spells = { "Refresh", "Refresh II", "Refresh III" };
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, refresh_spells[OptionsForm.config.autoRefresh_Spell]);
            playerRefresh[partyMemberId] = DateTime.Now;
        }

        private void protectPlayer(byte partyMemberId)
        {
            string[] protect_spells = { "Protect", "Protect II", "Protect III", "Protect IV", "Protect V" };
            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, protect_spells[OptionsForm.config.autoProtect_Spell]);
            playerProtect[partyMemberId] = DateTime.Now;
        }

        private void shellPlayer(byte partyMemberId)
        {
            string[] shell_spells = { "Shell", "Shell II", "Shell III", "Shell IV", "Shell V" };

            CastSpell(_ELITEAPIMonitored.Party.GetPartyMembers()[partyMemberId].Name, shell_spells[OptionsForm.config.autoShell_Spell]);
            playerShell[partyMemberId] = DateTime.Now;
        }

        private bool ActiveSpikes()
        {
            if ((OptionsForm.config.plSpikes_Spell == 0) && plStatusCheck(StatusEffect.Blaze_Spikes))
            {
                return true;
            }
            else if ((OptionsForm.config.plSpikes_Spell == 1) && plStatusCheck(StatusEffect.Ice_Spikes))
            {
                return true;
            }
            else if ((OptionsForm.config.plSpikes_Spell == 2) && plStatusCheck(StatusEffect.Shock_Spikes))
            {
                return true;
            }
            return false;
        }

        private bool PLInParty()
        {
            // FALSE IS WANTED WHEN NOT IN PARTY

            if (_ELITEAPIPL.Player.TargetID == _ELITEAPIMonitored.Player.TargetID)
            {// PL and Monitored are the same
                return true;
            }

            return _ELITEAPIPL.Party.GetPartyMembers().Any(partyMember => partyMember.ID == _ELITEAPIPL.Player.TargetID);
        }

        private async void ActionTimer_TickAsync(object sender, EventArgs e)
        {
            if (_ELITEAPIPL == null || _ELITEAPIMonitored == null)
            {
                return;
            }

            if (_ELITEAPIPL.Player.LoginStatus != (int)LoginStatus.LoggedIn || _ELITEAPIMonitored.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                return;
            }

            if (JobAbilityLock_Check || CastingBackground_Check)
            {// Action already happening
                return;
            }

            // Grab current time for calculations below

            currentTime = DateTime.Now;
            // Calculate time since haste was cast on particular player
            playerHasteSpan[0] = currentTime.Subtract(playerHaste[0]);
            playerHasteSpan[1] = currentTime.Subtract(playerHaste[1]);
            playerHasteSpan[2] = currentTime.Subtract(playerHaste[2]);
            playerHasteSpan[3] = currentTime.Subtract(playerHaste[3]);
            playerHasteSpan[4] = currentTime.Subtract(playerHaste[4]);
            playerHasteSpan[5] = currentTime.Subtract(playerHaste[5]);
            playerHasteSpan[6] = currentTime.Subtract(playerHaste[6]);
            playerHasteSpan[7] = currentTime.Subtract(playerHaste[7]);
            playerHasteSpan[8] = currentTime.Subtract(playerHaste[8]);
            playerHasteSpan[9] = currentTime.Subtract(playerHaste[9]);
            playerHasteSpan[10] = currentTime.Subtract(playerHaste[10]);
            playerHasteSpan[11] = currentTime.Subtract(playerHaste[11]);
            playerHasteSpan[12] = currentTime.Subtract(playerHaste[12]);
            playerHasteSpan[13] = currentTime.Subtract(playerHaste[13]);
            playerHasteSpan[14] = currentTime.Subtract(playerHaste[14]);
            playerHasteSpan[15] = currentTime.Subtract(playerHaste[15]);
            playerHasteSpan[16] = currentTime.Subtract(playerHaste[16]);
            playerHasteSpan[17] = currentTime.Subtract(playerHaste[17]);

            playerHaste_IISpan[0] = currentTime.Subtract(playerHaste_II[0]);
            playerHaste_IISpan[1] = currentTime.Subtract(playerHaste_II[1]);
            playerHaste_IISpan[2] = currentTime.Subtract(playerHaste_II[2]);
            playerHaste_IISpan[3] = currentTime.Subtract(playerHaste_II[3]);
            playerHaste_IISpan[4] = currentTime.Subtract(playerHaste_II[4]);
            playerHaste_IISpan[5] = currentTime.Subtract(playerHaste_II[5]);
            playerHaste_IISpan[6] = currentTime.Subtract(playerHaste_II[6]);
            playerHaste_IISpan[7] = currentTime.Subtract(playerHaste_II[7]);
            playerHaste_IISpan[8] = currentTime.Subtract(playerHaste_II[8]);
            playerHaste_IISpan[9] = currentTime.Subtract(playerHaste_II[9]);
            playerHaste_IISpan[10] = currentTime.Subtract(playerHaste_II[10]);
            playerHaste_IISpan[11] = currentTime.Subtract(playerHaste_II[11]);
            playerHaste_IISpan[12] = currentTime.Subtract(playerHaste_II[12]);
            playerHaste_IISpan[13] = currentTime.Subtract(playerHaste_II[13]);
            playerHaste_IISpan[14] = currentTime.Subtract(playerHaste_II[14]);
            playerHaste_IISpan[15] = currentTime.Subtract(playerHaste_II[15]);
            playerHaste_IISpan[16] = currentTime.Subtract(playerHaste_II[16]);
            playerHaste_IISpan[17] = currentTime.Subtract(playerHaste_II[17]);

            playerFlurrySpan[0] = currentTime.Subtract(playerFlurry[0]);
            playerFlurrySpan[1] = currentTime.Subtract(playerFlurry[1]);
            playerFlurrySpan[2] = currentTime.Subtract(playerFlurry[2]);
            playerFlurrySpan[3] = currentTime.Subtract(playerFlurry[3]);
            playerFlurrySpan[4] = currentTime.Subtract(playerFlurry[4]);
            playerFlurrySpan[5] = currentTime.Subtract(playerFlurry[5]);
            playerFlurrySpan[6] = currentTime.Subtract(playerFlurry[6]);
            playerFlurrySpan[7] = currentTime.Subtract(playerFlurry[7]);
            playerFlurrySpan[8] = currentTime.Subtract(playerFlurry[8]);
            playerFlurrySpan[9] = currentTime.Subtract(playerFlurry[9]);
            playerFlurrySpan[10] = currentTime.Subtract(playerFlurry[10]);
            playerFlurrySpan[11] = currentTime.Subtract(playerFlurry[11]);
            playerFlurrySpan[12] = currentTime.Subtract(playerFlurry[12]);
            playerFlurrySpan[13] = currentTime.Subtract(playerFlurry[13]);
            playerFlurrySpan[14] = currentTime.Subtract(playerFlurry[14]);
            playerFlurrySpan[15] = currentTime.Subtract(playerFlurry[15]);
            playerFlurrySpan[16] = currentTime.Subtract(playerFlurry[16]);
            playerFlurrySpan[17] = currentTime.Subtract(playerFlurry[17]);

            playerFlurry_IISpan[0] = currentTime.Subtract(playerFlurry_II[0]);
            playerFlurry_IISpan[1] = currentTime.Subtract(playerFlurry_II[1]);
            playerFlurry_IISpan[2] = currentTime.Subtract(playerFlurry_II[2]);
            playerFlurry_IISpan[3] = currentTime.Subtract(playerFlurry_II[3]);
            playerFlurry_IISpan[4] = currentTime.Subtract(playerFlurry_II[4]);
            playerFlurry_IISpan[5] = currentTime.Subtract(playerFlurry_II[5]);
            playerFlurry_IISpan[6] = currentTime.Subtract(playerFlurry_II[6]);
            playerFlurry_IISpan[7] = currentTime.Subtract(playerFlurry_II[7]);
            playerFlurry_IISpan[8] = currentTime.Subtract(playerFlurry_II[8]);
            playerFlurry_IISpan[9] = currentTime.Subtract(playerFlurry_II[9]);
            playerFlurry_IISpan[10] = currentTime.Subtract(playerFlurry_II[10]);
            playerFlurry_IISpan[11] = currentTime.Subtract(playerFlurry_II[11]);
            playerFlurry_IISpan[12] = currentTime.Subtract(playerFlurry_II[12]);
            playerFlurry_IISpan[13] = currentTime.Subtract(playerFlurry_II[13]);
            playerFlurry_IISpan[14] = currentTime.Subtract(playerFlurry_II[14]);
            playerFlurry_IISpan[15] = currentTime.Subtract(playerFlurry_II[15]);
            playerFlurry_IISpan[16] = currentTime.Subtract(playerFlurry_II[16]);
            playerFlurry_IISpan[17] = currentTime.Subtract(playerFlurry_II[17]);

            // Calculate time since protect was cast on particular player
            playerProtect_Span[0] = currentTime.Subtract(playerProtect[0]);
            playerProtect_Span[1] = currentTime.Subtract(playerProtect[1]);
            playerProtect_Span[2] = currentTime.Subtract(playerProtect[2]);
            playerProtect_Span[3] = currentTime.Subtract(playerProtect[3]);
            playerProtect_Span[4] = currentTime.Subtract(playerProtect[4]);
            playerProtect_Span[5] = currentTime.Subtract(playerProtect[5]);
            playerProtect_Span[6] = currentTime.Subtract(playerProtect[6]);
            playerProtect_Span[7] = currentTime.Subtract(playerProtect[7]);
            playerProtect_Span[8] = currentTime.Subtract(playerProtect[8]);
            playerProtect_Span[9] = currentTime.Subtract(playerProtect[9]);
            playerProtect_Span[10] = currentTime.Subtract(playerProtect[10]);
            playerProtect_Span[11] = currentTime.Subtract(playerProtect[11]);
            playerProtect_Span[12] = currentTime.Subtract(playerProtect[12]);
            playerProtect_Span[13] = currentTime.Subtract(playerProtect[13]);
            playerProtect_Span[14] = currentTime.Subtract(playerProtect[14]);
            playerProtect_Span[15] = currentTime.Subtract(playerProtect[15]);
            playerProtect_Span[16] = currentTime.Subtract(playerProtect[16]);
            playerProtect_Span[17] = currentTime.Subtract(playerProtect[17]);

            // Calculate time since Stormspell was cast on particular player
            playerStormspellSpan[0] = currentTime.Subtract(playerStormspell[0]);
            playerStormspellSpan[1] = currentTime.Subtract(playerStormspell[1]);
            playerStormspellSpan[2] = currentTime.Subtract(playerStormspell[2]);
            playerStormspellSpan[3] = currentTime.Subtract(playerStormspell[3]);
            playerStormspellSpan[4] = currentTime.Subtract(playerStormspell[4]);
            playerStormspellSpan[5] = currentTime.Subtract(playerStormspell[5]);
            playerStormspellSpan[6] = currentTime.Subtract(playerStormspell[6]);
            playerStormspellSpan[7] = currentTime.Subtract(playerStormspell[7]);
            playerStormspellSpan[8] = currentTime.Subtract(playerStormspell[8]);
            playerStormspellSpan[9] = currentTime.Subtract(playerStormspell[9]);
            playerStormspellSpan[10] = currentTime.Subtract(playerStormspell[10]);
            playerStormspellSpan[11] = currentTime.Subtract(playerStormspell[11]);
            playerStormspellSpan[12] = currentTime.Subtract(playerStormspell[12]);
            playerStormspellSpan[13] = currentTime.Subtract(playerStormspell[13]);
            playerStormspellSpan[14] = currentTime.Subtract(playerStormspell[14]);
            playerStormspellSpan[15] = currentTime.Subtract(playerStormspell[15]);
            playerStormspellSpan[16] = currentTime.Subtract(playerStormspell[16]);
            playerStormspellSpan[17] = currentTime.Subtract(playerStormspell[17]);

            // Calculate time since shell was cast on particular player
            playerShell_Span[0] = currentTime.Subtract(playerShell[0]);
            playerShell_Span[1] = currentTime.Subtract(playerShell[1]);
            playerShell_Span[2] = currentTime.Subtract(playerShell[2]);
            playerShell_Span[3] = currentTime.Subtract(playerShell[3]);
            playerShell_Span[4] = currentTime.Subtract(playerShell[4]);
            playerShell_Span[5] = currentTime.Subtract(playerShell[5]);
            playerShell_Span[6] = currentTime.Subtract(playerShell[6]);
            playerShell_Span[7] = currentTime.Subtract(playerShell[7]);
            playerShell_Span[8] = currentTime.Subtract(playerShell[8]);
            playerShell_Span[9] = currentTime.Subtract(playerShell[9]);
            playerShell_Span[10] = currentTime.Subtract(playerShell[10]);
            playerShell_Span[11] = currentTime.Subtract(playerShell[11]);
            playerShell_Span[12] = currentTime.Subtract(playerShell[12]);
            playerShell_Span[13] = currentTime.Subtract(playerShell[13]);
            playerShell_Span[14] = currentTime.Subtract(playerShell[14]);
            playerShell_Span[15] = currentTime.Subtract(playerShell[15]);
            playerShell_Span[16] = currentTime.Subtract(playerShell[16]);
            playerShell_Span[17] = currentTime.Subtract(playerShell[17]);

            // Calculate time since phalanx II was cast on particular player
            playerPhalanx_IISpan[0] = currentTime.Subtract(playerPhalanx_II[0]);
            playerPhalanx_IISpan[1] = currentTime.Subtract(playerPhalanx_II[1]);
            playerPhalanx_IISpan[2] = currentTime.Subtract(playerPhalanx_II[2]);
            playerPhalanx_IISpan[3] = currentTime.Subtract(playerPhalanx_II[3]);
            playerPhalanx_IISpan[4] = currentTime.Subtract(playerPhalanx_II[4]);
            playerPhalanx_IISpan[5] = currentTime.Subtract(playerPhalanx_II[5]);

            // Calculate time since regen was cast on particular player
            playerRegen_Span[0] = currentTime.Subtract(playerRegen[0]);
            playerRegen_Span[1] = currentTime.Subtract(playerRegen[1]);
            playerRegen_Span[2] = currentTime.Subtract(playerRegen[2]);
            playerRegen_Span[3] = currentTime.Subtract(playerRegen[3]);
            playerRegen_Span[4] = currentTime.Subtract(playerRegen[4]);
            playerRegen_Span[5] = currentTime.Subtract(playerRegen[5]);

            // Calculate time since Refresh was cast on particular player
            playerRefresh_Span[0] = currentTime.Subtract(playerRefresh[0]);
            playerRefresh_Span[1] = currentTime.Subtract(playerRefresh[1]);
            playerRefresh_Span[2] = currentTime.Subtract(playerRefresh[2]);
            playerRefresh_Span[3] = currentTime.Subtract(playerRefresh[3]);
            playerRefresh_Span[4] = currentTime.Subtract(playerRefresh[4]);
            playerRefresh_Span[5] = currentTime.Subtract(playerRefresh[5]);

            // Calculate time since Songs were cast on particular player
            playerSong1_Span[0] = currentTime.Subtract(playerSong1[0]);
            playerSong2_Span[0] = currentTime.Subtract(playerSong2[0]);
            playerSong3_Span[0] = currentTime.Subtract(playerSong3[0]);
            playerSong4_Span[0] = currentTime.Subtract(playerSong4[0]);

            // Calculate time since Adloquium were cast on particular player
            playerAdloquium_Span[0] = currentTime.Subtract(playerAdloquium[0]);
            playerAdloquium_Span[1] = currentTime.Subtract(playerAdloquium[1]);
            playerAdloquium_Span[2] = currentTime.Subtract(playerAdloquium[2]);
            playerAdloquium_Span[3] = currentTime.Subtract(playerAdloquium[3]);
            playerAdloquium_Span[4] = currentTime.Subtract(playerAdloquium[4]);
            playerAdloquium_Span[5] = currentTime.Subtract(playerAdloquium[5]);
            playerAdloquium_Span[6] = currentTime.Subtract(playerAdloquium[6]);
            playerAdloquium_Span[7] = currentTime.Subtract(playerAdloquium[7]);
            playerAdloquium_Span[8] = currentTime.Subtract(playerAdloquium[8]);
            playerAdloquium_Span[9] = currentTime.Subtract(playerAdloquium[9]);
            playerAdloquium_Span[10] = currentTime.Subtract(playerAdloquium[10]);
            playerAdloquium_Span[11] = currentTime.Subtract(playerAdloquium[11]);
            playerAdloquium_Span[12] = currentTime.Subtract(playerAdloquium[12]);
            playerAdloquium_Span[13] = currentTime.Subtract(playerAdloquium[13]);
            playerAdloquium_Span[14] = currentTime.Subtract(playerAdloquium[14]);
            playerAdloquium_Span[15] = currentTime.Subtract(playerAdloquium[15]);
            playerAdloquium_Span[16] = currentTime.Subtract(playerAdloquium[16]);
            playerAdloquium_Span[17] = currentTime.Subtract(playerAdloquium[17]);


            Last_SongCast_Timer_Span[0] = currentTime.Subtract(Last_SongCast_Timer[0]);

            // Calculate time since Piannisimo Songs were cast on particular player
            pianissimo1_1_Span[0] = currentTime.Subtract(playerPianissimo1_1[0]);
            pianissimo2_1_Span[0] = currentTime.Subtract(playerPianissimo2_1[0]);
            pianissimo1_2_Span[0] = currentTime.Subtract(playerPianissimo1_2[0]);
            pianissimo2_2_Span[0] = currentTime.Subtract(playerPianissimo2_2[0]);


            int songs_currently_up1 = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == 197 || b == 198 || b == 195 || b == 199 || b == 200 || b == 215 || b == 196 || b == 214 || b == 216 || b == 218 || b == 222).Count();


            if (_ELITEAPIPL.Player.Status == (int) Status.Dead || _ELITEAPIPL.Player.Status == (int)Status.EngagedDead)
            {
                if (OptionsForm.config.pauseOnKO)
                {
                    Pause();
                    return;
                }

                if (OptionsForm.config.AcceptRaise)
                {
                    if (_ELITEAPIPL.Menu.IsMenuOpen && _ELITEAPIPL.Menu.HelpName == "Revival" && _ELITEAPIPL.Menu.MenuIndex == 1 && ((OptionsForm.config.AcceptRaiseOnlyWhenNotInCombat && _ELITEAPIMonitored.Player.Status != 1) || !OptionsForm.config.AcceptRaiseOnlyWhenNotInCombat))
                    {
                        await Task.Delay(2000);
                        currentAction.Text = "Accepting Raise or Reraise.";
                        _ELITEAPIPL.ThirdParty.KeyPress(EliteMMO.API.Keys.NUMPADENTER);
                        await Task.Delay(5000);
                        currentAction.Text = string.Empty;
                    }
                }

                return;
            }

            // If CastingLock is not FALSE and you're not Terrorized, Petrified, or Stunned run the actions
            if (plStatusCheck(StatusEffect.Terror) || plStatusCheck(StatusEffect.Petrification) || plStatusCheck(StatusEffect.Stun))
            {
                return;
            }

            // FIRST IF YOU ARE SILENCED OR DOOMED ATTEMPT REMOVAL NOW
            if (plStatusCheck(StatusEffect.Silence) && OptionsForm.config.plSilenceItemEnabled)
            {
                // Check to make sure we have echo drops
                if ((GetInventoryItemCount(_ELITEAPIPL, GetItemId(plSilenceitemName)) > 0 || GetTempItemCount(_ELITEAPIPL, GetItemId(plSilenceitemName)) > 0))
                {
                    Item_Wait(plSilenceitemName);
                }

            }
            else if ((plStatusCheck(StatusEffect.Doom) && OptionsForm.config.plDoomEnabled) /* Add more options from UI HERE*/)
            {
                // Check to make sure we have holy water
                if (GetInventoryItemCount(_ELITEAPIPL, GetItemId(plDoomItemName)) > 0 || GetTempItemCount(_ELITEAPIPL, GetItemId(plDoomItemName)) > 0)
                {
                    _ELITEAPIPL.ThirdParty.SendString(string.Format("/item \"{0}\" <me>", plDoomItemName));
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            else if (OptionsForm.config.DivineSeal && _ELITEAPIPL.Player.MPP <= 11 && (GetAbilityRecast("Divine Seal") == 0) && !_ELITEAPIPL.Player.Buffs.Contains((short)StatusEffect.Weakness))
            {
                JobAbility_Wait("Divine Seal", "Divine Seal");
            }
            else if (OptionsForm.config.Convert && (_ELITEAPIPL.Player.MP <= OptionsForm.config.convertMP) && (GetAbilityRecast("Convert") == 0) && !_ELITEAPIPL.Player.Buffs.Contains((short)StatusEffect.Weakness))
            {
                _ELITEAPIPL.ThirdParty.SendString("/ja \"Convert\" <me>");
                return;
            }
            else if (OptionsForm.config.RadialArcana && (_ELITEAPIPL.Player.MP <= OptionsForm.config.RadialArcanaMP) && (GetAbilityRecast("Radial Arcana") == 0) && !_ELITEAPIPL.Player.Buffs.Contains((short)StatusEffect.Weakness))
            {
                // Check if a pet is already active
                if (_ELITEAPIPL.Player.Pet.HealthPercent >= 1 && _ELITEAPIPL.Player.Pet.Distance <= 9)
                {
                    JobAbility_Wait("Radial Arcana", "Radial Arcana");
                }
                else if (_ELITEAPIPL.Player.Pet.HealthPercent >= 1 && _ELITEAPIPL.Player.Pet.Distance >= 9 && (GetAbilityRecast("Full Circle") == 0))
                {
                    _ELITEAPIPL.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                    await Task.Delay(2000);
                    string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.RadialArcana_Spell, 2);
                    CastSpell("<me>", SpellCheckedResult);
                }
                else
                {
                    string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.RadialArcana_Spell, 2);
                    CastSpell("<me>", SpellCheckedResult);
                }
            }
            else if (OptionsForm.config.FullCircle)
            {


                // When out of range Distance is 59 Yalms regardless, Must be within 15 yalms to gain
                // the effect

                //Check if "pet" is active and out of range of the monitored player
                if (_ELITEAPIPL.Player.Pet.HealthPercent >= 1)
                {
                    if (OptionsForm.config.Fullcircle_GEOTarget == true && OptionsForm.config.LuopanSpell_Target != "")
                    {

                        ushort PetsIndex = _ELITEAPIPL.Player.PetIndex;

                        EliteAPI.XiEntity PetsEntity = _ELITEAPIPL.Entity.GetEntity(PetsIndex);

                        int FullCircle_CharID = 0;

                        for (int x = 0; x < 2048; x++)
                        {
                            EliteAPI.XiEntity entity = _ELITEAPIPL.Entity.GetEntity(x);

                            if (entity.Name != null && entity.Name.ToLower().Equals(OptionsForm.config.LuopanSpell_Target.ToLower()))
                            {
                                FullCircle_CharID = Convert.ToInt32(entity.TargetID);
                                break;
                            }
                        }

                        if (FullCircle_CharID != 0)
                        {
                            EliteAPI.XiEntity FullCircleEntity = _ELITEAPIPL.Entity.GetEntity(FullCircle_CharID);

                            float fX = PetsEntity.X - FullCircleEntity.X;
                            float fY = PetsEntity.Y - FullCircleEntity.Y;
                            float fZ = PetsEntity.Z - FullCircleEntity.Z;

                            float generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

                            if (generatedDistance >= 10)
                            {
                                FullCircle_Timer.Enabled = true;
                            }
                        }

                    }
                    else if (OptionsForm.config.Fullcircle_GEOTarget == false && _ELITEAPIMonitored.Player.Status == 1)
                    {
                        ushort PetsIndex = _ELITEAPIPL.Player.PetIndex;

                        EliteAPI.XiEntity PetsEntity = _ELITEAPIMonitored.Entity.GetEntity(PetsIndex);

                        if (PetsEntity.Distance >= 10)
                        {
                            FullCircle_Timer.Enabled = true;
                        }
                    }

                }
            }
            else if ((OptionsForm.config.Troubadour) && (GetAbilityRecast("Troubadour") == 0) && (HasAbility("Troubadour")) && songs_currently_up1 == 0)
            {
                JobAbility_Wait("Troubadour", "Troubadour");
            }
            else if ((OptionsForm.config.Nightingale) && (GetAbilityRecast("Nightingale") == 0) && (HasAbility("Nightingale")) && songs_currently_up1 == 0)
            {
                JobAbility_Wait("Nightingale", "Nightingale");
            }

            if (_ELITEAPIPL.Player.MP <= (int)OptionsForm.config.mpMinCastValue && _ELITEAPIPL.Player.MP != 0)
            {
                if (OptionsForm.config.lowMPcheckBox && !islowmp && !OptionsForm.config.healLowMP)
                {
                    _ELITEAPIPL.ThirdParty.SendString("/tell " + _ELITEAPIMonitored.Player.Name + " MP is low!");
                    islowmp = true;
                    return;
                }
                islowmp = true;
                return;
            }
            if (_ELITEAPIPL.Player.MP > (int)OptionsForm.config.mpMinCastValue && _ELITEAPIPL.Player.MP != 0)
            {
                if (OptionsForm.config.lowMPcheckBox && islowmp && !OptionsForm.config.healLowMP)
                {
                    _ELITEAPIPL.ThirdParty.SendString("/tell " + _ELITEAPIMonitored.Player.Name + " MP OK!");
                    islowmp = false;
                }
            }

            if (OptionsForm.config.healLowMP == true && _ELITEAPIPL.Player.MP <= OptionsForm.config.healWhenMPBelow && _ELITEAPIPL.Player.Status == 0)
            {
                if (OptionsForm.config.lowMPcheckBox && !islowmp)
                {
                    _ELITEAPIPL.ThirdParty.SendString("/tell " + _ELITEAPIMonitored.Player.Name + " MP is seriously low, /healing.");
                    islowmp = true;
                }
                _ELITEAPIPL.ThirdParty.SendString("/heal");
            }
            else if (OptionsForm.config.standAtMP == true && _ELITEAPIPL.Player.MPP >= OptionsForm.config.standAtMP_Percentage && _ELITEAPIPL.Player.Status == 33)
            {
                if (OptionsForm.config.lowMPcheckBox && !islowmp)
                {
                    _ELITEAPIPL.ThirdParty.SendString("/tell " + _ELITEAPIMonitored.Player.Name + " MP has recovered.");
                    islowmp = false;
                }
                _ELITEAPIPL.ThirdParty.SendString("/heal");
            }

            // Only perform actions if PL is stationary PAUSE GOES HERE
            if ((_ELITEAPIPL.Player.X == plX) && (_ELITEAPIPL.Player.Y == plY) && (_ELITEAPIPL.Player.Z == plZ) && (_ELITEAPIPL.Player.LoginStatus == (int)LoginStatus.LoggedIn) && JobAbilityLock_Check != true && CastingBackground_Check != true && curePlease_autofollow == false && ((_ELITEAPIPL.Player.Status == (uint)Status.Idle) || (_ELITEAPIPL.Player.Status == (uint)Status.Engaged)))
            {
                // IF SILENCED THIS NEEDS TO BE REMOVED BEFORE ANY MAGIC IS ATTEMPTED
                if (OptionsForm.config.plSilenceItem == 0)
                {
                    plSilenceitemName = "Catholicon";
                }
                else if (OptionsForm.config.plSilenceItem == 1)
                {
                    plSilenceitemName = "Echo Drops";
                }
                else if (OptionsForm.config.plSilenceItem == 2)
                {
                    plSilenceitemName = "Remedy";
                }
                else if (OptionsForm.config.plSilenceItem == 3)
                {
                    plSilenceitemName = "Remedy Ointment";
                }
                else if (OptionsForm.config.plSilenceItem == 4)
                {
                    plSilenceitemName = "Vicar's Drink";
                }

                foreach (StatusEffect plEffect in _ELITEAPIPL.Player.Buffs)
                {
                    if (plEffect == StatusEffect.Silence && OptionsForm.config.plSilenceItemEnabled)
                    {
                        // Check to make sure we have echo drops
                        if (GetInventoryItemCount(_ELITEAPIPL, GetItemId(plSilenceitemName)) > 0 || GetTempItemCount(_ELITEAPIPL, GetItemId(plSilenceitemName)) > 0)
                        {
                            _ELITEAPIPL.ThirdParty.SendString(string.Format("/item \"{0}\" <me>", plSilenceitemName));
                            await Task.Delay(4000);
                            break;
                        }
                    }
                }

                /////////////////////////// PL CURE //////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                if (_ELITEAPIPL.Player.HP > 0 && (_ELITEAPIPL.Player.HPP <= OptionsForm.config.monitoredCurePercentage) && OptionsForm.config.enableOutOfPartyHealing == true && PLInParty() == false)
                {
                    CureCalculator_PL(false);
                }

                /////////////////////////// CURAGA //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // Set array values for GUI "Enabled" checkboxes
                var partyMembersEnabled = new bool[18]
                {
                    player0enabled.Checked,
                    player1enabled.Checked,
                    player2enabled.Checked,
                    player3enabled.Checked,
                    player4enabled.Checked,
                    player5enabled.Checked,
                    player6enabled.Checked,
                    player7enabled.Checked,
                    player8enabled.Checked,
                    player9enabled.Checked,
                    player10enabled.Checked,
                    player11enabled.Checked,
                    player12enabled.Checked,
                    player13enabled.Checked,
                    player14enabled.Checked,
                    player15enabled.Checked,
                    player16enabled.Checked,
                    player17enabled.Checked,
                };

                RunCuragas(partyMembersEnabled);
                RunCures(partyMembersEnabled);

                // RUN DEBUFF REMOVAL - CONVERTED TO FUNCTION SO CAN BE RUN IN MULTIPLE AREAS
                RunDebuffChecker();

                // PL Auto Buffs

                string BarspellName = string.Empty;
                int BarspellBuffID = 0;
                bool BarSpell_AOE = false;

                if (OptionsForm.config.AOE_Barelemental == false)
                {
                    SpellsData barspell = barspells.Where(c => c.spell_position == OptionsForm.config.plBarElement_Spell && c.type == 1 && c.aoe_version != true).SingleOrDefault();

                    BarspellName = barspell.Spell_Name;
                    BarspellBuffID = barspell.buffID;
                    BarSpell_AOE = false;
                }
                else
                {
                    SpellsData barspell = barspells.Where(c => c.spell_position == OptionsForm.config.plBarElement_Spell && c.type == 1 && c.aoe_version == true).SingleOrDefault();

                    BarspellName = barspell.Spell_Name;
                    BarspellBuffID = barspell.buffID;
                    BarSpell_AOE = true;
                }

                string BarstatusName = string.Empty;
                int BarstatusBuffID = 0;
                bool BarStatus_AOE = false;

                if (OptionsForm.config.AOE_Barstatus == false)
                {
                    SpellsData barstatus = barspells.Where(c => c.spell_position == OptionsForm.config.plBarStatus_Spell && c.type == 2 && c.aoe_version != true).SingleOrDefault();

                    BarstatusName = barstatus.Spell_Name;
                    BarstatusBuffID = barstatus.buffID;
                    BarStatus_AOE = false;
                }
                else
                {
                    SpellsData barstatus = barspells.Where(c => c.spell_position == OptionsForm.config.plBarStatus_Spell && c.type == 2 && c.aoe_version == true).SingleOrDefault();

                    BarstatusName = barstatus.Spell_Name;
                    BarstatusBuffID = barstatus.buffID;
                    BarStatus_AOE = true;
                }

                SpellsData enspell = enspells.Where(c => c.spell_position == OptionsForm.config.plEnspell_Spell && c.type == 1).SingleOrDefault();
                SpellsData stormspell = stormspells.Where(c => c.spell_position == OptionsForm.config.plStormSpell_Spell).SingleOrDefault();

                if ((OptionsForm.config.Composure) && (!plStatusCheck(StatusEffect.Composure)) && (GetAbilityRecast("Composure") == 0) && (HasAbility("Composure")))
                {

                    JobAbility_Wait("Composure", "Composure");
                }
                else if ((OptionsForm.config.LightArts) && (!plStatusCheck(StatusEffect.Light_Arts)) && (!plStatusCheck(StatusEffect.Addendum_White)) && (GetAbilityRecast("Light Arts") == 0) && (HasAbility("Light Arts")))
                {
                    JobAbility_Wait("Light Arts", "Light Arts");
                }
                else if ((OptionsForm.config.AddendumWhite) && (!plStatusCheck(StatusEffect.Addendum_White)) && (plStatusCheck(StatusEffect.Light_Arts)) && (GetAbilityRecast("Stratagems") == 0) && (HasAbility("Stratagems")))
                {
                    JobAbility_Wait("Addendum: White", "Addendum: White");
                }
                else if ((OptionsForm.config.DarkArts) && (!plStatusCheck(StatusEffect.Dark_Arts)) && (!plStatusCheck(StatusEffect.Addendum_Black)) && (GetAbilityRecast("Dark Arts") == 0) && (HasAbility("Dark Arts")))
                {
                    JobAbility_Wait("Dark Arts", "Dark Arts");
                }
                else if ((OptionsForm.config.AddendumBlack) && (plStatusCheck(StatusEffect.Dark_Arts)) && (!plStatusCheck(StatusEffect.Addendum_Black)) && (GetAbilityRecast("Stratagems") == 0) && (HasAbility("Stratagems")))
                {
                    JobAbility_Wait("Addendum: Black", "Addendum: Black");
                }
                else if ((OptionsForm.config.plReraise) && (OptionsForm.config.EnlightenmentReraise) && (!plStatusCheck(StatusEffect.Reraise)) && _ELITEAPIPL.Player.MainJob == 20 && !BuffChecker(401, 0) && HasAbility("Enlightenment"))
                {


                    if (!plStatusCheck(StatusEffect.Enlightenment) && (GetAbilityRecast("Enlightenment") == 0))
                    {
                        JobAbility_Wait("Reraise, Enlightenment", "Enlightenment");
                    }


                    if ((OptionsForm.config.plReraise_Level == 1) && _ELITEAPIPL.Player.HasSpell(_ELITEAPIPL.Resources.GetSpell("Reraise", 0).Index) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise");
                    }
                    else if ((OptionsForm.config.plReraise_Level == 2) && _ELITEAPIPL.Player.HasSpell(_ELITEAPIPL.Resources.GetSpell("Reraise II", 0).Index) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise II");
                    }
                    else if ((OptionsForm.config.plReraise_Level == 3) && _ELITEAPIPL.Player.HasSpell(_ELITEAPIPL.Resources.GetSpell("Reraise III", 0).Index) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise III");
                    }
                    else if ((OptionsForm.config.plReraise_Level == 4) && _ELITEAPIPL.Player.HasSpell(_ELITEAPIPL.Resources.GetSpell("Reraise III", 0).Index) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise III");
                    }

                }
                else if ((OptionsForm.config.plReraise) && (!plStatusCheck(StatusEffect.Reraise)) && CheckReraiseLevelPossession() == true)
                {
                    if ((OptionsForm.config.plReraise_Level == 1) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise");
                    }
                    else if ((OptionsForm.config.plReraise_Level == 2) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise II");
                    }
                    else if ((OptionsForm.config.plReraise_Level == 3) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise III");
                    }
                    else if ((OptionsForm.config.plReraise_Level == 4) && _ELITEAPIPL.Player.MP > 150)
                    {
                        CastSpell("<me>", "Reraise IV");
                    }
                }
                else if ((OptionsForm.config.plUtsusemi) && (BuffChecker(444, 0) != true && BuffChecker(445, 0) != true && BuffChecker(446, 0) != true))
                {
                    if (CheckSpellRecast("Utsusemi: Ni") == 0 && CanCastSpell("Utsusemi: Ni") && GetInventoryItemCount(_ELITEAPIPL, GetItemId("Shihei")) > 0)
                    {
                        CastSpell("<me>", "Utsusemi: Ni");
                    }
                    else if (CheckSpellRecast("Utsusemi: Ichi") == 0 && CanCastSpell("Utsusemi: Ichi") && (BuffChecker(62, 0) != true && BuffChecker(444, 0) != true && BuffChecker(445, 0) != true && BuffChecker(446, 0) != true) && GetInventoryItemCount(_ELITEAPIPL, GetItemId("Shihei")) > 0)
                    {
                        CastSpell("<me>", "Utsusemi: Ichi");
                    }
                }
                else if ((OptionsForm.config.plProtect) && (!plStatusCheck(StatusEffect.Protect)))
                {
                    string protectSpell = string.Empty;
                    if (OptionsForm.config.autoProtect_Spell == 0)
                    {
                        protectSpell = "Protect";
                    }
                    else if (OptionsForm.config.autoProtect_Spell == 1)
                    {
                        protectSpell = "Protect II";
                    }
                    else if (OptionsForm.config.autoProtect_Spell == 2)
                    {
                        protectSpell = "Protect III";
                    }
                    else if (OptionsForm.config.autoProtect_Spell == 3)
                    {
                        protectSpell = "Protect IV";
                    }
                    else if (OptionsForm.config.autoProtect_Spell == 4)
                    {
                        protectSpell = "Protect V";
                    }

                    if (protectSpell != string.Empty && CheckSpellRecast(protectSpell) == 0 && CanCastSpell(protectSpell))
                    {
                        if ((OptionsForm.config.Accession && OptionsForm.config.accessionProShell && _ELITEAPIPL.Party.GetPartyMembers().Count() > 2) && ((_ELITEAPIPL.Player.MainJob == 5 && _ELITEAPIPL.Player.SubJob == 20) || _ELITEAPIPL.Player.MainJob == 20) && currentSCHCharges >= 1 && (HasAbility("Accession")))
                        {
                            if (!plStatusCheck(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Protect, Accession", "Accession");
                                return;
                            }
                        }

                        CastSpell("<me>", protectSpell);
                    }
                }
                else if ((OptionsForm.config.plShell) && (!plStatusCheck(StatusEffect.Shell)))
                {
                    string shellSpell = string.Empty;
                    if (OptionsForm.config.autoShell_Spell == 0)
                    {
                        shellSpell = "Shell";
                    }
                    else if (OptionsForm.config.autoShell_Spell == 1)
                    {
                        shellSpell = "Shell II";
                    }
                    else if (OptionsForm.config.autoShell_Spell == 2)
                    {
                        shellSpell = "Shell III";
                    }
                    else if (OptionsForm.config.autoShell_Spell == 3)
                    {
                        shellSpell = "Shell IV";
                    }
                    else if (OptionsForm.config.autoShell_Spell == 4)
                    {
                        shellSpell = "Shell V";
                    }

                    if (shellSpell != string.Empty && CheckSpellRecast(shellSpell) == 0 && CanCastSpell(shellSpell))
                    {
                        if ((OptionsForm.config.Accession && OptionsForm.config.accessionProShell && _ELITEAPIPL.Party.GetPartyMembers().Count() > 2) && ((_ELITEAPIPL.Player.MainJob == 5 && _ELITEAPIPL.Player.SubJob == 20) || _ELITEAPIPL.Player.MainJob == 20) && currentSCHCharges >= 1 && (HasAbility("Accession")))
                        {
                            if (!plStatusCheck(StatusEffect.Accession))
                            {
                                JobAbility_Wait("Shell, Accession", "Accession");
                                return;
                            }
                        }

                        CastSpell("<me>", shellSpell);
                    }
                }
                else if ((OptionsForm.config.plBlink) && (!plStatusCheck(StatusEffect.Blink)) && (CheckSpellRecast("Blink") == 0) && (CanCastSpell("Blink")))
                {

                    if (OptionsForm.config.Accession && OptionsForm.config.blinkAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Blink, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.blinkPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Blink, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", "Blink");
                }
                else if ((OptionsForm.config.plPhalanx) && (!plStatusCheck(StatusEffect.Phalanx)) && (CheckSpellRecast("Phalanx") == 0) && CanCastSpell("Phalanx"))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.phalanxAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Phalanx, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.phalanxPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Phalanx, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", "Phalanx");
                }
                else if ((OptionsForm.config.plRefresh) && (!plStatusCheck(StatusEffect.Refresh)) && CheckRefreshLevelPossession())
                {
                    if ((OptionsForm.config.plRefresh_Level == 1) && (CheckSpellRecast("Refresh") == 0) && CanCastSpell("Refresh"))
                    {
                        if (OptionsForm.config.Accession && OptionsForm.config.refreshAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                        {
                            JobAbility_Wait("Refresh, Accession", "Accession");
                            return;
                        }

                        if (OptionsForm.config.Perpetuance && OptionsForm.config.refreshPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                        {
                            JobAbility_Wait("Refresh, Perpetuance", "Perpetuance");
                            return;
                        }

                        CastSpell("<me>", "Refresh");
                    }
                    else if ((OptionsForm.config.plRefresh_Level == 2) && (CheckSpellRecast("Refresh II") == 0) && CanCastSpell("Refresh II"))
                    {
                        CastSpell("<me>", "Refresh II");
                    }
                    else if ((OptionsForm.config.plRefresh_Level == 3) && (CheckSpellRecast("Refresh III") == 0) && CanCastSpell("Refresh III"))
                    {
                        CastSpell("<me>", "Refresh III");
                    }
                }
                else if ((OptionsForm.config.plRegen) && (!plStatusCheck(StatusEffect.Regen)) && CheckRegenLevelPossession() == true)
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.regenAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Regen, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.regenPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Regen, Perpetuance", "Perpetuance");
                        return;
                    }

                    if ((OptionsForm.config.plRegen_Level == 1) && _ELITEAPIPL.Player.MP > 15)
                    {
                        CastSpell("<me>", "Regen");
                    }
                    else if ((OptionsForm.config.plRegen_Level == 2) && _ELITEAPIPL.Player.MP > 36)
                    {
                        CastSpell("<me>", "Regen II");
                    }
                    else if ((OptionsForm.config.plRegen_Level == 3) && _ELITEAPIPL.Player.MP > 64)
                    {
                        CastSpell("<me>", "Regen III");
                    }
                    else if ((OptionsForm.config.plRegen_Level == 4) && _ELITEAPIPL.Player.MP > 82)
                    {
                        CastSpell("<me>", "Regen IV");
                    }
                    else if ((OptionsForm.config.plRegen_Level == 5) && _ELITEAPIPL.Player.MP > 100)
                    {
                        CastSpell("<me>", "Regen V");
                    }
                }
                else if ((OptionsForm.config.plAdloquium) && (!plStatusCheck(StatusEffect.Regain)) && (CheckSpellRecast("Adloquium") == 0) && CanCastSpell("Adloquium"))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.adloquiumAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Adloquium, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.adloquiumPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Adloquium, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", "Adloquium");
                }
                else if ((OptionsForm.config.plStoneskin) && (!plStatusCheck(StatusEffect.Stoneskin)) && (CheckSpellRecast("Stoneskin") == 0) && CanCastSpell("Stoneskin"))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.stoneskinAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Stoneskin, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.stoneskinPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Stoneskin, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", "Stoneskin");
                }
                else if ((OptionsForm.config.plAquaveil) && (!plStatusCheck(StatusEffect.Aquaveil)) && (CheckSpellRecast("Aquaveil") == 0) && CanCastSpell("Aquaveil"))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.aquaveilAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Aquaveil, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.aquaveilPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Aquaveil, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", "Aquaveil");
                }
                else if ((OptionsForm.config.plShellra) && (!plStatusCheck(StatusEffect.Shell)) && CheckShellraLevelPossession() == true)
                {
                    CastSpell("<me>", GetShellraLevel(OptionsForm.config.plShellra_Level));
                }
                else if ((OptionsForm.config.plProtectra) && (!plStatusCheck(StatusEffect.Protect)) && CheckProtectraLevelPossession() == true)
                {
                    CastSpell("<me>", GetProtectraLevel(OptionsForm.config.plProtectra_Level));
                }
                else if ((OptionsForm.config.plBarElement) && (!BuffChecker(BarspellBuffID, 0) && (CheckSpellRecast(BarspellName) == 0) && CanCastSpell(BarspellName)))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.barspellAccession && currentSCHCharges > 0 && HasAbility("Accession") && BarSpell_AOE == false && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Barspell, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.barspellPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Barspell, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", BarspellName);
                }
                else if ((OptionsForm.config.plBarStatus) && (!BuffChecker(BarstatusBuffID, 0) && (CheckSpellRecast(BarstatusName) == 0) && (CanCastSpell(BarstatusName))))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.barstatusAccession && currentSCHCharges > 0 && HasAbility("Accession") && BarStatus_AOE == false && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Barstatus, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.barstatusPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Barstatus, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", BarstatusName);
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 0) && !plStatusCheck(StatusEffect.STR_Boost2) && (CheckSpellRecast("Gain-STR") == 0) && (CanCastSpell("Gain-STR")))
                {
                    CastSpell("<me>", "Gain-STR");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 1) && !plStatusCheck(StatusEffect.DEX_Boost2) && (CheckSpellRecast("Gain-DEX") == 0) && (CanCastSpell("Gain-DEX")))
                {
                    CastSpell("<me>", "Gain-DEX");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 2) && !plStatusCheck(StatusEffect.VIT_Boost2) && (CheckSpellRecast("Gain-VIT") == 0) && (CanCastSpell("Gain-VIT")))
                {
                    CastSpell("<me>", "Gain-VIT");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 3) && !plStatusCheck(StatusEffect.AGI_Boost2) && (CheckSpellRecast("Gain-AGI") == 0) && (CanCastSpell("Gain-AGI")))
                {
                    CastSpell("<me>", "Gain-AGI");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 4) && !plStatusCheck(StatusEffect.INT_Boost2) && (CheckSpellRecast("Gain-INT") == 0) && (CanCastSpell("Gain-INT")))
                {
                    CastSpell("<me>", "Gain-INT");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 5) && !plStatusCheck(StatusEffect.MND_Boost2) && (CheckSpellRecast("Gain-MND") == 0) && (CanCastSpell("Gain-MND")))
                {
                    CastSpell("<me>", "Gain-MND");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 6) && !plStatusCheck(StatusEffect.CHR_Boost2) && (CheckSpellRecast("Gain-CHR") == 0) && (CanCastSpell("Gain-CHR")))
                {
                    CastSpell("<me>", "Gain-CHR");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 7) && !plStatusCheck(StatusEffect.STR_Boost2) && (CheckSpellRecast("Boost-STR") == 0) && (CanCastSpell("Boost-STR")))
                {
                    CastSpell("<me>", "Boost-STR");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 8) && !plStatusCheck(StatusEffect.DEX_Boost2) && (CheckSpellRecast("Boost-DEX") == 0) && (CanCastSpell("Boost-DEX")))
                {
                    CastSpell("<me>", "Boost-DEX");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 9) && !plStatusCheck(StatusEffect.VIT_Boost2) && (CheckSpellRecast("Boost-VIT") == 0) && (CanCastSpell("Boost-VIT")))
                {
                    CastSpell("<me>", "Boost-VIT");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 10) && !plStatusCheck(StatusEffect.AGI_Boost2) && (CheckSpellRecast("Boost-AGI") == 0) && (CanCastSpell("Boost-AGI")))
                {
                    CastSpell("<me>", "Boost-AGI");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 11) && !plStatusCheck(StatusEffect.INT_Boost2) && (CheckSpellRecast("Boost-INT") == 0) && (CanCastSpell("Boost-INT")))
                {
                    CastSpell("<me>", "Boost-INT");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 12) && !plStatusCheck(StatusEffect.MND_Boost2) && (CheckSpellRecast("Boost-MND") == 0) && (CanCastSpell("Boost-MND")))
                {
                    CastSpell("<me>", "Boost-MND");
                }
                else if (OptionsForm.config.plGainBoost && (OptionsForm.config.plGainBoost_Spell == 13) && !plStatusCheck(StatusEffect.CHR_Boost2) && (CheckSpellRecast("Boost-CHR") == 0) && (CanCastSpell("Boost-CHR")))
                {
                    CastSpell("<me>", "Boost-CHR");
                }
                else if ((OptionsForm.config.plStormSpell) && (!BuffChecker(stormspell.buffID, 0) && (CheckSpellRecast(stormspell.Spell_Name) == 0) && (CanCastSpell(stormspell.Spell_Name))))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.stormspellAccession && currentSCHCharges > 0 && HasAbility("Accession") && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Stormspell, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.stormspellPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Stormspell, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", stormspell.Spell_Name);
                }
                else if ((OptionsForm.config.plKlimaform) && !plStatusCheck(StatusEffect.Klimaform))
                {
                    if ((CheckSpellRecast("Klimaform") == 0) && (CanCastSpell("Klimaform")))
                    {
                        CastSpell("<me>", "Klimaform");
                    }
                }
                else if ((OptionsForm.config.plTemper) && (!plStatusCheck(StatusEffect.Multi_Strikes)))
                {
                    if ((OptionsForm.config.plTemper_Level == 1) && (CheckSpellRecast("Temper") == 0) && (CanCastSpell("Temper")))
                    {
                        CastSpell("<me>", "Temper");
                    }
                    else if ((OptionsForm.config.plTemper_Level == 2) && (CheckSpellRecast("Temper II") == 0) && (CanCastSpell("Temper II")))
                    {
                        CastSpell("<me>", "Temper II");
                    }
                }
                else if ((OptionsForm.config.plHaste) && (!plStatusCheck(StatusEffect.Haste)))
                {
                    if ((OptionsForm.config.plHaste_Level == 1) && (CheckSpellRecast("Haste") == 0) && (CanCastSpell("Haste")))
                    {
                        CastSpell("<me>", "Haste");
                    }
                    else if ((OptionsForm.config.plHaste_Level == 2) && (CheckSpellRecast("Haste II") == 0) && (CanCastSpell("Haste II")))
                    {
                        CastSpell("<me>", "Haste II");
                    }
                }
                else if ((OptionsForm.config.plSpikes) && ActiveSpikes() == false)
                {
                    if ((OptionsForm.config.plSpikes_Spell == 0) && (CheckSpellRecast("Blaze Spikes") == 0) && (CanCastSpell("Blaze Spikes")))
                    {
                        CastSpell("<me>", "Blaze Spikes");
                    }
                    else if ((OptionsForm.config.plSpikes_Spell == 1) && (CheckSpellRecast("Ice Spikes") == 0) && (CanCastSpell("Ice Spikes")))
                    {
                        CastSpell("<me>", "Ice Spikes");
                    }
                    else if ((OptionsForm.config.plSpikes_Spell == 2) && (CheckSpellRecast("Shock Spikes") == 0) && (CanCastSpell("Shock Spikes")))
                    {
                        CastSpell("<me>", "Shock Spikes");
                    }
                }
                else if ((OptionsForm.config.plEnspell) && (!BuffChecker(enspell.buffID, 0) && (CheckSpellRecast(enspell.Spell_Name) == 0) && (CanCastSpell(enspell.Spell_Name))))
                {
                    if (OptionsForm.config.Accession && OptionsForm.config.enspellAccession && currentSCHCharges > 0 && HasAbility("Accession") && enspell.spell_position < 6 && !plStatusCheck(StatusEffect.Accession))
                    {
                        JobAbility_Wait("Enspell, Accession", "Accession");
                        return;
                    }

                    if (OptionsForm.config.Perpetuance && OptionsForm.config.enspellPerpetuance && currentSCHCharges > 0 && HasAbility("Perpetuance") && enspell.spell_position < 6 && !plStatusCheck(StatusEffect.Perpetuance))
                    {
                        JobAbility_Wait("Enspell, Perpetuance", "Perpetuance");
                        return;
                    }

                    CastSpell("<me>", enspell.Spell_Name);
                }
                else if ((OptionsForm.config.plAuspice) && (!plStatusCheck(StatusEffect.Auspice)) && (CheckSpellRecast("Auspice") == 0) && (CanCastSpell("Auspice")))
                {
                    CastSpell("<me>", "Auspice");
                }

                // ENTRUSTED INDI SPELL CASTING, WILL BE CAST SO LONG AS ENTRUST IS ACTIVE
                else if ((OptionsForm.config.EnableGeoSpells) && (plStatusCheck((StatusEffect)584)) && _ELITEAPIPL.Player.Status != 33)
                {
                    string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.EntrustedSpell_Spell, 1);
                    if (SpellCheckedResult == "SpellError_Cancel")
                    {
                        OptionsForm.config.EnableGeoSpells = false;
                        MessageBox.Show("An error has occurred with Entrusted INDI spell casting, please report what spell was active at the time.");
                    }
                    else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                    {
                    }
                    else
                    {
                        if (OptionsForm.config.EntrustedSpell_Target == string.Empty)
                        {
                            CastSpell(_ELITEAPIMonitored.Player.Name, SpellCheckedResult);
                        }
                        else
                        {
                            CastSpell(OptionsForm.config.EntrustedSpell_Target, SpellCheckedResult);
                        }
                    }
                }

                // CAST NON ENTRUSTED INDI SPELL
                else if (OptionsForm.config.EnableGeoSpells && !BuffChecker(612, 0) && _ELITEAPIPL.Player.Status != 33 && (CheckEngagedStatus() == true || !OptionsForm.config.IndiWhenEngaged))
                {
                    string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.IndiSpell_Spell, 1);

                    if (SpellCheckedResult == "SpellError_Cancel")
                    {
                        OptionsForm.config.EnableGeoSpells = false;
                        MessageBox.Show("An error has occurred with INDI spell casting, please report what spell was active at the time.");
                    }
                    else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                    {
                    }
                    else
                    {
                        CastSpell("<me>", SpellCheckedResult);
                    }

                }

                // GEO SPELL CASTING 
                else if ((OptionsForm.config.EnableLuopanSpells) && (_ELITEAPIPL.Player.Pet.HealthPercent < 1) && (CheckEngagedStatus() == true))
                {
                    // Use BLAZE OF GLORY if ENABLED
                    if (OptionsForm.config.BlazeOfGlory && GetAbilityRecast("Blaze of Glory") == 0 && HasAbility("Blaze of Glory") && CheckEngagedStatus() == true && GEO_EnemyCheck() == true)
                    {
                        JobAbility_Wait("Blaze of Glory", "Blaze of Glory");
                    }

                    // Grab GEO spell name
                    string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.GeoSpell_Spell, 2);

                    if (SpellCheckedResult == "SpellError_Cancel")
                    {
                        OptionsForm.config.EnableGeoSpells = false;
                        MessageBox.Show("An error has occurred with GEO spell casting, please report what spell was active at the time.");
                    }
                    else if (SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
                    {
                        // Do nothing and continue on with the program
                    }
                    else
                    {
                        if (_ELITEAPIPL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                        { // PLAYER CHARACTER TARGET
                            if (OptionsForm.config.LuopanSpell_Target == string.Empty)
                            {

                                if (BuffChecker(516, 0)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                                {
                                    EclipticStillUp = true;
                                }

                                CastSpell(_ELITEAPIMonitored.Player.Name, SpellCheckedResult);
                            }
                            else
                            {
                                if (BuffChecker(516, 0)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                                {
                                    EclipticStillUp = true;
                                }

                                CastSpell(OptionsForm.config.LuopanSpell_Target, SpellCheckedResult);
                            }
                        }
                        else
                        { // ENEMY BASED TARGET NEED TO ASSURE PLAYER IS ENGAGED
                            if (CheckEngagedStatus() == true)
                            {

                                int GrabbedTargetID = GrabGEOTargetID();

                                if (GrabbedTargetID != 0)
                                {

                                    _ELITEAPIPL.Target.SetTarget(GrabbedTargetID);
                                    await Task.Delay(TimeSpan.FromSeconds(1));

                                    if (BuffChecker(516, 0)) // IF ECLIPTIC IS UP THEN ACTIVATE THE BOOL
                                    {
                                        EclipticStillUp = true;
                                    }

                                    CastSpell("<t>", SpellCheckedResult);
                                    await Task.Delay(TimeSpan.FromSeconds(4));
                                    if (OptionsForm.config.DisableTargettingCancel == false)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds((double)OptionsForm.config.TargetRemoval_Delay));
                                        _ELITEAPIPL.Target.SetTarget(0);
                                    }
                                }
                            }
                        }
                    }
                }

                else if ((OptionsForm.config.autoTarget == true) && (CheckSpellRecast(OptionsForm.config.autoTargetSpell) == 0) && (CanCastSpell(OptionsForm.config.autoTargetSpell)))
                {
                    if (OptionsForm.config.Hate_SpellType == 1) // PARTY BASED HATE SPELL
                    {
                        int enemyID = CheckEngagedStatus_Hate();

                        if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                        {
                            CastSpell(OptionsForm.config.autoTarget_Target, OptionsForm.config.autoTargetSpell);
                            lastKnownEstablisherTarget = enemyID;
                        }
                    }
                    else // ENEMY BASED TARGET
                    {
                        int enemyID = CheckEngagedStatus_Hate();

                        if (enemyID != 0 && enemyID != lastKnownEstablisherTarget)
                        {
                            _ELITEAPIPL.Target.SetTarget(enemyID);
                            await Task.Delay(TimeSpan.FromMilliseconds(500));
                            CastSpell("<t>", OptionsForm.config.autoTargetSpell);
                            lastKnownEstablisherTarget = enemyID;
                            await Task.Delay(TimeSpan.FromMilliseconds(1000));

                            if (OptionsForm.config.DisableTargettingCancel == false)
                            {
                                await Task.Delay(TimeSpan.FromSeconds((double)OptionsForm.config.TargetRemoval_Delay));
                                _ELITEAPIPL.Target.SetTarget(0);
                            }
                        }
                    }
                }

                // BARD SONGS

                else if (OptionsForm.config.enableSinging && !plStatusCheck(StatusEffect.Silence) && (_ELITEAPIPL.Player.Status == 1 || _ELITEAPIPL.Player.Status == 0))
                {
                    Run_BardSongs();

                }


                // so PL job abilities are in order
                if (!plStatusCheck(StatusEffect.Amnesia) && (_ELITEAPIPL.Player.Status == 1 || _ELITEAPIPL.Player.Status == 0))
                {
                    if ((OptionsForm.config.AfflatusSolace) && (!plStatusCheck(StatusEffect.Afflatus_Solace)) && (GetAbilityRecast("Afflatus Solace") == 0) && (HasAbility("Afflatus Solace")))
                    {
                        JobAbility_Wait("Afflatus Solace", "Afflatus Solace");
                    }
                    else if ((OptionsForm.config.AfflatusMisery) && (!plStatusCheck(StatusEffect.Afflatus_Misery)) && (GetAbilityRecast("Afflatus Misery") == 0) && (HasAbility("Afflatus Misery")))
                    {
                        JobAbility_Wait("Afflatus Misery", "Afflatus Misery");
                    }
                    else if ((OptionsForm.config.Composure) && (!plStatusCheck(StatusEffect.Composure)) && (GetAbilityRecast("Composure") == 0) && (HasAbility("Composure")))
                    {
                        JobAbility_Wait("Composure #2", "Composure");
                    }
                    else if ((OptionsForm.config.LightArts) && (!plStatusCheck(StatusEffect.Light_Arts)) && (!plStatusCheck(StatusEffect.Addendum_White)) && (GetAbilityRecast("Light Arts") == 0) && (HasAbility("Light Arts")))
                    {
                        JobAbility_Wait("Light Arts #2", "Light Arts");
                    }
                    else if ((OptionsForm.config.AddendumWhite) && (!plStatusCheck(StatusEffect.Addendum_White)) && (GetAbilityRecast("Stratagems") == 0) && (HasAbility("Stratagems")))
                    {
                        JobAbility_Wait("Addendum: White", "Addendum: White");
                    }
                    else if ((OptionsForm.config.Sublimation) && (!plStatusCheck(StatusEffect.Sublimation_Activated)) && (!plStatusCheck(StatusEffect.Sublimation_Complete)) && (!plStatusCheck(StatusEffect.Refresh)) && (GetAbilityRecast("Sublimation") == 0) && (HasAbility("Sublimation")))
                    {
                        JobAbility_Wait("Sublimation, Charging", "Sublimation");
                    }
                    else if ((OptionsForm.config.Sublimation) && ((_ELITEAPIPL.Player.MPMax - _ELITEAPIPL.Player.MP) > OptionsForm.config.sublimationMP) && (plStatusCheck(StatusEffect.Sublimation_Complete)) && (GetAbilityRecast("Sublimation") == 0) && (HasAbility("Sublimation")))
                    {
                        JobAbility_Wait("Sublimation, Recovery", "Sublimation");
                    }
                    else if ((OptionsForm.config.DivineCaress) && (OptionsForm.config.plDebuffEnabled || OptionsForm.config.monitoredDebuffEnabled || OptionsForm.config.enablePartyDebuffRemoval) && (GetAbilityRecast("Divine Caress") == 0) && (HasAbility("Divine Caress")))
                    {
                        JobAbility_Wait("Divine Caress", "Divine Caress");
                    }
                    else if (OptionsForm.config.Entrust && !plStatusCheck((StatusEffect)584) && CheckEngagedStatus() == true && GetAbilityRecast("Entrust") == 0 && HasAbility("Entrust"))
                    {
                        JobAbility_Wait("Entrust", "Entrust");
                    }
                    else if (OptionsForm.config.Dematerialize && CheckEngagedStatus() == true && _ELITEAPIPL.Player.Pet.HealthPercent >= 90 && GetAbilityRecast("Dematerialize") == 0 && HasAbility("Dematerialize"))
                    {
                        JobAbility_Wait("Dematerialize", "Dematerialize");
                    }
                    else if (OptionsForm.config.EclipticAttrition && CheckEngagedStatus() == true && _ELITEAPIPL.Player.Pet.HealthPercent >= 90 && GetAbilityRecast("Ecliptic Attrition") == 0 && HasAbility("Ecliptic Attrition") && (BuffChecker(516, 2) != true) && EclipticStillUp != true)
                    {
                        JobAbility_Wait("Ecliptic Attrition", "Ecliptic Attrition");
                    }
                    else if (OptionsForm.config.LifeCycle && CheckEngagedStatus() == true && _ELITEAPIPL.Player.Pet.HealthPercent <= 30 && _ELITEAPIPL.Player.Pet.HealthPercent >= 5 && _ELITEAPIPL.Player.HPP >= 90 && GetAbilityRecast("Life Cycle") == 0 && HasAbility("Life Cycle"))
                    {
                        JobAbility_Wait("Life Cycle", "Life Cycle");
                    }
                    else if ((OptionsForm.config.Devotion) && (GetAbilityRecast("Devotion") == 0) && (HasAbility("Devotion")) && _ELITEAPIPL.Player.HPP > 80 && (!OptionsForm.config.DevotionWhenEngaged || (_ELITEAPIMonitored.Player.Status == 1)))
                    {
                        // First Generate the current party number, this will be used
                        // regardless of the type
                        int memberOF = GeneratePT_structure();

                        // Now generate the party
                        IEnumerable<EliteAPI.PartyMember> cParty = _ELITEAPIMonitored.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == _ELITEAPIPL.Player.ZoneId);

                        // Make sure member number is not 0 (null) or 4 (void)
                        if (memberOF != 0 && memberOF != 4)
                        {
                            // Run through Each party member as we're looking for either a specifc name or if set otherwise anyone with the MP criteria in the current party.
                            foreach (EliteAPI.PartyMember pData in cParty)
                            {
                                // If party of party v1
                                if (memberOF == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5)
                                {
                                    if (!string.IsNullOrEmpty(pData.Name) && pData.Name != _ELITEAPIPL.Player.Name)
                                    {
                                        if ((OptionsForm.config.DevotionTargetType == 0))
                                        {
                                            if (pData.Name == OptionsForm.config.DevotionTargetName)
                                            {
                                                EliteAPI.XiEntity playerInfo = _ELITEAPIPL.Entity.GetEntity((int)pData.TargetIndex);
                                                if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= OptionsForm.config.DevotionMP && pData.CurrentMPP <= 30)
                                                {
                                                    _ELITEAPIPL.ThirdParty.SendString("/ja \"Devotion\" " + OptionsForm.config.DevotionTargetName);
                                                    Thread.Sleep(TimeSpan.FromSeconds(2));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            EliteAPI.XiEntity playerInfo = _ELITEAPIPL.Entity.GetEntity((int)pData.TargetIndex);

                                            if ((pData.CurrentMP <= OptionsForm.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 30)
                                            {
                                                _ELITEAPIPL.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
                                                Thread.Sleep(TimeSpan.FromSeconds(2));
                                                break;
                                            }
                                        }
                                    }
                                } // If part of party 2
                                else if (memberOF == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11)
                                {
                                    if (!string.IsNullOrEmpty(pData.Name) && pData.Name != _ELITEAPIPL.Player.Name)
                                    {
                                        if ((OptionsForm.config.DevotionTargetType == 0))
                                        {
                                            if (pData.Name == OptionsForm.config.DevotionTargetName)
                                            {
                                                EliteAPI.XiEntity playerInfo = _ELITEAPIPL.Entity.GetEntity((int)pData.TargetIndex);
                                                if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= OptionsForm.config.DevotionMP)
                                                {
                                                    _ELITEAPIPL.ThirdParty.SendString("/ja \"Devotion\" " + OptionsForm.config.DevotionTargetName);
                                                    Thread.Sleep(TimeSpan.FromSeconds(2));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            EliteAPI.XiEntity playerInfo = _ELITEAPIPL.Entity.GetEntity((int)pData.TargetIndex);

                                            if ((pData.CurrentMP <= OptionsForm.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 50)
                                            {
                                                _ELITEAPIPL.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
                                                Thread.Sleep(TimeSpan.FromSeconds(2));
                                                break;
                                            }
                                        }
                                    }
                                } // If part of party 3
                                else if (memberOF == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17)
                                {
                                    if (!string.IsNullOrEmpty(pData.Name) && pData.Name != _ELITEAPIPL.Player.Name)
                                    {
                                        if ((OptionsForm.config.DevotionTargetType == 0))
                                        {
                                            if (pData.Name == OptionsForm.config.DevotionTargetName)
                                            {
                                                EliteAPI.XiEntity playerInfo = _ELITEAPIPL.Entity.GetEntity((int)pData.TargetIndex);
                                                if (playerInfo.Distance < 10 && playerInfo.Distance > 0 && pData.CurrentMP <= OptionsForm.config.DevotionMP)
                                                {
                                                    _ELITEAPIPL.ThirdParty.SendString("/ja \"Devotion\" " + OptionsForm.config.DevotionTargetName);
                                                    Thread.Sleep(TimeSpan.FromSeconds(2));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            EliteAPI.XiEntity playerInfo = _ELITEAPIPL.Entity.GetEntity((int)pData.TargetIndex);

                                            if ((pData.CurrentMP <= OptionsForm.config.DevotionMP) && (playerInfo.Distance < 10) && pData.CurrentMPP <= 50)
                                            {
                                                _ELITEAPIPL.ThirdParty.SendString("/ja \"Devotion\" " + pData.Name);
                                                Thread.Sleep(TimeSpan.FromSeconds(2));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }







                var playerBuffOrder = _ELITEAPIMonitored.Party.GetPartyMembers().OrderBy(p => p.MemberNumber).OrderBy(p => p.Active == 0).Where(p => p.Active == 1);

                string[] shell_spells = { "Shell", "Shell II", "Shell III", "Shell IV", "Shell V" };
                string[] protect_spells = { "Protect", "Protect II", "Protect III", "Protect IV", "Protect V" };
                string[] regen_spells = { "Regen", "Regen II", "Regen III", "Regen IV", "Regen V" };
                string[] refresh_spells = { "Refresh", "Refresh II", "Refresh III" };

                // Auto Casting
                foreach (var charDATA in playerBuffOrder)
                {
                    // Grab the Storm Spells name to perform checks.
                    string StormSpell_Enabled = CheckStormspell(charDATA.MemberNumber);

                    // Grab storm spell Data for Buff ID etc...
                    SpellsData PTstormspell = stormspells.Where(c => c.Spell_Name == StormSpell_Enabled).SingleOrDefault();

                    // PL BASED BUFFS
                    if (_ELITEAPIPL.Player.Name == charDATA.Name)
                    {

                        if (autoHasteEnabled[charDATA.MemberNumber] && CheckSpellRecast("Haste") == 0 && CanCastSpell("Haste") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !plStatusCheck(StatusEffect.Haste) && !plStatusCheck(StatusEffect.Slow))
                        {
                            hastePlayer(charDATA.MemberNumber);
                        }
                        if (autoHaste_IIEnabled[charDATA.MemberNumber] && CheckSpellRecast("Haste II") == 0 && CanCastSpell("Haste II") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !plStatusCheck(StatusEffect.Haste) && !plStatusCheck(StatusEffect.Slow))
                        {
                            haste_IIPlayer(charDATA.MemberNumber);
                        }
                        if (autoAdloquium_Enabled[charDATA.MemberNumber] && CheckSpellRecast("Adloquium") == 0 && CanCastSpell("Adloquium") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !BuffChecker(170, 0))
                        {
                            AdloquiumPlayer(charDATA.MemberNumber);
                        }
                        if (autoFlurryEnabled[charDATA.MemberNumber] && CheckSpellRecast("Flurry") == 0 && CanCastSpell("Flurry") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !BuffChecker(581, 0) && !plStatusCheck(StatusEffect.Slow))
                        {
                            FlurryPlayer(charDATA.MemberNumber);
                        }
                        if (autoFlurry_IIEnabled[charDATA.MemberNumber] && CheckSpellRecast("Flurry II") == 0 && CanCastSpell("Flurry II") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !BuffChecker(581, 0) && !plStatusCheck(StatusEffect.Slow))
                        {
                            Flurry_IIPlayer(charDATA.MemberNumber);
                        }
                        if (autoShell_Enabled[charDATA.MemberNumber] && CheckSpellRecast(shell_spells[OptionsForm.config.autoShell_Spell]) == 0 && CanCastSpell(shell_spells[OptionsForm.config.autoShell_Spell]) && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && _ELITEAPIPL.Player.Status != 33 && !plStatusCheck(StatusEffect.Shell))
                        {
                            shellPlayer(charDATA.MemberNumber);
                        }
                        if (autoProtect_Enabled[charDATA.MemberNumber] && CheckSpellRecast(protect_spells[OptionsForm.config.autoProtect_Spell]) == 0 && CanCastSpell(protect_spells[OptionsForm.config.autoProtect_Spell]) && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && _ELITEAPIPL.Player.Status != 33 && !plStatusCheck(StatusEffect.Protect))
                        {
                            protectPlayer(charDATA.MemberNumber);
                        }
                        if ((autoPhalanx_IIEnabled[charDATA.MemberNumber]) && (CheckSpellRecast("Phalanx II") == 0) && (CanCastSpell("Phalanx II")) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !plStatusCheck(StatusEffect.Phalanx))
                        {
                            Phalanx_IIPlayer(charDATA.MemberNumber);
                        }
                        if ((autoRegen_Enabled[charDATA.MemberNumber]) && (CheckSpellRecast(regen_spells[OptionsForm.config.autoRegen_Spell]) == 0) && (CanCastSpell(regen_spells[OptionsForm.config.autoRegen_Spell])) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !plStatusCheck(StatusEffect.Regen))
                        {
                            Regen_Player(charDATA.MemberNumber);
                        }
                        if ((autoRefreshEnabled[charDATA.MemberNumber]) && (CheckSpellRecast(refresh_spells[OptionsForm.config.autoRefresh_Spell]) == 0) && (CanCastSpell(refresh_spells[OptionsForm.config.autoRefresh_Spell])) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !plStatusCheck(StatusEffect.Refresh))
                        {
                            Refresh_Player(charDATA.MemberNumber);
                        }
                        if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !BuffChecker(PTstormspell.buffID, 0) && CheckSpellRecast(PTstormspell.Spell_Name) == 0 && CanCastSpell(PTstormspell.Spell_Name))
                        {
                            StormSpellPlayer(charDATA.MemberNumber, PTstormspell.Spell_Name);
                        }
                    }
                    // MONITORED PLAYER BASED BUFFS
                    else if (_ELITEAPIMonitored.Player.Name == charDATA.Name)
                    {
                        if (autoHasteEnabled[charDATA.MemberNumber] && CheckSpellRecast("Haste") == 0 && CanCastSpell("Haste") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !monitoredStatusCheck(StatusEffect.Haste) && !monitoredStatusCheck(StatusEffect.Slow))
                        {
                            hastePlayer(charDATA.MemberNumber);
                        }
                        if (autoHaste_IIEnabled[charDATA.MemberNumber] && CheckSpellRecast("Haste II") == 0 && CanCastSpell("Haste II") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !monitoredStatusCheck(StatusEffect.Haste) && !monitoredStatusCheck(StatusEffect.Slow))
                        {
                            haste_IIPlayer(charDATA.MemberNumber);
                        }
                        if (autoAdloquium_Enabled[charDATA.MemberNumber] && CheckSpellRecast("Adloquium") == 0 && CanCastSpell("Adloquium") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !BuffChecker(170, 1))
                        {
                            AdloquiumPlayer(charDATA.MemberNumber);
                        }
                        if (autoFlurryEnabled[charDATA.MemberNumber] && CheckSpellRecast("Flurry") == 0 && CanCastSpell("Flurry") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !BuffChecker(581, 1) && !monitoredStatusCheck(StatusEffect.Slow))
                        {
                            FlurryPlayer(charDATA.MemberNumber);
                        }
                        if (autoFlurry_IIEnabled[charDATA.MemberNumber] && CheckSpellRecast("Flurry II") == 0 && CanCastSpell("Flurry II") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && !BuffChecker(581, 1) && !monitoredStatusCheck(StatusEffect.Slow))
                        {
                            Flurry_IIPlayer(charDATA.MemberNumber);
                        }
                        if (autoShell_Enabled[charDATA.MemberNumber] && CheckSpellRecast(shell_spells[OptionsForm.config.autoShell_Spell]) == 0 && CanCastSpell(shell_spells[OptionsForm.config.autoShell_Spell]) && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && _ELITEAPIPL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Shell))
                        {
                            shellPlayer(charDATA.MemberNumber);
                        }
                        if (autoProtect_Enabled[charDATA.MemberNumber] && CheckSpellRecast(protect_spells[OptionsForm.config.autoProtect_Spell]) == 0 && CanCastSpell(protect_spells[OptionsForm.config.autoProtect_Spell]) && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && _ELITEAPIPL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Protect))
                        {
                            protectPlayer(charDATA.MemberNumber);
                        }
                        if ((autoPhalanx_IIEnabled[charDATA.MemberNumber]) && (CheckSpellRecast("Phalanx II") == 0) && (CanCastSpell("Phalanx II")) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Phalanx))
                        {
                            Phalanx_IIPlayer(charDATA.MemberNumber);
                        }
                        if ((autoRegen_Enabled[charDATA.MemberNumber]) && (CheckSpellRecast(regen_spells[OptionsForm.config.autoRegen_Spell]) == 0) && (CanCastSpell(regen_spells[OptionsForm.config.autoRegen_Spell])) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Regen))
                        {
                            Regen_Player(charDATA.MemberNumber);
                        }
                        if ((autoRefreshEnabled[charDATA.MemberNumber]) && (CheckSpellRecast(refresh_spells[OptionsForm.config.autoRefresh_Spell]) == 0) && (CanCastSpell(refresh_spells[OptionsForm.config.autoRefresh_Spell])) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !monitoredStatusCheck(StatusEffect.Refresh))
                        {
                            Refresh_Player(charDATA.MemberNumber);
                        }
                        if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && !BuffChecker(PTstormspell.buffID, 1) && CheckSpellRecast(PTstormspell.Spell_Name) == 0 && CanCastSpell(PTstormspell.Spell_Name))
                        {
                            StormSpellPlayer(charDATA.MemberNumber, PTstormspell.Spell_Name);
                        }
                    }
                    else
                    {
                        if (autoHasteEnabled[charDATA.MemberNumber] && CheckSpellRecast("Haste") == 0 && CanCastSpell("Haste") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && playerHasteSpan[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoHasteMinutes)
                        {
                            hastePlayer(charDATA.MemberNumber);
                        }
                        if (autoHaste_IIEnabled[charDATA.MemberNumber] && CheckSpellRecast("Haste II") == 0 && CanCastSpell("Haste II") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && playerHaste_IISpan[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoHasteMinutes)
                        {
                            haste_IIPlayer(charDATA.MemberNumber);
                        }
                        if (autoAdloquium_Enabled[charDATA.MemberNumber] && CheckSpellRecast("Adloquium") == 0 && CanCastSpell("Adloquium") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && playerAdloquium_Span[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoAdloquiumMinutes)
                        {
                            AdloquiumPlayer(charDATA.MemberNumber);
                        }
                        if (autoFlurryEnabled[charDATA.MemberNumber] && CheckSpellRecast("Flurry") == 0 && CanCastSpell("Flurry") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && playerFlurrySpan[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoHasteMinutes)
                        {
                            FlurryPlayer(charDATA.MemberNumber);
                        }
                        if (autoFlurry_IIEnabled[charDATA.MemberNumber] && CheckSpellRecast("Flurry II") == 0 && CanCastSpell("Flurry II") && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && playerHasteSpan[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoHasteMinutes)
                        {
                            Flurry_IIPlayer(charDATA.MemberNumber);
                        }
                        if (autoShell_Enabled[charDATA.MemberNumber] && CheckSpellRecast(shell_spells[OptionsForm.config.autoShell_Spell]) == 0 && CanCastSpell(shell_spells[OptionsForm.config.autoShell_Spell]) && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && _ELITEAPIPL.Player.Status != 33 && playerShell_Span[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoShellMinutes)
                        {
                            shellPlayer(charDATA.MemberNumber);
                        }
                        if (autoProtect_Enabled[charDATA.MemberNumber] && CheckSpellRecast(protect_spells[OptionsForm.config.autoProtect_Spell]) == 0 && CanCastSpell(protect_spells[OptionsForm.config.autoProtect_Spell]) && _ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue && CanCastOnPartyMember(charDATA.MemberNumber) && _ELITEAPIPL.Player.Status != 33 && playerProtect_Span[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoProtect_Minutes)
                        {
                            protectPlayer(charDATA.MemberNumber);
                        }
                        if ((autoPhalanx_IIEnabled[charDATA.MemberNumber]) && (CheckSpellRecast("Phalanx II") == 0) && (CanCastSpell("Phalanx II")) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && playerPhalanx_IISpan[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoPhalanxIIMinutes)
                        {
                            Phalanx_IIPlayer(charDATA.MemberNumber);
                        }
                        if ((autoRegen_Enabled[charDATA.MemberNumber]) && (CheckSpellRecast(regen_spells[OptionsForm.config.autoRegen_Spell]) == 0) && (CanCastSpell(regen_spells[OptionsForm.config.autoRegen_Spell])) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && playerRegen_Span[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoRegen_Minutes)
                        {
                            Regen_Player(charDATA.MemberNumber);
                        }
                        if ((autoRefreshEnabled[charDATA.MemberNumber]) && (CheckSpellRecast(refresh_spells[OptionsForm.config.autoRefresh_Spell]) == 0) && (CanCastSpell(refresh_spells[OptionsForm.config.autoRefresh_Spell])) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && playerRefresh_Span[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoRefresh_Minutes)
                        {
                            Refresh_Player(charDATA.MemberNumber);
                        }
                        if (CheckIfAutoStormspellEnabled(charDATA.MemberNumber) && (_ELITEAPIPL.Player.MP > OptionsForm.config.mpMinCastValue) && (CanCastOnPartyMember(charDATA.MemberNumber)) && _ELITEAPIPL.Player.Status != 33 && CheckSpellRecast(PTstormspell.Spell_Name) == 0 && CanCastSpell(PTstormspell.Spell_Name) && playerStormspellSpan[charDATA.MemberNumber].Minutes >= OptionsForm.config.autoStormspellMinutes)
                        {
                            StormSpellPlayer(charDATA.MemberNumber, PTstormspell.Spell_Name);
                        }
                    }
                }
            }
        }

        private void RunCuragas(bool[] partyMembersEnabled)
        {
            int memberOF_curaga = GeneratePT_structure();

            if (memberOF_curaga == 0 && memberOF_curaga == 4)
            {
                return;
            }

            var cParty_curaga = _ELITEAPIMonitored.Party.GetPartyMembers()
                .Where(p => partyMembersEnabled[p.MemberNumber])
                .Where(p => p.Active == 1 && p.CurrentHP > 0)
                .Where(p => p.CurrentHPP <= OptionsForm.config.curagaCurePercentage)
                .Where(CanCastOnPartyMember)
                .OrderBy(p => p.CurrentHPP);

            if (cParty_curaga.Count() >= OptionsForm.config.curagaRequiredMembers)
            {
                CuragaCalculator(cParty_curaga.First());
            }
        }

        private void RunCures(bool[] partyMembersEnabled)
        {
            // Set array values for GUI "High Priority" checkboxes
            var highPriorityBoxes = new bool[18]
            {
                player0priority.Checked,
                player1priority.Checked,
                player2priority.Checked,
                player3priority.Checked,
                player4priority.Checked,
                player5priority.Checked,
                player6priority.Checked,
                player7priority.Checked,
                player8priority.Checked,
                player9priority.Checked,
                player10priority.Checked,
                player11priority.Checked,
                player12priority.Checked,
                player13priority.Checked,
                player14priority.Checked,
                player15priority.Checked,
                player16priority.Checked,
                player17priority.Checked,
            };

            var playerHpOrder = _ELITEAPIMonitored.Party.GetPartyMembers()
                .Where(p => partyMembersEnabled[p.MemberNumber])
                .Where(p => p.Active == 1 && p.CurrentHP > 0)
                .Where(CanCastOnPartyMember)
                .OrderByDescending(p => {
                    return OptionsForm.config.enableMonitoredPriority
                        && p.ID == _ELITEAPIMonitored.Player.TargetID;
                })
                .OrderByDescending(p => {
                    return highPriorityBoxes[p.MemberNumber]
                        || p.CurrentHP <= OptionsForm.config.priorityCurePercentage;
                })
                .OrderBy(p => p.CurrentHPP);

            if (playerHpOrder.Any())
            {
                CureCalculator(playerHpOrder.First(), true);
            }
        }

        private void Pause()
        {
            pauseButton.Text = "Paused!";
            pauseButton.ForeColor = Color.Red;

            actionTimer.Enabled = false;

            ActiveBuffs.Clear();

            pauseActions = true;

            song_casting = 0;
            ForceSongRecast = true;

            if (!OptionsForm.config.FFXIDefaultAutoFollow)
            {
                _ELITEAPIPL.AutoFollow.IsAutoFollowing = false;
            }
        }

        private bool CheckIfAutoStormspellEnabled(byte id)
        {

            if (OptionsForm.config.autoStorm_Spell == 0)
            {
                if (autoSandstormEnabled[id])
                {
                    return true;
                }
                else if (autoWindstormEnabled[id])
                {
                    return true;
                }
                else if (autoFirestormEnabled[id])
                {
                    return true;
                }
                else if (autoRainstormEnabled[id])
                {
                    return true;
                }
                else if (autoHailstormEnabled[id])
                {
                    return true;
                }
                else if (autoThunderstormEnabled[id])
                {
                    return true;
                }
                else if (autoVoidstormEnabled[id])
                {
                    return true;
                }
                else if (autoAurorastormEnabled[id])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (OptionsForm.config.autoStorm_Spell == 1)
            {
                if (autoSandstormEnabled[id])
                {
                    return true;
                }
                else if (autoWindstormEnabled[id])
                {
                    return true;
                }
                else if (autoFirestormEnabled[id])
                {
                    return true;
                }
                else if (autoRainstormEnabled[id])
                {
                    return true;
                }
                else if (autoHailstormEnabled[id])
                {
                    return true;
                }
                else if (autoThunderstormEnabled[id])
                {
                    return true;
                }

                else if (autoVoidstormEnabled[id])
                {
                    return true;
                }
                else if (autoAurorastormEnabled[id])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private string CheckStormspell(byte id)
        {
            if (OptionsForm.config.autoStorm_Spell == 0)
            {
                if (autoSandstormEnabled[id])
                {
                    return "Sandstorm";
                }
                else if (autoWindstormEnabled[id])
                {
                    return "Windstorm";
                }
                else if (autoFirestormEnabled[id])
                {
                    return "Firestorm";
                }
                else if (autoRainstormEnabled[id])
                {
                    return "Rainstorm";
                }
                else if (autoHailstormEnabled[id])
                {
                    return "Hailstorm";
                }
                else if (autoThunderstormEnabled[id])
                {
                    return "Thunderstorm";
                }
                else if (autoVoidstormEnabled[id])
                {
                    return "Voidstorm";
                }
                else if (autoAurorastormEnabled[id])
                {
                    return "Aurorastorm";
                }
                else
                {
                    return "false";
                }
            }
            else if (OptionsForm.config.autoStorm_Spell == 1)
            {
                if (autoSandstormEnabled[id])
                {
                    return "Sandstorm II";
                }
                else if (autoWindstormEnabled[id])
                {
                    return "Windstorm II";
                }
                else if (autoFirestormEnabled[id])
                {
                    return "Firestorm II";
                }
                else if (autoRainstormEnabled[id])
                {
                    return "Rainstorm II";
                }
                else if (autoHailstormEnabled[id])
                {
                    return "Hailstorm II";
                }
                else if (autoThunderstormEnabled[id])
                {
                    return "Thunderstorm II";
                }

                else if (autoVoidstormEnabled[id])
                {
                    return "Voidstorm II";
                }
                else if (autoAurorastormEnabled[id])
                {
                    return "Aurorastorm II";
                }
                else
                {
                    return "false";
                }
            }
            else
            {
                return "false";
            }
        }

        private string GetShellraLevel(decimal p)
        {
            switch ((int)p)
            {
                case 1:
                    return "Shellra";

                case 2:
                    return "Shellra II";

                case 3:
                    return "Shellra III";

                case 4:
                    return "Shellra IV";

                case 5:
                    return "Shellra V";

                default:
                    return "Shellra";
            }
        }

        private string GetProtectraLevel(decimal p)
        {
            switch ((int)p)
            {
                case 1:
                    return "Protectra";

                case 2:
                    return "Protectra II";

                case 3:
                    return "Protectra III";

                case 4:
                    return "Protectra IV";

                case 5:
                    return "Protectra V";

                default:
                    return "Protectra";
            }
        }

        private string ReturnGeoSpell(int GEOSpell_ID, int GeoSpell_Type)
        {
            // GRAB THE SPELL FROM THE CUSTOM LIST
            GeoData GeoSpell = GeomancerInfo.Where(c => c.geo_position == GEOSpell_ID).FirstOrDefault();

            if (GeoSpell_Type == 1)
            {
                if (CanCastSpell(GeoSpell.indi_spell))
                {
                    if (CheckSpellRecast(GeoSpell.indi_spell) == 0)
                    {
                        return GeoSpell.indi_spell;
                    }
                    else
                    {
                        return "SpellRecast";
                    }
                }
                else
                {
                    return "SpellNA";
                }
            }
            else if (GeoSpell_Type == 2)
            {
                if (CanCastSpell(GeoSpell.geo_spell))
                {
                    if (CheckSpellRecast(GeoSpell.geo_spell) == 0)
                    {
                        return GeoSpell.geo_spell;
                    }
                    else
                    {
                        return "SpellRecast";
                    }
                }
                else
                {
                    return "SpellNA";
                }
            }
            else
            {
                return "SpellError_Cancel";
            }
        }

        private void player0optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 0;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[0];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[0];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[0];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[0];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[0];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[0];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[0];

            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player1optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 1;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[1];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[1];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[1];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[1];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[1];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[1];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[1];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player2optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 2;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[2];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[2];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[2];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[2];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[2];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[2];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[2];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player3optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 3;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[3];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[3];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[3];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[3];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[3];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[3];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[3];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player4optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 4;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[4];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[4];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[4];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[4];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[4];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[4];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[4];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player5optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 5;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[5];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[5];
            autoAdloquiumToolStripMenuItem.Checked = autoAdloquium_Enabled[5];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[5];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[5];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[5];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[5];
            playerOptions.Show(party0, new Point(0, 0));
        }

        private void player6optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 6;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[6];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[6];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[6];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[6];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[6];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[6];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player7optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 7;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[7];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[7];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[7];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[7];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[7];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[7];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player8optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 8;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[8];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[8];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[8];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[8];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[8];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[8];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player9optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 9;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[9];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[9];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[9];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[9];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[9];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[9];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player10optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 10;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[10];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[10];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[10];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[10];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[10];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[10];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player11optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 11;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[11];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[11];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[11];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[11];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[11];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[11];
            playerOptions.Show(party1, new Point(0, 0));
        }

        private void player12optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 12;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[12];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[12];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[12];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[12];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[12];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[12];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player13optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 13;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[13];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[13];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[13];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[13];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[13];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[13];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player14optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 14;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[14];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[14];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[14];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[14];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[14];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[14];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player15optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 15;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[15];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[15];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[15];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[15];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[15];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[15];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player16optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 16;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[16];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[16];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[16];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[16];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[16];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[16];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player17optionsButton_Click(object sender, EventArgs e)
        {
            playerOptionsSelected = 17;
            autoHasteToolStripMenuItem.Checked = autoHasteEnabled[17];
            autoHasteIIToolStripMenuItem.Checked = autoHaste_IIEnabled[17];
            autoFlurryToolStripMenuItem.Checked = autoFlurryEnabled[17];
            autoFlurryIIToolStripMenuItem.Checked = autoFlurry_IIEnabled[17];
            autoProtectToolStripMenuItem.Checked = autoProtect_Enabled[17];
            autoShellToolStripMenuItem.Checked = autoShell_Enabled[17];
            playerOptions.Show(party2, new Point(0, 0));
        }

        private void player0buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 0;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[0];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[0];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[0];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[0];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[0];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[0];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[0];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[0];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[0];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[0];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[0];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player1buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 1;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[1];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[1];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[1];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[1];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[1];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[1];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[1];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[1];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[1];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[1];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[1];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player2buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 2;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[2];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[2];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[2];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[2];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[2];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[2];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[2];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[2];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[2];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[2];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[2];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player3buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 3;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[3];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[3];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[3];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[3];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[3];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[3];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[3];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[3];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[3];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[3];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[3];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player4buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 4;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[4];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[4];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[4];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[4];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[4];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[4];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[4];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[4];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[4];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[4];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[4];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void player5buffsButton_Click(object sender, EventArgs e)
        {
            autoOptionsSelected = 5;
            autoPhalanxIIToolStripMenuItem1.Checked = autoPhalanx_IIEnabled[5];
            autoRegenVToolStripMenuItem.Checked = autoRegen_Enabled[5];
            autoRefreshIIToolStripMenuItem.Checked = autoRefreshEnabled[5];
            SandstormToolStripMenuItem.Checked = autoSandstormEnabled[5];
            RainstormToolStripMenuItem.Checked = autoRainstormEnabled[5];
            WindstormToolStripMenuItem.Checked = autoWindstormEnabled[5];
            FirestormToolStripMenuItem.Checked = autoFirestormEnabled[5];
            HailstormToolStripMenuItem.Checked = autoHailstormEnabled[5];
            ThunderstormToolStripMenuItem.Checked = autoThunderstormEnabled[5];
            VoidstormToolStripMenuItem.Checked = autoVoidstormEnabled[5];
            AurorastormToolStripMenuItem.Checked = autoAurorastormEnabled[5];
            autoOptions.Show(party0, new Point(0, 0));
        }

        private void Item_Wait(string ItemName)
        {
            if (CastingBackground_Check != true && JobAbilityLock_Check != true)
            {
                Invoke((MethodInvoker)(async () =>
                {
                    JobAbilityLock_Check = true;
                    castingLockLabel.Text = "Casting is LOCKED for ITEM Use.";
                    currentAction.Text = "Using an Item: " + ItemName;
                    _ELITEAPIPL.ThirdParty.SendString("/item \"" + ItemName + "\" <me>");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    castingLockLabel.Text = "Casting is UNLOCKED";
                    currentAction.Text = string.Empty;
                    castingSpell = string.Empty;
                    JobAbilityLock_Check = false;
                }));
            }
        }

        private void JobAbility_Wait(string JobabilityDATA, string JobAbilityName)
        {
            if (CastingBackground_Check != true && JobAbilityLock_Check != true)
            {
                Invoke((MethodInvoker)(async () =>
          {
              JobAbilityLock_Check = true;
              castingLockLabel.Text = "Casting is LOCKED for a JA.";
              currentAction.Text = "Using a Job Ability: " + JobabilityDATA;
              _ELITEAPIPL.ThirdParty.SendString("/ja \"" + JobAbilityName + "\" <me>");
              await Task.Delay(TimeSpan.FromSeconds(2));
              castingLockLabel.Text = "Casting is UNLOCKED";
              currentAction.Text = string.Empty;
              castingSpell = string.Empty;
              JobAbilityLock_Check = false;
          }));
            }
        }

        private void autoHasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoHasteEnabled[playerOptionsSelected] = !autoHasteEnabled[playerOptionsSelected];
            autoHaste_IIEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoHasteIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoHaste_IIEnabled[playerOptionsSelected] = !autoHaste_IIEnabled[playerOptionsSelected];
            autoHasteEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoAdloquiumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoAdloquium_Enabled[playerOptionsSelected] = !autoAdloquium_Enabled[playerOptionsSelected];
        }

        private void autoFlurryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoFlurryEnabled[playerOptionsSelected] = !autoFlurryEnabled[playerOptionsSelected];
            autoHasteEnabled[playerOptionsSelected] = false;
            autoHaste_IIEnabled[playerOptionsSelected] = false;
            autoFlurry_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoFlurryIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoFlurry_IIEnabled[playerOptionsSelected] = !autoFlurry_IIEnabled[playerOptionsSelected];
            autoHasteEnabled[playerOptionsSelected] = false;
            autoFlurryEnabled[playerOptionsSelected] = false;
            autoHaste_IIEnabled[playerOptionsSelected] = false;
        }

        private void autoProtectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoProtect_Enabled[playerOptionsSelected] = !autoProtect_Enabled[playerOptionsSelected];
        }

        private void enableDebuffRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string generated_name = _ELITEAPIMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name.ToLower();
            characterNames_naRemoval.Add(generated_name);
        }

        private void autoShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoShell_Enabled[playerOptionsSelected] = !autoShell_Enabled[playerOptionsSelected];
        }

        private void autoPhalanxIIToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            autoPhalanx_IIEnabled[autoOptionsSelected] = !autoPhalanx_IIEnabled[autoOptionsSelected];
        }

        private void autoRegenVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoRegen_Enabled[autoOptionsSelected] = !autoRegen_Enabled[autoOptionsSelected];
        }

        private void autoRefreshIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoRefreshEnabled[autoOptionsSelected] = !autoRefreshEnabled[autoOptionsSelected];
        }

        private void hasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hastePlayer(playerOptionsSelected);
        }

        private void followToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm.config.autoFollowName = _ELITEAPIMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void stopfollowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm.config.autoFollowName = string.Empty;
        }

        private void EntrustTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm.config.EntrustedSpell_Target = _ELITEAPIMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void GeoTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm.config.LuopanSpell_Target = _ELITEAPIMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void DevotionTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm.config.DevotionTargetName = _ELITEAPIMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void HateEstablisherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm.config.autoTarget_Target = _ELITEAPIMonitored.Party.GetPartyMembers()[playerOptionsSelected].Name;
        }

        private void setAllStormsFalse(byte autoOptionsSelected)
        {
            // MessageBox.Show("SONG DATA: " + activeStorm + " " + autoOptionsSelected);

            autoSandstormEnabled[autoOptionsSelected] = false;
            autoRainstormEnabled[autoOptionsSelected] = false;
            autoFirestormEnabled[autoOptionsSelected] = false;
            autoWindstormEnabled[autoOptionsSelected] = false;
            autoHailstormEnabled[autoOptionsSelected] = false;
            autoThunderstormEnabled[autoOptionsSelected] = false;
            autoVoidstormEnabled[autoOptionsSelected] = false;
            autoAurorastormEnabled[autoOptionsSelected] = false;
        }

        private void SandstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoSandstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoSandstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void RainstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoRainstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoRainstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void WindstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoWindstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoWindstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void FirestormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoFirestormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoFirestormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void HailstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoHailstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoHailstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void ThunderstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoThunderstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoThunderstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void VoidstormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoVoidstormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoVoidstormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void AurorastormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool currentStatus = autoAurorastormEnabled[autoOptionsSelected];
            setAllStormsFalse(autoOptionsSelected);
            autoAurorastormEnabled[autoOptionsSelected] = !currentStatus;
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            song_casting = 0;
            ForceSongRecast = true;

            if (pauseActions == false)
            {
                Pause();
            }
            else
            {
                pauseButton.Text = "Pause";
                pauseButton.ForeColor = Color.Black;
                actionTimer.Enabled = true;
                pauseActions = false;

                if (OptionsForm.config.MinimiseonStart == true && WindowState != FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Minimized;
                }

                LoadLuaAddon();
            }
        }

        private void Debug_Click(object sender, EventArgs e)
        {
            if (_ELITEAPIMonitored == null)
            {

                MessageBox.Show("Attach to process before pressing this button", "Error");
                return;
            }

            MessageBox.Show(debug_MSG_show);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (TopMost)
            {
                TopMost = false;
            }
            else
            {
                TopMost = true;
            }
        }

        private void MouseClickTray(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && Visible == false)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            else
            {
                Hide();
                WindowState = FormWindowState.Minimized;
            }
        }

        private bool CheckShellraLevelPossession()
        {
            switch ((int)OptionsForm.config.plShellra_Level)
            {
                case 1:
                    if (CanCastSpell("Shellra") == true && CheckSpellRecast("Shellra") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 2:
                    if (CanCastSpell("Shellra II") == true && CheckSpellRecast("Shellra II") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 3:
                    if (CanCastSpell("Shellra III") == true && CheckSpellRecast("Shellra III") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 4:
                    if (CanCastSpell("Shellra IV") == true && CheckSpellRecast("Shellra IV") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 5:
                    if (CanCastSpell("Shellra V") == true && CheckSpellRecast("Shellra V") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        private bool CheckProtectraLevelPossession()
        {
            switch ((int)OptionsForm.config.plProtectra_Level)
            {
                case 1:
                    if (CanCastSpell("Protectra") == true && CheckSpellRecast("Protectra") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 2:
                    if (CanCastSpell("Protectra II") == true && CheckSpellRecast("Protectra II") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 3:
                    if (CanCastSpell("Protectra III") == true && CheckSpellRecast("Protectra III") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 4:
                    if (CanCastSpell("Protectra IV") == true && CheckSpellRecast("Protectra IV") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 5:
                    if (CanCastSpell("Protectra V") == true && CheckSpellRecast("Protectra V") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        private bool CheckReraiseLevelPossession()
        {
            switch (OptionsForm.config.plReraise_Level)
            {
                case 1:
                    if (CanCastSpell("Reraise") == true && CheckSpellRecast("Reraise") == 0)
                    {
                        // Check SCH possiblity
                        if (_ELITEAPIPL.Player.MainJob == 20 && _ELITEAPIPL.Player.SubJob != 3 && !BuffChecker(401, 0))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                case 2:

                    if (CanCastSpell("Reraise II") == true && CheckSpellRecast("Reraise II") == 0)
                    {
                        if (_ELITEAPIPL.Player.MainJob == 20 && !BuffChecker(401, 0))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                case 3:

                    if (CanCastSpell("Reraise III") == true && CheckSpellRecast("Reraise III") == 0)
                    {
                        if (_ELITEAPIPL.Player.MainJob == 20 && !BuffChecker(401, 0))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                case 4:
                    if (CanCastSpell("Reraise IV") == true && CheckSpellRecast("Reraise IV") == 0)
                    {
                        if (_ELITEAPIPL.Player.MainJob == 20 && !BuffChecker(401, 0))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        private bool CheckRefreshLevelPossession()
        {
            switch (OptionsForm.config.plRefresh_Level)
            {
                case 1:
                    return CanCastSpell("Refresh");

                case 2:
                    return CanCastSpell("Refresh II");

                case 3:
                    return CanCastSpell("Refresh III");

                default:
                    return false;
            }
        }

        private bool CheckRegenLevelPossession()
        {
            switch (OptionsForm.config.plRegen_Level)
            {
                case 1:
                    return CanCastSpell("Regen");

                case 2:
                    return CanCastSpell("Regen II");

                case 3:
                    return CanCastSpell("Regen III");

                case 4:
                    return CanCastSpell("Regen IV");

                case 5:
                    return CanCastSpell("Regen V");

                default:
                    return false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Dispose();

            if (_ELITEAPIPL == null)
            {
                return;
            }

            AddonManager.Unload();
            if (OptionsForm.config.enableHotKeys)
            {
                AddonManager.UnbindHotKeys();
            }
        }

        private int followID()
        {
            if ((setinstance2.Enabled == true) && !string.IsNullOrEmpty(OptionsForm.config.autoFollowName) && !pauseActions)
            {
                for (int x = 0; x < 2048; x++)
                {
                    EliteAPI.XiEntity entity = _ELITEAPIPL.Entity.GetEntity(x);

                    if (entity.Name != null && entity.Name.ToLower().Equals(OptionsForm.config.autoFollowName.ToLower()))
                    {
                        return Convert.ToInt32(entity.TargetID);
                    }
                }
                return -1;
            }
            else
            {
                return -1;
            }
        }

        private void showErrorMessage(string ErrorMessage)
        {
            pauseActions = true;
            pauseButton.Text = "Error!";
            pauseButton.ForeColor = Color.Red;
            actionTimer.Enabled = false;
            MessageBox.Show(ErrorMessage);
        }

        public bool plMonitoredSameParty()
        {
            int PT_Structutre_NO = GeneratePT_structure();

            // Now generate the party
            IEnumerable<EliteAPI.PartyMember> cParty = _ELITEAPIMonitored.Party.GetPartyMembers().Where(p => p.Active != 0 && p.Zone == _ELITEAPIPL.Player.ZoneId);

            // Make sure member number is not 0 (null) or 4 (void)
            if (PT_Structutre_NO != 0 && PT_Structutre_NO != 4)
            {
                // Run through Each party member as we're looking for either a specific name or if set
                // otherwise anyone with the MP criteria in the current party.
                foreach (EliteAPI.PartyMember pData in cParty)
                {
                    if (PT_Structutre_NO == 1 && pData.MemberNumber >= 0 && pData.MemberNumber <= 5 && pData.Name == _ELITEAPIMonitored.Player.Name)
                    {
                        return true;
                    }
                    else if (PT_Structutre_NO == 2 && pData.MemberNumber >= 6 && pData.MemberNumber <= 11 && pData.Name == _ELITEAPIMonitored.Player.Name)
                    {
                        return true;
                    }
                    else if (PT_Structutre_NO == 3 && pData.MemberNumber >= 12 && pData.MemberNumber <= 17 && pData.Name == _ELITEAPIMonitored.Player.Name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int GeneratePT_structure()
        {
            // FIRST CHECK THAT BOTH THE PL AND MONITORED PLAYER ARE IN THE SAME PT/ALLIANCE
            var party = _ELITEAPIMonitored.Party.GetPartyMembers();

            int partyChecker = 0;

            foreach (var member in party)
            {
                if (member.Name == _ELITEAPIPL.Player.Name)
                {
                    partyChecker++;
                }
                if (member.Name == _ELITEAPIMonitored.Player.Name)
                {
                    partyChecker++;
                }
            }

            if (partyChecker < 2)
            {// PL not in party
                return 4;
            }

            int plParty = party
                .Where(p => p.Name == _ELITEAPIPL.Player.Name)
                .Select(p => p.MemberNumber)
                .FirstOrDefault();

            if (plParty <= 5)
            {// Same Party
                return 1;
            }

            if (plParty <= 11)
            {// Alli Party 1
                return 2;
            }

            if (plParty <= 17)
            {// Alli Party 2
                return 3;
            }

            // PL not in party
            return 0;
        }

        private void resetSongTimer_Tick(object sender, EventArgs e)
        {
            song_casting = 0;
        }

        private void checkSCHCharges_Tick(object sender, EventArgs e)
        {
            if (_ELITEAPIPL != null && _ELITEAPIMonitored != null)
            {
                int MainJob = _ELITEAPIPL.Player.MainJob;
                int SubJob = _ELITEAPIPL.Player.SubJob;

                if (MainJob == 20 || SubJob == 20)
                {
                    if (plStatusCheck(StatusEffect.Light_Arts) || plStatusCheck(StatusEffect.Addendum_White))
                    {
                        int currentRecastTimer = GetAbilityRecastBySpellId(231);

                        int SpentPoints = _ELITEAPIPL.Player.GetJobPoints(20).SpentJobPoints;

                        int MainLevel = _ELITEAPIPL.Player.MainJobLevel;
                        int SubLevel = _ELITEAPIPL.Player.SubJobLevel;

                        int baseTimer = 240;
                        int baseCharges = 1;

                        // Generate the correct timer between charges depending on level / Job Points
                        if (MainLevel == 99 && SpentPoints > 550 && MainJob == 20)
                        {
                            baseTimer = 33;
                            baseCharges = 5;
                        }
                        else if (MainLevel >= 90 && SpentPoints < 550 && MainJob == 20)
                        {
                            baseTimer = 48;
                            baseCharges = 5;
                        }
                        else if (MainLevel >= 70 && MainLevel < 90 && MainJob == 20)
                        {
                            baseTimer = 60;
                            baseCharges = 4;
                        }
                        else if (MainLevel >= 50 && MainLevel < 70 && MainJob == 20)
                        {
                            baseTimer = 80;
                            baseCharges = 3;
                        }
                        else if ((MainLevel >= 30 && MainLevel < 50 && MainJob == 20) || (SubLevel >= 30 && SubLevel < 50 && SubJob == 20))
                        {
                            baseTimer = 120;
                            baseCharges = 2;
                        }
                        else if ((MainLevel >= 10 && MainLevel < 30 && MainJob == 20) || (SubLevel >= 10 && SubLevel < 30 && SubJob == 20))
                        {
                            baseTimer = 240;
                            baseCharges = 1;
                        }

                        // Now knowing what the time between charges is lets calculate how many
                        // charges are available

                        if (currentRecastTimer == 0)
                        {
                            currentSCHCharges = baseCharges;
                        }
                        else
                        {
                            int t = currentRecastTimer / 60;

                            int stratsUsed = t / baseTimer;

                            currentSCHCharges = (int)Math.Ceiling((decimal)baseCharges - stratsUsed);

                            if (baseTimer == 120)
                            {
                                currentSCHCharges -= 1;
                            }
                        }
                    }
                }
            }
        }

        private bool CheckEngagedStatus()
        {
            if (_ELITEAPIMonitored == null || _ELITEAPIPL == null) { return false; }


            if (OptionsForm.config.GeoWhenEngaged == false)
            {
                return true;
            }
            else if (OptionsForm.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(OptionsForm.config.LuopanSpell_Target))
            {
                for (int x = 0; x < 2048; x++)
                {
                    EliteAPI.XiEntity z = _ELITEAPIPL.Entity.GetEntity(x);
                    if (z.Name != string.Empty && z.Name != null)
                    {
                        if (z.Name.ToLower() == OptionsForm.config.LuopanSpell_Target.ToLower()) // A match was located so use this entity as a check.
                        {
                            if (z.Status == 1)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                return false;
            }
            else
            {
                if (_ELITEAPIMonitored.Player.Status == 1)
                {
                    return true;


                }
                else
                {
                    return false;
                }
            }
        }

        private void EclipticTimer_Tick(object sender, EventArgs e)
        {
            if (_ELITEAPIMonitored == null || _ELITEAPIPL == null) { return; }

            if (_ELITEAPIPL.Player.Pet.HealthPercent >= 1)
            {
                EclipticStillUp = true;
            }
            else
            {
                EclipticStillUp = false;
            }
        }

        private bool GEO_EnemyCheck()
        {
            if (_ELITEAPIMonitored == null || _ELITEAPIPL == null) { return false; }

            // Grab GEO spell name
            string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.GeoSpell_Spell, 2);

            if (SpellCheckedResult == "SpellError_Cancel" || SpellCheckedResult == "SpellRecast" || SpellCheckedResult == "SpellUnknown")
            {
                // Do nothing and continue on with the program
                return true;
            }
            else
            {
                if (_ELITEAPIPL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 5)
                {
                    return true; // SPELL TARGET IS PLAYER THEREFORE ONLY THE DEFAULT CHECK IS REQUIRED SO JUST RETURN TRUE TO VOID THIS CHECK
                }
                else
                {
                    if (OptionsForm.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(OptionsForm.config.LuopanSpell_Target))
                    {
                        for (int x = 0; x < 2048; x++)
                        {
                            EliteAPI.XiEntity z = _ELITEAPIPL.Entity.GetEntity(x);
                            if (z.Name != string.Empty && z.Name != null)
                            {
                                if (z.Name.ToLower() == OptionsForm.config.LuopanSpell_Target.ToLower()) // A match was located so use this entity as a check.
                                {
                                    if (z.Status == 1)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        return false;
                    }
                    else
                    {
                        if (_ELITEAPIMonitored.Player.Status == 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        private int CheckEngagedStatus_Hate()
        {
            if (OptionsForm.config.AssistSpecifiedTarget == true && OptionsForm.config.autoTarget_Target != String.Empty)
            {
                IDFound = 0;

                for (int x = 0; x < 2048; x++)
                {
                    EliteAPI.XiEntity z = _ELITEAPIPL.Entity.GetEntity(x);

                    if (z.Name != null && z.Name.ToLower() == OptionsForm.config.autoTarget_Target.ToLower())
                    {
                        if (z.Status == 1)
                        {
                            return z.TargetingIndex;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
                return 0;
            }
            else
            {
                if (_ELITEAPIMonitored.Player.Status == 1)
                {
                    EliteAPI.TargetInfo target = _ELITEAPIMonitored.Target.GetTargetInfo();
                    EliteAPI.XiEntity entity = _ELITEAPIMonitored.Entity.GetEntity(Convert.ToInt32(target.TargetIndex));
                    return Convert.ToInt32(entity.TargetID);

                }
                else
                {
                    return 0;
                }
            }
        }

        private int GrabGEOTargetID()
        {
            if (OptionsForm.config.specifiedEngageTarget == true && OptionsForm.config.LuopanSpell_Target != String.Empty)
            {
                IDFound = 0;

                for (int x = 0; x < 2048; x++)
                {
                    EliteAPI.XiEntity z = _ELITEAPIPL.Entity.GetEntity(x);

                    if (z.Name != null && z.Name.ToLower() == OptionsForm.config.LuopanSpell_Target.ToLower())
                    {
                        if (z.Status == 1)
                        {
                            return z.TargetingIndex;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
                return 0;
            }
            else
            {
                if (_ELITEAPIMonitored.Player.Status == 1)
                {
                    EliteAPI.TargetInfo target = _ELITEAPIMonitored.Target.GetTargetInfo();
                    EliteAPI.XiEntity entity = _ELITEAPIMonitored.Entity.GetEntity(Convert.ToInt32(target.TargetIndex));
                    return Convert.ToInt32(entity.TargetID);

                }
                else
                {
                    return 0;
                }
            }
        }

        private int GrabDistance_GEO()
        {
            string checkedName = string.Empty;
            string name1 = string.Empty;

            if (OptionsForm.config.specifiedEngageTarget == true && !string.IsNullOrEmpty(OptionsForm.config.LuopanSpell_Target))
            {
                checkedName = OptionsForm.config.LuopanSpell_Target;
            }
            else
            {
                checkedName = _ELITEAPIMonitored.Player.Name;
            }

            for (int x = 0; x < 2048; x++)
            {
                EliteAPI.XiEntity entityGEO = _ELITEAPIPL.Entity.GetEntity(x);

                if (!string.IsNullOrEmpty(checkedName) && !string.IsNullOrEmpty(entityGEO.Name))
                {
                    name1 = entityGEO.Name;

                    if (name1 == checkedName)
                    {
                        return (int)entityGEO.Distance;
                    }
                }
            }

            return 0;
        }

        private void updateInstances_Tick(object sender, EventArgs e)
        {
            if ((_ELITEAPIPL != null && _ELITEAPIPL.Player.LoginStatus == (int)LoginStatus.Loading) || (_ELITEAPIMonitored != null && _ELITEAPIMonitored.Player.LoginStatus == (int)LoginStatus.Loading))
            {
                return;
            }

            IEnumerable<Process> pol = Process.GetProcessesByName("pol").Union(Process.GetProcessesByName("xiloader")).Union(Process.GetProcessesByName("edenxi"));

            if (pol.Count() < 1)
            {
            }
            else
            {
                POLID.Items.Clear();
                POLID2.Items.Clear();
                processids.Items.Clear();

                int selectedPOLID = 0;
                int selectedPOLID2 = 0;

                for (int i = 0; i < pol.Count(); i++)
                {
                    POLID.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    POLID2.Items.Add(pol.ElementAt(i).MainWindowTitle);
                    processids.Items.Add(pol.ElementAt(i).Id);

                    if (_ELITEAPIPL != null && _ELITEAPIPL.Player.Name != null)
                    {
                        if (pol.ElementAt(i).MainWindowTitle.ToLower() == _ELITEAPIPL.Player.Name.ToLower())
                        {
                            selectedPOLID = i;
                            plLabel.Text = "Selected PL: " + _ELITEAPIPL.Player.Name;
                            Text = notifyIcon1.Text = _ELITEAPIPL.Player.Name + " - " + "Cure Please v" + Application.ProductVersion;
                        }
                    }

                    if (_ELITEAPIMonitored != null && _ELITEAPIMonitored.Player.Name != null)
                    {
                        if (pol.ElementAt(i).MainWindowTitle == _ELITEAPIMonitored.Player.Name)
                        {
                            selectedPOLID2 = i;
                            monitoredLabel.Text = "Monitored Player: " + _ELITEAPIMonitored.Player.Name;
                        }
                    }
                }
                POLID.SelectedIndex = selectedPOLID;
                POLID2.SelectedIndex = selectedPOLID2;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                Hide();
            }
            else if (FormWindowState.Normal == WindowState)
            {
            }
        }

        private void CheckCustomActions_TickAsync(object sender, EventArgs e)
        {
            if (_ELITEAPIPL != null && _ELITEAPIMonitored != null)
            {

                int cmdTime = _ELITEAPIMonitored.ThirdParty.ConsoleIsNewCommand();

                if (lastCommand != cmdTime)
                {
                    lastCommand = cmdTime;

                    if (_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(0) == "cureplease")
                    {
                        int argCount = _ELITEAPIMonitored.ThirdParty.ConsoleGetArgCount();

                        // 0 = cureplease or cp so ignore
                        // 1 = command to run
                        // 2 = (if set) PL's name

                        if (argCount >= 3)
                        {
                            if ((_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "stop" || _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "pause") && _ELITEAPIPL.Player.Name == _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(2))
                            {
                                Pause();
                            }
                            else if ((_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "unpause" || _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "start") && _ELITEAPIPL.Player.Name.ToLower() == _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(2).ToLower())
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                                song_casting = 0;
                                ForceSongRecast = true;
                            }
                            else if ((_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "toggle") && _ELITEAPIPL.Player.Name.ToLower() == _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(2).ToLower())
                            {
                                pauseButton.PerformClick();
                            }
                            else
                            {

                            }
                        }
                        else if (argCount < 3)
                        {
                            if (_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "stop" || _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "pause")
                            {
                                Pause();
                            }
                            else if (_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "unpause" || _ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "start")
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                                song_casting = 0;
                                ForceSongRecast = true;
                            }
                            else if (_ELITEAPIMonitored.ThirdParty.ConsoleGetArg(1) == "toggle")
                            {
                                pauseButton.PerformClick();
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            // DO NOTHING
                        }
                    }
                }
            }
        }

        public void Run_BardSongs()
        {







            PL_BRDCount = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == 195 || b == 196 || b == 197 || b == 198 || b == 199 || b == 200 || b == 201 || b == 214 || b == 215 || b == 216 || b == 218 || b == 219 || b == 222).Count();



            if ((OptionsForm.config.enableSinging) && _ELITEAPIPL.Player.Status != 33)
            {

                debug_MSG_show = "ORDER: " + song_casting;

                SongData song_1 = SongInfo.Where(c => c.song_position == OptionsForm.config.song1).FirstOrDefault();
                SongData song_2 = SongInfo.Where(c => c.song_position == OptionsForm.config.song2).FirstOrDefault();
                SongData song_3 = SongInfo.Where(c => c.song_position == OptionsForm.config.song3).FirstOrDefault();
                SongData song_4 = SongInfo.Where(c => c.song_position == OptionsForm.config.song4).FirstOrDefault();

                SongData dummy1_song = SongInfo.Where(c => c.song_position == OptionsForm.config.dummy1).FirstOrDefault();
                SongData dummy2_song = SongInfo.Where(c => c.song_position == OptionsForm.config.dummy2).FirstOrDefault();

                // Check the distance of the Monitored player
                int Monitoreddistance = 50;


                EliteAPI.XiEntity monitoredTarget = _ELITEAPIPL.Entity.GetEntity((int)_ELITEAPIMonitored.Player.TargetID);
                Monitoreddistance = (int)monitoredTarget.Distance;

                int Songs_Possible = 0;

                if (song_1.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }
                if (song_2.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }
                if (dummy1_song != null && dummy1_song.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }
                if (dummy2_song != null && dummy2_song.song_name.ToLower() != "blank")
                {
                    Songs_Possible++;
                }

                // List to make it easy to check how many of each buff is needed.
                List<int> SongDataMax = new List<int> { song_1.buff_id, song_2.buff_id, song_3.buff_id, song_4.buff_id };

                // Check Whether e have the songs Currently Up
                int count1_type = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.buff_id).Count();
                int count2_type = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.buff_id).Count();
                int count3_type = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.buff_id).Count();
                int count4_type = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.buff_id).Count();
                int count5_type = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.buff_id).Count();
                int count6_type = _ELITEAPIPL.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.buff_id).Count();

                int MON_count1_type = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_1.buff_id).Count();
                int MON_count2_type = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_2.buff_id).Count();
                int MON_count3_type = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy1_song.buff_id).Count();
                int MON_count4_type = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_3.buff_id).Count();
                int MON_count5_type = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == dummy2_song.buff_id).Count();
                int MON_count6_type = _ELITEAPIMonitored.Player.GetPlayerInfo().Buffs.Where(b => b == song_4.buff_id).Count();


                if (ForceSongRecast == true) { song_casting = 0; ForceSongRecast = false; }


                // SONG NUMBER #4
                if (song_casting == 3 && PL_BRDCount >= 3 && song_4.song_name.ToLower() != "blank" && count6_type < SongDataMax.Where(c => c == song_4.buff_id).Count() && Last_Song_Cast != song_4.song_name)
                {
                    if (PL_BRDCount == 3)
                    {
                        if (CheckSpellRecast(dummy2_song.song_name) == 0 && (CanCastSpell(dummy2_song.song_name)))
                        {
                            CastSpell("<me>", dummy2_song.song_name);
                        }
                    }
                    else
                    {
                        if (CheckSpellRecast(song_4.song_name) == 0 && (CanCastSpell(song_4.song_name)))
                        {
                            CastSpell("<me>", song_4.song_name);
                            Last_Song_Cast = song_4.song_name;
                            Last_SongCast_Timer[0] = DateTime.Now;
                            playerSong4[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }

                }
                else if (song_casting == 3 && song_4.song_name.ToLower() != "blank" && count6_type >= SongDataMax.Where(c => c == song_4.buff_id).Count())
                {
                    song_casting = 0;
                }


                // SONG NUMBER #3
                else if (song_casting == 2 && PL_BRDCount >= 2 && song_3.song_name.ToLower() != "blank" && count4_type < SongDataMax.Where(c => c == song_3.buff_id).Count() && Last_Song_Cast != song_3.song_name)
                {
                    if (PL_BRDCount == 2)
                    {
                        if (CheckSpellRecast(dummy1_song.song_name) == 0 && (CanCastSpell(dummy1_song.song_name)))
                        {
                            CastSpell("<me>", dummy1_song.song_name);
                        }
                    }
                    else
                    {
                        if (CheckSpellRecast(song_3.song_name) == 0 && (CanCastSpell(song_3.song_name)))
                        {
                            CastSpell("<me>", song_3.song_name);
                            Last_Song_Cast = song_3.song_name;
                            Last_SongCast_Timer[0] = DateTime.Now;
                            playerSong3[0] = DateTime.Now;
                            song_casting = 3;
                        }
                    }
                }
                else if (song_casting == 2 && song_3.song_name.ToLower() != "blank" && count4_type >= SongDataMax.Where(c => c == song_3.buff_id).Count())
                {
                    song_casting = 3;
                }


                // SONG NUMBER #2
                else if (song_casting == 1 && song_2.song_name.ToLower() != "blank" && count2_type < SongDataMax.Where(c => c == song_2.buff_id).Count() && Last_Song_Cast != song_4.song_name)
                {
                    if (CheckSpellRecast(song_2.song_name) == 0 && (CanCastSpell(song_2.song_name)))
                    {
                        CastSpell("<me>", song_2.song_name);
                        Last_Song_Cast = song_2.song_name;
                        Last_SongCast_Timer[0] = DateTime.Now;
                        playerSong2[0] = DateTime.Now;
                        song_casting = 2;
                    }
                }
                else if (song_casting == 1 && song_2.song_name.ToLower() != "blank" && count2_type >= SongDataMax.Where(c => c == song_2.buff_id).Count())
                {
                    song_casting = 2;
                }

                // SONG NUMBER #1
                else if ((song_casting == 0) && song_1.song_name.ToLower() != "blank" && count1_type < SongDataMax.Where(c => c == song_1.buff_id).Count() && Last_Song_Cast != song_4.song_name)
                {
                    if (CheckSpellRecast(song_1.song_name) == 0 && (CanCastSpell(song_1.song_name)))
                    {
                        CastSpell("<me>", song_1.song_name);
                        Last_Song_Cast = song_1.song_name;
                        Last_SongCast_Timer[0] = DateTime.Now;
                        playerSong1[0] = DateTime.Now;
                        song_casting = 1;
                    }

                }
                else if (song_casting == 0 && song_2.song_name.ToLower() != "blank" && count1_type >= SongDataMax.Where(c => c == song_1.buff_id).Count())
                {
                    song_casting = 1;
                }


                // ONCE ALL SONGS HAVE BEEN CAST ONLY RECAST THEM WHEN THEY MEET THE THRESHOLD SET ON SONG RECAST AND BLOCK IF IT'S SET AT LAUNCH DEFAULTS
                if (playerSong1[0] != DefaultTime && playerSong1_Span[0].Minutes >= OptionsForm.config.recastSongTime)
                {
                    if ((OptionsForm.config.SongsOnlyWhenNear && Monitoreddistance < 10) || OptionsForm.config.SongsOnlyWhenNear == false)
                    {
                        if (CheckSpellRecast(song_1.song_name) == 0 && (CanCastSpell(song_1.song_name)))
                        {
                            CastSpell("<me>", song_1.song_name);
                            playerSong1[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }
                else if (playerSong2[0] != DefaultTime && playerSong2_Span[0].Minutes >= OptionsForm.config.recastSongTime)
                {
                    if ((OptionsForm.config.SongsOnlyWhenNear && Monitoreddistance < 10) || OptionsForm.config.SongsOnlyWhenNear == false)
                    {
                        if (CheckSpellRecast(song_2.song_name) == 0 && (CanCastSpell(song_2.song_name)))
                        {
                            CastSpell("<me>", song_2.song_name);
                            playerSong2[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }
                else if (playerSong3[0] != DefaultTime && playerSong3_Span[0].Minutes >= OptionsForm.config.recastSongTime)
                {
                    if ((OptionsForm.config.SongsOnlyWhenNear && Monitoreddistance < 10) || OptionsForm.config.SongsOnlyWhenNear == false)
                    {
                        if (CheckSpellRecast(song_3.song_name) == 0 && (CanCastSpell(song_3.song_name)))
                        {
                            CastSpell("<me>", song_3.song_name);
                            playerSong3[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }
                else if (playerSong4[0] != DefaultTime && playerSong4_Span[0].Minutes >= OptionsForm.config.recastSongTime)
                {
                    if ((OptionsForm.config.SongsOnlyWhenNear && Monitoreddistance < 10) || OptionsForm.config.SongsOnlyWhenNear == false)
                    {
                        if (CheckSpellRecast(song_4.song_name) == 0 && (CanCastSpell(song_4.song_name)))
                        {
                            CastSpell("<me>", song_4.song_name);
                            playerSong4[0] = DateTime.Now;
                            song_casting = 0;
                        }
                    }
                }


            }
        }




        private void Follow_BGW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            // MAKE SURE BOTH ELITEAPI INSTANCES ARE ACTIVE, THE BOT ISN'T PAUSED, AND THERE IS AN AUTOFOLLOWTARGET NAMED
            if (_ELITEAPIPL != null && _ELITEAPIMonitored != null && !string.IsNullOrEmpty(OptionsForm.config.autoFollowName) && !pauseActions)
            {

                if (OptionsForm.config.FFXIDefaultAutoFollow != true)
                {
                    // CANCEL ALL PREVIOUS FOLLOW ACTIONS
                    _ELITEAPIPL.AutoFollow.IsAutoFollowing = false;
                    curePlease_autofollow = false;
                    stuckWarning = false;
                    stuckCount = 0;
                }

                // RUN THE FUNCTION TO GRAB THE ID OF THE FOLLOW TARGET THIS ALSO MAKES SURE THEY ARE IN RANGE TO FOLLOW
                int followersTargetID = followID();

                // If the FOLLOWER'S ID is NOT -1 THEN THEY WERE LOCATED SO CONTINUE THE CHECKS
                if (followersTargetID != -1)
                {
                    // GRAB THE FOLLOW TARGETS ENTITY TABLE TO CHECK DISTANCE ETC
                    EliteAPI.XiEntity followTarget = _ELITEAPIPL.Entity.GetEntity(followersTargetID);

                    if (Math.Truncate(followTarget.Distance) >= (int)OptionsForm.config.autoFollowDistance && curePlease_autofollow == false)
                    {
                        // THE DISTANCE IS GREATER THAN REQUIRED SO IF AUTOFOLLOW IS NOT ACTIVE THEN DEPENDING ON THE TYPE, FOLLOW

                        // SQUARE ENIX FINAL FANTASY XI DEFAULT AUTO FOLLOW
                        if (OptionsForm.config.FFXIDefaultAutoFollow == true && _ELITEAPIPL.AutoFollow.IsAutoFollowing != true)
                        {
                            // IF THE CURRENT TARGET IS NOT THE FOLLOWERS TARGET ID THEN CHANGE THAT NOW
                            if (_ELITEAPIPL.Target.GetTargetInfo().TargetIndex != followersTargetID)
                            {
                                // FIRST REMOVE THE CURRENT TARGET
                                _ELITEAPIPL.Target.SetTarget(0);
                                // NOW SET THE NEXT TARGET AFTER A WAIT
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                _ELITEAPIPL.Target.SetTarget(followersTargetID);
                            }
                            // IF THE TARGET IS CORRECT BUT YOU'RE NOT LOCKED ON THEN DO SO NOW
                            else if (_ELITEAPIPL.Target.GetTargetInfo().TargetIndex == followersTargetID && !_ELITEAPIPL.Target.GetTargetInfo().LockedOn)
                            {
                                _ELITEAPIPL.ThirdParty.SendString("/lockon <t>");
                            }
                            // EVERYTHING SHOULD BE FINE SO FOLLOW THEM
                            else
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                _ELITEAPIPL.ThirdParty.SendString("/follow");
                            }
                        }
                        // ELITEAPI'S IMPROVED AUTO FOLLOW
                        else if (OptionsForm.config.FFXIDefaultAutoFollow != true && _ELITEAPIPL.AutoFollow.IsAutoFollowing != true)
                        {
                            // IF YOU ARE TOO FAR TO FOLLOW THEN STOP AND IF ENABLED WARN THE MONITORED PLAYER
                            if (OptionsForm.config.autoFollow_Warning == true && Math.Truncate(followTarget.Distance) >= 40 && _ELITEAPIMonitored.Player.Name != _ELITEAPIPL.Player.Name && followWarning == 0)
                            {
                                string createdTell = "/tell " + _ELITEAPIMonitored.Player.Name + " " + "You're too far to follow.";
                                _ELITEAPIPL.ThirdParty.SendString(createdTell);
                                followWarning = 1;
                                Thread.Sleep(TimeSpan.FromSeconds(0.3));
                            }
                            else if (Math.Truncate(followTarget.Distance) <= 40)
                            {
                                // ONLY TARGET AND BEGIN FOLLOW IF TARGET IS AT THE DEFINED DISTANCE
                                if (Math.Truncate(followTarget.Distance) >= (int)OptionsForm.config.autoFollowDistance && Math.Truncate(followTarget.Distance) <= 48)
                                {
                                    followWarning = 0;

                                    // Cancel current target this is to make sure the character is not locked
                                    // on and therefore unable to move freely. Wait 5ms just to allow it to work

                                    _ELITEAPIPL.Target.SetTarget(0);
                                    Thread.Sleep(TimeSpan.FromSeconds(0.1));

                                    float Target_X;
                                    float Target_Y;
                                    float Target_Z;

                                    EliteAPI.XiEntity FollowerTargetEntity = _ELITEAPIPL.Entity.GetEntity(followersTargetID);

                                    if (!string.IsNullOrEmpty(FollowerTargetEntity.Name))
                                    {
                                        while (Math.Truncate(followTarget.Distance) >= (int)OptionsForm.config.autoFollowDistance)
                                        {

                                            float Player_X = _ELITEAPIPL.Player.X;
                                            float Player_Y = _ELITEAPIPL.Player.Y;
                                            float Player_Z = _ELITEAPIPL.Player.Z;


                                            if (FollowerTargetEntity.Name == _ELITEAPIMonitored.Player.Name)
                                            {
                                                Target_X = _ELITEAPIMonitored.Player.X;
                                                Target_Y = _ELITEAPIMonitored.Player.Y;
                                                Target_Z = _ELITEAPIMonitored.Player.Z;
                                                float dX = Target_X - Player_X;
                                                float dY = Target_Y - Player_Y;
                                                float dZ = Target_Z - Player_Z;

                                                _ELITEAPIPL.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

                                                _ELITEAPIPL.AutoFollow.IsAutoFollowing = true;
                                                curePlease_autofollow = true;


                                                lastX = _ELITEAPIPL.Player.X;
                                                lastY = _ELITEAPIPL.Player.Y;
                                                lastZ = _ELITEAPIPL.Player.Z;

                                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                            }
                                            else
                                            {
                                                Target_X = FollowerTargetEntity.X;
                                                Target_Y = FollowerTargetEntity.Y;
                                                Target_Z = FollowerTargetEntity.Z;

                                                float dX = Target_X - Player_X;
                                                float dY = Target_Y - Player_Y;
                                                float dZ = Target_Z - Player_Z;


                                                _ELITEAPIPL.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

                                                _ELITEAPIPL.AutoFollow.IsAutoFollowing = true;
                                                curePlease_autofollow = true;


                                                lastX = _ELITEAPIPL.Player.X;
                                                lastY = _ELITEAPIPL.Player.Y;
                                                lastZ = _ELITEAPIPL.Player.Z;

                                                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                                            }

                                            // STUCK CHECKER
                                            float genX = lastX - _ELITEAPIPL.Player.X;
                                            float genY = lastY - _ELITEAPIPL.Player.Y;
                                            float genZ = lastZ - _ELITEAPIPL.Player.Z;

                                            double distance = Math.Sqrt(genX * genX + genY * genY + genZ * genZ);

                                            if (distance < .1)
                                            {
                                                stuckCount = stuckCount + 1;
                                                if (OptionsForm.config.autoFollow_Warning == true && stuckWarning != true && FollowerTargetEntity.Name == _ELITEAPIMonitored.Player.Name && stuckCount == 10)
                                                {
                                                    string createdTell = "/tell " + _ELITEAPIMonitored.Player.Name + " " + "I appear to be stuck.";
                                                    _ELITEAPIPL.ThirdParty.SendString(createdTell);
                                                    stuckWarning = true;
                                                }
                                            }
                                        }

                                        _ELITEAPIPL.AutoFollow.IsAutoFollowing = false;
                                        curePlease_autofollow = false;
                                        stuckWarning = false;
                                        stuckCount = 0;
                                    }
                                }
                            }
                            else
                            {
                                // YOU ARE NOT AT NOR FURTHER THAN THE DISTANCE REQUIRED SO CANCEL ELITEAPI AUTOFOLLOW
                                curePlease_autofollow = false;
                            }
                        }
                    }
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));

        }

        private void Follow_BGW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Follow_BGW.RunWorkerAsync();
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Opacity = trackBar1.Value * 0.01;
        }

        private Form settings;

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            if ((settings == null) || (settings.IsDisposed))
            {
                settings = new OptionsForm();
            }
            settings.Show();

        }

        private void ChatLogButton_Click(object sender, EventArgs e)
        {
            ChatLogForm form4 = new ChatLogForm(this);

            if (_ELITEAPIPL != null)
            {
                form4.Show();
            }
        }

        private void PartyBuffsButton_Click(object sender, EventArgs e)
        {
            PartyBuffsForm PartyBuffs = new PartyBuffsForm(this);
            if (_ELITEAPIPL != null)
            {
                PartyBuffs.Show();
            }
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            new AboutForm().Show();
        }

        private void AddonReader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!OptionsForm.config.EnableAddOn)
            {
                return;
            }

            if (_ELITEAPIMonitored == null || _ELITEAPIPL == null)
            {
                return;
            }

            bool done = false;

            UdpClient listener = new UdpClient(Convert.ToInt32(OptionsForm.config.listeningPort));
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(OptionsForm.config.ipAddress), Convert.ToInt32(OptionsForm.config.listeningPort));
            string received_data;
            byte[] receive_byte_array;
            try
            {
                while (!done)
                {

                    receive_byte_array = listener.Receive(ref groupEP);

                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);



                    string[] commands = received_data.Split('_');

                    // MessageBox.Show(commands[1] + " " + commands[2]);
                    if (commands[1] == "casting" && commands.Count() == 3 && OptionsForm.config.trackCastingPackets == true)
                    {
                        if (commands[2] == "blocked")
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                CastingBackground_Check = true;
                                castingLockLabel.Text = "PACKET: Casting is LOCKED";
                            }));

                            if (!ProtectCasting.IsBusy) { ProtectCasting.RunWorkerAsync(); }
                        }
                        else if (commands[2] == "interrupted")
                        {
                            Invoke((MethodInvoker)(async () =>
                            {
                                ProtectCasting.CancelAsync();
                                castingLockLabel.Text = "PACKET: Casting is INTERRUPTED";
                                await Task.Delay(TimeSpan.FromSeconds(3));
                                castingLockLabel.Text = "Casting is UNLOCKED";
                                CastingBackground_Check = false;
                            }));
                        }
                        else if (commands[2] == "finished")
                        {
                            Invoke((MethodInvoker)(async () =>
                            {
                                ProtectCasting.CancelAsync();
                                castingLockLabel.Text = "PACKET: Casting is soon to be AVAILABLE!";
                                await Task.Delay(TimeSpan.FromSeconds(3));
                                castingLockLabel.Text = "Casting is UNLOCKED";
                                currentAction.Text = string.Empty;
                                castingSpell = string.Empty;
                                CastingBackground_Check = false;
                            }));
                        }
                    }
                    else if (commands[1] == "confirmed")
                    {
                        AddOnStatus.BackColor = Color.ForestGreen;
                    }
                    else if (commands[1] == "command")
                    {
                        // MessageBox.Show(commands[2]);
                        if (commands[2] == "start" || commands[2] == "unpause")
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                pauseButton.Text = "Pause";
                                pauseButton.ForeColor = Color.Black;
                                actionTimer.Enabled = true;
                                pauseActions = false;
                                song_casting = 0;
                                ForceSongRecast = true;
                            }));
                        }
                        if (commands[2] == "stop" || commands[2] == "pause")
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                Pause();
                            }));
                        }
                        if (commands[2] == "toggle")
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                pauseButton.PerformClick();
                            }));
                        }
                    }
                    else if (commands[1] == "buffs" && commands.Count() == 4)
                    {
                        lock (ActiveBuffs)
                        {
                            ActiveBuffs.RemoveAll(buf => buf.CharacterName == commands[2]);

                            ActiveBuffs.Add(new BuffStorage
                            {
                                CharacterName = commands[2],
                                CharacterBuffs = commands[3]
                            });
                        }

                    }
                }
            }
            catch (Exception)
            {
                //  Console.WriteLine(error1.ToString());
            }

            listener.Close();

            Thread.Sleep(TimeSpan.FromSeconds(0.3));
        }

        private void AddonReader_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            AddonReader.RunWorkerAsync();
        }


        private void FullCircle_Timer_Tick(object sender, EventArgs e)
        {

            if (_ELITEAPIPL.Player.Pet.HealthPercent >= 1)
            {
                ushort PetsIndex = _ELITEAPIPL.Player.PetIndex;

                if (OptionsForm.config.Fullcircle_GEOTarget == true && OptionsForm.config.LuopanSpell_Target != "")
                {
                    EliteAPI.XiEntity PetsEntity = _ELITEAPIPL.Entity.GetEntity(PetsIndex);

                    int FullCircle_CharID = 0;

                    for (int x = 0; x < 2048; x++)
                    {
                        EliteAPI.XiEntity entity = _ELITEAPIPL.Entity.GetEntity(x);

                        if (entity.Name != null && entity.Name.ToLower().Equals(OptionsForm.config.LuopanSpell_Target.ToLower()))
                        {
                            FullCircle_CharID = Convert.ToInt32(entity.TargetID);
                            break;
                        }
                    }

                    if (FullCircle_CharID != 0)
                    {
                        EliteAPI.XiEntity FullCircleEntity = _ELITEAPIPL.Entity.GetEntity(FullCircle_CharID);

                        float fX = PetsEntity.X - FullCircleEntity.X;
                        float fY = PetsEntity.Y - FullCircleEntity.Y;
                        float fZ = PetsEntity.Z - FullCircleEntity.Z;

                        float generatedDistance = (float)Math.Sqrt((fX * fX) + (fY * fY) + (fZ * fZ));

                        if (generatedDistance >= 10)
                        {
                            _ELITEAPIPL.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }

                }
                else if (OptionsForm.config.Fullcircle_GEOTarget == false && _ELITEAPIMonitored.Player.Status == 1)
                {


                    string SpellCheckedResult = ReturnGeoSpell(OptionsForm.config.GeoSpell_Spell, 2);



                    if (OptionsForm.config.Fullcircle_DisableEnemy != true || (OptionsForm.config.Fullcircle_DisableEnemy == true && _ELITEAPIPL.Resources.GetSpell(SpellCheckedResult, 0).ValidTargets == 32))
                    {
                        EliteAPI.XiEntity PetsEntity = _ELITEAPIMonitored.Entity.GetEntity(PetsIndex);

                        if (PetsEntity.Distance >= 10 && PetsEntity.Distance != 0 && GetAbilityRecast("Full Circle") == 0)
                        {
                            _ELITEAPIPL.ThirdParty.SendString("/ja \"Full Circle\" <me>");
                        }
                    }
                }
            }

            FullCircle_Timer.Enabled = false;
        }

        private void AddOnStatus_Click(object sender, EventArgs e)
        {
            if (_ELITEAPIPL == null)
            {
                return;
            }

            AddonManager.Verify();
        }

        private void ProtectCasting_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1.0));
            int count = 0;
            float lastPercent = 0;
            float castPercent = _ELITEAPIPL.CastBar.Percent;
            while (castPercent < 1)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
                castPercent = _ELITEAPIPL.CastBar.Percent;
                if (lastPercent != castPercent)
                {
                    count = 0;
                    lastPercent = castPercent;
                }
                else if (count == 10)
                {

                    break;
                }
                else
                {
                    count++;
                    lastPercent = castPercent;
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(2.0));

            castingSpell = string.Empty;

            castingLockLabel.Invoke(new Action(() => { castingLockLabel.Text = "Casting is UNLOCKED"; }));
            castingSpell = string.Empty;

            CastingBackground_Check = false;
        }

        private void JobAbility_Delay_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                JobAbilityLock_Check = true;
                castingLockLabel.Text = "Casting is LOCKED for a JA.";
                currentAction.Text = "Using a Job Ability: " + JobAbilityCMD;
                Thread.Sleep(TimeSpan.FromSeconds(2));
                castingLockLabel.Text = "Casting is UNLOCKED";
                currentAction.Text = string.Empty;
                castingSpell = string.Empty;
                // JobAbilityLock_Check = false;
                JobAbilityCMD = String.Empty;
            }));
        }

        private void CustomCommand_Tracker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
        }

        private void CustomCommand_Tracker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            CustomCommand_Tracker.RunWorkerAsync();
        }
    }

    // END OF THE FORM SCRIPT

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
