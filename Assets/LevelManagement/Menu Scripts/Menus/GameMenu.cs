//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace LevelManagement
//{
//    public class GameMenu : Menu<GameMenu>
//    {
//        public void OnPausePressed()
//        {
//            Time.timeScale = 0;

//            PauseMenu.Open();
//        }

//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelManagement
{
    public class GameMenu : Menu<GameMenu>
    {
        // Pauses the game and opens the pause menu
        public void OnPausePressed()
        {
            Time.timeScale = 0;

            PauseMenu.Open();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (transform.parent.Find("PauseMenu(Clone)").gameObject.activeInHierarchy)
                {
                    Time.timeScale = 1;
                    base.OnBackPressed();
                }
                else
                {
                    OnPausePressed();
                }
            }
        }

    }
}
