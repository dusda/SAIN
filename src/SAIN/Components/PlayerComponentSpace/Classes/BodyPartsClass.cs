﻿using EFT;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Models.Structs;
using SAIN.SAINComponent;
using System.Collections.Generic;
using System.Linq;

namespace SAIN.Components
{
  public class BodyPartsClass : PlayerComponentBase
  {
    public PartDictionary Parts { get; } = new PartDictionary();

    public SAINBodyPart[] PartsArray { get; private set; }

    public BodyPartsClass(PlayerComponent component) : base(component)
    {
      createParts();
      PartsArray = Parts.Values.ToArray();
    }

    private void createParts()
    {
      PlayerBones playerBones = Player.PlayerBones;
      foreach (var part in PartToBoneTypes.PartsToCollidersTypes)
      {
        EBodyPart bodyPartType = part.Key;
        SAINBodyPart bodyPart = createPart(bodyPartType, playerBones, part.Value);
        Parts.Add(bodyPartType, bodyPart);
      }

      //StringBuilder stringBuilder = new StringBuilder();
      //stringBuilder.AppendLine($"Got Parts:");
      //foreach (var part in Parts) {
      //    stringBuilder.AppendLine($"{part.Key} : {part.Value.Colliders.Count}");
      //}
      //Logger.LogDebug(stringBuilder.ToString());
    }

    private BifacialTransform getTransform(EBodyPart bodyPart, PlayerBones bones)
    {
      switch (bodyPart)
      {
        case EBodyPart.Head:
          return bones.Head;

        case EBodyPart.Chest:
          return bones.Ribcage;

        case EBodyPart.Stomach:
          return bones.Pelvis;

        case EBodyPart.LeftArm:
          return bones.LeftShoulder;

        case EBodyPart.RightArm:
          return bones.RightShoulder;

        case EBodyPart.LeftLeg:
          return bones.LeftThigh1;

        default:
          return bones.RightThigh1;
      }
    }

    private SAINBodyPart createPart(EBodyPart bodyPartType, PlayerBones playerBones, EBodyPartColliderType[] colliderTypes)
    {
      BifacialTransform transform = getTransform(bodyPartType, playerBones);
      if (transform == null)
      {
        Logger.LogDebug($"{bodyPartType} has null bifacial transform");
      }
      List<BodyPartCollider> colliders = getColliders(playerBones, colliderTypes);
      if (colliders?.Count == 0)
      {
        Logger.LogWarning($"No Colliders for {bodyPartType}!");
      }
      return new SAINBodyPart(bodyPartType, transform, colliders);
    }

    private List<BodyPartCollider> getColliders(PlayerBones playerBones, EBodyPartColliderType[] colliderTypes)
    {
      var colliderList = new List<BodyPartCollider>();
      if (playerBones == null)
      {
        Logger.LogError("Player bones null");
        return colliderList;
      }
      if (colliderTypes == null)
      {
        Logger.LogError("colliderTypes null");
        return colliderList;
      }
      var bodyParts = playerBones.BodyPartCollidersDictionary;
      foreach (var type in colliderTypes)
      {
        if (!bodyParts.TryGetValue(type, out BodyPartCollider collider))
        {
          Logger.LogDebug($"{type} not in collider dictionary");
        }
        else if (collider == null || collider.Collider == null)
        {
          Logger.LogDebug($"{type} has null collider");
        }
        else if (collider.transform == null)
        {
          Logger.LogDebug($"{type} has null transform");
        }
        else
        {
          if (collider.Collider.transform == null)
          {
            Logger.LogDebug($"{type} collider.Collider has null transform");
          }
          colliderList.Add(collider);
        }
      }
      return colliderList;
    }

    private static class PartToBoneTypes
    {
      public static readonly EBodyPart[] PartTypes =
      [
          EBodyPart.Head,
                EBodyPart.Chest,
                EBodyPart.Stomach,
                EBodyPart.LeftArm,
                EBodyPart.RightArm,
                EBodyPart.LeftLeg,
                EBodyPart.RightLeg,
            ];

      private static readonly EBodyPartColliderType[] _headParts = [
          //EBodyPartColliderType.ParietalHead,
          EBodyPartColliderType.BackHead,
                EBodyPartColliderType.Jaw,
                EBodyPartColliderType.HeadCommon,
                EBodyPartColliderType.NeckFront,
                EBodyPartColliderType.NeckBack,
            ];

      private static readonly EBodyPartColliderType[] _upperBodyParts = [
          EBodyPartColliderType.SpineTop,
                EBodyPartColliderType.RibcageUp,
                EBodyPartColliderType.RightSideChestUp,
                EBodyPartColliderType.LeftSideChestUp,
            ];

      private static readonly EBodyPartColliderType[] _lowerBodyParts = [
          EBodyPartColliderType.SpineDown,
                EBodyPartColliderType.RibcageLow,
                EBodyPartColliderType.RightSideChestDown,
                EBodyPartColliderType.LeftSideChestDown,
                EBodyPartColliderType.Pelvis,
            ];

      private static readonly EBodyPartColliderType[] _leftArmParts = [
          EBodyPartColliderType.LeftUpperArm,
                EBodyPartColliderType.LeftForearm,
            ];

      private static readonly EBodyPartColliderType[] _rightArmParts = [
          EBodyPartColliderType.RightUpperArm,
                EBodyPartColliderType.RightForearm,
            ];

      private static readonly EBodyPartColliderType[] _leftLegParts = [
          EBodyPartColliderType.LeftCalf,
                EBodyPartColliderType.LeftThigh,
            ];

      private static readonly EBodyPartColliderType[] _rightLegParts = [
          EBodyPartColliderType.RightCalf,
                EBodyPartColliderType.RightThigh,
            ];

      public static readonly Dictionary<EBodyPart, EBodyPartColliderType[]> PartsToCollidersTypes = new()
            {
                {EBodyPart.Head,  _headParts },
                {EBodyPart.Chest,  _upperBodyParts },
                {EBodyPart.Stomach,  _lowerBodyParts },
                {EBodyPart.LeftArm,  _leftArmParts },
                {EBodyPart.LeftLeg,  _leftLegParts },
                {EBodyPart.RightArm,  _rightArmParts },
                {EBodyPart.RightLeg,  _rightLegParts },
            };
    }
  }
}
