#pragma once

// In computer science, a lock or mutex (from mutual exclusion) is a synchronization mechanism for enforcing limits on access to a resource in an environment
// where there are many threads of execution. A lock is designed to enforce a mutual exclusion concurrency control policy.
//
// "Lock (computer science)", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Lock_(computer_science)&oldid=875674239

#if PLATFORM_FUTEX_NATIVE_SUPPORT
#include "Internal/Baselib_Lock_FutexBased.inl.h"
#else
#include "Internal/Baselib_Lock_SemaphoreBased.inl.h"
#endif

// Creates a lock synchronization primitive.
//
// If there are not enough system resources to create a lock, process abort is triggered.
//
// \returns          A struct representing a lock instance. Use Baselib_Lock_Free to free the lock.
static inline Baselib_Lock Baselib_Lock_Create(void);

// Try to acquire lock and return immediately.
//
// If lock is held, either by this or another thread, then lock is not acquired and function return false.
//
// If successful this function is guaranteed to emit an acquire barrier.
//
// \returns          true if lock was acquired.
COMPILER_WARN_UNUSED_RESULT
static inline bool Baselib_Lock_TryAcquire(Baselib_Lock* lock);

// Acquire lock.
//
// If lock is held, either by this or another thread, then the function wait for lock to be released.
//
// This function is guaranteed to emit an acquire barrier.
static inline void Baselib_Lock_Acquire(Baselib_Lock* lock);

// Try to acquire lock.
//
// If lock is held, either by this or another thread, then the function wait for timeoutInMilliseconds for lock to be released.
//
// Acquire with a zero timeout differs from TryAcquire in that TryAcquire is guaranteed to be a user space operation
// while Acquire may enter the kernel and cause a context switch.
//
// When a lock is acquired this function is guaranteed to emit an acquire barrier.
//
// Timeout passed to this function may be subject to system clock resolution.
// If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
//
// \returns          true if lock was acquired.
COMPILER_WARN_UNUSED_RESULT
static inline bool Baselib_Lock_TryTimedAcquire(Baselib_Lock* lock, uint32_t timeoutInMilliseconds);

// Release lock and make it available to other threads.
//
// This function can be called from any thread, not only the thread that acquired the lock.
// If no lock was previously held calling this function result in a no-op.
//
// When the lock is released this function is guaranteed to emit a release barrier.
static inline void Baselib_Lock_Release(Baselib_Lock* lock);

// Reclaim resources and memory held by lock.
//
// If threads are waiting on the lock, calling free may trigger an assert and may cause process abort.
// Calling this function with a nullptr result in a no-op
static inline void Baselib_Lock_Free(Baselib_Lock* lock);
