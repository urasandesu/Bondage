/* 
 * File: IObservableMixin.cs
 * 
 * Author: Akira Sugiura (urasandesu@gmail.com)
 * 
 * 
 * Copyright (c) 2017 Akira Sugiura
 *  
 *  This software is MIT License.
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */



using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Urasandesu.Bondage.Application.Mixins.System
{
    public static class IObservableMixin
    {
        public static IObservable<TOut> Drain<TSource, TOut>(this IObservable<TSource> source, Func<TSource, IObservable<TOut>> selector)
        {
            return Observable.Defer(() =>
            {
                var queue = new BehaviorSubject<Unit>(new Unit());
                return source.Zip(queue, (v, q) => v).SelectMany(v => selector(v).Do(_ => { }, () => queue.OnNext(new Unit())));
            });
        }

        public static IObservable<TSource> DelayUntilNextSchedule<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        {

            return source.Drain(_ => Observable.Empty<TSource>().Delay(dueTime).StartWith(_));
        }

        public static IObservable<TSource> DelayUntilNextSchedule<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime)
        {
            return source.Drain(_ => Observable.Empty<TSource>().Delay(dueTime).StartWith(_));
        }

        public static IObservable<TSource> DelayUntilNextSchedule<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
        {
            return source.Drain(_ => Observable.Empty<TSource>().Delay(dueTime, scheduler).StartWith(_));
        }

        public static IObservable<TSource> DelayUntilNextSchedule<TSource>(this IObservable<TSource> source, Func<TSource, TimeSpan> delayDurationSelector)
        {
            if (delayDurationSelector == null)
                throw new ArgumentNullException(nameof(delayDurationSelector));
            return source.Drain(_ => Observable.Empty<TSource>().Delay(delayDurationSelector(_)).StartWith(_));
        }

        public static IObservable<TSource> DelayUntilNextSchedule<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
        {
            return source.Drain(_ => Observable.Empty<TSource>().Delay(dueTime, scheduler).StartWith(_));
        }
    }
}
