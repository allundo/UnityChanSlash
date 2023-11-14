using UnityEngine;
using UniRx;
using System;
using System.Collections;
using TMPro;

public class MessageControllerTest : MessageController
{
    protected Coroutine messageLoopCoroutine;
    protected ISubject<Unit> closeSubject = new Subject<Unit>();
    protected TextMeshProUGUI tmTitle;
    protected TextMeshProUGUI tmSentence;

    public string title => tmTitle.text;
    public string sentence => tmSentence.text;

    protected override void Awake()
    {
        base.Awake();
        tmTitle = window.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        tmSentence = window.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    protected override void Inactivator()
    {
        closeSubject.OnNext(Unit.Default);
        base.Inactivator();
    }

    public IObservable<Unit> AutoReadMessageData(MessageData data, float readSecPerLiteral = 0.1f)
    {
        if (messageLoopCoroutine != null) StopCoroutine(messageLoopCoroutine);
        messageLoopCoroutine = StartCoroutine(MessageLoop(data, readSecPerLiteral));

        return closeSubject.SelectMany(_ =>
        {
            StopCoroutine(messageLoopCoroutine);
            return Observable.Return(Unit.Default);
        }).First();
    }

    public IObservable<Unit> AutoReadMessageData(BloodMessageData data, float readSecPerLiteral = 0.1f)
    {
        if (messageLoopCoroutine != null) StopCoroutine(messageLoopCoroutine);
        messageLoopCoroutine = StartCoroutine(MessageLoop(data, readSecPerLiteral));

        return closeSubject.SelectMany(_ =>
        {
            StopCoroutine(messageLoopCoroutine);
            return Observable.Return(Unit.Default);
        }).First();
    }

    public IEnumerator MessageLoop(MessageData data, float readSecPerLiteral = 0.1f)
    {
        InputMessageData(data);
        yield return null;

        yield return new WaitForSeconds(0.5f);

        foreach (var mes in data.Source)
        {
            var duration = mes.sentence.Length / mes.literalsPerSec;
            var readTime = mes.sentence.Length * readSecPerLiteral;
            yield return new WaitForSeconds(readTime);

            OnPointerUp(null);
            yield return null;

            // Full sentence is displayed by a tap simulation.
            if (duration > readTime)
            {
                yield return new WaitForSeconds(readTime);
                // Tap to next message.
                OnPointerUp(null);
            }
        }
    }
}