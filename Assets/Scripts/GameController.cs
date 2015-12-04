using UnityEngine;
using System.Collections;
using System.IO;

public class GameController : MonoBehaviour {

    public const int Cloume=4;    
    private int[,] iMap = new int[Cloume, Cloume];
    private int[,] backMap;
    public static int[,] saveMap = new int[Cloume, Cloume]; 
    private bool isMove = false;
    private static string Theme="Madoka";
    private static string strBackground = "默认";
    private int Score = 0;
    private int BestScore = 0;
    private bool isNewBestScore = false;
    private string appDBPath;
    private bool isSaveOrLoad = true;
    private int backScore = 0;

    //游戏元素
    private UISprite[,] GameSprites = new UISprite[Cloume, Cloume];
    //游戏退出面板，游戏结束面板，游戏设置面板
    //标题样式
    private UISprite TitleSprite;
    private UISprite SaveGamePanel;
    private UISprite SetGamePanel;
    private UISprite GameOverPanel;
    private UISprite GameMenuWidget;
    private UISprite StartGameWidget;
    private UIPopupList ThemeList;
    private UIPopupList BGList;

    private UILabel MainScoreLabel;
    private UILabel MainBestScoreLabel;
    private UILabel GameOverScoreLabel;
    private UILabel RandomLabel;
    private UILabel GameDataBtnName1;
    private UILabel GameDataBtnName2;
    private UILabel GameDataBtnName3;
    private UILabel SaveOrLoadLabel;
    private UILabel[] GameDataBtnName = new UILabel[4];
    private UIButton QuitGameBtn;
    private UIButton StartGameBtn;
    private UIButton SetGameBtn;
    private UIButton ReturnSettingBtn;
    private UIButton ReturnMenuBtn;
    private UIButton BackUpBtn;
    private UIButton ReStartBtn;


    void Start()
    {      
        InitGameSprite();
    }
   
    void Update()
    {
        GameMove();
        if ((Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Escape)) && StartGameWidget.gameObject.activeSelf == true) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (StartGameWidget.gameObject.activeSelf == true)
            {
                if (GameOverPanel.gameObject.activeSelf == false && SaveGamePanel.gameObject.activeSelf == false && SetGamePanel.gameObject.activeSelf == false)
                {
                    backUp();

                }
                else if (GameOverPanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    GameOverPanel.gameObject.SetActive(false);
                }
                else if (SaveGamePanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    SaveGamePanel.gameObject.SetActive(false);
                }
                else if (SetGamePanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    SetGamePanel.gameObject.SetActive(false);
                }

            }
            if (SaveGamePanel.gameObject.activeSelf == true)
            {
                SetBtnState(true);
                SaveGamePanel.gameObject.SetActive(false);
            }
            if (SetGamePanel.gameObject.activeSelf == true)
            {
                SetBtnState(true);
                SetGamePanel.gameObject.SetActive(false);
            }
        }
    }

    #region 游戏的移动操作
#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
    //编辑器靠上下左右键
    private void GameMove()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                backMap = (int[,])iMap.Clone();
                backScore = Score;
                SwipeRight();
                DrawMap(Theme);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                backScore = Score;
                backMap = (int[,])iMap.Clone();
                SwipeLeft();
                DrawMap(Theme);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                backScore = Score;
                backMap = (int[,])iMap.Clone();
                SwipeUp();
                DrawMap(Theme);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                backScore = Score;
                backMap = (int[,])iMap.Clone();
                SwipeDown();
                DrawMap(Theme);
            }
        }
    }
#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
    //触屏靠两次触摸的差距
    private Vector2 firstPosition = Vector2.zero, lastPosition = Vector2.zero, offSet = Vector2.zero;
    private void GameMove()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                firstPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                lastPosition = touch.position;
                offSet = lastPosition - firstPosition;
                //if (offSet.x > offSet.y)
                if (Mathf.Abs(offSet.x) - Mathf.Abs(offSet.y) > 3)
                {
                    if (offSet.x >= 5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeRight();
                    }
                    else if (offSet.x <= -5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeLeft();

                    }
                }
                else if (Mathf.Abs(offSet.y) - Mathf.Abs(offSet.x) > 3)
                {
                    if (offSet.y >= 5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeUp();
                    }
                    else if (offSet.y <= -5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeDown();
                    }
                }
                DrawMap(Theme);
            }
        }
    }
   
#endif
    #endregion


    #region 地图模式的转换
    //将字符串模式的地图转换成二维数组模式
    System.StringSplitOptions ssop = System.StringSplitOptions.RemoveEmptyEntries;
    private void SplitStrMap(string strMap, out int[,] a)
    {
        a = new int[Cloume, Cloume];
        string[] MapOrignX = strMap.Split(new char[] { ';' }, ssop);
        for (int i = 0; i < MapOrignX.Length; ++i)
        {
            string[] MapOrignY = MapOrignX[i].Split(new char[] { ',' }, ssop);
            for (int j = 0; j < MapOrignY.Length; ++j)
            {

                a[i, j] = int.Parse(MapOrignY[j]);
            }
        }
    }
    //将二维数组模式的地图转换成字符串模式
    private void MapToString(int[,] intMap, out string outStrMap)
    {
        outStrMap = "";
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for (int iy = 0; iy < Cloume; ++iy)
            {
                outStrMap = outStrMap.Trim() + intMap[ix, iy];
                if (iy != Cloume - 1)
                    outStrMap += ",";
            }
            if (ix != Cloume - 1)
                outStrMap += ";";
        }
    }
    #endregion



    //显示其他面板时，设置底层的按钮无法点击
    private void SetBtnState(bool isTrue)
    {
        StartGameBtn.isEnabled = isTrue;
        SetGameBtn.isEnabled = isTrue;
        QuitGameBtn.isEnabled = isTrue;
        ReturnSettingBtn.isEnabled = isTrue;
        ReturnMenuBtn.isEnabled = isTrue;
        BackUpBtn.isEnabled = isTrue;
        ReStartBtn.isEnabled = isTrue;
    }

    //初始化地图数组
    private void InitIMap()
    {
        Score = 0;
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for(int iy = 0; iy < Cloume; ++iy)
            {
                iMap[ix, iy] = 0;
            }
        }
    }

    //初始化游戏Sprite
    private void InitGameSprite()
    {
        GameMenuWidget = this.transform.parent.Find("GameMenu").GetComponent<UISprite>();
        StartGameWidget = this.transform.parent.Find("StartGame").GetComponent<UISprite>();
        StartGameWidget.spriteName = strBackground;
        SetGamePanel = this.transform.parent.Find("SetGamePanel").GetComponent<UISprite>();
        SaveGamePanel = this.transform.parent.Find("SaveGamePanel").GetComponent<UISprite>();
        GameOverPanel = this.transform.parent.Find("GameOverPanel").GetComponent<UISprite>();
        QuitGameBtn = this.transform.parent.Find("GameMenu/QuitGameBtn").GetComponent<UIButton>();
        ReturnSettingBtn = this.transform.parent.Find("StartGame/ReturnSettingBtn").GetComponent<UIButton>();
        StartGameBtn = this.transform.parent.Find("GameMenu/StartGameBtn").GetComponent<UIButton>();
        SetGameBtn = this.transform.parent.Find("GameMenu/SetGameBtn").GetComponent<UIButton>();
        ReturnMenuBtn = this.transform.parent.Find("StartGame/ReturnMenuBtn").GetComponent<UIButton>();
        BackUpBtn = this.transform.parent.Find("StartGame/BackUpBtn").GetComponent<UIButton>();
        ReStartBtn = this.transform.parent.Find("StartGame/ReStartBtn").GetComponent<UIButton>();

        ThemeList= this.transform.parent.Find("SetGamePanel/ThemeList").GetComponent<UIPopupList>();
        BGList = this.transform.parent.Find("SetGamePanel/BGList").GetComponent<UIPopupList>();
        TitleSprite = this.transform.parent.Find("StartGame/TitleSprite").GetComponent<UISprite>();
        MainScoreLabel = this.transform.parent.Find("StartGame/ScoreBg/Score").GetComponent<UILabel>();
        MainBestScoreLabel = this.transform.parent.Find("StartGame/BestScoreBg/BestScore").GetComponent<UILabel>();
        GameOverScoreLabel = this.transform.parent.Find("GameOverPanel/ScoreLabel").GetComponent<UILabel>();
        RandomLabel = this.transform.parent.Find("GameOverPanel/RandomLabel").GetComponent<UILabel>();
        SaveOrLoadLabel = this.transform.parent.Find("SaveGamePanel/Label").GetComponent<UILabel>();
        GameDataBtnName1 = this.transform.parent.Find("SaveGamePanel/GameDataBtn1/Label").GetComponent<UILabel>();
        GameDataBtnName2 = this.transform.parent.Find("SaveGamePanel/GameDataBtn2/Label").GetComponent<UILabel>();
        GameDataBtnName3 = this.transform.parent.Find("SaveGamePanel/GameDataBtn3/Label").GetComponent<UILabel>();
        GameDataBtnName[1] = GameDataBtnName1;
        GameDataBtnName[2] = GameDataBtnName2;
        GameDataBtnName[3] = GameDataBtnName3;

        for (int ix = 0; ix < Cloume; ++ix)
        {
            for (int iy = 0; iy < Cloume; ++iy)
            {
                GameSprites[ix, iy] = this.transform.parent.Find("StartGame/GameMap/Sprite" + ix + iy).GetComponent<UISprite>();

            }
        }
        #region 展开数组初始化
        //GameSprites[0, 0] = this.transform.Find("Sprite00").GetComponent<UISprite>();
        //GameSprites[0, 1] = this.transform.Find("Sprite01").GetComponent<UISprite>();
        //GameSprites[0, 2] = this.transform.Find("Sprite02").GetComponent<UISprite>();
        //GameSprites[0, 3] = this.transform.Find("Sprite03").GetComponent<UISprite>();
        //GameSprites[1, 0] = this.transform.Find("Sprite10").GetComponent<UISprite>();
        //GameSprites[1, 1] = this.transform.Find("Sprite11").GetComponent<UISprite>();
        //GameSprites[1, 2] = this.transform.Find("Sprite12").GetComponent<UISprite>();
        //GameSprites[1, 3] = this.transform.Find("Sprite13").GetComponent<UISprite>();
        //GameSprites[2, 0] = this.transform.Find("Sprite20").GetComponent<UISprite>();
        //GameSprites[2, 1] = this.transform.Find("Sprite21").GetComponent<UISprite>();
        //GameSprites[2, 2] = this.transform.Find("Sprite22").GetComponent<UISprite>();
        //GameSprites[2, 3] = this.transform.Find("Sprite23").GetComponent<UISprite>();
        //GameSprites[3, 0] = this.transform.Find("Sprite30").GetComponent<UISprite>();
        //GameSprites[3, 1] = this.transform.Find("Sprite31").GetComponent<UISprite>();
        //GameSprites[3, 2] = this.transform.Find("Sprite32").GetComponent<UISprite>();
        //GameSprites[3, 3] = this.transform.Find("Sprite33").GetComponent<UISprite>();
        #endregion
    }


    #region 按键点击事件
    #region 菜单界面
    //开始游戏
    public void StartGameBtnClick()
    {
        InitIMap();
        AddSprite();
        AddSprite();
        DrawMap(Theme);
        MainBestScoreLabel.text = BestScore.ToString();
        GameMenuWidget.gameObject.SetActive(false);
        StartGameWidget.gameObject.SetActive(true);
    }
    //设置游戏
    public void SetGameBtnClick()
    {
        SetBtnState(false);
        SetGamePanel.gameObject.SetActive(true);
    }
    //退出游戏
    public void QuitGame()
    {
        SetBtnState(true);
        InitIMap();
        if (isNewBestScore)
        {
            isNewBestScore = false;
        }
        StartGameWidget.gameObject.SetActive(false);
        GameMenuWidget.gameObject.SetActive(true);
    }
    #endregion

    #region 开始游戏
    //返回菜单
    public void ReturnMenuBtnClick()
    {
        SetBtnState(true);
        InitIMap();
        if (isNewBestScore)
        {
            isNewBestScore = false;
        }
        StartGameWidget.gameObject.SetActive(false);
        GameMenuWidget.gameObject.SetActive(true);
    }

  

    //重新开始
    public void GameReStart()
    {

        if (isNewBestScore)
        {
            isNewBestScore = false;
        }
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for (int iy = 0; iy < Cloume; ++iy)
            {
                iMap[ix, iy] = 0;
            }
        }
        Score = 0;
        MainScoreLabel.text = Score.ToString();
        AddSprite();
        AddSprite();
        DrawMap(Theme);
        if (GameOverPanel.gameObject.activeSelf == true)
        {
            GameOverPanel.gameObject.SetActive(false);
            SetBtnState(true);
        }
    }

    //后退一步
    public void backUp()
    {
        try
        {
            if (isMove)
            {
                iMap = (int[,])backMap.Clone();
                Score = backScore;
                MainScoreLabel.text = Score + "";
                DrawMap(Theme);
                isMove = false;
            }
        }
        catch (System.Exception)
        {
            return;
        }
    }

    #endregion

    #region 设置游戏
    //保存设置
    public void ChangeSetBtnClick()
    {
        string strTheme = "";
        strBackground = BGList.value.Trim();
        switch (ThemeList.value.Trim())
        {
            case "魔法少女小圆":
                strTheme = "Madoka";
                break;
            case "晓美焰":
                strTheme = "Homura";
                break;
            case "LoveLive":
                strTheme = "LoveLive";
                break;
        }
        StartGameWidget.spriteName = strBackground;
        if (strTheme != Theme)
        {
            Theme = strTheme;
            if (StartGameWidget.gameObject.activeSelf == true)
                DrawMap(Theme);
        }
        SetBtnState(true);
        SetGamePanel.gameObject.SetActive(false);
    }
    //继续游戏
    public void CloseSetGamePanelBtnClick()
    {
        SetBtnState(true);
        SetGamePanel.gameObject.SetActive(false);
    }

    #endregion

    public void CloseSaveGamePannelBtnClick()
    {
        SetBtnState(true);
        SaveGamePanel.gameObject.SetActive(false);
    }
    #endregion



    //向地图中随机添加一个元素
    private void AddSprite()
    {
        int rx, ry;
        rx = Random.Range(0, 4);
        ry = Random.Range(0, 4);
        if (iMap[rx, ry] == 0)
        {
            int ri = Random.Range(1, 101);
            iMap[rx, ry] =(ri > 90) ?  4 :  2;
        }
        else AddSprite();
        GameOver();
    }

    //显示游戏结束面板
    private void GameOver()
    {
        if (isGameOver())
        {
            GameOverScoreLabel.text = Score+"";
            RandomLabel.text = "离最高分还差" + (BestScore - Score) + "分~";
            if (isNewBestScore)
            {
                //新记录
                RandomLabel.text = "新纪录~";
            }
            SetBtnState(false);
            GameOverPanel.gameObject.SetActive(true);
        }
    }

    #region 绘制地图
    private void DrawMap(string theme)
    {
        for(int ix = 0; ix < Cloume; ++ix)
        {
            for(int iy = 0; iy < Cloume; ++iy)
            {
                if (iMap[ix, iy] == 0)
                {
                    GameSprites[ix, iy].alpha = 0.4f;
                }
                else GameSprites[ix, iy].alpha = 0.9f;
                switch (iMap[ix, iy])
                {
                    case 0:                      
                        GameSprites[ix, iy].spriteName = "0";
                        break;
                    case 2:
                        GameSprites[ix, iy].spriteName = theme + "2";
                        break;
                    case 4:
                        GameSprites[ix, iy].spriteName = theme + "4";
                        break;
                    case 8:
                        GameSprites[ix, iy].spriteName = theme + "8";
                        break;
                    case 16:
                        GameSprites[ix, iy].spriteName = theme + "16";
                        break;
                    case 32:
                        GameSprites[ix, iy].spriteName = theme + "32";
                        break;
                    case 64:
                        GameSprites[ix, iy].spriteName = theme + "64";
                        break;
                    case 128:
                        GameSprites[ix, iy].spriteName = theme + "128";
                        break;
                    case 256:
                        GameSprites[ix, iy].spriteName = theme + "256";
                        break;
                    case 512:
                        GameSprites[ix, iy].spriteName = theme + "512";
                        break;
                    case 1024:
                        GameSprites[ix, iy].spriteName = theme + "1024";
                        break;
                    case 2048:
                        GameSprites[ix, iy].spriteName = theme + "2048";
                        break;
                    case 4096:
                        GameSprites[ix, iy].spriteName = theme + "4096";
                        break;
                    case 8192:
                        GameSprites[ix, iy].spriteName = theme + "8192";
                        break;
                    case 16384:
                        GameSprites[ix, iy].spriteName = theme + "16384";
                        break;
                    case 32768:
                        GameSprites[ix, iy].spriteName = theme + "32768";
                        break;
                    case 65536:
                        GameSprites[ix, iy].spriteName = theme + "65536";
                        break;
                    case 131072:
                        GameSprites[ix, iy].spriteName = theme + "131072";
                        break;
                    case 262144:
                        GameSprites[ix, iy].spriteName = theme + "262144";
                        break;
                }
                
            }
        }
    }

    #endregion

    //检测游戏是否结束
    private bool isGameOver()
    {
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for (int iy = 0; iy < Cloume; ++iy)
            {
                //1.如果还有位置为0，则游戏未结束。
                //2.如果所有的元素都不为0，判断是否有元素与上下左右相同，如果木有则游戏结束。
                if (iMap[ix, iy] == 0 || (ix > 0 && iMap[ix, iy] == iMap[ix - 1, iy]) || (ix < 3 && iMap[ix, iy] == iMap[ix + 1, iy])
                    || (iy > 0 && iMap[ix, iy] == iMap[ix, iy - 1]) || (iy < 3 && iMap[ix, iy] == iMap[ix, iy + 1]))
                {
                    return false;
                }

            }
        }
        return true;
    }

    #region 移动
    private void SwipeRight()
    {
        isMove = false;
        for (int ix = 0; ix < Cloume; ix++)
        {
            for(int iy = Cloume-1; iy >= 0; iy--)
            {
                for(int ia = iy - 1; ia >= 0; ia--)
                {
                    if (iMap[ix, ia] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ix, ia];
                            iMap[ix, ia] = 0;
                            iy++;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ix, ia])
                        {
                            iMap[ix, iy] += iMap[ix, ia];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ix, ia] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }

    private void SwipeLeft()
    {
        isMove = false;
        for (int ix = 0; ix < Cloume; ix++)
        {
            for(int iy = 0; iy <Cloume; iy++)
            {
                for(int ia = iy + 1; ia < Cloume; ia++)
                {
                    if (iMap[ix, ia] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ix, ia];
                            iMap[ix, ia] = 0;
                            iy--;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ix, ia])
                        {
                            iMap[ix, iy] += iMap[ix, ia];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ix, ia] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }

    private void SwipeUp()
    {
        isMove = false;
        for (int iy = 0; iy < Cloume; iy++)
        {
            for(int ix = 0; ix <Cloume; ix++)
            {
                for(int ib = ix + 1; ib <Cloume; ib++)
                {
                    if (iMap[ib, iy] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ib, iy];
                            iMap[ib, iy] = 0;
                            ix--;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ib, iy])
                        {
                            iMap[ix, iy] += iMap[ib, iy];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ib, iy] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }

    private void SwipeDown()
    {
        isMove = false;
        for (int iy = 0; iy < Cloume; iy++)
        {
            for (int ix = Cloume - 1; ix >= 0; ix--)
            {
                for (int ib = ix - 1; ib >= 0; ib--)
                {
                    if (iMap[ib, iy] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ib, iy];
                            iMap[ib, iy] = 0;
                            ix++;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ib, iy])
                        {
                            iMap[ix, iy] += iMap[ib, iy];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ib, iy] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }
    #endregion

}
