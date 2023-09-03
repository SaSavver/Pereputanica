using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Character
{
    Slon = 0,
    Lisa = 1,
    Krokodil = 2,
    Kit = 3,
    Zaec = 4,
    Kozel = 5,
    Petuschara = 6,
    Kot = 7,
    Volchara = 8,
    Medved = 9,
    Pes = 10,
    Stroitel = 11,
    Kosmonavt = 12,
    Hokeist = 13,
    Sadovnik = 15,
    Vodolaz = 16,
    Vrach = 17,
    Povar = 18,
    Ment = 19,
    Kloun = 20,
    Malar = 21,
    Pozharnui = 22,
    Telephon = 23,
    Chainik = 24,
    Sapog,
    Vaza,
    Kruzhka,
    Kaktus,
    Chasu,
    Zont,
    Tort,
    Akvarium,
    Gitara,
    Kolaska,
    Tramvai,
    Velosiped,
    Shar,
    Korabl,
    Traktor,
    Poezd,
    Samosval,
    Vertolet,
    Mashina,
    Raketa
}

[CreateAssetMenu(fileName = "BooksConfig", menuName = "SavvaGames/BooksConfig")]
public class BooksConfig : ScriptableObject
{
    public List<BookCharacter> BookCharacters = new List<BookCharacter>();

    public List<BookData> BookDatas = new List<BookData>();

    public BookCharacter GetCharacter(Character character) => BookCharacters.FirstOrDefault(book => book.Character == character);

    public BookData GetData(BookID book) => BookDatas.FirstOrDefault(bookD => bookD.Book == book);
}

[System.Serializable]
public class BookData
{
    public BookID Book;
    public string PriceIOS;
    public string PriceAndroid;
}


[System.Serializable]
public class BookCharacter
{
    public Character Character;
    public Sprite SpriteUpper;
    public Sprite SpriteBottom;
    public string TextUpper;
    public string TextBottom;
    public AudioClip UpperAudio;
    public AudioClip BottomAudio;
}