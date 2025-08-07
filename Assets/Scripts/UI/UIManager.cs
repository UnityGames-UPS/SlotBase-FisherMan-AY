using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{


    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;


    [Header("Paytable Popup")]
    [SerializeField] private Button Paytable_button;
    [SerializeField] private GameObject PaytablePopup_Object;
    [SerializeField] private Button PaytableExit_Button;
    [SerializeField] private Button Left_Arrow;
    [SerializeField] private Button Right_Arrow;
    [SerializeField] private GameObject[] paytableList;
    [SerializeField] private int CurrentIndex = 0;
    [SerializeField] private TMP_Text[] SymbolsText;
    [SerializeField] private TMP_Text Scatter_Text;
    [SerializeField] private TMP_Text Jackpot_Text;
    [SerializeField] private TMP_Text Bonus_Text;
    [SerializeField] private TMP_Text Wild_Text;

    [Header("Win Popup")]
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private Image Win_Image;
    [SerializeField] private Sprite BigWin_Sprite;
    [SerializeField] private Sprite HugeWin_Sprite;
    [SerializeField] private Sprite MegaWin_Sprite;
    [SerializeField] private Sprite Jackpot_Sprite;
    [SerializeField] private Sprite Scatter_Sprite;
    [SerializeField] private TMP_Text Win_Text;
    [SerializeField] private Button SkipWinAnimation;


    [Header("Menu popup")]
    [SerializeField] private Transform Menu_button_grp;
    [SerializeField] private Button Menu_button;
    [SerializeField] private bool isOpen = false;
    [SerializeField] private Sprite MenuOpenSprite;
    [SerializeField] private Sprite MenuCloseSprite;


    [Header("Settings popup")]
    [SerializeField] private Button Setting_button;
    [SerializeField] private GameObject settingObject;
    [SerializeField] private Button SoundON_Button;
    [SerializeField] private Button SoundOff_Button;
    [SerializeField] private Button MusicON_Button;
    [SerializeField] private Button MusicOff_Button;
    [SerializeField] private Button Setting_exit_button;


    [Header("Splash Screen")]
    [SerializeField] private GameObject spalsh_screen;
    [SerializeField] private Image progressbar;

    [Header("Scripts")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotManager;
    [SerializeField] private SocketIOManager socketManager;


    [Header("Quit Popup")]
    [SerializeField] private GameObject QuitPopupObject;
    [SerializeField] private Button no_Button;
    [SerializeField] private Button yes_Button;
    [SerializeField] private Button ExitCancel_Button;
    [SerializeField] private Button GameExit_Button;

    [Header("disconnection popup")]
    [SerializeField] private GameObject DisconnectPopupObject;
    [SerializeField] private Button CloseDisconnect_Button;
    [Header("Reconection Popup")]
    [SerializeField]
    private GameObject ReconectingPopup_Object;
    [Header("low balance popup")]
    [SerializeField] private Button Close_Button;
    [SerializeField] private GameObject LowBalancePopup_Object;

    [Header("AnotherDevice Popup")]
    [SerializeField] private Button CloseAD_Button;
    [SerializeField] private GameObject ADPopup_Object;

    [SerializeField] private Button m_AwakeGameButton;

    [SerializeField] internal GameObject RaycastBlocker;


    private bool isExit = false;


    private bool isMusic = true;
    private bool isSound = true;
    private Tween WinPopupTextTween;
    private Tween ClosePopupTween;

    //COMPLETED: slot add splash screen

    //COMPLETED: slot populate all symbol text

    //COMPLETED: slot set disconnection

    //COMPLETED: slot set audio control

    private void Awake()
    {
        // if (spalsh_screen) spalsh_screen.SetActive(true);
        // StartCoroutine(LoadingRoutine());

        SimulateClickByDefault();

    }

    private void SimulateClickByDefault()
    {
        Debug.Log("Awaken The Game...");
        m_AwakeGameButton.onClick.AddListener(() => { Debug.Log("Called The Game..."); });
        m_AwakeGameButton.onClick.Invoke();
    }
    private void Start()
    {

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); ResetInfoScreens(); });


        if (Paytable_button) Paytable_button.onClick.RemoveAllListeners();
        if (Paytable_button) Paytable_button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        if (Left_Arrow) Left_Arrow.onClick.RemoveAllListeners();
        if (Left_Arrow) Left_Arrow.onClick.AddListener(delegate { Slide(-1); });

        if (Right_Arrow) Right_Arrow.onClick.RemoveAllListeners();
        if (Right_Arrow) Right_Arrow.onClick.AddListener(delegate { Slide(1); });

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopupObject); });

        if (no_Button) no_Button.onClick.RemoveAllListeners();
        if (no_Button) no_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopupObject); } });

        if (ExitCancel_Button) ExitCancel_Button.onClick.RemoveAllListeners();
        if (ExitCancel_Button) ExitCancel_Button.onClick.AddListener(delegate { if (!isExit) { ClosePopup(QuitPopupObject); } });

        if (yes_Button) yes_Button.onClick.RemoveAllListeners();
        if (yes_Button) yes_Button.onClick.AddListener(CallOnExitFunction);

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction(); socketManager.ReactNativeCallOnFailedToConnect(); }); //BackendChanges

        if (Close_Button) Close_Button.onClick.RemoveAllListeners();
        if (Close_Button) Close_Button.onClick.AddListener(delegate { ClosePopup(LowBalancePopup_Object); });

        //Menu
        if (Menu_button) Menu_button.onClick.RemoveAllListeners();
        if (Menu_button) Menu_button.onClick.AddListener(OnMenuClick);

        //settings
        if (Setting_button) Setting_button.onClick.RemoveAllListeners();
        if (Setting_button) Setting_button.onClick.AddListener(delegate { OpenPopup(settingObject); });

        if (Setting_exit_button) Setting_exit_button.onClick.RemoveAllListeners();
        if (Setting_exit_button) Setting_exit_button.onClick.AddListener(delegate { ClosePopup(settingObject); });

        if (SoundON_Button) SoundON_Button.onClick.RemoveAllListeners();
        if (SoundON_Button) SoundON_Button.onClick.AddListener(delegate { ToggleSound(true); });

        if (SoundOff_Button) SoundOff_Button.onClick.RemoveAllListeners();
        if (SoundOff_Button) SoundOff_Button.onClick.AddListener(delegate { ToggleSound(false); });

        if (MusicON_Button) MusicON_Button.onClick.RemoveAllListeners();
        if (MusicON_Button) MusicON_Button.onClick.AddListener(delegate { ToggleMusic(true); });

        if (MusicOff_Button) MusicOff_Button.onClick.RemoveAllListeners();
        if (MusicOff_Button) MusicOff_Button.onClick.AddListener(delegate { ToggleMusic(false); });

        if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
        if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

        if (SkipWinAnimation) SkipWinAnimation.onClick.RemoveAllListeners();
        if (SkipWinAnimation) SkipWinAnimation.onClick.AddListener(SkipWin);

        ToggleMusic(true);
        ToggleMusic(true);



    }



    //COMPLETED: slot add socket in loading splash screen
    private IEnumerator LoadingRoutine()
    {
        float fillAmount = 0.7f;
        progressbar.DOFillAmount(fillAmount, 3f).SetEase(Ease.Linear);
        yield return new WaitForSecondsRealtime(3f);
        yield return new WaitUntil(() => !socketManager.isLoading);
        progressbar.DOFillAmount(1, 1f).SetEase(Ease.Linear);
        yield return new WaitForSecondsRealtime(1f);
        if (spalsh_screen) spalsh_screen.SetActive(false);
    }




    internal void PopulateWin(int value, double amount)
    {
        switch (value)
        {
            case 1:
                if (Win_Image) Win_Image.sprite = BigWin_Sprite;
                break;
            case 2:
                if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
                break;
            case 3:
                if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
                break;
            case 4:
                if (Win_Image) Win_Image.sprite = Jackpot_Sprite;
                break;
            case 5:
                if (Win_Image) Win_Image.sprite = Scatter_Sprite;
                break;
        }
        StartPopupAnim(amount, false);

    }


    private void StartPopupAnim(double amount, bool jackpot = false)
    {
        double initAmount = 0;

        if (WinPopup_Object) WinPopup_Object.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        WinPopupTextTween = DOTween.To(() => initAmount, (val) => initAmount = val, amount, 5f).OnUpdate(() =>
        {

            if (Win_Text) Win_Text.text = initAmount.ToString("f3");

        });

        ClosePopupTween = DOVirtual.DelayedCall(6f, () =>
        {

            ClosePopup(WinPopup_Object);
            Win_Text.text = "";

            slotManager.CheckPopups = false;
        });
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopupObject || !DisconnectPopupObject.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    internal void LowBalPopup()
    {

        OpenPopup(LowBalancePopup_Object);
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    private void Slide(int i)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (CurrentIndex < paytableList.Length - 1 && i > 0)
        {
            paytableList[CurrentIndex].SetActive(false);
            paytableList[CurrentIndex + 1].SetActive(true);
            CurrentIndex++;
        }

        if (CurrentIndex >= 1 && i < 0)
        {
            paytableList[CurrentIndex].SetActive(false);
            paytableList[CurrentIndex - 1].SetActive(true);
            CurrentIndex--;
        }
    }

    private void ResetInfoScreens()
    {
        CurrentIndex = 0;
        for (int i = 0; i < paytableList.Length; i++)
        {
            paytableList[i].SetActive(false);
        }
        paytableList[CurrentIndex].SetActive(true);
    }

    void SkipWin()
    {
        Debug.Log("Skip win called");
        if (ClosePopupTween != null)
        {
            ClosePopupTween.Kill();
            ClosePopupTween = null;
        }
        if (WinPopupTextTween != null)
        {
            WinPopupTextTween.Kill();
            WinPopupTextTween = null;
        }
        ClosePopup(WinPopup_Object);
        slotManager.CheckPopups = false;
    }

    void OnMenuClick()
    {
        isOpen = !isOpen;
        if (audioController) audioController.PlayButtonAudio();

        if (isOpen)
        {
            if (Menu_button) Menu_button.image.sprite = MenuCloseSprite;
            for (int i = 0; i < Menu_button_grp.childCount - 1; i++)
            {
                Menu_button_grp.GetChild(i).DOLocalMoveY(+130 * (i + 1), 0.1f * (i + 1));
            }
        }
        else
        {

            if (Menu_button) Menu_button.image.sprite = MenuOpenSprite;

            for (int i = 0; i < Menu_button_grp.childCount - 1; i++)
            {
                Menu_button_grp.GetChild(i).DOLocalMoveY(5, 0.1f * (i + 1));
            }
        }

    }

    internal void InitialiseUIData(Paylines symbolsText, double multi)
    {
        PopulateSymbolsPayout(symbolsText, multi);
    }

    private void PopulateSymbolsPayout(Paylines paylines, double multi)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = null;
            if (paylines.symbols[i].multiplier[0] != 0)
            {
                text += "5x - " + (paylines.symbols[i].multiplier[0] * multi).ToString();
            }
            if (paylines.symbols[i].multiplier[1] != 0)
            {
                text += "\n4x - " + (paylines.symbols[i].multiplier[1] * multi).ToString();
            }
            if (paylines.symbols[i].multiplier[2] != 0)
            {
                text += "\n3x - " + (paylines.symbols[i].multiplier[2] * multi).ToString();
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
        }

        for (int i = 0; i < paylines.symbols.Count; i++)
        {

            if (paylines.symbols[i].name.ToUpper() == "SCATTER")
            {
                if (Scatter_Text) Scatter_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].name.ToUpper() == "JACKPOT")
            {
                if (Jackpot_Text) Jackpot_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].name.ToUpper() == "BONUS")
            {
                if (Bonus_Text) Bonus_Text.text = paylines.symbols[i].description.ToString();
            }
            if (paylines.symbols[i].name.ToUpper() == "WILD")
            {
                if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
            }
        }

    }


    internal void DisconnectionPopup()
    {
        //if (isReconnection)
        //{
        //    OpenPopup(ReconnectPopup_Object);
        //}
        //else
        //{
        //    ClosePopup(ReconnectPopup_Object);
        if (!isExit)
        {
            OpenPopup(DisconnectPopupObject);
        }
        //}
    }
    internal void ReconnectionPopup()
    {
        OpenPopup(ReconectingPopup_Object);
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
        // Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }
    internal void CheckAndClosePopups()
    {

        if (ReconectingPopup_Object.activeInHierarchy)
        {
            ClosePopup(ReconectingPopup_Object);
        }
        if (DisconnectPopupObject.activeInHierarchy)
        {
            ClosePopup(DisconnectPopupObject);
        }
    }

    private void ToggleMusic(bool isMusic)
    {
        // isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicON_Button) MusicON_Button.interactable = false;
            if (MusicOff_Button) MusicOff_Button.interactable = true;
            audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicON_Button) MusicON_Button.interactable = true;
            if (MusicOff_Button) MusicOff_Button.interactable = false;

            audioController.ToggleMute(true, "bg");
        }
    }


    private void ToggleSound(bool isSound)
    {
        // isSound = !isSound;

        if (isSound)
        {
            //if (SoundOn_Object) SoundOn_Object.SetActive(true);
            //if (SoundOff_Object) SoundOff_Object.SetActive(false);

            if (SoundON_Button) SoundON_Button.interactable = false;
            if (SoundOff_Button) SoundOff_Button.interactable = true;
            if (audioController) audioController.ToggleMute(false, "button");
            if (audioController) audioController.ToggleMute(false, "wl");
        }
        else
        {
            //if (SoundOn_Object) SoundOn_Object.SetActive(false);
            //if (SoundOff_Object) SoundOff_Object.SetActive(true);
            if (SoundON_Button) SoundON_Button.interactable = true;
            if (SoundOff_Button) SoundOff_Button.interactable = false;

            if (audioController) audioController.ToggleMute(true, "button");
            if (audioController) audioController.ToggleMute(true, "wl");
        }
    }
}
