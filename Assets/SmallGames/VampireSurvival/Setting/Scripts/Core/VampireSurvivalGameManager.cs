using Unity.VisualScripting;
using UnityEngine;
using VampireSurvival.Player;

namespace VampireSurvival.Core
{
    public class VampireSurvivalGameManager : MonoBehaviour
    {
        [Header("Vampire Survival Data")]
        public static VampireSurvivalData VSData { get; private set; }

        private void Awake()
        {
            VSData = new VampireSurvivalData();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
    
