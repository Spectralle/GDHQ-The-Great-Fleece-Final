using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class TriggerActions : MonoBehaviour
{
    [SerializeField, Tag] private string _activationTag;
    [SerializeField] private bool _singleActivation = true;
    [SerializeField, Range(0, 10)] private int _activationCooldown = 5;
    [HorizontalLine]
    [SerializeField] private UnityEvent _onEnterActions;
    [Header("No single activation with Stay events. Cooldown used instead.")]
    [SerializeField] private UnityEvent _onStayActions;
    [SerializeField] private UnityEvent _onExitActions;

    private bool _hasActivated;
    private float _cooldownTimer = 0f;


    private void OnTriggerEnter(Collider other)
    {
        if ((_singleActivation && !_hasActivated) || (!_singleActivation && _cooldownTimer <= 0f))
        {
            if (_activationTag != string.Empty && other.CompareTag(_activationTag))
            {
                _onEnterActions.Invoke();

                if (_singleActivation)
                    _hasActivated = true;
                else
                    _cooldownTimer = _activationCooldown;
            }
            else if (_activationTag == string.Empty)
            {
                _onEnterActions.Invoke();

                if (_singleActivation)
                    _hasActivated = true;
                else
                    _cooldownTimer = _activationCooldown;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_cooldownTimer <= 0f)
        {
            if (_activationTag != string.Empty && other.CompareTag(_activationTag))
            {
                _onStayActions.Invoke();
                _cooldownTimer = _activationCooldown;
            }
            else if (_activationTag == string.Empty)
            {
                _onStayActions.Invoke();
                _cooldownTimer = _activationCooldown;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((_singleActivation && !_hasActivated) || (!_singleActivation && _cooldownTimer <= 0f))
        {
            if (_activationTag != string.Empty && other.CompareTag(_activationTag))
            {
                _onExitActions.Invoke();

                if (_singleActivation)
                    _hasActivated = true;
                else
                    _cooldownTimer = _activationCooldown;
            }
            else if (_activationTag == string.Empty)
            {
                _onExitActions.Invoke();

                if (_singleActivation)
                    _hasActivated = true;
                else
                    _cooldownTimer = _activationCooldown;
            }
        }
    }

    private void Update()
    {
        if (_cooldownTimer > 0.0f)
            _cooldownTimer -= Time.deltaTime;
    }
}
