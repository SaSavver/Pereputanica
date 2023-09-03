using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BookID
{
    AnimalBook,
    ThingsBook,
    JobsBook,
    TransportBook
}


[System.Serializable]
public class Book
{
    public BookID BookID;

    public Character[] Characters;

    public bool IsAvailable;

    public BookView ViewH;
    public BookView ViewV;

    public Action<Book> OnBuyBook;

    public string BookPrefsKey => $"{BookID.ToString()}_Available";

    public void InitBook()
    {
        if (!PlayerPrefs.HasKey(BookPrefsKey))
            PlayerPrefs.SetInt(BookPrefsKey, IsAvailable ? 1 : 0);

        ViewH.OnPurchaseButtonClicked += () => OnBuyBook?.Invoke(this);
        ViewV.OnPurchaseButtonClicked += () => OnBuyBook?.Invoke(this);
    }

    public void SetPrice(string price)
    {
        ViewH.SetPrice(price);
        ViewV.SetPrice(price);
    }

    public void ChangeBookState(bool isAvailable)
    {
        IsAvailable = isAvailable;
        PlayerPrefs.SetInt(BookPrefsKey, isAvailable ? 1 : 0);

        ViewH.ChangeBookState(isAvailable);
        ViewV.ChangeBookState(isAvailable);
    }
}
