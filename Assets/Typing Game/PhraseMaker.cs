using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string character;
    public string type;
}

[System.Serializable]
public class CharacterList
{
    public List<CharacterData> characters;
}

public class PhraseMaker : MonoBehaviour
{
    [SerializeField] private int _setCount = 3;
    [SerializeField] private int _phraseLength = 9;
    [SerializeField] private int _phraseCount = 9;

    private CharacterList _characterList;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadCharacters()
    {
        _characterList = JsonUtility.FromJson<CharacterList>(Resources.Load<TextAsset>("Json/CharacterDictionary").ToString());
    }

    public QuestionList CreatePhrases()
    {
        LoadCharacters();
        if (_phraseLength * _phraseCount != _characterList.characters.Count)
        {
            Debug.Log("Error: Phrase length * phrase count does not equal character count");
            return null;
        }

        QuestionList questionList = new QuestionList();
        questionList.phrases = new List<QuestionData>();
        for (int i = 0; i < _setCount; i++)
        {
            LoadCharacters();
            for (int j = 0; j < _phraseCount; j++)
            {
                string phrase = "";
                for (int k = 0; k < _phraseLength; k++)
                {
                    int randomIndex = Random.Range(0, _characterList.characters.Count);
                    phrase += _characterList.characters[randomIndex].character;
                    _characterList.characters.RemoveAt(randomIndex);
                }

                QuestionData questionData = new QuestionData();
                questionData.rank = j;
                questionData.IForm = phrase;
                questionData.lemma = phrase;
                questionList.phrases.Add(questionData);
            }
        }

        return questionList;
    }
}
