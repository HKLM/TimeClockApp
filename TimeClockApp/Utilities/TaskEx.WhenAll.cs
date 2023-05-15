﻿#pragma warning disable MA0042 // Do not use blocking call
#pragma warning disable CA2012 // Use ValueTasks correctly
#nullable enable

using System.Runtime.CompilerServices;

namespace TimeClockApp.Utilities;

/// <summary>
/// Using code by GÉRALD BARRÉ aka. meziantou
/// https://github.com/meziantou/Meziantou.Framework
/// https://www.meziantou.net/get-the-result-of-multiple-tasks-in-a-valuetuple-and-whenall.htm
/// </summary>
public static partial class TaskEx
{
    public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
    {

        ArgumentNullException.ThrowIfNull(task1);

        ArgumentNullException.ThrowIfNull(task2);

        await Task.WhenAll(task1, task2).ConfigureAwait(false);
        return (task1.Result, task2.Result);
    }

    public static TaskAwaiter<(T1, T2)> GetAwaiter<T1, T2>(this (Task<T1>, Task<T2>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2).GetAwaiter();
    }
    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {

        ArgumentNullException.ThrowIfNull(task1);

        ArgumentNullException.ThrowIfNull(task2);

        ArgumentNullException.ThrowIfNull(task3);

        await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result);
    }

    public static TaskAwaiter<(T1, T2, T3)> GetAwaiter<T1, T2, T3>(this (Task<T1>, Task<T2>, Task<T3>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).GetAwaiter();
    }
    public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
    {

        ArgumentNullException.ThrowIfNull(task1);

        ArgumentNullException.ThrowIfNull(task2);

        ArgumentNullException.ThrowIfNull(task3);

        ArgumentNullException.ThrowIfNull(task4);

        await Task.WhenAll(task1, task2, task3, task4).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result);
    }

    public static TaskAwaiter<(T1, T2, T3, T4)> GetAwaiter<T1, T2, T3, T4>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4).GetAwaiter();
    }
    public static async Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
    {

        ArgumentNullException.ThrowIfNull(task1);

        ArgumentNullException.ThrowIfNull(task2);

        ArgumentNullException.ThrowIfNull(task3);

        ArgumentNullException.ThrowIfNull(task4);

        ArgumentNullException.ThrowIfNull(task5);

        await Task.WhenAll(task1, task2, task3, task4, task5).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
    }

    public static TaskAwaiter<(T1, T2, T3, T4, T5)> GetAwaiter<T1, T2, T3, T4, T5>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5).GetAwaiter();
    }
    public static async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
    {

        ArgumentNullException.ThrowIfNull(task1);

        ArgumentNullException.ThrowIfNull(task2);

        ArgumentNullException.ThrowIfNull(task3);

        ArgumentNullException.ThrowIfNull(task4);

        ArgumentNullException.ThrowIfNull(task5);

        ArgumentNullException.ThrowIfNull(task6);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
    }

    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6)> GetAwaiter<T1, T2, T3, T4, T5, T6>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6).GetAwaiter();
    }
    public static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
    {

        ArgumentNullException.ThrowIfNull(task1);

        ArgumentNullException.ThrowIfNull(task2);

        ArgumentNullException.ThrowIfNull(task3);

        ArgumentNullException.ThrowIfNull(task4);

        ArgumentNullException.ThrowIfNull(task5);

        ArgumentNullException.ThrowIfNull(task6);

        ArgumentNullException.ThrowIfNull(task7);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
    }

    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7).GetAwaiter();
    }

    public static TaskAwaiter GetAwaiter(this (Task, Task) tasks)
    {
        return Task.WhenAll(tasks.Item1, tasks.Item2).GetAwaiter();
    }
    public static TaskAwaiter GetAwaiter(this (Task, Task, Task) tasks)
    {
        return Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).GetAwaiter();
    }
    public static TaskAwaiter GetAwaiter(this (Task, Task, Task, Task) tasks)
    {
        return Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4).GetAwaiter();
    }
    public static TaskAwaiter GetAwaiter(this (Task, Task, Task, Task, Task) tasks)
    {
        return Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5).GetAwaiter();
    }
    public static TaskAwaiter GetAwaiter(this (Task, Task, Task, Task, Task, Task) tasks)
    {
        return Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6).GetAwaiter();
    }
    public static TaskAwaiter GetAwaiter(this (Task, Task, Task, Task, Task, Task, Task) tasks)
    {
        return Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7).GetAwaiter();
    }

    public static async ValueTask<(T1, T2)> WhenAll<T1, T2>(ValueTask<T1> task1, ValueTask<T2> task2)
    {
        List<Exception>? observedExceptions = null;
        T1 result1;
        try
        {
            result1 = await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result1);
        }
        T2 result2;
        try
        {
            result2 = await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result2);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }

        return (result1, result2);
    }

    public static ValueTaskAwaiter<(T1, T2)> GetAwaiter<T1, T2>(this (ValueTask<T1>, ValueTask<T2>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2).GetAwaiter();
    }
    public static async ValueTask<(T1, T2, T3)> WhenAll<T1, T2, T3>(ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3)
    {
        List<Exception>? observedExceptions = null;
        T1 result1;
        try
        {
            result1 = await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result1);
        }
        T2 result2;
        try
        {
            result2 = await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result2);
        }
        T3 result3;
        try
        {
            result3 = await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result3);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }

        return (result1, result2, result3);
    }

    public static ValueTaskAwaiter<(T1, T2, T3)> GetAwaiter<T1, T2, T3>(this (ValueTask<T1>, ValueTask<T2>, ValueTask<T3>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).GetAwaiter();
    }
    public static async ValueTask<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4)
    {
        List<Exception>? observedExceptions = null;
        T1 result1;
        try
        {
            result1 = await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result1);
        }
        T2 result2;
        try
        {
            result2 = await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result2);
        }
        T3 result3;
        try
        {
            result3 = await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result3);
        }
        T4 result4;
        try
        {
            result4 = await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result4);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }

        return (result1, result2, result3, result4);
    }

    public static ValueTaskAwaiter<(T1, T2, T3, T4)> GetAwaiter<T1, T2, T3, T4>(this (ValueTask<T1>, ValueTask<T2>, ValueTask<T3>, ValueTask<T4>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4).GetAwaiter();
    }
    public static async ValueTask<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5)
    {
        List<Exception>? observedExceptions = null;
        T1 result1;
        try
        {
            result1 = await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result1);
        }
        T2 result2;
        try
        {
            result2 = await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result2);
        }
        T3 result3;
        try
        {
            result3 = await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result3);
        }
        T4 result4;
        try
        {
            result4 = await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result4);
        }
        T5 result5;
        try
        {
            result5 = await task5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result5);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }

        return (result1, result2, result3, result4, result5);
    }

    public static ValueTaskAwaiter<(T1, T2, T3, T4, T5)> GetAwaiter<T1, T2, T3, T4, T5>(this (ValueTask<T1>, ValueTask<T2>, ValueTask<T3>, ValueTask<T4>, ValueTask<T5>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5).GetAwaiter();
    }
    public static async ValueTask<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6)
    {
        List<Exception>? observedExceptions = null;
        T1 result1;
        try
        {
            result1 = await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result1);
        }
        T2 result2;
        try
        {
            result2 = await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result2);
        }
        T3 result3;
        try
        {
            result3 = await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result3);
        }
        T4 result4;
        try
        {
            result4 = await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result4);
        }
        T5 result5;
        try
        {
            result5 = await task5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result5);
        }
        T6 result6;
        try
        {
            result6 = await task6.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result6);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }

        return (result1, result2, result3, result4, result5, result6);
    }

    public static ValueTaskAwaiter<(T1, T2, T3, T4, T5, T6)> GetAwaiter<T1, T2, T3, T4, T5, T6>(this (ValueTask<T1>, ValueTask<T2>, ValueTask<T3>, ValueTask<T4>, ValueTask<T5>, ValueTask<T6>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6).GetAwaiter();
    }
    public static async ValueTask<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7)
    {
        List<Exception>? observedExceptions = null;
        T1 result1;
        try
        {
            result1 = await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result1);
        }
        T2 result2;
        try
        {
            result2 = await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result2);
        }
        T3 result3;
        try
        {
            result3 = await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result3);
        }
        T4 result4;
        try
        {
            result4 = await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result4);
        }
        T5 result5;
        try
        {
            result5 = await task5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result5);
        }
        T6 result6;
        try
        {
            result6 = await task6.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result6);
        }
        T7 result7;
        try
        {
            result7 = await task7.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
            Unsafe.SkipInit(out result7);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }

        return (result1, result2, result3, result4, result5, result6, result7);
    }

    public static ValueTaskAwaiter<(T1, T2, T3, T4, T5, T6, T7)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7>(this (ValueTask<T1>, ValueTask<T2>, ValueTask<T3>, ValueTask<T4>, ValueTask<T5>, ValueTask<T6>, ValueTask<T7>) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7).GetAwaiter();
    }

    public static async ValueTask WhenAll(ValueTask task1, ValueTask task2)
    {
        List<Exception>? observedExceptions = null;
        try
        {
            await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }
    }

    public static ValueTaskAwaiter GetAwaiter(this (ValueTask, ValueTask) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2).GetAwaiter();
    }
    public static async ValueTask WhenAll(ValueTask task1, ValueTask task2, ValueTask task3)
    {
        List<Exception>? observedExceptions = null;
        try
        {
            await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }
    }

    public static ValueTaskAwaiter GetAwaiter(this (ValueTask, ValueTask, ValueTask) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).GetAwaiter();
    }
    public static async ValueTask WhenAll(ValueTask task1, ValueTask task2, ValueTask task3, ValueTask task4)
    {
        List<Exception>? observedExceptions = null;
        try
        {
            await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }
    }

    public static ValueTaskAwaiter GetAwaiter(this (ValueTask, ValueTask, ValueTask, ValueTask) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4).GetAwaiter();
    }
    public static async ValueTask WhenAll(ValueTask task1, ValueTask task2, ValueTask task3, ValueTask task4, ValueTask task5)
    {
        List<Exception>? observedExceptions = null;
        try
        {
            await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }
    }

    public static ValueTaskAwaiter GetAwaiter(this (ValueTask, ValueTask, ValueTask, ValueTask, ValueTask) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5).GetAwaiter();
    }
    public static async ValueTask WhenAll(ValueTask task1, ValueTask task2, ValueTask task3, ValueTask task4, ValueTask task5, ValueTask task6)
    {
        List<Exception>? observedExceptions = null;
        try
        {
            await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task6.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }
    }

    public static ValueTaskAwaiter GetAwaiter(this (ValueTask, ValueTask, ValueTask, ValueTask, ValueTask, ValueTask) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6).GetAwaiter();
    }
    public static async ValueTask WhenAll(ValueTask task1, ValueTask task2, ValueTask task3, ValueTask task4, ValueTask task5, ValueTask task6, ValueTask task7)
    {
        List<Exception>? observedExceptions = null;
        try
        {
            await task1.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task2.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task3.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task4.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task6.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }
        try
        {
            await task7.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            observedExceptions ??= new();
            observedExceptions.Add(ex);
        }

        if (observedExceptions != null)
        {
            throw new AggregateException(observedExceptions);
        }
    }

    public static ValueTaskAwaiter GetAwaiter(this (ValueTask, ValueTask, ValueTask, ValueTask, ValueTask, ValueTask, ValueTask) tasks)
    {
        return WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6, tasks.Item7).GetAwaiter();
    }
}