﻿using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CanvasOffsetXPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    RectTransform verticalLineRect;

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        // Initialize canvas offset x
        model.OnLoadedMusicObservable.Subscribe(_ => model.CanvasOffsetX.Value = -Screen.width * 0.45f * model.CanvasScaleFactor.Value);

        this.UpdateAsObservable()
            .SkipUntil(canvasEvents.VerticalLineOnMouseDownObservable)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition.x)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Select(b => (b[1] - b[0]) * model.CanvasScaleFactor.Value)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Select(x => new { x, max = Screen.width * 0.5f * 0.95f * model.CanvasScaleFactor.Value })
            .Select(v => Mathf.Clamp(v.x, -v.max, v.max))
            .Subscribe(x => model.CanvasOffsetX.Value = x);

        model.CanvasOffsetX.DistinctUntilChanged().Subscribe(x =>
        {
            var pos = verticalLineRect.localPosition;
            pos.x = x;
            verticalLineRect.localPosition = pos;
        });
    }
}