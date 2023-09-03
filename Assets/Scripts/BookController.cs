using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum BodyPart
{
    Upper,
    Bottom
}

public class BookController : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private ImageScroll _upperScroller;
    [SerializeField] private ImageScroll _bottomScroller;
    [SerializeField] private Text _bookTextU;
    [SerializeField] private Text _bookTextB;
    [SerializeField] private float _xOffsetToChange = 150f;
    [SerializeField] private float _xMaxOffset = 400f;
    [SerializeField] private float _moveTime = 0.1f;
  
    [SerializeField] private Button _audioButton;
    [SerializeField] private Character[] _characters;


    private GameState _currentGameState;


    public void SetUpView(GameState gameState)
    {
        _currentGameState = gameState;
        _characters = _gameManager.GetBook(gameState.BookID)?.Characters;
        _upperScroller.Init(_xOffsetToChange, _xMaxOffset, _moveTime, OnImageScrolled);
        _bottomScroller.Init(_xOffsetToChange, _xMaxOffset, _moveTime, OnImageScrolled);
        _audioButton.onClick.AddListener(PlayAudio);
        RefreshImages();
        RefreshTexts();
    }

    private void PlayAudio()
    {
        var upperAudio = GetAudioFor(_currentGameState.UpperIdx, BodyPart.Upper);
        var bottomAudio = GetAudioFor(_currentGameState.BottomIdx, BodyPart.Bottom);
        _gameManager.PlayAudio(upperAudio, bottomAudio);
    }

    public void StopAudio()
    {
        _gameManager.StopAudio();
    }
    
    private void OnImageScrolled(ImageScroll source, int dir)
    {
        var isUpper = source == _upperScroller;
        if(isUpper)
        {
            if (dir > 0)
                NextUpper();
            else
                PrevUpper();
        }
        else
        {
            if (dir > 0)
                NextBottom();
            else
                PrevBottom();
        }
        RefreshImages();
        RefreshTexts();
    }

    private void NextUpper()
    {
        _currentGameState.UpperIdx++;
        if (_currentGameState.UpperIdx > _characters.Length - 1)
            _currentGameState.UpperIdx = 0;
        if (_currentGameState.UpperIdx < 0)
            _currentGameState.UpperIdx = _characters.Length - 1;
        StopAudio();
    }

    private void PrevUpper()
    {
        _currentGameState.UpperIdx--;
        if (_currentGameState.UpperIdx > _characters.Length - 1)
            _currentGameState.UpperIdx = 0;
        if (_currentGameState.UpperIdx < 0)
            _currentGameState.UpperIdx = _characters.Length - 1;
        StopAudio();
    }

    private void NextBottom()
    {
        _currentGameState.BottomIdx++;
        if (_currentGameState.BottomIdx > _characters.Length - 1)
            _currentGameState.BottomIdx = 0;
        if (_currentGameState.BottomIdx < 0)
            _currentGameState.BottomIdx = _characters.Length - 1;
        StopAudio();
    }

    private void PrevBottom()
    {
        _currentGameState.BottomIdx--;
        if (_currentGameState.BottomIdx > _characters.Length - 1)
            _currentGameState.BottomIdx = 0;
        if (_currentGameState.BottomIdx < 0)
            _currentGameState.BottomIdx = _characters.Length - 1;
        StopAudio();
    }


    public void Next()
    {
        NextUpper();
        NextBottom();
        RefreshImages();
        RefreshTexts();
    }

    public void Prev()
    {
        PrevUpper();
        PrevBottom();
        RefreshImages();
        RefreshTexts();
    }

    private Sprite GetSpriteFor(Character character, BodyPart bodyPart)
    {
        Sprite data = null;
        var info = _gameManager.BooksConfig.GetCharacter(character);
        if (bodyPart == BodyPart.Upper)
            data = info.SpriteUpper;
        if (bodyPart == BodyPart.Bottom)
            data = info.SpriteBottom;
        return data;
    }

    private AudioClip GetAudioFor(Character character, BodyPart bodyPart)
    {
        AudioClip data = null;
        var info = _gameManager.BooksConfig.GetCharacter(character);
        if (bodyPart == BodyPart.Upper)
            data = info.UpperAudio;
        if (bodyPart == BodyPart.Bottom)
            data = info.BottomAudio;
        return data;
    }

    private string GetTextFor(Character character, BodyPart bodyPart)
    {
        string data = null;
        var info = _gameManager.BooksConfig.GetCharacter(character);
        if (bodyPart == BodyPart.Upper)
            data = info.TextUpper;
        if (bodyPart == BodyPart.Bottom)
            data = info.TextBottom;
        return data;
    }

    private Sprite GetSpriteFor(int idx, BodyPart body)
    {
        var character = _characters[idx];
        return GetSpriteFor(character, body);
    }

    private string GetTextFor(int idx, BodyPart body)
    {
        var character = _characters[idx];
        return GetTextFor(character, body);
    }

    private AudioClip GetAudioFor(int idx, BodyPart body)
    {
        var character = _characters[idx];
        return GetAudioFor(character, body);
    }

    private void RefreshImages()
    {
        var upperPrev = GetSpriteFor(_currentGameState.UpperIdx - 1 < 0 ? _characters[_characters.Length - 1] : _characters[_currentGameState.UpperIdx - 1], BodyPart.Upper);
        var upperCenter = GetSpriteFor(_currentGameState.UpperIdx, BodyPart.Upper);
        var upperNext = GetSpriteFor(_currentGameState.UpperIdx + 1 > _characters.Length - 1 ? _characters[0] : _characters[_currentGameState.UpperIdx + 1], BodyPart.Upper);
        _upperScroller.SetSprites(upperPrev, upperCenter, upperNext);

        var bottomPrev = GetSpriteFor(_currentGameState.BottomIdx - 1 < 0 ? _characters[_characters.Length - 1] : _characters[_currentGameState.BottomIdx - 1], BodyPart.Bottom);
        var bottom = GetSpriteFor(_currentGameState.BottomIdx, BodyPart.Bottom);
        var bottomNext = GetSpriteFor(_currentGameState.BottomIdx + 1 > _characters.Length - 1 ? _characters[0] : _characters[_currentGameState.BottomIdx + 1],
            BodyPart.Bottom);
        _bottomScroller.SetSprites(bottomPrev, bottom, bottomNext);
    }

    private void RefreshTexts()
    {
        var builderU = new StringBuilder();
        var builderB = new StringBuilder();

        var upperCenter = GetTextFor(_currentGameState.UpperIdx, BodyPart.Upper);
        var byTwoLinesU = upperCenter.Split('~');
        builderU.Append(byTwoLinesU[0]);
        builderU.Append("\n");
        builderU.Append(byTwoLinesU[1]);

        var bottom = GetTextFor(_currentGameState.BottomIdx, BodyPart.Bottom);
        var byTwoLinesB = bottom.Split('~');
        builderB.Append(byTwoLinesB[0]);
        builderB.Append("\n");
        builderB.Append(byTwoLinesB[1]);

        _bookTextU.text = builderU.ToString();
        _bookTextB.text = builderB.ToString();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Prev();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Next();
        }
    }
}
