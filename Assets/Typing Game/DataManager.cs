using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModificationType
{
    Type,
    Change,
    Delete,
    InputConsonant,
    SelectConsonant,
    SelectVowel,
    Cancel,
}

[System.Serializable]
public class ModificationData
{
    public string modificationType;
    public int index;
    public string input;
    public float timeStamp;
}

[System.Serializable]
public class AnswerData
{
    public List<ModificationData> log;
    public string question;
    public string input;
    public float time;
}

public enum InputType
{
    ProposalDouble,
    ProposalSingle,
    JoyFlick,
}

[System.Serializable]
public class Data
{
    public string name;
    public string inputType;
    public List<AnswerData> answerDataList;
    public float time;
}

public class DataManager : MonoBehaviour
{
    Data _data;
    AnswerData _answerData;
    float _startTime;
    float _questionStartTime;


    void Start()
    {

    }

    public void InitializeData(string name, InputType inputType)
    {
        _data = new Data();
        _data.name = name;
        _data.inputType = inputType.ToString();
        _data.answerDataList = new List<AnswerData>();
        _startTime = Time.time;
    }

    public void InitializeAnswerData(string question)
    {
        _answerData = new AnswerData();
        _answerData.question = question;
        _answerData.log = new List<ModificationData>();
        _questionStartTime = Time.time;
    }

    public void AddModificationData(ModificationType modificationType, int index, char input)
    {
        ModificationData modificationData = new ModificationData();
        modificationData.modificationType = modificationType.ToString();
        modificationData.index = index;
        modificationData.input = input.ToString();
        modificationData.timeStamp = Time.time - _questionStartTime;
        _answerData.log.Add(modificationData);
    }

    public void AddAnswerData(string input)
    {
        _answerData.time = Time.time - _questionStartTime;
        _answerData.input = input;
        _data.answerDataList.Add(_answerData);
    }

    public void SaveData()
    {
        _data.time = Time.time - _startTime;
        string json = JsonUtility.ToJson(_data);
        string path = Application.dataPath + "/Resources/Results/" + _data.name + _data.inputType.ToString() + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log("Data Saved to " + path);
    }

    public float GetCharacterPerMinute()
    {
        float characterPerMinute = 0;
        foreach (AnswerData answerData in _data.answerDataList)
        {
            characterPerMinute += answerData.input.Length / answerData.time * 60;
        }
        characterPerMinute /= _data.answerDataList.Count;
        return characterPerMinute;
    }

    public float GetCurrentCharacterPerMinute()
    {
        float characterPerMinute = 0;
        characterPerMinute = _answerData.input.Length / _answerData.time * 60;
        return characterPerMinute;
    }
}
