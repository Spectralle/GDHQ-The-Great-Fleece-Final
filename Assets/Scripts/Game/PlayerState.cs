using UnityEngine;

namespace TGF
{
    public class PlayerState : MonoBehaviour
    {
        [SerializeField] private AnimState _animState;


        public AnimState Get() => _animState;
        public void Set(AnimState state) => _animState = state;
        public bool Compare(AnimState state) => _animState == state;
    }
}