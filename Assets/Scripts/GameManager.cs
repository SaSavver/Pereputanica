using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BooksConfig booksConfig;
    [SerializeField] private Purchaser _purchaser;
    [SerializeField] private ShowAdvertManager _adsManager;
    [SerializeField] private int _exitToShowAds = 2;
    private int _toShowAdsRemains;
    private Coroutine _playAudioRoutine;

    [SerializeField]
    private GameObject _mainMenuH;
    [SerializeField]
    private GameObject _mainMenuV;

    [SerializeField] private Button _restoreButtonH;
    [SerializeField] private Button _restoreButtonV;

    [SerializeField]
    private Book[] _books;

    [SerializeField] private GameObject _horizontalCanvas;
    [SerializeField] private GameObject _varticalCanvas;
    [SerializeField] private BookController _verticalView;
    [SerializeField] private BookController _horizontalView;
    [SerializeField] private Button _hBack;
    [SerializeField] private Button _vBack;
    [SerializeField] private AudioSource _audioPoint;

    [SerializeField] private Button _vAudio;
    [SerializeField] private Button _hAudio;

    [SerializeField] private TutorialManager _tutorialManager;

    public static event Action OnSoundPlayed;
    public static event Action OnExitToMainMenu;

    private GameState _currentState;
    public GameState State => _currentState;

    public BooksConfig BooksConfig => booksConfig;
    public Book GetBook(BookID bookId) => _books.FirstOrDefault(book => book.BookID == bookId);

    private ScreenOrientation screenOrientation;

    private List<ScreenOrientation> _hOrientations = new List<ScreenOrientation>()
    {
        ScreenOrientation.LandscapeLeft,
        ScreenOrientation.LandscapeRight,
        ScreenOrientation.LandscapeLeft
    };

    private List<ScreenOrientation> _vOrientations = new List<ScreenOrientation>()
    {
        ScreenOrientation.Portrait,
        ScreenOrientation.PortraitUpsideDown
    };

    private bool _tutorialIsInited;

    public static event Action<ScreenOrientation> OnOrientaitonChanged;


    void Awake()
    {
        _currentState = new GameState();
        foreach (var book in _books)
        {
            book.InitBook();
            book.ViewH.GetComponent<Button>().onClick.AddListener(() => OpenBook(book));
            book.ViewV.GetComponent<Button>().onClick.AddListener(() => OpenBook(book));
            _hBack.onClick.AddListener(CloseBook);
            _vBack.onClick.AddListener(CloseBook);
            book.ChangeBookState(CheckBookAvailability(book));
            book.OnBuyBook += TryBuyBook;
        }
        TutorialManager.StartBookForTutorial += OpenBookForTutorial;
        TutorialManager.ReturnToMainScreen += GoToMainScreenForTutorial;
#if UNITY_ANDROID
        _restoreButtonH.gameObject.SetActive(false);
        _restoreButtonV.gameObject.SetActive(false);
#else
       _restoreButtonH.gameObject.SetActive(true);
        _restoreButtonV.gameObject.SetActive(true);
#endif

        _restoreButtonH.onClick.AddListener(RestorePurchasesIOS);
        _restoreButtonV.onClick.AddListener(RestorePurchasesIOS);
        _purchaser.OnBookPurchased += OnBookPurchased;

        SetPrices();
        _toShowAdsRemains = _exitToShowAds;
    }

    private void RestorePurchasesIOS()
    {
        _purchaser.RestorePurchases();
    }

    private void OnDestroy()
    {
        TutorialManager.StartBookForTutorial -= OpenBookForTutorial;
        TutorialManager.ReturnToMainScreen -= GoToMainScreenForTutorial;
    }

    private void OpenBookForTutorial()
    {
        OpenBook(_books.First());
    }

    private void GoToMainScreenForTutorial()
    {
        CloseBook();
    }


    private void Update()
    {
        if(_hOrientations.Contains(Screen.orientation))
        {
            OnOrientaitonChanged?.Invoke(ScreenOrientation.LandscapeLeft);
            SetUiFor(ScreenOrientation.LandscapeLeft);
        }
        else if(_vOrientations.Contains(Screen.orientation))
        {
            OnOrientaitonChanged?.Invoke(ScreenOrientation.Portrait);
            SetUiFor(ScreenOrientation.Portrait);
        }
        if(Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKey(KeyCode.Escape))
            {
                CloseBook();
            }
        }
    }

    private void SetPrices()
    {
        foreach (var book in _books)
        {
            var bookData = booksConfig.GetData(book.BookID);
#if UNITY_ANDROID
            book.SetPrice(bookData.PriceAndroid);
#else
            book.SetPrice(bookData.PriceIOS);
#endif
        }

    }

    private void SetUiFor(ScreenOrientation orientation)
    {
        if(screenOrientation != orientation)
        {
            var isH = orientation == ScreenOrientation.LandscapeLeft;
            _horizontalCanvas.SetActive(orientation == ScreenOrientation.LandscapeLeft);
            _varticalCanvas.SetActive(orientation == ScreenOrientation.Portrait);
            if (isH)
                _horizontalView.SetUpView(State);
            else
                _verticalView.SetUpView(State);
            screenOrientation = orientation;
            if (!_tutorialIsInited)
            {
                _tutorialManager.TryToStart();
                _tutorialIsInited = true;
            }
        }
    }

    void OpenBook(Book book)
    {
        if (!CheckBookAvailability(book))
            return;

        _currentState.BookID = book.BookID;
        _mainMenuH.gameObject.SetActive(false);
        _mainMenuV.gameObject.SetActive(false);
        _verticalView.gameObject.SetActive(true);
        _horizontalView.gameObject.SetActive(true);
        _verticalView.SetUpView(_currentState);
        _horizontalView.SetUpView(_currentState);
    }

    void CloseBook()
    {
        OnExitToMainMenu?.Invoke();
        _mainMenuH.gameObject.SetActive(true);
        _mainMenuV.gameObject.SetActive(true);
        _verticalView.gameObject.SetActive(false);
        _horizontalView.gameObject.SetActive(false);
        _currentState.BottomIdx = _currentState.UpperIdx = 0;
        StopAudio();
        _toShowAdsRemains--;
        if(_toShowAdsRemains <= 0 && !_purchaser.AnyPurchase)
        {
            _adsManager.ShowIntersitial();
            _toShowAdsRemains = _exitToShowAds;
        }
    }

    public void PlayAudio(AudioClip clipUpper, AudioClip clipBottom)
    {
        OnSoundPlayed?.Invoke();
        if (_audioPoint.isPlaying || _audioPoint != null)
        {
            StopAudio();
        }
        _playAudioRoutine = StartCoroutine(PlaySpeech(clipUpper, clipBottom));
    }

    public void StopAudio()
    {
        if(_playAudioRoutine != null)
            StopCoroutine(_playAudioRoutine);
        _audioPoint.Stop();
        _vAudio.interactable = true;
        _hAudio.interactable = true;
    }

    private IEnumerator PlaySpeech(AudioClip clipUpper, AudioClip clipBottom)
    {
        _vAudio.interactable = false;
        _hAudio.interactable = false;
        _audioPoint.clip = clipUpper;
        _audioPoint.Play();
        yield return new WaitForSeconds(clipUpper.length);
        _audioPoint.clip = clipBottom;
        _audioPoint.Play();
        yield return new WaitForSeconds(clipBottom.length);
        _playAudioRoutine = null;
        _vAudio.interactable = true;
        _hAudio.interactable = true;
    }


    void TryBuyBook(Book book)
    {
        if (CheckBookAvailability(book))
            return;

        _purchaser.BuyBook(book.BookID);
    }

    private void OnBookPurchased(BookPurchase bookData)
    {
        var book = _books.FirstOrDefault(book => book.BookID == bookData.BookID);
        if (book != null)
        {
            book.ChangeBookState(true);
        }
    }

    bool CheckBookAvailability(Book book)
    {
        return PlayerPrefs.GetInt(book.BookPrefsKey) == 1 ? true : false;
    }
}

public class GameState
{
    public BookID BookID;
    public int UpperIdx;
    public int BottomIdx;
}