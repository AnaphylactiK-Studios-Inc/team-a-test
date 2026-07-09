using UnityEngine;
using UnityEngine.UI;

public class PlayerBody : MonoBehaviour
{
    public int playerHealth = 100;
    public int playerShield = 100;
    public int playerMaxHealth = 100;
    public int playerMaxShield = 100;
    [SerializeField] Image healthBarFill;
    [SerializeField] Image shieldBarFill;
    [SerializeField] GameObject gameOverScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        playerHealth = playerMaxHealth;
        playerShield = playerMaxShield;

        gameOverScreen.SetActive(false);
    }
    public void TakeDamage(float amount)
    {
        // damage shield first
        if (playerShield > 0)
        {
            if (amount > playerShield)
            {
                float leftoverDamage = amount - playerShield;
                playerShield = 0;
                if (shieldBarFill != null)
                    shieldBarFill.fillAmount = 0f;

                playerHealth = Mathf.Max(0, playerHealth - (int)leftoverDamage);
                if (healthBarFill != null)
                    healthBarFill.fillAmount = (float)playerHealth / playerMaxHealth;
            }
            else
            {
                playerShield = Mathf.Max(0, playerShield - (int)amount);
                if (shieldBarFill != null)
                    shieldBarFill.fillAmount = (float)playerShield / playerMaxShield;
            }
        }
        else
        {
            playerHealth = Mathf.Max(0, playerHealth - (int)amount);

            if (healthBarFill != null)
                healthBarFill.fillAmount = (float)playerHealth / playerMaxHealth;
        }


        if (playerHealth <= 0)
            GameOver();
    }

    public void Heal(float amount)
    {
        playerHealth = Mathf.Min(playerMaxHealth, playerHealth + (int)amount);
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)playerHealth / playerMaxHealth;
    }

    public void ReplenishShield(float amount)
    {
        playerShield = Mathf.Min(playerMaxShield, playerShield + (int)amount);
        if (shieldBarFill != null)
            shieldBarFill.fillAmount = (float)playerShield / playerMaxShield;
    }

    void GameOver()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
    }
}
