using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvival.Player;

namespace VampireSurvival.UI
{
    /// <summary>
    /// 实现游戏中HUD（Head-Up Display）的功能，显示玩家的当前状态、属性和其他重要信息。
    /// </summary>
    public class HUDPanel : MonoBehaviour
    {
        private TextMeshProUGUI missionNameText;
        private TextMeshProUGUI mapNameText;
        private TextMeshProUGUI gameProgressText;
        private TextMeshProUGUI creditText;

        private HorizontalLayoutGroup buffList;
        private Image weaponImage1;
        private Image weaponImage2;
        private Image weaponImage3;

        void Start()
        {
            // 初始化HUD显示，订阅玩家属性变化事件以更新HUD
            
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}

