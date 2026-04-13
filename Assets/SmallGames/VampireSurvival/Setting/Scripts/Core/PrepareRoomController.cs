using UnityEngine;
using UnityEngine.UI;
using UnityTV.Core;

namespace VampireSurvival.Core
{
    public class PrepareRoomController : MonoBehaviour
    {
        [SerializeField] private Button shopButton;
        [SerializeField] private Button doorButton;

        [Header("Scene")]
        [Tooltip("点击门时要切换到的场景名（必须在 Build Settings 中）")]
        [SerializeField] private string nextSceneName = "SGVS_01BattleScene";

        private void Awake()
        {
            shopButton = transform.parent.Find("Shop").GetComponent<Button>();
            doorButton = transform.parent.Find("Door").GetComponent<Button>();

            shopButton.onClick.AddListener(OnShopButtonClicked);
            doorButton.onClick.AddListener(OnDoorButtonClicked);
        }

        /// <summary>
        /// 打开商店界面，允许玩家购买物品。
        /// </summary>
        private void OnShopButtonClicked()
        {
            Debug.Log("Shop button clicked. Transitioning to shop scene...");
        }

        /// <summary>
        /// 进入下一个场景，开始新的关卡或挑战。
        /// </summary>
        private void OnDoorButtonClicked()
        {
            Debug.Log("Door button clicked. Transitioning to next level...");

            // 使用 SceneController 提供的静态方法切换场景
            SceneController.LoadScene(nextSceneName);
        }
    }
}
