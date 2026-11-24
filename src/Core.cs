using System;
using System.Collections.Generic;
using System.Globalization;
using Alta;
using Alta.Blacksmithing;
using Alta.Carpentry;
using Alta.Inventory;
using Alta.Pages;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomRecipesAPI.Core), "CustomRecipesAPI", CustomRecipesAPI.Port.Version, "CGNik, ported by Shadow", null)]
[assembly: MelonGame("Alta", "Att_Quest")]

namespace CustomRecipesAPI
{
    public class SpawnPatch
    {
    }

    public class Core : MelonMod
    {
        public static event Action PreSetUpRecipes = () => { };
        public static event Action SetUpRecipes = () => { };
        public static event Action PostSetUpRecipes = () => { };
        public static event Action PostPatches = () => { };

        public static List<Item> itemsToAddToStandardMouldPress = new List<Item>();
        public static List<Item> itemsToAddToHebiosMouldPress = new List<Item>();
        public static List<Item> itemsToAddToSmelter = new List<Item>();
        public static List<Item> itemsToAddToSmelterAutodock = new List<Item>();
        public static Dictionary<uint, Vector3> smelterSpawnPositionOffsets = new Dictionary<uint, Vector3>();
        public static Dictionary<uint, Vector3> smelterSpawnRotationOffsets = new Dictionary<uint, Vector3>();
        private static MouldDefinition mouldDefinition_axeHeadCurveMould;

        public static Dictionary<int, Item> VanillaOreAndIngotItems = new Dictionary<int, Item>();

        public enum VanillaOreAndIngotIndexers
        {
            CopperOre = 42614,
            IronOre = 42566,
            GoldOre = 5732,
            SilverOre = 5698,
            MythrilOre = 4758,
            CopperIngot = 5802,
            IronIngot = 7204,
            GoldIngot = 17090,
            SilverIngot = 57718,
            MythrilIngot = 60398,
            CarsiIngot = 24084,
            EvinonSteelIngot = 32224,
            OrchiIngot = 16422,
            RedIronIngot = 30996,
            WhiteGoldIngot = 24016
        }

        public enum PageCollectionHashes
        {
            BlacksmithBasicsCollection = 11654,
            ChiselingCollection = 716,
            EdibleObjectsCollection = 17390,
            GenericCraftingCollection = 57634,
            HuntingBasicsCollection = 29810,
            MiningBasicsCollection = 13678,
            StewIngredientCollection = 62188,
            StewingRecipesCollection = 27908,
            WoodcuttingBasicsCollection = 48982
        }

        public override void OnInitializeMelon()
        {
            Port.Init(LoggerInstance, RunSetup);
            LoggerInstance.Msg("Initialized.");
        }

        private static void InvokeEach(Action action, string eventName)
        {
            if (action == null)
                return;

            foreach (Action subscriber in action.GetInvocationList())
            {
                try
                {
                    subscriber();
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"{eventName} subscriber {subscriber.Method.DeclaringType?.FullName}.{subscriber.Method.Name} from {subscriber.Method.DeclaringType?.Assembly.GetName().Name} failed, continuing with the others");
                    MelonLogger.Error(ex);
                }
            }
        }

        public static void AddItemsToFilters()
        {
            PickupDockSettings mouldPressInputDockStandard = Plat.DockSettings("Mould Press Input Dock Standard");
            if (mouldPressInputDockStandard != null)
                Port.Filtered += Plat.AddUnique(mouldPressInputDockStandard.IncludedItems, itemsToAddToStandardMouldPress);

            PickupDockSettings mouldPressInputDockHebios = Plat.DockSettings("Mould Press Input Dock Hebios");
            if (mouldPressInputDockHebios != null)
                Port.Filtered += Plat.AddUnique(mouldPressInputDockHebios.IncludedItems, itemsToAddToHebiosMouldPress);

            PickupDockSettings oreDock = Plat.DockSettings("Ore Dock");
            if (oreDock != null)
                Port.Filtered += Plat.AddUnique(oreDock.IncludedItems, itemsToAddToSmelter);

            PickupTag smelterAutodockTag = Plat.Tag("tags/Smelter Autodock");
            if (smelterAutodockTag == null)
                return;
            foreach (Item item in itemsToAddToSmelterAutodock)
            {
                if (Plat.AddUnique(item.Tags, smelterAutodockTag))
                    Port.Filtered++;
            }
        }

        public override void OnLateInitializeMelon()
        {
            Port.Setup("late start");
        }

        private static void RunSetup()
        {
            float start = Time.realtimeSinceStartup;
            FillVanillaOreAndIngotItems();
            mouldDefinition_axeHeadCurveMould = Plat.Find<MouldDefinition>(22952u);
            if (mouldDefinition_axeHeadCurveMould == null)
                Port.Error("the game has no Axe Head Curve Mould (22952), so moulds set up here get no allowed materials");

            InvokeEach(PreSetUpRecipes, nameof(PreSetUpRecipes));

            InvokeEach(SetUpRecipes, nameof(SetUpRecipes));

            InvokeEach(PostSetUpRecipes, nameof(PostSetUpRecipes));

            Port.Run("item filters", AddItemsToFilters);

            InvokeEach(PostPatches, nameof(PostPatches));

            Port.Info(Port.Summary() + ", setup took " + (Time.realtimeSinceStartup - start).ToString("0.00", CultureInfo.InvariantCulture) + " s");
        }

        private static void FillVanillaOreAndIngotItems()
        {
            foreach (int hash in Port.OreAndIngotOrder)
            {
                Item item = Plat.Find<Item>((uint)hash);
                if (item != null)
                    VanillaOreAndIngotItems[hash] = item;
                else
                    Port.Error("the game has no item " + hash + ", so VanillaOreAndIngotItems leaves it out");
            }
        }

        public static void AddMouldItemComponent(Item item, MouldDefinition mouldDefinition)
        {
            MouldItemComponent mouldItemComponent = Plat.NewMouldItemComponent();
            mouldItemComponent.name = item.name + "Mould Item Component";
            Plat.SetDefinition(mouldItemComponent, mouldDefinition);
            Plat.Components(item).Add(mouldItemComponent);
        }

        public static void FixMouldDefinitionInspectorValues(Item item, MouldDefinition mouldDefinition)
        {
            Plat.SetAllowedMaterials(mouldDefinition, Plat.AllowedMaterials(mouldDefinition_axeHeadCurveMould));
            Plat.SetProduct(mouldDefinition, item);
        }

        public static void RegisterMouldDefinition(MouldDefinition mouldDefinition)
        {
            Plat.Register(mouldDefinition);
            Port.Moulds++;
        }

        public static void AddSmelterOffsets(Item item, Vector3? positionOffset, Vector3? rotationOffset)
        {
            if (positionOffset != null)
            {
                smelterSpawnPositionOffsets[item.Prefab.Hash] = (Vector3)positionOffset;
            }
            else
            {
                smelterSpawnPositionOffsets.Remove(item.Prefab.Hash);
            }

            if (rotationOffset != null)
            {
                smelterSpawnRotationOffsets[item.Prefab.Hash] = (Vector3)rotationOffset;
            }
            else
            {
                smelterSpawnRotationOffsets.Remove(item.Prefab.Hash);
            }
        }

        public static void SetUpMould(Item item, MouldDefinition mouldDefinition, bool addMouldItemComponent = false, bool addToStandardPress = false, bool addToHebiosPress = false, Vector3? positionOffset = null, Vector3? rotationOffset = null)
        {
            if (addMouldItemComponent)
            {
                AddMouldItemComponent(item, mouldDefinition);
            }

            FixMouldDefinitionInspectorValues(item, mouldDefinition);

            RegisterMouldDefinition(mouldDefinition);

            AddSmelterOffsets(item, positionOffset, rotationOffset);

            if (addToStandardPress)
            {
                itemsToAddToStandardMouldPress.Add(item);
            }

            if (addToHebiosPress)
            {
                itemsToAddToHebiosMouldPress.Add(item);
            }
        }

        public static void FixSmeltingRecipeInspectorValues(SmeltingRecipe smeltingRecipe, Item[] inputs, Item[] outputs)
        {
            Plat.FixRecipeItems(smeltingRecipe, inputs, outputs);
        }

        public static void RegisterSmeltingRecipe(SmeltingRecipe smeltingRecipe)
        {
            Plat.Register(smeltingRecipe);
            Port.SmeltingRecipes++;
        }

        public static void AddSmeltingRecipeToSmelterUpgrades(SmeltingRecipe smeltingRecipe, SmelterUpgrades smelterUpgrades = null, bool addToSimpleServerDefaultUpgrades = true)
        {
            if (addToSimpleServerDefaultUpgrades)
            {
                SmelterUpgrades smelterUpgrades_simpleServerDefaultUpgrades = Plat.Need<SmelterUpgrades>(5674u, "Simple Server Default Upgrades");
                Plat.AppendRecipe(smelterUpgrades_simpleServerDefaultUpgrades, smeltingRecipe);
            }

            if (smelterUpgrades != null)
            {
                Plat.AppendRecipe(smelterUpgrades, smeltingRecipe);
            }
        }

        public static void SetUpSmeltingRecipe(SmeltingRecipe smeltingRecipe, Item[] inputs, Item[] outputs, SmelterUpgrades smelterUpgrades = null, bool addToSimpleServerDefaultUpgrades = true)
        {
            FixSmeltingRecipeInspectorValues(smeltingRecipe, inputs, outputs);
            RegisterSmeltingRecipe(smeltingRecipe);
            AddSmeltingRecipeToSmelterUpgrades(smeltingRecipe, smelterUpgrades, addToSimpleServerDefaultUpgrades);
        }

        public static ItemCount CreateItemCount(Item item, int count)
        {
            ItemCount itemCount = Plat.NewItemCount();
            Plat.SetItemCount(itemCount, item, count);
            return itemCount;
        }

        public static SmeltingRecipe CreateSmeltingRecipe(string name, int hash, float duration, ItemCount[] inputs, ItemCount[] outputs)
        {
            SmeltingRecipe smeltingRecipe = Plat.Copy(Plat.Need<SmeltingRecipe>(31642u, "smelting recipe 31642"));
            smeltingRecipe.name = name;
            Plat.SetHash(smeltingRecipe, hash);
            Plat.SetRecipe(smeltingRecipe, duration, inputs, outputs);
            return smeltingRecipe;
        }

        public static SmeltingRecipe SetUpSmeltingRecipeThroughCode(string name, int hash, float duration, ItemCount[] inputs, ItemCount[] outputs, SmelterUpgrades smelterUpgrades = null, bool addToSimpleServerDefaultUpgrades = true)
        {
            SmeltingRecipe smeltingRecipe = CreateSmeltingRecipe(name, hash, duration, inputs, outputs);
            RegisterSmeltingRecipe(smeltingRecipe);
            AddSmeltingRecipeToSmelterUpgrades(smeltingRecipe, smelterUpgrades, addToSimpleServerDefaultUpgrades);
            return smeltingRecipe;
        }

        public static void FixChiselDefinitionInspectorValues(ChiselDefinition chiselDefinition, Glyph glyph)
        {
            chiselDefinition.recipeGlyph = glyph;
        }

        public static void RegisterChiselDefinition(ChiselDefinition chiselDefinition)
        {
            Plat.Register(chiselDefinition);
            Port.Chisels++;
        }

        public static void AddToPageCollection(PageCollection pageCollection, UnityEngine.Object obj)
        {
            Plat.AppendPage(pageCollection, obj);
            Port.Pages++;
        }

        public static PrefabSpawnPosition CreatePrefabSpawnPosition(uint prefabHash, Vector3 position, Quaternion rotation)
        {
            PrefabSpawnPosition prefabSpawnPosition = Plat.NewPrefabSpawnPosition();
            Plat.SetSpawnPosition(prefabSpawnPosition, prefabHash, position, rotation);
            return prefabSpawnPosition;
        }

        public static ChiselDefinition CreateChiselDefinition(string name, int hash, Glyph glyph, ChiselBlockSize size, List<int> chiselOrder, PrefabSpawnPosition[] prefabs)
        {
            ChiselDefinition chiselDefinition = Plat.Copy(Plat.Need<ChiselDefinition>(46222u, "chisel definition 46222"));
            chiselDefinition.name = name;
            Plat.SetHash(chiselDefinition, hash);
            chiselDefinition.recipeGlyph = glyph;
            Plat.SetChisel(chiselDefinition, size, chiselOrder, prefabs);
            return chiselDefinition;
        }

        public static void SetUpChiselDefinition(ChiselDefinition chiselDefinition, Glyph glyph)
        {
            FixChiselDefinitionInspectorValues(chiselDefinition, glyph);
            RegisterChiselDefinition(chiselDefinition);
            AddToPageCollection(Plat.Need<PageCollection>((uint)PageCollectionHashes.ChiselingCollection, "Chiseling page collection"), chiselDefinition);
        }

        public static ChiselDefinition SetUpChiselDefinitionThroughCode(string name, int hash, Glyph glyph, ChiselBlockSize size, List<int> chiselOrder, PrefabSpawnPosition[] prefabs)
        {
            ChiselDefinition chiselDefinition = CreateChiselDefinition(name, hash, glyph, size, chiselOrder, prefabs);
            RegisterChiselDefinition(chiselDefinition);
            AddToPageCollection(Plat.Need<PageCollection>((uint)PageCollectionHashes.ChiselingCollection, "Chiseling page collection"), chiselDefinition);
            return chiselDefinition;
        }
    }
}
