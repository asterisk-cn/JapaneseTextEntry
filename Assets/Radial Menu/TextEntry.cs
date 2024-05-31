using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextEntry : MonoBehaviour
{
    public RadialMenu radialMenuRight;
    public RadialMenu radialMenuLeft;

    [SerializeField]
    private TypingManager _typingManager;


    string[] _consonant;
    string[][] _character;
    string[][][] _characterList;

    int consonantIndex;
    int vowelIndex;

    private TextEntryInputs _textEntryInputs;

    public RadialMenuSetting _radialMenuSettingRight;
    public RadialMenuSetting _radialMenuSettingLeft;

    string _currentCharacter;
    int[] _currentCharacterIndex = new int[3];

    // TODO: 子音と母音の入力処理を分ける

    void Awake()
    {
        _consonant = new string[] { "あ", "か", "さ", "た", "な", "は", "ま", "や", "ら", "わ" };

        _characterList = new string[][][] {
            new string[][] {
                new string[] { "あ", "い", "う", "え", "お" },
                new string[] { "か", "き", "く", "け", "こ" },
                new string[] { "さ", "し", "す", "せ", "そ" },
                new string[] { "た", "ち", "つ", "て", "と" },
                new string[] { "な", "に", "ぬ", "ね", "の" },
                new string[] { "は", "ひ", "ふ", "へ", "ほ" },
                new string[] { "ま", "み", "む", "め", "も" },
                new string[] { "や", "", "ゆ", "", "よ" },
                new string[] { "ら", "り", "る", "れ", "ろ" },
                new string[] { "わ", "を", "ん", "ー", "" }
            },
            new string[][] {
                new string[] { "", "", "", "", "" },
                new string[] { "が", "ぎ", "ぐ", "げ", "ご" },
                new string[] { "ざ", "じ", "ず", "ぜ", "ぞ" },
                new string[] { "だ", "ぢ", "づ", "で", "ど" },
                new string[] { "", "", "", "", "" },
                new string[] { "ば", "び", "ぶ", "べ", "ぼ" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" }
            },
            new string[][] {
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "ぱ", "ぴ", "ぷ", "ぺ", "ぽ" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
            },
            new string[][] {
                new string[] { "ぁ", "ぃ", "ぅ", "ぇ", "ぉ" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "っ", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" },
                new string[] {"ゃ", "", "ゅ", "", "ょ" },
                new string[] { "", "", "", "", "" },
                new string[] { "", "", "", "", "" }
            }
        };
    }

    int[] GetCharacterIndex(string character)
    {
        if (character == "")
        {
            return new int[] { -1, -1, -1 };
        }

        for (int i = 0; i < _characterList.Length; i++)
        {
            for (int j = 0; j < _characterList[i].Length; j++)
            {
                for (int k = 0; k < _characterList[i][j].Length; k++)
                {
                    if (_characterList[i][j][k] == character)
                    {
                        return new int[] { i, j, k };
                    }
                }
            }
        }

        return new int[] { -1, -1, -1 };
    }

    public void SetRadialMenuSetting(RadialMenuSetting radialMenuSettingRight, RadialMenuSetting radialMenuSettingLeft)
    {
        _radialMenuSettingRight = radialMenuSettingRight;
        _radialMenuSettingLeft = radialMenuSettingLeft;

        radialMenuRight.globalOffset = _radialMenuSettingRight.globalOffset;
        radialMenuRight.useStickPress = _radialMenuSettingRight.useStickPress;
        radialMenuRight.inverse = _radialMenuSettingRight.inverse;

        radialMenuLeft.globalOffset = _radialMenuSettingLeft.globalOffset;
        radialMenuLeft.useStickPress = _radialMenuSettingLeft.useStickPress;
        radialMenuLeft.inverse = _radialMenuSettingLeft.inverse;

        radialMenuRight.UpdateRadialMenu();
        radialMenuLeft.UpdateRadialMenu();
    }

    void Start()
    {
        clearText();
        ResetTextEntry();
    }

    public void InputConsonant(int index, RL updateRL = RL.Both, bool isSelect = true)
    {
        if (consonantIndex == index)
        {
            return;
        }


        if (isSelect)
        {
            SelectRadialMenu(index);
        }
        else
        {
            ClearSelect();
        }

        consonantIndex = index;
        if (updateRL == RL.Right)
        {
            SetLabels(_characterList[0][consonantIndex], RL.Right);
        }
        else if (updateRL == RL.Left)
        {
            SetLabels(_characterList[0][consonantIndex], RL.Left);
        }
        else
        {
            SetLabels(_characterList[0][consonantIndex], RL.Right);
            SetLabels(_characterList[0][consonantIndex], RL.Left);
        }

        _typingManager.InputConsonant(_consonant[consonantIndex]);
        CheckTextEntry();
    }

    public void InputVowel(int index, bool isSelect = true)
    {

        if (isSelect)
        {
            SelectRadialMenu(index);
        }
        else
        {
            ClearSelect();
        }

        vowelIndex = index;

        CheckTextEntry();
    }

    void CheckTextEntry()
    {
        if (consonantIndex == -1 || vowelIndex == -1)
        {
            return;
        }

        textEntry();
        _currentCharacterIndex = new int[] { 0, consonantIndex, vowelIndex };
    }

    // public void OnTextEntry(int index)
    // {
    //     selectRadialMenu(index);

    //     if (consonantIndex == -1)
    //     {
    //         consonantIndex = index;
    //         setLabels(_character[consonantIndex]);
    //     }
    //     else if (vowelIndex == -1)
    //     {
    //         vowelIndex = index;
    //         textEntry();
    //         _currentCharacterIndex = new int[] { 0, consonantIndex, vowelIndex };
    //         resetTextEntry();
    //     }
    //     else
    //     {
    //         _currentCharacterIndex = new int[] { -1, -1, -1 };
    //         resetTextEntry();
    //     }
    // }

    public void ResetTextEntry()
    {
        consonantIndex = -1;
        vowelIndex = -1;
        SetBothLabels(_consonant);
        ClearSelect();
    }

    public void SelectRadialMenu(int index)
    {
        if (index < 5)
        {
            radialMenuRight.selectButton(index);
        }
        else
        {
            radialMenuLeft.selectButton(index - 5);
        }
    }

    void ClearSelect()
    {
        radialMenuRight.ClearSelect();
        radialMenuLeft.ClearSelect();
    }

    void SetLabels(string[] labels, RL rl)
    {
        if (rl == RL.Right)
        {
            radialMenuRight.setLabels(labels);
        }
        else
        {
            radialMenuLeft.setLabels(labels);
        }
    }

    void SetBothLabels(string[] labels)
    {
        radialMenuRight.setLabels(labels[0..5]);
        radialMenuLeft.setLabels(labels[5..10]);
    }

    void textEntry()
    {
        if (_characterList[0][consonantIndex][vowelIndex % 5] == "")
        {
            return;
        }
        _typingManager.TypeCharacter(_characterList[0][consonantIndex][vowelIndex % 5][0]);
    }

    string GetCharacter(int[] index)
    {
        if (index.Length != 3)
        {
            // TODO: Error
            return "";
        }

        index[2] %= 5;

        return _characterList[index[0]][index[1]][index[2]];
    }

    public void ChangeCharacterCase()
    {
        _currentCharacter = _typingManager.GetCurrentCharacter();
        if (_currentCharacter == "")
        {
            return;
        }

        var index = GetCharacterIndex(_currentCharacter);

        for (int i = 0; i < 3; i++)
        {
            index[0] += 1;
            index[0] %= 4;
            var nextCharacter = GetCharacter(index);
            if (nextCharacter != "")
            {
                _typingManager.ChangeCharacter(nextCharacter[0]);
                return;
            }
        }
    }

    public void DeleteCharacter()
    {
        _typingManager.DeleteCharacter();
    }

    public void Send()
    {
        _typingManager.Send();
    }

    public void clearText()
    {
        _currentCharacterIndex = new int[] { -1, -1, -1 };
        _typingManager.InitializeQuestion();
    }
}
