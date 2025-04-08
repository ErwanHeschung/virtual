using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    public int foodCount = 0;
    public TMP_Text foodCountText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            UpdateUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddFood()
    {
        foodCount++;
        UpdateUI();
    }

    public bool RemoveFood(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Amount to remove must be greater than zero.");
            return false;
        }
        foodCount -= amount;
        if (foodCount < 0)
        {
            foodCount = 0;
        }
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (foodCountText != null)
        {
            foodCountText.text = "Food: " + foodCount;
        }
    }
}