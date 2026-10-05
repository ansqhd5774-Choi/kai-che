using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class RuntimeCombatVisualSlot : MonoBehaviour
    {
        public string ContentId { get; private set; }
        public Transform ModelRoot { get; private set; }
        public Transform WeaponRoot { get; private set; }
        public bool IsPlaceholder3D { get; private set; }

        public void Configure(
            string contentId,
            Transform modelRoot,
            Transform weaponRoot,
            bool isPlaceholder3D)
        {
            ContentId = contentId ?? string.Empty;
            ModelRoot = modelRoot;
            WeaponRoot = weaponRoot;
            IsPlaceholder3D = isPlaceholder3D;
        }

        public GameObject ReplaceModel(GameObject prefab)
        {
            if (prefab == null || ModelRoot == null)
                return null;

            for (int i = ModelRoot.childCount - 1; i >= 0; i--)
                Destroy(ModelRoot.GetChild(i).gameObject);

            GameObject instance = Instantiate(prefab, ModelRoot);
            instance.name = prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            IsPlaceholder3D = false;

            Debug.Log(
                "KAI_CHE_VISUAL_PREFAB_REPLACED_"
                + ContentId);
            return instance;
        }
    }
}
