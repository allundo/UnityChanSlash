using UnityEngine;
using UniRx;
using System;
using System.Collections;

public class MessageControllerTest : MessageController
{
    protected Coroutine messageLoopCoroutine;
    protected ISubject<Unit> closeSubject = new Subject<Unit>();
    protected override void Inactivator()
    {
        closeSubject.OnNext(Unit.Default);
        base.Inactivator();
    }

    public IObservable<Unit> AutoReadMessageData(MessageData[] data, float readSecPerLiteral = 0.1f)
    {
        messageLoopCoroutine = StartCoroutine(MessageLoop(data, readSecPerLiteral));

        return closeSubject.SelectMany(_ =>
        {
            StopCoroutine(messageLoopCoroutine);
            return Observable.Return(Unit.Default);
        }).First();
    }

    public IEnumerator MessageLoop(MessageData[] data, float readSecPerLiteral = 0.1f)
    {
        InputMessageData(data);
        yield return null;

        // Resume pausing not to affect the other testings.
        TimeManager.Instance.Resume(false);
        yield return new WaitForSeconds(0.5f);

        foreach (var mes in data)
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