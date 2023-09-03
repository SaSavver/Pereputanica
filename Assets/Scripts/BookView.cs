using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookView : MonoBehaviour
{
    [SerializeField] private GameObject _closedFade;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _buyButton;

    [SerializeField] private Button _buyButtonField;

    [SerializeField] private TextMeshProUGUI _priceText;

    public Action OnPurchaseButtonClicked;

    private void Awake()
    {
        _buyButtonField.onClick.AddListener(BuyButtonClicked);
    }

    public void ChangeBookState(bool isAvailable)
    {
        _closedFade.SetActive(!isAvailable);
        _lock.SetActive(!isAvailable);
        _buyButton.SetActive(!isAvailable);
        _buyButtonField.gameObject.SetActive(!isAvailable);
        _priceText.gameObject.SetActive(!isAvailable);
    }

    public void SetPrice(string price)
    {
        _priceText.text = price;
    }

    void BuyButtonClicked()
    {
        OnPurchaseButtonClicked?.Invoke();
    }
}
