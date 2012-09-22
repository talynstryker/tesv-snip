namespace TESVSnip.Domain.Model
{
    internal static class FlagDefs
    {
        public static readonly string[] RecFlags1 =
            {
                "ESM file", null, null, null, null, "Deleted", "Constant/HiddenFromLocalMap/BorderRegion/HasTreeLOD", "Localized/IsPerch/AddOnLODObject/TurnOffFire/TreatSpellsAsPowers", "MustUpdateAnims/Inaccessible/DoesntLightWater", "HiddenFromLocalMap/StartsDead/MotionBlurCastsShadows", "PersistentReference/QuestItem/DisplaysInMainMenu", "Initially disabled"
                , "Ignored", null, null, "Visible when distant", "RandomAnimationStart/NeverFades/IsfullLOD", "Dangerous/OffLimits(Interior cell)/DoesntLightLandscape/HighDetailLOD/CanHoldNPC", "Compressed", "CantWait/HasCurrents", "IgnoreObjectInteraction"
                , null, null, "IsMarker", null, "Obstacle/NoAIAcquire", "NavMeshFilter", "NavMeshBoundingBox", "MustExitToTalk/ShowInWorldMap", "ChildCanUse/DontHavokSettle", "NavMeshGround NoRespawn", "MultiBound", 
            };

        public static string GetRecFlags1Desc(uint flags)
        {
            string desc = string.Empty;
            bool b = false;
            for (int i = 0; i < 32; i++)
            {
                if ((flags & (uint)(1 << i)) > 0)
                {
                    if (b)
                    {
                        desc += ", ";
                    }

                    b = true;
                    desc += RecFlags1[i] == null ? "Unknown (" + ((uint)(1 << i)).ToString("x") + ")" : RecFlags1[i];
                }
            }

            return desc;
        }
    }
}