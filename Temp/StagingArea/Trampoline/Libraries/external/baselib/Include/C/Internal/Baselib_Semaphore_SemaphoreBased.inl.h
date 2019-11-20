#pragma once

#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemSemaphore.h"
#include "../Baselib_Thread.h"

#if PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "It's highly recommended to use Baselib_Semaphore_FutexBased.inl.h on platforms which has native semaphore support"
#endif

typedef struct Baselib_Semaphore
{
    ALIGN_AS(PLATFORM_CACHE_LINE_SIZE) int32_t count;
    const Baselib_SystemSemaphore_Handle handle;
} Baselib_Semaphore;

static inline Baselib_Semaphore Baselib_Semaphore_Create(void)
{
    Baselib_Semaphore semaphore = {0, Baselib_SystemSemaphore_Create()};
    return semaphore;
}

static inline bool Baselib_Semaphore_TryAcquire(Baselib_Semaphore* semaphore)
{
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->count);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&semaphore->count, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

static inline void Baselib_Semaphore_Acquire(Baselib_Semaphore* semaphore)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return;

    Baselib_SystemSemaphore_Acquire(semaphore->handle);
}

static inline bool Baselib_Semaphore_TryTimedAcquire(Baselib_Semaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return true;

    if (OPTIMIZER_LIKELY(Baselib_SystemSemaphore_TryTimedAcquire(semaphore->handle, timeoutInMilliseconds)))
        return true;

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
    while (!Baselib_SystemSemaphore_TryAcquire(semaphore->handle));
    return true;
}

static inline void Baselib_Semaphore_Release(Baselib_Semaphore* semaphore, const uint16_t _count)
{
    const int32_t count = _count;
    int32_t previousCount = Baselib_atomic_fetch_add_32_release(&semaphore->count, count);

    // This should only be possible if thousands of threads enter this function simultaneously posting with a high count.
    // See overflow protection below.
    BaselibAssert(previousCount <= (previousCount + count), "Semaphore count overflow (current: %d, added: %d).", previousCount, count);

    if (OPTIMIZER_UNLIKELY(previousCount < 0))
    {
        const int32_t waitingThreads = -previousCount;
        const int32_t threadsToWakeup = count < waitingThreads ? count : waitingThreads;
        Baselib_SystemSemaphore_Release(semaphore->handle, threadsToWakeup);
        return;
    }

    // overflow protection
    // we clamp count to MaxGuaranteedCount when count exceed MaxGuaranteedCount * 2
    // this way we won't have to do clamping on every iteration
    while (OPTIMIZER_UNLIKELY(previousCount > Baselib_Semaphore_MaxGuaranteedCount * 2))
    {
        const int32_t maxCount = Baselib_Semaphore_MaxGuaranteedCount;
        if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->count, &previousCount, maxCount))
            return;
    }
}

static inline uint32_t Baselib_Semaphore_ResetAndReleaseWaitingThreads(Baselib_Semaphore* semaphore)
{
    const int32_t count = Baselib_atomic_exchange_32_release(&semaphore->count, 0);
    if (OPTIMIZER_LIKELY(count >= 0))
        return 0;
    const int32_t threadsToWakeup = -count;
    Baselib_SystemSemaphore_Release(semaphore->handle, threadsToWakeup);
    return threadsToWakeup;
}

static inline void Baselib_Semaphore_Free(Baselib_Semaphore* semaphore)
{
    if (!semaphore)
        return;
    const int32_t count = Baselib_atomic_load_32_seq_cst(&semaphore->count);
    BaselibAssert(count >= 0, "Destruction is not allowed when there are still threads waiting on the semaphore.");
    Baselib_SystemSemaphore_Free(semaphore->handle);
}
