#pragma once

#include "../Baselib_CountdownTimer.h"
#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemFutex.h"
#include "../Baselib_Thread.h"

#if !PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "Only use this implementation on top of a proper futex, in all other situations us Baselib_Sempahore_SemaphoreBased.inl.h"
#endif

// Align to cache line
// the idea here is that threads waking up from sleep should not have to
// access the cache line where count is stored, and only touch wakeups.
// the only exception to that rule is if we hit a timeout.
typedef struct Baselib_CappedSemaphore
{
    ALIGN_AS(PLATFORM_CACHE_LINE_SIZE) int32_t wakeups;
    ALIGN_AS(PLATFORM_CACHE_LINE_SIZE) int32_t count;
    const int32_t cap;
} Baselib_CappedSemaphore;

static inline Baselib_CappedSemaphore Baselib_CappedSemaphore_Create(const uint16_t cap)
{
    Baselib_CappedSemaphore semaphore = {0, 0, cap};
    return semaphore;
}

static inline bool Detail_Baselib_CappedSemaphore_ConsumeWakeup(Baselib_CappedSemaphore* semaphore)
{
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->wakeups);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->wakeups, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

static inline bool Baselib_CappedSemaphore_TryAcquire(Baselib_CappedSemaphore* semaphore)
{
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->count);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&semaphore->count, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

static inline void Baselib_CappedSemaphore_Acquire(Baselib_CappedSemaphore* semaphore)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return;

    while (!Detail_Baselib_CappedSemaphore_ConsumeWakeup(semaphore))
    {
        Baselib_SystemFutex_Wait(&semaphore->wakeups, 0, UINT32_MAX);
    }
}

static inline bool Baselib_CappedSemaphore_TryTimedAcquire(Baselib_CappedSemaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return true;

    if (Detail_Baselib_CappedSemaphore_ConsumeWakeup(semaphore))
        return true;

    uint32_t timeLeft = timeoutInMilliseconds;
    const Baselib_CountdownTimer timer = Baselib_CountdownTimer_StartMs(timeoutInMilliseconds);
    do
    {
        Baselib_SystemFutex_Wait(&semaphore->wakeups, 0, timeLeft);
        if (Detail_Baselib_CappedSemaphore_ConsumeWakeup(semaphore))
            return true;
        timeLeft = Baselib_CountdownTimer_GetTimeLeftInMilliseconds(timer);
    }
    while (timeLeft);

    // When timeout occurs we need to make sure we do one of the following:
    // Increase count by one from a negative value (give our acquired token back) or consume a wakeup.
    //
    // If count is not negative it's likely we are racing with a release operation in which case we
    // may end up having a successful acquire operation.
    do
    {
        int32_t count = Baselib_atomic_load_32_relaxed(&semaphore->count);
        while (count < 0)
        {
            if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->count, &count, count + 1))
                return false;
        }
        // Likely a race, yield to give the release operation room to complete.
        // This includes a fully memory barrier which ensures that there is no reordering between changing/reading count and wakeup consumption.
        Baselib_Thread_YieldExecution();
    }
    while (!Detail_Baselib_CappedSemaphore_ConsumeWakeup(semaphore));
    return true;
}

static inline uint16_t Baselib_CappedSemaphore_Release(Baselib_CappedSemaphore* semaphore,  const uint16_t _count)
{
    int32_t count = _count;
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->count);
    do
    {
        if (previousCount == semaphore->cap)
            return 0;

        if (previousCount + count > semaphore->cap)
            count = semaphore->cap - previousCount;
    }
    while (!Baselib_atomic_compare_exchange_weak_32_release_relaxed(&semaphore->count, &previousCount, previousCount + count));

    if (OPTIMIZER_UNLIKELY(previousCount < 0))
    {
        const int32_t waitingThreads = -previousCount;
        const int32_t threadsToWakeup = count < waitingThreads ? count : waitingThreads;
        Baselib_atomic_fetch_add_32_relaxed(&semaphore->wakeups, threadsToWakeup);
        Baselib_SystemFutex_Notify(&semaphore->wakeups, threadsToWakeup, Baselib_WakeupFallbackStrategy_OneByOne);
    }
    return count;
}

static inline uint32_t Baselib_CappedSemaphore_ResetAndReleaseWaitingThreads(Baselib_CappedSemaphore* semaphore)
{
    const int32_t count = Baselib_atomic_exchange_32_release(&semaphore->count, 0);
    if (OPTIMIZER_LIKELY(count >= 0))
        return 0;
    const int32_t threadsToWakeup = -count;
    Baselib_atomic_fetch_add_32_relaxed(&semaphore->wakeups, threadsToWakeup);
    Baselib_SystemFutex_Notify(&semaphore->wakeups, threadsToWakeup, Baselib_WakeupFallbackStrategy_All);
    return threadsToWakeup;
}

static inline void Baselib_CappedSemaphore_Free(Baselib_CappedSemaphore* semaphore)
{
    if (!semaphore)
        return;
    const int32_t count = Baselib_atomic_load_32_seq_cst(&semaphore->count);
    BaselibAssert(count >= 0, "Destruction is not allowed when there are still threads waiting on the semaphore.");
}
