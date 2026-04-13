using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VampireSurvival.Core;

namespace VampireSurvival.Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        private InputSystem_Actions inputSystem;
        private Animator animator;

        [SerializeField] private float moveSpeed;

        [SerializeField] private float sprintRecoveryTime; // 冲刺恢复时间，2s
        [SerializeField] private float sprintContinueTime; // 冲刺无敌时间，1s
        [SerializeField] private int sprintCount;
        [SerializeField] private int sprintIndex;

        private Coroutine sprintCoroutine;
        private Coroutine sprintDurationCoroutine; // 冲刺持续协程

        [SerializeField] private bool isSprinting = false; // 本地冲刺状态标识
        [SerializeField] private float sprintMultiplier = 10f; // 本地冲刺速度倍数

        private Vector2 inputDir = Vector2.zero;
        private Vector2 lastInputDir = Vector2.zero;

        private void Awake()
        {
            inputSystem = new InputSystem_Actions();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (VampireSurvivalData.instance == null)
            {
                Debug.LogError("VampireSurvivalData instance is null! Please ensure it is initialized before accessing.");
                return;
            }

            moveSpeed = VampireSurvivalData.instance.moveSpeed;
            sprintRecoveryTime = VampireSurvivalData.baseSprintRecoveryTime;
            sprintContinueTime = VampireSurvivalData.baseSprintContinueTime;
            sprintCount = VampireSurvivalData.instance.sprintCount;
            sprintIndex = 0;
            sprintCoroutine = null;
        }

        private void OnEnable()
        {
            inputSystem.Enable();

            inputSystem.Player.Move.performed += OnMovePerformed;
            inputSystem.Player.Move.canceled += OnMoveCanceled;

            inputSystem.Player.Sprint.performed += OnSprintPerformed;
            inputSystem.Player.Sprint.canceled += OnSprintCanceled;
        }

        private void OnDisable()
        {
            inputSystem.Player.Sprint.performed -= OnSprintPerformed;
            inputSystem.Player.Sprint.canceled -= OnSprintCanceled;

            inputSystem.Player.Move.performed -= OnMovePerformed;
            inputSystem.Player.Move.canceled -= OnMoveCanceled;

            inputSystem.Disable();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Move performed: " + context.ReadValue<Vector2>());
            inputDir = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            Debug.Log("Move canceled");
            inputDir = Vector2.zero;
        }

        /// <summary>
        /// 实现玩家的冲刺功能：
        /// 当按下冲刺键时，如果还有剩余的冲刺次数，则进入冲刺状态，增加移动速度并给予短时间无敌。
        /// </summary>
        /// <param name="context"></param>
        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (sprintCount > 0)
            {
                sprintCount--;
                sprintIndex++;
                Debug.Log("Sprint performed: sprintCount=" + sprintCount + ", sprintIndex=" + sprintIndex);
                
                if (sprintCoroutine == null)
                {
                    Debug.Log("Starting sprint recovery coroutine");
                    sprintCoroutine = StartCoroutine(SprintRecoveryCoroutine());
                }
                // 开始冲刺：设置速度、状态并启动冲刺持续计时
                moveSpeed = VampireSurvivalData.instance.moveSpeed * sprintMultiplier;
                StartSprint();
            }
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            // 手动取消冲刺时结束当前冲刺
            EndSprint();
        }

            // Update is called once per frame
        private void FixedUpdate()
        {
            Move();
        }

        /// <summary>
        /// 实现玩家的基本移动功能
        /// </summary>
        private void Move()
        {
            transform.Translate(moveSpeed * inputDir * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 开始冲刺：设置冲刺状态、增加速度并给予短时间无敌
        /// </summary>
        private void StartSprint()
        {
            if (isSprinting) return; // 已经在冲刺中
            isSprinting = true;

            // 在冲刺开始时给予短时间无敌
            SetPlayerInvulnerable(true);

            // 启动冲刺持续协程，冲刺持续时间以配置为准
            if (sprintDurationCoroutine != null)
                StopCoroutine(sprintDurationCoroutine);
            sprintDurationCoroutine = StartCoroutine(SprintDurationCoroutine(sprintContinueTime));
        }

        /// <summary>
        /// 结束冲刺：重置速度、状态并取消无敌
        /// </summary>
        private void EndSprint()
        {
            if (!isSprinting) return;
            isSprinting = false;

            // 重置移动速度为基础速度
            moveSpeed = VampireSurvivalData.instance.moveSpeed;

            // 停止冲刺持续协程
            if (sprintDurationCoroutine != null)
            {
                StopCoroutine(sprintDurationCoroutine);
                sprintDurationCoroutine = null;
            }

            // 结束冲刺后无需立即取消无敌，保持1s无敌时间（如果sprintContinueTime设置为0可以立即取消）
            StartCoroutine(EndInvulnerabilityAfterDelay(sprintContinueTime));
        }

        private IEnumerator SprintDurationCoroutine(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            // 冲刺持续时间结束，重置冲刺状态
            sprintDurationCoroutine = null;
            EndSprint();
        }

        private IEnumerator EndInvulnerabilityAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            SetPlayerInvulnerable(false);
        }

        /// <summary>
        /// 设置 PlayerHealthController 的冲刺状态（无敌）
        /// PlayerHealthController 中的 isInSprint 已为 public，直接赋值即可，不使用反射
        /// </summary>
        /// <param name="invulnerable"></param>
        private void SetPlayerInvulnerable(bool invulnerable)
        {
            var healthInstance = PlayerHealthController.instance;
            if (healthInstance == null) return;

            // 直接设置 public 字段，避免使用反射
            healthInstance.isInSprint = invulnerable;
        }

        // 实现冲刺恢复的协程
        private IEnumerator SprintRecoveryCoroutine()
        {
            while (sprintIndex > 0)
            {
                yield return new WaitForSeconds(sprintRecoveryTime);
                sprintIndex--;
                sprintCount++;
            }
            sprintCoroutine = null; // 恢复完成后重置协程引用
        }
    }
}