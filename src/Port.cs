using System;
using MelonLoader;

namespace CustomRecipesAPI
{
    internal static class Port
    {
        internal const string Version = "1.0.0";

        internal const string Release = "v10";

        internal const string Build = "Quest";

        internal static readonly int[] OreAndIngotOrder =
        {
            42614, 42566, 5732, 5698, 4758, 5802, 7204, 17090, 57718, 60398, 24084, 32224, 16422, 30996, 24016
        };

        const int LateStartFrames = 300;

        static MelonLogger.Instance log;
        static bool done;
        static int frames;
        static Action setup;

        internal static int Moulds;
        internal static int SmeltingRecipes;
        internal static int Chisels;
        internal static int Pages;
        internal static int Filtered;

        internal static void Init(MelonLogger.Instance logger, Action run)
        {
            log = logger;
            setup = run;
            Info(Build + " build of CGNik's CustomRecipesAPI " + Release + ", player side only");
            MelonEvents.OnUpdate.Subscribe(Tick);
        }

        internal static void Setup(string from)
        {
            if (done || setup == null)
                return;
            done = true;
            Info("setting up from " + from);
            Run("setup", setup);
        }

        static void Tick()
        {
            if (done)
                return;
            if (++frames == LateStartFrames)
                Setup("frame " + LateStartFrames + ", late start never came");
        }

        internal static void Run(string what, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Error(what + " failed: " + ex);
            }
        }

        internal static string Summary()
        {
            return "ready: " + Moulds + Plural(Moulds, " mould") + ", " + SmeltingRecipes + Plural(SmeltingRecipes, " smelting recipe") + ", " + Chisels + Plural(Chisels, " chisel shape")
                + ", " + Pages + Plural(Pages, " page") + " added, " + Filtered + Plural(Filtered, " filter entry", " filter entries") + " added";
        }

        static string Plural(int n, string one, string many = null) => n == 1 ? one : many ?? one + "s";

        internal static void Info(string message) => log?.Msg(message);

        internal static void Error(string message)
        {
            if (log != null)
                log.Error(message);
            else
                MelonLogger.Error(message);
        }
    }
}
