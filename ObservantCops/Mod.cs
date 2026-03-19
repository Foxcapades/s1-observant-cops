using MelonLoader;
#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.Core.Items.Framework;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Vision;
using Il2CppSystem;
#elif MONO
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
#endif

[assembly: MelonInfo(typeof(ObservantCops.Mod), ObservantCops.Mod.MOD_NAME, "1.1.0", "Foxcapades")]
[assembly: MelonGame("TVGS", "Schedule I")]

#nullable enable
namespace ObservantCops {
  public class Mod: MelonMod {
    public const string MOD_NAME = "Observant Cops";

    private const string STATE_LABEL = "holding_suspicious_item";

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
      if (sceneName == "Main")
        sceneSetup();
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
      if (sceneName == "Main")
        teardown();
    }

    private static void setupActions() {
      var inv = PlayerSingleton<PlayerInventory>.Instance;

      if (inv == null) {
        Melon<Mod>.Instance.LoggerInstance.Error("inventory was null");
        return;
      }

      playerSetup(inv);
    }

    private static void onEquippedSlotChanged(int _) {
      var legality = PlayerSingleton<PlayerInventory>.Instance?.EquippedItem?.Definition?.legalStatus
        ?? ELegalStatus.Legal;

      var player = Player.Local!;

      if (player.IsInVehicle) {
        player.Visibility.RemoveState(STATE_LABEL);
        return;
      }

      if (player.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None) {
        if (legality != ELegalStatus.Legal) {
          player.Visibility.ApplyState(STATE_LABEL, EVisualState.DisobeyingCurfew);
        } else {
          player.Visibility.RemoveState(STATE_LABEL);
        }
      } else {
        player.Visibility.RemoveState(STATE_LABEL, 4f);
      }
    }

#if IL2CPP
    private static Action? setupActionRef;
    private static Action<int>? onEquipChangeRef;

    private static void sceneSetup() {
      setupActionRef = DelegateSupport.ConvertDelegate<Action>(setupActions);
      Player.onLocalPlayerSpawned += setupActionRef;
    }

    private static void playerSetup(PlayerInventory inv) {
      onEquipChangeRef = DelegateSupport.ConvertDelegate<Action<int>>(onEquippedSlotChanged);
      inv.onEquippedSlotChanged += onEquipChangeRef;
    }

    private static void teardown() {
      if (setupActionRef != null)
        Player.onLocalPlayerSpawned -= setupActionRef;

      if (onEquipChangeRef != null) {
        var inv = PlayerSingleton<PlayerInventory>.Instance;
        if (inv != null)
          inv.onEquippedSlotChanged -= onEquipChangeRef;
      }
    }
#elif MONO
    private static void sceneSetup() {
      Player.onLocalPlayerSpawned += setupActions;
    }

    private static void playerSetup(PlayerInventory inv) {
      inv.onEquippedSlotChanged += onEquippedSlotChanged;
    }

    private static void teardown() {
      Player.onLocalPlayerSpawned -= setupActions;

      var inv = PlayerSingleton<PlayerInventory>.Instance;

      if (inv != null)
        inv.onEquippedSlotChanged -= onEquippedSlotChanged;
    }
#endif
  }
}
