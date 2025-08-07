using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{


    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> slotmatrix;     //class to store the result matrix


    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button")]
    [SerializeField]
    private List<Button> StaticLine_Buttons;
    [Header("Line Button Objects")]
    [SerializeField]
    private List<ManageLineButtons> StaticLine_Scripts;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;


    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text BetPerLine_text;
    [SerializeField]
    private TMP_Text lines_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button LinePlus_Button;
    [SerializeField]
    private Button LineMinus_Button;

    [SerializeField]
    private Button Turbo_Button;
    [SerializeField]
    private Button StopSpin_Button;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    private Tweener WinTween = null;
    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 5;          //number of columns

    [SerializeField]
    int verticalVisibility = 3;

    [Header("scripts")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private SocketIOManager SocketManager;
    [SerializeField] private PayoutCalculation PayCalculator;
    [SerializeField] private Bonus_Controller bonus_Controller;


    Coroutine AutoSpinRoutine = null;
    Coroutine tweenroutine = null;
    bool IsAutoSpin = false;
    private bool IsSpinning = false;
    internal int BetCounter = 0;
    private int LineCounter = 0;
    internal int linecounter = 12;
    private double currentTotalBet;
    private double currentbalance;
    internal bool CheckPopups = false;


    private bool StopSpinToggle;
    private float SpinDelay = 0.2f;
    private bool IsTurboOn;
    private bool WasAutoSpinOn;

    private Tween balanceTween;

    [SerializeField] private Sprite TurboToggleSprite;
    private void Start()
    {

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);


        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);


        if (lines_text != null)
        {
            lines_text.text = "12";
        }


        // if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        // if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ChangeLine(true); });
        // if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        // if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ChangeLine(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);


        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() =>
        {
            StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false);
            audioController.PlayButtonAudio();
        });
        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(() =>
        {
            audioController.PlayButtonAudio();
            TurboToggle();
        });

        tweenHeight = (13 * IconSizeFactor) - 280;

    }

    void TurboToggle()
    {
        if (IsTurboOn)
        {
            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = TurboToggleSprite;
            // Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);
        }
        else
        {
            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            Turbo_Button.image.color = new Color(1, 1, 1, 1);
        }
    }
    private void AutoSpin()
    {
        if (!IsAutoSpin && !IsSpinning)
        {

            IsAutoSpin = true;
            ToggleButtonGrp(false);
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            // if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            WasAutoSpinOn = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
        WasAutoSpinOn = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);

        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            // yield return new WaitForSeconds(0.1f);
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    internal void Fetchlines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        // StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {


        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
            LineID = LineID - 1;
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = new List<int>();


        for (int j = 0; j < 5; j++)
        {

            y_points.Add(SocketManager.InitialData.lines[LineID][j]);

        }

        // PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);

    }

    //Destroy Static lines from button hovers
    internal void DestroyStaticLine()
    {

        PayCalculator.ResetStaticLine();
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.InitialData.bets.Count - 1;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count).ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        // Comparebalance();
        uIManager.InitialiseUIData(SocketManager.UIData.paylines, SocketManager.InitialData.bets[BetCounter]);
    }



    void OnBetOne(bool IncDec)
    {
        // if (audioController) audioController.PlayButtonAudio();

        if (BetCounter < SocketManager.InitialData.bets.Count - 1)
        {
            BetCounter++;
        }
        else
        {
            BetCounter = 0;
        }
        Debug.Log("Index:" + BetCounter);
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count;
        if (TotalBet_text) TotalBet_text.text = currentTotalBet.ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter > SocketManager.InitialData.bets.Count - 1)
            {
                BetCounter = 0;
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.InitialData.bets.Count - 1;
            }
        }
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count;
        if (TotalBet_text) TotalBet_text.text = currentTotalBet.ToString();
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        // Comparebalance();
        uIManager.InitialiseUIData(SocketManager.UIData.paylines, SocketManager.InitialData.bets[BetCounter]);
    }


    //COMPLETE: slot set ui properly at initial and multiparshhet
    internal void SetInitialUI()
    {
        BetCounter = 0;
        LineCounter = SocketManager.InitialData.lines.Count - 1;
        currentbalance = SocketManager.PlayerData.balance;
        currentTotalBet = SocketManager.InitialData.bets[BetCounter] * SocketManager.InitialData.lines.Count;
        if (TotalBet_text) TotalBet_text.text = currentTotalBet.ToString();
        Debug.Log("my bets is " + SocketManager.InitialData.bets[BetCounter]);
        if (lines_text) lines_text.text = (LineCounter + 1).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000".ToString();
        if (balance_text) balance_text.text = currentbalance.ToString("f3");
        Debug.Log("my bets is " + currentbalance);
        if (BetPerLine_text) BetPerLine_text.text = SocketManager.InitialData.bets[BetCounter].ToString();
        uIManager.InitialiseUIData(SocketManager.UIData.paylines, SocketManager.InitialData.bets[0]);
        Comparebalance();
    }


    internal void shuffleInitialMatrix()
    {

        for (int k = 0; k < slotmatrix.Count * 3; k++)
        {
            slotmatrix[k / 3].slotImages[k % 3].sprite = myImages[UnityEngine.Random.Range(0, myImages.Length)];
        }


        // GenerateMatrix(number);
    }

    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {

        if (audioController) audioController.PlayButtonAudio("spin");

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        // if (SlotStart_Button) SlotStart_Button.interactable = false;
        // if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        WinningsAnim(false);
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());

    }


    //COMPLETED: slot compare balance
    private IEnumerator TweenRoutine()
    {


        if (currentbalance < currentTotalBet)
        {
            Comparebalance();
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(1f);
            }
            ToggleButtonGrp(true);
            yield break;
        }
        IsSpinning = true;
        if (!IsTurboOn && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }
        ToggleButtonGrp(false);
        if (audioController) audioController.PlaySpinBonusAudio();

        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }


        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        balanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (balance_text) balance_text.text = initAmount.ToString("f3");
        });

        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);
        // yield return new WaitForSeconds(0.9f);



        //COMPLETED: slot populate result data

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int resultNum = int.Parse(SocketManager.ResultData.matrix[i][j]);
                //  Debug.Log("i :" + i + "  j  " + j + " res num " + resultNum);
                if (slotmatrix[j].slotImages[i]) slotmatrix[j].slotImages[i].sprite = myImages[resultNum];
            }
        }

        if (IsTurboOn)
        {
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }


        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle = false;
        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        if (audioController) audioController.StopApinBonusAudio();
        if (audioController) audioController.StopWLAaudio();

        //COMPLETED: slot chek result and payline and animation
        // yield return new WaitForSeconds(0.1f);
        if (SocketManager.ResultData.payload.winAmount > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }

        if (SocketManager.ResultData.payload.winAmount > 0)
        {
            //List<int> winLine = new();
            //foreach (var item in SocketManager.ResultData.payload.wins)
            //{
            //    winLine.Add(item.line);
            //}
            CheckPayoutLineBackend(SocketManager.ResultData.payload.wins);
            //  if (m_Gamble_Button) m_Gamble_Button.interactable = true;
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }
        CheckForFeaturesAnimation();


        //COMPLETED: slot check all wins
        CheckPopups = true;

        currentbalance = SocketManager.PlayerData.balance;
        if (SocketManager.ResultData.jackpot.isTriggered)
        {
            uIManager.PopulateWin(4, SocketManager.ResultData.jackpot.amount);
            yield return new WaitUntil(() => !CheckPopups);
            CheckPopups = true;

        }

        if (SocketManager.ResultData.bonus.istriggered)
        {
            bonus_Controller.StartBonusGame();
            yield return new WaitUntil(() => bonus_Controller.isfinished);
            bonus_Controller.FinishBonusGame(ref CheckPopups);

        }
        else if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 5 && SocketManager.ResultData.payload.winAmount < currentTotalBet * 10 && SocketManager.ResultData.jackpot.isTriggered)
        {
            uIManager.PopulateWin(1, SocketManager.ResultData.payload.winAmount);
        }
        else if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 10 && SocketManager.ResultData.payload.winAmount < currentTotalBet * 15 && SocketManager.ResultData.jackpot.isTriggered)
        {
            uIManager.PopulateWin(2, SocketManager.ResultData.payload.winAmount);
        }
        else if (SocketManager.ResultData.payload.winAmount >= currentTotalBet * 15 && SocketManager.ResultData.payload.winAmount < currentTotalBet * 20 && SocketManager.ResultData.jackpot.isTriggered)
        {
            uIManager.PopulateWin(3, SocketManager.ResultData.payload.winAmount);
        }
        else if (SocketManager.ResultData.jackpot.isTriggered)
        {
            uIManager.PopulateWin(4, SocketManager.ResultData.payload.winAmount);
        }
        else if (SocketManager.ResultData.scatter.amount > 0)
        {
            uIManager.PopulateWin(5, SocketManager.ResultData.payload.winAmount);
        }
        else
        {
            CheckPopups = false;
        }

        balanceTween?.Kill();

        if (TotalWin_text) TotalWin_text.text = SocketManager.ResultData.payload.winAmount.ToString("f3");
        if (balance_text) balance_text.text = SocketManager.PlayerData.balance.ToString("f3");

        if (SocketManager.ResultData.payload.winAmount > 0)
            WinningsAnim(true);

        yield return new WaitUntil(() => !CheckPopups);


        if (!IsAutoSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(SpinDelay);
            IsSpinning = false;
        }


    }

    void WinningsAnim(bool toggle)
    {
        if (toggle)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            TotalWin_text.text = "0.00";
        }
    }

    private void Comparebalance()
    {
        if (currentbalance < currentTotalBet)
        {
            uIManager.LowBalPopup();
            // if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            // if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        // else
        // {
        // if (AutoSpin_Button) AutoSpin_Button.interactable = true;
        // if (SlotStart_Button) SlotStart_Button.interactable = true;
        // }
    }
    internal double GetCurrentbetperLine()
    {
        return SocketManager.InitialData.bets[BetCounter];
    }
    internal void CallCloseSocket()
    {
        StartCoroutine(SocketManager.CloseSocket());
    }
    private void CallOnExitFunction()
    {
        CallCloseSocket();
        Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }
    private void CheckForFeaturesAnimation()
    {
        bool playScatter = false;
        bool playBonus = false;
        bool playFreespin = false;
        if (SocketManager.ResultData.scatter.amount > 0)
        {
            playScatter = true;
        }
        if (SocketManager.ResultData.bonus.istriggered)
        {
            playBonus = true;
        }
        if (SocketManager.ResultData.jackpot.amount > 0)
        {
            playFreespin = true;
        }
        PlayFeatureAnimation(playScatter, playBonus, playFreespin);
    }
    private void PlayFeatureAnimation(bool scatter = false, bool bonus = false, bool freeSpin = false)
    {
        for (int i = 0; i < SocketManager.ResultData.matrix.Count; i++)
        {
            for (int j = 0; j < SocketManager.ResultData.matrix[i].Count; j++)
            {

                if (int.TryParse(SocketManager.ResultData.matrix[i][j], out int parsedNumber))
                {
                    if (scatter && parsedNumber == 7)
                    {
                        StartGameAnimation(slotmatrix[j].slotImages[i].gameObject);
                    }
                    if (bonus && parsedNumber == 8)
                    {
                        StartGameAnimation(slotmatrix[j].slotImages[i].gameObject);
                    }
                    if (freeSpin && parsedNumber == 10)
                    {
                        StartGameAnimation(slotmatrix[j].slotImages[i].gameObject);
                    }
                }

            }
        }
    }
    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (LinePlus_Button) LinePlus_Button.interactable = toggle;
        if (LineMinus_Button) LineMinus_Button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;

    }
    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.transform.GetChild(0).GetComponent<ImageAnimation>();
        animObjects.transform.GetChild(0).gameObject.SetActive(true);
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
    }

    private void CheckPayoutLineBackend(List<Win> Wins)
    {
        List<int> points_anim = null;
        if (Wins.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");

            for (int i = 0; i < Wins.Count; i++)
            {
                List<int> y_points = new List<int>();
                for (int j = 0; j < 5; j++)
                {
                    y_points.Add(SocketManager.InitialData.lines[Wins[i].line][j]);
                }

                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            for (int i = 0; i < Wins.Count; i++)
            {

                points_anim = Wins[i].positions;

                for (int k = 0; k < Wins[i].positions.Count; k++)
                {
                    StartGameAnimation(slotmatrix[Wins[i].positions[k]].slotImages[SocketManager.InitialData.lines[Wins[i].line][k]].gameObject);

                    //if (points_anim[k] >= 10)
                    //{
                    //}
                    //else
                    //{
                    //    StartGameAnimation(slotmatrix[0].slotImages[points_anim[k]].gameObject);
                    //}
                }
            }
        }

    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(Transform slotTransform, int index, bool isStop)
    {
        alltweens[index].Pause();
        int tweenpos = (3 * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        if (isStop || IsTurboOn)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

