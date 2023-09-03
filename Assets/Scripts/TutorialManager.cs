using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialState
{
    NotActive,
    Scroll,
    Sound,
    RotateScreen,
    Exit
}

[System.Serializable]
public class TutorialStep
{
    public TutorialState State;
    public GameObject StepWindow;
    public GameObject DoneMark;
}

public class TutorialManager : MonoBehaviour
{
    private int _tutorialsPassed;

    [SerializeField]
    private TutorialStep[] _tutorialStepsV;

    [SerializeField]
    private TutorialStep[] _tutorialStepsH;

    private TutorialStep[] _tutorialSteps;

    [SerializeField]
    private Button[] _skipButtons;
    [SerializeField]
    private Button[] FinishTutorButtons;

    [SerializeField]
    private Canvas _tutorialCanvasV;
    [SerializeField]
    private Canvas _tutorialCanvasH;

    [SerializeField]
    private TutorialState _currentState;

    private bool _skipAll;

    public static event Action StartBookForTutorial;
    public static event Action ReturnToMainScreen;

    private ScreenOrientation _currentScreenOrientation;
    private bool isInited;

    private void Awake()
    {
        GameManager.OnOrientaitonChanged += OnOrientationChanged;
    }

    public void TryToStart()
    {
        //todo: ����� �������� �� ����� ���� ������������ � �������� ��!!!!!
        _tutorialsPassed = PlayerPrefs.GetInt("tutorialsPassed");
        _skipAll = PlayerPrefs.GetInt("tutorialsSkip") == 1 ? true : false;
        if(_skipAll || _tutorialsPassed >= _tutorialSteps.Length)
        {
            var allTuts = _tutorialStepsH.Concat(_tutorialStepsV);
            foreach (var tutor in allTuts)
            {
                tutor.StepWindow.SetActive(false);
                tutor.DoneMark.SetActive(false);
            }
            return;
        }

        TutorialState tutorToStart = TutorialState.Scroll;
        switch (_tutorialsPassed)
        {
            case 0:
                StartBookForTutorial?.Invoke();
                break;
            case 1:
                tutorToStart = TutorialState.Sound;
                StartBookForTutorial?.Invoke();
                break;
            case 2:
                tutorToStart = TutorialState.Exit;
                StartBookForTutorial?.Invoke();
                break;
            case 3:
                tutorToStart = TutorialState.RotateScreen;
                break;
        }
        OpenTutorial(tutorToStart);

        ImageScroll.OnDragFinished += OnTutorialEnded;
        GameManager.OnSoundPlayed += OnTutorialEnded;
        GameManager.OnExitToMainMenu += OnTutorialEnded;

        foreach (var button in FinishTutorButtons) 
        {
            button.onClick.AddListener(OnTutorialEnded);
        }

        foreach (var skipButton in _skipButtons)
        {
            skipButton.onClick.AddListener(SkipAll);
        }
    }

    private void DropSubscriptions()
    {
        ImageScroll.OnDragFinished -= OnTutorialEnded;
        GameManager.OnSoundPlayed -= OnTutorialEnded;
        GameManager.OnExitToMainMenu -= OnTutorialEnded;
        GameManager.OnOrientaitonChanged -= OnOrientationChanged;
        foreach (var button in FinishTutorButtons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void OpenTutorial(TutorialState state)
    {
        var step = _tutorialSteps.First(s => s.State == state);
        foreach(var tutor in _tutorialSteps)
        {
            tutor.StepWindow.SetActive(tutor == step);
            tutor.DoneMark.SetActive(false);
        }
        var canvasGroup = step.StepWindow.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        DOFade(canvasGroup, 1f, 1f);
        _currentState = state;
    }

    private IEnumerator TutorialDone(TutorialState state)
    {
        switch (state)
        {
            case TutorialState.Scroll:
                PlayerPrefs.SetInt("tutorialsPassed", 1);
                break;
            case TutorialState.Sound:
                PlayerPrefs.SetInt("tutorialsPassed", 2);
                break;
            case TutorialState.Exit:
                PlayerPrefs.SetInt("tutorialsPassed", 3);
                break;
            case TutorialState.RotateScreen:
                PlayerPrefs.SetInt("tutorialsPassed", 4);
                break;
        }
        var step = _tutorialSteps.First(s => s.State == state);
        step.DoneMark.SetActive(true);
        yield return new WaitForSeconds(1f);
        var canvasGroup = step.StepWindow.GetComponent<CanvasGroup>();
        DOFade(canvasGroup, 0f, 1f);
        yield return new WaitForSeconds(1f);
        step.StepWindow.SetActive(false);
        switch(state)
        {
            case TutorialState.Scroll:
                OpenTutorial(TutorialState.Sound);
                break;
            case TutorialState.Sound:
                OpenTutorial(TutorialState.Exit);
                break;
            case TutorialState.Exit:
                OpenTutorial(TutorialState.RotateScreen);
                break;
            case TutorialState.RotateScreen:
                _currentState = TutorialState.NotActive;
                DropSubscriptions();
                break;
        }

    }

    private IEnumerator ClearTutorial()
    {
        var step = _tutorialSteps.First(s => s.State == _currentState);
        var canvasGroup = step.StepWindow.GetComponent<CanvasGroup>();
        DOFade(canvasGroup, 0f, 1f);
        yield return new WaitForSeconds(1f);
        step.StepWindow.SetActive(false);
        DropSubscriptions();
        ReturnToMainScreen?.Invoke();
    }

    private void SkipAll()
    {
        PlayerPrefs.SetInt("tutorialsSkip", 1);
        StartCoroutine(ClearTutorial());
    }

    void OnTutorialEnded()
    {
        StartCoroutine(TutorialDone(_currentState));
    }

    public TweenerCore<float, float, FloatOptions> DOFade(CanvasGroup target, float endValue, float duration)
    {
        TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.alpha, x => target.alpha = x, endValue, duration);
        t.SetTarget(target);
        return t;
    }

    void OnOrientationChanged(ScreenOrientation orientation)
    {
        if (_currentScreenOrientation == orientation && isInited)
            return;
        _currentScreenOrientation = orientation;
        if (!isInited)
            isInited = true;
        if (orientation == ScreenOrientation.LandscapeLeft)
        {
            _tutorialSteps = _tutorialStepsH;
        }
        else if (orientation == ScreenOrientation.Portrait)
        {
            _tutorialSteps = _tutorialStepsV;
        }

        _tutorialCanvasV.gameObject.SetActive(orientation == ScreenOrientation.Portrait);
        _tutorialCanvasH.gameObject.SetActive(orientation == ScreenOrientation.LandscapeLeft);

        if (_currentState == TutorialState.NotActive)
            return;
        OpenTutorial(_currentState);
    }
}
