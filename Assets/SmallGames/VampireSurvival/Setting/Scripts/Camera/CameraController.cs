using UnityEngine;
using VampireSurvival.Player;


namespace VampireSurvival.Camera
{
    public class CameraController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        private void LateUpdate()
        {
            CameraFollow();
        }

        private void CameraFollow()
        {
            Vector3 targetPos = PlayerHealthController.instance.transform.position;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }
    }
}