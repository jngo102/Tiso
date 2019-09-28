using System;
using Modding;
using UnityEngine;

// Taken from https://github.com/5FiftySix6/HollowKnight.Pale-Prince/blob/master/Pale%20Prince/SaveSettings.cs

namespace Tiso
{
    [Serializable]
    public class SaveSettings : ModSettings, ISerializationCallbackReceiver
    {
        public BossStatue.Completion completion = new BossStatue.Completion
        {
            isUnlocked = true
        };

        public bool AltStatue
        {
            get => GetBool();
            set => SetBool(value);
        }

        public void OnBeforeSerialize()
        {
            StringValues["Completion"] = JsonUtility.ToJson(completion);
        }

        public void OnAfterDeserialize()
        {
            StringValues.TryGetValue("Completion", out string @out);

            if (string.IsNullOrEmpty(@out)) return;

            completion = JsonUtility.FromJson<BossStatue.Completion>(@out);
        }
    }
}