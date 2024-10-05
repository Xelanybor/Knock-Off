using UnityEngine;
using TMPro;

public class StockContainer : MonoBehaviour
{
    // StockContainer is assigned to each player.

    // Has a list of SingleStocks that it manages.
    // Get's the amount of SingleStocks to create from the marble's stockCount.

    [SerializeField] private SingleStock singleStockPrefab;
    // Text Objects
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] public int stockCount;
    [SerializeField] public int percentage;
    // We also get a percentage counter from the marble.

    private SingleStock[] stocks;
    private string name;

    public void setPlayerName(string name)
    {
        this.name = name;
        playerNameText.text = name;

    }

    public void setPercentage(int percentage)
    {
        this.percentage = percentage;
        percentageText.text = percentage.ToString() + "%";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
