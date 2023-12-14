using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UniRx;

[RequireComponent(typeof(Image))]
public class DiaryUI : MonoBehaviour
{
    [SerializeField] private PaperMemo prefabPaperMemo = default;
    [SerializeField] private MessageController messageController = default;

    private Vector2 paperSize;
    private List<PaperMemo> diaryList = new List<PaperMemo>();

    private bool isValid = false;
    private PaperMemo currentSelected;

    private static readonly int MAX_COLUMNS = 8;

    void Awake()
    {
        paperSize = prefabPaperMemo.GetComponent<RectTransform>().sizeDelta;
    }

    public void SetDiaries(MessageData[] diaries)
    {
        int length = diaries.Length;
        int rows = length / MAX_COLUMNS + 1;
        int columns = length / rows;

        int span = Screen.width / columns;

        for (int j = 0; j < rows; ++j)
        {
            for (int i = 0; i < columns; ++i)
            {
                int id = i + j * columns;

                if (id >= length) break;
                if (diaries[id] == null) continue;

                diaryList.Add(
                    Instantiate(prefabPaperMemo, transform)
                    .SetID(id + 1)
                    .SetPos(span * (0.5f + i), -paperSize.y * (0.5f + j))
                    .SetMessage(diaries[id]));
            }
        }

        isValid = true;

        Observable.Merge(diaryList.Select(memo => memo.Selected))
            .Subscribe(memo => SetCurrentSelected(memo))
            .AddTo(this);

        Observable.Merge(diaryList.Select(memo => memo.Press))
            .Where(_ => isValid)
            .Subscribe(_ =>
            {
                isValid = false;
                messageController.InputMessageData(currentSelected.diaryData);
                diaryList.ForEach(btn => btn.SetInteractable(false), currentSelected);
                currentSelected.Open();
            })
            .AddTo(this);

        messageController.OnInactive
            .Subscribe(_ =>
            {
                isValid = true;
                diaryList.ForEach(btn => btn.SetInteractable(true));
                currentSelected.Close();
            })
            .AddTo(this);

        SetActive(false);
    }

    private void SetCurrentSelected(PaperMemo selected)
    {
        if (currentSelected != selected) currentSelected?.Deselect();
        currentSelected = selected;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive && diaryList.Count > 0);
    }
}
