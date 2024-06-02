using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Assertions;

using UnityEngine.InputSystem;

[Serializable]
public class RadialMenuSetting
{
    public float globalOffset;
    public bool useStickPress;
    public bool inverse = false;
    [HideInInspector] public float angleOffset;
    [HideInInspector] public RL radialRL;
    [HideInInspector] public RadialType radialType;
}

public enum RL
{
    Right,
    Left,
    Both,
    None
}

public enum RadialType
{
    Consonant,
    Vowel
}

public class TextEntryInputs : MonoBehaviour
{
    // elementに角度の範囲を持たせるほうがよさそう
    enum InputType
    {
        OnEnter,
        OnRelease,
        Double,
        DoubleRelease
    }

    enum InputState
    {
        ConsonantReady,
        ConsonantInputting,
        ConsonantDone,
        VowelReady,
        VowelInputting,
        VowelDone
    }

    [Header("Input Settings")]
    [SerializeField] private float _stickDeadZone = 0.35f;
    [SerializeField] private float _stickDeadZoneRelease = 0.125f;
    [SerializeField] private int _elementRightCount = 5;
    [SerializeField] private int _elementLeftCount = 5;
    [SerializeField] private InputType _inputType = InputType.OnEnter;
    [SerializeField] private bool _updateLabelBoth = false;

    [Header("Radial Menu Settings")]
    [SerializeField] private RadialMenuSetting _radialMenuSettingRightConsonant;
    [SerializeField] private RadialMenuSetting _radialMenuSettingRightVowel;
    [SerializeField] private RadialMenuSetting _radialMenuSettingLeftConsonant;
    [SerializeField] private RadialMenuSetting _radialMenuSettingLeftVowel;

    [SerializeField] private TypingManager _typingManager;

    [SerializeField] public UnityEvent OnClearText;
    [SerializeField] public UnityEvent OnStartTest;

    public int selectingIndex { get { return _index; } }

    private TextEntry _textEntry;
    private int[] _elementCount = new int[2];
    private RadialMenuSetting[,] _radialMenuSettings = new RadialMenuSetting[2, 2];
    private RadialType _currentRadialTypeRight = RadialType.Consonant;
    private RadialType _currentRadialTypeLeft = RadialType.Consonant;
    private Vector2[] _input = new Vector2[2];
    RL _nextRadialRL = RL.Both;
    bool[] _isStickReady = new bool[2] { true, true };

    [Header("Input Values")]
    [SerializeField] private int _index = 0;

    Vector2 _rightStickInput;
    Vector2 _leftStickInput;
    Vector2 _rightStickInputPrev;
    Vector2 _leftStickInputPrev;
    bool _isRightStickReleasedThisFrame = false;
    bool _isLeftStickReleasedThisFrame = false;
    bool _isRightStickPressed = false;
    bool _isLeftStickPressed = false;
    bool _isRightStickButtonReleasedThisFrame = false;
    bool _isLeftStickButtonReleasedThisFrame = false;
    bool _isChangePressed = false;
    bool _isBackSpacePressed = false;
    bool _isSendPressed = false;

    bool _hasRightStickInput = false;
    bool _hasLeftStickInput = false;
    bool _isStickPressed { get { return _isRightStickPressed || _isLeftStickPressed; } }
    bool _hasInput { get { return _hasRightStickInput || _hasLeftStickInput || _isStickPressed; } }

    RadialMenu _radialMenuRight { get { return _textEntry.radialMenuRight; } }
    RadialMenu _radialMenuLeft { get { return _textEntry.radialMenuLeft; } }
    InputState _inputState = InputState.ConsonantReady;

    bool HasStickInput(Vector2 input, bool currentHasStickInput)
    {
        if (currentHasStickInput)
        {
            return input.magnitude > _stickDeadZoneRelease;
        }
        else
        {
            return input.magnitude > _stickDeadZone;
        }
    }

    void SetRadialMenuSetting(RadialMenuSetting radialMenuSettings, RL radialRL, RadialType radialType)
    {
        radialMenuSettings.radialRL = radialRL;
        radialMenuSettings.radialType = radialType;
        if (radialMenuSettings.useStickPress)
        {
            radialMenuSettings.angleOffset = 360f / (float)(_elementCount[(int)radialRL] - 1);
        }
        else
        {
            radialMenuSettings.angleOffset = 360f / (float)_elementCount[(int)radialRL];
        }
    }

    void Awake()
    {
        _textEntry = GetComponent<TextEntry>();

        if (_textEntry == null)
            Debug.LogError("TextEntryInputs: TextEntry for radial menu " + gameObject.name + " could not be found. Please ensure this is an object parented to a canvas.");

        _elementCount[0] = _elementRightCount;
        _elementCount[1] = _elementLeftCount;

        SetRadialMenuSetting(_radialMenuSettingRightConsonant, RL.Right, RadialType.Consonant);
        SetRadialMenuSetting(_radialMenuSettingRightVowel, RL.Right, RadialType.Vowel);
        SetRadialMenuSetting(_radialMenuSettingLeftConsonant, RL.Left, RadialType.Consonant);
        SetRadialMenuSetting(_radialMenuSettingLeftVowel, RL.Left, RadialType.Vowel);

        _radialMenuSettings[(int)RL.Right, (int)RadialType.Consonant] = _radialMenuSettingRightConsonant;
        _radialMenuSettings[(int)RL.Right, (int)RadialType.Vowel] = _radialMenuSettingRightVowel;
        _radialMenuSettings[(int)RL.Left, (int)RadialType.Consonant] = _radialMenuSettingLeftConsonant;
        _radialMenuSettings[(int)RL.Left, (int)RadialType.Vowel] = _radialMenuSettingLeftVowel;

        _input[(int)RL.Right] = _rightStickInput;
        _input[(int)RL.Left] = _leftStickInput;
    }

    void Start()
    {
        _radialMenuRight.UpdateRadialMenu();
        _radialMenuLeft.UpdateRadialMenu();
    }

    public void OnRightStick(InputValue value)
    {
        var cur = _hasRightStickInput;
        if (cur)
        {
            _rightStickInputPrev = _rightStickInput;
        }

        var v = value.Get<Vector2>();
        if (cur && isCircleLineCollision(_rightStickInputPrev, v, _stickDeadZoneRelease))
        {
            v = new Vector2(0, 0);
        }
        _rightStickInput = v;
    }

    public void OnLeftStick(InputValue value)
    {
        var cur = _hasLeftStickInput;
        if (cur)
        {
            _leftStickInputPrev = _leftStickInput;
        }

        var v = value.Get<Vector2>();
        if (cur && isCircleLineCollision(_leftStickInputPrev, v, _stickDeadZoneRelease))
        {
            v = new Vector2(0, 0);
        }
        _leftStickInput = v;
    }

    bool isCircleLineCollision(Vector2 A, Vector2 B, float r)
    {
        Vector2 AB = B - A;
        Vector2 AO = -A;
        float t = Vector2.Dot(AO, AB) / Vector2.Dot(AB, AB);
        t = Mathf.Clamp(t, 0, 1);
        Vector2 D = A + t * AB;
        Vector2 DO = D;
        return DO.magnitude <= r;
    }

    public void OnRightStickButton(InputValue value)
    {
        var v = value.isPressed;
        var cur = _isRightStickPressed;
        _isRightStickPressed = v && _radialMenuSettings[(int)RL.Right, (int)_currentRadialTypeRight].useStickPress;
        _isRightStickButtonReleasedThisFrame = cur && !_isRightStickPressed;
    }

    public void OnLeftStickButton(InputValue value)
    {
        var v = value.isPressed;
        var cur = _isLeftStickPressed;
        _isLeftStickPressed = v && _radialMenuSettings[(int)RL.Left, (int)_currentRadialTypeLeft].useStickPress;
        _isLeftStickButtonReleasedThisFrame = cur && !_isLeftStickPressed;
    }

    public void OnChange(InputValue value)
    {
        var v = value.isPressed;
        _isChangePressed = v;
    }

    public void OnBackSpace(InputValue value)
    {
        var v = value.isPressed;
        _isBackSpacePressed = v;
    }

    public void OnSend(InputValue value)
    {
        var v = value.isPressed;
        _isSendPressed = v;
    }

    public void OnClear(InputValue value)
    {
        OnClearText?.Invoke();
        ResetInputState();
    }

    public void OnStart(InputValue value)
    {
        OnStartTest?.Invoke();
    }

    void UpdateInput()
    {
        var cur = false;
        // RightStick
        cur = _hasRightStickInput;
        _hasRightStickInput = HasStickInput(_rightStickInput, _hasRightStickInput);
        _isRightStickReleasedThisFrame = cur && !_hasRightStickInput;

        // LeftStick
        cur = _hasLeftStickInput;
        _hasLeftStickInput = HasStickInput(_leftStickInput, _hasLeftStickInput);
        _isLeftStickReleasedThisFrame = cur && !_hasLeftStickInput;

        // RightStickButton
        // cur = _isRightStickPressed;
        // _isRightStickButtonReleasedThisFrame = cur && !_isRightStickPressed;

        // LeftStickButton
        // cur = _isLeftStickPressed;
        // _isLeftStickButtonReleasedThisFrame = cur && !_isLeftStickPressed;
    }

    void LateUpdate()
    {
        _isRightStickButtonReleasedThisFrame = false;
        _isLeftStickButtonReleasedThisFrame = false;
        _isRightStickReleasedThisFrame = false;
        _isLeftStickReleasedThisFrame = false;

        if (!_isRightStickPressed && !_hasRightStickInput)
        {
            _isStickReady[(int)RL.Right] = true;
        }

        if (!_isLeftStickPressed && !_hasLeftStickInput)
        {
            _isStickReady[(int)RL.Left] = true;
        }

        _isChangePressed = false;
        _isBackSpacePressed = false;
        _isSendPressed = false;
    }

    void Update()
    {
        UpdateInput();

        if (_inputState == InputState.ConsonantReady)
        {
            if (_isChangePressed)
            {
                _textEntry.ChangeCharacterCase();
                return;
            }
            if (_isBackSpacePressed)
            {
                _textEntry.DeleteCharacter();
                return;
            }
            if (_isSendPressed)
            {
                _textEntry.Send();
                return;
            }
        }
        else
        {
            if (_isBackSpacePressed)
            {
                ResetInputState();
                _typingManager.Cancel();
                return;
            }
        }

        if (_typingManager.IsMeasuring && _typingManager.HasWrongInput)
        {
            ResetInputState();
            return;
        }

        if (_inputType == InputType.OnEnter)
        {
            UpdateInputStateOnEnter();
        }
        else if (_inputType == InputType.OnRelease)
        {
            UpdateInputStateOnRelease();
        }
        else if (_inputType == InputType.Double)
        {
            UpdateInputStateDouble();
        }
        else if (_inputType == InputType.DoubleRelease)
        {
            UpdateInputStateDoubleRelease();
        }
    }

    public void ResetInputState()
    {
        _inputState = InputState.ConsonantReady;
        _textEntry.SetRadialMenuSetting(_radialMenuSettingRightConsonant, _radialMenuSettingLeftConsonant);
        _currentRadialTypeRight = RadialType.Consonant;
        _currentRadialTypeLeft = RadialType.Consonant;

        _textEntry.ResetTextEntry();
    }

    void UpdateInputStateOnEnter()
    {
        if (_inputState == InputState.ConsonantReady)
        {
            var radialRL = GetInputRadialRL();
            if (radialRL == RL.None)
            {
                return;
            }
            else
            {
                _index = CalculateInputIndex(radialRL, RadialType.Consonant);
                InputRadialMenu(_index, radialRL, RadialType.Consonant);
                _nextRadialRL = InverseRL(radialRL);
                _inputState = InputState.VowelReady;
                SetVowelRadialMenuSetting(_nextRadialRL);
                return;
            }
        }

        if (_inputState == InputState.VowelReady)
        {
            var radialRL = GetInputRadialRL();
            if (radialRL == RL.None)
            {
                return;
            }
            else
            {
                _index = CalculateInputIndex(radialRL, RadialType.Vowel);
                InputRadialMenu(_index, radialRL, RadialType.Vowel);
                ResetInputState();
            }
        }
    }

    void UpdateInputStateOnRelease()
    {
        if (_inputState == InputState.ConsonantReady)
        {
            var radialRL = GetInputRadialRL();
            if (radialRL == RL.None)
            {
                return;
            }
            else
            {
                _index = CalculateInputIndex(radialRL, RadialType.Consonant);
                // InputRadialMenu(_index, radialRL, RadialType.Consonant, isSelect: true, disableStick:  false);
                SelectRadialMenu(_index);
                _nextRadialRL = radialRL;
                _inputState = InputState.ConsonantInputting;
                return;
            }
        }

        if (_inputState == InputState.ConsonantInputting)
        {
            // TODO: 押し込みながら傾けて、放したあと傾きを戻すと2回入力される
            if (_isRightStickButtonReleasedThisFrame || _isLeftStickButtonReleasedThisFrame)
            {
                RL radialRL;
                if (_isRightStickButtonReleasedThisFrame)
                {
                    radialRL = RL.Right;
                }
                else
                {
                    radialRL = RL.Left;
                }
                InputRadialMenu(_index, radialRL, RadialType.Consonant, isSelect: false, disableStick: true);
                _nextRadialRL = InverseRL(radialRL);
                _inputState = InputState.VowelReady;
                // SetVowelRadialMenuSetting(_nextRadialRL);
                _textEntry.SetRadialMenuSetting(_radialMenuSettingRightVowel, _radialMenuSettingLeftVowel);
                return;
            }
            else if (_isRightStickReleasedThisFrame || _isLeftStickReleasedThisFrame)
            {
                if (_isRightStickReleasedThisFrame)
                {
                    _index = CalculateStickInputIndex(_rightStickInputPrev, _radialMenuSettings[(int)RL.Right, (int)RadialType.Consonant]);
                }
                else
                {
                    _index = CalculateStickInputIndex(_leftStickInputPrev, _radialMenuSettings[(int)RL.Left, (int)RadialType.Consonant]) + _elementCount[(int)RL.Right];
                }

                InputRadialMenu(_index, RL.None, RadialType.Consonant, isSelect: false, disableStick: false);
                _nextRadialRL = InverseRL(_nextRadialRL);
                _inputState = InputState.VowelReady;
                // SetVowelRadialMenuSetting(_nextRadialRL);
                _textEntry.SetRadialMenuSetting(_radialMenuSettingRightVowel, _radialMenuSettingLeftVowel);
                _currentRadialTypeRight = RadialType.Vowel;
                _currentRadialTypeLeft = RadialType.Vowel;
                return;
            }
            else if (!_hasInput)
            {
                InputRadialMenu(_index, RL.None, RadialType.Consonant, disableStick: false);
                _nextRadialRL = InverseRL(_nextRadialRL);
                _inputState = InputState.VowelReady;
                // SetVowelRadialMenuSetting(_nextRadialRL);
                _textEntry.SetRadialMenuSetting(_radialMenuSettingRightVowel, _radialMenuSettingLeftVowel);
                _currentRadialTypeRight = RadialType.Vowel;
                _currentRadialTypeLeft = RadialType.Vowel;
                return;
            }
            else
            {
                var radialRL = GetInputRadialRL();
                _index = CalculateInputIndex(radialRL, RadialType.Consonant);
                // InputRadialMenu(_index, radialRL, RadialType.Consonant, isSelect: true, disableStick: false);
                SelectRadialMenu(_index);
                return;
            }
        }

        if (_inputState == InputState.VowelReady)
        {
            var radialRL = GetInputRadialRL();
            if (radialRL == RL.None)
            {
                return;
            }
            else
            {
                _index = CalculateInputIndex(radialRL, RadialType.Vowel);
                // InputRadialMenu(_index, radialRL, RadialType.Vowel, isSelect: true, disableStick: false);
                SelectRadialMenu(_index);
                _nextRadialRL = radialRL;
                _inputState = InputState.VowelInputting;
                return;
            }
        }

        if (_inputState == InputState.VowelInputting)
        {
            // TODO: 同上
            if (_isRightStickButtonReleasedThisFrame || _isLeftStickButtonReleasedThisFrame)
            {
                RL radialRL;
                if (_isRightStickButtonReleasedThisFrame)
                {
                    radialRL = RL.Right;
                }
                else
                {
                    radialRL = RL.Left;
                }
                InputRadialMenu(_index, radialRL, RadialType.Vowel, isSelect: false, disableStick: true);
                _inputState = InputState.VowelDone;
                return;
            }
            else if (_isRightStickReleasedThisFrame || _isLeftStickReleasedThisFrame)
            {
                RL radialRL;
                if (_isRightStickReleasedThisFrame)
                {
                    radialRL = RL.Right;
                    _index = CalculateStickInputIndex(_rightStickInputPrev, _radialMenuSettings[(int)RL.Right, (int)RadialType.Vowel]);
                }
                else
                {
                    radialRL = RL.Left;
                    _index = CalculateStickInputIndex(_leftStickInputPrev, _radialMenuSettings[(int)RL.Left, (int)RadialType.Vowel]) + _elementCount[(int)RL.Right];
                }
                InputRadialMenu(_index, radialRL, RadialType.Vowel, isSelect: false, disableStick: false);
                _inputState = InputState.VowelDone;
                return;
            }
            // else if (!_hasInput)
            // {
            //     InputRadialMenu(_index, RL.None, RadialType.Vowel, isSelect: false, disableStick: false);
            //     _nextRadialRL = InverseRL(_nextRadialRL);
            //     _inputState = InputState.VowelDone;
            //     return;
            // }
            else
            {
                var radialRL = GetInputRadialRL();
                // if (radialRL == RL.None)
                // {
                //     return;
                // }
                _index = CalculateInputIndex(radialRL, RadialType.Vowel);
                // InputRadialMenu(_index, radialRL, RadialType.Vowel, isSelect: true, disableStick: false);
                SelectRadialMenu(_index);
                _nextRadialRL = radialRL;
                return;
            }
        }

        if (_inputState == InputState.VowelDone)
        {
            ResetInputState();
            return;
        }
    }

    void UpdateInputStateDouble()
    {
        if (_inputState == InputState.ConsonantReady)
        {
            var radialRL = GetInputRadialRL();
            if (radialRL == RL.None)
            {
                return;
            }
            else
            {
                _index = CalculateInputIndex(radialRL, RadialType.Consonant);
                InputRadialMenu(_index, radialRL, RadialType.Consonant);
                _nextRadialRL = InverseRL(radialRL);
                _inputState = InputState.VowelReady;
                // TODO: refactor
                SetVowelRadialMenuSetting(_nextRadialRL);
                return;
            }
        }

        if (_inputState == InputState.VowelReady)
        {
            var radialRL = GetInputRadialRL();
            if (_isStickReady[(int)InverseRL(_nextRadialRL)])
            {
                if (_nextRadialRL == RL.Right)
                {
                    _index = 0;
                    InputRadialMenu(_index, RL.Right, RadialType.Vowel);
                }
                else
                {
                    _index = _elementCount[(int)RL.Right];
                    InputRadialMenu(_index, RL.Left, RadialType.Vowel);
                }
                _inputState = InputState.VowelDone;
            }
            else if (radialRL == RL.None)
            {
                var consonantIndex = CalculateInputIndex(InverseRL(_nextRadialRL), RadialType.Consonant);
                InputRadialMenu(consonantIndex, InverseRL(_nextRadialRL), RadialType.Consonant);
                return;
            }
            else
            {
                // TODO:
                var consonantIndex = CalculateInputIndex(InverseRL(_nextRadialRL), RadialType.Consonant);
                InputRadialMenu(consonantIndex, InverseRL(_nextRadialRL), RadialType.Consonant);

                _index = CalculateInputIndex(radialRL, RadialType.Vowel);
                InputRadialMenu(_index, radialRL, RadialType.Vowel);
                _inputState = InputState.VowelDone;
                return;
            }
        }

        if (_inputState == InputState.VowelDone)
        {
            if (_hasInput)
            {
                return;
            }
            else
            {
                ResetInputState();
                return;
            }
        }
    }

    void UpdateInputStateDoubleRelease()
    {
        if (_inputState == InputState.ConsonantReady)
        {
            var radialRL = GetInputRadialRL();
            if (radialRL == RL.None)
            {
                return;
            }
            else
            {
                _index = CalculateInputIndex(radialRL, RadialType.Consonant);
                InputRadialMenu(_index, radialRL, RadialType.Consonant);
                _nextRadialRL = InverseRL(radialRL);
                _inputState = InputState.VowelReady;
                // TODO: refactor
                SetVowelRadialMenuSetting(_nextRadialRL);
                return;
            }
        }

        if (_inputState == InputState.VowelReady)
        {
            var radialRL = GetInputRadialRL();
            if (_isRightStickReleasedThisFrame || _isLeftStickReleasedThisFrame)
            {
                if (_nextRadialRL == RL.Right)
                {
                    _index = 0;
                    InputRadialMenu(_index, RL.Right, RadialType.Vowel);
                }
                else
                {
                    _index = _elementCount[(int)RL.Right];
                    InputRadialMenu(_index, RL.Left, RadialType.Vowel);
                }
                _inputState = InputState.VowelDone;
            }
            else
            {
                var consonantIndex = CalculateInputIndex(InverseRL(_nextRadialRL), RadialType.Consonant);
                InputRadialMenu(consonantIndex, InverseRL(_nextRadialRL), RadialType.Consonant);

                if (radialRL == _nextRadialRL)
                {
                    _index = CalculateInputIndex(radialRL, RadialType.Vowel);
                    SelectRadialMenu(_index);
                    _inputState = InputState.VowelInputting;
                }

                return;
            }
        }

        if (_inputState == InputState.VowelInputting)
        {
            if (_isRightStickReleasedThisFrame || _isLeftStickReleasedThisFrame)
            {
                InputRadialMenu(_index, _nextRadialRL, RadialType.Vowel, isSelect: false, disableStick: false);
                _inputState = InputState.VowelDone;
                return;
            }
            else
            {
                var radialRL = GetInputRadialRL();
                _index = CalculateInputIndex(radialRL, RadialType.Vowel);
                SelectRadialMenu(_index);
                return;
            }
        }

        if (_inputState == InputState.VowelDone)
        {
            if (_hasInput)
            {
                return;
            }
            else
            {
                ResetInputState();
                return;
            }
        }
    }

    void SetVowelRadialMenuSetting(RL nextRadialRL)
    {
        Assert.IsTrue(nextRadialRL == RL.Right || nextRadialRL == RL.Left);

        if (_updateLabelBoth)
        {
            _currentRadialTypeRight = RadialType.Vowel;
            _currentRadialTypeLeft = RadialType.Vowel;
            _textEntry.SetRadialMenuSetting(_radialMenuSettingRightVowel, _radialMenuSettingLeftVowel);
        }

        if (nextRadialRL == RL.Right)
        {
            _currentRadialTypeRight = RadialType.Vowel;
            _textEntry.SetRadialMenuSetting(_radialMenuSettingRightVowel, _radialMenuSettingLeftConsonant);
        }

        if (nextRadialRL == RL.Left)
        {
            _currentRadialTypeLeft = RadialType.Vowel;
            _textEntry.SetRadialMenuSetting(_radialMenuSettingRightConsonant, _radialMenuSettingLeftVowel);
        }
    }

    RL InverseRL(RL rl)
    {
        if (rl == RL.Right)
        {
            return RL.Left;
        }
        if (rl == RL.Left)
        {
            return RL.Right;
        }
        return RL.None;
    }

    void SelectRadialMenu(int index)
    {
        _textEntry.SelectRadialMenu(index);
    }

    void InputRadialMenu(int index, RL radialRL, RadialType radialType, bool isSelect = true, bool disableStick = true)
    {
        RL updateRL = InverseRL(radialRL);
        if (_updateLabelBoth)
        {
            updateRL = RL.Both;
        }

        if (radialType == RadialType.Consonant)
        {
            _textEntry.InputConsonant(index, updateRL, isSelect);
        }
        else
        {
            _textEntry.InputVowel(index, isSelect);
        }

        if (disableStick)
        {
            _isStickReady[(int)radialRL] = false;
        }
    }

    RL GetInputRadialRL()
    {
        if (_isRightStickPressed && _isStickReady[(int)RL.Right])
        {
            return RL.Right;
        }
        if (_isLeftStickPressed && _isStickReady[(int)RL.Left])
        {
            return RL.Left;
        }

        var hasRightStickInput = _hasRightStickInput && _isStickReady[(int)RL.Right];
        var hasLeftStickInput = _hasLeftStickInput && _isStickReady[(int)RL.Left];

        if (hasRightStickInput && hasLeftStickInput)
        {
            if (_rightStickInput.magnitude > _leftStickInput.magnitude)
            {
                return RL.Right;
            }
            else
            {
                return RL.Left;
            }
        }

        if (hasRightStickInput)
        {
            return RL.Right;
        }

        if (hasLeftStickInput)
        {
            return RL.Left;
        }

        return RL.None;
    }

    int CalculateInputIndex(RL radialRL, RadialType radialType)
    {
        switch (radialRL)
        {
            case RL.Right:
                if (_isRightStickPressed)
                {
                    return 0;
                }
                return CalculateStickInputIndex(_rightStickInput, _radialMenuSettings[(int)RL.Right, (int)radialType]);
            case RL.Left:
                if (_isLeftStickPressed)
                {
                    return _elementCount[(int)RL.Right];
                }
                return CalculateStickInputIndex(_leftStickInput, _radialMenuSettings[(int)RL.Left, (int)radialType]) + _elementCount[(int)RL.Right];
            default:
                throw new System.ArgumentException("Invalid RadialRL");
        }
    }

    int CalculateStickInputIndex(Vector2 input, RadialMenuSetting radialMenuSetting)
    {
        float globalOffset = radialMenuSetting.globalOffset;
        var angleOffset = radialMenuSetting.angleOffset;

        float rawAngle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        var currentAngle = !radialMenuSetting.inverse ? (-rawAngle + 90 + (angleOffset / 2f)) : -(-rawAngle + 90 + (angleOffset / 2f));
        currentAngle = normalizeAngle(currentAngle - globalOffset);

        var index = (int)(currentAngle / angleOffset);
        // スティックの押し込みが有効な場合は、インデックスを1進める
        if (radialMenuSetting.useStickPress)
        {
            index += 1;
        }

        // Debug.Log("rawX: " + input.x + " rawY: " + input.y + " mag: " + input.magnitude);
        // Debug.Log("currentAngle: " + currentAngle + " index: " + index);
        // Debug.Log("rawAngle: " + rawAngle + " globalOffset: " + globalOffset + " angleOffset: " + angleOffset);
        if (index < 0 || index >= _elementCount[(int)radialMenuSetting.radialRL])
        {
            throw new System.ArgumentException("Invalid index " + index);
        }
        return index;
    }

    private float normalizeAngle(float angle)
    {
        while (angle < 0)
            angle += 360;
        while (angle >= 360)
            angle -= 360;
        return angle;
    }
}
