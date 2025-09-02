// ==========================================
// THREADING AND CONCURRENCY - COMPLETE DEMO
// Location: 04-CSharp-Advanced/10-Threading-Concurrency/
// ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


    // ==========================================
    // 1. THREAD LIFECYCLE DEMONSTRATION
    // ==========================================
    
    public class ThreadLifecycleDemo
    {
        private static Thread _workerThread;
        private static bool _shouldPause = false;
        private static bool _shouldStop = false;
        private static readonly object _pauseLock = new object();
        
        public static void DemonstrateThreadLifecycle()
        {
            Console.WriteLine("=== THREAD LIFECYCLE DEMO ===");
            
            // Create and start thread
            _workerThread = new Thread(WorkerThreadMethod)
            {
                Name = "WorkerThread",
                IsBackground = false // Foreground thread
            };
            
            Console.WriteLine($"Thread State: {_workerThread.ThreadState}");
            Console.WriteLine("Starting thread...");
            _workerThread.Start();
            
            Thread.Sleep(2000); // Let it run for 2 seconds
            
            // Pause thread
            Console.WriteLine("\nPausing thread...");
            _shouldPause = true;
            Thread.Sleep(3000);
            
            // Resume thread
            Console.WriteLine("Resuming thread...");
            lock (_pauseLock)
            {
                _shouldPause = false;
                Monitor.Pulse(_pauseLock); // Wake up the paused thread
            }
            
            Thread.Sleep(2000);
            
            // Stop thread
            Console.WriteLine("\nStopping thread...");
            _shouldStop = true;
            
            // Wait for thread to complete
            _workerThread.Join();
            Console.WriteLine($"Thread State: {_workerThread.ThreadState}");
            Console.WriteLine("Thread lifecycle demo completed.\n");
        }
        
        private static void WorkerThreadMethod()
        {
            int counter = 0;
            
            while (!_shouldStop)
            {
                // Check if we should pause
                lock (_pauseLock)
                {
                    while (_shouldPause && !_shouldStop)
                    {
                        Console.WriteLine("Thread paused...");
                        Monitor.Wait(_pauseLock);
                        if (!_shouldStop)
                            Console.WriteLine("Thread resumed!");
                    }
                }
                
                if (_shouldStop) break;
                
                counter++;
                Console.WriteLine($"[{Thread.CurrentThread.Name}] Working... Count: {counter}");
                Thread.Sleep(500);
            }
            
            Console.WriteLine($"[{Thread.CurrentThread.Name}] Work completed. Final count: {counter}");
        }
    }
    
    // ==========================================
    // 2. LOCK TYPES DEMONSTRATION
    // ==========================================
    
    public class LockTypesDemo
    {
        private static int _sharedCounter = 0;
        private static readonly object _lockObject = new object();
        private static readonly Mutex _mutex = new Mutex();
        private static readonly Semaphore _semaphore = new Semaphore(2, 2); // Allow 2 concurrent access
        private static readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        private static readonly List<string> _sharedList = new List<string>();
        
        public static void DemonstrateLockTypes()
        {
            Console.WriteLine("=== LOCK TYPES DEMO ===");
            
            // 1. Monitor/Lock demonstration
            Console.WriteLine("\n1. MONITOR/LOCK DEMONSTRATION");
            DemonstrateMonitorLock();
            
            // 2. Mutex demonstration
            Console.WriteLine("\n2. MUTEX DEMONSTRATION");
            DemonstrateMutex();
            
            // 3. Semaphore demonstration
            Console.WriteLine("\n3. SEMAPHORE DEMONSTRATION");
            DemonstrateSemaphore();
            
            // 4. Reader-Writer Lock demonstration
            Console.WriteLine("\n4. READER-WRITER LOCK DEMONSTRATION");
            DemonstrateReaderWriterLock();
            
            Console.WriteLine("Lock types demo completed.\n");
        }
        
        private static void DemonstrateMonitorLock()
        {
            _sharedCounter = 0;
            var threads = new Thread[5];
            
            for (int i = 0; i < 5; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() => IncrementWithLock(threadId))
                {
                    Name = $"LockThread-{threadId}"
                };
            }
            
            foreach (var thread in threads)
                thread.Start();
            
            foreach (var thread in threads)
                thread.Join();
            
            Console.WriteLine($"Final counter value with lock: {_sharedCounter}");
        }
        
        private static void IncrementWithLock(int threadId)
        {
            for (int i = 0; i < 1000; i++)
            {
                lock (_lockObject) // Monitor.Enter/Exit
                {
                    _sharedCounter++;
                }
            }
            Console.WriteLine($"Thread {threadId} completed");
        }
        
        private static void DemonstrateMutex()
        {
            var threads = new Thread[3];
            
            for (int i = 0; i < 3; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() => MutexWorker(threadId))
                {
                    Name = $"MutexThread-{threadId}"
                };
            }
            
            foreach (var thread in threads)
                thread.Start();
            
            foreach (var thread in threads)
                thread.Join();
        }
        
        private static void MutexWorker(int threadId)
        {
            Console.WriteLine($"Thread {threadId} waiting for mutex...");
            
            _mutex.WaitOne(); // Acquire mutex
            
            try
            {
                Console.WriteLine($"Thread {threadId} acquired mutex, working...");
                Thread.Sleep(1000); // Simulate work
                Console.WriteLine($"Thread {threadId} completed work");
            }
            finally
            {
                _mutex.ReleaseMutex(); // Always release in finally block
                Console.WriteLine($"Thread {threadId} released mutex");
            }
        }
        
        private static void DemonstrateSemaphore()
        {
            var threads = new Thread[5];
            
            for (int i = 0; i < 5; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() => SemaphoreWorker(threadId))
                {
                    Name = $"SemaphoreThread-{threadId}"
                };
            }
            
            foreach (var thread in threads)
                thread.Start();
            
            foreach (var thread in threads)
                thread.Join();
        }
        
        private static void SemaphoreWorker(int threadId)
        {
            Console.WriteLine($"Thread {threadId} waiting for semaphore...");
            
            _semaphore.WaitOne(); // Acquire semaphore slot
            
            try
            {
                Console.WriteLine($"Thread {threadId} acquired semaphore, working...");
                Thread.Sleep(2000); // Simulate work
                Console.WriteLine($"Thread {threadId} completed work");
            }
            finally
            {
                _semaphore.Release(); // Release semaphore slot
                Console.WriteLine($"Thread {threadId} released semaphore");
            }
        }
        
        private static void DemonstrateReaderWriterLock()
        {
            _sharedList.Clear();
            var threads = new List<Thread>();
            
            // Create reader threads
            for (int i = 0; i < 3; i++)
            {
                int readerId = i;
                var readerThread = new Thread(() => ReaderWorker(readerId))
                {
                    Name = $"Reader-{readerId}"
                };
                threads.Add(readerThread);
            }
            
            // Create writer threads
            for (int i = 0; i < 2; i++)
            {
                int writerId = i;
                var writerThread = new Thread(() => WriterWorker(writerId))
                {
                    Name = $"Writer-{writerId}"
                };
                threads.Add(writerThread);
            }
            
            // Start all threads
            foreach (var thread in threads)
                thread.Start();
            
            // Wait for all threads
            foreach (var thread in threads)
                thread.Join();
            
            Console.WriteLine($"Final list count: {_sharedList.Count}");
        }
        
        private static void ReaderWorker(int readerId)
        {
            for (int i = 0; i < 5; i++)
            {
                _readerWriterLock.EnterReadLock();
                
                try
                {
                    Console.WriteLine($"Reader {readerId} reading... Count: {_sharedList.Count}");
                    Thread.Sleep(100); // Simulate read time
                }
                finally
                {
                    _readerWriterLock.ExitReadLock();
                }
                
                Thread.Sleep(200);
            }
        }
        
        private static void WriterWorker(int writerId)
        {
            for (int i = 0; i < 3; i++)
            {
                _readerWriterLock.EnterWriteLock();
                
                try
                {
                    string item = $"Writer{writerId}-Item{i}";
                    _sharedList.Add(item);
                    Console.WriteLine($"Writer {writerId} added: {item}");
                    Thread.Sleep(500); // Simulate write time
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
                
                Thread.Sleep(300);
            }
        }
    }
    
    // ==========================================
    // 3. THREAD-SAFE COLLECTIONS
    // ==========================================
    
    public class ThreadSafeCollectionsDemo
    {
        public static void DemonstrateThreadSafeCollections()
        {
            Console.WriteLine("=== THREAD-SAFE COLLECTIONS DEMO ===");
            
            // ConcurrentQueue
            Console.WriteLine("\n1. CONCURRENT QUEUE");
            DemonstrateConcurrentQueue();
            
            // ConcurrentStack
            Console.WriteLine("\n2. CONCURRENT STACK");
            DemonstrateConcurrentStack();
            
            // ConcurrentDictionary
            Console.WriteLine("\n3. CONCURRENT DICTIONARY");
            DemonstrateConcurrentDictionary();
            
            // ConcurrentBag
            Console.WriteLine("\n4. CONCURRENT BAG");
            DemonstrateConcurrentBag();
            
            Console.WriteLine("Thread-safe collections demo completed.\n");
        }
        
        private static void DemonstrateConcurrentQueue()
        {
            var queue = new ConcurrentQueue<string>();
            var threads = new List<Thread>();
            
            // Producer threads
            for (int i = 0; i < 3; i++)
            {
                int producerId = i;
                var producer = new Thread(() =>
                {
                    for (int j = 0; j < 5; j++)
                    {
                        string item = $"Producer{producerId}-Item{j}";
                        queue.Enqueue(item);
                        Console.WriteLine($"Enqueued: {item}");
                        Thread.Sleep(100);
                    }
                });
                threads.Add(producer);
            }
            
            // Consumer threads
            for (int i = 0; i < 2; i++)
            {
                int consumerId = i;
                var consumer = new Thread(() =>
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if (queue.TryDequeue(out string item))
                        {
                            Console.WriteLine($"Consumer{consumerId} dequeued: {item}");
                        }
                        else
                        {
                            Console.WriteLine($"Consumer{consumerId} found empty queue");
                        }
                        Thread.Sleep(150);
                    }
                });
                threads.Add(consumer);
            }
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            Console.WriteLine($"Remaining items in queue: {queue.Count}");
        }
        
        private static void DemonstrateConcurrentStack()
        {
            var stack = new ConcurrentStack<int>();
            var threads = new List<Thread>();
            
            // Pusher threads
            for (int i = 0; i < 2; i++)
            {
                int pusherId = i;
                var pusher = new Thread(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        int value = pusherId * 100 + j;
                        stack.Push(value);
                        Console.WriteLine($"Pushed: {value}");
                        Thread.Sleep(50);
                    }
                });
                threads.Add(pusher);
            }
            
            // Popper thread
            var popper = new Thread(() =>
            {
                for (int i = 0; i < 15; i++)
                {
                    if (stack.TryPop(out int value))
                    {
                        Console.WriteLine($"Popped: {value}");
                    }
                    else
                    {
                        Console.WriteLine("Stack was empty");
                    }
                    Thread.Sleep(80);
                }
            });
            threads.Add(popper);
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            Console.WriteLine($"Remaining items in stack: {stack.Count}");
        }
        
        private static void DemonstrateConcurrentDictionary()
        {
            var dict = new ConcurrentDictionary<string, int>();
            var threads = new List<Thread>();
            
            // Writer threads
            for (int i = 0; i < 3; i++)
            {
                int writerId = i;
                var writer = new Thread(() =>
                {
                    for (int j = 0; j < 5; j++)
                    {
                        string key = $"Key{writerId}-{j}";
                        int value = writerId * 100 + j;
                        
                        dict.TryAdd(key, value);
                        Console.WriteLine($"Added: {key} = {value}");
                        
                        // Update existing key
                        dict.AddOrUpdate(key, value, (k, oldValue) => oldValue + 10);
                        Console.WriteLine($"Updated: {key} = {dict[key]}");
                        
                        Thread.Sleep(100);
                    }
                });
                threads.Add(writer);
            }
            
            // Reader thread
            var reader = new Thread(() =>
            {
                Thread.Sleep(500); // Let some data accumulate
                
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine($"Dictionary contents ({dict.Count} items):");
                    foreach (var kvp in dict.Take(5))
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                    Thread.Sleep(200);
                }
            });
            threads.Add(reader);
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            Console.WriteLine($"Final dictionary size: {dict.Count}");
        }
        
        private static void DemonstrateConcurrentBag()
        {
            var bag = new ConcurrentBag<string>();
            var threads = new List<Thread>();
            
            // Multiple threads adding items
            for (int i = 0; i < 4; i++)
            {
                int threadId = i;
                var thread = new Thread(() =>
                {
                    for (int j = 0; j < 5; j++)
                    {
                        string item = $"Thread{threadId}-Item{j}";
                        bag.Add(item);
                        Console.WriteLine($"Added to bag: {item}");
                        Thread.Sleep(50);
                    }
                });
                threads.Add(thread);
            }
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            Console.WriteLine($"Bag contains {bag.Count} items:");
            foreach (string item in bag.Take(10))
            {
                Console.WriteLine($"  {item}");
            }
        }
    }
    
    // ==========================================
    // 4. THREAD COMMUNICATION
    // ==========================================
    
    public class ThreadCommunicationDemo
    {
        private static readonly object _waitLock = new object();
        private static bool _dataReady = false;
        private static string _sharedData = "";
        
        private static readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private static readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        
        public static void DemonstrateThreadCommunication()
        {
            Console.WriteLine("=== THREAD COMMUNICATION DEMO ===");
            
            // Monitor Wait/Pulse
            Console.WriteLine("\n1. MONITOR WAIT/PULSE");
            DemonstrateWaitPulse();
            
            // AutoResetEvent
            Console.WriteLine("\n2. AUTO RESET EVENT");
            DemonstrateAutoResetEvent();
            
            // ManualResetEvent
            Console.WriteLine("\n3. MANUAL RESET EVENT");
            DemonstrateManualResetEvent();
            
            Console.WriteLine("Thread communication demo completed.\n");
        }
        
        private static void DemonstrateWaitPulse()
        {
            var consumer = new Thread(ConsumerMethod) { Name = "Consumer" };
            var producer = new Thread(ProducerMethod) { Name = "Producer" };
            
            consumer.Start();
            Thread.Sleep(100); // Let consumer start waiting
            producer.Start();
            
            consumer.Join();
            producer.Join();
        }
        
        private static void ConsumerMethod()
        {
            lock (_waitLock)
            {
                while (!_dataReady)
                {
                    Console.WriteLine("Consumer waiting for data...");
                    Monitor.Wait(_waitLock);
                }
                
                Console.WriteLine($"Consumer received data: {_sharedData}");
            }
        }
        
        private static void ProducerMethod()
        {
            Thread.Sleep(2000); // Simulate data preparation
            
            lock (_waitLock)
            {
                _sharedData = "Important Data from Producer";
                _dataReady = true;
                Console.WriteLine("Producer prepared data, notifying consumer...");
                Monitor.Pulse(_waitLock);
            }
        }
        
        private static void DemonstrateAutoResetEvent()
        {
            var threads = new Thread[3];
            
            for (int i = 0; i < 3; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    Console.WriteLine($"Thread {threadId} waiting for signal...");
                    _autoResetEvent.WaitOne();
                    Console.WriteLine($"Thread {threadId} received signal and proceeding");
                    Thread.Sleep(1000);
                    Console.WriteLine($"Thread {threadId} completed");
                })
                { Name = $"AutoResetThread-{threadId}" };
            }
            
            foreach (var thread in threads) thread.Start();
            
            Thread.Sleep(1000);
            Console.WriteLine("Signaling threads one by one...");
            
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Sending signal #{i + 1}");
                _autoResetEvent.Set(); // Only one thread will be released
            }
            
            foreach (var thread in threads) thread.Join();
        }
        
        private static void DemonstrateManualResetEvent()
        {
            _manualResetEvent.Reset(); // Ensure it starts unsignaled
            
            var threads = new Thread[3];
            
            for (int i = 0; i < 3; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    Console.WriteLine($"Thread {threadId} waiting for manual reset event...");
                    _manualResetEvent.WaitOne();
                    Console.WriteLine($"Thread {threadId} released and proceeding");
                    Thread.Sleep(500);
                    Console.WriteLine($"Thread {threadId} completed");
                })
                { Name = $"ManualResetThread-{threadId}" };
            }
            
            foreach (var thread in threads) thread.Start();
            
            Thread.Sleep(2000);
            Console.WriteLine("Setting manual reset event - all threads will be released");
            _manualResetEvent.Set(); // All threads will be released simultaneously
            
            foreach (var thread in threads) thread.Join();
        }
    }
    
    // ==========================================
    // 5. PERFORMANCE COMPARISON
    // ==========================================
    
    public class ThreadingPerformanceDemo
    {
        public static void DemonstratePerformance()
        {
            Console.WriteLine("=== THREADING PERFORMANCE DEMO ===");
            
            const int iterations = 10_000_000;
            
            // Single-threaded
            Console.WriteLine("\n1. SINGLE-THREADED COUNTER");
            var sw = Stopwatch.StartNew();
            int counter1 = SingleThreadedCounter(iterations);
            sw.Stop();
            Console.WriteLine($"Single-threaded result: {counter1}, Time: {sw.ElapsedMilliseconds}ms");
            
            // Multi-threaded without synchronization (race condition)
            Console.WriteLine("\n2. MULTI-THREADED WITHOUT LOCK (Race Condition)");
            sw = Stopwatch.StartNew();
            int counter2 = MultiThreadedCounterNoLock(iterations);
            sw.Stop();
            Console.WriteLine($"Multi-threaded (no lock) result: {counter2}, Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Expected: {iterations}, Lost updates: {iterations - counter2}");
            
            // Multi-threaded with lock
            Console.WriteLine("\n3. MULTI-THREADED WITH LOCK");
            sw = Stopwatch.StartNew();
            int counter3 = MultiThreadedCounterWithLock(iterations);
            sw.Stop();
            Console.WriteLine($"Multi-threaded (with lock) result: {counter3}, Time: {sw.ElapsedMilliseconds}ms");
            
            // Multi-threaded with Interlocked
            Console.WriteLine("\n4. MULTI-THREADED WITH INTERLOCKED");
            sw = Stopwatch.StartNew();
            int counter4 = MultiThreadedCounterWithInterlocked(iterations);
            sw.Stop();
            Console.WriteLine($"Multi-threaded (Interlocked) result: {counter4}, Time: {sw.ElapsedMilliseconds}ms");
            
            Console.WriteLine("Performance demo completed.\n");
        }
        
        private static int SingleThreadedCounter(int iterations)
        {
            int counter = 0;
            for (int i = 0; i < iterations; i++)
            {
                counter++;
            }
            return counter;
        }
        
        private static int MultiThreadedCounterNoLock(int iterations)
        {
            int counter = 0;
            int iterationsPerThread = iterations / 4;
            var threads = new Thread[4];
            
            for (int i = 0; i < 4; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        counter++; // Race condition!
                    }
                });
            }
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            return counter;
        }
        
        private static int MultiThreadedCounterWithLock(int iterations)
        {
            int counter = 0;
            int iterationsPerThread = iterations / 4;
            var lockObject = new object();
            var threads = new Thread[4];
            
            for (int i = 0; i < 4; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        lock (lockObject)
                        {
                            counter++;
                        }
                    }
                });
            }
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            return counter;
        }
        
        private static int MultiThreadedCounterWithInterlocked(int iterations)
        {
            int counter = 0;
            int iterationsPerThread = iterations / 4;
            var threads = new Thread[4];
            
            for (int i = 0; i < 4; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        Interlocked.Increment(ref counter);
                    }
                });
            }
            
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            
            return counter;
        }
    }
    
    // ==========================================
    // MAIN PROGRAM
    // ==========================================
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== .NET THREADING AND CONCURRENCY DEMONSTRATION ===");
            Console.WriteLine("Press any key between demos to continue...\n");
            
            try
            {
                // 1. Thread Lifecycle
                ThreadLifecycleDemo.DemonstrateThreadLifecycle();
                Console.WriteLine("Press any key to continue to Lock Types demo...");
                Console.ReadKey();
                Console.Clear();
                
                // 2. Lock Types
                LockTypesDemo.DemonstrateLockTypes();
                Console.WriteLine("Press any key to continue to Thread-Safe Collections demo...");
                Console.ReadKey();
                Console.Clear();
                
                // 3. Thread-Safe Collections
                ThreadSafeCollectionsDemo.DemonstrateThreadSafeCollections();
                Console.WriteLine("Press any key to continue to Thread Communication demo...");
                Console.ReadKey();
                Console.Clear();
                
                // 4. Thread Communication
                ThreadCommunicationDemo.DemonstrateThreadCommunication();
                Console.WriteLine("Press any key to continue to Performance demo...");
                Console.ReadKey();
                Console.Clear();
                
                // 5. Performance Comparison
                ThreadingPerformanceDemo.DemonstratePerformance();
                
                Console.WriteLine("=== ALL THREADING DEMOS COMPLETED ===");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
