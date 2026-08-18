using PEAKUsageSkills.Core;
using PEAKUsageSkills.GameAdapters;
using UnityEngine;

namespace PEAKUsageSkills.Tracking
{
    internal sealed class ActivitySampler : MonoBehaviour
    {
        private const float SampleInterval = 0.2f;
        private Character? sampledCharacter;
        private Vector3 lastPosition;
        private float lastSampleTime;
        private bool hasPosition;

        private void Update()
        {
            if (Time.unscaledTime - lastSampleTime < SampleInterval)
            {
                return;
            }

            lastSampleTime = Time.unscaledTime;
            Character character = Character.localCharacter;
            if (character == null || !Plugin.RunState.IsGameplayScene)
            {
                ResetSample(character);
                return;
            }

            // PEAK moves the character through its physics body parts; the Character
            // root transform can remain stationary while the player is moving.
            Vector3 position = character.Center;
            if (sampledCharacter != character || !hasPosition)
            {
                sampledCharacter = character;
                lastPosition = position;
                hasPosition = true;
                return;
            }

            Vector3 delta = position - lastPosition;
            lastPosition = position;
            float distance = delta.magnitude;
            if (Plugin.Diagnostics != null)
            {
                Plugin.Diagnostics.Metrics.LastPhysicalMovementDistance = distance;
            }

            // Scene transitions, teleports, and respawns are not physical work samples.
            if (distance > 5f)
            {
                Plugin.Diagnostics?.RecordRejected(SkillId.Strength, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.WallClimbing, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.RopeClimbing, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.VineClimbing, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.Athletics, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.PackRat, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.WetGrip, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.ClimbingTenacity, "TeleportDelta", distance);
                Plugin.Diagnostics?.RecordRejected(SkillId.HungerTolerance, "TeleportDelta", distance);
                return;
            }

            if (distance >= 0.01f)
            {
                float rawWeight = Plugin.Diagnostics?.Metrics.RawWeight ?? 0f;
                if (rawWeight >= 0.025f)
                {
                    double work = rawWeight * distance;
                    Plugin.Progression.AwardWork(
                        SkillId.Strength,
                        work,
                        Plugin.Settings.StrengthXpPerWork.Value,
                        character.data.isClimbingAnything ? "WeightedClimbing" : "WeightedMovement");
                }

                int packRatTrainingLoad = InventorySkillService.GetPackRatTrainingLoad(character);
                if (packRatTrainingLoad > 0)
                {
                    Plugin.Progression.AwardWork(
                        SkillId.PackRat,
                        distance * packRatTrainingLoad,
                        Plugin.Settings.PackRatXpPerWork.Value,
                        Plugin.Progression.GetLevel(SkillId.PackRat) < 10
                            ? "FullVanillaInventoryBootstrap"
                            : "OverCapacityMovement");
                }

                float hunger = character.refs?.afflictions?.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) ?? 0f;
                double hungerWork = SkillMath.HungerMovementWork(
                    hunger,
                    distance,
                    Plugin.Settings.HungerTrainingThreshold.Value);
                if (hungerWork > 0d)
                {
                    Plugin.Progression.AwardWork(
                        SkillId.HungerTolerance,
                        hungerWork,
                        Plugin.Settings.HungerMovementXpPerWork.Value,
                        "HungryMovement");
                }
            }

            bool wallClimbing = character.data.isClimbing
                && !character.data.isRopeClimbing
                && !character.data.isVineClimbing;
            bool intentionalClimbInput = character.input != null && character.input.movementInput.sqrMagnitude >= 0.01f;
            float verticalProgress = wallClimbing && intentionalClimbInput ? distance : 0f;
            float ropeProgress = character.data.isRopeClimbing && intentionalClimbInput ? distance : 0f;
            float vineProgress = character.data.isVineClimbing && intentionalClimbInput ? distance : 0f;
            if (Plugin.Diagnostics != null)
            {
                Plugin.Diagnostics.Metrics.LastWallVerticalDelta = verticalProgress;
                Plugin.Diagnostics.Metrics.LastRopeVerticalDelta = ropeProgress;
                Plugin.Diagnostics.Metrics.LastVineVerticalDelta = vineProgress;
            }

            if (verticalProgress >= 0.005f)
            {
                if (Plugin.Diagnostics != null)
                {
                    Plugin.Diagnostics.Metrics.SessionWallVerticalDistance += verticalProgress;
                }

                Plugin.Progression.AwardWork(
                    SkillId.WallClimbing,
                    verticalProgress,
                    Plugin.Settings.WallClimbingXpPerMeter.Value,
                    "IntentionalWallDistance");

                float slippy = Mathf.Clamp01(character.data.slippy);
                if (slippy > 0.001f)
                {
                    Plugin.Progression.AwardWork(
                        SkillId.WetGrip,
                        verticalProgress * slippy,
                        Plugin.Settings.WetGripXpPerMeter.Value,
                        "SlipperyWallDistance");
                }

                if (character.GetTotalStamina() < 0.20f)
                {
                    Plugin.Progression.AwardWork(
                        SkillId.ClimbingTenacity,
                        verticalProgress,
                        Plugin.Settings.ClimbingTenacityXpPerMeter.Value,
                        "LowStaminaWallDistance");
                }
            }

            if (ropeProgress >= 0.005f)
            {
                Plugin.Progression.AwardWork(
                    SkillId.RopeClimbing,
                    ropeProgress,
                    Plugin.Settings.RopeClimbingXpPerMeter.Value,
                    "IntentionalRopeDistance");
            }

            if (vineProgress >= 0.005f)
            {
                Plugin.Progression.AwardWork(
                    SkillId.VineClimbing,
                    vineProgress,
                    Plugin.Settings.VineClimbingXpPerMeter.Value,
                    "IntentionalVineDistance");
            }

            float horizontalDistance = new Vector2(delta.x, delta.z).magnitude;
            bool intentionalGroundMovement = character.data.isGrounded
                && !character.data.isClimbingAnything
                && character.input != null
                && character.input.movementInput.sqrMagnitude >= 0.01f;
            if (intentionalGroundMovement && horizontalDistance >= 0.01f)
            {
                Plugin.Progression.AwardWork(
                    SkillId.Athletics,
                    horizontalDistance,
                    character.data.isSprinting
                        ? Plugin.Settings.AthleticsSprintXpPerMeter.Value
                        : Plugin.Settings.AthleticsXpPerMeter.Value,
                    character.data.isSprinting ? "GroundSprinting" : "GroundMovement");
            }
        }

        private void ResetSample(Character? character)
        {
            sampledCharacter = character;
            hasPosition = false;
            if (Plugin.Diagnostics != null)
            {
                Plugin.Diagnostics.Metrics.LastWallVerticalDelta = 0f;
                Plugin.Diagnostics.Metrics.LastRopeVerticalDelta = 0f;
                Plugin.Diagnostics.Metrics.LastVineVerticalDelta = 0f;
            }
        }
    }
}
