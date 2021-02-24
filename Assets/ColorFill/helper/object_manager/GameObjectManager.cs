using ColorFill.game.elements;
using UnityEngine;

namespace ColorFill.helper.object_manager
{
    public class GameObjectManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;
        
        public static GameObjectManager Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            Util.DontDestroyOnLoad<GameObjectManager>(gameObject);
        }

        public GameObject GetObject(GameObjectType type)
        {
            var typeIndex = (int) type;
            return Instantiate(prefabs[typeIndex]);
        }

        public void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}