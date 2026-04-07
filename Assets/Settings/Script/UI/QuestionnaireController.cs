using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityTV.Core;
using UnityTV.Player;

namespace UnityTV.UI
{
    /// <summary>
    /// Controls the character creation questionnaire with Ista AI
    /// Handles all 5 questions and initializes player data
    /// </summary>
    public class QuestionnaireController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TextMeshProUGUI istaDialogueText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button[] optionButtons; // Up to 5 options per question
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private Button nameSubmitButton;
        [SerializeField] private GameObject loadingPanel;

        [Header("Character Display")]
        [SerializeField] private Image istaCharacterImage;
        [SerializeField] private Sprite istaDefaultSprite;
        [SerializeField] private Sprite istaHappySprite;
        [SerializeField] private Sprite istaThinkingSprite;

        [Header("Animation")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private float delayBetweenQuestions = 2f;

        // Question tracking
        private int currentQuestionIndex = 0;
        private string playerName = "";

        // Player choices
        private CareerType selectedCareer;
        private HousingType selectedHousing;
        private FearType selectedFear;
        private bool hasPartner;

        // Questions data
        private List<Question> questions;

        private void Start()
        {
            InitializeQuestions();
            SetupUI();
            StartQuestionnaire();
        }

        private void SetupUI()
        {
            // Hide all panels initially
            nameInputPanel.SetActive(false);
            optionsPanel.SetActive(false);
            loadingPanel.SetActive(false);

            // Setup name submit button
            nameSubmitButton.onClick.AddListener(OnNameSubmitted);

            // Setup option buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i; // Capture for lambda
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            }
        }

        private void InitializeQuestions()
        {
            questions = new List<Question>
            {
                // Question 1: Career Choice
                new Question
                {
                    istaIntro = "您好{0}!我是您的首席AI助理伊丝塔!感谢您选择归一电视!\n\n下面,请回答五个问题,以便让我能够给您提供更优质的个性化服务!",
                    questionText = "1、请问您是出于什么理由选择的归一电视?",
                    options = new string[]
                    {
                        "A.倒霉的毕业生",
                        "B.转业的中年人【未解锁】",
                        "C.追梦的创业人【未解锁】",
                        "D.普通的毕业生【未解锁】",
                        "E.???【未解锁】"
                    },
                    optionDescriptions = new string[]
                    {
                        "大学毕业后被职业培训诈骗到几乎身无分文,无法在10天之内找到工作就会被驱逐",
                        "未解锁",
                        "未解锁",
                        "未解锁",
                        "未解锁"
                    },
                    enabledOptions = new bool[] { true, false, false, false, false },
                    response = "嗯...看来您现在情况不太妙呢?不过没关系!归一电视就是为了帮您打破现状而诞生的产品!\n\n那么,请继续回答第二个问题吧!"
                },

                // Question 2: Career Goal
                new Question
                {
                    questionText = "2、请问您理想的工作是?",
                    options = new string[]
                    {
                        "A.医生",
                        "B.警察",
                        "C.公司职员",
                        "D.行商",
                        "E.科学家【第一周目无法解锁】"
                    },
                    optionDescriptions = new string[]
                    {
                        "俗话说得好,什么时代都少不了医生,就连现在这个时代也一样!",
                        "一份需要坚定信仰的光辉职业!没有一腔热血和强健的体魄可是当不了警察的哦!",
                        "平平淡淡才是真,但现在就业形势不景气,想要平平淡淡也得拼尽全力呢",
                        "当今这个时代催生出的特别行业,无论是城内还是城外,如果没有行商,人们就没有办法像现在这样生活了",
                        "第一周目无法解锁"
                    },
                    enabledOptions = new bool[] { true, true, true, true, false },
                    response = "收到收到!真没想到您会选择这份职业作为理想的工作呢!"
                },

                // Question 3: Housing Preference
                new Question
                {
                    questionText = "3、那么接下来,衣食住行衣食住行,对您而言,理想的住宅是什么样的呢?",
                    options = new string[]
                    {
                        "A.能够安心工作的恬静之家",
                        "B.能够自由锻炼的刚猛之家",
                        "C.能够放松自我的宅宅之家",
                        "D.能够狂野烹饪的美食之家"
                    },
                    optionDescriptions = new string[] { "", "", "", "" },
                    enabledOptions = new bool[] { true, true, true, true },
                    responses = new string[]
                    {
                        "原来您是一位喜欢安静的人!了解了解...我懂的,对成年人来说,安静的独处是最宝贵的时光嘛~",
                        "原来您是一位喜欢健身的人!了解了解...我懂的,有什么比洗澡的时候看到镜子里的腹肌更有成就感的事情呢~",
                        "原来您是一位宅宅!了解了解...我懂的,放假在家就是比去外面人挤人舒服嘛!好!那么下一个问题!",
                        "原来您是一位美食家!了解了解...我懂的,没什么比做一桌好菜更能取悦自己的了!"
                    }
                },

                // Question 4: Partner Status
                new Question
                {
                    questionText = "4、您有理想中的对象吗?",
                    options = new string[]
                    {
                        "A.有",
                        "B.没有"
                    },
                    optionDescriptions = new string[] { "", "" },
                    enabledOptions = new bool[] { true, true },
                    responses = new string[]
                    {
                        "嘿嘿...别那么看着我嘛,协议也要求我不能再问用户更深入的问题了",
                        "也是啦~对刚毕业的大学生来说,现在就谈对象什么的也太早了!"
                    },
                    followUp = "好!那么最后一个问题"
                },

                // Question 5: Fear Type
                new Question
                {
                    questionText = "5、您最深的恐惧是什么?",
                    options = new string[]
                    {
                        "A.注视",
                        "B.虫类",
                        "C.黑暗",
                        "D.死亡"
                    },
                    optionDescriptions = new string[] { "", "", "", "" },
                    enabledOptions = new bool[] { true, true, true, true },
                    response = "感谢您的回答!现在归一电视将进入启动前的初始化进程!\n\n您可以稍事休息,伊丝塔会很快帮你安排好一切的!",
                    isAtmosphereChange = true // Darker atmosphere for fear question
                }
            };
        }

        private void StartQuestionnaire()
        {
            // Show name input first
            ShowNameInput();
        }

        private void ShowNameInput()
        {
            nameInputPanel.SetActive(true);
            playerNameInput.Select();
        }

        private void OnNameSubmitted()
        {
            playerName = playerNameInput.text.Trim();

            if (string.IsNullOrEmpty(playerName))
            {
                // Show error
                Debug.LogWarning("Player name cannot be empty!");
                return;
            }

            nameInputPanel.SetActive(false);
            StartCoroutine(ShowQuestionSequence());
        }

        private System.Collections.IEnumerator ShowQuestionSequence()
        {
            for (int i = 0; i < questions.Count; i++)
            {
                currentQuestionIndex = i;
                yield return StartCoroutine(DisplayQuestion(questions[i]));
            }

            // All questions answered - complete character creation
            CompleteQuestionnaire();
        }

        private System.Collections.IEnumerator DisplayQuestion(Question question)
        {
            // Show Ista intro if this is the first question
            if (currentQuestionIndex == 0 && !string.IsNullOrEmpty(question.istaIntro))
            {
                string intro = string.Format(question.istaIntro, playerName);
                yield return StartCoroutine(TypeText(istaDialogueText, intro));
                yield return new WaitForSeconds(1f);
            }

            // Change atmosphere for fear question
            if (question.isAtmosphereChange)
            {
                // TODO: Darken screen, change music
                istaCharacterImage.sprite = istaThinkingSprite;
            }

            // Display question
            questionText.text = question.questionText;

            // Show options
            ShowOptions(question);

            // Wait for player to select an option
            yield return new WaitUntil(() => question.answerSelected);

            // Hide options
            HideOptions();

            // Show response
            string response = question.responses != null && question.responses.Length > question.selectedOptionIndex
                ? question.responses[question.selectedOptionIndex]
                : question.response;

            istaCharacterImage.sprite = istaHappySprite;
            yield return StartCoroutine(TypeText(istaDialogueText, response));

            // Show follow-up if exists
            if (!string.IsNullOrEmpty(question.followUp))
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(TypeText(istaDialogueText, question.followUp));
            }

            yield return new WaitForSeconds(delayBetweenQuestions);
        }

        private void ShowOptions(Question question)
        {
            optionsPanel.SetActive(true);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < question.options.Length)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].interactable = question.enabledOptions[i];

                    TextMeshProUGUI buttonText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    buttonText.text = question.options[i];

                    // Add description if exists
                    if (!string.IsNullOrEmpty(question.optionDescriptions[i]))
                    {
                        buttonText.text += $"\n<size=70%><i>{question.optionDescriptions[i]}</i></size>";
                    }
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void HideOptions()
        {
            optionsPanel.SetActive(false);
        }

        private void OnOptionSelected(int optionIndex)
        {
            Question currentQuestion = questions[currentQuestionIndex];
            currentQuestion.selectedOptionIndex = optionIndex;
            currentQuestion.answerSelected = true;

            // Store the selection
            StoreAnswer(currentQuestionIndex, optionIndex);

            Debug.Log($"Question {currentQuestionIndex + 1}: Selected option {optionIndex}");
        }

        private void StoreAnswer(int questionIndex, int optionIndex)
        {
            switch (questionIndex)
            {
                case 0: // Background (always A for now)
                    // Unlucky graduate
                    break;

                case 1: // Career
                    selectedCareer = (CareerType)optionIndex;
                    break;

                case 2: // Housing
                    selectedHousing = (HousingType)optionIndex;
                    break;

                case 3: // Partner
                    hasPartner = (optionIndex == 0);
                    break;

                case 4: // Fear
                    selectedFear = (FearType)optionIndex;
                    break;
            }
        }

        private void CompleteQuestionnaire()
        {
            // Show loading screen
            loadingPanel.SetActive(true);

            // Create player data
            PlayerData playerData = new PlayerData();
            playerData.InitializeFromQuestionnaire(
                playerName,
                selectedCareer,
                selectedHousing,
                selectedFear,
                hasPartner
            );

            // Complete character creation through GameManager
            StartCoroutine(FinalizeCreation(playerData));
        }

        private System.Collections.IEnumerator FinalizeCreation(PlayerData playerData)
        {
            // Simulate initialization time
            yield return new WaitForSeconds(2f);

            // Send to GameManager
            GameManager.Instance.CompleteCharacterCreation(playerData);
        }

        private System.Collections.IEnumerator TypeText(TextMeshProUGUI textComponent, string fullText)
        {
            textComponent.text = "";

            foreach (char c in fullText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
        }
    }

    /// <summary>
    /// Question data structure
    /// </summary>
    [System.Serializable]
    public class Question
    {
        public string istaIntro; // Ista's introduction for this question
        public string questionText;
        public string[] options;
        public string[] optionDescriptions;
        public bool[] enabledOptions;
        public string response; // Default response
        public string[] responses; // Multiple responses based on choice
        public string followUp; // Follow-up text after response
        public bool isAtmosphereChange; // For question 5 (fear)

        [System.NonSerialized]
        public bool answerSelected = false;
        [System.NonSerialized]
        public int selectedOptionIndex = -1;
    }
}