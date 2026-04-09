using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityTV.Core;
using UnityTV.Player;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// 访客对话场景控制器
    /// 处理NPC对话，特别是Anchilo的首次访问
    /// </summary>
    public class DoorTalkingController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image npcPortrait; // NPC立绘
        [SerializeField] private TextMeshProUGUI npcNameText; // NPC名字
        [SerializeField] private TextMeshProUGUI dialogueText; // 对话文本
        [SerializeField] private GameObject dialoguePanel; // 对话面板
        [SerializeField] private Button nextButton; // 下一句按钮
        [SerializeField] private Button[] choiceButtons; // 选项按钮（如果有分支对话）

        [Header("Character Sprites")]
        [SerializeField] private Sprite anchiloSprite;
        [SerializeField] private Sprite anchiloHappySprite;
        [SerializeField] private Sprite anchiloSeriousSprite;

        [Header("Settings")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 2f;

        // Dialogue state
        private Queue<DialogueLine> dialogueQueue;
        private bool isTyping = false;
        private bool dialogueComplete = false;

        private void Start()
        {
            Debug.Log("[DoorTalking] Scene started");
            InitializeUI();
            StartDialogue();
        }

        private void InitializeUI()
        {
            // Hide choice buttons initially
            if (choiceButtons != null)
            {
                foreach (Button btn in choiceButtons)
                {
                    if (btn) btn.gameObject.SetActive(false);
                }
            }

            // Setup next button
            if (nextButton)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(DisplayNextLine);
                Debug.Log("[DoorTalking] Next button configured");
            }
            else
            {
                Debug.LogError("[DoorTalking] Next button not assigned!");
            }

            dialogueQueue = new Queue<DialogueLine>();
            Debug.Log("[DoorTalking] UI initialized");
        }

        private void StartDialogue()
        {
            Debug.Log("[DoorTalking] Starting dialogue...");

            // Check which NPC is visiting
            // For first playthrough, it's always Anchilo
            if (GameManager.Instance?.PlayerData != null &&
                !GameManager.Instance.PlayerData.AnchiloVisited)
            {
                Debug.Log("[DoorTalking] Starting Anchilo dialogue (first visit)");
                StartAnchiloDialogue();
            }
            else
            {
                Debug.Log("[DoorTalking] Starting default dialogue");
                StartDefaultDialogue();
            }

            Debug.Log($"[DoorTalking] Dialogue queue has {dialogueQueue.Count} lines");
        }

        private void StartAnchiloDialogue()
        {
            // Anchilo's introduction dialogue
            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "???",
                text = "*咚咚咚* （敲门声）",
                sprite = null
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "???",
                text = "你好！请问有人在家吗？",
                sprite = anchiloSprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = $"啊！你好，{GameManager.Instance.PlayerData.PlayerName}！我是安奇洛，住在隔壁的邻居。",
                sprite = anchiloHappySprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "听说你最近搬进来了，所以特地来打个招呼！",
                sprite = anchiloSprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "咦？你买了归一电视？那可是个好东西！",
                sprite = anchiloHappySprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "不过...你知道怎么用吗？归一电视可不是普通的电视哦。",
                sprite = anchiloSeriousSprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "让我来教你吧！归一电视有6个频道，每个频道都能帮你提升不同的能力。",
                sprite = anchiloSprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "不过要小心第VI频道...那里面藏着一些...奇怪的东西。",
                sprite = anchiloSeriousSprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "但是！如果你能克服恐惧，就能得到巨大的成长！",
                sprite = anchiloHappySprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "好了，规则我都告诉你了。记住，你只有10天时间找到工作，加油吧！",
                sprite = anchiloSprite
            });

            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "安奇洛",
                text = "如果遇到什么问题，随时可以来找我！我就住在隔壁~",
                sprite = anchiloHappySprite,
                isLastLine = true
            });

            DisplayNextLine();
        }

        private void StartDefaultDialogue()
        {
            dialogueQueue.Enqueue(new DialogueLine
            {
                speaker = "系统",
                text = "暂时没有访客。",
                isLastLine = true
            });

            DisplayNextLine();
        }

        private void DisplayNextLine()
        {
            // If currently typing, skip to end of current line
            if (isTyping)
            {
                StopAllCoroutines();

                // Check if there are still lines in the queue
                if (dialogueQueue.Count > 0)
                {
                    dialogueText.text = dialogueQueue.Peek().text;
                }

                isTyping = false;
                return;
            }

            // Check if queue is empty
            if (dialogueQueue.Count == 0)
            {
                Debug.Log("[DoorTalking] No more dialogue lines");
                EndDialogue();
                return;
            }

            DialogueLine line = dialogueQueue.Dequeue();

            // Update speaker name
            if (npcNameText)
            {
                npcNameText.text = line.speaker;
            }

            // Update portrait
            if (npcPortrait && line.sprite != null)
            {
                npcPortrait.sprite = line.sprite;
                npcPortrait.gameObject.SetActive(true);
            }
            else if (npcPortrait)
            {
                npcPortrait.gameObject.SetActive(false);
            }

            // Display dialogue text
            StartCoroutine(TypeText(line.text));

            // Check if this is the last line
            if (line.isLastLine)
            {
                dialogueComplete = true;
            }
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";

            // Disable next button while typing
            if (nextButton)
            {
                nextButton.interactable = false;
            }

            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }

            isTyping = false;

            // Re-enable next button
            if (nextButton)
            {
                nextButton.interactable = true;
            }

            // Auto advance if enabled
            if (autoAdvance && dialogueQueue.Count > 0)
            {
                yield return new WaitForSeconds(autoAdvanceDelay);
                DisplayNextLine();
            }
        }

        private void EndDialogue()
        {
            Debug.Log("Dialogue ended");

            // Mark Anchilo as visited and unlock full systems
            if (GameManager.Instance?.PlayerData != null &&
                !GameManager.Instance.PlayerData.AnchiloVisited)
            {
                GameManager.Instance.PlayerData.UnlockFullSystems();
                Debug.Log("Full systems unlocked!");
            }

            // Return to living room after a short delay
            Invoke(nameof(ReturnToLivingRoom), 1f);
        }

        private void ReturnToLivingRoom()
        {
            Debug.Log("[DoorTalking] Returning to Living Room");
            SceneController.LoadScene("02_LivingRoom");
        }

        // Optional: Allow clicking anywhere to advance
        private float lastClickTime = 0f;
        private const float clickCooldown = 0.3f; // Prevent spam clicking

        private void Update()
        {
            // Mouse click to advance (with cooldown)
            if (Input.GetMouseButtonDown(0) && Time.time > lastClickTime + clickCooldown)
            {
                if (!isTyping && dialogueQueue.Count > 0)
                {
                    lastClickTime = Time.time;
                    DisplayNextLine();
                }
            }

            // ESC to skip dialogue (for testing)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[DoorTalking] ESC pressed - skipping dialogue");
                EndDialogue();
            }

            // Space bar to advance (alternative to clicking)
            if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastClickTime + clickCooldown)
            {
                if (!isTyping && dialogueQueue.Count > 0)
                {
                    lastClickTime = Time.time;
                    DisplayNextLine();
                }
            }
        }
    }

    /// <summary>
    /// 对话行数据结构
    /// </summary>
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker; // 说话者名字
        public string text; // 对话内容
        public Sprite sprite; // 立绘精灵
        public bool isLastLine; // 是否是最后一句
    }
}