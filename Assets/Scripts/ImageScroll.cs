using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Linq;

public class ImageScroll : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private bool _isVerticalLayout = true;
    [SerializeField] private Image _centerImg;
    [SerializeField] private Image _leftImg;
    [SerializeField] private Image _rightImg;

    [SerializeField] private RectTransform _movingBody;

    public static event Action OnDragFinished;
    
    private float _moveTime = 0.1f;
    private float _xImageStep
    {
        get
        {
            if (_isVerticalLayout)
            {
                var mRt = transform as RectTransform;
                var mySizeX = Screen.width + mRt.sizeDelta.x;
                return mySizeX / 3f;
            }
            else
            {
                var mRt = transform as RectTransform;
                var mySizeX = (Screen.width * 0.625f) + Mathf.Abs(mRt.offsetMin.x);
                return mySizeX / 3f;  
            }
        }
    }
    private Vector3 _centerImgStartPos;
    private float _xToChange;
    private float _maxXOffset;
    private Action<ImageScroll, int> callBackDone;
    private bool _isLocked;
    private bool _startPosSaved;

    public void Init(float gapToReset, float maxOffset, float moveTime, Action<ImageScroll, int> callback)
    {
        if(!_startPosSaved)
        {   
            _centerImgStartPos = _movingBody.localPosition;
            _startPosSaved = true;
        }
        _xToChange = gapToReset;
        _moveTime = moveTime;
        _maxXOffset = maxOffset;
        callBackDone = callback;
    }

    private void ResetPosition()
    {
        StartCoroutine(MoveTo(_centerImgStartPos, _moveTime));
    }

    public void SetSprites(Sprite leftSprite, Sprite centerSprite, Sprite rightSprite)
    {
        _leftImg.gameObject.SetActive(leftSprite != null);
        _rightImg.gameObject.SetActive(rightSprite != null);
        _rightImg.sprite = rightSprite;
        _leftImg.sprite = leftSprite;
        _centerImg.sprite = centerSprite;
        _movingBody.localPosition = _centerImgStartPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isLocked)
            return;
        var offset = new Vector3(eventData.delta.x, 0f, 0f);
        _movingBody.localPosition += offset;
        _movingBody.localPosition = new Vector3(
            Mathf.Clamp(_movingBody.localPosition.x, -_maxXOffset, _maxXOffset), 
            _movingBody.localPosition.y, _movingBody.localPosition.z);
    }

    IEnumerator MoveTo(Vector3 targetPos, float totalTime) 
    {
        _isLocked = true;
        var startPos = _movingBody.localPosition;
        var currentTime = 0f;
        while (currentTime <= totalTime)
        {
            _movingBody.localPosition = Vector3.Lerp(startPos, targetPos, currentTime / totalTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        _isLocked = false;
    }

    public TweenerCore<Color, Color, ColorOptions> DOFade(Image target, float endValue, float duration)
    {
        TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration);
        t.SetTarget(target);
        return t;
    }


    private Coroutine _fadeRoutine;
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isLocked)
            return;
        var currentX = _movingBody.localPosition.x;
        var diff = currentX - _centerImgStartPos.x;
        if (Mathf.Abs(diff) < _xToChange)
        {
            ResetPosition();
        }
        else
        {
            var dir = diff > 0 ? -1 : 1;
            //if (_fadeRoutine != null)
            //    StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(ChangeView(dir));
            OnDragFinished?.Invoke();
        }
    }

    private IEnumerator ChangeView(int dir)
    {
        yield return StartCoroutine(MoveTo(_centerImgStartPos - new Vector3(dir == -1 ? -_xImageStep : _xImageStep, 0f, 0f), _moveTime));
        //DOFade(_rightImg, 0f, 0f);
        //DOFade(_leftImg, 0f, 0f);
        callBackDone?.Invoke(this, dir);
        yield return new WaitForSeconds(0.25f);
        //DOFade(_rightImg, 1f, 0.5f);
        //DOFade(_leftImg, 1f, 0.5f);
        //_fadeRoutine = null;
    }
}
