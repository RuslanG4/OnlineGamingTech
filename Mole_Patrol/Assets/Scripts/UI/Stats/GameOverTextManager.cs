using TMPro;
using UnityEngine;

public class GameOverTexts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text wavesSurvivedText;
    public TMP_Text enemiesKilledText;
    public TMP_Text goldMadeText;
    public TMP_Text towersPlaced;

    public CanvasGroup LoadingScreen;

    void Start()
    {
        CanvasGroupController.DisableGroup(LoadingScreen);
        GameManager.RunTimeGameData gameData = GameManager.Instance.GetRunTimeData();
        enemiesKilledText.text =  "Total Enemies Killed: " + gameData.enemiesKilled.ToString();
        goldMadeText.text = "Total Gold Made: " + gameData.totalGoldMade.ToString();
        towersPlaced.text = "Total Towers Placed: " + gameData.towersPlaced.ToString();
        wavesSurvivedText.text = "Total Waves Survived: " + gameData.wavesSurvived.ToString();
    }

    public void ReturnHome()
    {
        UIGeneralController.LoadScene(LoadingScreen, "Menu");
    }
}
