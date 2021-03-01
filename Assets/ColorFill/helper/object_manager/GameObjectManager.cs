using ColorFill.game.elements;
using UnityEngine;

namespace ColorFill.helper.object_manager
{
    public class GameObjectManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;
        
        public static GameObjectManager Instance { get; private set; }
        public int FullFillCount;

        void Awake()
        {
            Instance = this;
            Util.DontDestroyOnLoad<GameObjectManager>(gameObject);
        }

        public GameObject GetObject(GameObjectType type)
        {
            if (type == GameObjectType.FullFill)
            {
                FullFillCount += 1;
            }
            var typeIndex = (int) type;
            return Instantiate(prefabs[typeIndex]);
        }
        
        public GameObject GetObject(GameObjectType type,Transform parent)
        {
            var gameObj = GetObject(type);
            gameObj.transform.SetParent(parent);
            return gameObj;
        }

        public GameObject GetObject(GameObjectType type, Vector3 worldPosition)
        {
            if (type == GameObjectType.FullFill)
            {
                FullFillCount += 1;
            }
            var typeIndex = (int) type;
            var prefab = prefabs[typeIndex];
            return Instantiate(prefab, worldPosition, Quaternion.identity);
        }

        public void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}