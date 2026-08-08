using UnityEngine;
using UnityEngine.UI;

public class health : MonoBehaviour
{
    [SerializeField]
    private PlayerScript player;

    [SerializeField] private Image[] hearts;

    private void Start()
    {
        UpdateHPUI();
    }
    private void Update()
    {
        UpdateHPUI();
    }
    private void UpdateHPUI()
    {
        if (player == null)
            return;

        int currentHP = player.CurrentHP;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            // HPが残っているハートだけ表示
            hearts[i].enabled = i < currentHP;
        }
    }
}
