using System;
using System.Collections.Generic;
using Alta;
using Alta.Blacksmithing;
using Alta.Carpentry;
using Alta.Inventory;
using Alta.Networking;
using Alta.Pages;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomRecipesAPI
{
    internal static class Plat
    {
        internal static T Find<T>(uint hash) where T : HashedGeneralValue<T>
        {
            HashedGeneralValue<T>.CheckItems();
            Il2CppSystem.Collections.Generic.Dictionary<uint, T> items = HashedGeneralValue<T>.items;
            return items != null && items.ContainsKey(hash) ? items[hash] : null;
        }

        internal static T Need<T>(uint hash, string what) where T : HashedGeneralValue<T>
        {
            T value = Find<T>(hash);
            if (value == null)
                throw new InvalidOperationException("the game has no " + what + " (" + hash + ")");
            return value;
        }

        internal static void Register<T>(T value) where T : HashedGeneralValue<T>
        {
            HashedGeneralValue<T>.CheckItems();
            Il2CppSystem.Collections.Generic.Dictionary<uint, T> items = HashedGeneralValue<T>.items;
            if (items == null)
                throw new InvalidOperationException("the game's " + typeof(T).Name + " list is not loaded");
            uint hash = value.Hash;
            if (items.ContainsKey(hash))
                throw new ArgumentException("An item with the same key has already been added. Key: " + hash);
            items.Add(hash, value);
        }

        internal static void SetHash(HashedGeneralValue value, int hash)
        {
            value.hash = hash;
        }

        internal static T Copy<T>(T asset) where T : Object
        {
            Object made = Object.Instantiate((Object)asset);
            return made == null ? null : made.TryCast<T>();
        }

        internal static PickupDockSettings DockSettings(string name)
        {
            PickupDockSettings found = LoadedDockSettings(name);
            if (found == null)
            {
                PrefabManager.PrepareSpawnSetups();
                found = LoadedDockSettings(name);
            }
            if (found == null)
                Port.Error("the game has no dock settings named " + name + ", so its filter is left as it was");
            return found;
        }

        static PickupDockSettings LoadedDockSettings(string name)
        {
            foreach (Object o in Resources.FindObjectsOfTypeAll(Il2CppType.Of<PickupDockSettings>()))
            {
                PickupDockSettings settings = o == null ? null : o.TryCast<PickupDockSettings>();
                if (settings != null && settings.name == name)
                    return settings;
            }
            return null;
        }

        internal static PickupTag Tag(string path)
        {
            Object o = Resources.Load(path, Il2CppType.Of<PickupTag>());
            PickupTag tag = o == null ? null : o.TryCast<PickupTag>();
            if (tag == null)
                Port.Error("the game has no pickup tag at " + path + ", so no item gets it");
            return tag;
        }

        internal static int AddUnique(Il2CppSystem.Collections.Generic.List<Item> list, List<Item> others)
        {
            int added = 0;
            foreach (Item other in others)
            {
                if (!Contains(list, other))
                {
                    list.Add(other);
                    added++;
                }
            }
            return added;
        }

        internal static bool AddUnique(Il2CppSystem.Collections.Generic.List<PickupTag> list, PickupTag tag)
        {
            if (Contains(list, tag))
                return false;
            list.Add(tag);
            return true;
        }

        static bool Contains<T>(Il2CppSystem.Collections.Generic.List<T> list, T value) where T : Object
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Same(list[i], value))
                    return true;
            }
            return false;
        }

        static bool Same(Object a, Object b)
        {
            if (a == null || b == null)
                return (object)a == null && (object)b == null;
            return a.Pointer == b.Pointer;
        }

        internal static MouldItemComponent NewMouldItemComponent()
        {
            ScriptableObject made = ScriptableObject.CreateInstance(Il2CppType.Of<MouldItemComponent>());
            return made == null ? null : made.TryCast<MouldItemComponent>();
        }

        internal static void SetDefinition(MouldItemComponent component, MouldDefinition definition)
        {
            component.definition = definition;
        }

        internal static Il2CppSystem.Collections.Generic.List<ItemComponent> Components(Item item) => item.components;

        internal static ItemSet AllowedMaterials(MouldDefinition definition)
        {
            if (definition == null)
                return null;
            return definition.allowedMaterials;
        }

        internal static void SetAllowedMaterials(MouldDefinition definition, ItemSet materials)
        {
            definition.allowedMaterials = materials;
        }

        internal static void SetProduct(MouldDefinition definition, Item product)
        {
            definition.product = product;
        }

        internal static void FixRecipeItems(SmeltingRecipe recipe, Item[] inputs, Item[] outputs)
        {
            Il2CppReferenceArray<ItemCount> input = recipe.input;
            for (int i = 0; i < input.Length; i++)
                input[i].item = inputs[i];
            Il2CppReferenceArray<ItemCount> output = recipe.output;
            for (int i = 0; i < output.Length; i++)
                output[i].item = outputs[i];
        }

        internal static void AppendRecipe(SmelterUpgrades upgrades, SmeltingRecipe recipe)
        {
            List<SmeltingRecipe> list = List(upgrades.recipes);
            list.Add(recipe);
            upgrades.recipes = Array(list);
        }

        internal static ItemCount NewItemCount()
        {
            SmeltingRecipe donor = Need<SmeltingRecipe>(31642u, "smelting recipe 31642");
            Il2CppReferenceArray<ItemCount> input = donor.input;
            if (input == null || input.Length == 0)
                throw new InvalidOperationException("smelting recipe 31642 has no input to copy an item count from");
            return Clone(input[0]);
        }

        internal static void SetItemCount(ItemCount itemCount, Item item, int count)
        {
            itemCount.item = item;
            itemCount.count = count;
        }

        internal static void SetRecipe(SmeltingRecipe recipe, float duration, ItemCount[] inputs, ItemCount[] outputs)
        {
            recipe.duration = duration;
            recipe.input = inputs == null ? null : Array(inputs);
            recipe.output = outputs == null ? null : Array(outputs);
        }

        internal static void AppendPage(PageCollection collection, Object page)
        {
            List<Object> list = List(collection.pageBases);
            list.Add(page);
            collection.pageBases = Array(list);
        }

        internal static PrefabSpawnPosition NewPrefabSpawnPosition()
        {
            ChiselDefinition donor = Need<ChiselDefinition>(46222u, "chisel definition 46222");
            Il2CppReferenceArray<PrefabSpawnPosition> prefabs = donor.prefabs;
            if (prefabs == null || prefabs.Length == 0)
                throw new InvalidOperationException("chisel definition 46222 has no prefab position to copy");
            return Clone(prefabs[0]);
        }

        internal static void SetSpawnPosition(PrefabSpawnPosition spawn, uint prefabHash, Vector3 position, Quaternion rotation)
        {
            spawn.prefabHash = prefabHash;
            spawn.position = position;
            spawn.rotation = rotation;
        }

        internal static void SetChisel(ChiselDefinition definition, ChiselBlockSize size, List<int> order, PrefabSpawnPosition[] prefabs)
        {
            definition.size = size;
            Il2CppSystem.Collections.Generic.List<int> kept = definition.chiselOrder;
            if (order == null)
                definition.chiselOrder = null;
            else
            {
                kept.Clear();
                foreach (int step in order)
                    kept.Add(step);
            }
            definition.prefabs = prefabs == null ? null : Array(prefabs);
        }

        static T Clone<T>(T value) where T : Il2CppSystem.Object
        {
            Il2CppSystem.Object copy = value.MemberwiseClone();
            T typed = copy == null ? null : copy.TryCast<T>();
            if (typed == null)
                throw new InvalidOperationException("the copy of a " + typeof(T).Name + " came back empty");
            return typed;
        }

        static Il2CppReferenceArray<T> Array<T>(IList<T> list) where T : Il2CppSystem.Object
        {
            Il2CppSystem.Array made = Il2CppSystem.Array.CreateInstance(Il2CppType.Of<T>(), list.Count);
            Il2CppReferenceArray<T> array = new Il2CppReferenceArray<T>(made.Pointer);
            for (int i = 0; i < list.Count; i++)
                array[i] = list[i];
            return array;
        }

        static List<T> List<T>(Il2CppReferenceArray<T> array) where T : Il2CppSystem.Object
        {
            List<T> list = new List<T>();
            if (array == null)
                return list;
            for (int i = 0; i < array.Length; i++)
                list.Add(array[i]);
            return list;
        }
    }
}
