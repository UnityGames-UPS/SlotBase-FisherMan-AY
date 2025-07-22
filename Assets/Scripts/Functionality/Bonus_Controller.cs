using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Bonus_Controller : MonoBehaviour
{
    [SerializeField] private SocketIOManager socketmanager;

    // Start is called before the first frame update
    [SerializeField] private Button[] chest;
    [SerializeField] private ImageAnimation[] chestAnim;
    [SerializeField] private TMP_Text[] reward_text;
    private List<double> resultData= new List<double>();
    [SerializeField] private GameObject bonusObject;

    public int openCount;
    public bool isfinished = false;
    private bool opening = false;

    [SerializeField] private List<int> openIndex;
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotBehaviour;

    [SerializeField] private GameObject WinPopUp;
    [SerializeField] private TMP_Text WinPopUpText;
    private double winAmount;

    [SerializeField] private List<string> Fakeresult=new List<string>();

    //COMPLETED: slot add audio

    //COMPLETED: check bonus with socket and auto spin
    private void Start()
    {
        for (int i = 0; i < chest.Length; i++)
        {
            int index = i;
            chest[i].onClick.RemoveAllListeners();
            chest[i].onClick.AddListener(delegate { OnChestOpen(index); });
        }

        // StartBonusGame(Fakeresult);
    }

    internal void StartBonusGame()
    {
        //for (int i = 0; i < result.Count; i++)
        //{
        //    resultData.Add(result[i]);
        //}

        audioController.StopBgAudio();
        audioController.StopWLAaudio();
        audioController.playBgAudio("bonus");

        bonusObject.SetActive(true);

    }

    internal void FinishBonusGame(ref bool chechPopups)
    {
        opening = false;
        isfinished = false;
        resultData.Clear();
        openCount = 0;
        winAmount = 0;
        WinPopUpText.text = "";
        bonusObject.SetActive(false);
        WinPopUp.SetActive(false);

        audioController.playBgAudio("normal");
        foreach (Button item in chest)
        {
            item.interactable = true;
        }

        chechPopups=false;
    }


    void OnChestOpen(int index)
    {
        if (isfinished) return;
        if (opening) return;
        audioController.PlayButtonAudio();
        StartCoroutine(chestOpenRoutine(index));

    }

    IEnumerator chestOpenRoutine(int index)
    {
        // audioController.PlaySpinBonusAudio("bonus");
        opening = true;
        openIndex.Add(index);
        chest[index].interactable = false;
        bool gameFinishied = false;

        socketmanager.OnBonusCollect(index);
        chestAnim[index].transform.DOShakePosition(1f, new Vector3(15, 0, 0), 30, 90, true);
        yield return new WaitUntil(()=> socketmanager.isResultdone);
        chestAnim[index].StartAnimation();
        // audioController.StopApinBonusAudio();

        if (socketmanager.bonusData.payload.payout > 0)
        {
            audioController.PlayWLAudio("bonuswin");
            reward_text[index].text = socketmanager.bonusData.payload.winAmount.ToString("f2");
            winAmount += socketmanager.bonusData.payload.winAmount;
            // reward_text[index].text = "+ " + (resultData[openCount] * 1).ToString("f2");
            // winAmount += (resultData[openCount] * 1);
        }
        else
        {
            audioController.PlayWLAudio("bonuslose");
            reward_text[index].text = "game Over";
            gameFinishied = true;
        }
        // reward_text[index].color =Color.black;
        reward_text[index].transform.localScale = Vector3.zero;
        reward_text[index].gameObject.SetActive(true);
        reward_text[index].transform.DOScale(1, 0.8f);
        reward_text[index].transform.DOLocalMoveY(235, 0.8f);
        yield return new WaitForSeconds(1.5f);
        reward_text[index].gameObject.SetActive(false);
        reward_text[index].transform.localPosition = new Vector3(-50, -42);
        openCount++;
        audioController.StopWLAaudio();

        if (gameFinishied)
        {

            WinPopUp.transform.GetChild(0).localScale = Vector3.zero;
            WinPopUpText.text = winAmount.ToString();
            WinPopUp.SetActive(true);
            WinPopUp.transform.transform.GetChild(0).DOScale(Vector3.one, 0.8f);
            yield return new WaitForSeconds(3f);
            isfinished = true;

        }
        opening = false;

    }


}
