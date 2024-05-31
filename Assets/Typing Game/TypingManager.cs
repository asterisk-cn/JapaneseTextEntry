using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System.Linq;

[System.Serializable]
public class QuestionData
{
    public int rank;
    public string IForm;
    public string lemma;
}

[System.Serializable]
public class QuestionList
{
    public List<QuestionData> phrases;
}

public enum QuestionType
{
    PhraseList,
    CharacterList,
}

public class TypingManager : MonoBehaviour
{
    [Header("Data Settings")]
    [SerializeField] private string _playerName;
    [SerializeField] private InputType _inputType = InputType.ProposalDouble;
    [SerializeField] private int _questionCount = 20;
    [SerializeField] private QuestionType _questionType = QuestionType.PhraseList;

    [SerializeField] private TextMeshProUGUI _questionTextDisplay;
    [SerializeField] private TextMeshProUGUI _inputTextDisplay;

    [SerializeField] private TextEntryInputs _textEntryInputs;
    [SerializeField] private GameObject _questionCountDisplay;
    [SerializeField] private TextMeshProUGUI _totalQuestionCountDisplay;
    [SerializeField] private TextMeshProUGUI _currentQuestionCountDisplay;
    [SerializeField] private TextMeshProUGUI _currentCharacterPerMinuteDisplay;
    [SerializeField] private TextMeshProUGUI _totalCharacterPerMinuteDisplay;
    [SerializeField] private TextMeshProUGUI _recentCharacterPerMinuteDisplay;
    [SerializeField] private TextMeshProUGUI _bestCharacterPerMinuteDisplay;

    private DataManager _dataManager;
    private PhraseMaker _phraseMaker;

    private string _question;
    private QuestionList _questionList;
    private string _inputText;
    public bool IsMeasuring = false;
    private int _currentQuestionIndex = 0;

    private uint _wrongColor = 0xFF0000FF;
    public bool HasWrongInput { get; private set; }

    private List<float> _characterPerMinuteList = new List<float>();
    private float _previousTime = 0;

    private void Awake()
    {
        _dataManager = this.GetComponent<DataManager>();
        _phraseMaker = this.GetComponent<PhraseMaker>();
    }

    void Start()
    {
        ClearText();
        LoadQuestions();

        _previousTime = Time.time;
    }

    void Update()
    {
        bool isCorrect = true;
        _inputTextDisplay.text = "";
        for (int i = 0; i < _inputText.Length; i++)
        {
            if (i >= _question.Length)
            {
                _inputTextDisplay.text += "<color=#" + _wrongColor.ToString("X") + ">" + _inputText[i] + "</color>";
                isCorrect = false;
            }
            else if (_inputText[i] == _question[i])
            {
                _inputTextDisplay.text += _inputText[i];
            }
            else
            {
                _inputTextDisplay.text += "<color=#" + _wrongColor.ToString("X") + ">" + _inputText[i] + "</color>";
                isCorrect = false;
            }
        }

        HasWrongInput = !isCorrect;

        if (IsMeasuring) Send();
    }

    void LoadQuestions(int questionCount = -1)
    {
        var questionList = JsonUtility.FromJson<QuestionList>(Resources.Load<TextAsset>("Json/PhraseDictionary").ToString());

        if (questionCount == -1)
        {
            _questionList = questionList;
        }
        else
        {
            _questionList.phrases = new List<QuestionData>();
            for (int i = 0; i < questionCount; i++)
            {
                // int questionIndex = Random.Range(0, questionList.phrases.Count);
                int questionIndex = i;
                _questionList.phrases.Add(questionList.phrases[questionIndex]);
                questionList.phrases.RemoveAt(questionIndex);
            }
        }
    }

    public void TypeCharacter(char input, bool isChange = false)
    {
        if (IsMeasuring && !isChange)
        {
            _dataManager.AddModificationData(ModificationType.Type, _inputText.Length, input);
        }

        _inputText += input;
        return;
    }

    public void ChangeCharacter(char input)
    {
        if (IsMeasuring)
        {
            _dataManager.AddModificationData(ModificationType.Change, _inputText.Length - 1, input);
        }

        DeleteCharacter(true);
        TypeCharacter(input, true);
        return;
    }

    public void SelectConsonant(string consonant)
    {
        if (IsMeasuring)
        {
            _dataManager.AddModificationData(ModificationType.SelectConsonant, _inputText.Length, consonant[0]);
        }
        return;
    }

    public void SelectVowel(string vowel)
    {
        if (IsMeasuring)
        {
            _dataManager.AddModificationData(ModificationType.SelectVowel, _inputText.Length, vowel[0]);
        }
        return;
    }

    public void InputConsonant(string consonant)
    {
        if (IsMeasuring)
        {
            _dataManager.AddModificationData(ModificationType.InputConsonant, _inputText.Length, consonant[0]);
        }
        return;
    }

    public void Cancel()
    {
        if (IsMeasuring)
        {
            _dataManager.AddModificationData(ModificationType.Cancel, _inputText.Length, ' ');
        }
        return;
    }

    public void DeleteCharacter(bool isChange = false)
    {
        if (_inputText.Length == 0)
        {
            return;
        }

        if (IsMeasuring && !isChange)
        {
            _dataManager.AddModificationData(ModificationType.Delete, _inputText.Length - 1, _inputText[_inputText.Length - 1]);
        }

        _inputText = _inputText.Substring(0, _inputText.Length - 1);
        return;
    }

    public void ClearText()
    {
        _inputText = "";
    }

    public void InitializeQuestion()
    {
        int questionIndex;
        if (IsMeasuring)
        {
            questionIndex = _currentQuestionIndex;
        }
        else
        {
            // questionIndex = Random.Range(0, _questionList.phrases.Count);
            questionIndex = _currentQuestionIndex;
        }

        _question = _questionList.phrases[questionIndex].IForm;
        _questionTextDisplay.text = _question;
        ClearText();
        _previousTime = Time.time;

        if (IsMeasuring)
        {
            _dataManager.InitializeAnswerData(_question);
        }
    }

    public void Send()
    {
        if (IsMeasuring)
        {
            if (_inputText != _question) return;

            _dataManager.AddAnswerData(_inputText);
            _currentQuestionIndex++;
            _currentQuestionCountDisplay.text = (_currentQuestionIndex + 1).ToString();
            _currentCharacterPerMinuteDisplay.text = _dataManager.GetCurrentCharacterPerMinute().ToString("F2");
            _totalCharacterPerMinuteDisplay.text = _dataManager.GetCharacterPerMinute().ToString("F2");

            if (_currentQuestionIndex >= _questionList.phrases.Count)
            {
                Debug.Log("Measuring Finished");
                _questionCountDisplay.SetActive(false);
                _dataManager.SaveData();
                IsMeasuring = false;
                LoadQuestions();
            }
        }
        else
        {
            _currentQuestionIndex++;
        }
        UpdateCharacterPerMinute();

        InitializeQuestion();
    }

    public string GetCurrentCharacter()
    {
        if (_inputText.Length == 0)
        {
            return "";
        }

        return _inputText[_inputText.Length - 1].ToString();
    }

    public void StartMeasuring()
    {
        if (IsMeasuring)
        {
            return;
        }

        if (_questionType == QuestionType.CharacterList)
        {
            _questionList = _phraseMaker.CreatePhrases();
        }
        else if (_questionType == QuestionType.PhraseList)
        {
            LoadQuestions(_questionCount);
        }
        _dataManager.InitializeData(_playerName, _inputType);
        IsMeasuring = true;
        _currentQuestionIndex = 0;
        _questionCountDisplay.SetActive(true);
        _totalQuestionCountDisplay.text = _questionList.phrases.Count.ToString();
        _currentQuestionCountDisplay.text = "1";
        ResetCharacterPerMinute();

        InitializeQuestion();
        Debug.Log("Measuring Started");
        _textEntryInputs.ResetInputState();
    }

    float GetCharacterPerMinute()
    {
        if (IsMeasuring)
        {
            return _dataManager.GetCurrentCharacterPerMinute();
        }
        else
        {
            float time = Time.time - _previousTime;
            return _inputText.Length / time * 60;
        }
    }

    void UpdateCharacterPerMinute()
    {
        float characterPerMinute = GetCharacterPerMinute();
        _characterPerMinuteList.Add(characterPerMinute);

        _currentCharacterPerMinuteDisplay.text = characterPerMinute.ToString("F1");

        if (_characterPerMinuteList.Count == 0)
        {
            return;
        }

        _bestCharacterPerMinuteDisplay.text = _characterPerMinuteList.Max().ToString("F1");
        _recentCharacterPerMinuteDisplay.text = _characterPerMinuteList.Skip(Mathf.Max(0, _characterPerMinuteList.Count - 10)).Average().ToString("F1");
        _totalCharacterPerMinuteDisplay.text = _characterPerMinuteList.Average().ToString("F1");
    }

    void ResetCharacterPerMinute()
    {
        _previousTime = Time.time;
        _characterPerMinuteList.Clear();

        _currentCharacterPerMinuteDisplay.text = "0";
        _totalCharacterPerMinuteDisplay.text = "0";
        _recentCharacterPerMinuteDisplay.text = "0";
        _bestCharacterPerMinuteDisplay.text = "0";
    }
}
