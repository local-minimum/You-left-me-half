using UnityEngine;

namespace DeCrawl.Primitives
{
    public class IdentifiableEntity : MonoBehaviour
    {
        [SerializeField, Tooltip("Leave empty to use game object name as identifier")]
        string overrideNameId;

        public string Id
        {
            get
            {
                return string.IsNullOrEmpty(overrideNameId) ? name : overrideNameId;
            }
        }
    }
}
